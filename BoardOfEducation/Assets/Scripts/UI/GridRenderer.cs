using System.Collections;
using BoardOfEducation.Game;
using UnityEngine;
using UnityEngine.UI;

namespace BoardOfEducation.UI
{
    /// <summary>
    /// Renders the grid and robot on a Canvas.
    /// Grid occupies upper ~65% of screen.
    /// </summary>
    public class GridRenderer : MonoBehaviour
    {
        private Canvas _canvas;
        private GridData _grid;
        private Image[,] _cellImages;
        private RectTransform _robotRect;
        private Image _robotImage;

        private float _cellSize;
        private Vector2 _gridOrigin; // top-left of grid in canvas coords

        private static readonly Color EmptyColor = new Color(0.25f, 0.25f, 0.3f);
        private static readonly Color BlockedColor = new Color(0.1f, 0.1f, 0.12f);
        private static readonly Color StartColor = new Color(0.2f, 0.5f, 0.9f);
        private static readonly Color GoalColor = new Color(1f, 0.85f, 0.2f);
        private static readonly Color GapColor = new Color(0.05f, 0.05f, 0.08f, 0.5f);
        private static readonly Color RobotColor = new Color(0.3f, 1f, 0.4f);

        public void Initialize(Canvas canvas, GridData grid)
        {
            _canvas = canvas;
            _grid = grid;
            BuildGrid();
            CreateRobot();
            MoveRobotImmediate(grid.StartPos, grid.StartDir);
        }

        private void BuildGrid()
        {
            // Grid area: centered in upper 65% of screen
            float screenW = Screen.width;
            float screenH = Screen.height;
            float gridAreaH = screenH * 0.65f;
            float gridAreaW = screenW * 0.9f;

            // Calculate cell size to fit grid
            float maxCellW = gridAreaW / _grid.Cols;
            float maxCellH = gridAreaH / _grid.Rows;
            _cellSize = Mathf.Min(maxCellW, maxCellH, 150f);

            float totalGridW = _cellSize * _grid.Cols;
            float totalGridH = _cellSize * _grid.Rows;

            // Center grid horizontally, vertically in upper area
            float gridX = (screenW - totalGridW) / 2f;
            float gridY = screenH * 0.08f + (gridAreaH - totalGridH) / 2f;
            _gridOrigin = new Vector2(gridX, gridY);

            _cellImages = new Image[_grid.Rows, _grid.Cols];

            for (int row = 0; row < _grid.Rows; row++)
            {
                for (int col = 0; col < _grid.Cols; col++)
                {
                    var cellType = _grid.Cells[row, col];
                    var go = new GameObject($"Cell_{row}_{col}");
                    go.transform.SetParent(_canvas.transform, false);

                    var img = go.AddComponent<Image>();
                    img.color = GetCellColor(cellType);
                    img.raycastTarget = false;

                    var rt = img.rectTransform;
                    rt.anchorMin = Vector2.zero;
                    rt.anchorMax = Vector2.zero;
                    rt.pivot = new Vector2(0, 1);

                    // Canvas coords: origin bottom-left, Y-up
                    float canvasX = _gridOrigin.x + col * _cellSize;
                    float canvasY = Screen.height - _gridOrigin.y - row * _cellSize;
                    rt.anchoredPosition = new Vector2(canvasX, canvasY);
                    rt.sizeDelta = new Vector2(_cellSize - 4, _cellSize - 4); // 4px gap

                    _cellImages[row, col] = img;

                    // Add label for Start/Goal
                    if (cellType == CellType.Start || cellType == CellType.Goal)
                    {
                        var labelGo = new GameObject("Label");
                        labelGo.transform.SetParent(go.transform, false);
                        var label = labelGo.AddComponent<Text>();
                        label.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                        label.fontSize = (int)(_cellSize * 0.3f);
                        label.fontStyle = FontStyle.Bold;
                        label.color = Color.white;
                        label.alignment = TextAnchor.MiddleCenter;
                        label.text = cellType == CellType.Start ? "START" : "GOAL";
                        label.raycastTarget = false;
                        var lrt = label.rectTransform;
                        lrt.anchorMin = Vector2.zero;
                        lrt.anchorMax = Vector2.one;
                        lrt.offsetMin = Vector2.zero;
                        lrt.offsetMax = Vector2.zero;
                    }
                }
            }
        }

        private void CreateRobot()
        {
            var go = new GameObject("Robot");
            go.transform.SetParent(_canvas.transform, false);

            _robotImage = go.AddComponent<Image>();
            _robotImage.color = RobotColor;
            _robotImage.raycastTarget = false;

            _robotRect = _robotImage.rectTransform;
            _robotRect.anchorMin = Vector2.zero;
            _robotRect.anchorMax = Vector2.zero;
            _robotRect.pivot = new Vector2(0.5f, 0.5f);
            float robotSize = _cellSize * 0.6f;
            _robotRect.sizeDelta = new Vector2(robotSize, robotSize);
        }

        private Vector2 GridToCanvas(Vector2Int gridPos)
        {
            float canvasX = _gridOrigin.x + gridPos.x * _cellSize + _cellSize / 2f;
            float canvasY = Screen.height - (_gridOrigin.y + gridPos.y * _cellSize + _cellSize / 2f);
            return new Vector2(canvasX, canvasY);
        }

        private float DirectionToAngle(Direction dir)
        {
            switch (dir)
            {
                case Direction.Up: return 0f;
                case Direction.Right: return -90f;
                case Direction.Down: return -180f;
                case Direction.Left: return 90f;
                default: return 0f;
            }
        }

        public void MoveRobotImmediate(Vector2Int pos, Direction dir)
        {
            if (_robotRect == null) return;
            _robotRect.anchoredPosition = GridToCanvas(pos);
            _robotRect.localEulerAngles = new Vector3(0, 0, DirectionToAngle(dir));
        }

        /// <summary>
        /// Animate robot moving one step. Returns coroutine.
        /// </summary>
        public IEnumerator AnimateStep(StepResult from, StepResult to, float duration = 0.4f)
        {
            if (_robotRect == null) yield break;

            Vector2 startPos = GridToCanvas(from.Position);
            Vector2 endPos = GridToCanvas(to.Position);
            float startAngle = DirectionToAngle(from.Facing);
            float endAngle = DirectionToAngle(to.Facing);

            // Handle angle wrapping
            float angleDiff = Mathf.DeltaAngle(startAngle, endAngle);

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0, 1, elapsed / duration);

                _robotRect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
                _robotRect.localEulerAngles = new Vector3(0, 0, startAngle + angleDiff * t);

                // Bounce for jump
                if (to.Jumped)
                {
                    float bounce = Mathf.Sin(t * Mathf.PI) * _cellSize * 0.3f;
                    var pos = _robotRect.anchoredPosition;
                    pos.y += bounce;
                    _robotRect.anchoredPosition = pos;
                }

                yield return null;
            }

            _robotRect.anchoredPosition = endPos;
            _robotRect.localEulerAngles = new Vector3(0, 0, endAngle);

            // Flash red on failure
            if (!to.Success)
            {
                _robotImage.color = Color.red;
                yield return new WaitForSeconds(0.3f);
                _robotImage.color = RobotColor;
            }
        }

        /// <summary>
        /// Highlight a cell (e.g., on success/failure).
        /// </summary>
        public void HighlightCell(Vector2Int pos, Color color)
        {
            if (_grid.InBounds(pos) && _cellImages != null)
                _cellImages[pos.y, pos.x].color = color;
        }

        public void ResetCellColors()
        {
            if (_grid == null || _cellImages == null) return;
            for (int row = 0; row < _grid.Rows; row++)
                for (int col = 0; col < _grid.Cols; col++)
                    _cellImages[row, col].color = GetCellColor(_grid.Cells[row, col]);
        }

        public void Cleanup()
        {
            if (_cellImages != null)
            {
                for (int row = 0; row < _grid.Rows; row++)
                    for (int col = 0; col < _grid.Cols; col++)
                        if (_cellImages[row, col] != null)
                            Destroy(_cellImages[row, col].gameObject);
            }
            if (_robotImage != null)
                Destroy(_robotImage.gameObject);
        }

        private static Color GetCellColor(CellType type)
        {
            switch (type)
            {
                case CellType.Empty: return EmptyColor;
                case CellType.Blocked: return BlockedColor;
                case CellType.Start: return StartColor;
                case CellType.Goal: return GoalColor;
                case CellType.Gap: return GapColor;
                default: return EmptyColor;
            }
        }
    }
}

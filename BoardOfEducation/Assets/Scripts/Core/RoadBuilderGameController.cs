using System.Collections;
using System.Text;
using BoardOfEducation.Game;
using BoardOfEducation.UI;
using UnityEngine;

namespace BoardOfEducation.Core
{
    public enum GameState { ShowingLevel, Running, Success, Failure }

    /// <summary>
    /// State machine controlling the Robot Road Builder game flow.
    /// ShowingLevel → Running → Success/Failure → next level or retry.
    /// </summary>
    public class RoadBuilderGameController : MonoBehaviour
    {
        private SequenceSlotManager _slotManager;
        private GridRenderer _gridRenderer;
        private SlotDisplay _slotDisplay;
        private StatusDisplay _statusDisplay;
        private Canvas _canvas;
        private PieceTracker _pieceTracker;

        private GridData _currentGrid;
        private int _currentLevel;
        private GameState _state;

        public void Initialize(Canvas canvas, PieceTracker pieceTracker,
                               SequenceSlotManager slotManager, GridRenderer gridRenderer,
                               SlotDisplay slotDisplay, StatusDisplay statusDisplay)
        {
            _canvas = canvas;
            _pieceTracker = pieceTracker;
            _slotManager = slotManager;
            _gridRenderer = gridRenderer;
            _slotDisplay = slotDisplay;
            _statusDisplay = statusDisplay;

            _slotManager.OnAllSlotsFilled += OnSlotsFilled;
            _slotManager.OnSlotsCleared += OnSlotsCleared;
        }

        public void LoadLevel(int levelIndex)
        {
            _currentLevel = levelIndex;

            // Clean up previous level
            _gridRenderer.Cleanup();
            _slotDisplay.Cleanup();

            // Load new level
            _currentGrid = RoadBuilderLevels.GetLevel(levelIndex);

            // Re-initialize slot manager for new piece count
            _slotManager.Initialize(_pieceTracker, _currentGrid.RequiredPieces);

            // Build visuals
            _gridRenderer.Initialize(_canvas, _currentGrid);
            _slotDisplay.Initialize(_canvas, _slotManager);

            // Update status
            _statusDisplay.SetLevelName($"Level {levelIndex + 1}: {_currentGrid.LevelName}");
            _statusDisplay.SetInstruction(_currentGrid.Instruction);
            _statusDisplay.SetStatus($"Place {_currentGrid.RequiredPieces} robot piece{(_currentGrid.RequiredPieces > 1 ? "s" : "")}!",
                                      new Color(1, 1, 0.7f));

            _state = GameState.ShowingLevel;
            Debug.Log($"[RoadBuilder] Loaded level {levelIndex + 1}: {_currentGrid.LevelName} ({_currentGrid.RequiredPieces} pieces)");
        }

        private void Update()
        {
            if (_pieceTracker == null || _statusDisplay == null) return;

            var pieces = _pieceTracker.ActivePieces;
            var sb = new StringBuilder();
            sb.Append($"Pieces on board: {pieces.Count}");

            if (pieces.Count > 0)
            {
                var slotGlyphs = _slotManager.GetSlotGlyphIds();
                foreach (var kvp in pieces)
                {
                    var p = kvp.Value;
                    string cmdName;
                    if (CommandMapping.TryGetCommand(p.GlyphId, out var cmd))
                        cmdName = CommandMapping.GetCommandName(cmd);
                    else
                        cmdName = $"Unknown({p.GlyphId})";

                    // Find which slot this piece is in (if any)
                    int slotIndex = -1;
                    if (slotGlyphs != null)
                    {
                        for (int i = 0; i < slotGlyphs.Length; i++)
                        {
                            if (slotGlyphs[i] == p.GlyphId)
                            {
                                slotIndex = i;
                                break;
                            }
                        }
                    }

                    string slotStr = slotIndex >= 0 ? $" -> Slot {slotIndex + 1}" : "";
                    sb.Append($"\n  {cmdName}{slotStr}");
                }
            }

            _statusDisplay.SetPieceInfo(sb.ToString());
        }

        private void OnSlotsFilled()
        {
            Debug.Log($"[GameController] OnSlotsFilled — state={_state}");
            if (_state != GameState.ShowingLevel) return;
            StartCoroutine(RunSequence());
        }

        private void OnSlotsCleared()
        {
            if (_state == GameState.Failure)
            {
                // Reset robot to start and allow retry
                _gridRenderer.ResetCellColors();
                _gridRenderer.MoveRobotImmediate(_currentGrid.StartPos, _currentGrid.StartDir);
                _statusDisplay.SetStatus($"Place {_currentGrid.RequiredPieces} robot piece{(_currentGrid.RequiredPieces > 1 ? "s" : "")}!",
                                          new Color(1, 1, 0.7f));
                _state = GameState.ShowingLevel;
            }
        }

        private IEnumerator RunSequence()
        {
            _state = GameState.Running;
            _statusDisplay.SetStatus("Running...", new Color(0.5f, 0.8f, 1f));

            var commands = _slotManager.GetCurrentSequence();
            Debug.Log($"[GameController] Running sequence: {string.Join(", ", commands)}");
            var steps = RobotSimulator.Run(_currentGrid, commands);
            Debug.Log($"[GameController] Simulator returned {steps.Count} steps");

            // Animate each step
            for (int i = 1; i < steps.Count; i++)
            {
                Debug.Log($"[GameController] Step {i}: pos={steps[i].Position} facing={steps[i].Facing} success={steps[i].Success} jumped={steps[i].Jumped} goal={steps[i].ReachedGoal}");
                yield return StartCoroutine(_gridRenderer.AnimateStep(steps[i - 1], steps[i]));
                yield return new WaitForSeconds(0.1f);
            }

            // Check final result
            var last = steps[steps.Count - 1];
            if (last.ReachedGoal)
            {
                _state = GameState.Success;
                Debug.Log($"[GameController] SUCCESS — robot reached goal!");
                _statusDisplay.SetStatus("You did it!", new Color(0.3f, 1f, 0.4f));
                _gridRenderer.HighlightCell(_currentGrid.GoalPos, new Color(0.3f, 1f, 0.4f));

                yield return new WaitForSeconds(2f);

                // Load next level
                int next = _currentLevel + 1;
                if (next < RoadBuilderLevels.Count)
                {
                    LoadLevel(next);
                }
                else
                {
                    _statusDisplay.SetStatus("All levels complete!", new Color(1f, 0.85f, 0.2f));
                }
            }
            else
            {
                _state = GameState.Failure;
                Debug.Log($"[GameController] FAILURE — robot at {last.Position}, success={last.Success}");
                _statusDisplay.SetStatus("Try again! Lift pieces to retry.", new Color(1f, 0.3f, 0.2f));
            }
        }

        private void OnDestroy()
        {
            if (_slotManager != null)
            {
                _slotManager.OnAllSlotsFilled -= OnSlotsFilled;
                _slotManager.OnSlotsCleared -= OnSlotsCleared;
            }
        }
    }
}

using UnityEngine;
using UnityEngine.UI;

namespace BoardOfEducation.UI
{
    /// <summary>
    /// Shows level name and status messages at the top of the screen.
    /// </summary>
    public class StatusDisplay : MonoBehaviour
    {
        private Text _levelText;
        private Text _statusText;
        private Text _pieceInfoText;

        public void Initialize(Canvas canvas)
        {
            // Level name
            var levelGo = new GameObject("LevelName");
            levelGo.transform.SetParent(canvas.transform, false);
            _levelText = levelGo.AddComponent<Text>();
            _levelText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            _levelText.fontSize = 40;
            _levelText.fontStyle = FontStyle.Bold;
            _levelText.color = Color.white;
            _levelText.alignment = TextAnchor.MiddleCenter;
            _levelText.raycastTarget = false;

            var shadow = levelGo.AddComponent<Shadow>();
            shadow.effectColor = Color.black;
            shadow.effectDistance = new Vector2(2, -2);

            var lrt = _levelText.rectTransform;
            lrt.anchorMin = new Vector2(0.1f, 0.93f);
            lrt.anchorMax = new Vector2(0.9f, 0.99f);
            lrt.offsetMin = Vector2.zero;
            lrt.offsetMax = Vector2.zero;

            // Status message
            var statusGo = new GameObject("StatusMessage");
            statusGo.transform.SetParent(canvas.transform, false);
            _statusText = statusGo.AddComponent<Text>();
            _statusText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            _statusText.fontSize = 32;
            _statusText.color = new Color(1, 1, 0.7f);
            _statusText.alignment = TextAnchor.MiddleCenter;
            _statusText.raycastTarget = false;

            var shadow2 = statusGo.AddComponent<Shadow>();
            shadow2.effectColor = Color.black;

            var srt = _statusText.rectTransform;
            srt.anchorMin = new Vector2(0.1f, 0.87f);
            srt.anchorMax = new Vector2(0.9f, 0.93f);
            srt.offsetMin = Vector2.zero;
            srt.offsetMax = Vector2.zero;

            // Piece info (below status)
            var pieceInfoGo = new GameObject("PieceInfo");
            pieceInfoGo.transform.SetParent(canvas.transform, false);
            _pieceInfoText = pieceInfoGo.AddComponent<Text>();
            _pieceInfoText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            _pieceInfoText.fontSize = 24;
            _pieceInfoText.color = new Color(0.8f, 0.9f, 1f);
            _pieceInfoText.alignment = TextAnchor.UpperCenter;
            _pieceInfoText.raycastTarget = false;

            var shadow3 = pieceInfoGo.AddComponent<Shadow>();
            shadow3.effectColor = Color.black;

            var prt = _pieceInfoText.rectTransform;
            prt.anchorMin = new Vector2(0.1f, 0.78f);
            prt.anchorMax = new Vector2(0.9f, 0.87f);
            prt.offsetMin = Vector2.zero;
            prt.offsetMax = Vector2.zero;
        }

        public void SetLevelName(string name)
        {
            if (_levelText != null)
                _levelText.text = name;
        }

        public void SetStatus(string message)
        {
            if (_statusText != null)
                _statusText.text = message;
        }

        public void SetStatus(string message, Color color)
        {
            if (_statusText != null)
            {
                _statusText.text = message;
                _statusText.color = color;
            }
        }

        public void SetPieceInfo(string info)
        {
            if (_pieceInfoText != null)
                _pieceInfoText.text = info;
        }
    }
}

using System;
using System.IO;
using UnityEngine;

namespace BoardOfEducation
{
    /// <summary>
    /// Logs piece interactions to a CSV file for learning analytics.
    /// Schema: timestamp, session_id, player_id, piece_id, action, position, rotation, game_state
    /// </summary>
    public class InteractionLogger
    {
        private readonly string _sessionId;
        private readonly string _logPath;
        private readonly StreamWriter _writer;
        private bool _headerWritten;

        public InteractionLogger(string sessionId)
        {
            _sessionId = sessionId ?? Guid.NewGuid().ToString("N")[..8];
            var dir = Path.Combine(Application.persistentDataPath, "interaction_logs");
            Directory.CreateDirectory(dir);
            _logPath = Path.Combine(dir, $"session_{_sessionId}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv");
            _writer = new StreamWriter(_logPath, append: true);
            _headerWritten = false;
        }

        /// <summary>
        /// Log a piece interaction.
        /// </summary>
        public void Log(string playerId, string pieceId, string action, Vector2 position, float orientationDegrees, string gameState)
        {
            EnsureHeader();
            var timestamp = DateTime.UtcNow.ToString("o");
            var posStr = $"{position.x:F1},{position.y:F1}";
            var line = $"{timestamp},{_sessionId},{playerId},{pieceId},{action},{posStr},{orientationDegrees:F1},{EscapeCsv(gameState)}";
            _writer.WriteLine(line);
            _writer.Flush();
        }

        /// <summary>
        /// Log a system event (e.g., session start, puzzle complete).
        /// </summary>
        public void LogSystem(string action, string gameState)
        {
            EnsureHeader();
            var timestamp = DateTime.UtcNow.ToString("o");
            var line = $"{timestamp},{_sessionId},system,_,{action},_,_,{EscapeCsv(gameState)}";
            _writer.WriteLine(line);
            _writer.Flush();
        }

        private void EnsureHeader()
        {
            if (_headerWritten) return;
            _writer.WriteLine("timestamp,session_id,player_id,piece_id,action,position,rotation,game_state");
            _headerWritten = true;
        }

        private static string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
                return '"' + value.Replace("\"", "\"\"") + '"';
            return value;
        }

        public void Close()
        {
            _writer?.Close();
        }

        public string LogPath => _logPath;
        public string SessionId => _sessionId;
    }
}

using System;
using System.Collections;
using System.Reflection;
using Board.Input;
using Board.Input.Simulation;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BoardOfEducation.DebugTools
{
    /// <summary>
    /// Temporary helper to inject one simulated Forward glyph contact via MCP-driven Play Mode.
    /// Remove after test validation.
    /// </summary>
    public class McpForwardPieceInjector : MonoBehaviour
    {
        [SerializeField] private bool injectOnStart = true;
        [SerializeField] private Vector2 screenPosition = new Vector2(300f, 850f);
        [SerializeField] private float injectDelaySeconds = 0.5f;
        [SerializeField] private string forwardIconAssetPath =
            "Packages/fun.board/Editor/Assets/Simulation/Icons/BoardArcade/BoardArcadeRobotYellow.asset";

        private Component _injectedContact;

        private IEnumerator Start()
        {
            if (!injectOnStart)
                yield break;

            yield return new WaitForSeconds(injectDelaySeconds);
            InjectForward();
        }

        [ContextMenu("Inject Forward Piece")]
        public void InjectForward()
        {
            try
            {
                BoardContactSimulation.Enable();
                var simulation = BoardContactSimulation.instance;
                if (simulation == null)
                {
                    UnityEngine.Debug.LogError("[MCP Injector] BoardContactSimulation instance unavailable.");
                    return;
                }

                var icon = LoadForwardIcon();
                if (icon == null)
                {
                    UnityEngine.Debug.LogError("[MCP Injector] Failed to locate Forward icon asset.");
                    return;
                }

                simulation.currentIcon = icon;
                _injectedContact = CreateAndPlaceContact(simulation, icon, screenPosition);

                if (_injectedContact != null)
                    UnityEngine.Debug.Log($"[MCP Injector] Injected Forward piece (glyph={icon.glyphId}) at {screenPosition}.");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[MCP Injector] Injection failed: {ex.Message}");
            }
        }

        [ContextMenu("Lift Injected Piece")]
        public void LiftInjectedPiece()
        {
            if (_injectedContact == null)
                return;

            var contactType = _injectedContact.GetType();
            var liftMethod = contactType.GetMethod("Lift", BindingFlags.Public | BindingFlags.Instance);
            liftMethod?.Invoke(_injectedContact, null);
            _injectedContact = null;
            UnityEngine.Debug.Log("[MCP Injector] Lifted injected piece.");
        }

        private static Component CreateAndPlaceContact(BoardContactSimulation simulation, BoardContactSimulationIcon icon, Vector2 position)
        {
            var contactType = simulation.GetType().Assembly.GetType("Board.Input.Simulation.BoardSimulationContact");
            if (contactType == null)
                return null;

            var go = new GameObject("McpInjectedForwardContact");
            go.transform.SetParent(simulation.transform, false);

            var contact = go.AddComponent(contactType);
            var iconProperty = contactType.GetProperty("icon", BindingFlags.Public | BindingFlags.Instance);
            iconProperty?.SetValue(contact, icon);

            var moveMethod = contactType.GetMethod("MoveTo", BindingFlags.Public | BindingFlags.Instance);
            moveMethod?.Invoke(contact, new object[] { position });

            var placeMethod = contactType.GetMethod("Place", BindingFlags.Public | BindingFlags.Instance);
            placeMethod?.Invoke(contact, new object[] { false });

            return contact;
        }

        private BoardContactSimulationIcon LoadForwardIcon()
        {
            BoardContactSimulationIcon icon = null;

#if UNITY_EDITOR
            icon = AssetDatabase.LoadAssetAtPath<BoardContactSimulationIcon>(forwardIconAssetPath);

            if (icon == null || icon.contactType != BoardContactType.Glyph || icon.glyphId != 0)
            {
                string[] guids = AssetDatabase.FindAssets("t:BoardContactSimulationIcon");
                foreach (var guid in guids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var candidate = AssetDatabase.LoadAssetAtPath<BoardContactSimulationIcon>(path);
                    if (candidate != null && candidate.contactType == BoardContactType.Glyph && candidate.glyphId == 0)
                    {
                        icon = candidate;
                        break;
                    }
                }
            }
#endif

            return icon;
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Board.Input;
using Board.Input.Simulation;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
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

            yield return new WaitForSecondsRealtime(injectDelaySeconds);
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
                {
                    UnityEngine.Debug.Log($"[MCP Injector] Injected Forward piece (glyph={icon.glyphId}) at {screenPosition}.");
                }
                else
                {
                    UnityEngine.Debug.LogError("[MCP Injector] Injection failed: could not create simulation contact.");
                }
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
            var contactType = ResolveContactType(simulation);
            if (contactType == null)
            {
                var candidates = string.Join(", ", GetCandidateContactTypes(simulation));
                UnityEngine.Debug.LogError($"[MCP Injector] Could not resolve simulation contact type. Candidates: {candidates}");
                return null;
            }

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

        private static Type ResolveContactType(BoardContactSimulation simulation)
        {
            var assembly = simulation.GetType().Assembly;
            var exact = assembly.GetType("Board.Input.Simulation.BoardSimulationContact");
            if (exact != null)
                return exact;

            foreach (var type in assembly.GetTypes())
            {
                if (!typeof(Component).IsAssignableFrom(type))
                    continue;
                if (!string.Equals(type.Namespace, "Board.Input.Simulation", StringComparison.Ordinal))
                    continue;

                var iconProperty = type.GetProperty("icon", BindingFlags.Public | BindingFlags.Instance);
                var hasMoveTo = type.GetMethod("MoveTo", BindingFlags.Public | BindingFlags.Instance) != null;
                var hasPlace = type.GetMethod("Place", BindingFlags.Public | BindingFlags.Instance) != null;

                if (iconProperty != null && hasMoveTo && hasPlace)
                    return type;
            }

            return null;
        }

        private static IEnumerable<string> GetCandidateContactTypes(BoardContactSimulation simulation)
        {
            var assembly = simulation.GetType().Assembly;
            foreach (var type in assembly.GetTypes())
            {
                if (!typeof(Component).IsAssignableFrom(type))
                    continue;
                if (!string.Equals(type.Namespace, "Board.Input.Simulation", StringComparison.Ordinal))
                    continue;
                if (type.Name.IndexOf("Contact", StringComparison.OrdinalIgnoreCase) < 0)
                    continue;
                yield return type.FullName;
            }
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
#endif

using UnityEngine;

namespace BoardOfEducation.Game
{
    /// <summary>
    /// Defines a named zone on the Board surface.
    /// Bounds are in screen-space pixels (origin top-left, per Board SDK).
    /// </summary>
    [System.Serializable]
    public class SortingZone
    {
        public string Name;
        public Rect ScreenBounds; // screen-space pixel rect
        public Color Color;

        public SortingZone(string name, Rect screenBounds, Color color)
        {
            Name = name;
            ScreenBounds = screenBounds;
            Color = color;
        }

        public bool Contains(Vector2 screenPosition)
        {
            return ScreenBounds.Contains(screenPosition);
        }
    }
}

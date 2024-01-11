using System;
using UnityEngine;

namespace YIUIFramework
{
    /// <summary>
    /// <see cref="UnityEngine.Vector2"/>.
    /// </summary>
    public static class Vector2Extensions
    {
        /// <summary>
        /// Parse Vector2 from a string.
        /// </summary>
        public static Vector2 Parse(string text)
        {
            Vector2 value;
            if (!TryParse(text, out value))
            {
                var msg = string.Format(
                    "The string {0} can not convert to Rect.", text);
                throw new FormatException(msg);
            }

            return value;
        }

        /// <summary>
        /// Try to parse Vector2 from a string.
        /// </summary>
        public static bool TryParse(string text, out Vector2 v)
        {
            if (text.Length < 2 ||
                text[0] != '(' ||
                text[text.Length - 1] != ')')
            {
                v = Vector2.zero;
                return false;
            }

            var elements = text.Substring(1, text.Length - 2).Split(',');
            if (elements.Length != 2)
            {
                v = Vector2.zero;
                return false;
            }

            float x = float.Parse(elements[0]);
            float y = float.Parse(elements[1]);
            v = new Vector2(x, y);
            return true;
        }
    }
}
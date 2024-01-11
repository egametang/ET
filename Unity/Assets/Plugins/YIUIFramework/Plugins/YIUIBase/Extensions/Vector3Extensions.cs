using System;
using UnityEngine;

namespace YIUIFramework
{
    /// <summary>
    /// <see cref="UnityEngine.Vector3"/>.
    /// </summary>
    public static class Vector3Extensions
    {
        /// <summary>
        /// Parse Vector3 from a string.
        /// </summary>
        public static Vector3 Parse(string text)
        {
            Vector3 value;
            if (!TryParse(text, out value))
            {
                var msg = string.Format(
                    "The string {0} can not convert to Rect.", text);
                throw new FormatException(msg);
            }

            return value;
        }

        /// <summary>
        /// Try to parse Vector3 from a string.
        /// </summary>
        public static bool TryParse(string text, out Vector3 v)
        {
            if (text.Length < 2 ||
                text[0] != '(' ||
                text[text.Length - 1] != ')')
            {
                v = Vector3.zero;
                return false;
            }

            var elements = text.Substring(1, text.Length - 2).Split(',');
            if (elements.Length != 3)
            {
                v = Vector3.zero;
                return false;
            }

            float x = float.Parse(elements[0]);
            float y = float.Parse(elements[1]);
            float z = float.Parse(elements[2]);
            v = new Vector3(x, y, z);
            return true;
        }
    }
}
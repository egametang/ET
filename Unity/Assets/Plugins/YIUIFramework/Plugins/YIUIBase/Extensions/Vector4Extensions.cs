using System;
using UnityEngine;

namespace YIUIFramework
{
    /// <summary>
    /// <see cref="UnityEngine.Vector4"/>.
    /// </summary>
    public static class Vector4Extensions
    {
        /// <summary>
        /// Parse Vector4 from a string.
        /// </summary>
        public static Vector4 Parse(string text)
        {
            Vector4 value;
            if (!TryParse(text, out value))
            {
                var msg = string.Format(
                    "The string {0} can not convert to Rect.", text);
                throw new FormatException(msg);
            }

            return value;
        }

        /// <summary>
        /// Try to parse Vector4 from a string.
        /// </summary>
        public static bool TryParse(string text, out Vector4 v)
        {
            if (text.Length < 2 ||
                text[0] != '(' ||
                text[text.Length - 1] != ')')
            {
                v = Vector4.zero;
                return false;
            }

            var elements = text.Substring(1, text.Length - 2).Split(',');
            if (elements.Length != 4)
            {
                v = Vector4.zero;
                return false;
            }

            float x = float.Parse(elements[0]);
            float y = float.Parse(elements[1]);
            float z = float.Parse(elements[2]);
            float w = float.Parse(elements[3]);
            v = new Vector4(x, y, z, w);
            return true;
        }
    }
}
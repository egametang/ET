using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace YIUIFramework
{
    /// <summary>
    /// <see cref="Rect"/>
    /// </summary>
    public static class RectExtensions
    {
        private static Regex parseRegex;
        
        public static Rect Parse(string text)
        {
            Rect value;
            if (!TryParse(text, out value))
            {
                var msg = string.Format(
                    "The string {0} can not convert to Rect.", text);
                throw new FormatException(msg);
            }

            return value;
        }
        
        public static bool TryParse(string text, out Rect rect)
        {
            if (parseRegex == null)
            {
                parseRegex = new Regex(
                    @"^\(x:(.*), y:(.*), width:(.*), height:(.*)\)$");
            }

            var match = parseRegex.Match(text);
            if (!match.Success || match.Groups.Count != 5)
            {
                rect = Rect.zero;
                return false;
            }

            float x;
            if (!float.TryParse(match.Groups[1].Value, out x))
            {
                rect = Rect.zero;
                return false;
            }

            float y;
            if (!float.TryParse(match.Groups[2].Value, out y))
            {
                rect = Rect.zero;
                return false;
            }

            float width;
            if (!float.TryParse(match.Groups[3].Value, out width))
            {
                rect = Rect.zero;
                return false;
            }

            float height;
            if (!float.TryParse(match.Groups[4].Value, out height))
            {
                rect = Rect.zero;
                return false;
            }

            rect = new Rect(x, y, width, height);
            return true;
        }
    }
}
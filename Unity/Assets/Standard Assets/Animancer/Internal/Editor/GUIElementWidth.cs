// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only] A cached width calculation for GUI elements.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/GUIElementWidth
    /// 
    public class GUIElementWidth
    {
        /************************************************************************************************************************/

        private GUIStyle _Style;
        private string _Text;
        private float _Width;

        /************************************************************************************************************************/

        /// <summary>Returns the width the `text` would take up if drawn with the `style`.</summary>
        public float GetWidth(GUIStyle style, string text)
        {
            if (_Style != style || _Text != text)
            {
                _Style = style;
                _Text = text;
                _Width = style.CalculateWidth(text);

                OnRecalculate(style, text);
            }

            return _Width;
        }

        /// <summary>Called when <see cref="GetWidth"/> is called with different parameters.</summary>
        protected virtual void OnRecalculate(GUIStyle style, string text) { }

        /************************************************************************************************************************/
    }

    /// <summary>[Editor-Only]
    /// A cached width calculation for GUI elements which accounts for boldness in prefab overrides.
    /// </summary>
    public sealed class GUIElementWidthBoldable : GUIElementWidth
    {
        /************************************************************************************************************************/

        private float _BoldWidth;

        /// <inheritdoc/>
        protected override void OnRecalculate(GUIStyle style, string text)
        {
            var fontStyle = style.fontStyle;
            style.fontStyle = FontStyle.Bold;
            _BoldWidth = style.CalculateWidth(text);
            style.fontStyle = fontStyle;
        }

        /************************************************************************************************************************/

        /// <summary>Returns the width the `text` would take up if drawn with the `style`.</summary>
        public float GetWidth(GUIStyle style, string text, bool bold)
        {
            var regularWidth = GetWidth(style, text);
            return bold ? _BoldWidth : regularWidth;
        }

        /// <summary>Returns the width the `text` would take up if drawn with the `style`.</summary>
        public float GetWidth(GUIStyle style, string text, SerializedProperty property)
            => GetWidth(style, text, property != null && property.prefabOverride);

        /************************************************************************************************************************/
    }
}

#endif


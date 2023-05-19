// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

#if UNITY_EDITOR

using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only] Various GUI utilities used throughout Animancer.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/AnimancerGUI
    /// 
    public static class AnimancerGUI
    {
        /************************************************************************************************************************/
        #region Standard Values
        /************************************************************************************************************************/

        /// <summary>The highlight color used for fields showing a warning.</summary>
        public static readonly Color
            WarningFieldColor = new Color(1, 0.9f, 0.6f);

        /// <summary>The highlight color used for fields showing an error.</summary>
        public static readonly Color
            ErrorFieldColor = new Color(1, 0.6f, 0.6f);

        /************************************************************************************************************************/

        /// <summary><see cref="GUILayout.ExpandWidth"/> set to false.</summary>
        public static readonly GUILayoutOption[]
            DontExpandWidth = { GUILayout.ExpandWidth(false) };

        /************************************************************************************************************************/

        /// <summary>Wrapper around <see cref="EditorGUIUtility.singleLineHeight"/>.</summary>
        public static float LineHeight => EditorGUIUtility.singleLineHeight;

        /************************************************************************************************************************/

        /// <summary>Wrapper around <see cref="EditorGUIUtility.standardVerticalSpacing"/>.</summary>
        public static float StandardSpacing => EditorGUIUtility.standardVerticalSpacing;

        /************************************************************************************************************************/

        private static float _IndentSize = -1;

        /// <summary>
        /// The number of pixels of indentation for each <see cref="EditorGUI.indentLevel"/> increment.
        /// </summary>
        public static float IndentSize
        {
            get
            {
                if (_IndentSize < 0)
                {
                    var indentLevel = EditorGUI.indentLevel;
                    EditorGUI.indentLevel = 1;
                    _IndentSize = EditorGUI.IndentedRect(new Rect()).x;
                    EditorGUI.indentLevel = indentLevel;
                }

                return _IndentSize;
            }
        }

        /************************************************************************************************************************/

        private static float _ToggleWidth = -1;

        /// <summary>The width of a standard <see cref="GUISkin.toggle"/> with no label.</summary>
        public static float ToggleWidth
        {
            get
            {
                if (_ToggleWidth == -1)
                    _ToggleWidth = GUI.skin.toggle.CalculateWidth(GUIContent.none);
                return _ToggleWidth;
            }
        }

        /************************************************************************************************************************/

        /// <summary>The color of the standard label text.</summary>
        public static Color TextColor => GUI.skin.label.normal.textColor;

        /************************************************************************************************************************/

        /// <summary>
        /// A more compact <see cref="EditorStyles.miniButton"/> with a fixed size as a tiny box.
        /// </summary>
        public static readonly GUIStyle MiniButton = new GUIStyle(EditorStyles.miniButton)
        {
            margin = new RectOffset(0, 0, 2, 0),
            padding = new RectOffset(2, 3, 2, 2),
            alignment = TextAnchor.MiddleCenter,
            fixedHeight = LineHeight,
            fixedWidth = LineHeight - 1
        };

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Layout
        /************************************************************************************************************************/

        /// <summary>Indicates where <see cref="LayoutSingleLineRect"/> should add the <see cref="StandardSpacing"/>.</summary>
        public enum SpacingMode
        {
            /// <summary>No extra space.</summary>
            None,

            /// <summary>Add extra space before the new area.</summary>
            Before,

            /// <summary>Add extra space after the new area.</summary>
            After,

            /// <summary>Add extra space before and after the new area.</summary>
            BeforeAndAfter
        }

        /// <summary>
        /// Uses <see cref="GUILayoutUtility.GetRect(float, float)"/> to get a <see cref="Rect"/> occupying a single
        /// standard line with the <see cref="StandardSpacing"/> added according to the specified `spacing`.
        /// </summary>
        public static Rect LayoutSingleLineRect(SpacingMode spacing = SpacingMode.None)
        {
            Rect rect;
            switch (spacing)
            {
                case SpacingMode.None:
                    return GUILayoutUtility.GetRect(0, LineHeight);

                case SpacingMode.Before:
                    rect = GUILayoutUtility.GetRect(0, LineHeight + StandardSpacing);
                    rect.yMin += StandardSpacing;
                    return rect;

                case SpacingMode.After:
                    rect = GUILayoutUtility.GetRect(0, LineHeight + StandardSpacing);
                    rect.yMax -= StandardSpacing;
                    return rect;

                case SpacingMode.BeforeAndAfter:
                    rect = GUILayoutUtility.GetRect(0, LineHeight + StandardSpacing * 2);
                    rect.yMin += StandardSpacing;
                    rect.yMax -= StandardSpacing;
                    return rect;

                default:
                    throw new ArgumentException($"Unknown {nameof(StandardSpacing)}: " + spacing, nameof(spacing));
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// If the <see cref="Rect.height"/> is positive, this method moves the <see cref="Rect.y"/> by that amount and
        /// adds the <see cref="EditorGUIUtility.standardVerticalSpacing"/>.
        /// </summary>
        public static void NextVerticalArea(ref Rect area)
        {
            if (area.height > 0)
                area.y += area.height + StandardSpacing;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Subtracts the `width` from the left side of the `area` and returns a new <see cref="Rect"/> occupying the
        /// removed section.
        /// </summary>
        public static Rect StealFromLeft(ref Rect area, float width, float padding = 0)
        {
            var newRect = new Rect(area.x, area.y, width, area.height);
            area.xMin += width + padding;
            return newRect;
        }

        /// <summary>
        /// Subtracts the `width` from the right side of the `area` and returns a new <see cref="Rect"/> occupying the
        /// removed section.
        /// </summary>
        public static Rect StealFromRight(ref Rect area, float width, float padding = 0)
        {
            area.width -= width + padding;
            return new Rect(area.xMax + padding, area.y, width, area.height);
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Divides the given `area` such that the fields associated with both labels will have equal space
        /// remaining after the labels themselves.
        /// </summary>
        public static void SplitHorizontally(Rect area, string label0, string label1,
             out float width0, out float width1, out Rect rect0, out Rect rect1)
        {
            width0 = CalculateLabelWidth(label0);
            width1 = CalculateLabelWidth(label1);

            const float Padding = 1;

            rect0 = rect1 = area;

            var remainingWidth = area.width - width0 - width1 - Padding;
            rect0.width = width0 + remainingWidth * 0.5f;
            rect1.xMin = rect0.xMax + Padding;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Calls <see cref="GUIStyle.CalcMinMaxWidth"/> and returns the max width.
        /// </summary>
        public static float CalculateWidth(this GUIStyle style, GUIContent content)
        {
            style.CalcMinMaxWidth(content, out _, out var width);
            return width;
        }

        /// <summary>
        /// Calls <see cref="GUIStyle.CalcMinMaxWidth"/> and returns the max width.
        /// <para></para>
        /// This method uses the <see cref="TempContent(string, string, bool)"/>.
        /// </summary>
        public static float CalculateWidth(this GUIStyle style, string content)
            => style.CalculateWidth(TempContent(content, null, false));

        /************************************************************************************************************************/

        /// <summary>
        /// Creates a <see cref="ConversionCache{TKey, TValue}"/> for calculating the GUI width occupied by text using the
        /// specified `style`.
        /// </summary>
        public static ConversionCache<string, float> CreateWidthCache(GUIStyle style)
            => new ConversionCache<string, float>((text) => style.CalculateWidth(text));

        /************************************************************************************************************************/

        private static ConversionCache<string, float> _LabelWidthCache;

        /// <summary>
        /// Calls <see cref="GUIStyle.CalcMinMaxWidth"/> using <see cref="GUISkin.label"/> and returns the max
        /// width. The result is cached for efficient reuse.
        /// <para></para>
        /// This method uses the <see cref="TempContent(string, string, bool)"/>.
        /// </summary>
        public static float CalculateLabelWidth(string text)
        {
            if (_LabelWidthCache == null)
                _LabelWidthCache = CreateWidthCache(GUI.skin.label);

            return _LabelWidthCache.Convert(text);
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Begins a vertical layout group using the given style and decreases the
        /// <see cref="EditorGUIUtility.labelWidth"/> to compensate for the indentation.
        /// </summary>
        public static void BeginVerticalBox(GUIStyle style)
        {
            if (style == null)
            {
                GUILayout.BeginVertical();
                return;
            }

            GUILayout.BeginVertical(style);
            EditorGUIUtility.labelWidth -= style.padding.left;
        }

        /// <summary>
        /// Ends a layout group started by <see cref="BeginVerticalBox"/> and restores the
        /// <see cref="EditorGUIUtility.labelWidth"/>.
        /// </summary>
        public static void EndVerticalBox(GUIStyle style)
        {
            if (style != null)
                EditorGUIUtility.labelWidth += style.padding.left;

            GUILayout.EndVertical();
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Labels
        /************************************************************************************************************************/

        /// <summary>Used by <see cref="TempContent(string, string, bool)"/>.</summary>
        private static GUIContent _TempContent;

        /// <summary>
        /// Returns a <see cref="GUIContent"/> with the specified parameters. The same instance is returned by
        /// every subsequent call.
        /// </summary>
        public static GUIContent TempContent(string text = null, string tooltip = null, bool narrowText = true)
        {
            AnimancerUtilities.NewIfNull(ref _TempContent);

            if (narrowText)
                text = GetNarrowText(text);

            _TempContent.text = text;
            _TempContent.tooltip = tooltip;
            return _TempContent;
        }

        /// <summary>
        /// Returns a <see cref="GUIContent"/> with the <see cref="SerializedProperty.displayName"/> and
        /// <see cref="SerializedProperty.tooltip"/>. The same instance is returned by every subsequent call.
        /// </summary>
        public static GUIContent TempContent(SerializedProperty property, bool narrowText = true)
            => TempContent(property.displayName, property.tooltip, narrowText);

        /************************************************************************************************************************/

        private static ConversionCache<float, string> _F1Cache;
        private static float _WeightValueWidth = -1;

        /// <summary>
        /// Draws a label showing the `weight` aligned to the right side of the `area` and reduces its
        /// <see cref="Rect.width"/> to remove that label from its area.
        /// </summary>
        public static void DoWeightLabel(ref Rect area, float weight)
        {
            string label;
            if (weight < 0)
            {
                label = "-?";
            }
            else
            {
                if (_F1Cache == null)
                {
                    _F1Cache = new ConversionCache<float, string>((value) => value.ToString("F1"));
                }

                label = _F1Cache.Convert(weight);
            }

            var style = ObjectPool.GetCachedResult(() => new GUIStyle(GUI.skin.label));
            if (_WeightValueWidth < 0)
                _WeightValueWidth = style.CalculateWidth("0.0");

            style.normal.textColor = Color.Lerp(Color.grey, TextColor, weight);
            style.fontStyle = Mathf.Approximately(weight * 10, (int)(weight * 10)) ?
                FontStyle.Normal : FontStyle.Italic;

            var weightArea = StealFromRight(ref area, _WeightValueWidth);

            GUI.Label(weightArea, label, style);
        }

        /************************************************************************************************************************/

        /// <summary>The <see cref="EditorGUIUtility.labelWidth"/> from before <see cref="BeginTightLabel"/>.</summary>
        private static float _TightLabelWidth;

        /// <summary>Stores the <see cref="EditorGUIUtility.labelWidth"/> and changes it to the exact width of the `label`.</summary>
        public static string BeginTightLabel(string label)
        {
            _TightLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = CalculateLabelWidth(label) + EditorGUI.indentLevel * IndentSize;
            return GetNarrowText(label);
        }

        /// <summary>Reverts <see cref="EditorGUIUtility.labelWidth"/> to its previous value.</summary>
        public static void EndTightLabel()
        {
            EditorGUIUtility.labelWidth = _TightLabelWidth;
        }

        /************************************************************************************************************************/

        private static ConversionCache<string, string> _NarrowTextCache;

        /// <summary>
        /// Returns the `text` without any spaces if <see cref="EditorGUIUtility.wideMode"/> is false.
        /// Otherwise simply returns the `text` without any changes.
        /// </summary>
        public static string GetNarrowText(string text)
        {
            if (EditorGUIUtility.wideMode ||
                string.IsNullOrEmpty(text))
                return text;

            if (_NarrowTextCache == null)
                _NarrowTextCache = new ConversionCache<string, string>((str) => str.Replace(" ", ""));

            return _NarrowTextCache.Convert(text);
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Events
        /************************************************************************************************************************/

        /// <summary>
        /// Returns true and uses the current event if it is <see cref="EventType.MouseUp"/> inside the specified
        /// `area`.
        /// </summary>
        public static bool TryUseClickEvent(Rect area, int button = -1)
        {
            var currentEvent = Event.current;
            if (currentEvent.type == EventType.MouseUp &&
                (button < 0 || currentEvent.button == button) &&
                area.Contains(currentEvent.mousePosition))
            {
                GUI.changed = true;
                currentEvent.Use();

                if (currentEvent.button == 2)
                    Deselect();

                return true;
            }
            else return false;
        }

        /// <summary>
        /// Returns true and uses the current event if it is <see cref="EventType.MouseUp"/> inside the last GUI Layout
        /// <see cref="Rect"/> that was drawn.
        /// </summary>
        public static bool TryUseClickEventInLastRect(int button = -1)
            => TryUseClickEvent(GUILayoutUtility.GetLastRect(), button);

        /************************************************************************************************************************/

        /// <summary>
        /// Invokes `onDrop` if the <see cref="Event.current"/> is a drag and drop event inside the `dropArea`.
        /// </summary>
        public static void HandleDragAndDrop<T>(Rect dropArea, Func<T, bool> validate, Action<T> onDrop,
            DragAndDropVisualMode mode = DragAndDropVisualMode.Link) where T : class
        {
            if (!dropArea.Contains(Event.current.mousePosition))
                return;

            bool isDrop;
            switch (Event.current.type)
            {
                case EventType.DragUpdated:
                    isDrop = false;
                    break;

                case EventType.DragPerform:
                    isDrop = true;
                    break;

                default:
                    return;
            }

            TryDrop(DragAndDrop.objectReferences, validate, onDrop, isDrop, mode);
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Updates the <see cref="DragAndDrop.visualMode"/> or calls `onDrop` for each of the `objects`.
        /// </summary>
        private static void TryDrop<T>(IEnumerable objects, Func<T, bool> validate, Action<T> onDrop, bool isDrop,
            DragAndDropVisualMode mode) where T : class
        {
            if (objects == null)
                return;

            var droppedAny = false;

            foreach (var obj in objects)
            {
                var t = obj as T;

                if (t != null && (validate == null || validate(t)))
                {
                    Deselect();

                    if (!isDrop)
                    {
                        DragAndDrop.visualMode = mode;
                        break;
                    }
                    else
                    {
                        onDrop(t);
                        droppedAny = true;
                    }
                }
            }

            if (droppedAny)
                GUIUtility.ExitGUI();
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Uses <see cref="HandleDragAndDrop"/> to deal with drag and drop operations involving
        /// <see cref="AnimationClip"/>s of <see cref="IAnimationClipSource"/>s.
        /// </summary>
        public static void HandleDragAndDropAnimations(Rect dropArea, Action<AnimationClip> onDrop,
            DragAndDropVisualMode mode = DragAndDropVisualMode.Link)
        {
            HandleDragAndDrop(dropArea, (clip) => !clip.legacy, onDrop, mode);

            HandleDragAndDrop<IAnimationClipSource>(dropArea, null, (source) =>
            {
                using (ObjectPool.Disposable.AcquireList<AnimationClip>(out var clips))
                {
                    source.GetAnimationClips(clips);
                    TryDrop(clips, (clip) => !clip.legacy, onDrop, true, mode);
                }
            }, mode);

            HandleDragAndDrop<IAnimationClipCollection>(dropArea, null, (collection) =>
            {
                using (ObjectPool.Disposable.AcquireSet<AnimationClip>(out var clips))
                {
                    collection.GatherAnimationClips(clips);
                    TryDrop(clips, (clip) => !clip.legacy, onDrop, true, mode);
                }
            }, mode);
        }

        /************************************************************************************************************************/

        /// <summary>Deselects any selected IMGUI control.</summary>
        public static void Deselect() => GUIUtility.keyboardControl = 0;

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Fields
        /************************************************************************************************************************/

        /// <summary>
        /// Draw a <see cref="EditorGUI.FloatField(Rect, GUIContent, float)"/> with an alternate cached string when it
        /// is not selected (for example, "1" might become "1s" to indicate "seconds").
        /// </summary>
        public static float DoSpecialFloatField(Rect area, GUIContent label, float value, ConversionCache<float, string> toString)
        {
            // Treat most events normally, but when repainting show a text field with the cached string.

            if (label != null)
            {
                if (Event.current.type != EventType.Repaint)
                    return EditorGUI.FloatField(area, label, value);

                var dragArea = new Rect(area.x, area.y, EditorGUIUtility.labelWidth, area.height);
                EditorGUIUtility.AddCursorRect(dragArea, MouseCursor.SlideArrow);

                EditorGUI.TextField(area, label, toString.Convert(value));
            }
            else
            {
                var indentLevel = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;

                if (Event.current.type != EventType.Repaint)
                    value = EditorGUI.FloatField(area, value);
                else
                    EditorGUI.TextField(area, toString.Convert(value));

                EditorGUI.indentLevel = indentLevel;
            }

            return value;
        }

        /// <summary>
        /// Draw a <see cref="EditorGUI.FloatField(Rect, GUIContent, float)"/> with an alternate cached string when it
        /// is not selected (for example, "1" might become "1s" to indicate "seconds").
        /// </summary>
        public static void DoFloatFieldWithSuffix(Rect area, GUIContent label, SerializedProperty property,
            ConversionCache<float, string> toString)
        {
            label = EditorGUI.BeginProperty(area, label, property);
            EditorGUI.BeginChangeCheck();
            var value = DoSpecialFloatField(area, label, property.floatValue, toString);
            if (EditorGUI.EndChangeCheck())
                property.floatValue = value;
            EditorGUI.EndProperty();
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Draw a <see cref="GUI.Toggle(Rect, bool, GUIContent)"/> which sets the value to <see cref="float.NaN"/>
        /// when disabled followed by two float fields to display the <see cref="SerializedProperty.floatValue"/> as
        /// both normalized time and seconds.
        /// </summary>
        public static void DoOptionalTimeField(ref Rect area, GUIContent label, SerializedProperty property,
            bool timeIsNormalized, float length, float defaultValue = 0, bool isOptional = true)
        {
            label = EditorGUI.BeginProperty(area, label, property);
            EditorGUI.BeginChangeCheck();

            var value = DoOptionalTimeField(ref area, label, property.floatValue, timeIsNormalized,
                length, defaultValue, isOptional);

            if (EditorGUI.EndChangeCheck())
                property.floatValue = value;
            EditorGUI.EndProperty();
        }

        /************************************************************************************************************************/

        private static ConversionCache<float, string> _XSuffixCache, _SSuffixCache;

        /// <summary>
        /// Draw a <see cref="GUI.Toggle(Rect, bool, GUIContent)"/> which sets the value to <see cref="float.NaN"/>
        /// when disabled followed by two float fields to display the `time` both normalized and in seconds.
        /// </summary>
        public static float DoOptionalTimeField(ref Rect area, GUIContent label, float time, bool timeIsNormalized,
             float length, float defaultValue = 0, bool isOptional = true)
        {
            if (_XSuffixCache == null)
            {
                _XSuffixCache = new ConversionCache<float, string>((x) => x + "x");
                _SSuffixCache = new ConversionCache<float, string>((s) => s + "s");
            }

            area.height = LineHeight;

            bool showNormalized, showSeconds;
            if (length > 0)
            {
                showNormalized = showSeconds = true;
            }
            else
            {
                showNormalized = timeIsNormalized;
                showSeconds = !timeIsNormalized;
            }

            var labelWidth = EditorGUIUtility.labelWidth;
            var enabled = GUI.enabled;

            var toggleArea = area;
            if (isOptional)
            {
                toggleArea.x += EditorGUIUtility.labelWidth;

                toggleArea.width = ToggleWidth;
                EditorGUIUtility.labelWidth += toggleArea.width;

                EditorGUIUtility.AddCursorRect(toggleArea, MouseCursor.Arrow);

                // We need to draw the toggle after everything else to it goes on top of the label. But we want it to
                // get priority for input events, so we disable the other controls during those events in its area.
                var currentEvent = Event.current;
                if (enabled && toggleArea.Contains(currentEvent.mousePosition))
                {
                    switch (currentEvent.type)
                    {
                        case EventType.Repaint:
                        case EventType.Layout:
                            break;

                        default:
                            GUI.enabled = false;
                            break;
                    }
                }
            }
            else if (float.IsNaN(time))
            {
                time = defaultValue;
            }

            var displayTime = float.IsNaN(time) ? defaultValue : time;

            var normalizedArea = area;
            var secondsArea = area;

            if (showNormalized)
            {
                if (showSeconds)
                {
                    var split = (EditorGUIUtility.labelWidth + normalizedArea.xMax - StandardSpacing) * 0.5f;
                    normalizedArea.xMax = split;
                    secondsArea.xMin = split + StandardSpacing;
                }

                var normalizedTime = timeIsNormalized ? displayTime : displayTime / length;

                EditorGUI.BeginChangeCheck();
                normalizedTime = DoSpecialFloatField(normalizedArea, label, normalizedTime, _XSuffixCache);
                if (EditorGUI.EndChangeCheck())
                    time = timeIsNormalized ? normalizedTime : normalizedTime * length;
            }

            EditorGUIUtility.labelWidth = labelWidth;

            if (showSeconds)
            {
                var rawTime = timeIsNormalized ? displayTime * length : displayTime;

                if (showNormalized)
                    label = null;

                EditorGUI.BeginChangeCheck();
                rawTime = DoSpecialFloatField(secondsArea, label, rawTime, _SSuffixCache);
                if (EditorGUI.EndChangeCheck())
                {
                    if (timeIsNormalized)
                    {
                        if (length != 0)
                            time = rawTime / length;
                    }
                    else
                    {
                        time = rawTime;
                    }
                }
            }

            GUI.enabled = enabled;

            if (isOptional)
                DoOptionalTimeToggle(toggleArea, ref time, defaultValue);

            return time;
        }

        /************************************************************************************************************************/

        private static void DoOptionalTimeToggle(Rect area, ref float time, float defaultValue)
        {
#if UNITY_2019_3_OR_NEWER
            area.x += 2;
#endif

            var wasEnabled = !float.IsNaN(time);

            // Use the EditorGUI method instead to properly handle EditorGUI.showMixedValue.
            //var isEnabled = GUI.Toggle(area, wasEnabled, GUIContent.none);

            var indentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            var isEnabled = EditorGUI.Toggle(area, wasEnabled);

            EditorGUI.indentLevel = indentLevel;

            if (isEnabled != wasEnabled)
            {
                time = isEnabled ? defaultValue : float.NaN;
                Deselect();
            }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}

#endif


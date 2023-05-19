// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using static Animancer.Editor.AnimancerPlayableDrawer;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only] Draws the Inspector GUI for an <see cref="AnimancerState"/>.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/AnimancerStateDrawer_1
    /// 
    public class AnimancerStateDrawer<T> : AnimancerNodeDrawer<T> where T : AnimancerState
    {
        /************************************************************************************************************************/

        /// <summary>
        /// Creates a new <see cref="AnimancerStateDrawer{T}"/> to manage the Inspector GUI for the `target`.
        /// </summary>
        public AnimancerStateDrawer(T target) => Target = target;

        /************************************************************************************************************************/

        /// <summary>The <see cref="GUIStyle"/> used for the area encompassing this drawer is <c>null</c>.</summary>
        protected override GUIStyle RegionStyle => null;

        /************************************************************************************************************************/

        /// <summary>Determines whether the <see cref="AnimancerState.MainObject"/> field can occupy the whole line.</summary>
        private bool IsAssetUsedAsKey =>
            Target.DebugName == null &&
            (Target.Key == null || ReferenceEquals(Target.Key, Target.MainObject));

        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override bool AutoNormalizeSiblingWeights => AutoNormalizeWeights;

        /************************************************************************************************************************/

        /// <summary>
        /// Draws the state's main label: an <see cref="Object"/> field if it has a
        /// <see cref="AnimancerState.MainObject"/>, otherwise just a simple text label.
        /// <para></para>
        /// Also shows a bar to indicate its progress.
        /// </summary>
        protected override void DoLabelGUI(Rect area)
        {
            string label;
            if (Target.DebugName != null)
            {
                label = Target.DebugName;
            }
            else if (IsAssetUsedAsKey)
            {
                label = "";
            }
            else
            {
                var key = Target.Key;
                if (key is string str)
                    label = $"\"{str}\"";
                else
                    label = key.ToString();
            }

            HandleLabelClick(area);

            AnimancerGUI.DoWeightLabel(ref area, Target.Weight);

            AnimationBindings.DoBindingMatchGUI(ref area, Target);

            var mainObject = Target.MainObject;
            if (!ReferenceEquals(mainObject, null))
            {
                EditorGUI.BeginChangeCheck();

                mainObject = EditorGUI.ObjectField(area, label, mainObject, typeof(Object), false);

                if (EditorGUI.EndChangeCheck())
                    Target.MainObject = mainObject;
            }
            else if (Target.DebugName != null)
            {
                EditorGUI.LabelField(area, Target.DebugName);
            }
            else
            {
                EditorGUI.LabelField(area, label, Target.ToString());
            }

            // Highlight a section of the label based on the time like a loading bar.
            area.width -= 18;// Remove the area for the Object Picker icon to line the bar up with the field.
            DoTimeHighlightBarGUI(area, Target.IsPlaying, Target.EffectiveWeight, Target.Time, Target.Length, Target.IsLooping);
        }

        /************************************************************************************************************************/

        /// <summary>Draws a progress bar to show the animation time.</summary>
        public static void DoTimeHighlightBarGUI(Rect area, bool isPlaying, float weight, float time, float length, bool isLooping)
        {
            var color = GUI.color;

            if (ScaleTimeBarByWeight)
            {
                var height = area.height;
                area.height *= Mathf.Clamp01(weight) * 0.75f + 0.25f;
                area.y += height - area.height;
            }

            // Green = Playing, Yelow = Paused.
            GUI.color = isPlaying ? new Color(0.15f, 0.7f, 0.15f, 0.35f) : new Color(0.7f, 0.7f, 0.15f, 0.35f);

            area = EditorGUI.IndentedRect(area);

            var wrappedTime = GetWrappedTime(time, length, isLooping);
            if (length > 0)
                area.width *= Mathf.Clamp01(wrappedTime / length);

            GUI.DrawTexture(area, Texture2D.whiteTexture);

            GUI.color = color;
        }

        /************************************************************************************************************************/

        /// <summary>Handles Ctrl + Click on the label to CrossFade the animation.</summary>
        private void HandleLabelClick(Rect area)
        {
            var currentEvent = Event.current;
            if (currentEvent.type != EventType.MouseUp ||
                !currentEvent.control ||
                !area.Contains(currentEvent.mousePosition))
                return;

            currentEvent.Use();

            Target.Root.UnpauseGraph();
            var fadeDuration = Target.CalculateEditorFadeDuration(AnimancerPlayable.DefaultFadeDuration);
            Target.Root.Play(Target, fadeDuration);
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override void DoFoldoutGUI(Rect area)
        {
            float foldoutWidth;
            if (IsAssetUsedAsKey)
            {
                foldoutWidth = EditorGUI.indentLevel * AnimancerGUI.IndentSize;
            }
            else
            {
                foldoutWidth = EditorGUIUtility.labelWidth;
            }

            area.xMin -= 2;
            area.width = foldoutWidth;

            var hierarchyMode = EditorGUIUtility.hierarchyMode;
            EditorGUIUtility.hierarchyMode = true;

            IsExpanded = EditorGUI.Foldout(area, IsExpanded, GUIContent.none, true);

            EditorGUIUtility.hierarchyMode = hierarchyMode;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Gets the current <see cref="AnimancerState.Time"/>.
        /// If the state is looping, the value is modulo by the <see cref="AnimancerState.Length"/>.
        /// </summary>
        private float GetWrappedTime(out float length) => GetWrappedTime(Target.Time, length = Target.Length, Target.IsLooping);

        /// <summary>
        /// Gets the current <see cref="AnimancerState.Time"/>.
        /// If the state is looping, the value is modulo by the <see cref="AnimancerState.Length"/>.
        /// </summary>
        private static float GetWrappedTime(float time, float length, bool isLooping)
        {
            var wrappedTime = time;

            if (isLooping)
            {
                wrappedTime = AnimancerUtilities.Wrap(wrappedTime, length);
                if (wrappedTime == 0 && time != 0)
                    wrappedTime = length;
            }

            return wrappedTime;
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override void DoDetailsGUI()
        {
            if (!IsExpanded)
                return;

            EditorGUI.indentLevel++;
            DoTimeSliderGUI();
            DoNodeDetailsGUI();
            DoOnEndGUI();
            EditorGUI.indentLevel--;
        }

        /************************************************************************************************************************/

        /// <summary>Draws a slider for controlling the current <see cref="AnimancerState.Time"/>.</summary>
        private void DoTimeSliderGUI()
        {
            if (Target.Length <= 0)
                return;

            var time = GetWrappedTime(out var length);

            if (length == 0)
                return;

            var area = AnimancerGUI.LayoutSingleLineRect(AnimancerGUI.SpacingMode.Before);

            var normalized = DoNormalizedTimeToggle(ref area);

            string label;
            float max;
            if (normalized)
            {
                label = "Normalized Time";
                time /= length;
                max = 1;
            }
            else
            {
                label = "Time";
                max = length;
            }

            DoLoopCounterGUI(ref area, length);

            EditorGUI.BeginChangeCheck();
            label = AnimancerGUI.BeginTightLabel(label);
            time = EditorGUI.Slider(area, label, time, 0, max);
            AnimancerGUI.EndTightLabel();
            if (AnimancerGUI.TryUseClickEvent(area, 2))
                time = 0;
            if (EditorGUI.EndChangeCheck())
            {
                if (normalized)
                    Target.NormalizedTime = time;
                else
                    Target.Time = time;
            }
        }

        /************************************************************************************************************************/

        private bool DoNormalizedTimeToggle(ref Rect area)
        {
            var content = AnimancerGUI.TempContent("N");
            var style = AnimancerGUI.MiniButton;

            var width = UseNormalizedTimeSlidersWidth.GetWidth(style, content.text);
            var toggleArea = AnimancerGUI.StealFromRight(ref area, width);

            UseNormalizedTimeSliders.Value = GUI.Toggle(toggleArea, UseNormalizedTimeSliders, content, style);
            return UseNormalizedTimeSliders;
        }

        /************************************************************************************************************************/

        private static ConversionCache<int, string> _LoopCounterCache;

        private void DoLoopCounterGUI(ref Rect area, float length)
        {
            if (_LoopCounterCache == null)
                _LoopCounterCache = new ConversionCache<int, string>((x) => "x" + x);

            string label;
            var normalizedTime = Target.Time / length;
            if (float.IsNaN(normalizedTime))
            {
                label = "NaN";
            }
            else
            {
                var loops = Mathf.FloorToInt(Target.Time / length);
                label = _LoopCounterCache.Convert(loops);
            }

            var width = AnimancerGUI.CalculateLabelWidth(label);

            var labelArea = AnimancerGUI.StealFromRight(ref area, width);

            GUI.Label(labelArea, label);
        }

        /************************************************************************************************************************/

        private void DoOnEndGUI()
        {
            if (!Target.HasEvents || Target.Events.OnEnd == null)
                return;

            var area = AnimancerGUI.LayoutSingleLineRect(AnimancerGUI.SpacingMode.Before);

            EditorGUI.LabelField(area, "OnEnd: " + Target.Events.OnEnd.Method);
        }

        /************************************************************************************************************************/
        #region Context Menu
        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override void PopulateContextMenu(GenericMenu menu)
        {
            AddContextMenuFunctions(menu);

            menu.AddFunction("Play",
                !Target.IsPlaying || Target.Weight != 1,
                () =>
                {
                    Target.Root.UnpauseGraph();
                    Target.Root.Play(Target);
                });

            AnimancerEditorUtilities.AddFadeFunction(menu, "Cross Fade (Ctrl + Click)",
                Target.Weight != 1,
                Target, (duration) =>
                {
                    Target.Root.UnpauseGraph();
                    Target.Root.Play(Target, duration);
                });

            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Destroy State"), false, () => Target.Destroy());

            menu.AddSeparator("");

            AddDisplayOptions(menu);

            AnimancerEditorUtilities.AddDocumentationLink(menu, "State Documentation", Strings.DocsURLs.States);
        }

        /************************************************************************************************************************/

        /// <summary>Adds the details of this state to the `menu`.</summary>
        protected virtual void AddContextMenuFunctions(GenericMenu menu)
        {
            menu.AddDisabledItem(new GUIContent($"{DetailsPrefix}{nameof(AnimancerState.Key)}: {Target.Key}"));

            var length = Target.Length;
            if (!float.IsNaN(length))
                menu.AddDisabledItem(new GUIContent($"{DetailsPrefix}{nameof(AnimancerState.Length)}: {length}"));

            menu.AddDisabledItem(new GUIContent($"{DetailsPrefix}Playable Path: {Target.GetPath()}"));

            var mainAsset = Target.MainObject;
            if (mainAsset != null)
            {
                var assetPath = AssetDatabase.GetAssetPath(mainAsset);
                if (assetPath != null)
                    menu.AddDisabledItem(new GUIContent($"{DetailsPrefix}Asset Path: {assetPath.Replace("/", "->")}"));
            }

            if (Target.HasEvents)
            {
                var events = Target.Events;
                for (int i = 0; i < events.Count; i++)
                {
                    var index = i;
                    AddEventFunctions(menu, "Event " + index, events[index],
                        () => events.SetCallback(index, AnimancerEvent.DummyCallback),
                        () => events.Remove(index));
                }

                AddEventFunctions(menu, "End Event", events.endEvent,
                    () => events.endEvent = new AnimancerEvent(float.NaN, null), null);
            }
        }

        /************************************************************************************************************************/

        private void AddEventFunctions(GenericMenu menu, string name, AnimancerEvent animancerEvent,
            GenericMenu.MenuFunction clearEvent, GenericMenu.MenuFunction removeEvent)
        {
            name = $"Events/{name}/";

            menu.AddDisabledItem(new GUIContent($"{name}{nameof(AnimancerState.NormalizedTime)}: {animancerEvent.normalizedTime}"));

            bool canInvoke;
            if (animancerEvent.callback == null)
            {
                menu.AddDisabledItem(new GUIContent(name + "Callback: null"));
                canInvoke = false;
            }
            else if (animancerEvent.callback == AnimancerEvent.DummyCallback)
            {
                menu.AddDisabledItem(new GUIContent(name + "Callback: Dummy"));
                canInvoke = false;
            }
            else
            {
                var label = name +
                    (animancerEvent.callback.Target != null ? ("Target: " + animancerEvent.callback.Target) : "Target: null");

                var targetObject = animancerEvent.callback.Target as Object;
                menu.AddFunction(label,
                    targetObject != null,
                    () => Selection.activeObject = targetObject);

                menu.AddDisabledItem(new GUIContent(
                    $"{name}Declaring Type: {animancerEvent.callback.Method.DeclaringType.FullName}"));

                menu.AddDisabledItem(new GUIContent(
                    $"{name}Method: {animancerEvent.callback.Method}"));

                canInvoke = true;
            }

            if (clearEvent != null)
                menu.AddFunction(name + "Clear", canInvoke || !float.IsNaN(animancerEvent.normalizedTime), clearEvent);

            if (removeEvent != null)
                menu.AddFunction(name + "Remove", true, removeEvent);

            menu.AddFunction(name + "Invoke", canInvoke, () => animancerEvent.Invoke(Target));
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}

#endif


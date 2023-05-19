// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Animancer.Editor
{
    /// <summary>[Internal]
    /// A custom Inspector for an <see cref="AnimancerLayer"/> which sorts and exposes some of its internal values.
    /// </summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/AnimancerLayerDrawer
    /// 
    public sealed class AnimancerLayerDrawer : AnimancerNodeDrawer<AnimancerLayer>
    {
        /************************************************************************************************************************/

        /// <summary>The states in the target layer which have non-zero <see cref="AnimancerNode.Weight"/>.</summary>
        public readonly List<AnimancerState> ActiveStates = new List<AnimancerState>();

        /// <summary>The states in the target layer which have zero <see cref="AnimancerNode.Weight"/>.</summary>
        public readonly List<AnimancerState> InactiveStates = new List<AnimancerState>();

        /************************************************************************************************************************/

        /// <summary>The <see cref="GUIStyle"/> used for the area encompassing this drawer is <see cref="GUISkin.box"/>.</summary>
        protected override GUIStyle RegionStyle => GUI.skin.box;

        /************************************************************************************************************************/
        #region Gathering
        /************************************************************************************************************************/

        /// <summary>
        /// Initialises an editor in the list for each layer in the `animancer`.
        /// <para></para>
        /// The `count` indicates the number of elements actually being used. Spare elements are kept in the list in
        /// case they need to be used again later.
        /// </summary>
        internal static void GatherLayerEditors(AnimancerPlayable animancer, List<AnimancerLayerDrawer> editors, out int count)
        {
            count = animancer.Layers.Count;
            for (int i = 0; i < count; i++)
            {
                AnimancerLayerDrawer editor;
                if (editors.Count <= i)
                {
                    editor = new AnimancerLayerDrawer();
                    editors.Add(editor);
                }
                else
                {
                    editor = editors[i];
                }

                editor.GatherStates(animancer.Layers._Layers[i]);
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Sets the target `layer` and sorts its states and their keys into the active/inactive lists.
        /// </summary>
        private void GatherStates(AnimancerLayer layer)
        {
            Target = layer;

            ActiveStates.Clear();
            InactiveStates.Clear();

            foreach (var state in layer)
            {
                if (AnimancerPlayableDrawer.HideInactiveStates && state.Weight == 0)
                    continue;

                if (!AnimancerPlayableDrawer.SeparateActiveFromInactiveStates || state.Weight != 0)
                {
                    ActiveStates.Add(state);
                }
                else
                {
                    InactiveStates.Add(state);
                }
            }

            SortAndGatherKeys(ActiveStates);
            SortAndGatherKeys(InactiveStates);
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Sorts any entries that use another state as their key to come right after that state.
        /// See <see cref="AnimancerPlayable.Play(AnimancerState, float, FadeMode)"/>.
        /// </summary>
        private static void SortAndGatherKeys(List<AnimancerState> states)
        {
            var count = states.Count;
            if (count == 0)
                return;

            if (AnimancerPlayableDrawer.SortStatesByName)
            {
                states.Sort((x, y) =>
                {
                    if (x.MainObject == null)
                        return y.MainObject == null ? 0 : 1;
                    else if (y.MainObject == null)
                        return -1;

                    return x.MainObject.name.CompareTo(y.MainObject.name);
                });
            }

            // Sort any states that use another state as their key to be right after the key.
            for (int i = 0; i < count; i++)
            {
                var state = states[i];
                var key = state.Key;

                var keyState = key as AnimancerState;
                if (keyState == null)
                    continue;

                var keyStateIndex = states.IndexOf(keyState);
                if (keyStateIndex < 0 || keyStateIndex + 1 == i)
                    continue;

                states.RemoveAt(i);

                if (keyStateIndex < i)
                    keyStateIndex++;

                states.Insert(keyStateIndex, state);

                i--;
            }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/

        /// <summary>Draws the layer's name and weight.</summary>
        protected override void DoLabelGUI(Rect area)
        {
            var label = Target.IsAdditive ? "Additive" : "Override";
            if (Target._Mask != null)
                label = $"{label} ({Target._Mask.name})";

            area.xMin += FoldoutIndent;

            AnimancerGUI.DoWeightLabel(ref area, Target.Weight);

            EditorGUIUtility.labelWidth -= FoldoutIndent;
            EditorGUI.LabelField(area, Target.ToString(), label);
            EditorGUIUtility.labelWidth += FoldoutIndent;
        }

        /************************************************************************************************************************/

        /// <summary>The number of pixels of indentation required to fit the foldout arrow.</summary>
        const float FoldoutIndent = 12;

        /// <inheritdoc/>
        protected override void DoFoldoutGUI(Rect area)
        {
            var hierarchyMode = EditorGUIUtility.hierarchyMode;
            EditorGUIUtility.hierarchyMode = true;

            area.xMin += FoldoutIndent;
            IsExpanded = EditorGUI.Foldout(area, IsExpanded, GUIContent.none, true);

            EditorGUIUtility.hierarchyMode = hierarchyMode;
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override void DoDetailsGUI()
        {
            if (IsExpanded)
            {
                EditorGUI.indentLevel++;
                GUILayout.BeginHorizontal();
                GUILayout.Space(FoldoutIndent);
                GUILayout.BeginVertical();

                DoLayerDetailsGUI();
                DoNodeDetailsGUI();

                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
                EditorGUI.indentLevel--;
            }

            DoStatesGUI();
        }

        /************************************************************************************************************************/

        private static readonly GUIElementWidth AdditiveToggleWidth = new GUIElementWidth();
        private static readonly GUIElementWidth AdditiveLabelWidth = new GUIElementWidth();

        /// <summary>
        /// Draws controls for <see cref="AnimancerLayer.IsAdditive"/> and <see cref="AnimancerLayer._Mask"/>.
        /// </summary>
        private void DoLayerDetailsGUI()
        {
            var area = AnimancerGUI.LayoutSingleLineRect(AnimancerGUI.SpacingMode.Before);
            area = EditorGUI.IndentedRect(area);

            var labelWidth = EditorGUIUtility.labelWidth;
            var indentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            var additiveLabel = AnimancerGUI.GetNarrowText("Is Additive");

            var additiveWidth = AdditiveToggleWidth.GetWidth(GUI.skin.toggle, additiveLabel);
            var maskRect = AnimancerGUI.StealFromRight(ref area, area.width - additiveWidth);

            // Additive.
            EditorGUIUtility.labelWidth = AdditiveLabelWidth.GetWidth(GUI.skin.label, additiveLabel);
            Target.IsAdditive = EditorGUI.Toggle(area, additiveLabel, Target.IsAdditive);

            // Mask.
            var maskLabel = AnimancerGUI.TempContent("Mask");
            EditorGUIUtility.labelWidth = GUI.skin.label.CalculateWidth(maskLabel);
            EditorGUI.BeginChangeCheck();
            Target._Mask = (AvatarMask)EditorGUI.ObjectField(maskRect, maskLabel, Target._Mask, typeof(AvatarMask), false);
            if (EditorGUI.EndChangeCheck())
                Target.SetMask(Target._Mask);

            EditorGUI.indentLevel = indentLevel;
            EditorGUIUtility.labelWidth = labelWidth;
        }

        /************************************************************************************************************************/

        private void DoStatesGUI()
        {
            if (AnimancerPlayableDrawer.HideInactiveStates)
            {
                DoStatesGUI("Active States", ActiveStates);
            }
            else if (AnimancerPlayableDrawer.SeparateActiveFromInactiveStates)
            {
                DoStatesGUI("Active States", ActiveStates);
                DoStatesGUI("Inactive States", InactiveStates);
            }
            else
            {
                DoStatesGUI("States", ActiveStates);
            }

            if (Target.Index == 0 &&
                Target.Weight != 0 &&
                !Target.IsAdditive &&
                !Mathf.Approximately(Target.GetTotalWeight(), 1))
            {
                EditorGUILayout.HelpBox(
                    "The total Weight of all states in this layer does not equal 1, which will likely give undesirable results." +
                    " Click here for more information.",
                    MessageType.Warning);

                if (AnimancerGUI.TryUseClickEventInLastRect())
                    EditorUtility.OpenWithDefaultApp(Strings.DocsURLs.Fading);
            }
        }

        /************************************************************************************************************************/

        /// <summary>Draws all `states` in the given list.</summary>
        private void DoStatesGUI(string label, List<AnimancerState> states)
        {
            var area = AnimancerGUI.LayoutSingleLineRect();

            const string Label = "Weight";
            var width = AnimancerGUI.CalculateLabelWidth(Label);
            GUI.Label(AnimancerGUI.StealFromRight(ref area, width), Label);

            EditorGUI.LabelField(area, label, states.Count.ToString());

            EditorGUI.indentLevel++;
            for (int i = 0; i < states.Count; i++)
            {
                DoStateGUI(states[i]);
            }
            EditorGUI.indentLevel--;
        }

        /************************************************************************************************************************/

        /// <summary>Cached Inspectors that have already been created for states.</summary>
        private readonly Dictionary<AnimancerState, IAnimancerNodeDrawer>
            StateInspectors = new Dictionary<AnimancerState, IAnimancerNodeDrawer>();

        /// <summary>Draws the Inspector for the given `state`.</summary>
        private void DoStateGUI(AnimancerState state)
        {
            if (!StateInspectors.TryGetValue(state, out var inspector))
            {
                inspector = state.CreateDrawer();
                StateInspectors.Add(state, inspector);
            }

            inspector.DoGUI();
            DoChildStatesGUI(state);
        }

        /************************************************************************************************************************/

        /// <summary>Draws all child states of the `state`.</summary>
        private void DoChildStatesGUI(AnimancerState state)
        {
            EditorGUI.indentLevel++;

            foreach (var child in state)
            {
                if (child == null)
                    continue;

                DoStateGUI(child);
            }

            EditorGUI.indentLevel--;
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void DoGUI()
        {
            if (!Target.IsValid)
                return;

            base.DoGUI();

            var area = GUILayoutUtility.GetLastRect();
            HandleDragAndDropAnimations(area, Target.Root.Component, Target.Index);
        }

        /// <summary>
        /// If <see cref="AnimationClip"/>s or <see cref="IAnimationClipSource"/>s are dropped inside the `dropArea`,
        /// this method creates a new state in the `target` for each animation.
        /// </summary>
        public static void HandleDragAndDropAnimations(Rect dropArea, IAnimancerComponent target, int layerIndex)
        {
            AnimancerGUI.HandleDragAndDropAnimations(dropArea, (clip) =>
            {
                target.Playable.Layers[layerIndex].GetOrCreateState(clip);
            });
        }

        /************************************************************************************************************************/
        #region Context Menu
        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override void PopulateContextMenu(GenericMenu menu)
        {
            menu.AddDisabledItem(new GUIContent($"{DetailsPrefix}{nameof(Target.CurrentState)}: {Target.CurrentState}"));
            menu.AddDisabledItem(new GUIContent($"{DetailsPrefix}{nameof(Target.CommandCount)}: {Target.CommandCount}"));

            menu.AddFunction("Stop",
                HasAnyStates((state) => state.IsPlaying || state.Weight != 0),
                () => Target.Stop());

            AnimancerEditorUtilities.AddFadeFunction(menu, "Fade In",
                Target.Index > 0 && Target.Weight != 1, Target,
                (duration) => Target.StartFade(1, duration));
            AnimancerEditorUtilities.AddFadeFunction(menu, "Fade Out",
                Target.Index > 0 && Target.Weight != 0, Target,
                (duration) => Target.StartFade(0, duration));

            AnimancerEditorUtilities.AddContextMenuIK(menu, Target);

            menu.AddSeparator("");

            menu.AddFunction("Destroy States",
                ActiveStates.Count > 0 || InactiveStates.Count > 0,
                () => Target.DestroyStates());

            AnimancerPlayableDrawer.AddRootFunctions(menu, Target.Root);

            menu.AddSeparator("");

            AnimancerPlayableDrawer.AddDisplayOptions(menu);

            AnimancerEditorUtilities.AddDocumentationLink(menu, "Layer Documentation", Strings.DocsURLs.Layers);

            menu.ShowAsContext();
        }

        /************************************************************************************************************************/

        private bool HasAnyStates(Func<AnimancerState, bool> condition)
        {
            foreach (var state in Target)
            {
                if (condition(state))
                    return true;
            }

            return false;
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}

#endif


// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Animancer.Editor
{
    internal partial class TransitionPreviewWindow
    {
        /// <summary>Inspector panel for the <see cref="TransitionPreviewWindow"/>.</summary>
        /// <remarks>
        /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/transitions#previews">Previews</see>
        /// </remarks>
        [Serializable]
        private sealed class Inspector
        {
            /************************************************************************************************************************/

            public const string
                PreviousAnimationKey = "Previous Animation",
                NextAnimationKey = "Next Animation";

            private static readonly string[]
                TabNames = { "Preview", "Settings" };

            /************************************************************************************************************************/

            [SerializeField] private int _CurrentTab;

            [NonSerialized] private AnimationClip[] _OtherAnimations;

            [SerializeField]
            private AnimationClip _PreviousAnimation;
            public AnimationClip PreviousAnimation => _PreviousAnimation;

            [SerializeField]
            private AnimationClip _NextAnimation;
            public AnimationClip NextAnimation => _NextAnimation;

            private readonly AnimancerPlayableDrawer
                PlayableDrawer = new AnimancerPlayableDrawer();

            /************************************************************************************************************************/

            [NonSerialized]
            private bool _IsResizing;

            public void DoResizeGUI(Rect borderArea)
            {
                borderArea.width = 10;
                borderArea.x -= borderArea.width * 0.5f;

                EditorGUIUtility.AddCursorRect(borderArea, MouseCursor.ResizeHorizontal);

                var currentEvent = Event.current;
                switch (currentEvent.type)
                {
                    case EventType.MouseDown:
                        _IsResizing = borderArea.Contains(currentEvent.mousePosition);
                        if (_IsResizing)
                            currentEvent.Use();
                        break;

                    case EventType.MouseUp:
                        if (_IsResizing)
                        {
                            _IsResizing = false;
                            currentEvent.Use();
                        }
                        break;

                    case EventType.MouseDrag:
                        if (_IsResizing)
                        {
                            Settings.InspectorWidth = Mathf.Clamp(
                                Settings.InspectorWidth - currentEvent.delta.x,
                                250,
                                _Instance.position.width - 250);
                            currentEvent.Use();
                        }
                        break;
                }
            }

            /************************************************************************************************************************/

            [SerializeField]
            private Vector2 _Scroll;

            public void DoInspectorGUI()
            {
                var hierarchyMode = EditorGUIUtility.hierarchyMode;
                EditorGUIUtility.hierarchyMode = true;

                var width = Settings.InspectorWidth;
                EditorGUIUtility.labelWidth = Math.Max(width * 0.55f - 60, 100);
                EditorGUIUtility.wideMode = width > 300;

                GUILayout.BeginVertical(GUILayout.Width(width));
                {
                    GUILayout.Space(AnimancerGUI.StandardSpacing * 2);
                    _CurrentTab = GUILayout.Toolbar(_CurrentTab, TabNames);
                    _CurrentTab = Mathf.Clamp(_CurrentTab, 0, TabNames.Length - 1);

                    _Scroll = GUILayout.BeginScrollView(_Scroll, GUILayout.Width(width));

                    switch (_CurrentTab)
                    {
                        case 0: DoPreviewInspectorGUI(); break;
                        case 1: Settings.DoInspectorGUI(); break;
                        default: GUILayout.Label("Tab index is out of bounds"); break;
                    }

                    GUILayout.EndScrollView();
                }
                GUILayout.EndVertical();

                EditorGUIUtility.hierarchyMode = hierarchyMode;
            }

            /************************************************************************************************************************/
            #region Preview Tab
            /************************************************************************************************************************/

            private void DoPreviewInspectorGUI()
            {
                if (!_Instance._TransitionProperty.IsValid())
                {
                    GUILayout.Label("No target property");
                    _Instance.DestroyTransitionProperty();
                    return;
                }

                DoTransitionPropertyGUI();
                DoTransitionGUI();
                DoPreviewSettingsGUI();

                var animancer = _Instance._Scene.InstanceAnimancer;
                if (animancer != null)
                {
                    PlayableDrawer.DoGUI(animancer.Component);
                    if (animancer.IsGraphPlaying)
                        GUI.changed = true;
                }
            }

            /************************************************************************************************************************/

            private void DoTransitionPropertyGUI()
            {
                var property = _Instance._TransitionProperty;
                property.Update();

                var enabled = GUI.enabled;
                GUI.enabled = false;
                {
                    EditorGUI.showMixedValue = property.TargetObjects.Length > 1;
                    EditorGUILayout.ObjectField(property.TargetObject, typeof(Object), true);
                    EditorGUI.showMixedValue = false;

                    GUILayout.Label(property.Property.GetFriendlyPath());
                }
                GUI.enabled = enabled;
            }

            /************************************************************************************************************************/

            private void DoTransitionGUI()
            {
                if (!Settings.ShowTransition)
                    return;

                var property = _Instance._TransitionProperty;

                var isExpanded = property.Property.isExpanded;
                property.Property.isExpanded = true;
                var height = EditorGUI.GetPropertyHeight(property, true);

                const float Indent = 12;

                var padding = GUI.skin.box.padding;

                var area = GUILayoutUtility.GetRect(0, height + padding.horizontal - padding.bottom);
                area.x += Indent + padding.left;
                area.width -= Indent + padding.horizontal;

                EditorGUI.BeginChangeCheck();
                EditorGUI.PropertyField(area, property, true);
                property.Property.isExpanded = isExpanded;
                if (EditorGUI.EndChangeCheck())
                    property.ApplyModifiedProperties();
            }

            /************************************************************************************************************************/

            private void DoPreviewSettingsGUI()
            {
                GUILayout.BeginVertical(GUI.skin.box);
                EditorGUILayout.LabelField("Preview Details", "(Not Serialized)");

                DoModelGUI();
                DoAnimatorSelectorGUI();

                DoAnimationFieldGUI(AnimancerGUI.TempContent("Previous Animation",
                    "The animation for the preview to play before the target transition"),
                    ref _PreviousAnimation, (clip) => _PreviousAnimation = clip);

                var animancer = _Instance._Scene.InstanceAnimancer;
                DoCurrentAnimationGUI(animancer);

                DoAnimationFieldGUI(AnimancerGUI.TempContent("Next Animation",
                    "The animation for the preview to play after the target transition"),
                    ref _NextAnimation, (clip) => _NextAnimation = clip);

                if (animancer != null)
                {
                    if (animancer.IsGraphPlaying)
                    {
                        if (GUILayout.Button("Pause", EditorStyles.miniButton))
                            animancer.PauseGraph();
                    }
                    else
                    {
                        var enabled = GUI.enabled;
                        GUI.enabled = _Instance.GetTransition().IsValid();

                        if (GUILayout.Button("Play Transition", EditorStyles.miniButton))
                        {
                            if (_PreviousAnimation != null && _PreviousAnimation.length > 0)
                            {
                                _Instance._Scene.InstanceAnimancer.Stop();
                                var fromState = animancer.States.GetOrCreate(PreviousAnimationKey, _PreviousAnimation, true);
                                animancer.Play(fromState);
                                _Instance._Scene.OnPlayAnimation();
                                fromState.Time = 0;

                                var warnings = OptionalWarning.UnsupportedEvents.DisableTemporarily();
                                fromState.Events.endEvent = new AnimancerEvent(1 / fromState.Length, PlayTransition);
                                warnings.Enable();
                            }
                            else
                            {
                                PlayTransition();
                            }

                            _Instance._Scene.InstanceAnimancer.UnpauseGraph();
                        }

                        GUI.enabled = enabled;
                    }
                }

                GUILayout.EndVertical();
            }

            /************************************************************************************************************************/

            private void DoModelGUI()
            {
                var model = _Instance._Scene.OriginalRoot != null ? _Instance._Scene.OriginalRoot.gameObject : null;

                EditorGUI.BeginChangeCheck();

                var color = GUI.color;
                if (model == null)
                    GUI.color = AnimancerGUI.WarningFieldColor;

                if (DoDropdownObjectField(AnimancerGUI.TempContent("Model"), true, ref model, AnimancerGUI.SpacingMode.After))
                {
                    var menu = new GenericMenu();

                    menu.AddItem(new GUIContent("Default Humanoid"), Scene.IsDefaultHumanoid(model),
                        () => _Instance._Scene.OriginalRoot = Scene.DefaultHumanoid.transform);
                    menu.AddItem(new GUIContent("Default Sprite"), Scene.IsDefaultSprite(model),
                        () => _Instance._Scene.OriginalRoot = Scene.DefaultSprite.transform);

                    var models = Settings.Models;
                    if (models.Count == 0)
                    {
                        menu.AddDisabledItem(new GUIContent("No other model prefabs have been used yet"));
                    }
                    else
                    {
                        for (int i = models.Count - 1; i >= 0; i--)
                        {
                            var otherModel = models[i];
                            if (otherModel == null)
                            {
                                models.RemoveAt(i--);
                                AnimancerSettings.SetDirty();
                            }
                            else
                            {
                                var path = AssetDatabase.GetAssetPath(otherModel);
                                if (path != null)
                                    path = path.Replace('/', '\\');
                                else
                                    path = otherModel.name;

                                menu.AddItem(new GUIContent(path), otherModel == model,
                                    () => _Instance._Scene.OriginalRoot = otherModel.transform);
                            }
                        }
                    }

                    menu.ShowAsContext();
                }

                GUI.color = color;

                if (EditorGUI.EndChangeCheck())
                    _Instance._Scene.OriginalRoot = model != null ? model.transform : null;
            }

            /************************************************************************************************************************/

            private void DoAnimatorSelectorGUI()
            {
                var instanceAnimators = _Instance._Scene.InstanceAnimators;
                if (instanceAnimators == null ||
                    instanceAnimators.Length <= 1)
                    return;

                var area = AnimancerGUI.LayoutSingleLineRect(AnimancerGUI.SpacingMode.After);
                var labelArea = AnimancerGUI.StealFromLeft(ref area, EditorGUIUtility.labelWidth, AnimancerGUI.StandardSpacing);
                GUI.Label(labelArea, nameof(Animator));

                var selectedAnimator = _Instance._Scene.SelectedInstanceAnimator;
                var label = AnimancerGUI.TempContent(selectedAnimator != null ? selectedAnimator.name : "None");
                var clicked = EditorGUI.DropdownButton(area, label, FocusType.Passive);

                if (!clicked)
                    return;

                var menu = new GenericMenu();

                for (int i = 0; i < instanceAnimators.Length; i++)
                {
                    var animator = instanceAnimators[i];
                    label = new GUIContent(animator.name);
                    var index = i;
                    menu.AddItem(label, animator == selectedAnimator, () =>
                    {
                        _Instance._Scene.SetSelectedAnimator(index);
                        _Instance._Scene.NormalizedTime = 0;
                    });
                }

                menu.ShowAsContext();
            }

            /************************************************************************************************************************/

            public void GatherAnimations()
            {
                AnimationGatherer.GatherFromGameObject(_Instance._Scene.OriginalRoot.gameObject, ref _OtherAnimations, true);

                if (_OtherAnimations.Length > 0 &&
                    (_PreviousAnimation == null || _NextAnimation == null))
                {
                    var defaultClip = _OtherAnimations[0];
                    var defaultClipIsIdle = false;

                    for (int i = 0; i < _OtherAnimations.Length; i++)
                    {
                        var clip = _OtherAnimations[i];

                        if (defaultClipIsIdle && clip.name.Length > defaultClip.name.Length)
                            continue;

                        if (clip.name.IndexOf("idle", StringComparison.CurrentCultureIgnoreCase) >= 0)
                        {
                            defaultClip = clip;
                            break;
                        }
                    }

                    if (_PreviousAnimation == null)
                        _PreviousAnimation = defaultClip;
                    if (_NextAnimation == null)
                        _NextAnimation = defaultClip;
                }
            }

            /************************************************************************************************************************/

            private void DoAnimationFieldGUI(GUIContent label, ref AnimationClip clip, Action<AnimationClip> setClip)
            {
                var showDropdown = _OtherAnimations != null && _OtherAnimations.Length > 0;

                if (DoDropdownObjectField(label, showDropdown, ref clip))
                {
                    var menu = new GenericMenu();

                    menu.AddItem(new GUIContent("None"), clip == null, () => setClip(null));

                    for (int i = 0; i < _OtherAnimations.Length; i++)
                    {
                        var animation = _OtherAnimations[i];
                        menu.AddItem(new GUIContent(animation.name), animation == clip, () => setClip(animation));
                    }

                    menu.ShowAsContext();
                }
            }

            /************************************************************************************************************************/

            private static bool DoDropdownObjectField<T>(GUIContent label, bool showDropdown, ref T obj,
                AnimancerGUI.SpacingMode spacingMode = AnimancerGUI.SpacingMode.None) where T : Object
            {
                var area = AnimancerGUI.LayoutSingleLineRect(spacingMode);

                var labelWidth = EditorGUIUtility.labelWidth;

#if UNITY_2019_3_OR_NEWER
                labelWidth += 2;
                area.xMin -= 1;
#else
                area.xMin += 1;
                area.xMax -= 1;
#endif

                var spacing = AnimancerGUI.StandardSpacing;
                var labelArea = AnimancerGUI.StealFromLeft(ref area, labelWidth - spacing, spacing);

                obj = (T)EditorGUI.ObjectField(area, obj, typeof(T), true);

                if (showDropdown)
                {
                    return EditorGUI.DropdownButton(labelArea, label, FocusType.Passive);
                }
                else
                {
                    GUI.Label(labelArea, label);
                    return false;
                }
            }

            /************************************************************************************************************************/

            private void DoCurrentAnimationGUI(AnimancerPlayable animancer)
            {
                const string Label = "Current Animation";

                var enabled = GUI.enabled;
                GUI.enabled = false;

                string text = null;

                if (animancer != null)
                {
                    var transition = _Instance.GetTransition();
                    var mainObject = transition.MainObject;
                    if (mainObject != null)
                    {
                        EditorGUILayout.ObjectField(Label, mainObject, typeof(Object), true);
                    }
                    else
                    {
                        var key = transition.Key;
                        if (key != null)
                            text = animancer.States[transition].ToString();
                        else
                            text = transition.ToString();
                    }
                }
                else
                {
                    text = _Instance._TransitionProperty.Property.GetFriendlyPath();
                }

                if (text != null)
                    EditorGUILayout.LabelField(Label, text);

                GUI.enabled = enabled;
            }

            /************************************************************************************************************************/

            private void PlayTransition()
            {
                var transition = _Instance.GetTransition();
                var animancer = _Instance._Scene.InstanceAnimancer;
                var oldState = animancer.States[transition];

                var targetState = animancer.Play(transition);
                _Instance._Scene.OnPlayAnimation();

                if (oldState != null && oldState != animancer.States[transition])
                    oldState.Destroy();

                var warnings = OptionalWarning.UnsupportedEvents.DisableTemporarily();
                targetState.Events.OnEnd = () =>
                {
                    if (_NextAnimation != null)
                    {
                        var toState = animancer.States.GetOrCreate(NextAnimationKey, _NextAnimation, true);
                        animancer.Play(toState, AnimancerPlayable.DefaultFadeDuration);
                        _Instance._Scene.OnPlayAnimation();
                    }
                    else
                    {
                        animancer.Layers[0].IncrementCommandCount();
                    }
                };
                warnings.Enable();
            }

            /************************************************************************************************************************/
            #endregion
            /************************************************************************************************************************/
        }
    }
}

#endif


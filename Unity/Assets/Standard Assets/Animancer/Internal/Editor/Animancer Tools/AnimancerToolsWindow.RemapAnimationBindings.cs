// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Animancer.Editor
{
    partial class AnimancerToolsWindow
    {
        /// <summary>[Editor-Only] [Pro-Only] 
        /// An <see cref="AnimationModifierPanel"/> for changing which bones an <see cref="AnimationClip"/>s controls.
        /// </summary>
        /// <remarks>
        /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/tools/remap-animation-bindings">Remap Animation Bindings</see>
        /// </remarks>
        [Serializable]
        public sealed class RemapAnimationBindings : AnimationModifierPanel
        {
            /************************************************************************************************************************/

            [SerializeField] private List<string> _NewBindingPaths;

            [NonSerialized] private readonly List<List<EditorCurveBinding>> BindingGroups = new List<List<EditorCurveBinding>>();
            [NonSerialized] private readonly List<string> OldBindingPaths = new List<string>();
            [NonSerialized] private bool _OldBindingPathsAreDirty;
            [NonSerialized] private ReorderableList _OldBindingPathsDisplay;
            [NonSerialized] private ReorderableList _NewBindingPathsDisplay;

            /************************************************************************************************************************/

            /// <inheritdoc/>
            public override string Name => "Remap Animation Bindings";

            /// <inheritdoc/>
            public override string HelpURL => Strings.DocsURLs.RemapAnimationBindings;

            /// <inheritdoc/>
            public override string Instructions
            {
                get
                {
                    if (Animation == null)
                        return "Select the animation you want to remap.";

                    if (OldBindingPaths.Count == 0)
                    {
                        if (Animation.humanMotion)
                            return "The selected animation only has Humanoid bindings which cannot be remapped.";

                        return "The selected animation does not have any bindings.";
                    }

                    return "Enter the new paths to change the bindings into then click Save As.";
                }
            }

            /************************************************************************************************************************/

            /// <inheritdoc/>
            public override void OnEnable(int index)
            {
                base.OnEnable(index);

                AnimancerUtilities.NewIfNull(ref _NewBindingPaths);

                if (Animation == null)
                    _NewBindingPaths.Clear();

                _OldBindingPathsDisplay = CreateReorderableStringList(OldBindingPaths, "Old Binding Paths");
                _NewBindingPathsDisplay = CreateReorderableStringList(_NewBindingPaths, "New Binding Paths", (area, i) =>
                {
                    var color = GUI.color;

                    var path = _NewBindingPaths[i];
                    if (path != OldBindingPaths[i])
                        GUI.color = new Color(0.15f, 0.7f, 0.15f, 1);

                    path = EditorGUI.TextField(area, path);
                    GUI.color = color;
                    return path;
                });
            }

            /************************************************************************************************************************/

            /// <inheritdoc/>
            protected override void OnAnimationChanged()
            {
                base.OnAnimationChanged();
                _OldBindingPathsAreDirty = true;
            }

            /************************************************************************************************************************/

            /// <inheritdoc/>
            public override void DoBodyGUI()
            {
                base.DoBodyGUI();
                GatherBindings();

                GUILayout.BeginHorizontal();
                {
                    GUILayout.BeginVertical();
                    GUI.enabled = false;
                    _OldBindingPathsDisplay.DoLayoutList();
                    GUI.enabled = true;
                    GUILayout.EndVertical();

                    GUILayout.BeginVertical();
                    _NewBindingPathsDisplay.DoLayoutList();
                    GUILayout.EndVertical();
                }
                GUILayout.EndHorizontal();

                GUI.enabled = Animation != null;

                GUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("Reset"))
                    {
                        AnimancerGUI.Deselect();
                        RecordUndo();
                        _NewBindingPaths.Clear();
                        _OldBindingPathsAreDirty = true;
                    }

                    if (GUILayout.Button("Copy All"))
                    {
                        AnimancerGUI.Deselect();
                        CopyAll();
                    }

                    if (GUILayout.Button("Paste All"))
                    {
                        AnimancerGUI.Deselect();
                        PasteAll();
                    }

                    if (GUILayout.Button("Save As"))
                    {
                        if (SaveAs())
                        {
                            _OldBindingPathsAreDirty = true;
                        }
                    }
                }
                GUILayout.EndHorizontal();
            }

            /************************************************************************************************************************/

            /// <summary>Gathers the bindings from the <see cref="AnimationModifierPanel.Animation"/>.</summary>
            private void GatherBindings()
            {
                if (!_OldBindingPathsAreDirty)
                    return;

                _OldBindingPathsAreDirty = false;

                BindingGroups.Clear();
                OldBindingPaths.Clear();

                if (Animation == null)
                {
                    _NewBindingPaths.Clear();
                    return;
                }

                var isHumanoid = Animation.humanMotion;

                AnimationBindings.OnAnimationChanged(Animation);
                var bindings = AnimationBindings.GetBindings(Animation);
                Array.Sort(bindings, (a, b) =>
                {
                    var result = EditorUtility.NaturalCompare(a.path, b.path);
                    if (result != 0)
                        return result;

                    return EditorUtility.NaturalCompare(a.propertyName, b.propertyName);
                });

                string previousPath = null;
                List<EditorCurveBinding> previousGroup = null;
                for (int i = 0; i < bindings.Length; i++)
                {
                    var binding = bindings[i];
                    if (isHumanoid &&
                        string.IsNullOrEmpty(binding.path) &&
                        IsHumanoidBinding(binding.propertyName))
                        continue;

                    var path = binding.path;
                    if (path == previousPath)
                    {
                        previousGroup.Add(binding);
                        continue;
                    }

                    previousPath = path;
                    previousGroup = new List<EditorCurveBinding> { binding };

                    BindingGroups.Add(previousGroup);

                    OldBindingPaths.Add(path);
                    if (_NewBindingPaths.Count < OldBindingPaths.Count)
                        _NewBindingPaths.Add(path);
                }

                if (_NewBindingPaths.Count > OldBindingPaths.Count)
                    _NewBindingPaths.RemoveRange(OldBindingPaths.Count, _NewBindingPaths.Count - OldBindingPaths.Count);
            }

            /************************************************************************************************************************/

            private static HashSet<string> _HumanoidBindingNames;

            /// <summary>Is the `propertyName` one of the bindings used by Humanoid animations?</summary>
            private static bool IsHumanoidBinding(string propertyName)
            {
                if (_HumanoidBindingNames == null)
                {
                    _HumanoidBindingNames = new HashSet<string>
                    {
                        "RootT.x", "RootT.y", "RootT.z",
                        "RootQ.x", "RootQ.y", "RootQ.z", "RootQ.w",
                        "LeftFootT.x", "LeftFootT.y", "LeftFootT.z",
                        "LeftFootQ.x", "LeftFootQ.y", "LeftFootQ.z", "LeftFootQ.w",
                        "RightFootT.x", "RightFootT.y", "RightFootT.z",
                        "RightFootQ.x", "RightFootQ.y", "RightFootQ.z", "RightFootQ.w",
                        "LeftHandT.x", "LeftHandT.y", "LeftHandT.z",
                        "LeftHandQ.x", "LeftHandQ.y", "LeftHandQ.z", "LeftHandQ.w",
                        "RightHandT.x", "RightHandT.y", "RightHandT.z",
                        "RightHandQ.x", "RightHandQ.y", "RightHandQ.z", "RightHandQ.w",
                        "Spine Front-Back", "Spine Left-Right", "Spine Twist Left-Right",
                        "Chest Front-Back", "Chest Left-Right", "Chest Twist Left-Right",
                        "UpperChest Front-Back", "UpperChest Left-Right", "UpperChest Twist Left-Right",
                        "Neck Nod Down-Up", "Neck Tilt Left-Right", "Neck Turn Left-Right",
                        "Head Nod Down-Up", "Head Tilt Left-Right", "Head Turn Left-Right",
                        "Left Eye Down-Up", "Left Eye In-Out",
                        "Right Eye Down-Up", "Right Eye In-Out",
                        "Jaw Close", "Jaw Left-Right",
                        "Left Upper Leg Front-Back", "Left Upper Leg In-Out", "Left Upper Leg Twist In-Out",
                        "Left Lower Leg Stretch", "Left Lower Leg Twist In-Out",
                        "Left Foot Up-Down", "Left Foot Twist In-Out",
                        "Left Toes Up-Down",
                        "Right Upper Leg Front-Back", "Right Upper Leg In-Out", "Right Upper Leg Twist In-Out",
                        "Right Lower Leg Stretch", "Right Lower Leg Twist In-Out",
                        "Right Foot Up-Down", "Right Foot Twist In-Out",
                        "Right Toes Up-Down",
                        "Left Shoulder Down-Up", "Left Shoulder Front-Back",
                        "Left Arm Down-Up", "Left Arm Front-Back", "Left Arm Twist In-Out",
                        "Left Forearm Stretch", "Left Forearm Twist In-Out",
                        "Left Hand Down-Up", "Left Hand In-Out",
                        "Right Shoulder Down-Up", "Right Shoulder Front-Back",
                        "Right Arm Down-Up", "Right Arm Front-Back", "Right Arm Twist In-Out",
                        "Right Forearm Stretch", "Right Forearm Twist In-Out",
                        "Right Hand Down-Up", "Right Hand In-Out",
                        "LeftHand.Thumb.Spread", "LeftHand.Thumb.1 Stretched", "LeftHand.Thumb.2 Stretched", "LeftHand.Thumb.3 Stretched",
                        "LeftHand.Index.Spread", "LeftHand.Index.1 Stretched", "LeftHand.Index.2 Stretched", "LeftHand.Index.3 Stretched",
                        "LeftHand.Middle.Spread", "LeftHand.Middle.1 Stretched", "LeftHand.Middle.2 Stretched", "LeftHand.Middle.3 Stretched",
                        "LeftHand.Ring.Spread", "LeftHand.Ring.1 Stretched", "LeftHand.Ring.2 Stretched", "LeftHand.Ring.3 Stretched",
                        "LeftHand.Little.Spread", "LeftHand.Little.1 Stretched", "LeftHand.Little.2 Stretched", "LeftHand.Little.3 Stretched",
                        "RightHand.Thumb.Spread", "RightHand.Thumb.1 Stretched", "RightHand.Thumb.2 Stretched", "RightHand.Thumb.3 Stretched",
                        "RightHand.Index.Spread", "RightHand.Index.1 Stretched", "RightHand.Index.2 Stretched", "RightHand.Index.3 Stretched",
                        "RightHand.Middle.Spread", "RightHand.Middle.1 Stretched", "RightHand.Middle.2 Stretched", "RightHand.Middle.3 Stretched",
                        "RightHand.Ring.Spread",  "RightHand.Ring.1 Stretched", "RightHand.Ring.2 Stretched", "RightHand.Ring.3 Stretched",
                        "RightHand.Little.Spread", "RightHand.Little.1 Stretched", "RightHand.Little.2 Stretched", "RightHand.Little.3 Stretched",
                    };
                }

                return _HumanoidBindingNames.Contains(propertyName);
            }

            /************************************************************************************************************************/

            /// <summary>Copies all of the <see cref="_NewBindingPaths"/> to the system clipboard.</summary>
            private void CopyAll()
            {
                var text = ObjectPool.AcquireStringBuilder();

                for (int i = 0; i < _NewBindingPaths.Count; i++)
                {
                    text.AppendLine(_NewBindingPaths[i]);
                }

                EditorGUIUtility.systemCopyBuffer = text.ReleaseToString();
            }

            /// <summary>Pastes the string from the system clipboard into the <see cref="_NewBindingPaths"/>.</summary>
            private void PasteAll()
            {
                using (var reader = new StringReader(EditorGUIUtility.systemCopyBuffer))
                {
                    for (int i = 0; i < _NewBindingPaths.Count; i++)
                    {
                        var line = reader.ReadLine();
                        if (line == null)
                            return;

                        _NewBindingPaths[i] = line;
                    }
                }
            }

            /************************************************************************************************************************/

            /// <inheritdoc/>
            protected override void Modify(AnimationClip animation)
            {
                for (int iGroup = 0; iGroup < BindingGroups.Count; iGroup++)
                {
                    var oldPath = OldBindingPaths[iGroup];
                    var newPath = _NewBindingPaths[iGroup];
                    if (oldPath == newPath)
                        continue;

                    var group = BindingGroups[iGroup];
                    for (int iBinding = 0; iBinding < group.Count; iBinding++)
                    {
                        var binding = group[iBinding];
                        if (binding.isPPtrCurve)
                        {
                            var curve = AnimationUtility.GetObjectReferenceCurve(animation, binding);
                            AnimationUtility.SetObjectReferenceCurve(animation, binding, null);

                            binding.path = newPath;
                            AnimationUtility.SetObjectReferenceCurve(animation, binding, curve);
                        }
                        else
                        {
                            var curve = AnimationUtility.GetEditorCurve(animation, binding);
                            AnimationUtility.SetEditorCurve(animation, binding, null);

                            binding.path = newPath;
                            AnimationUtility.SetEditorCurve(animation, binding, curve);
                        }
                    }
                }
            }

            /************************************************************************************************************************/
        }
    }
}

#endif


// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Animancer.Editor
{
    partial class AnimancerToolsWindow
    {
        /// <summary>[Editor-Only] [Pro-Only] 
        /// An <see cref="AnimationModifierPanel"/> for changing which <see cref="Sprite"/>s an
        /// <see cref="AnimationClip"/> uses.
        /// </summary>
        /// <remarks>
        /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/tools/remap-sprite-animation">Remap Sprite Animation</see>
        /// </remarks>
        [Serializable]
        public sealed class RemapSpriteAnimation : AnimationModifierPanel
        {
            /************************************************************************************************************************/

            [SerializeField] private List<Sprite> _NewSprites;

            [NonSerialized] private readonly List<Sprite> OldSprites = new List<Sprite>();
            [NonSerialized] private bool _OldSpritesAreDirty;
            [NonSerialized] private ReorderableList _OldSpriteDisplay;
            [NonSerialized] private ReorderableList _NewSpriteDisplay;
            [NonSerialized] private EditorCurveBinding _SpriteBinding;
            [NonSerialized] private ObjectReferenceKeyframe[] _SpriteKeyframes;

            /************************************************************************************************************************/

            /// <inheritdoc/>
            public override string Name => "Remap Sprite Animation";

            /// <inheritdoc/>
            public override string HelpURL => Strings.DocsURLs.RemapSpriteAnimation;

            /// <inheritdoc/>
            public override string Instructions
            {
                get
                {
                    if (Animation == null)
                        return "Select the animation you want to remap.";

                    if (OldSprites.Count == 0)
                        return "The selected animation does not use Sprites.";

                    return "Assign the New Sprites that you want to replace the Old Sprites with then click Save As." +
                        " You can Drag and Drop multiple Sprites onto the New Sprites list at the same time.";
                }
            }

            /************************************************************************************************************************/

            /// <inheritdoc/>
            public override void OnEnable(int index)
            {
                base.OnEnable(index);

                AnimancerUtilities.NewIfNull(ref _NewSprites);

                if (Animation == null)
                    _NewSprites.Clear();

                _OldSpriteDisplay = CreateReorderableSpriteList(OldSprites, "Old Sprites");
                _NewSpriteDisplay = CreateReorderableSpriteList(_NewSprites, "New Sprites");
            }

            /************************************************************************************************************************/

            /// <inheritdoc/>
            protected override void OnAnimationChanged()
            {
                base.OnAnimationChanged();
                _OldSpritesAreDirty = true;
            }

            /************************************************************************************************************************/

            /// <inheritdoc/>
            public override void DoBodyGUI()
            {
                base.DoBodyGUI();
                GatherOldSprites();

                GUILayout.BeginHorizontal();
                {
                    GUILayout.BeginVertical();
                    GUI.enabled = false;
                    _OldSpriteDisplay.DoLayoutList();
                    GUI.enabled = true;
                    GUILayout.EndVertical();

                    GUILayout.BeginVertical();
                    _NewSpriteDisplay.DoLayoutList();
                    GUILayout.EndVertical();

                    HandleDragAndDrop(GUILayoutUtility.GetLastRect());
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
                        _NewSprites.Clear();
                        _OldSpritesAreDirty = true;
                    }

                    if (GUILayout.Button("Save As"))
                    {
                        if (SaveAs())
                        {
                            _OldSpritesAreDirty = true;
                        }
                    }
                }
                GUILayout.EndHorizontal();
            }

            /************************************************************************************************************************/

            /// <summary>Gathers the <see cref="OldSprites"/> from the <see cref="AnimationModifierPanel.Animation"/>.</summary>
            private void GatherOldSprites()
            {
                if (!_OldSpritesAreDirty)
                    return;

                _OldSpritesAreDirty = false;

                OldSprites.Clear();
                _NewSprites.Clear();

                if (Animation == null)
                    return;

                var bindings = AnimationUtility.GetObjectReferenceCurveBindings(Animation);
                for (int iBinding = 0; iBinding < bindings.Length; iBinding++)
                {
                    var binding = bindings[iBinding];
                    if (binding.type == typeof(SpriteRenderer) && binding.propertyName == "m_Sprite")
                    {
                        _SpriteBinding = binding;
                        _SpriteKeyframes = AnimationUtility.GetObjectReferenceCurve(Animation, binding);

                        for (int iKeyframe = 0; iKeyframe < _SpriteKeyframes.Length; iKeyframe++)
                        {
                            var reference = _SpriteKeyframes[iKeyframe].value as Sprite;
                            if (reference != null)
                                OldSprites.Add(reference);
                        }

                        _NewSprites.AddRange(OldSprites);

                        return;
                    }
                }
            }

            /************************************************************************************************************************/

            private int _DropIndex;

            /// <summary>
            /// If <see cref="Sprite"/>s are dropped into the `area`, this method assigns them as the
            /// <see cref="_NewSprites"/>.
            /// </summary>
            private void HandleDragAndDrop(Rect area)
            {
                _DropIndex = 0;
                AnimancerGUI.HandleDragAndDrop<Sprite>(area, (sprite) => true, (sprite) =>
                {
                    if (_DropIndex < _NewSprites.Count)
                    {
                        RecordUndo();
                        _NewSprites[_DropIndex++] = sprite;
                    }
                });
            }

            /************************************************************************************************************************/

            /// <inheritdoc/>
            protected override void Modify(AnimationClip animation)
            {
                for (int i = 0; i < _SpriteKeyframes.Length; i++)
                {
                    _SpriteKeyframes[i].value = _NewSprites[i];
                }

                AnimationUtility.SetObjectReferenceCurve(animation, _SpriteBinding, _SpriteKeyframes);
            }

            /************************************************************************************************************************/
        }
    }
}

#endif


// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;

namespace Animancer.Editor
{
    partial class AnimancerToolsWindow
    {
        /// <summary>[Editor-Only] [Pro-Only] A base <see cref="Panel"/> for modifying <see cref="AnimationClip"/>s.</summary>
        /// <remarks>
        /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/tools">Animancer Tools</see>
        /// </remarks>
        [Serializable]
        public abstract class AnimationModifierPanel : Panel
        {
            /************************************************************************************************************************/

            [SerializeField]
            private AnimationClip _Animation;

            /// <summary>The currently selected <see cref="AnimationClip"/> asset.</summary>
            public AnimationClip Animation => _Animation;

            /************************************************************************************************************************/

            /// <inheritdoc/>
            public override void OnEnable(int index)
            {
                base.OnEnable(index);
                OnAnimationChanged();
            }

            /************************************************************************************************************************/

            /// <inheritdoc/>
            public override void OnSelectionChanged()
            {
                if (Selection.activeObject is AnimationClip animation)
                {
                    _Animation = animation;
                    OnAnimationChanged();
                }
            }

            /************************************************************************************************************************/

            /// <summary>Called whenever the selected <see cref="Animation"/> changes.</summary>
            protected virtual void OnAnimationChanged() { }

            /************************************************************************************************************************/

            /// <inheritdoc/>
            public override void DoBodyGUI()
            {
                BeginChangeCheck();
                var animation = (AnimationClip)EditorGUILayout.ObjectField("Animation", _Animation, typeof(AnimationClip), false);
                if (EndChangeCheck(ref _Animation, animation))
                    OnAnimationChanged();
            }

            /************************************************************************************************************************/

            /// <summary>Calls <see cref="Panel.SaveModifiedAsset"/> on the animation.</summary>
            protected bool SaveAs()
            {
                AnimancerGUI.Deselect();

                if (SaveModifiedAsset(
                    "Save Modified Animation",
                    "Where would you like to save the new animation?",
                    _Animation,
                    Modify))
                {
                    _Animation = null;
                    OnAnimationChanged();
                    return true;
                }
                else return false;
            }

            /************************************************************************************************************************/

            /// <summary>Override this to apply the desired modifications to the `animation` before it is saved.</summary>
            protected virtual void Modify(AnimationClip animation) { }

            /************************************************************************************************************************/
        }
    }
}

#endif


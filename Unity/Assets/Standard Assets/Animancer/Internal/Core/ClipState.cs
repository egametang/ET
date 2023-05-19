// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using Object = UnityEngine.Object;

namespace Animancer
{
    /// <summary>An <see cref="AnimancerState"/> which plays an <see cref="AnimationClip"/>.</summary>
    /// <remarks>
    /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/playing/states">States</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/ClipState
    /// 
    public sealed class ClipState : AnimancerState
    {
        /************************************************************************************************************************/
        #region Fields and Properties
        /************************************************************************************************************************/

        /// <summary>The <see cref="AnimationClip"/> which this state plays.</summary>
        private AnimationClip _Clip;

        /// <summary>The <see cref="AnimationClip"/> which this state plays.</summary>
        public override AnimationClip Clip
        {
            get => _Clip;
            set => ChangeMainObject(ref _Clip, value);
        }

        /// <summary>The <see cref="AnimationClip"/> which this state plays.</summary>
        public override Object MainObject
        {
            get => _Clip;
            set => Clip = (AnimationClip)value;
        }

        /************************************************************************************************************************/

        /// <summary>The <see cref="AnimationClip.length"/>.</summary>
        public override float Length => _Clip.length;

        /************************************************************************************************************************/

        /// <summary>The <see cref="Motion.isLooping"/>.</summary>
        public override bool IsLooping => _Clip.isLooping;

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override Vector3 AverageVelocity => _Clip.averageSpeed;

        /************************************************************************************************************************/
        #region Inverse Kinematics
        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override bool ApplyAnimatorIK
        {
            get
            {
                Validate.AssertPlayable(this);
                return ((AnimationClipPlayable)_Playable).GetApplyPlayableIK();
            }
            set
            {
                Validate.AssertPlayable(this);
                ((AnimationClipPlayable)_Playable).SetApplyPlayableIK(value);
            }
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override bool ApplyFootIK
        {
            get
            {
                Validate.AssertPlayable(this);
                return ((AnimationClipPlayable)_Playable).GetApplyFootIK();
            }
            set
            {
                Validate.AssertPlayable(this);
                ((AnimationClipPlayable)_Playable).SetApplyFootIK(value);
            }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Methods
        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="ClipState"/> and sets its <see cref="Clip"/>.</summary>
        /// <exception cref="ArgumentNullException">The `clip` is null.</exception>
        public ClipState(AnimationClip clip)
        {
            if (clip == null)
                throw new ArgumentNullException(nameof(clip));

            Validate.AssertNotLegacy(clip);

            _Clip = clip;
        }

        /************************************************************************************************************************/

        /// <summary>Creates and assigns the <see cref="AnimationClipPlayable"/> managed by this node.</summary>
        protected override void CreatePlayable(out Playable playable)
        {
            Validate.AssertNotLegacy(_Clip);

            var root = Root;
            var clipPlayable = AnimationClipPlayable.Create(root._Graph, _Clip);
            playable = clipPlayable;
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void Destroy()
        {
            _Clip = null;
            base.Destroy();
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Inspector
#if UNITY_EDITOR
        /************************************************************************************************************************/

        /// <summary>[Editor-Only] Returns a <see cref="Drawer"/> for this state.</summary>
        protected internal override Editor.IAnimancerNodeDrawer CreateDrawer() => new Drawer(this);

        /************************************************************************************************************************/

        /// <summary>[Editor-Only] Draws the Inspector GUI for a <see cref="ClipState"/>.</summary>
        /// <remarks>
        /// Unfortunately the tool used to generate this documentation does not currently support nested types with
        /// identical names, so only one <c>Drawer</c> class will actually have a documentation page.
        /// <para></para>
        /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/transitions">Transitions</see>
        /// </remarks>
        public sealed class Drawer : Editor.AnimancerStateDrawer<ClipState>
        {
            /************************************************************************************************************************/

            /// <summary>
            /// Creates a new <see cref="Drawer"/> to manage the Inspector GUI for the `state`.
            /// </summary>
            public Drawer(ClipState state) : base(state) { }

            /************************************************************************************************************************/

            /// <inheritdoc/>
            protected override void AddContextMenuFunctions(UnityEditor.GenericMenu menu)
            {
                menu.AddDisabledItem(new GUIContent(DetailsPrefix + "Animation Type: " +
                    Editor.AnimationBindings.GetAnimationType(Target._Clip)));

                base.AddContextMenuFunctions(menu);

                Editor.AnimancerEditorUtilities.AddContextMenuIK(menu, Target);
            }

            /************************************************************************************************************************/
        }

        /************************************************************************************************************************/
#endif
        #endregion
        /************************************************************************************************************************/
        #region Transition
        /************************************************************************************************************************/

        /// <summary>
        /// A serializable <see cref="ITransition"/> which can create a <see cref="ClipState"/> when passed
        /// into <see cref="AnimancerPlayable.Play(ITransition)"/>.
        /// </summary>
        /// <remarks>
        /// Unfortunately the tool used to generate this documentation does not currently support nested types with
        /// identical names, so only one <c>Transition</c> class will actually have a documentation page.
        /// <para></para>
        /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/transitions">Transitions</see>
        /// </remarks>
        /// https://kybernetik.com.au/animancer/api/Animancer/Transition
        /// 
        [Serializable]
        public class Transition : Transition<ClipState>, IAnimationClipCollection
        {
            /************************************************************************************************************************/

            [SerializeField, Tooltip("The animation to play")]
            private AnimationClip _Clip;

            /// <summary>[<see cref="SerializeField"/>] The animation to play.</summary>
            public AnimationClip Clip
            {
                get => _Clip;
                set
                {
                    if (value != null)
                        Validate.AssertNotLegacy(value);

                    _Clip = value;
                }
            }

            /// <inheritdoc/>
            public override Object MainObject => _Clip;

            /// <summary>
            /// The <see cref="Clip"/> will be used as the <see cref="AnimancerState.Key"/> for the created state to be
            /// registered with.
            /// </summary>
            public override object Key => _Clip;

            /************************************************************************************************************************/

            [SerializeField, Tooltip(Strings.ProOnlyTag +
                "How fast the animation plays (1x = normal speed, 2x = double speed)")]
            private float _Speed = 1;

            /// <summary>[<see cref="SerializeField"/>]
            /// Determines how fast the animation plays (1x = normal speed, 2x = double speed).
            /// </summary>
            public override float Speed
            {
                get => _Speed;
                set => _Speed = value;
            }

            /************************************************************************************************************************/

            [SerializeField, Tooltip(Strings.ProOnlyTag + "If enabled, the animation's time will start at this value when played")]
            [UnityEngine.Serialization.FormerlySerializedAs("_StartTime")]
            private float _NormalizedStartTime = float.NaN;

            /// <summary>[<see cref="SerializeField"/>]
            /// Determines what <see cref="AnimancerState.NormalizedTime"/> to start the animation at.
            /// <para></para>
            /// The default value is <see cref="float.NaN"/> which indicates that this value is not used so the
            /// animation will continue from its current time.
            /// </summary>
            public override float NormalizedStartTime
            {
                get => _NormalizedStartTime;
                set => _NormalizedStartTime = value;
            }

            /// <summary>
            /// If this transition will set the <see cref="AnimancerState.Time"/>, then it needs to use
            /// <see cref="FadeMode.FromStart"/>.
            /// </summary>
            public override FadeMode FadeMode => float.IsNaN(_NormalizedStartTime) ? FadeMode.FixedSpeed : FadeMode.FromStart;

            /************************************************************************************************************************/

            /// <summary>[<see cref="ITransitionDetailed"/>] Returns <see cref="Motion.isLooping"/>.</summary>
            public override bool IsLooping => _Clip != null && _Clip.isLooping;

            /// <inheritdoc/>
            public override float MaximumDuration => _Clip != null ? _Clip.length : 0;

            /// <inheritdoc/>
            public override float AverageAngularSpeed => _Clip != null ? _Clip.averageAngularSpeed : default;

            /// <inheritdoc/>
            public override Vector3 AverageVelocity => _Clip != null ? _Clip.averageSpeed : default;

            /// <inheritdoc/>
            public override bool IsValid => _Clip != null && !_Clip.legacy;

            /************************************************************************************************************************/

            /// <summary>
            /// Creates and returns a new <see cref="ClipState"/>.
            /// <para></para>
            /// Note that using methods like <see cref="AnimancerPlayable.Play(ITransition)"/> will also call
            /// <see cref="ITransition.Apply"/>, so if you call this method manually you may want to call that method
            /// as well. Or you can just use <see cref="AnimancerUtilities.CreateStateAndApply"/>.
            /// <para></para>
            /// This method also assigns it as the <see cref="AnimancerState.Transition{TState}.State"/>.
            /// </summary>
            public override ClipState CreateState() => State = new ClipState(_Clip);

            /************************************************************************************************************************/

            /// <inheritdoc/>
            public override void Apply(AnimancerState state)
            {
                base.Apply(state);

                if (!float.IsNaN(_Speed))
                    state.Speed = _Speed;

                if (!float.IsNaN(_NormalizedStartTime))
                    state.NormalizedTime = _NormalizedStartTime;
                else if (state.Weight == 0)
                    state.NormalizedTime = AnimancerEvent.Sequence.GetDefaultNormalizedStartTime(_Speed);
            }

            /************************************************************************************************************************/

            /// <summary>Adds the <see cref="Clip"/> to the collection.</summary>
            void IAnimationClipCollection.GatherAnimationClips(ICollection<AnimationClip> clips) => clips.Gather(_Clip);

            /************************************************************************************************************************/
#if UNITY_EDITOR
            /************************************************************************************************************************/

            /// <summary>[Editor-Only] Draws the Inspector GUI for a <see cref="Transition"/>.</summary>
            /// <remarks>
            /// Unfortunately the tool used to generate this documentation does not currently support nested types with
            /// identical names, so only one <c>Drawer</c> class will actually have a documentation page.
            /// <para></para>
            /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/transitions">Transitions</see>
            /// </remarks>
            [UnityEditor.CustomPropertyDrawer(typeof(Transition), true)]
            public class Drawer : Editor.TransitionDrawer
            {
                /************************************************************************************************************************/

                /// <summary>Creates a new <see cref="Drawer"/>.</summary>
                public Drawer() : base(nameof(_Clip)) { }

                /************************************************************************************************************************/
            }

            /************************************************************************************************************************/
#endif
            /************************************************************************************************************************/
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}


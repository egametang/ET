// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Animancer
{
    /// <summary>Plays a single <see cref="AnimationClip"/> on startup.</summary>
    /// <remarks>
    /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/playing/component-types">Component Types</see>
    /// </remarks>
    /// <example>
    /// <see href="https://kybernetik.com.au/animancer/docs/examples/fine-control/solo-animation">Solo Animation</see>
    /// </example>
    /// https://kybernetik.com.au/animancer/api/Animancer/SoloAnimation
    /// 
    [AddComponentMenu(Strings.MenuPrefix + "Solo Animation")]
    [DefaultExecutionOrder(-5000)]// Initialise before anything else tries to use this component.
    [HelpURL(Strings.DocsURLs.APIDocumentation + "/" + nameof(SoloAnimation))]
    public sealed class SoloAnimation : MonoBehaviour, IAnimationClipSource
    {
        /************************************************************************************************************************/
        #region Fields and Properties
        /************************************************************************************************************************/

        [SerializeField, Tooltip("The Animator component which this script controls")]
        private Animator _Animator;

        /// <summary>[<see cref="SerializeField"/>]
        /// The <see cref="UnityEngine.Animator"/> component which this script controls.
        /// <para></para>
        /// If you need to set this value at runtime you are likely better off using a proper
        /// <see cref="AnimancerComponent"/>.
        /// </summary>
        public Animator Animator
        {
            get => _Animator;
            set
            {
                _Animator = value;
                Awake();
            }
        }

        /************************************************************************************************************************/

        [SerializeField, Tooltip("The AnimationClip which will be played by OnEnable")]
        private AnimationClip _Clip;

        /// <summary>[<see cref="SerializeField"/>]
        /// The <see cref="AnimationClip"/> which will be played by <see cref="OnEnable"/>.
        /// <para></para>
        /// If you need to set this value at runtime you are likely better off using a proper
        /// <see cref="AnimancerComponent"/>.
        /// </summary>
        public AnimationClip Clip
        {
            get => _Clip;
            set
            {
                _Clip = value;
                Awake();
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// If true, disabling this object will stop and rewind the animation. Otherwise it will simply be paused
        /// and will resume from its current state when it is re-enabled.
        /// <para></para>
        /// The default value is true.
        /// <para></para>
        /// This property wraps <see cref="Animator.keepAnimatorControllerStateOnDisable"/> and inverts its value.
        /// The value is serialized by the <see cref="UnityEngine.Animator"/>.
        /// </summary>
        public bool StopOnDisable
        {
            get => !_Animator.keepAnimatorStateOnDisable;
            set => _Animator.keepAnimatorStateOnDisable = !value;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// The <see cref="PlayableGraph"/> being used to play the <see cref="Clip"/>.
        /// </summary>
        private PlayableGraph _Graph;

        /// <summary>
        /// The <see cref="AnimationClipPlayable"/> being used to play the <see cref="Clip"/>.
        /// </summary>
        private AnimationClipPlayable _Playable;

        /************************************************************************************************************************/

        private bool _IsPlaying;

        /// <summary>
        /// Indicates whether the animation is playing (true) or paused (false).
        /// </summary>
        public bool IsPlaying
        {
            get => _IsPlaying;
            set
            {
                _IsPlaying = value;

                if (!IsInitialised)
                    return;

                if (value)
                    _Graph.Play();
                else
                    _Graph.Stop();
            }
        }

        /************************************************************************************************************************/

        [SerializeField, Tooltip("The speed at which the animation plays (default 1)")]
        private float _Speed = 1;

        /// <summary>[<see cref="SerializeField"/>]
        /// The speed at which the animation is playing (default 1).
        /// </summary>
        /// <exception cref="ArgumentException">This component is not yet <see cref="Awake"/>.</exception>
        public float Speed
        {
            get => _Speed;
            set
            {
                _Speed = value;
                _Playable.SetSpeed(value);
                IsPlaying = value != 0;
            }
        }

        /************************************************************************************************************************/

        [SerializeField, Tooltip("Determines whether Foot IK will be applied to the model (if it is Humanoid)")]
        private bool _FootIK;

        /// <summary>[<see cref="SerializeField"/>]
        /// Determines whether Foot IK will be applied to the model (if it is Humanoid).
        /// <para></para>
        /// The developers of Unity have states that they believe it looks better with this enabled, but more often
        /// than not it just makes the legs end up in a slightly different pose to what the animator intended.
        /// </summary>
        /// <exception cref="ArgumentException">This component is not yet <see cref="Awake"/>.</exception>
        public bool FootIK
        {
            get => _FootIK;
            set
            {
                _FootIK = value;
                _Playable.SetApplyFootIK(value);
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// The number of seconds that have passed since the start of the animation.
        /// </summary>
        /// <exception cref="ArgumentException">This component is not yet <see cref="Awake"/>.</exception>
        public float Time
        {
            get => (float)_Playable.GetTime();
            set
            {
                // We need to call SetTime twice to ensure that animation events aren't triggered incorrectly.
                _Playable.SetTime(value);
                _Playable.SetTime(value);

                IsPlaying = true;
            }
        }

        /// <summary>
        /// The <see cref="Time"/> of this state as a portion of the <see cref="AnimationClip.length"/>, meaning the
        /// value goes from 0 to 1 as it plays from start to end, regardless of how long that actually takes.
        /// <para></para>
        /// This value will continue increasing after the animation passes the end of its length and it will either
        /// freeze in place or start again from the beginning according to whether it is looping or not.
        /// <para></para>
        /// The fractional part of the value (<c>NormalizedTime % 1</c>) is the percentage (0-1) of progress in the
        /// current loop while the integer part (<c>(int)NormalizedTime</c>) is the number of times the animation has
        /// been looped.
        /// </summary>
        /// <exception cref="ArgumentException">This component is not yet <see cref="Awake"/>.</exception>
        public float NormalizedTime
        {
            get => Time / _Clip.length;
            set => Time = value * _Clip.length;
        }

        /************************************************************************************************************************/

        /// <summary>Indicates whether the <see cref="PlayableGraph"/> is valid.</summary>
        public bool IsInitialised => _Graph.IsValid();

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
#if UNITY_EDITOR
        /************************************************************************************************************************/

        /// <summary>[Editor-Only]
        /// Tries to find an <see cref="UnityEngine.Animator"/> component on this <see cref="GameObject"/> or its
        /// children or parents (in that order).
        /// </summary>
        /// <remarks>
        /// Called by the Unity Editor when this component is first added (in Edit Mode) and whenever the Reset command
        /// is executed from its context menu.
        /// </remarks>
        private void Reset()
        {
            _Animator = Editor.AnimancerEditorUtilities.GetComponentInHierarchy<Animator>(gameObject);
        }

        /************************************************************************************************************************/

        /// <summary>[Editor-Only]
        /// Tries to find an <see cref="UnityEngine.Animator"/> component on this <see cref="GameObject"/> or its
        /// parents or children (in that order).
        /// </summary>
        /// <remarks>
        /// Called by the Unity Editor in Edit Mode whenever an instance of this script is loaded or a value is changed
        /// in the Inspector.
        /// </remarks>
        private void OnValidate()
        {
            if (IsInitialised)
            {
                Speed = Speed;
                FootIK = FootIK;
            }
        }

        /************************************************************************************************************************/
#endif
        /************************************************************************************************************************/

        /// <summary>Initialises everything needed to play the <see cref="Clip"/>.</summary>
        /// <remarks>Called by Unity when this component is first initialised.</remarks>
        private void Awake()
        {
            if (_Clip == null || _Animator == null)
                return;

            if (_Graph.IsValid())
                _Graph.Destroy();

            _Playable = AnimationPlayableUtilities.PlayClip(_Animator, _Clip, out _Graph);

            _Playable.SetSpeed(_Speed);

            if (!_FootIK)
                _Playable.SetApplyFootIK(false);

            if (!_Clip.isLooping)
                _Playable.SetDuration(_Clip.length);
        }

        /************************************************************************************************************************/

        /// <summary>Plays the <see cref="Clip"/> on the target <see cref="Animator"/>.</summary>
        /// <remarks>Called by Unity when this component becomes enabled and active.</remarks>
        private void OnEnable()
        {
            IsPlaying = true;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Checks if the animation is done so it can pause the <see cref="PlayableGraph"/> to improve performance.
        /// </summary>
        /// <remarks>Called by Unity every frame while this component is enabled and active.</remarks>
        private void Update()
        {
            if (!IsPlaying)
                return;

            if (_Graph.IsDone())
            {
                IsPlaying = false;
            }
            else if (_Speed < 0 && Time <= 0)
            {
                IsPlaying = false;
                Time = 0;
            }
        }

        /************************************************************************************************************************/

        /// <summary>Ensures that the <see cref="_Graph"/> is properly cleaned up.</summary>
        /// <remarks>Called by Unity when this component becomes disabled or inactive.</remarks>
        private void OnDisable()
        {
            IsPlaying = false;

            if (_Animator.keepAnimatorStateOnDisable)
                return;

            if (IsInitialised)
            {
                // We need to call SetTime twice to ensure that animation events aren't triggered incorrectly.
                _Playable.SetTime(0);
                _Playable.SetTime(0);
            }
        }

        /************************************************************************************************************************/

        /// <summary>Ensures that the <see cref="PlayableGraph"/> is properly cleaned up.</summary>
        /// <remarks>Called by Unity when this component is destroyed.</remarks>
        private void OnDestroy()
        {
            if (IsInitialised)
                _Graph.Destroy();
        }

        /************************************************************************************************************************/

#if UNITY_EDITOR
        /// <summary>[Editor-Only]
        /// Ensures that the <see cref="PlayableGraph"/> is destroyed.
        /// </summary>
        ~SoloAnimation()
        {
            UnityEditor.EditorApplication.delayCall += OnDestroy;
        }
#endif

        /************************************************************************************************************************/

        /// <summary>[<see cref="IAnimationClipSource"/>] Adds the <see cref="Clip"/> to the list.</summary>
        public void GetAnimationClips(List<AnimationClip> clips)
        {
            if (_Clip != null)
                clips.Add(_Clip);
        }

        /************************************************************************************************************************/
    }
}

/************************************************************************************************************************/
#if UNITY_EDITOR
/************************************************************************************************************************/

namespace Animancer.Editor
{
    [UnityEditor.CustomEditor(typeof(SoloAnimation)), UnityEditor.CanEditMultipleObjects]
    internal sealed class SoloAnimationEditor : UnityEditor.Editor
    {
        /************************************************************************************************************************/

        /// <summary>The animator referenced by each target.</summary>
        private Animator[] _Animators;

        /// <summary>A <see cref="UnityEditor.SerializedObject"/> encapsulating the <see cref="_Animators"/>.</summary>
        private UnityEditor.SerializedObject _SerializedAnimator;

        /// <summary>The <see cref="Animator.keepAnimatorControllerStateOnDisable"/> property.</summary>
        private UnityEditor.SerializedProperty _KeepStateOnDisable;

        /************************************************************************************************************************/

        public override void OnInspectorGUI()
        {
            DoSerializedFieldsGUI();
            RefreshSerializedAnimator();
            DoStopOnDisableGUI();
            DoRuntimeDetailsGUI();
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Draws the target's serialized fields.
        /// </summary>
        private void DoSerializedFieldsGUI()
        {
            serializedObject.Update();

            var property = serializedObject.GetIterator();

            property.NextVisible(true);

            if (property.name != "m_Script")
                UnityEditor.EditorGUILayout.PropertyField(property, true);

            while (property.NextVisible(false))
            {
                UnityEditor.EditorGUILayout.PropertyField(property, true);
            }

            serializedObject.ApplyModifiedProperties();
        }

        /************************************************************************************************************************/

        private void RefreshSerializedAnimator()
        {
            var targets = this.targets;

            if (_Animators == null || _Animators.Length != targets.Length)
                _Animators = new Animator[targets.Length];

            var dirty = false;
            var hasAll = true;

            for (int i = 0; i < _Animators.Length; i++)
            {
                var animator = (targets[i] as SoloAnimation).Animator;
                if (_Animators[i] != animator)
                {
                    _Animators[i] = animator;
                    dirty = true;
                }

                if (animator == null)
                    hasAll = false;
            }

            if (!dirty)
                return;

            OnDisable();

            if (!hasAll)
                return;

            _SerializedAnimator = new UnityEditor.SerializedObject(_Animators);
            _KeepStateOnDisable = _SerializedAnimator.FindProperty("m_KeepAnimatorControllerStateOnDisable");
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Draws a toggle inverted from the <see cref="Animator.keepAnimatorControllerStateOnDisable"/> field.
        /// </summary>
        private void DoStopOnDisableGUI()
        {
            var area = AnimancerGUI.LayoutSingleLineRect();

            var label = AnimancerGUI.TempContent("Stop On Disable",
                "If true, disabling this object will stop and rewind all animations." +
                " Otherwise they will simply be paused and will resume from their current states when it is re-enabled.");

            if (_KeepStateOnDisable != null)
            {
                _KeepStateOnDisable.serializedObject.Update();

                label = UnityEditor.EditorGUI.BeginProperty(area, label, _KeepStateOnDisable);

                _KeepStateOnDisable.boolValue = !UnityEditor.EditorGUI.Toggle(area, label, !_KeepStateOnDisable.boolValue);

                UnityEditor.EditorGUI.EndProperty();

                _KeepStateOnDisable.serializedObject.ApplyModifiedProperties();
            }
            else
            {
                var enabled = GUI.enabled;
                GUI.enabled = false;
                UnityEditor.EditorGUI.Toggle(area, label, false);
                GUI.enabled = enabled;
            }
        }

        /************************************************************************************************************************/

        /// <summary>Draws the target's runtime details.</summary>
        private void DoRuntimeDetailsGUI()
        {
            if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode ||
                targets.Length != 1)
                return;

            AnimancerGUI.BeginVerticalBox(GUI.skin.box);

            var target = (SoloAnimation)this.target;
            if (!target.IsInitialised)
            {
                GUILayout.Label("Not Initialised");
            }
            else
            {
                UnityEditor.EditorGUI.BeginChangeCheck();
                var isPlaying = UnityEditor.EditorGUILayout.Toggle("Is Playing", target.IsPlaying);
                if (UnityEditor.EditorGUI.EndChangeCheck())
                    target.IsPlaying = isPlaying;

                UnityEditor.EditorGUI.BeginChangeCheck();
                var time = UnityEditor.EditorGUILayout.FloatField("Time", target.Time);
                if (UnityEditor.EditorGUI.EndChangeCheck())
                    target.Time = time;

                time = AnimancerUtilities.Wrap01(target.NormalizedTime);
                if (time == 0 && target.Time != 0)
                    time = 1;

                UnityEditor.EditorGUI.BeginChangeCheck();
                time = UnityEditor.EditorGUILayout.Slider("Normalized Time", time, 0, 1);
                if (UnityEditor.EditorGUI.EndChangeCheck())
                    target.NormalizedTime = time;
            }

            AnimancerGUI.EndVerticalBox(GUI.skin.box);
            Repaint();
        }

        /************************************************************************************************************************/

        private void OnDisable()
        {
            if (_SerializedAnimator != null)
            {
                _SerializedAnimator.Dispose();
                _SerializedAnimator = null;
                _KeepStateOnDisable = null;
            }
        }

        /************************************************************************************************************************/
    }
}

/************************************************************************************************************************/
#endif
/************************************************************************************************************************/


// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

using UnityEngine;

namespace Animancer
{
    /// <summary>A <see cref="MixerParameterTween{TParameter}"/> which uses <see cref="Mathf.LerpUnclamped"/>.</summary>
    /// <example><code>
    /// [SerializeField] private AnimancerComponent _Animancer;
    /// [SerializeField] private LinearMixerState.Transition _Mixer;
    /// 
    /// private MixerParameterTweenFloat _MixerTween;
    /// 
    /// private void Awake()
    /// {
    ///     // Play creates the LinearMixerState from the transition.
    ///     _Animancer.Play(_Mixer);
    /// 
    ///     // Now that the state exists, we can create a tween for it.
    ///     _MixerTween = new MixerParameterTweenFloat(_Mixer.State);
    /// 
    ///     // Start tweening the parameter towards 0.5 over a period of 0.25 seconds.
    ///     _MixerTween.Start(0.5f, 0.25f);
    /// }
    /// </code></example>
    /// https://kybernetik.com.au/animancer/api/Animancer/MixerParameterTweenFloat
    /// 
    public class MixerParameterTweenFloat : MixerParameterTween<float>
    {
        public MixerParameterTweenFloat() { }
        public MixerParameterTweenFloat(MixerState<float> mixer) : base(mixer) { }

        protected override float CalculateCurrentValue() => Mathf.LerpUnclamped(StartValue, EndValue, Progress);
    }

    /************************************************************************************************************************/

    /// <summary>A <see cref="MixerParameterTween{TParameter}"/> which uses <see cref="Vector2.LerpUnclamped"/>.</summary>
    /// <example>See <see cref="MixerParameterTweenFloat"/>.</example>
    /// https://kybernetik.com.au/animancer/api/Animancer/MixerParameterTweenVector2
    /// 
    public class MixerParameterTweenVector2 : MixerParameterTween<Vector2>
    {
        public MixerParameterTweenVector2() { }
        public MixerParameterTweenVector2(MixerState<Vector2> mixer) : base(mixer) { }

        protected override Vector2 CalculateCurrentValue() => Vector2.LerpUnclamped(StartValue, EndValue, Progress);
    }

    /************************************************************************************************************************/

    /// <summary>A system which interpolates a <see cref="MixerState{TParameter}.Parameter"/> over time.</summary>
    /// <example>See <see cref="MixerParameterTweenFloat"/>.</example>
    /// https://kybernetik.com.au/animancer/api/Animancer/MixerParameterTween_1
    /// 
    public abstract class MixerParameterTween<TParameter> : Key, IUpdatable
    {
        /************************************************************************************************************************/

        /// <summary>The target <see cref="MixerState{TParameter}"/>.</summary>
        public MixerState<TParameter> Mixer { get; set; }

        /************************************************************************************************************************/

        /// <summary>The value of the <see cref="MixerState{TParameter}.Parameter"/> when this tween started.</summary>
        public TParameter StartValue { get; set; }

        /// <summary>The target value this tween is moving the <see cref="MixerState{TParameter}.Parameter"/> towards.</summary>
        public TParameter EndValue { get; set; }

        /************************************************************************************************************************/

        /// <summary>The amount of time this tween will take (in seconds).</summary>
        public float Duration { get; set; }

        /// <summary>The amount of time that has passed since the <see cref="Start"/> (in seconds).</summary>
        public float Time { get; set; }

        /// <summary>The normalized progress (0 to 1) of this tween towards its goal.</summary>
        public float Progress
        {
            get => Time / Duration;
            set => Time = value * Duration;
        }

        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="MixerParameterTween{TParameter}"/>.</summary>
        public MixerParameterTween() { }

        /// <summary>Creates a new <see cref="MixerParameterTween{TParameter}"/> and sets the <see cref="Mixer"/>.</summary>
        public MixerParameterTween(MixerState<TParameter> mixer) => Mixer = mixer;

        /************************************************************************************************************************/

        /// <summary>
        /// Sets the details of this tween and registers it to be updated so that it can apply its effects every frame.
        /// </summary>
        public void Start(TParameter endValue, float duration)
        {
#if UNITY_ASSERTIONS
            Debug.Assert(Mixer != null, nameof(Mixer) + " is null.");
            Debug.Assert(Mixer.Root != null, $"{nameof(Mixer)}.{nameof(Mixer.Root)} is null.");
#endif

            StartValue = Mixer.Parameter;
            EndValue = endValue;
            Duration = duration;
            Time = 0;

            Mixer.Root.RequireUpdate(this);
        }

        /************************************************************************************************************************/

        /// <summary>Stops this tween from updating.</summary>
        public void Stop() => Mixer?.Root?.CancelUpdate(this);

        /************************************************************************************************************************/

        /// <summary>Is this tween currently being updated?</summary>
        public bool IsActive => IsInList(this);

        /************************************************************************************************************************/

        /// <summary>
        /// Called every update while this tween is active to calculate the what value to set the
        /// <see cref="MixerState{TParameter}.Parameter"/> to. Usually based on the <see cref="StartValue"/>,
        /// <see cref="EndValue"/>, and <see cref="Progress"/>.
        /// </summary>
        protected abstract TParameter CalculateCurrentValue();

        /************************************************************************************************************************/

        void IUpdatable.EarlyUpdate()
        {
            Time += AnimancerPlayable.DeltaTime;

            if (Time < Duration)// Tween.
            {
                Mixer.Parameter = CalculateCurrentValue();
            }
            else// End.
            {
                Time = Duration;
                Mixer.Parameter = EndValue;
                Stop();
            }
        }

        /************************************************************************************************************************/

        void IUpdatable.LateUpdate() { }

        void IUpdatable.OnDestroy() { }

        /************************************************************************************************************************/
    }
}

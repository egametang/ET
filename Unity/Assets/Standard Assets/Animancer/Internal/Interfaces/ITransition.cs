// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

using UnityEngine;

namespace Animancer
{
    /// <summary>An object that can create an <see cref="AnimancerState"/> and set its details.</summary>
    /// <remarks>
    /// Transitions are generally used as arguments for <see cref="AnimancerPlayable.Play(ITransition)"/>.
    /// <para></para>
    /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/transitions">Transitions</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/ITransition
    /// 
    public interface ITransition : IHasKey
    {
        /************************************************************************************************************************/

        /// <summary>
        /// Creates and returns a new <see cref="AnimancerState"/>.
        /// <para></para>
        /// Note that using methods like <see cref="AnimancerPlayable.Play(ITransition)"/> will also call
        /// <see cref="Apply"/>, so if you call this method manually you may want to call that method as well. Or you
        /// can just use <see cref="AnimancerUtilities.CreateStateAndApply"/>.
        /// </summary>
        /// <remarks>
        /// The first time a transition is used on an object, this method is called to create the state and register it
        /// in the internal dictionary using the <see cref="IHasKey.Key"/> so that it can be reused later on.
        /// </remarks>
        AnimancerState CreateState();

        /// <summary>
        /// When a transition is passed into <see cref="AnimancerPlayable.Play(ITransition)"/>, this property
        /// determines which <see cref="Animancer.FadeMode"/> will be used.
        /// </summary>
        FadeMode FadeMode { get; }

        /// <summary>The amount of time the transition should take (in seconds).</summary>
        float FadeDuration { get; }

        /// <summary>
        /// Called by <see cref="AnimancerPlayable.Play(ITransition)"/> to apply any modifications to the `state`.
        /// </summary>
        /// <remarks>
        /// Unlike <see cref="CreateState"/>, this method is called every time the transition is used so it can do
        /// things like set the <see cref="AnimancerState.Events"/> or starting <see cref="AnimancerState.Time"/>.
        /// </remarks>
        void Apply(AnimancerState state);

        /************************************************************************************************************************/
    }

    /// <summary>An <see cref="ITransition"/> with some additional details for the Unity Editor GUI.</summary>
    /// <remarks>
    /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/transitions">Transitions</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/ITransitionDetailed
    /// 
    public interface ITransitionDetailed : ITransition
    {
        /************************************************************************************************************************/

        /// <summary>Indicates what the value of <see cref="AnimancerState.IsLooping"/> will be for the created state.</summary>
        bool IsLooping { get; }

        /// <summary>Determines what <see cref="AnimancerState.NormalizedTime"/> to start the animation at.</summary>
        float NormalizedStartTime { get; set; }

        /// <summary>Determines how fast the animation plays (1x = normal speed).</summary>
        float Speed { get; set; }

        /// <summary>Events which will be triggered as the animation plays.</summary>
        AnimancerEvent.Sequence Events { get; }

        /// <summary>[<see cref="SerializeField"/>] Events which will be triggered as the animation plays.</summary>
        ref AnimancerEvent.Sequence.Serializable SerializedEvents { get; }

        /// <summary>The maximum amount of time the animation is expected to take (in seconds).</summary>
        /// <remarks>The actual duration can vary in states like <see cref="MixerState"/>.</remarks>
        float MaximumDuration { get; }

        /// <summary>The <see cref="Motion.averageAngularSpeed"/> that the created state will have.</summary>
        /// <remarks>The actual average velocity can vary in states like <see cref="MixerState"/>.</remarks>
        float AverageAngularSpeed { get; }

        /// <summary>The <see cref="Motion.averageSpeed"/> that the created state will have.</summary>
        /// <remarks>The actual average velocity can vary in states like <see cref="MixerState"/>.</remarks>
        Vector3 AverageVelocity { get; }

        /// <summary>Indicates whether this transition can create a valid <see cref="AnimancerState"/>.</summary>
        bool IsValid { get; }

        /// <summary>The <see cref="AnimancerState.MainObject"/> that the created state will have.</summary>
        Object MainObject { get; }

        /************************************************************************************************************************/
    }

    public partial class AnimancerUtilities
    {
        /************************************************************************************************************************/

        /// <summary>Calls <see cref="AnimancerPlayable.Play(AnimationClip)"/> or <see cref="AnimancerPlayable.Play(ITransition)"/>.</summary>
        /// <remarks>Returns null if the `clipOrTransition` is null or an unsupported type.</remarks>
        public static AnimancerState TryPlay(AnimancerPlayable animancer, Object clipOrTransition)
        {
            if (clipOrTransition is AnimationClip clip)
                return animancer.Play(clip);
            else if (clipOrTransition is ITransitionDetailed transition)
                return animancer.Play(transition);
            else
                return null;
        }

        /************************************************************************************************************************/

        /// <summary>Outputs the <see cref="Motion.averageAngularSpeed"/> or <see cref="ITransitionDetailed.AverageAngularSpeed"/>.</summary>
        /// <remarks>Returns false if the `clipOrTransition` is null or an unsupported type.</remarks>
        public static bool TryGetAverageAngularSpeed(Object clipOrTransition, out float averageAngularSpeed)
        {
            if (clipOrTransition is Motion motion)
            {
                averageAngularSpeed = motion.averageAngularSpeed;
                return true;
            }
            else if (clipOrTransition is ITransitionDetailed transition)
            {
                averageAngularSpeed = transition.AverageAngularSpeed;
                return true;
            }
            else
            {
                averageAngularSpeed = default;
                return false;
            }
        }

        /************************************************************************************************************************/

        /// <summary>Outputs the <see cref="Motion.averageSpeed"/> or <see cref="ITransitionDetailed.AverageVelocity"/>.</summary>
        /// <remarks>Returns false if the `clipOrTransition` is null or an unsupported type.</remarks>
        public static bool TryGetAverageVelocity(Object clipOrTransition, out Vector3 averageVelocity)
        {
            if (clipOrTransition is Motion motion)
            {
                averageVelocity = motion.averageSpeed;
                return true;
            }
            else if (clipOrTransition is ITransitionDetailed transition)
            {
                averageVelocity = transition.AverageVelocity;
                return true;
            }
            else
            {
                averageVelocity = default;
                return false;
            }
        }

        /************************************************************************************************************************/

        /// <summary>Outputs the <see cref="Motion.isLooping"/> or <see cref="ITransitionDetailed.IsLooping"/>.</summary>
        /// <remarks>Returns false if the `clipOrTransition` is null or an unsupported type.</remarks>
        public static bool TryGetIsLooping(Object clipOrTransition, out bool isLooping)
        {
            if (clipOrTransition is Motion motion)
            {
                isLooping = motion.isLooping;
                return true;
            }
            else if (clipOrTransition is ITransitionDetailed transition)
            {
                isLooping = transition.IsLooping;
                return true;
            }
            else
            {
                isLooping = false;
                return false;
            }
        }

        /************************************************************************************************************************/

        /// <summary>Outputs the <see cref="AnimationClip.length"/> or <see cref="ITransitionDetailed.MaximumDuration"/>.</summary>
        /// <remarks>Returns false if the `clipOrTransition` is null or an unsupported type.</remarks>
        public static bool TryGetLength(Object clipOrTransition, out float length)
        {
            if (clipOrTransition is AnimationClip clip)
            {
                length = clip.length;
                return true;
            }
            else if (clipOrTransition is ITransitionDetailed transition)
            {
                length = transition.MaximumDuration;
                return true;
            }
            else
            {
                length = 0;
                return false;
            }
        }

        /************************************************************************************************************************/
    }
}


// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

using System;
using System.Text;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Animancer
{
    /// <summary>
    /// A callback to be invoked by Animation Events that have been triggered by a specific animation.
    /// </summary>
    /// 
    /// <remarks>
    /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/events/animation">Animation Events</see>
    /// </remarks>
    /// 
    /// <example>
    /// To set up a receiver for an Animation Event with the Function Name "Event":
    /// <para></para><code>
    /// /// &lt;summary&gt;A callback for Animation Events with the Function Name "Event".&lt;/summary&gt;
    /// public AnimationEventReceiver onEvent;
    /// 
    /// /// &lt;summary&gt;Called by Animation Events.&lt;/summary&gt;
    /// private void Event(AnimationEvent animationEvent)
    /// {
    ///     // This is optional and will automatically be compiled out of runtime builds.
    ///     // It allows the receiver to perform additional safety checks.
    ///     onEvent.SetFunctionName("Event");
    ///     
    ///     onEvent.HandleEvent(animationEvent);
    /// }
    /// </code>
    /// Then to register a callback to that receiver:
    /// <para></para><code>
    /// var state = animancer.Play(clip);
    /// onEvent.Set(state, (animationEvent) =>
    /// {
    ///     ...
    /// });
    /// </code></example>
    /// 
    /// https://kybernetik.com.au/animancer/api/Animancer/AnimationEventReceiver
    /// 
    public struct AnimationEventReceiver
    {
        /************************************************************************************************************************/

        private AnimancerState _Source;
        private int _SourceID;

        /// <summary>
        /// If set, only Animation Events caused by this state before the
        /// <see cref="AnimancerLayer.CommandCount"/> changes will actually trigger the <see cref="Callback"/>.
        /// </summary>
        public AnimancerState Source
        {
            get
            {
                if (_Source == null ||
                    _Source.Layer.CommandCount != _SourceID)
                    return null;

                return _Source;
            }
            set
            {
                _Source = value;

                if (value != null)
                    _SourceID = value.Layer.CommandCount;
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// A delegate that will be invoked by <see cref="HandleEvent"/>.
        /// <para></para>
        /// It is recommended that you use <see cref="Set"/> or manually specify a <see cref="Source"/> when assigning
        /// this reference. Otherwise events from an animation that is fading out might trigger a callback you just
        /// registered for a new animation that is fading in.
        /// </summary>
        public Action<AnimationEvent> Callback { get; set; }

        /************************************************************************************************************************/

        /// <summary>
        /// Creates a new <see cref="AnimationEventReceiver"/> and sets the <see cref="Source"/> and
        /// <see cref="Callback"/>.
        /// </summary>
        public AnimationEventReceiver(AnimancerState source, Action<AnimationEvent> callback)
        {
            _Source = source;
            _SourceID = source != null ? source.Layer.CommandCount : -1;

            Callback = callback;

#if UNITY_EDITOR
            FunctionName = null;
            ValidateSourceHasCorrectEvent();
#endif
        }

        /// <summary>
        /// Sets the <see cref="Source"/> and <see cref="Callback"/>.
        /// </summary>
        public void Set(AnimancerState source, Action<AnimationEvent> callback)
        {
            Source = source;
            Callback = callback;

#if UNITY_EDITOR
            ValidateSourceHasCorrectEvent();
#endif
        }

        /************************************************************************************************************************/

#if UNITY_EDITOR
        /// <summary>[Editor-Only] The function name of the event that this receiver is intended for.</summary>
        public string FunctionName { get; private set; }
#endif

        /// <summary>[Editor-Conditional]
        /// Sets the <see cref="FunctionName"/> so <see cref="Set"/> can perform additional safety checks to ensure
        /// that the <see cref="AnimancerState.Clip"/> actually has an event with the expected `name` and also to allow
        /// <see cref="HandleEvent"/> to verify that any events it is given have that `name` as well.
        /// </summary>
        [System.Diagnostics.Conditional(Strings.UnityEditor)]
        public void SetFunctionName(string name)
        {
#if UNITY_EDITOR
            FunctionName = name;
#endif
        }

#if UNITY_EDITOR
        /// <summary>[Editor-Only]
        /// If a <see cref="Source"/> and <see cref="FunctionName"/> have been assigned but the
        /// <see cref="AnimancerState.Clip"/> has no event with that name, this method logs a warning.
        /// </summary>
        private void ValidateSourceHasCorrectEvent()
        {
            if (FunctionName == null || _Source == null || AnimancerUtilities.HasEvent(_Source, FunctionName))
                return;

            var message = ObjectPool.AcquireStringBuilder()
                .Append("No Animation Event was found in ")
                .Append(_Source)
                .Append(" with the Function Name '")
                .Append(FunctionName)
                .Append('\'');

            if (_Source != null)
            {
                message.Append('\n');
                _Source.Root.AppendDescription(message);
            }

            Debug.LogWarning(message.ReleaseToString(), _Source.Root?.Component as Object);
        }
#endif

        /************************************************************************************************************************/

        /// <summary>
        /// Clears the <see cref="Source"/> and <see cref="Callback"/>.
        /// </summary>
        public void Clear()
        {
            _Source = null;
            Callback = null;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Invokes the <see cref="Callback"/> if either no <see cref="Source"/> has been set or if it is still
        /// current and its <see cref="AnimancerState.Clip"/> matches the one triggering the event.
        /// </summary>
        public bool HandleEvent(AnimationEvent animationEvent)
        {
            if (Callback == null)
                return false;

            if (_Source != null)
            {
                if (_Source.Layer.CommandCount != _SourceID ||
                    !ReferenceEquals(_Source.Clip, animationEvent.animatorClipInfo.clip))
                    return false;
            }

#if UNITY_EDITOR
            if (FunctionName != null && FunctionName != animationEvent.functionName)
                throw new ArgumentException(
                    $"Function Name Mismatch: receiver.{nameof(FunctionName)}='{FunctionName}'" +
                    $" while {nameof(animationEvent)}.{nameof(animationEvent.functionName)}='{animationEvent.functionName}'");
#endif

            Callback(animationEvent);
            return true;
        }

        /************************************************************************************************************************/
    }
}

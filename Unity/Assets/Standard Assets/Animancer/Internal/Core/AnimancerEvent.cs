// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Animancer
{
    /// <summary>
    /// A <see cref="callback"/> delegate paired with a <see cref="normalizedTime"/> to determine when to invoke it.
    /// </summary>
    /// <remarks>
    /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/events/animancer">Animancer Events</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/AnimancerEvent
    /// 
    public partial struct AnimancerEvent : IEquatable<AnimancerEvent>
    {
        /************************************************************************************************************************/
        #region Event
        /************************************************************************************************************************/

        /// <summary>The <see cref="AnimancerState.NormalizedTime"/> at which to invoke the <see cref="callback"/>.</summary>
        public float normalizedTime;

        /// <summary>The delegate to invoke when the <see cref="normalizedTime"/> passes.</summary>
        public Action callback;

        /************************************************************************************************************************/

        /// <summary>The largest possible float value less than 1.</summary>
        /// <remarks>
        /// This value is useful for placing events at the end of a looping animation since they do not allow the
        /// <see cref="normalizedTime"/> to be greater than or equal to 1.
        /// </remarks>
        public const float AlmostOne = 0.99999994f;

        /************************************************************************************************************************/

        /// <summary>Does nothing.</summary>
        /// <remarks>This delegate is used for events which would otherwise have a <c>null</c> <see cref="callback"/>.</remarks>
        public static readonly Action DummyCallback = Dummy;

        /// <summary>Does nothing.</summary>
        /// <remarks>Used by <see cref="DummyCallback"/>.</remarks>
        private static void Dummy() { }

        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="AnimancerEvent"/>.</summary>
        public AnimancerEvent(float normalizedTime, Action callback)
        {
            this.normalizedTime = normalizedTime;
            this.callback = callback;
        }

        /************************************************************************************************************************/

        /// <summary>Returns "AnimancerEvent(normalizedTime, callback)".</summary>
        public override string ToString()
        {
            var text = ObjectPool.AcquireStringBuilder()
                .Append($"{nameof(AnimancerEvent)}(")
                .Append(normalizedTime)
                .Append(", ");

            if (callback == null)
            {
                text.Append("null)");
            }
            else if (callback.Target == null)
            {
                text.Append(callback.Method.Name)
                    .Append(")");
            }
            else
            {
                text.Append(callback.Target)
                    .Append('.')
                    .Append(callback.Method.Name)
                    .Append(")");
            }

            return text.ReleaseToString();
        }

        /************************************************************************************************************************/

        /// <summary>Appends the details of this event to the `text`.</summary>
        public void AppendDetails(StringBuilder text, string name, string delimiter = "\n")
        {
            text.Append(delimiter).Append(name).Append(".NormalizedTime: ").Append(normalizedTime);

            if (callback != null)
            {
                text.Append(delimiter).Append(name).Append($".{nameof(callback.Target)}: ").Append(callback.Target);
                text.Append(delimiter).Append(name).Append($".{nameof(callback.Method)}: ").Append(callback.Method);
            }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Invocation
        /************************************************************************************************************************/

        /// <summary>The <see cref="AnimancerState"/> currently triggering an event using <see cref="Invoke"/>.</summary>
        public static AnimancerState CurrentState => _CurrentState;
        private static AnimancerState _CurrentState;

        /************************************************************************************************************************/

        /// <summary>The <see cref="AnimancerEvent"/> currently being triggered by <see cref="Invoke"/>.</summary>
        public static ref readonly AnimancerEvent CurrentEvent => ref _CurrentEvent;
        private static AnimancerEvent _CurrentEvent;

        /************************************************************************************************************************/

        /// <summary>
        /// Sets the static <see cref="CurrentState"/> and <see cref="CurrentEvent"/> then invokes the <see cref="callback"/>.
        /// <para></para>
        /// This method catches and logs any exception thrown by the <see cref="callback"/>.
        /// </summary>
        /// <exception cref="NullReferenceException">The <see cref="callback"/> is null.</exception>
        public void Invoke(AnimancerState state)
        {
            var previousState = _CurrentState;
            var previousEvent = _CurrentEvent;

            _CurrentState = state;
            _CurrentEvent = this;

            try
            {
                callback();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, state?.Root?.Component as Object);
            }

            _CurrentState = previousState;
            _CurrentEvent = previousEvent;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Returns either the `minDuration` or the <see cref="AnimancerState.RemainingDuration"/> of the
        /// <see cref="CurrentState"/> state (whichever is higher).
        /// </summary>
        public static float GetFadeOutDuration(float minDuration = AnimancerPlayable.DefaultFadeDuration)
        {
            var state = CurrentState;
            if (state == null)
                return minDuration;

            var time = state.Time;
            var speed = state.EffectiveSpeed;

            float remainingDuration;
            if (state.IsLooping)
            {
                var previousTime = time - speed * Time.deltaTime;
                var inverseLength = 1f / state.Length;

                // If we just passed the end of the animation, the remaining duration would technically be the full
                // duration of the animation, so we most likely want to use the minimum duration instead.
                if (Math.Floor(time * inverseLength) != Math.Floor(previousTime * inverseLength))
                    return minDuration;
            }

            if (speed > 0)
            {
                remainingDuration = (state.Length - time) * speed;
            }
            else
            {
                remainingDuration = time * -speed;
            }

            return Math.Max(minDuration, remainingDuration);
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Operators
        /************************************************************************************************************************/

        /// <summary>Are the <see cref="normalizedTime"/> and <see cref="callback"/> equal?</summary>
        public static bool operator ==(AnimancerEvent a, AnimancerEvent b) =>
            a.normalizedTime == b.normalizedTime &&
            a.callback == b.callback;

        /// <summary>Are the <see cref="normalizedTime"/> and <see cref="callback"/> not equal?</summary>
        public static bool operator !=(AnimancerEvent a, AnimancerEvent b) => !(a == b);

        /************************************************************************************************************************/

        /// <summary>[<see cref="IEquatable{AnimancerEvent}"/>]
        /// Are the <see cref="normalizedTime"/> and <see cref="callback"/> of this event equal to those of the
        /// `animancerEvent`?
        /// </summary>
        public bool Equals(AnimancerEvent animancerEvent) => this == animancerEvent;

        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is AnimancerEvent animancerEvent && this == animancerEvent;

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            const int Multiplyer = -1521134295;

            var hashCode = -78069441;
            hashCode = hashCode * Multiplyer + normalizedTime.GetHashCode();

            if (callback != null)
                hashCode = hashCode * Multiplyer + callback.GetHashCode();

            return hashCode;
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}


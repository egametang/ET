// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

using System;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Animancer
{
    /// https://kybernetik.com.au/animancer/api/Animancer/AnimancerState
    /// 
    partial class AnimancerState
    {
        /************************************************************************************************************************/

        /// <summary>
        /// The <see cref="IUpdatable"/> that manages the events of this state.
        /// <para></para>
        /// This field is null by default, acquires its reference from an <see cref="ObjectPool"/> when accessed, and
        /// if it contains no events at the end of an update it releases the reference back to the pool.
        /// </summary>
        private EventRunner _EventRunner;

        /************************************************************************************************************************/

        /// <summary>
        /// A list of <see cref="AnimancerEvent"/>s that will occur while this state plays as well as one that
        /// specifically defines when this state ends.
        /// <para></para>
        /// Accessing this property will acquire a spare <see cref="AnimancerEvent.Sequence"/> from the
        /// <see cref="ObjectPool"/> if none was already assigned. You can use <see cref="HasEvents"/> to check
        /// beforehand.
        /// <para></para>
        /// <em>Animancer Lite does not allow the use of events in runtime builds, except for
        /// <see cref="AnimancerEvent.Sequence.OnEnd"/>.</em>
        /// </summary>
        public AnimancerEvent.Sequence Events
        {
            get
            {
                EventRunner.Acquire(this);
                return _EventRunner.Events;
            }
            set
            {
                if (value != null)
                {
                    EventRunner.Acquire(this);
                    _EventRunner.Events = value;
                }
                else if (_EventRunner != null)
                {
                    _EventRunner.Events = null;
                }
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Indicates whether this state currently has an <see cref="AnimancerEvent.Sequence"/> (since accessing the
        /// <see cref="Events"/> would automatically get one from the <see cref="ObjectPool"/>).
        /// </summary>
        public bool HasEvents => _EventRunner != null;

        /************************************************************************************************************************/

#if UNITY_ASSERTIONS
        /// <summary>[Assert-Only]
        /// Returns null if Animancer Events will work properly on this type of state, or a message explaining why they
        /// might not work.
        /// </summary>
        protected virtual string UnsupportedEventsMessage => null;
#endif

        /************************************************************************************************************************/
        #region Object Pools
        /************************************************************************************************************************/

        /// <summary>
        /// The capacity of the internal list in the <see cref="ObjectPool{T}"/> for <see cref="EventRunner"/>.
        /// </summary>
        public static int EventPoolCapacity
        {
            get => ObjectPool<EventRunner>.Capacity;
            set => ObjectPool<EventRunner>.Capacity = value;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// The number of spare items in the <see cref="ObjectPool{T}"/> for <see cref="EventRunner"/>.
        /// </summary>
        public static int EventPoolCount
        {
            get => ObjectPool<EventRunner>.Count;
            set => ObjectPool<EventRunner>.Count = value;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// If the <see cref="EventPoolCount"/> is less than the specified value, this method increases it to that
        /// value by creating new objects.
        /// </summary>
        public static void SetMinEventPoolCount(int count) => ObjectPool<EventRunner>.SetMinCount(count);

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/

        /// <summary>An <see cref="IUpdatable"/> which manages the triggering of events.</summary>
        /// <remarks>
        /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/events/animancer">Animancer Events</see>
        /// </remarks>
        private sealed class EventRunner : Key, IUpdatable
        {
            /************************************************************************************************************************/
            #region Pooling
            /************************************************************************************************************************/

            /// <summary>
            /// If the `state` has no <see cref="EventRunner"/>, this method gets one from the
            /// <see cref="ObjectPool"/>.
            /// </summary>
            public static void Acquire(AnimancerState state)
            {
                var runner = state._EventRunner;
                if (runner != null)
                    return;

                ObjectPool.Acquire(out runner);

#if UNITY_ASSERTIONS
                runner._LoggedEndEventInterrupt = false;

                OptionalWarning.UnsupportedEvents.Log(state.UnsupportedEventsMessage, state.Root?.Component);

                if (runner._State != null)
                    Debug.LogError(runner + " already has a state even though it was in the list of spares.",
                        state.Root?.Component as Object);

                if (runner._Events != null)
                    Debug.LogError(runner + " has event sequence even though it was in the list of spares.",
                        state.Root?.Component as Object);

                if (runner._GotEventsFromPool)
                    Debug.LogError(runner + " is marked as having pooled events even though it has no events.",
                        state.Root?.Component as Object);

                if (runner._NextEventIndex != RecalculateEventIndex)
                    Debug.LogError($"{runner} has a {nameof(_NextEventIndex)} even though it was pooled.",
                        state.Root?.Component as Object);

                if (IsInList(runner))
                    Debug.LogError(runner + " is currently in a Keyed List even though it was also in the list of spares.",
                        state.Root?.Component as Object);
#endif

                runner._IsLooping = state.IsLooping;
                runner._State = state;
                state._EventRunner = runner;
                state.Root?.RequireUpdate(runner);
            }

            /************************************************************************************************************************/

            /// <summary>Returns this <see cref="EventRunner"/> to the <see cref="ObjectPool"/>.</summary>
            private void Release()
            {
                if (_State == null)
                    return;

                _State.Root?.CancelUpdate(this);

                Events = null;
                _State._EventRunner = null;
                _State = null;

                ObjectPool.Release(this);
            }

            /************************************************************************************************************************/

            /// <summary>
            /// If the <see cref="AnimancerEvent.Sequence"/> was acquired from the <see cref="ObjectPool"/>, this
            /// method clears it. Otherwise it simply discards the reference.
            /// </summary>
            public static void TryClear(EventRunner events)
            {
                if (events != null)
                    events.Events = null;
            }

            /************************************************************************************************************************/
            #endregion
            /************************************************************************************************************************/

            private AnimancerState _State;
            private AnimancerEvent.Sequence _Events;
            private bool _GotEventsFromPool;
            private bool _IsLooping;
            private float _PreviousTime;
            private int _NextEventIndex = RecalculateEventIndex;
            private int _SequenceVersion;
            private bool _WasPlayingForwards;

            private const int RecalculateEventIndex = int.MinValue;

            /// <summary>
            /// This system accounts for external modifications to the sequence, but modifying it while checking which
            /// of its events to update is not allowed because it would be impossible to efficiently keep track of
            /// which events have been checked/invoked and which still need to be checked.
            /// </summary>
            private const string SequenceVersionException =
                nameof(AnimancerState) + "." + nameof(AnimancerState.Events) + " sequence was modified while iterating through it." +
                " Events in a sequence must not modify that sequence.";

            /************************************************************************************************************************/

            public AnimancerEvent.Sequence Events
            {
                get
                {
                    if (_Events == null)
                    {
                        ObjectPool.Acquire(out _Events);
                        _GotEventsFromPool = true;

#if UNITY_ASSERTIONS
                        if (!_Events.IsEmpty)
                            Debug.LogError(_Events + " is not in its default state even though it was in the list of spares.",
                            _State?.Root?.Component as Object);
#endif
                    }

                    return _Events;
                }
                set
                {
                    if (_GotEventsFromPool)
                    {
                        _Events.Clear();
                        ObjectPool.Release(_Events);
                        _GotEventsFromPool = false;
                    }

                    _Events = value;
                    _NextEventIndex = RecalculateEventIndex;
                }
            }

            /************************************************************************************************************************/

            void IUpdatable.EarlyUpdate()
            {
                if (_Events == null || _Events.IsEmpty)
                {
                    Release();
                    return;
                }

                _PreviousTime = _State.NormalizedTime;
            }

            /************************************************************************************************************************/

            void IUpdatable.LateUpdate()
            {
                if (_Events == null || _Events.IsEmpty)
                {
                    Release();
                    return;
                }

                var length = _State.Length;
                if (length == 0)
                {
                    UpdateZeroLength();
                    return;
                }

                var currentTime = _State.Time / length;
                if (_PreviousTime == currentTime)
                    return;

                // General events are triggered on the frame when their time passes.
                // This happens either once or repeatedly depending on whether the animation is looping or not.
                CheckGeneralEvents(currentTime);
                if (_Events == null)
                {
                    Release();
                    return;
                }

                // End events are triggered every frame after their time passes. This ensures that assigning the event
                // after the time has passed will still trigger it rather than leaving it playing indefinitely.
                var endEvent = _Events.endEvent;
                if (endEvent.callback != null)
                {

                    if (currentTime > _PreviousTime)// Playing Forwards.
                    {
                        var eventTime = float.IsNaN(endEvent.normalizedTime) ?
                            1 : endEvent.normalizedTime;

                        if (currentTime > eventTime)
                        {
                            ValidateBeforeEndEvent();
                            endEvent.Invoke(_State);
                            ValidateAfterEndEvent(endEvent.callback);
                        }
                    }
                    else// Playing Backwards.
                    {
                        var eventTime = float.IsNaN(endEvent.normalizedTime) ?
                            0 : endEvent.normalizedTime;

                        if (currentTime < eventTime)
                        {
                            ValidateBeforeEndEvent();
                            endEvent.Invoke(_State);
                            ValidateAfterEndEvent(endEvent.callback);
                        }
                    }
                }
            }

            /************************************************************************************************************************/
            #region End Event Validation
            /************************************************************************************************************************/

#if UNITY_ASSERTIONS
            private bool _LoggedEndEventInterrupt;

            private const string DelegatesFieldName = "delegates";

            private static FieldInfo _DelegatesField;
            private static bool _GotDelegatesField;
            private static AnimancerLayer _BeforeEndLayer;
            private static int _BeforeEndCommandCount;
#endif

            /************************************************************************************************************************/

            /// <summary>[Assert-Conditional]
            /// Called after the <see cref="AnimancerEvent.Sequence.endEvent"/> is triggered to log a warning if the
            /// <see cref="_State"/> was not interrupted or the `callback` contains multiple calls to the same method.
            /// </summary>
            /// <remarks>
            /// It would be better if we could validate the callback when it is assigned to get a useful stack trace,
            /// but that is unfortunately not possible since <see cref="AnimancerEvent.Sequence.endEvent"/> needs to be
            /// a field for efficiency.
            /// </remarks>
            [System.Diagnostics.Conditional(Strings.Assertions)]
            private void ValidateBeforeEndEvent()
            {
#if UNITY_ASSERTIONS
                _BeforeEndLayer = _State.Layer;
                _BeforeEndCommandCount = _BeforeEndLayer.CommandCount;
#endif
            }

            /************************************************************************************************************************/

            /// <summary>[Assert-Conditional]
            /// Called after the <see cref="AnimancerEvent.Sequence.endEvent"/> is triggered to log a warning if the
            /// <see cref="_State"/> was not interrupted or the `callback` contains multiple calls to the same method.
            /// </summary>
            /// <remarks>
            /// It would be better if we could validate the callback when it is assigned to get a useful stack trace,
            /// but that is unfortunately not possible since <see cref="AnimancerEvent.Sequence.endEvent"/> needs to be
            /// a field for efficiency.
            /// </remarks>
            [System.Diagnostics.Conditional(Strings.Assertions)]
            private void ValidateAfterEndEvent(Action callback)
            {
#if UNITY_ASSERTIONS
                if (!_LoggedEndEventInterrupt &&
                    _Events != null)
                {
                    var layer = _State.Layer;
                    if (_BeforeEndLayer == layer &&
                        _BeforeEndCommandCount == layer.CommandCount &&
                       _State.Root.IsGraphPlaying &&
                       _State.IsPlaying &&
                       _State.EffectiveSpeed != 0)
                    {
                        _LoggedEndEventInterrupt = true;
                        if (OptionalWarning.EndEventInterrupt.IsEnabled())
                            OptionalWarning.EndEventInterrupt.Log(
                                $"An End Event did not actually end the animation ({callback.Method.DeclaringType.Name}.{callback.Method.Name})." +
                                " End Events are triggered every frame after their time has passed," +
                                " so in this case it might be necessary to explicitly clear the event or simply use a regular event instead." +
                                " If you intend for the event to keep getting triggered, you can just disable this warning.",
                                _State.Root?.Component);
                    }
                }

                if (OptionalWarning.DuplicateEvent.IsDisabled())
                    return;

                // The GetInvocationList method allocates garbage every time so instead we use reflection to get the
                // internal field value directly.
                if (!_GotDelegatesField)
                {
                    _GotDelegatesField = true;
                    _DelegatesField = typeof(MulticastDelegate).GetField(DelegatesFieldName,
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                    if (_DelegatesField != null && _DelegatesField.FieldType != typeof(Delegate[]))
                        _DelegatesField = null;

#if UNITY_EDITOR
                    if (_DelegatesField == null)
                        Editor.AnimancerEditorUtilities.RegisterNonCriticalMissingMember(nameof(MulticastDelegate), DelegatesFieldName);
#endif
                }

                if (_DelegatesField == null)
                    return;

                var obj = _DelegatesField.GetValue(callback);
                if (obj == null)
                    return;

                var delegates = (Delegate[])obj;
                var count = delegates.Length;
                for (int iA = 0; iA < count; iA++)
                {
                    var a = delegates[iA];
                    for (int iB = iA + 1; iB < count; iB++)
                    {
                        var b = delegates[iB];

                        if (a == b)
                        {
                            OptionalWarning.DuplicateEvent.Log(
                                $"The {nameof(AnimancerEvent)}.{nameof(AnimancerEvent.Sequence)}.{nameof(AnimancerEvent.Sequence.OnEnd)}" +
                                " callback being invoked contains multiple identical delegates which may mean" +
                                " that they are being unintentionally added multiple times." +
                                $"\n - State: {_State}" +
                                $"\n - Method: {a.Method.Name}",
                                _State.Root?.Component);
                        }
                        else if (a?.Method == b?.Method)
                        {
                            OptionalWarning.DuplicateEvent.Log(
                                $"The {nameof(AnimancerEvent)}.{nameof(AnimancerEvent.Sequence)}.{nameof(AnimancerEvent.Sequence.OnEnd)}" +
                                " callback being invoked contains multiple delegates using the same method with different targets." +
                                " This often happens when a Transition is shared by multiple objects," +
                                " in which case it can be avoided by giving each object its own" +
                                $" {nameof(AnimancerEvent)}.{nameof(AnimancerEvent.Sequence)} as explained in the documentation:" +
                                $" {Strings.DocsURLs.SharedEventSequences}" +
                                $"\n - State: {_State}" +
                                $"\n - Method: {a.Method.Name}",
                                _State.Root?.Component);
                        }
                    }
                }
#endif
            }

            /************************************************************************************************************************/
            #endregion
            /************************************************************************************************************************/

            public void OnTimeChanged()
            {
                _NextEventIndex = RecalculateEventIndex;
            }

            /************************************************************************************************************************/

            /// <summary>If the state has zero length, trigger its end event every frame.</summary>
            private void UpdateZeroLength()
            {
                var speed = _State.EffectiveSpeed;
                if (speed == 0)
                    return;

                if (_Events.Count > 0)
                {
                    var sequenceVersion = _Events.Version;

                    int playDirectionInt;
                    if (speed < 0)
                    {
                        playDirectionInt = -1;
                        if (_NextEventIndex == RecalculateEventIndex ||
                            _SequenceVersion != sequenceVersion ||
                            _WasPlayingForwards)
                        {
                            _NextEventIndex = Events.Count - 1;
                            _SequenceVersion = sequenceVersion;
                            _WasPlayingForwards = false;
                        }
                    }
                    else
                    {
                        playDirectionInt = 1;
                        if (_NextEventIndex == RecalculateEventIndex ||
                            _SequenceVersion != sequenceVersion ||
                            !_WasPlayingForwards)
                        {
                            _NextEventIndex = 0;
                            _SequenceVersion = sequenceVersion;
                            _WasPlayingForwards = true;
                        }
                    }

                    if (!InvokeAllEvents(1, playDirectionInt))
                        return;
                }

                var endEvent = _Events.endEvent;
                if (endEvent.callback != null)
                    endEvent.Invoke(_State);
            }

            /************************************************************************************************************************/

            private void CheckGeneralEvents(float currentTime)
            {
                var count = _Events.Count;
                if (count == 0)
                    return;

                ValidateNextEventIndex(ref currentTime, out var playDirectionFloat, out var playDirectionInt);

                if (_IsLooping)// Looping.
                {
                    var animancerEvent = _Events[_NextEventIndex];
                    var eventTime = animancerEvent.normalizedTime * playDirectionFloat;

                    var loopDelta = GetLoopDelta(_PreviousTime, currentTime, eventTime);
                    if (loopDelta == 0)
                        return;

                    // For each additional loop, invoke all events without needing to check their times.
                    if (!InvokeAllEvents(loopDelta - 1, playDirectionInt))
                        return;

                    var loopStartIndex = _NextEventIndex;

                    Invoke:
                    animancerEvent.Invoke(_State);

                    if (!NextEventLooped(playDirectionInt) ||
                        _NextEventIndex == loopStartIndex)
                        return;

                    animancerEvent = _Events[_NextEventIndex];
                    eventTime = animancerEvent.normalizedTime * playDirectionFloat;
                    if (loopDelta == GetLoopDelta(_PreviousTime, currentTime, eventTime))
                        goto Invoke;
                }
                else// Non-Looping.
                {
                    while ((uint)_NextEventIndex < (uint)count)
                    {
                        var animancerEvent = _Events[_NextEventIndex];
                        var eventTime = animancerEvent.normalizedTime * playDirectionFloat;

                        if (currentTime <= eventTime)
                            break;

                        animancerEvent.Invoke(_State);

                        if (!NextEvent(playDirectionInt))
                            return;
                    }
                }
            }

            /************************************************************************************************************************/

            private void ValidateNextEventIndex(ref float currentTime,
                out float playDirectionFloat, out int playDirectionInt)
            {
                var sequenceVersion = _Events.Version;

                if (currentTime < _PreviousTime)// Playing Backwards.
                {
                    var previousTime = _PreviousTime;
                    _PreviousTime = -previousTime;
                    currentTime = -currentTime;
                    playDirectionFloat = -1;
                    playDirectionInt = -1;

                    if (_NextEventIndex == RecalculateEventIndex ||
                        _SequenceVersion != sequenceVersion ||
                        _WasPlayingForwards)
                    {
                        _NextEventIndex = _Events.Count - 1;
                        _SequenceVersion = sequenceVersion;
                        _WasPlayingForwards = false;

                        if (_IsLooping)
                            previousTime = AnimancerUtilities.Wrap01(previousTime);

                        while (_NextEventIndex > 0 &&
                            _Events[_NextEventIndex].normalizedTime > previousTime)
                            _NextEventIndex--;

                        _Events.AssertNormalizedTimes(_State, _IsLooping);
                    }
                }
                else// Playing Forwards.
                {
                    playDirectionFloat = 1;
                    playDirectionInt = 1;

                    if (_NextEventIndex == RecalculateEventIndex ||
                        _SequenceVersion != sequenceVersion ||
                        !_WasPlayingForwards)
                    {
                        _NextEventIndex = 0;
                        _SequenceVersion = sequenceVersion;
                        _WasPlayingForwards = true;

                        var previousTime = _PreviousTime;
                        if (_IsLooping)
                            previousTime = AnimancerUtilities.Wrap01(previousTime);

                        var max = _Events.Count - 1;
                        while (_NextEventIndex < max &&
                            _Events[_NextEventIndex].normalizedTime < previousTime)
                            _NextEventIndex++;

                        _Events.AssertNormalizedTimes(_State, _IsLooping);
                    }
                }

                // This method could be slightly optimised for playback direction changes by using the current index
                // as the starting point instead of iterating from the edge of the sequence, but that would make it
                // significantly more complex for something that should not happen very often and would only matter if
                // there are lots of events (in which case the optimisation would be tiny compared to the cost of
                // actually invoking all those events and running the rest of the application).
            }

            /************************************************************************************************************************/

            private static int GetLoopDelta(float previousTime, float nextTime, float eventTime)
            {
                previousTime -= eventTime;
                var previousLoopCount = Mathf.FloorToInt(previousTime);
                var nextLoopCount = Mathf.FloorToInt(nextTime - eventTime);

                if (previousTime == previousLoopCount)
                    nextLoopCount++;

                return nextLoopCount - previousLoopCount;
            }

            /************************************************************************************************************************/

            private bool InvokeAllEvents(int count, int playDirectionInt)
            {
                var loopStartIndex = _NextEventIndex;
                while (count-- > 0)
                {
                    do
                    {
                        _Events[_NextEventIndex].Invoke(_State);

                        if (!NextEventLooped(playDirectionInt))
                            return false;
                    }
                    while (_NextEventIndex != loopStartIndex);
                }

                return true;
            }

            /************************************************************************************************************************/

            private bool NextEvent(int playDirectionInt)
            {
                if (_NextEventIndex == RecalculateEventIndex)
                    return false;

                if (_Events.Version != _SequenceVersion)
                    throw new InvalidOperationException(SequenceVersionException);

                _NextEventIndex += playDirectionInt;

                return true;
            }

            /************************************************************************************************************************/

            private bool NextEventLooped(int playDirectionInt)
            {
                if (!NextEvent(playDirectionInt))
                    return false;

                var count = _Events.Count;
                if (_NextEventIndex >= count)
                    _NextEventIndex = 0;
                else if (_NextEventIndex < 0)
                    _NextEventIndex = count - 1;

                return true;
            }

            /************************************************************************************************************************/

            void IUpdatable.OnDestroy() => Release();

            /************************************************************************************************************************/

            /// <summary>Returns "EventRunner (Target State)".</summary>
            public override string ToString()
            {
                return _State != null ?
                    $"{nameof(EventRunner)} ({_State})" :
                    $"{nameof(EventRunner)} (No Target State)";
            }

            /************************************************************************************************************************/
        }

        /************************************************************************************************************************/
    }
}


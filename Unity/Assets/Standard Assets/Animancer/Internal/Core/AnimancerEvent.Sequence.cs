// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Animancer
{
    partial struct AnimancerEvent
    {
        /// <summary>
        /// A variable-size list of <see cref="AnimancerEvent"/>s which keeps itself sorted according to their
        /// <see cref="normalizedTime"/>.
        /// </summary>
        /// <remarks>
        /// <em>Animancer Lite does not allow events (except for <see cref="OnEnd"/>) in runtime builds.</em>
        /// <para></para>
        /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/events/animancer">Animancer Events</see>
        /// </remarks>
        /// https://kybernetik.com.au/animancer/api/Animancer/Sequence
        /// 
        public sealed partial class Sequence : IEnumerable<AnimancerEvent>
        {
            /************************************************************************************************************************/
            #region Fields and Properties
            /************************************************************************************************************************/

            internal const string
                IndexOutOfRangeError = "index must be within the range of 0 <= index < " + nameof(Count);

            /************************************************************************************************************************/

            /// <summary>
            /// A zero length array of <see cref="AnimancerEvent"/>s which is used by all sequence before any elements
            /// are added to them (unless their <see cref="Capacity"/> is set manually).
            /// </summary>
            public static readonly AnimancerEvent[] NoEvents = new AnimancerEvent[0];

            /// <summary>All of the <see cref="AnimancerEvent"/>s in this sequence (excluding the <see cref="endEvent"/>).</summary>
            private AnimancerEvent[] _Events;

            /************************************************************************************************************************/

            /// <summary>[Pro-Only] The number of events in this sequence (excluding the <see cref="endEvent"/>).</summary>
            public int Count { get; private set; }

            /************************************************************************************************************************/

            /// <summary>Indicates whether the sequence has any events in it (including the <see cref="endEvent"/>).</summary>
            public bool IsEmpty
            {
                get
                {
                    return
                        endEvent.callback == null &&
                        float.IsNaN(endEvent.normalizedTime) &&
                        Count == 0;
                }
            }

            /************************************************************************************************************************/

            /// <summary>The initial <see cref="Capacity"/> that will be used if another value is not specified.</summary>
            public const int DefaultCapacity = 8;

            /// <summary>[Pro-Only]
            /// The size of the internal array used to hold events.
            /// <para></para>
            /// When set, the array is reallocated to the given size.
            /// <para></para>
            /// By default, the <see cref="Capacity"/> starts at 0 and increases to the <see cref="DefaultCapacity"/>
            /// when the first event is added.
            /// </summary>
            public int Capacity
            {
                get => _Events.Length;
                set
                {
                    if (value < Count)
                        throw new ArgumentOutOfRangeException(nameof(value),
                            $"{nameof(Capacity)} cannot be set lower than {nameof(Count)}");

                    if (value == _Events.Length)
                        return;

                    if (value > 0)
                    {
                        var newEvents = new AnimancerEvent[value];
                        if (Count > 0)
                            Array.Copy(_Events, 0, newEvents, 0, Count);
                        _Events = newEvents;
                    }
                    else
                    {
                        _Events = NoEvents;
                    }
                }
            }

            /************************************************************************************************************************/

            /// <summary>[Pro-Only]
            /// The number of times the contents of this sequence have been modified. This applies to general events,
            /// but not the <see cref="endEvent"/>.
            /// </summary>
            public int Version { get; private set; }

            /************************************************************************************************************************/
            #region End Event
            /************************************************************************************************************************/

            /// <summary>
            /// A <see cref="callback "/> that will be triggered every frame after the <see cref="normalizedTime"/> has
            /// passed. If you want it to only get triggered once, you can either have the event clear itself or just
            /// use a regular event instead.
            /// <para></para>
            /// Interrupting the animation does not trigger this event.
            /// <para></para>
            /// By default, the <see cref="normalizedTime"/> will be <see cref="float.NaN"/> so that it can choose the
            /// correct value based on the current play direction: forwards ends at 1 and backwards ends at 0.
            /// <para></para>
            /// <em>Animancer Lite does not allow the <see cref="normalizedTime"/> to be changed in Runtime Builds.</em>
            /// </summary>
            ///
            /// <example><code>
            /// void PlayAnimation(AnimancerComponent animancer, AnimationClip clip)
            /// {
            ///     var state = animancer.Play(clip);
            ///     state.Events.NormalizedEndTime = 0.75f;
            ///     state.Events.OnEnd = OnAnimationEnd;
            ///
            ///     // Or set the time and callback at the same time:
            ///     state.Events.endEvent = new AnimancerEvent(0.75f, OnAnimationEnd);
            /// }
            ///
            /// void OnAnimationEnd()
            /// {
            ///     Debug.Log("Animation ended");
            /// }
            /// </code></example>
            ///
            /// <remarks>
            /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/events/end">End Events</see>
            /// </remarks>
            /// 
            /// <seealso cref="OnEnd"/>
            /// <seealso cref="NormalizedEndTime"/>
            public AnimancerEvent endEvent = new AnimancerEvent(float.NaN, null);

            /************************************************************************************************************************/

            /// <summary>Shorthand for the <c>endEvent.callback</c>.</summary>
            /// <seealso cref="endEvent"/>
            /// <seealso cref="NormalizedEndTime"/>
            public ref Action OnEnd => ref endEvent.callback;

            /************************************************************************************************************************/

            /// <summary>[Pro-Only] Shorthand for <c>endEvent.normalizedTime</c>.</summary>
            /// <remarks>
            /// This value is <see cref="float.NaN"/> by default so that the actual time can be determined based on the
            /// <see cref="AnimancerNode.EffectiveSpeed"/>: positive speed ends at 1 and negative speed ends at 0.
            /// <para></para>
            /// Use <see cref="AnimancerState.NormalizedEndTime"/> to access that value.
            /// </remarks>
            /// <seealso cref="endEvent"/>
            /// <seealso cref="OnEnd"/>
            public ref float NormalizedEndTime => ref endEvent.normalizedTime;

            /************************************************************************************************************************/

            /// <summary>
            /// The default <see cref="AnimancerState.NormalizedTime"/> for an animation to start at when playing
            /// forwards is 0 (the start of the animation) and when playing backwards is 1 (the end of the animation).
            /// <para></para>
            /// `speed` 0 or <see cref="float.NaN"/> will also return 0.
            /// </summary>
            /// <remarks>
            /// This method has nothing to do with events, so it is only here because of
            /// <see cref="GetDefaultNormalizedEndTime"/>.
            /// </remarks>
            public static float GetDefaultNormalizedStartTime(float speed) => speed < 0 ? 1 : 0;

            /// <summary>
            /// The default <see cref="normalizedTime"/> for an <see cref="endEvent"/> when playing forwards is 1 (the
            /// end of the animation) and when playing backwards is 0 (the start of the animation).
            /// <para></para>
            /// `speed` 0 or <see cref="float.NaN"/> will also return 1.
            /// </summary>
            public static float GetDefaultNormalizedEndTime(float speed) => speed < 0 ? 0 : 1;

            /************************************************************************************************************************/
            #endregion
            /************************************************************************************************************************/
            #region Names
            /************************************************************************************************************************/

            private string[] _Names;

            /// <summary>[Pro-Only] The names of the events (excluding the <see cref="endEvent"/>).</summary>
            /// <remarks>This array can be null and <see cref="GetName"/> will return null for any missing elements.</remarks>
            public ref string[] Names => ref _Names;

            /************************************************************************************************************************/

            /// <summary>[Pro-Only]
            /// Returns the name of the event at the specified `index` or null if it is not included in the <see cref="Names"/>.
            /// </summary>
            public string GetName(int index)
            {
                if (_Names == null ||
                    _Names.Length <= index)
                    return null;
                else
                    return _Names[index];
            }

            /************************************************************************************************************************/

            /// <summary>[Pro-Only]
            /// Sets the name of the event at the specified `index`. If the <see cref="Names"/> did not previously
            /// include that `index` it will be resized with a size equal to the <see cref="Count"/>.
            /// </summary>
            public void SetName(int index, string name)
            {
                Debug.Assert((uint)index < (uint)Count, IndexOutOfRangeError);

                if (_Names == null)
                {
                    _Names = new string[Count];
                }
                else if (_Names.Length < index)
                {
                    var names = new string[Count];
                    Array.Copy(_Names, names, _Names.Length);
                }

                _Names[index] = name;
            }

            /************************************************************************************************************************/

            /// <summary>[Pro-Only]
            /// Returns the index of the event with the specified `name` or -1 if there is no such event.
            /// </summary>
            /// <seealso cref="Names"/>
            /// <seealso cref="GetName"/>
            /// <seealso cref="SetName"/>
            /// <seealso cref="IndexOfRequired"/>
            public int IndexOf(string name, int startIndex = 0)
            {
                if (_Names == null)
                    return -1;

                var count = Mathf.Min(Count, _Names.Length);
                for (; startIndex < count; startIndex++)
                    if (_Names[startIndex] == name)
                        return startIndex;

                return -1;
            }

            /// <summary>[Pro-Only]
            /// Returns the index of the event with the specified `name` or throws an <see cref="ArgumentException"/>
            /// if there is no such event.
            /// </summary>
            /// <exception cref="ArgumentException"/>
            /// <seealso cref="IndexOf"/>
            public int IndexOfRequired(string name, int startIndex = 0)
            {
                if (_Names != null)
                {
                    var count = Mathf.Min(Count, _Names.Length);
                    for (; startIndex < count; startIndex++)
                    {
                        if (_Names[startIndex] == name)
                        {
                            return startIndex;
                        }
                    }
                }

                throw new ArgumentException($"No event exists with the name '{name}'.");
            }

            /************************************************************************************************************************/
            #endregion
            /************************************************************************************************************************/
            #endregion
            /************************************************************************************************************************/
            #region Constructors
            /************************************************************************************************************************/

            /// <summary>
            /// Creates a new <see cref="Sequence"/> which starts at 0 <see cref="Capacity"/>.
            /// <para></para>
            /// Adding anything to the sequence will set the <see cref="Capacity"/> = <see cref="DefaultCapacity"/>
            /// and then double it whenever the <see cref="Count"/> would exceed the <see cref="Capacity"/>.
            /// </summary>
            public Sequence()
            {
                _Events = NoEvents;
            }

            /************************************************************************************************************************/

            /// <summary>[Pro-Only]
            /// Creates a new <see cref="Sequence"/> which starts with the specified <see cref="Capacity"/>. It will be
            /// initially empty, but will have room for the given number of elements before any reallocations are
            /// required.
            /// </summary>
            public Sequence(int capacity)
            {
                _Events = capacity > 0 ? new AnimancerEvent[capacity] : NoEvents;
            }

            /************************************************************************************************************************/

            /// <summary>
            /// Creates a new <see cref="Sequence"/> and copies the contents of `copyFrom` into it.
            /// </summary>
            public Sequence(Sequence copyFrom)
            {
                _Events = NoEvents;
                if (copyFrom != null)
                    CopyFrom(copyFrom);
            }

            /************************************************************************************************************************/

            /// <summary>[Pro-Only]
            /// Creates a new <see cref="Sequence"/>, copying and sorting the contents of the `collection` into it.
            /// The <see cref="Count"/> and <see cref="Capacity"/> will be equal to the
            /// <see cref="ICollection{T}.Count"/>.
            /// </summary>
            public Sequence(ICollection<AnimancerEvent> collection)
            {
                if (collection == null)
                    throw new ArgumentNullException(nameof(collection));

                var count = collection.Count;
                if (count == 0)
                {
                    _Events = NoEvents;
                }
                else
                {
                    _Events = new AnimancerEvent[count];
                    AddRange(collection);
                }
            }

            /************************************************************************************************************************/

            /// <summary>[Pro-Only]
            /// Creates a new <see cref="Sequence"/>, copying and sorting the contents of the `enumerable` into it.
            /// </summary>
            public Sequence(IEnumerable<AnimancerEvent> enumerable)
            {
                if (enumerable == null)
                    throw new ArgumentNullException(nameof(enumerable));

                _Events = NoEvents;
                AddRange(enumerable);
            }

            /************************************************************************************************************************/
            #endregion
            /************************************************************************************************************************/
            #region Iteration
            /************************************************************************************************************************/

            /// <summary>[Pro-Only] Returns the event at the specified `index`.</summary>
            public AnimancerEvent this[int index]
            {
                get
                {
                    Debug.Assert((uint)index < (uint)Count, IndexOutOfRangeError);
                    return _Events[index];
                }
            }

            /// <summary>[Pro-Only] Returns the event with the specified `name`.</summary>
            /// <exception cref="ArgumentException">There is no event with the specified `name`.</exception>
            public AnimancerEvent this[string name] => this[IndexOfRequired(name)];

            /************************************************************************************************************************/

            /// <summary>[Assert-Conditional]
            /// Throws an <see cref="ArgumentOutOfRangeException"/> if the <see cref="normalizedTime"/> of any events
            /// is less than 0 or greater than or equal to 1.
            /// <para></para>
            /// This does not include the <see cref="endEvent"/> since it works differently to other events.
            /// </summary>
            [System.Diagnostics.Conditional(Strings.Assertions)]
            public void AssertNormalizedTimes(AnimancerState state)
            {
                if (Count == 0 ||
                    (_Events[0].normalizedTime >= 0 && _Events[Count - 1].normalizedTime < 1))
                    return;

                throw new ArgumentOutOfRangeException(nameof(normalizedTime),
                    "Events on looping animations are triggered every loop and must be" +
                    $" within the range of 0 <= {nameof(normalizedTime)} < 1.\n{state}\n{DeepToString()}");
            }

            /// <summary>[Assert-Conditional]
            /// Calls <see cref="AssertNormalizedTimes(AnimancerState)"/> if `isLooping` is true.
            /// </summary>
            [System.Diagnostics.Conditional(Strings.Assertions)]
            public void AssertNormalizedTimes(AnimancerState state, bool isLooping)
            {
                if (isLooping)
                    AssertNormalizedTimes(state);
            }

            /************************************************************************************************************************/

            /// <summary>Returns a string containing the details of all events in this sequence.</summary>
            public string DeepToString(bool multiLine = true)
            {
                var text = ObjectPool.AcquireStringBuilder()
                    .Append(ToString())
                    .Append(" [")
                    .Append(Count)
                    .Append(multiLine ? "]\n{" : "] { ");

                for (int i = 0; i < Count; i++)
                {
                    if (multiLine)
                        text.Append("\n    ");
                    else if (i > 0)
                        text.Append(", ");

                    text.Append(this[i]);
                }

                text.Append(multiLine ? $"\n}}\n{nameof(endEvent)}=" : $" }} ({nameof(endEvent)}=")
                    .Append(endEvent);

                if (!multiLine)
                    text.Append(")");

                return text.ReleaseToString();
            }

            /************************************************************************************************************************/

            /// <summary>[Pro-Only] Returns an <see cref="Enumerator"/> for this sequence.</summary>
            public Enumerator GetEnumerator() => new Enumerator(this);

            IEnumerator<AnimancerEvent> IEnumerable<AnimancerEvent>.GetEnumerator() => new Enumerator(this);

            IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this);

            /************************************************************************************************************************/

            /// <summary>[Pro-Only]
            /// An iterator that can cycle through every event in a <see cref="Sequence"/> except for the
            /// <see cref="endEvent"/>.
            /// </summary>
            public struct Enumerator : IEnumerator<AnimancerEvent>
            {
                /************************************************************************************************************************/

                /// <summary>The target <see cref="AnimancerEvent.Sequence"/>.</summary>
                public readonly Sequence Sequence;

                private int _Index;
                private int _Version;
                private AnimancerEvent _Current;

                private const string InvalidVersion =
                    nameof(AnimancerEvent) + "." + nameof(AnimancerEvent.Sequence) + " was modified. Enumeration operation may not execute.";

                /************************************************************************************************************************/

                /// <summary>The event this iterator is currently pointing to.</summary>
                public AnimancerEvent Current => _Current;

                /// <summary>The event this iterator is currently pointing to.</summary>
                object IEnumerator.Current
                {
                    get
                    {
                        if (_Index == 0 || _Index == Sequence.Count + 1)
                            throw new InvalidOperationException(
                                "Operation is not valid due to the current state of the object.");

                        return Current;
                    }
                }

                /************************************************************************************************************************/

                /// <summary>Creates a new <see cref="Enumerator"/>.</summary>
                public Enumerator(Sequence sequence)
                {
                    Sequence = sequence;
                    _Index = 0;
                    _Version = sequence.Version;
                    _Current = default;
                }

                /************************************************************************************************************************/

                void IDisposable.Dispose() { }

                /************************************************************************************************************************/

                /// <summary>
                /// Moves to the next event in the <see cref="Sequence"/> and returns true if there is one.
                /// </summary>
                /// <exception cref="InvalidOperationException">
                /// The <see cref="Version"/> has changed since this iterator was created.
                /// </exception>
                public bool MoveNext()
                {
                    if (_Version != Sequence.Version)
                        throw new InvalidOperationException(InvalidVersion);

                    if ((uint)_Index < (uint)Sequence.Count)
                    {
                        _Current = Sequence._Events[_Index];
                        _Index++;
                        return true;
                    }
                    else
                    {
                        _Index = Sequence.Count + 1;
                        _Current = default;
                        return false;
                    }
                }

                /************************************************************************************************************************/

                /// <summary>
                /// Returns this iterator to the start of the <see cref="Sequence"/>.
                /// </summary>
                /// <exception cref="InvalidOperationException">
                /// The <see cref="Version"/> has changed since this iterator was created.
                /// </exception>
                void IEnumerator.Reset()
                {
                    if (_Version != Sequence.Version)
                        throw new InvalidOperationException(InvalidVersion);

                    _Index = 0;
                    _Current = default;
                }

                /************************************************************************************************************************/
            }

            /************************************************************************************************************************/
            #endregion
            /************************************************************************************************************************/
            #region Modification
            /************************************************************************************************************************/

            /// <summary>[Pro-Only]
            /// Adds the given event to this sequence. The <see cref="Count"/> is increased by one and if required, the
            /// <see cref="Capacity"/> is doubled to fit the new event.
            /// </summary>
            /// <remarks>
            /// This methods returns the index at which the event is added, which is determined by its
            /// <see cref="normalizedTime"/> to keep the sequence sorted in ascending order. If there are already any
            /// events with the same <see cref="normalizedTime"/>, the new event is added immediately after them.
            /// </remarks>
            /// <seealso cref="OptionalWarning.DuplicateEvent"/>
            public int Add(AnimancerEvent animancerEvent)
            {
#if UNITY_ASSERTIONS
                if (animancerEvent.callback == null)
                    throw new ArgumentNullException($"{nameof(AnimancerEvent)}.{nameof(callback)}");

#endif
                var index = Insert(animancerEvent.normalizedTime);
                AssertEventUniqueness(index, animancerEvent);
                _Events[index] = animancerEvent;
                return index;
            }

            /// <summary>[Pro-Only]
            /// Adds the given event to this sequence. The <see cref="Count"/> is increased by one and if required, the
            /// <see cref="Capacity"/> is doubled to fit the new event.
            /// </summary>
            /// <remarks>
            /// This methods returns the index at which the event is added, which is determined by its
            /// <see cref="normalizedTime"/> to keep the sequence sorted in ascending order. If there are already any
            /// events with the same <see cref="normalizedTime"/>, the new event is added immediately after them.
            /// </remarks>
            /// <seealso cref="OptionalWarning.DuplicateEvent"/>
            public int Add(float normalizedTime, Action callback)
                => Add(new AnimancerEvent(normalizedTime, callback));

            /// <summary>[Pro-Only]
            /// Adds the given event to this sequence. The <see cref="Count"/> is increased by one and if required, the
            /// <see cref="Capacity"/> is doubled to fit the new event.
            /// </summary>
            /// <remarks>
            /// This methods returns the index at which the event is added, which is determined by its
            /// <see cref="normalizedTime"/> to keep the sequence sorted in ascending order. If there are already any
            /// events with the same <see cref="normalizedTime"/>, the new event is added immediately after them.
            /// </remarks>
            /// <seealso cref="OptionalWarning.DuplicateEvent"/>
            public int Add(int indexHint, AnimancerEvent animancerEvent)
            {
#if UNITY_ASSERTIONS
                if (animancerEvent.callback == null)
                    throw new ArgumentNullException($"{nameof(AnimancerEvent)}.{nameof(callback)}");

#endif
                indexHint = Insert(indexHint, animancerEvent.normalizedTime);
                AssertEventUniqueness(indexHint, animancerEvent);
                _Events[indexHint] = animancerEvent;
                return indexHint;
            }

            /// <summary>[Pro-Only]
            /// Adds the given event to this sequence. The <see cref="Count"/> is increased by one and if required, the
            /// <see cref="Capacity"/> is doubled to fit the new event.
            /// </summary>
            /// <remarks>
            /// This methods returns the index at which the event is added, which is determined by its
            /// <see cref="normalizedTime"/> to keep the sequence sorted in ascending order. If there are already any
            /// events with the same <see cref="normalizedTime"/>, the new event is added immediately after them.
            /// </remarks>
            /// <seealso cref="OptionalWarning.DuplicateEvent"/>
            public int Add(int indexHint, float normalizedTime, Action callback)
                => Add(indexHint, new AnimancerEvent(normalizedTime, callback));

            /************************************************************************************************************************/

            /// <summary>[Pro-Only]
            /// Adds every event in the `enumerable` to this sequence. The <see cref="Count"/> is increased by one and if
            /// required, the <see cref="Capacity"/> is doubled to fit the new event.
            /// </summary>
            /// <seealso cref="OptionalWarning.DuplicateEvent"/>
            public void AddRange(IEnumerable<AnimancerEvent> enumerable)
            {
                foreach (var item in enumerable)
                    Add(item);
            }

            /************************************************************************************************************************/

            /// <summary>[Pro-Only] Adds the specified `callback` to the event at the specified `index`.</summary>
            /// <seealso cref="OptionalWarning.DuplicateEvent"/>
            public void AddCallback(int index, Action callback)
            {
                ref var animancerEvent = ref _Events[index];
                AssertCallbackUniqueness(animancerEvent.callback, callback, $"{nameof(callback)} being added");
                animancerEvent.callback += callback;
                Version++;
            }

            /// <summary>[Pro-Only] Adds the specified `callback` to the event with the specified `name`.</summary>
            /// <exception cref="ArgumentException">There is no event with the specified `name`.</exception>
            /// <seealso cref="IndexOfRequired"/>
            /// <seealso cref="OptionalWarning.DuplicateEvent"/>
            public void AddCallback(string name, Action callback) => AddCallback(IndexOfRequired(name), callback);

            /************************************************************************************************************************/

            /// <summary>[Pro-Only] Removes the specified `callback` from the event at the specified `index`.</summary>
            /// <remarks>
            /// If the <see cref="callback"/> would become null, it is instead set to the <see cref="DummyCallback"/>
            /// since they are not allowed to be null.
            /// </remarks>
            public void RemoveCallback(int index, Action callback)
            {
                ref var animancerEvent = ref _Events[index];
                animancerEvent.callback -= callback;
                if (animancerEvent.callback == null)
                    animancerEvent.callback = DummyCallback;
                Version++;
            }

            /// <summary>[Pro-Only] Removes the specified `callback` from the event with the specified `name`.</summary>
            /// <remarks>
            /// If the <see cref="callback"/> would become null, it is instead set to the <see cref="DummyCallback"/>
            /// since they are not allowed to be null.
            /// </remarks>
            /// <exception cref="ArgumentException">There is no event with the specified `name`.</exception>
            /// <seealso cref="IndexOfRequired"/>
            public void RemoveCallback(string name, Action callback) => RemoveCallback(IndexOfRequired(name), callback);

            /************************************************************************************************************************/

            /// <summary>[Pro-Only] Replaces the <see cref="callback"/> of the event at the specified `index`.</summary>
            /// <seealso cref="OptionalWarning.DuplicateEvent"/>
            public void SetCallback(int index, Action callback)
            {
#if UNITY_ASSERTIONS
                if (callback == null)
                    throw new ArgumentNullException(nameof(callback));

#endif
                ref var animancerEvent = ref _Events[index];
                AssertCallbackUniqueness(animancerEvent.callback, callback, $"{nameof(callback)} being assigned");
                animancerEvent.callback = callback;
                Version++;
            }

            /// <summary>[Pro-Only] Replaces the <see cref="callback"/> of the event with the specified `name`.</summary>
            /// <exception cref="ArgumentException">There is no event with the specified `name`.</exception>
            /// <seealso cref="IndexOfRequired"/>
            /// <seealso cref="OptionalWarning.DuplicateEvent"/>
            public void SetCallback(string name, Action callback) => SetCallback(IndexOfRequired(name), callback);

            /************************************************************************************************************************/

            /// <summary>[Assert-Conditional]
            /// Logs <see cref="OptionalWarning.DuplicateEvent"/> if the `oldCallback` is identical to the
            /// `newCallback` or just has the same <see cref="Delegate.Method"/>.
            /// </summary>
            [System.Diagnostics.Conditional(Strings.Assertions)]
            private static void AssertCallbackUniqueness(Action oldCallback, Action newCallback, string target)
            {
#if UNITY_ASSERTIONS
                if (OptionalWarning.DuplicateEvent.IsDisabled())
                    return;

                if (oldCallback == newCallback)
                {
                    OptionalWarning.DuplicateEvent.Log($"The {target}" +
                        " is identical to an existing event in the sequence" +
                        " which may mean that it is being unintentionally added multiple times.");
                }
                else if (oldCallback?.Method == newCallback?.Method)
                {
                    OptionalWarning.DuplicateEvent.Log($"The {target}" +
                        " is identical to an existing event in the sequence except for the target object." +
                        " This often happens when a Transition is shared by multiple objects," +
                        " in which case it can be avoided by giving each object its own" +
                        $" {nameof(AnimancerEvent)}.{nameof(Sequence)} as explained in the documentation:" +
                        $" {Strings.DocsURLs.SharedEventSequences}");
                }
#endif
            }

            /// <summary>[Assert-Conditional]
            /// Logs <see cref="OptionalWarning.DuplicateEvent"/> if the event at the specified `index` is identical to
            /// the `newEvent`.
            /// </summary>
            [System.Diagnostics.Conditional(Strings.Assertions)]
            private void AssertEventUniqueness(int index, AnimancerEvent newEvent)
            {
#if UNITY_ASSERTIONS
                if (OptionalWarning.DuplicateEvent.IsDisabled() || index == 0)
                    return;

                var previousEvent = _Events[index - 1];
                if (previousEvent.normalizedTime != newEvent.normalizedTime)
                    return;

                AssertCallbackUniqueness(previousEvent.callback, newEvent.callback, $"{nameof(AnimancerEvent)} being added");
#endif
            }

            /************************************************************************************************************************/

            /// <summary>[Pro-Only]
            /// Determines the index where a new event with the specified `normalizedTime` should be added in order to
            /// keep this sequence sorted, increases the <see cref="Count"/> by one, doubles the <see cref="Capacity"/>
            /// if required, moves any existing events to open up the chosen index, and returns that index.
            /// <para></para>
            /// This overload starts searching for the desired index from the end of the sequence, using the assumption
            /// that elements will usually be added in order.
            /// </summary>
            private int Insert(float normalizedTime)
            {
                var index = Count;
                while (index > 0 && _Events[index - 1].normalizedTime > normalizedTime)
                    index--;
                Insert(index);
                return index;
            }

            /// <summary>[Pro-Only]
            /// Determines the index where a new event with the specified `normalizedTime` should be added in order to
            /// keep this sequence sorted, increases the <see cref="Count"/> by one, doubles the <see cref="Capacity"/>
            /// if required, moves any existing events to open up the chosen index, and returns that index.
            /// <para></para>
            /// This overload starts searching for the desired index from the `hint`.
            /// </summary>
            private int Insert(int hint, float normalizedTime)
            {
                if (hint >= Count)
                    return Insert(normalizedTime);

                if (_Events[hint].normalizedTime > normalizedTime)
                {
                    while (hint > 0 && _Events[hint - 1].normalizedTime > normalizedTime)
                        hint--;
                }
                else
                {
                    while (hint < Count && _Events[hint].normalizedTime <= normalizedTime)
                        hint++;
                }

                Insert(hint);
                return hint;
            }

            /************************************************************************************************************************/

            /// <summary>[Pro-Only]
            /// Increases the <see cref="Count"/> by one, doubles the <see cref="Capacity"/> if required, and moves any
            /// existing events to open up the `index`.
            /// </summary>
            private void Insert(int index)
            {
                Debug.Assert((uint)index <= (uint)Count, IndexOutOfRangeError);

                var capacity = _Events.Length;
                if (Count == capacity)
                {
                    if (capacity == 0)
                    {
                        _Events = new AnimancerEvent[DefaultCapacity];
                    }
                    else
                    {
                        capacity *= 2;
                        if (capacity < DefaultCapacity)
                            capacity = DefaultCapacity;

                        var newEvents = new AnimancerEvent[capacity];

                        Array.Copy(_Events, 0, newEvents, 0, index);
                        if (Count > index)
                            Array.Copy(_Events, index, newEvents, index + 1, Count - index);

                        _Events = newEvents;
                    }
                }
                else if (Count > index)
                {
                    Array.Copy(_Events, index, _Events, index + 1, Count - index);
                }

                Count++;
                Version++;
            }

            /************************************************************************************************************************/

            /// <summary>[Pro-Only]
            /// Removes the event at the specified `index` from this sequence by decrementing the <see cref="Count"/>
            /// and copying all events after the removed one down one place.
            /// </summary>
            public void Remove(int index)
            {
                Debug.Assert((uint)index < (uint)Count, IndexOutOfRangeError);
                Count--;
                if (index + 1 < Count)
                    Array.Copy(_Events, index + 1, _Events, index, Count - index - 1);
                _Events[Count] = default;
                Version++;
            }

            /// <summary>[Pro-Only]
            /// Removes the event with the specified `name` from this sequence by decrementing the <see cref="Count"/>
            /// and copying all events after the removed one down one place. Returns true if the event was found and
            /// removed.
            /// </summary>
            public bool Remove(string name)
            {
                var index = IndexOf(name);
                if (index >= 0)
                {
                    Remove(index);
                    return true;
                }
                else return false;
            }

            /// <summary>[Pro-Only]
            /// Removes the `animancerEvent` from this sequence by decrementing the <see cref="Count"/> and copying all
            /// events after the removed one down one place. Returns true if the event was found and removed.
            /// </summary>
            public bool Remove(AnimancerEvent animancerEvent)
            {
                var count = Count;
                for (int i = 0; i < count; i++)
                {
                    if (_Events[i] == animancerEvent)
                    {
                        Remove(i);
                        return true;
                    }
                }

                return false;
            }

            /************************************************************************************************************************/

            /// <summary>[Pro-Only] Removes all events except the <see cref="endEvent"/>.</summary>
            /// <seealso cref="Clear"/>
            public void RemoveAll()
            {
                if (_Names != null)
                    Array.Clear(_Names, 0, _Names.Length);

                Array.Clear(_Events, 0, Count);
                Count = 0;
                Version++;
            }

            /// <summary>Removes all events, including the <see cref="endEvent"/>.</summary>
            /// <seealso cref="RemoveAll"/>
            public void Clear()
            {
                RemoveAll();
                endEvent = new AnimancerEvent(float.NaN, null);
            }

            /************************************************************************************************************************/
            #endregion
            /************************************************************************************************************************/
            #region Copying
            /************************************************************************************************************************/

            /// <summary>
            /// Copies all the events from the `source` to replace the previous contents of this sequence.
            /// </summary>
            public void CopyFrom(Sequence source)
            {
                if (source == null)
                {
                    if (_Names != null)
                        Array.Clear(_Names, 0, _Names.Length);

                    Array.Clear(_Events, 0, Count);
                    Count = 0;
                    Capacity = 0;
                    endEvent = default;
                    return;
                }

                if (source._Names == null)
                {
                    _Names = null;
                }
                else
                {
                    var nameCount = source._Names.Length;
                    if (_Names == null || _Names.Length != nameCount)
                        _Names = new string[nameCount];
                    Array.Copy(source._Names, 0, _Names, 0, nameCount);
                }

                var sourceCount = source.Count;

                if (Count > sourceCount)
                    Array.Clear(_Events, Count, sourceCount - Count);
                else if (_Events.Length < sourceCount)
                    Capacity = sourceCount;

                Count = sourceCount;

                Array.Copy(source._Events, 0, _Events, 0, sourceCount);

                endEvent = source.endEvent;
            }

            /************************************************************************************************************************/

            /// <summary>[<see cref="ICollection{T}"/>]
            /// Copies all the events from this sequence into the `array`, starting at the `index`.
            /// </summary>
            public void CopyTo(AnimancerEvent[] array, int index)
            {
                Array.Copy(_Events, 0, array, index, Count);
            }

            /************************************************************************************************************************/
            #endregion
            /************************************************************************************************************************/
        }
    }
}


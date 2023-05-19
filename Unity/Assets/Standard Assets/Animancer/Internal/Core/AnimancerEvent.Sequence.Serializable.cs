// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

//#define ANIMANCER_ULT_EVENTS

// If you edit this file to change the callback type to something other than UltEvents, you will need to change this
// alias as well as the HasPersistentCalls method below.
#if ANIMANCER_ULT_EVENTS
using SerializableCallback = UltEvents.UltEvent;
#else
using SerializableCallback = UnityEngine.Events.UnityEvent;
#endif

using UnityEngine;
using System;

namespace Animancer
{
    /// https://kybernetik.com.au/animancer/api/Animancer/AnimancerEvent
    /// 
    partial struct AnimancerEvent
    {
        /// https://kybernetik.com.au/animancer/api/Animancer/Sequence
        /// 
        partial class Sequence
        {
            /// <summary>
            /// An <see cref="AnimancerEvent.Sequence"/> that can be serialized and uses
            /// <see cref="SerializableCallback"/>s to define the <see cref="callback"/>s.
            /// </summary>
            /// <remarks>
            /// If you have Animancer Pro you can replace <see cref="SerializableCallback"/>s with
            /// <see href="https://kybernetik.com.au/ultevents">UltEvents</see> using the following procedure:
            /// <list type="number">
            /// <item>Select the <c>Assets/Plugins/Animancer/Animancer.asmdef</c> and add a Reference to the
            /// <c>UltEvents</c> Assembly Definition.</item>
            /// <item>Go into the Player Settings of your project and add <c>ANIMANCER_ULT_EVENTS</c> as a Scripting
            /// Define Symbol. Or you can simply edit this script to change the event type (it is located at
            /// <c>Assets/Plugins/Animancer/Internal/Core/AnimancerEvent.Sequence.Serializable.cs</c> by default.</item>
            /// </list>
            /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/events/animancer">Animancer Events</see>
            /// </remarks>
            /// https://kybernetik.com.au/animancer/api/Animancer/Serializable
            /// 
            [Serializable]
            public sealed class Serializable
#if UNITY_EDITOR
                : ISerializationCallbackReceiver
#endif
            {
                /************************************************************************************************************************/

                /// <summary>The serialized <see cref="Names"/>.</summary>
                [SerializeField]
                private string[] _Names;

                /************************************************************************************************************************/

                /// <summary>The serialized <see cref="normalizedTime"/>s.</summary>
                [SerializeField]
                private float[] _NormalizedTimes;

                /************************************************************************************************************************/

                /// <summary>The serialized <see cref="callback"/>s.</summary>
                /// <remarks>
                /// This array only needs to be large enough to hold the last event that actually contains any calls.
                /// Any empty or missing elements will simply use the <see cref="DummyCallback"/> at runtime.
                /// </remarks>
                [SerializeField]
                private SerializableCallback[] _Callbacks;

                /************************************************************************************************************************/
#if UNITY_EDITOR
                /************************************************************************************************************************/

                /// <summary>[Editor-Only, Internal] The name of the array field which stores the serialized <see cref="Names"/>.</summary>
                internal const string NamesField = nameof(_Names);

                /// <summary>[Editor-Only, Internal] The name of the array field which stores the <see cref="normalizedTime"/>s.</summary>
                internal const string NormalizedTimesField = nameof(_NormalizedTimes);

                /// <summary>[Editor-Only, Internal] The name of the array field which stores the serialized <see cref="callback"/>s.</summary>
                internal const string CallbacksField = nameof(_Callbacks);

                /************************************************************************************************************************/
#endif
                /************************************************************************************************************************/

                private Sequence _Sequence;

                /// <summary>
                /// The runtime <see cref="AnimancerEvent.Sequence"/> compiled from this <see cref="Serializable"/>.
                /// Each call after the first will return the same value.
                /// </summary>
                /// <remarks>
                /// Unlike <see cref="GetSequenceOptional"/>, this method will create an empty
                /// <see cref="AnimancerEvent.Sequence"/> instead of returning null.
                /// </remarks>
                public Sequence Sequence
                {
                    get
                    {
                        if (_Sequence == null)
                        {
                            GetSequenceOptional();

                            AnimancerUtilities.NewIfNull(ref _Sequence);
                        }
                        return _Sequence;
                    }
                    set => _Sequence = value;
                }

                /************************************************************************************************************************/

                /// <summary>
                /// Returns the runtime <see cref="AnimancerEvent.Sequence"/> compiled from this
                /// <see cref="Serializable"/>. Each call after the first will return the same value.
                /// </summary>
                /// <remarks>
                /// This method returns null if the sequence would be empty anyway and is used by the implicit
                /// conversion from <see cref="Serializable"/> to <see cref="AnimancerEvent.Sequence"/>.
                /// </remarks>
                public Sequence GetSequenceOptional()
                {
                    if (_Sequence != null ||
                        _NormalizedTimes == null)
                        return _Sequence;

                    var timeCount = _NormalizedTimes.Length;
                    if (timeCount == 0)
                        return null;

                    var callbackCount = _Callbacks.Length;

                    var callback = callbackCount >= timeCount-- ?
                        GetInvoker(_Callbacks[timeCount]) :
                        null;
                    var endEvent = new AnimancerEvent(_NormalizedTimes[timeCount], callback);

                    _Sequence = new Sequence(timeCount)
                    {
                        endEvent = endEvent,
                        Count = timeCount,
                        _Names = _Names,
                    };

                    for (int i = 0; i < timeCount; i++)
                    {
                        callback = i < callbackCount ? GetInvoker(_Callbacks[i]) : DummyCallback;
                        _Sequence._Events[i] = new AnimancerEvent(_NormalizedTimes[i], callback);
                    }

                    return _Sequence;
                }

                /// <summary>Calls <see cref="GetSequenceOptional"/>.</summary>
                public static implicit operator Sequence(Serializable serializable) => serializable?.GetSequenceOptional();

                /************************************************************************************************************************/

                /// <summary>
                /// If the `callback` has any persistent calls, this method returns a delegate to call its
                /// <see cref="SerializableCallback.Invoke"/> method. Otherwise it returns the
                /// <see cref="DummyCallback"/>.
                /// </summary>
                public static Action GetInvoker(SerializableCallback callback)
                    => HasPersistentCalls(callback) ? callback.Invoke : DummyCallback;

                /************************************************************************************************************************/

                /// <summary>
                /// Determines if the `callback` contains any method calls that will be serialized (otherwise the
                /// <see cref="DummyCallback"/> can be used instead of creating a new delegate to invoke the empty
                /// `callback`).
                /// </summary>
                public static bool HasPersistentCalls(SerializableCallback callback)
                {
                    if (callback == null)
                        return false;

                    // UnityEvents do not allow us to check if any dynamic calls are present.
                    // But we are not giving runtime access to the events so it does not really matter.
                    // UltEvents does allow it (via the HasCalls property), but we might as well be consistent.

#if ANIMANCER_ULT_EVENTS
                    var calls = callback.PersistentCallsList;
                    return calls != null && calls.Count > 0;
#else
                    return callback.GetPersistentEventCount() > 0;
#endif
                }

                /// <summary>
                /// Determines if the `callback` contains any method calls that will be serialized (otherwise the
                /// <see cref="DummyCallback"/> can be used instead of creating a new delegate to invoke the empty
                /// `callback`).
                /// </summary>
                /// <remarks>
                /// This method casts the `callback` to <see cref="SerializableCallback"/> so the caller does not need
                /// to know what type is actually being used.
                /// </remarks>
                public static bool HasPersistentCalls(object callback) => HasPersistentCalls((SerializableCallback)callback);

                /************************************************************************************************************************/

                /// <summary>Returns the <see cref="normalizedTime"/> of the <see cref="endEvent"/>.</summary>
                /// <remarks>If the value is not set, the value is determined by <see cref="GetDefaultNormalizedEndTime"/>.</remarks>
                public float GetNormalizedEndTime(float speed = 1)
                {
                    if (_NormalizedTimes == null || _NormalizedTimes.Length == 0)
                        return GetDefaultNormalizedEndTime(speed);
                    else
                        return _NormalizedTimes[_NormalizedTimes.Length - 1];
                }

                /************************************************************************************************************************/

                /// <summary>Sets the <see cref="normalizedTime"/> of the <see cref="endEvent"/>.</summary>
                public void SetNormalizedEndTime(float normalizedTime)
                {
                    if (_NormalizedTimes == null || _NormalizedTimes.Length == 0)
                        _NormalizedTimes = new float[] { normalizedTime };
                    else
                        _NormalizedTimes[_NormalizedTimes.Length - 1] = normalizedTime;
                }

                /************************************************************************************************************************/
#if UNITY_EDITOR
                /************************************************************************************************************************/

                /// <summary>[Editor-Only] Clears the <see cref="Sequence"/> so it can be recreated when necessary.</summary>
                void ISerializationCallbackReceiver.OnAfterDeserialize()
                {
                    _Sequence = null;
                }

                /// <summary>[Editor-Only] Ensures that the events are sorted by time (excluding the end event).</summary>
                void ISerializationCallbackReceiver.OnBeforeSerialize()
                {
                    if (_NormalizedTimes == null ||
                        _NormalizedTimes.Length <= 2)
                        goto Trim;

                    var context = Editor.EventSequenceDrawer.Context.Current;
                    var selectedEvent = context?.Property != null ? context.SelectedEvent : -1;

                    var timeCount = _NormalizedTimes.Length - 1;

                    var previousTime = _NormalizedTimes[0];

                    // Bubble Sort based on the normalized times.
                    for (int i = 1; i < timeCount; i++)
                    {
                        var time = _NormalizedTimes[i];
                        if (time >= previousTime)
                        {
                            previousTime = time;
                            continue;
                        }

                        _NormalizedTimes.Swap(i, i - 1);
                        DynamicSwap(ref _Callbacks, i);
                        DynamicSwap(ref _Names, i);

                        if (selectedEvent == i)
                            selectedEvent = i - 1;
                        else if (selectedEvent == i - 1)
                            selectedEvent = i;

                        if (i == 1)
                        {
                            i = 0;
                            previousTime = float.NegativeInfinity;
                        }
                        else
                        {
                            i -= 2;
                            previousTime = _NormalizedTimes[i];
                        }
                    }

                    // If the current animation is looping, clamp all times within the 0-1 range.
                    var transitionContext = Editor.TransitionDrawer.Context;
                    if (transitionContext != null &&
                        transitionContext.Transition != null &&
                        transitionContext.Transition.IsLooping)
                    {
                        for (int i = _NormalizedTimes.Length - 1; i >= 0; i--)
                        {
                            var time = _NormalizedTimes[i];
                            if (time < 0)
                                _NormalizedTimes[i] = 0;
                            else if (time > AlmostOne)
                                _NormalizedTimes[i] = AlmostOne;
                        }
                    }

                    // If the selected event was moved adjust the selection.
                    if (context?.Property != null && context.SelectedEvent != selectedEvent)
                    {
                        context.SelectedEvent = selectedEvent;
                        Editor.TransitionPreviewWindow.PreviewNormalizedTime = _NormalizedTimes[selectedEvent];
                    }

                    // Remove any empty data from the end of the arrays to reduce the serialized data size.
                    Trim:
                    Trim(ref _Callbacks, (callback) => callback != null && HasPersistentCalls(callback));
                    Trim(ref _Names, (name) => !string.IsNullOrEmpty(name));
                }

                /************************************************************************************************************************/

                /// <summary>
                /// Swaps <c>array[index]</c> with <c>array[index - 1]</c> while accounting for the possibility of the
                /// `index` being beyond the bounds of the `array`.
                /// </summary>
                private static void DynamicSwap<T>(ref T[] array, int index)
                {
                    var count = array != null ? array.Length : 0;

                    if (index == count)
                    {
                        Array.Resize(ref array, ++count);
                    }

                    if (index < count)
                    {
                        array.Swap(index, index - 1);
                    }
                }

                /************************************************************************************************************************/

                /// <summary>Removes unimportant values from the end of the `array`.</summary>
                private static void Trim<T>(ref T[] array, Func<T, bool> isImportant)
                {
                    if (array == null)
                        return;

                    var count = array.Length;

                    while (count >= 1)
                    {
                        var callback = array[count - 1];
                        if (isImportant(callback))
                            break;
                        else
                            count--;
                    }

                    Array.Resize(ref array, count);
                }

                /************************************************************************************************************************/
#endif
                /************************************************************************************************************************/
            }
        }
    }
}

/************************************************************************************************************************/
#if UNITY_EDITOR
/************************************************************************************************************************/

namespace Animancer.Editor
{
    /// <summary>[Editor-Only, Internal]
    /// A serializable container which holds a <see cref="SerializableCallback"/> in a field named "_Callback".
    /// </summary>
    /// <remarks>
    /// <see cref="DummySerializableCallback"/> needs to be in a file with the same name as it (otherwise it can't
    /// draw the callback properly) and this class needs to be in the same file as
    /// <see cref="AnimancerEvent.Sequence.Serializable"/> to use the <see cref="SerializableCallback"/> alias.
    /// </remarks>
    [Serializable]
    internal sealed class SerializableCallbackHolder
    {
#pragma warning disable CS0169 // Field is never used.
        [SerializeField]
        private SerializableCallback _Callback;
#pragma warning restore CS0169 // Field is never used.

        /// <summary>The name of the field which stores the <see cref="SerializableCallback"/>.</summary>
        internal const string CallbackField = nameof(_Callback);
    }
}

/************************************************************************************************************************/
#endif
/************************************************************************************************************************/


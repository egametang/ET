// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

using System;
using UnityEngine;

namespace Animancer.FSM
{
    /// <summary>An object with a <see cref="Priority"/>.</summary>
    /// <remarks>
    /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/fsm#state-selectors">State Selectors</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer.FSM/IPrioritizable
    /// 
    public interface IPrioritizable : IState
    {
        float Priority { get; }
    }

    public partial class StateMachine<TState>
    {
        /// <summary>A prioritised list of potential states for a <see cref="StateMachine{TState}"/> to enter.</summary>
        /// <remarks>
        /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/fsm#state-selectors">State Selectors</see>
        /// </remarks>
        /// https://kybernetik.com.au/animancer/api/Animancer.FSM/StateSelector
        /// 
        public class StateSelector
        {
            /************************************************************************************************************************/

            private float[] _Priorities;

            private TState[] _States;

            /************************************************************************************************************************/

            /// <summary>The <see cref="StateMachine{TState}"/> this selector is controlling.</summary>
            public readonly StateMachine<TState> StateMachine;

            /// <summary>The number of states currently in this selector.</summary>
            public int Count { get; private set; }

            /// <summary>The number of states this selector can hold before its internal arrays need to be expanded.</summary>
            public int Capacity
            {
                get => _Priorities.Length;
                set
                {
                    if (Count == 0)
                    {
                        _Priorities = new float[value];
                        _States = new TState[value];
                    }
                    else
                    {
                        var newPriorities = new float[value];
                        var newStates = new TState[value];

                        var copy = Mathf.Min(Count, value);
                        Array.Copy(_Priorities, newPriorities, copy);
                        Array.Copy(_States, newStates, copy);

                        _Priorities = newPriorities;
                        _States = newStates;
                    }
                }
            }

            /************************************************************************************************************************/

            /// <summary>Creates a new <see cref="StateSelector"/>.</summary>
            public StateSelector(StateMachine<TState> stateMachine, int capacity = 8)
            {
                _Priorities = new float[capacity];
                _States = new TState[capacity];
                StateMachine = stateMachine;
            }

            /************************************************************************************************************************/

            /// <summary>Adds the `state` to this selector with the specified `priority`.</summary>
            /// <remarks>If multiple states have the same priority, the most recently added will be treated as higher.</remarks>
            public void Add(float priority, TState state)
            {
                if (Capacity == Count)
                {
                    var capacity = Capacity;
                    if (capacity < 16)
                        capacity = 16;

                    while (capacity <= Count)
                        capacity *= 2;

                    Capacity = capacity;
                }

                var index = 0;
                for (; index < Count; index++)
                {
                    if (priority < _Priorities[index])
                    {
                        Array.Copy(_Priorities, index, _Priorities, index + 1, Count - index);
                        Array.Copy(_States, index, _States, index + 1, Count - index);
                        break;
                    }
                }

                _Priorities[index] = priority;
                _States[index] = state;

                Count++;
            }

            /// <summary>Adds the `state` to this selector with its <see cref="IPrioritizable.Priority"/>.</summary>
            /// <remarks>If multiple states have the same priority, the most recently added will be treated as higher.</remarks>
            public void Add<TPrioritizable>(TPrioritizable state)
                where TPrioritizable : TState, IPrioritizable
            {
                Add(state.Priority, state);
            }

            /************************************************************************************************************************/

            /// <summary>
            /// Calls <see cref="StateMachine{TState}.CanSetState"/> for each state in this selector in order of
            /// priority (highest first) until one returns true. Otherwise this method returns false.
            /// </summary>
            public bool CanSetState()
            {
                for (int i = Count - 1; i >= 0; i--)
                    if (StateMachine.CanSetState(_States[i]))
                        return true;

                return false;
            }

            /************************************************************************************************************************/

            /// <summary>
            /// Calls <see cref="StateMachine{TState}.TrySetState"/> for each state in this selector in order of
            /// priority (highest first). If one of them returns true, this method calls <see cref="Clear"/> and
            /// returns true. Otherwise it returns false.
            /// </summary>
            public bool TrySetState()
            {
                for (int i = Count - 1; i >= 0; i--)
                {
                    if (StateMachine.TrySetState(_States[i]))
                    {
                        Clear();
                        return true;
                    }
                }

                return false;
            }

            /************************************************************************************************************************/

            /// <summary>
            /// Calls <see cref="StateMachine{TState}.TryResetState"/> for each state in this selector in order of
            /// priority (highest first). If one of them returns true, this method calls <see cref="Clear"/> and
            /// returns true. Otherwise it returns false.
            /// </summary>
            public bool TryResetState()
            {
                for (int i = Count - 1; i >= 0; i--)
                {
                    if (StateMachine.TryResetState(_States[i]))
                    {
                        Clear();
                        return true;
                    }
                }

                return false;
            }

            /************************************************************************************************************************/

            /// <summary>
            /// Calls <see cref="StateMachine{TState}.ForceSetState"/> for the highest priority state in this selector
            /// and returns true. Or does nothing and returns false if there are no states.
            /// </summary>
            public bool ForceSetState()
            {
                if (Count == 0)
                    return false;

                StateMachine.ForceSetState(_States[Count - 1]);
                Clear();
                return true;
            }

            /************************************************************************************************************************/

            /// <summary>Removes all states from this selector.</summary>
            public void Clear()
            {
                Array.Clear(_Priorities, 0, Count);
                Array.Clear(_States, 0, Count);
                Count = 0;
            }

            /************************************************************************************************************************/
        }
    }
}

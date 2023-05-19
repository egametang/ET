// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Animancer.FSM
{
    /// <summary>A simple Finite State Machine system that registers each state with a particular key.</summary>
    /// <remarks>
    /// This class allows states to be registered with a particular key upfront and then accessed later using that key.
    /// See <see cref="StateMachine{TState}"/> for a system that does not bother keeping track of any states other than
    /// the active one.
    /// <para></para>
    /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/fsm">Finite State Machines</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer.FSM/StateMachine_2
    /// 
    [HelpURL(StateExtensions.APIDocumentationURL + nameof(StateMachine<TState>) + "_2")]
    public partial class StateMachine<TKey, TState> : StateMachine<TState>, IDictionary<TKey, TState>
        where TState : class, IState
    {
        /************************************************************************************************************************/

        /// <summary>The collection of states mapped to a particular key.</summary>
        public IDictionary<TKey, TState> Dictionary { get; set; }

        /// <summary>The current state.</summary>
        public TKey CurrentKey { get; private set; }

        /************************************************************************************************************************/

        /// <summary>Returns the <see cref="KeyChange{TKey}.PreviousKey"/>.</summary>
        public TKey PreviousKey => KeyChange<TKey>.PreviousKey;

        /// <summary>Returns the <see cref="KeyChange{TKey}.NextKey"/>.</summary>
        public TKey NextKey => KeyChange<TKey>.NextKey;

        /************************************************************************************************************************/

        /// <summary>
        /// Creates a new <see cref="StateMachine{TKey, TState}"/> with a new <see cref="Dictionary"/>, leaving the
        /// <see cref="CurrentState"/> null.
        /// </summary>
        public StateMachine()
        {
            Dictionary = new Dictionary<TKey, TState>();
        }

        /// <summary>
        /// Creates a new <see cref="StateMachine{TKey, TState}"/> which uses the specified `dictionary`, leaving the
        /// <see cref="CurrentState"/> null.
        /// </summary>
        public StateMachine(IDictionary<TKey, TState> dictionary)
        {
            Dictionary = dictionary;
        }

        /// <summary>
        /// Constructs a new <see cref="StateMachine{TKey, TState}"/> with a new <see cref="Dictionary"/> and
        /// immediately uses the `defaultKey` to enter the `defaultState`.
        /// </summary>
        /// <remarks>This calls <see cref="IState.OnEnterState"/> but not <see cref="IState.CanEnterState"/>.</remarks>
        public StateMachine(TKey defaultKey, TState defaultState)
        {
            Dictionary = new Dictionary<TKey, TState>
            {
                { defaultKey, defaultState }
            };
            ForceSetState(defaultKey, defaultState);
        }

        /// <summary>
        /// Constructs a new <see cref="StateMachine{TKey, TState}"/> which uses the specified `dictionary` and
        /// immediately uses the `defaultKey` to enter the `defaultState`.
        /// </summary>
        /// <remarks>This calls <see cref="IState.OnEnterState"/> but not <see cref="IState.CanEnterState"/>.</remarks>
        public StateMachine(IDictionary<TKey, TState> dictionary, TKey defaultKey, TState defaultState)
        {
            Dictionary = dictionary;
            Dictionary.Add(defaultKey, defaultState);
            ForceSetState(defaultKey, defaultState);
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Attempts to enter the specified `state` and returns true if successful.
        /// <para></para>
        /// This method returns true immediately if the specified `state` is already the
        /// <see cref="StateMachine{TState}.CurrentState"/>. To allow directly re-entering the same state, use
        /// <see cref="TryResetState(TKey, TState)"/> instead.
        /// </summary>
        public bool TrySetState(TKey key, TState state)
        {
            if (CurrentState == state)
                return true;
            else
                return TryResetState(key, state);
        }

        /// <summary>
        /// Attempts to enter the specified state associated with the specified `key` and returns it if successful.
        /// <para></para>
        /// This method returns true immediately if the specified `key` is already the <see cref="CurrentKey"/>. To
        /// allow directly re-entering the same state, use <see cref="TryResetState(TKey)"/> instead.
        /// </summary>
        public TState TrySetState(TKey key)
        {
            if (Equals(CurrentKey, key))
                return CurrentState;
            else
                return TryResetState(key);
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Attempts to enter the specified `state` and returns true if successful.
        /// <para></para>
        /// This method does not check if the `state` is already the <see cref="StateMachine{TState}.CurrentState"/>.
        /// To do so, use <see cref="TrySetState(TKey, TState)"/> instead.
        /// </summary>
        public bool TryResetState(TKey key, TState state)
        {
            KeyChange<TKey>.Begin(CurrentKey, key, out var previouslyActiveChange);
            try
            {
                if (!CanSetState(state))
                    return false;

                ForceSetState(key, state);
                return true;
            }
            finally
            {
                KeyChange<TKey>.End(previouslyActiveChange);
            }
        }

        /// <summary>
        /// Attempts to enter the specified state associated with the specified `key` and returns it if successful.
        /// <para></para>
        /// This method does not check if the `key` is already the <see cref="CurrentKey"/>. To do so, use
        /// <see cref="TrySetState(TKey)"/> instead.
        /// </summary>
        public TState TryResetState(TKey key)
        {
            if (Dictionary.TryGetValue(key, out var state) &&
                TryResetState(key, state))
                return state;
            else
                return null;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Calls <see cref="IState.OnExitState"/> on the current state then changes to the specified key and
        /// state and calls <see cref="IState.OnEnterState"/> on it.
        /// <para></para>
        /// Note that this method does not check <see cref="IState.CanExitState"/> or
        /// <see cref="IState.CanEnterState"/>. To do that, you should use
        /// <see cref="TrySetState(TKey, TState)"/> instead.
        /// </summary>
        public void ForceSetState(TKey key, TState state)
        {
            KeyChange<TKey>.Begin(CurrentKey, key, out var previouslyActiveChange);
            try
            {
                CurrentKey = key;
                ForceSetState(state);
            }
            finally
            {
                KeyChange<TKey>.End(previouslyActiveChange);
            }
        }

        /// <summary>
        /// Uses <see cref="ForceSetState(TKey, TState)"/> to change to the state mapped to the `key`. If nothing is mapped,
        /// it changes to default(TState).
        /// </summary>
        public TState ForceSetState(TKey key)
        {
            Dictionary.TryGetValue(key, out var state);
            ForceSetState(key, state);
            return state;
        }

        /************************************************************************************************************************/
        #region Dictionary Wrappers
        /************************************************************************************************************************/

        /// <summary>Gets or sets a particular state in the <see cref="Dictionary"/>.</summary>
        public TState this[TKey key] { get => Dictionary[key]; set => Dictionary[key] = value; }

        /// <summary>Gets an <see cref="ICollection{T}"/> containing the keys of the <see cref="Dictionary"/>.</summary>
        public ICollection<TKey> Keys => Dictionary.Keys;

        /// <summary>Gets an <see cref="ICollection{T}"/> containing the state of the <see cref="Dictionary"/>.</summary>
        public ICollection<TState> Values => Dictionary.Values;

        /// <summary>Gets the number of states contained in the <see cref="Dictionary"/>.</summary>
        public int Count => Dictionary.Count;

        /// <summary>Indicates whether the <see cref="Dictionary"/> is read-only.</summary>
        public bool IsReadOnly => Dictionary.IsReadOnly;

        /// <summary>Adds a state to the <see cref="Dictionary"/>.</summary>
        public void Add(TKey key, TState state) => Dictionary.Add(key, state);

        /// <summary>Removes a state from the <see cref="Dictionary"/>.</summary>
        public bool Remove(TKey key) => Dictionary.Remove(key);

        /// <summary>Removes all state from the <see cref="Dictionary"/>.</summary>
        public void Clear() => Dictionary.Clear();

        /// <summary>Determines whether the <see cref="Dictionary"/> contains a state with the specified `key`.</summary>
        public bool ContainsKey(TKey key) => Dictionary.ContainsKey(key);

        /// <summary>Gets the state associated with the specified `key` in the <see cref="Dictionary"/>.</summary>
        public bool TryGetValue(TKey key, out TState state) => Dictionary.TryGetValue(key, out state);

        /// <summary>Adds a state to the <see cref="Dictionary"/>.</summary>
        public void Add(KeyValuePair<TKey, TState> item) => Dictionary.Add(item);

        /// <summary>Removes a state from the <see cref="Dictionary"/>.</summary>
        public bool Remove(KeyValuePair<TKey, TState> item) => Dictionary.Remove(item);

        /// <summary>Determines whether the <see cref="Dictionary"/> contains a specific value.</summary>
        public bool Contains(KeyValuePair<TKey, TState> item) => Dictionary.Contains(item);

        /// <summary>Returns an enumerator that iterates through the <see cref="Dictionary"/>.</summary>
        public IEnumerator<KeyValuePair<TKey, TState>> GetEnumerator() => Dictionary.GetEnumerator();

        /// <summary>Returns an enumerator that iterates through the <see cref="Dictionary"/>.</summary>
        IEnumerator IEnumerable.GetEnumerator() => Dictionary.GetEnumerator();

        /// <summary>Copies the contents of the <see cref="Dictionary"/> to the `array` starting at the `arrayIndex`.</summary>
        public void CopyTo(KeyValuePair<TKey, TState>[] array, int arrayIndex) => Dictionary.CopyTo(array, arrayIndex);

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/

        /// <summary>
        /// Returns the state associated with the specified `key`, or null if none is present.
        /// </summary>
        public TState GetState(TKey key)
        {
            TryGetValue(key, out var state);
            return state;
        }

        /************************************************************************************************************************/

        /// <summary>Adds the specified `keys` and `states`. Both arrays must be the same size.</summary>
        public void AddRange(TKey[] keys, TState[] states)
        {
            Debug.Assert(keys.Length == states.Length, $"The '{nameof(keys)}' and '{nameof(states)}' arrays must be the same size.");

            for (int i = 0; i < keys.Length; i++)
            {
                Dictionary.Add(keys[i], states[i]);
            }
        }

        /************************************************************************************************************************/

        /// <summary>Sets the <see cref="CurrentKey"/> without actually changing the state.</summary>
        public void SetFakeKey(TKey key) => CurrentKey = key;

        /************************************************************************************************************************/

        /// <summary>
        /// Returns a string describing the type of this state machine and its <see cref="CurrentKey"/> and
        /// <see cref="StateMachine{TState}.CurrentState"/>.
        /// </summary>
        public override string ToString()
            => $"{GetType().FullName} -> {CurrentKey} -> {(CurrentState != null ? CurrentState.ToString() : "null")}";

        /************************************************************************************************************************/
    }
}

// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

//#define ANIMANCER_DONT_VALIDATE_STATE_CHANGES

using System;

namespace Animancer.FSM
{
    /// <summary>A static access point for the details of a state change in a <see cref="StateMachine{TState}"/>.</summary>
    /// <remarks>
    /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/fsm#state-change-details">State Change Details</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer.FSM/StateChange_1
    /// 
    public readonly struct StateChange<TState> where TState : class, IState
    {
        /************************************************************************************************************************/

        [ThreadStatic]
        private static StateChange<TState> _Current;

        private readonly bool _IsActive;
        private readonly TState _PreviousState;
        private readonly TState _NextState;

        /************************************************************************************************************************/

        /// <summary>Is a <see cref="StateChange{TState}"/> of this type currently occurring?</summary>
        public static bool IsActive => _Current._IsActive;

        /************************************************************************************************************************/

        /// <summary>The state currently being changed from.</summary>
        /// <exception cref="InvalidOperationException">[Assert-Only]
        /// <see cref="IsActive"/> is false so this property is likely being accessed on the wrong generic type.
        /// </exception>
        public static TState PreviousState
        {
            get
            {
                if (!IsActive)
                    StateChangeDebug.ThrowInactiveException(typeof(TState));

                return _Current._PreviousState;
            }
        }

        /************************************************************************************************************************/

        /// <summary>The state being changed into.</summary>
        /// <exception cref="InvalidOperationException">[Assert-Only]
        /// <see cref="IsActive"/> is false so this property is likely being accessed on the wrong generic type.
        /// </exception>
        public static TState NextState
        {
            get
            {
                if (!IsActive)
                    StateChangeDebug.ThrowInactiveException(typeof(TState));

                return _Current._NextState;
            }
        }

        /************************************************************************************************************************/

        private StateChange(TState previousState, TState nextState)
        {
            _IsActive = true;
            _PreviousState = previousState;
            _NextState = nextState;
        }

        /************************************************************************************************************************/

        internal static void Begin(TState previousState, TState nextState, out StateChange<TState> previouslyActiveChange)
        {
            previouslyActiveChange = _Current;// In case of recursive calls.
            _Current = new StateChange<TState>(previousState, nextState);

            StateChangeDebug.AddActiveChange(typeof(TState));
        }

        /************************************************************************************************************************/

        internal static void End(in StateChange<TState> previouslyActiveChange)
        {
            StateChangeDebug.RemoveActiveChange();

            // Usually this will be returning to default values (nulls), but if one state change causes another then
            // this will return to the first one after the second ends.
            _Current = previouslyActiveChange;
        }

        /************************************************************************************************************************/

        /// <summary>Returns a string describing the contents of this <see cref="StateChange{TState}"/>.</summary>
        public override string ToString() => IsActive ?
            $"{nameof(StateChange<TState>)}<{typeof(TState).FullName}" +
            $">({nameof(PreviousState)}={PreviousState}" +
            $", {nameof(NextState)}={NextState})" :
            $"{nameof(StateChange<TState>)}<{typeof(TState).FullName}(Not Currently Active)";

        /// <summary>Returns a string describing the contents of the current <see cref="StateChange{TState}"/>.</summary>
        public static string CurrentToString() => _Current.ToString();

        /************************************************************************************************************************/
    }

    /// <summary>[Assert-Only] [Internal]
    /// A system that keeps track of the active <see cref="StateChange{TState}"/>s to help with debugging.
    /// </summary>
    internal static class StateChangeDebug
    {
        /************************************************************************************************************************/

        /// <summary>The types currently being used in a <see cref="StateChange{TState}"/>.</summary>
        private static readonly System.Collections.Generic.List<Type>
            ActiveChanges = new System.Collections.Generic.List<Type>();

        /************************************************************************************************************************/

        /// <summary>Adds the `type` to the list of active changes.</summary>
        internal static void AddActiveChange(Type type) => ActiveChanges.Add(type);

        /// <summary>Removes the last active change added by <see cref="AddActiveChange"/>.</summary>
        internal static void RemoveActiveChange() => ActiveChanges.RemoveAt(ActiveChanges.Count - 1);

        /************************************************************************************************************************/

        /// <summary>
        /// Throws an <see cref="InvalidOperationException"/> explaining that the `type` is not currently being used in
        /// a <see cref="StateChange{TState}"/> and listing the types that are currently being used.
        /// </summary>
        public static void ThrowInactiveException(Type type)
        {
            var error = BuildErrorMessage(nameof(StateChange<IState>), type, ActiveChanges);
            throw new InvalidOperationException(error);
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Returns an error message explaining that the target change is not currently active and listing any others
        /// that are active.
        /// </summary>
        public static string BuildErrorMessage(string changeName, Type targetType, System.Collections.Generic.List<Type> types)
        {
            if (types.Count == 0)
                return $"No {changeName} is currently active.";

            var text = new System.Text.StringBuilder()
                .Append(changeName)
                .Append('<')
                .Append(targetType.FullName)
                .Append("> is not currently active, but ");

            if (types.Count == 1)
            {
                text.Append("1 other is active:");
            }
            else
            {
                text.Append(types.Count)
                    .Append(" others are active:");
            }

            for (int i = 0; i < types.Count; i++)
            {
                text.AppendLine()
                    .Append(" - ")
                    .Append(types[i].FullName);
            }

            return text.ToString();
        }

        /************************************************************************************************************************/
    }
}

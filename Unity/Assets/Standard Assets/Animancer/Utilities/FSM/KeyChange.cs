// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

//#define ANIMANCER_DONT_VALIDATE_STATE_CHANGES

using System;

namespace Animancer.FSM
{
    /// <summary>A static access point for the details of a key change in a <see cref="StateMachine{TKey, TState}"/>.</summary>
    /// <remarks>
    /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/fsm#state-change-details">State Change Details</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer.FSM/KeyChange_1
    /// 
    public readonly struct KeyChange<TKey>
    {
        /************************************************************************************************************************/

        [ThreadStatic]
        private static KeyChange<TKey> _Current;

        private readonly bool _IsActive;
        private readonly TKey _PreviousKey;
        private readonly TKey _NextKey;

        /************************************************************************************************************************/

        /// <summary>Is a <see cref="KeyChange{TKey}"/> of this type currently occurring?</summary>
        public static bool IsActive => _Current._IsActive;

        /************************************************************************************************************************/

        /// <summary>The key being changed from.</summary>
        /// <exception cref="InvalidOperationException">[Assert-Only]
        /// <see cref="IsActive"/> is false so this property is likely being accessed on the wrong generic type.
        /// </exception>
        public static TKey PreviousKey
        {
            get
            {
                if (!IsActive)
                    KeyChangeDebug.ThrowInactiveException(typeof(TKey));

                return _Current._PreviousKey;
            }
        }

        /************************************************************************************************************************/

        /// <summary>The key being changed into.</summary>
        /// <exception cref="InvalidOperationException">[Assert-Only]
        /// <see cref="IsActive"/> is false so this property is likely being accessed on the wrong generic type.
        /// </exception>
        public static TKey NextKey
        {
            get
            {
                if (!IsActive)
                    KeyChangeDebug.ThrowInactiveException(typeof(TKey));

                return _Current._NextKey;
            }
        }

        /************************************************************************************************************************/

        private KeyChange(TKey previousKey, TKey nextKey)
        {
            _IsActive = true;
            _PreviousKey = previousKey;
            _NextKey = nextKey;
        }

        /************************************************************************************************************************/

        internal static void Begin(TKey previousKey, TKey nextKey, out KeyChange<TKey> previouslyActiveChange)
        {
            previouslyActiveChange = _Current;// In case of recursive calls.
            _Current = new KeyChange<TKey>(previousKey, nextKey);

            KeyChangeDebug.AddActiveChange(typeof(TKey));
        }

        /************************************************************************************************************************/

        internal static void End(in KeyChange<TKey> previouslyActiveChange)
        {
            KeyChangeDebug.RemoveActiveChange();

            // Usually this will be returning to default values (nulls), but if one state change causes another then
            // this will return to the first one after the second ends.
            _Current = previouslyActiveChange;
        }

        /************************************************************************************************************************/

        /// <summary>Returns a string describing the contents of this <see cref="KeyChange{TKey}"/>.</summary>
        public override string ToString() => IsActive ?
            $"{nameof(KeyChange<TKey>)}<{typeof(TKey).FullName}" +
            $">({nameof(PreviousKey)}={PreviousKey}" +
            $", {nameof(NextKey)}={NextKey})" :
            $"{nameof(KeyChange<TKey>)}<{typeof(TKey).FullName}(Not Currently Active)";

        /// <summary>Returns a string describing the contents of the current <see cref="KeyChange{TKey}"/>.</summary>
        public static string CurrentToString() => _Current.ToString();

        /************************************************************************************************************************/
    }

    /// <summary>[Assert-Only] [Internal]
    /// A system that keeps track of the active <see cref="KeyChange{TKey}"/>s to help with debugging.
    /// </summary>
    internal static class KeyChangeDebug
    {
        /************************************************************************************************************************/

        /// <summary>The types currently being used in a <see cref="KeyChange{TKey}"/>.</summary>
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
        /// a <see cref="KeyChange{TKey}"/> and listing the types that are currently being used.
        /// </summary>
        public static void ThrowInactiveException(Type type)
        {
            var error = StateChangeDebug.BuildErrorMessage(nameof(KeyChange<object>), type, ActiveChanges);
            throw new InvalidOperationException(error);
        }

        /************************************************************************************************************************/
    }
}

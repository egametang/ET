// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

namespace Animancer.FSM
{
    public partial class StateMachine<TKey, TState>
    {
        /// <summary>
        /// A simple buffer that remembers any failed calls to
        /// <see cref="StateMachine{TKey, TState}.TrySetState(TKey, TState)"/> so that it can retry them each time you
        /// <see cref="Update"/> it until the <see cref="TimeOut"/> expires.
        /// </summary>
        /// <remarks>
        /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/fsm#input-buffers">Input Buffers</see>
        /// </remarks>
        /// https://kybernetik.com.au/animancer/api/Animancer.FSM/InputBuffer
        /// 
        public new class InputBuffer : StateMachine<TState>.InputBuffer
        {
            /************************************************************************************************************************/

            /// <summary>The <see cref="StateMachine{TKey, TState}"/> this buffer is feeding input to.</summary>
            public new readonly StateMachine<TKey, TState> StateMachine;

            /// <summary>The <typeparamref name="TKey"/> of the state this buffer is currently attempting to enter.</summary>
            public TKey BufferedKey { get; set; }

            /************************************************************************************************************************/

            /// <summary>Creates a new <see cref="InputBuffer"/> targeting the specified `stateMachine`.</summary>
            public InputBuffer(StateMachine<TKey, TState> stateMachine)
                : base(stateMachine)
            {
                StateMachine = stateMachine;
                BufferedKey = stateMachine.CurrentKey;
            }

            /************************************************************************************************************************/

            /// <summary>
            /// Attempts to enter the specified state and returns true if successful.
            /// Otherwise the state is remembered and attempted again every time
            /// <see cref="StateMachine{TState}.InputBuffer.Update"/> is called.
            /// </summary>
            public bool TrySetState(TKey key, TState state, float timeOut)
            {
                BufferedKey = key;
                return TrySetState(state, timeOut);
            }

            /// <summary>
            /// Attempts to enter the specified state and returns true if successful.
            /// Otherwise the state is remembered and attempted again every time
            /// <see cref="StateMachine{TState}.InputBuffer.Update"/> is called.
            /// </summary>
            public bool TrySetState(TKey key, float timeOut)
                => StateMachine.TryGetValue(key, out var state) && TrySetState(key, state, timeOut);

            /************************************************************************************************************************/

            /// <summary>
            /// Attempts to enter the <see cref="BufferedState"/> and returns true if successful.
            /// </summary>
            protected override bool TryEnterBufferedState()
                => StateMachine.TrySetState(BufferedKey, BufferedState);

            /************************************************************************************************************************/
        }
    }
}

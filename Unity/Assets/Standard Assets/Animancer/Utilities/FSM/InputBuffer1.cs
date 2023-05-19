// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

using UnityEngine;

namespace Animancer.FSM
{
    public partial class StateMachine<TState>
    {
        /// <summary>
        /// A simple buffer that remembers any failed calls to <see cref="StateMachine{TState}.TrySetState(TState)"/>
        /// so that it can retry them each time you <see cref="Update"/> it until the <see cref="TimeOut"/> expires.
        /// </summary>
        /// <remarks>
        /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/fsm#input-buffers">Input Buffers</see>
        /// </remarks>
        /// https://kybernetik.com.au/animancer/api/Animancer.FSM/InputBuffer
        /// 
        public class InputBuffer
        {
            /************************************************************************************************************************/

            /// <summary>The <see cref="StateMachine{TState}"/> this buffer is feeding input to.</summary>
            public readonly StateMachine<TState> StateMachine;

            /// <summary>The <typeparamref name="TState"/> this buffer is currently attempting to enter.</summary>
            public TState BufferedState { get; set; }

            /// <summary>The amount of time left before the <see cref="BufferedState"/> is cleared.</summary>
            public float TimeOut { get; set; }

            /// <summary>
            /// If true, the <see cref="TimeOut"/> will be updated using <see cref="Time.unscaledDeltaTime"/>.
            /// Otherwise it will use <see cref="Time.deltaTime"/>.
            /// </summary>
            public bool UseUnscaledTime { get; set; }

            /************************************************************************************************************************/

            /// <summary>Indicates whether there is currently a <see cref="BufferedState"/>.</summary>
            public bool IsBufferActive => BufferedState != null;

            /************************************************************************************************************************/

            /// <summary>Creates a new <see cref="InputBuffer"/> targeting the specified `stateMachine`.</summary>
            public InputBuffer(StateMachine<TState> stateMachine) => StateMachine = stateMachine;

            /************************************************************************************************************************/

            /// <summary>
            /// Attempts to enter the specified state and returns true if successful.
            /// Otherwise the state is remembered and attempted again every time <see cref="Update"/> is called.
            /// </summary>
            public bool TrySetState(TState state, float timeOut)
            {
                BufferedState = state;
                TimeOut = timeOut;

                // If you can enter that state immediately, clear the buffer.
                if (TryEnterBufferedState())
                {
                    Clear();
                    return true;
                }
                else
                {
                    return false;
                }
            }

            /************************************************************************************************************************/

            /// <summary>
            /// Attempts to enter the <see cref="BufferedState"/> and returns true if successful.
            /// </summary>
            protected virtual bool TryEnterBufferedState() => StateMachine.TryResetState(BufferedState);

            /************************************************************************************************************************/

            /// <summary>
            /// Attempts to enter the <see cref="BufferedState"/> if there is one and returns true if successful.
            /// Otherwise the <see cref="TimeOut"/> is decreased by <see cref="Time.deltaTime"/> and the
            /// <see cref="BufferedState"/> is forgotten after it reaches 0.
            /// <para></para>
            /// If <see cref="UseUnscaledTime"/> is true, it will use <see cref="Time.unscaledDeltaTime"/> instead.
            /// </summary>
            public bool Update() => Update(UseUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime);

            /// <summary>
            /// Attempts to enter the <see cref="BufferedState"/> if there is one and returns true if successful.
            /// Otherwise the <see cref="TimeOut"/> is decreased by `deltaTime` and the <see cref="BufferedState"/> is
            /// forgotten after it reaches 0.
            /// </summary>
            public bool Update(float deltaTime)
            {
                if (IsBufferActive)
                {
                    if (TryEnterBufferedState())
                    {
                        Clear();
                        return true;
                    }
                    else
                    {
                        TimeOut -= deltaTime;

                        if (TimeOut <= 0)
                            Clear();
                    }
                }

                return false;
            }

            /************************************************************************************************************************/

            /// <summary>
            /// Attempts to enter the <see cref="BufferedState"/> if there is one and returns true if successful.
            /// <para></para>
            /// Unlike <see cref="Update"/>, this method doesn't update the <see cref="TimeOut"/>.
            /// </summary>
            public bool CheckBuffer()
            {
                if (IsBufferActive && TryEnterBufferedState())
                {
                    Clear();
                    return true;
                }

                return false;
            }

            /************************************************************************************************************************/

            /// <summary>Clears the buffer.</summary>
            public void Clear() => BufferedState = null;

            /************************************************************************************************************************/
        }
    }
}

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security;

namespace ET
{
    public struct AsyncETVoidMethodBuilder
    {
        // 1. Static Create method.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden]
        public static AsyncETVoidMethodBuilder Create()
        {
            AsyncETVoidMethodBuilder builder = new AsyncETVoidMethodBuilder();
            return builder;
        }

        // 2. TaskLike Task property(void)
        [DebuggerHidden]
        public ETVoid Task => default;

        // 3. SetException
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden]
        public void SetException(Exception exception)
        {
            Log.Error(exception);
        }

        // 4. SetResult
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden]
        public void SetResult()
        {
            // do nothing
        }

        // 5. AwaitOnCompleted
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden]
        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : INotifyCompletion where TStateMachine : IAsyncStateMachine
        {
            awaiter.OnCompleted(stateMachine.MoveNext);
        }

        // 6. AwaitUnsafeOnCompleted
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden]
        [SecuritySafeCritical]
        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : ICriticalNotifyCompletion where TStateMachine : IAsyncStateMachine
        {
            awaiter.UnsafeOnCompleted(stateMachine.MoveNext);
        }

        // 7. Start
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden]
        public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
        {
            stateMachine.MoveNext();
        }

        // 8. SetStateMachine
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden]
        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
        }
    }
}
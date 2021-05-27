using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security;

namespace ET
{
    public struct ETAsyncTaskMethodBuilder
    {
        private ETTask tcs;

        // 1. Static Create method.
        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ETAsyncTaskMethodBuilder Create()
        {
            ETAsyncTaskMethodBuilder builder = new ETAsyncTaskMethodBuilder() { tcs = ETTask.Create(true) };
            return builder;
        }

        // 2. TaskLike Task property.
        [DebuggerHidden]
        public ETTask Task => this.tcs;

        // 3. SetException
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden]
        public void SetException(Exception exception)
        {
            this.tcs.SetException(exception);
        }

        // 4. SetResult
        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetResult()
        {
            this.tcs.SetResult();
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
            awaiter.OnCompleted(stateMachine.MoveNext);
        }

        // 7. Start
        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
        {
            stateMachine.MoveNext();
        }

        // 8. SetStateMachine
        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
        }
    }

    public struct ETAsyncTaskMethodBuilder<T>
    {
        private ETTask<T> tcs;

        // 1. Static Create method.
        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ETAsyncTaskMethodBuilder<T> Create()
        {
            ETAsyncTaskMethodBuilder<T> builder = new ETAsyncTaskMethodBuilder<T>() { tcs = ETTask<T>.Create(true) };
            return builder;
        }

        // 2. TaskLike Task property.
        [DebuggerHidden]
        public ETTask<T> Task => this.tcs;

        // 3. SetException
        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetException(Exception exception)
        {
            this.tcs.SetException(exception);
        }

        // 4. SetResult
        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetResult(T ret)
        {
            this.tcs.SetResult(ret);
        }

        // 5. AwaitOnCompleted
        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
            awaiter.OnCompleted(stateMachine.MoveNext);
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
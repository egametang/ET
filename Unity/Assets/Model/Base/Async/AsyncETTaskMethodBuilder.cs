using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security;

namespace ET
{
    public struct ETAsyncTaskMethodBuilder
    {
        public ETTaskCompletionSource Tcs;

        // 1. Static Create method.
        [DebuggerHidden]
        public static ETAsyncTaskMethodBuilder Create()
        {
            ETAsyncTaskMethodBuilder builder = new ETAsyncTaskMethodBuilder() { Tcs = new ETTaskCompletionSource() };
            return builder;
        }

        // 2. TaskLike Task property.
        [DebuggerHidden]
        public ETTask Task => this.Tcs.Task;

        // 3. SetException
        [DebuggerHidden]
        public void SetException(Exception exception)
        {
            this.Tcs.SetException(exception);
        }

        // 4. SetResult
        [DebuggerHidden]
        public void SetResult()
        {
            this.Tcs.SetResult();
        }

        // 5. AwaitOnCompleted
        [DebuggerHidden]
        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : INotifyCompletion where TStateMachine : IAsyncStateMachine
        {
            awaiter.OnCompleted(stateMachine.MoveNext);
        }

        // 6. AwaitUnsafeOnCompleted
        [DebuggerHidden]
        [SecuritySafeCritical]
        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : ICriticalNotifyCompletion where TStateMachine : IAsyncStateMachine
        {
            awaiter.OnCompleted(stateMachine.MoveNext);
        }

        // 7. Start
        [DebuggerHidden]
        public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
        {
            stateMachine.MoveNext();
        }

        // 8. SetStateMachine
        [DebuggerHidden]
        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
        }
    }

    public struct ETAsyncTaskMethodBuilder<T>
    {
        public ETTaskCompletionSource<T> Tcs;

        // 1. Static Create method.
        [DebuggerHidden]
        public static ETAsyncTaskMethodBuilder<T> Create()
        {
            ETAsyncTaskMethodBuilder<T> builder = new ETAsyncTaskMethodBuilder<T>() { Tcs = new ETTaskCompletionSource<T>() };
            return builder;
        }

        // 2. TaskLike Task property.
        [DebuggerHidden]
        public ETTask<T> Task => this.Tcs.Task;

        // 3. SetException
        [DebuggerHidden]
        public void SetException(Exception exception)
        {
            this.Tcs.SetException(exception);
        }

        // 4. SetResult
        [DebuggerHidden]
        public void SetResult(T ret)
        {
            this.Tcs.SetResult(ret);
        }

        // 5. AwaitOnCompleted
        [DebuggerHidden]
        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : INotifyCompletion where TStateMachine : IAsyncStateMachine
        {
            awaiter.OnCompleted(stateMachine.MoveNext);
        }

        // 6. AwaitUnsafeOnCompleted
        [DebuggerHidden]
        [SecuritySafeCritical]
        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : ICriticalNotifyCompletion where TStateMachine : IAsyncStateMachine
        {
            awaiter.OnCompleted(stateMachine.MoveNext);
        }

        // 7. Start
        [DebuggerHidden]
        public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
        {
            stateMachine.MoveNext();
        }

        // 8. SetStateMachine
        [DebuggerHidden]
        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
        }
    }
}
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security;

namespace ET
{
    public struct ETAsyncTaskMethodBuilder
    {
        private IStateMachineWrap iStateMachineWrap;
        
        private ETTask tcs;

        // 1. Static Create method.
        [DebuggerHidden]
        public static ETAsyncTaskMethodBuilder Create()
        {
            ETAsyncTaskMethodBuilder builder = new() { tcs = ETTask.Create(true) };
            return builder;
        }

        // 2. TaskLike Task property.
        [DebuggerHidden]
        public ETTask Task => this.tcs;

        // 3. SetException
        [DebuggerHidden]
        public void SetException(Exception exception)
        {
            if (this.iStateMachineWrap != null)
            {
                this.iStateMachineWrap.Recycle();
                this.iStateMachineWrap = null;
            }
            this.tcs.SetException(exception);
        }

        // 4. SetResult
        [DebuggerHidden]
        public void SetResult()
        {
            if (this.iStateMachineWrap != null)
            {
                this.iStateMachineWrap.Recycle();
                this.iStateMachineWrap = null;
            }
            this.tcs.SetResult();
        }

        // 5. AwaitOnCompleted
        [DebuggerHidden]
        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : class, IETTask, INotifyCompletion where TStateMachine : IAsyncStateMachine
        {
            this.iStateMachineWrap ??= StateMachineWrap<TStateMachine>.Fetch(ref stateMachine);
            awaiter.OnCompleted(this.iStateMachineWrap.MoveNext);
        }

        // 6. AwaitUnsafeOnCompleted
        [DebuggerHidden]
        [SecuritySafeCritical]
        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : ICriticalNotifyCompletion where TStateMachine : IAsyncStateMachine
        {
            this.iStateMachineWrap ??= StateMachineWrap<TStateMachine>.Fetch(ref stateMachine);
            awaiter.UnsafeOnCompleted(this.iStateMachineWrap.MoveNext);

            if (awaiter is not IETTask task)
            {
                return;
            }

            if (this.tcs.TaskType == TaskType.WithContext)
            {
                task.SetContext(this.tcs.Context);
                return;
            }
            
            this.tcs.Context = task;
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
        private IStateMachineWrap iStateMachineWrap;
        
        private ETTask<T> tcs;

        // 1. Static Create method.
        [DebuggerHidden]
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
        public void SetException(Exception exception)
        {
            if (this.iStateMachineWrap != null)
            {
                this.iStateMachineWrap.Recycle();
                this.iStateMachineWrap = null;
            }
            this.tcs.SetException(exception);
        }

        // 4. SetResult
        [DebuggerHidden]
        public void SetResult(T ret)
        {
            if (this.iStateMachineWrap != null)
            {
                this.iStateMachineWrap.Recycle();
                this.iStateMachineWrap = null;
            }
            this.tcs.SetResult(ret);
        }

        // 5. AwaitOnCompleted
        [DebuggerHidden]
        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : INotifyCompletion where TStateMachine : IAsyncStateMachine
        {
            this.iStateMachineWrap ??= StateMachineWrap<TStateMachine>.Fetch(ref stateMachine);
            awaiter.OnCompleted(this.iStateMachineWrap.MoveNext);
        }

        // 6. AwaitUnsafeOnCompleted
        [DebuggerHidden]
        [SecuritySafeCritical]
        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : ICriticalNotifyCompletion where TStateMachine : IAsyncStateMachine
        {
            this.iStateMachineWrap ??= StateMachineWrap<TStateMachine>.Fetch(ref stateMachine);
            awaiter.UnsafeOnCompleted(this.iStateMachineWrap.MoveNext);
            
            if (awaiter is not IETTask task)
            {
                return;
            }
            
            if (this.tcs.TaskType == TaskType.WithContext)
            {
                task.SetContext(this.tcs.Context);
                return;
            }
            
            this.tcs.Context = task;
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
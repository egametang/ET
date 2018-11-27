using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security;

namespace ETModel
{
    public struct AsyncETTaskMethodBuilder
    {
        private ETTaskCompletionSource tcs;
        private Action moveNext;

        // 1. Static Create method.
        [DebuggerHidden]
        public static AsyncETTaskMethodBuilder Create()
        {
            AsyncETTaskMethodBuilder builder = new AsyncETTaskMethodBuilder();
            return builder;
        }

        // 2. TaskLike Task property.
        [DebuggerHidden]
        public ETTask Task
        {
            get
            {
                if (this.tcs != null)
                {
                    return this.tcs.Task;
                }

                if (moveNext == null)
                {
                    return ETTask.CompletedTask;
                }

                this.tcs = new ETTaskCompletionSource();
                return this.tcs.Task;
            }
        }

        // 3. SetException
        [DebuggerHidden]
        public void SetException(Exception exception)
        {
            if (this.tcs == null)
            {
                this.tcs = new ETTaskCompletionSource();
            }

            if (exception is OperationCanceledException ex)
            {
                this.tcs.TrySetCanceled(ex);
            }
            else
            {
                this.tcs.TrySetException(exception);
            }
        }

        // 4. SetResult
        [DebuggerHidden]
        public void SetResult()
        {
            if (moveNext == null)
            {
            }
            else
            {
                if (this.tcs == null)
                {
                    this.tcs = new ETTaskCompletionSource();
                }

                this.tcs.TrySetResult();
            }
        }

        // 5. AwaitOnCompleted
        [DebuggerHidden]
        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
                where TAwaiter : INotifyCompletion
                where TStateMachine : IAsyncStateMachine
        {
            if (moveNext == null)
            {
                if (this.tcs == null)
                {
                    this.tcs = new ETTaskCompletionSource(); // built future.
                }

                var runner = new MoveNextRunner<TStateMachine>();
                moveNext = runner.Run;
                runner.StateMachine = stateMachine; // set after create delegate.
            }

            awaiter.OnCompleted(moveNext);
        }

        // 6. AwaitUnsafeOnCompleted
        [DebuggerHidden]
        [SecuritySafeCritical]
        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
                where TAwaiter : ICriticalNotifyCompletion
                where TStateMachine : IAsyncStateMachine
        {
            if (moveNext == null)
            {
                if (this.tcs == null)
                {
                    this.tcs = new ETTaskCompletionSource(); // built future.
                }

                var runner = new MoveNextRunner<TStateMachine>();
                moveNext = runner.Run;
                runner.StateMachine = stateMachine; // set after create delegate.
            }

            awaiter.UnsafeOnCompleted(moveNext);
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
        private T result;
        private ETTaskCompletionSource<T> tcs;
        private Action moveNext;

        // 1. Static Create method.
        [DebuggerHidden]
        public static ETAsyncTaskMethodBuilder<T> Create()
        {
            var builder = new ETAsyncTaskMethodBuilder<T>();
            return builder;
        }

        // 2. TaskLike Task property.
        [DebuggerHidden]
        public ETTask<T> Task
        {
            get
            {
                if (this.tcs != null)
                {
                    return new ETTask<T>(this.tcs);
                }

                if (moveNext == null)
                {
                    return new ETTask<T>(result);
                }

                this.tcs = new ETTaskCompletionSource<T>();
                return this.tcs.Task;
            }
        }

        // 3. SetException
        [DebuggerHidden]
        public void SetException(Exception exception)
        {
            if (this.tcs == null)
            {
                this.tcs = new ETTaskCompletionSource<T>();
            }

            if (exception is OperationCanceledException ex)
            {
                this.tcs.TrySetCanceled(ex);
            }
            else
            {
                this.tcs.TrySetException(exception);
            }
        }

        // 4. SetResult
        [DebuggerHidden]
        public void SetResult(T ret)
        {
            if (moveNext == null)
            {
                this.result = ret;
            }
            else
            {
                if (this.tcs == null)
                {
                    this.tcs = new ETTaskCompletionSource<T>();
                }

                this.tcs.TrySetResult(ret);
            }
        }

        // 5. AwaitOnCompleted
        [DebuggerHidden]
        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
                where TAwaiter : INotifyCompletion
                where TStateMachine : IAsyncStateMachine
        {
            if (moveNext == null)
            {
                if (this.tcs == null)
                {
                    this.tcs = new ETTaskCompletionSource<T>(); // built future.
                }

                var runner = new MoveNextRunner<TStateMachine>();
                moveNext = runner.Run;
                runner.StateMachine = stateMachine; // set after create delegate.
            }

            awaiter.OnCompleted(moveNext);
        }

        // 6. AwaitUnsafeOnCompleted
        [DebuggerHidden]
        [SecuritySafeCritical]
        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
                where TAwaiter : ICriticalNotifyCompletion
                where TStateMachine : IAsyncStateMachine
        {
            if (moveNext == null)
            {
                if (this.tcs == null)
                {
                    this.tcs = new ETTaskCompletionSource<T>(); // built future.
                }

                var runner = new MoveNextRunner<TStateMachine>();
                moveNext = runner.Run;
                runner.StateMachine = stateMachine; // set after create delegate.
            }

            awaiter.UnsafeOnCompleted(moveNext);
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
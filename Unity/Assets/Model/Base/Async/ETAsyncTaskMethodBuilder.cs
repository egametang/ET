using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security;

namespace ETModel
{
    public struct ETAsyncTaskMethodBuilder
    {
        private ETTaskCompletionSource promise;
        private Action moveNext;

        // 1. Static Create method.
        [DebuggerHidden]
        public static ETAsyncTaskMethodBuilder Create()
        {
            ETAsyncTaskMethodBuilder builder = new ETAsyncTaskMethodBuilder();
            return builder;
        }

        // 2. TaskLike Task property.
        [DebuggerHidden]
        public ETTask Task
        {
            get
            {
                if (promise != null)
                {
                    return promise.Task;
                }

                if (moveNext == null)
                {
                    return ETTask.CompletedTask;
                }

                this.promise = new ETTaskCompletionSource();
                return this.promise.Task;
            }
        }

        // 3. SetException
        [DebuggerHidden]
        public void SetException(Exception exception)
        {
            if (promise == null)
            {
                promise = new ETTaskCompletionSource();
            }

            if (exception is OperationCanceledException ex)
            {
                promise.TrySetCanceled(ex);
            }
            else
            {
                promise.TrySetException(exception);
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
                if (promise == null)
                {
                    promise = new ETTaskCompletionSource();
                }

                promise.TrySetResult();
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
                if (promise == null)
                {
                    promise = new ETTaskCompletionSource(); // built future.
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
                if (promise == null)
                {
                    promise = new ETTaskCompletionSource(); // built future.
                }

                var runner = new MoveNextRunner<TStateMachine>();
                moveNext = runner.Run;
                runner.StateMachine = stateMachine; // set after create delegate.
            }

            awaiter.UnsafeOnCompleted(moveNext);
        }

        // 7. Start
        [DebuggerHidden]
        public void Start<TStateMachine>(ref TStateMachine stateMachine)
                where TStateMachine : IAsyncStateMachine
        {
            stateMachine.MoveNext();
        }

        // 8. SetStateMachine
        [DebuggerHidden]
        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
        }
    }

    public struct AsyncUniTaskMethodBuilder<T>
    {
        private T result;
        private ETTaskCompletionSource<T> promise;
        private Action moveNext;

        // 1. Static Create method.
        [DebuggerHidden]
        public static AsyncUniTaskMethodBuilder<T> Create()
        {
            var builder = new AsyncUniTaskMethodBuilder<T>();
            return builder;
        }

        // 2. TaskLike Task property.
        [DebuggerHidden]
        public ETTask<T> Task
        {
            get
            {
                if (promise != null)
                {
                    return new ETTask<T>(promise);
                }

                if (moveNext == null)
                {
                    return new ETTask<T>(result);
                }

                this.promise = new ETTaskCompletionSource<T>();
                return new ETTask<T>(this.promise);
            }
        }

        // 3. SetException
        [DebuggerHidden]
        public void SetException(Exception exception)
        {
            if (promise == null)
            {
                promise = new ETTaskCompletionSource<T>();
            }

            if (exception is OperationCanceledException ex)
            {
                promise.TrySetCanceled(ex);
            }
            else
            {
                promise.TrySetException(exception);
            }
        }

        // 4. SetResult
        [DebuggerHidden]
        public void SetResult(T result)
        {
            if (moveNext == null)
            {
                this.result = result;
            }
            else
            {
                if (promise == null)
                {
                    promise = new ETTaskCompletionSource<T>();
                }

                promise.TrySetResult(result);
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
                if (promise == null)
                {
                    promise = new ETTaskCompletionSource<T>(); // built future.
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
                if (promise == null)
                {
                    promise = new ETTaskCompletionSource<T>(); // built future.
                }

                var runner = new MoveNextRunner<TStateMachine>();
                moveNext = runner.Run;
                runner.StateMachine = stateMachine; // set after create delegate.
            }

            awaiter.UnsafeOnCompleted(moveNext);
        }

        // 7. Start
        [DebuggerHidden]
        public void Start<TStateMachine>(ref TStateMachine stateMachine)
                where TStateMachine : IAsyncStateMachine
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
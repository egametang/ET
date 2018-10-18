using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading;

namespace ETModel
{
    internal class ExceptionHolder
    {
        private readonly ExceptionDispatchInfo exception;
        private bool calledGet;

        public ExceptionHolder(ExceptionDispatchInfo exception)
        {
            this.exception = exception;
        }

        public ExceptionDispatchInfo GetException()
        {
            if (calledGet)
            {
                return this.exception;
            }

            this.calledGet = true;
            GC.SuppressFinalize(this);

            return exception;
        }
    }

    public interface IResolvePromise
    {
        bool TrySetResult();
    }

    public interface IResolvePromise<T>
    {
        bool TrySetResult(T value);
    }

    public interface IRejectPromise
    {
        bool TrySetException(Exception e);
    }

    public interface ICancelPromise
    {
        bool TrySetCanceled();
    }

    public interface IPromise<T>: IResolvePromise<T>, IRejectPromise, ICancelPromise
    {
    }

    public interface IPromise: IResolvePromise, IRejectPromise, ICancelPromise
    {
    }

    public class ETTaskCompletionSource: IAwaiter, IPromise
    {
        // State(= AwaiterStatus)
        private const int Pending = 0;
        private const int Succeeded = 1;
        private const int Faulted = 2;
        private const int Canceled = 3;

        private int state;
        private ExceptionHolder exception;
        private object continuation; // action or list

        AwaiterStatus IAwaiter.Status => (AwaiterStatus) state;

        bool IAwaiter.IsCompleted => state != Pending;

        public ETTask Task => new ETTask(this);

        void IAwaiter.GetResult()
        {
            switch (this.state)
            {
                case Succeeded:
                    return;
                case Faulted:
                    this.exception.GetException().Throw();
                    return;
                case Canceled:
                {
                    if (this.exception != null)
                    {
                        this.exception.GetException().Throw(); // guranteed operation canceled exception.
                    }

                    throw new OperationCanceledException();
                }
                default:
                    throw new NotSupportedException("UniTask does not allow call GetResult directly when task not completed. Please use 'await'.");
            }
        }

        void ICriticalNotifyCompletion.UnsafeOnCompleted(Action action)
        {
            if (Interlocked.CompareExchange(ref continuation, action, null) == null)
            {
                if (state != Pending)
                {
                    TryInvokeContinuation();
                }
            }
            else
            {
                object c = continuation;
                if (c is Action action1)
                {
                    var list = new List<Action>();
                    list.Add(action1);
                    list.Add(action);
                    if (Interlocked.CompareExchange(ref continuation, list, action1) == action1)
                    {
                        goto TRYINVOKE;
                    }
                }

                var l = (List<Action>) continuation;
                lock (l)
                {
                    l.Add(action);
                }

                TRYINVOKE:
                if (state != Pending)
                {
                    TryInvokeContinuation();
                }
            }
        }

        private void TryInvokeContinuation()
        {
            object c = Interlocked.Exchange(ref continuation, null);
            if (c == null)
            {
                return;
            }

            if (c is Action action)
            {
                action.Invoke();
            }
            else
            {
                var l = (List<Action>) c;
                int cnt = l.Count;
                for (int i = 0; i < cnt; i++)
                {
                    l[i].Invoke();
                }
            }
        }

        public void SetResult()
        {
            if (this.TrySetResult())
            {
                return;
            }

            throw new InvalidOperationException("TaskT_TransitionToFinal_AlreadyCompleted");
        }

        public void SetException(Exception e)
        {
            if (this.TrySetException(e))
            {
                return;
            }

            throw new InvalidOperationException("TaskT_TransitionToFinal_AlreadyCompleted");
        }

        public bool TrySetResult()
        {
            if (Interlocked.CompareExchange(ref this.state, Succeeded, Pending) != Pending)
            {
                return false;
            }

            this.TryInvokeContinuation();
            return true;

        }

        public bool TrySetException(Exception e)
        {
            if (Interlocked.CompareExchange(ref this.state, Faulted, Pending) != Pending)
            {
                return false;
            }

            this.exception = new ExceptionHolder(ExceptionDispatchInfo.Capture(e));
            this.TryInvokeContinuation();
            return true;

        }

        public bool TrySetCanceled()
        {
            if (Interlocked.CompareExchange(ref this.state, Canceled, Pending) != Pending)
            {
                return false;
            }

            this.TryInvokeContinuation();
            return true;

        }

        public bool TrySetCanceled(OperationCanceledException e)
        {
            if (Interlocked.CompareExchange(ref this.state, Canceled, Pending) != Pending)
            {
                return false;
            }

            this.exception = new ExceptionHolder(ExceptionDispatchInfo.Capture(e));
            this.TryInvokeContinuation();
            return true;

        }

        void INotifyCompletion.OnCompleted(Action action)
        {
            ((ICriticalNotifyCompletion) this).UnsafeOnCompleted(action);
        }
    }

    public class ETTaskCompletionSource<T>: IAwaiter<T>, IPromise<T>
    {
        // State(= AwaiterStatus)
        private const int Pending = 0;
        private const int Succeeded = 1;
        private const int Faulted = 2;
        private const int Canceled = 3;

        private int state;
        private T value;
        private ExceptionHolder exception;
        private object continuation; // action or list

        bool IAwaiter.IsCompleted => state != Pending;

        public ETTask<T> Task => new ETTask<T>(this);
        public ETTask UnitTask => new ETTask(this);

        AwaiterStatus IAwaiter.Status => (AwaiterStatus) state;

        T IAwaiter<T>.GetResult()
        {
            switch (this.state)
            {
                case Succeeded:
                    return this.value;
                case Faulted:
                    this.exception.GetException().Throw();
                    return default;
                case Canceled:
                {
                    if (this.exception != null)
                    {
                        this.exception.GetException().Throw(); // guranteed operation canceled exception.
                    }

                    throw new OperationCanceledException();
                }
                default:
                    throw new NotSupportedException("UniTask does not allow call GetResult directly when task not completed. Please use 'await'.");
            }
        }

        void ICriticalNotifyCompletion.UnsafeOnCompleted(Action action)
        {
            if (Interlocked.CompareExchange(ref continuation, action, null) == null)
            {
                if (state != Pending)
                {
                    TryInvokeContinuation();
                }
            }
            else
            {
                var c = continuation;
                if (c is Action action1)
                {
                    var list = new List<Action>();
                    list.Add(action1);
                    list.Add(action);
                    if (Interlocked.CompareExchange(ref continuation, list, action1) == action1)
                    {
                        goto TRYINVOKE;
                    }
                }

                var l = (List<Action>) continuation;
                lock (l)
                {
                    l.Add(action);
                }

                TRYINVOKE:
                if (state != Pending)
                {
                    TryInvokeContinuation();
                }
            }
        }

        private void TryInvokeContinuation()
        {
            object c = Interlocked.Exchange(ref continuation, null);
            if (c == null)
            {
                return;
            }

            if (c is Action action)
            {
                action.Invoke();
                return;
            }

            var l = (List<Action>) c;
            int cnt = l.Count;
            for (int i = 0; i < cnt; i++)
            {
                l[i].Invoke();
            }
        }

        public void SetResult(T result)
        {
            if (this.TrySetResult(result))
            {
                return;
            }

            throw new InvalidOperationException("TaskT_TransitionToFinal_AlreadyCompleted");
        }

        public void SetException(Exception e)
        {
            if (this.TrySetException(e))
            {
                return;
            }

            throw new InvalidOperationException("TaskT_TransitionToFinal_AlreadyCompleted");
        }

        public bool TrySetResult(T result)
        {
            if (Interlocked.CompareExchange(ref this.state, Succeeded, Pending) != Pending)
            {
                return false;
            }

            this.value = result;
            this.TryInvokeContinuation();
            return true;

        }

        public bool TrySetException(Exception e)
        {
            if (Interlocked.CompareExchange(ref this.state, Faulted, Pending) != Pending)
            {
                return false;
            }

            this.exception = new ExceptionHolder(ExceptionDispatchInfo.Capture(e));
            this.TryInvokeContinuation();
            return true;

        }

        public bool TrySetCanceled()
        {
            if (Interlocked.CompareExchange(ref this.state, Canceled, Pending) != Pending)
            {
                return false;
            }

            this.TryInvokeContinuation();
            return true;

        }

        public bool TrySetCanceled(OperationCanceledException e)
        {
            if (Interlocked.CompareExchange(ref this.state, Canceled, Pending) != Pending)
            {
                return false;
            }

            this.exception = new ExceptionHolder(ExceptionDispatchInfo.Capture(e));
            this.TryInvokeContinuation();
            return true;

        }

        void IAwaiter.GetResult()
        {
            ((IAwaiter<T>) this).GetResult();
        }

        void INotifyCompletion.OnCompleted(Action action)
        {
            ((ICriticalNotifyCompletion) this).UnsafeOnCompleted(action);
        }
    }
}
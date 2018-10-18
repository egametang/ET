using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace ETModel
{
    /// <summary>
    /// Lightweight unity specified task-like object.
    /// </summary>
    [AsyncMethodBuilder(typeof (ETAsyncTaskMethodBuilder))]
    public partial struct ETTask: IEquatable<ETTask>
    {
        private static readonly ETTask<AsyncUnit> DefaultAsyncUnitTask = new ETTask<AsyncUnit>(AsyncUnit.Default);

        private readonly IAwaiter awaiter;

        [DebuggerHidden]
        public ETTask(IAwaiter awaiter)
        {
            this.awaiter = awaiter;
        }

        [DebuggerHidden]
        public ETTask(Func<ETTask> factory)
        {
            this.awaiter = new LazyPromise(factory);
        }

        [DebuggerHidden]
        public AwaiterStatus Status => awaiter?.Status ?? AwaiterStatus.Succeeded;

        [DebuggerHidden]
        public bool IsCompleted => awaiter?.IsCompleted ?? true;

        [DebuggerHidden]
        public void GetResult()
        {
            if (awaiter != null)
            {
                awaiter.GetResult();
            }
        }

        [DebuggerHidden]
        public Awaiter GetAwaiter()
        {
            return new Awaiter(this);
        }

        /// <summary>
        /// returns (bool IsCanceled) instead of throws OperationCanceledException.
        /// </summary>
        public ETTask<bool> SuppressCancellationThrow()
        {
            AwaiterStatus status = Status;
            switch (status)
            {
                case AwaiterStatus.Succeeded:
                    return CompletedTasks.False;
                case AwaiterStatus.Canceled:
                    return CompletedTasks.True;
                default:
                    return new ETTask<bool>(new IsCanceledAwaiter(this.awaiter));
            }
        }

        public bool Equals(ETTask other)
        {
            if (this.awaiter == null && other.awaiter == null)
            {
                return true;
            }

            if (this.awaiter != null && other.awaiter != null)
            {
                return this.awaiter == other.awaiter;
            }

            return false;
        }

        public override int GetHashCode()
        {
            if (this.awaiter == null)
            {
                return 0;
            }

            return this.awaiter.GetHashCode();
        }

        public override string ToString()
        {
            return this.awaiter == null? "()"
                    : this.awaiter.Status == AwaiterStatus.Succeeded? "()"
                    : "(" + this.awaiter.Status + ")";
        }

        public static implicit operator ETTask<AsyncUnit>(ETTask task)
        {
            if (task.awaiter == null)
            {
                return DefaultAsyncUnitTask;
            }

            if (task.awaiter.IsCompleted)
            {
                return DefaultAsyncUnitTask;
            }

            // UniTask<T> -> UniTask is free but UniTask -> UniTask<T> requires wrapping cost.
            return new ETTask<AsyncUnit>(new AsyncUnitAwaiter(task.awaiter));

        }

        private class AsyncUnitAwaiter: IAwaiter<AsyncUnit>
        {
            private readonly IAwaiter awaiter;

            public AsyncUnitAwaiter(IAwaiter awaiter)
            {
                this.awaiter = awaiter;
            }

            public bool IsCompleted => awaiter.IsCompleted;

            public AwaiterStatus Status => awaiter.Status;

            public AsyncUnit GetResult()
            {
                awaiter.GetResult();
                return AsyncUnit.Default;
            }

            public void OnCompleted(Action continuation)
            {
                awaiter.OnCompleted(continuation);
            }

            public void UnsafeOnCompleted(Action continuation)
            {
                awaiter.UnsafeOnCompleted(continuation);
            }

            void IAwaiter.GetResult()
            {
                awaiter.GetResult();
            }
        }

        private class IsCanceledAwaiter: IAwaiter<bool>
        {
            private readonly IAwaiter awaiter;

            public IsCanceledAwaiter(IAwaiter awaiter)
            {
                this.awaiter = awaiter;
            }

            public bool IsCompleted => awaiter.IsCompleted;

            public AwaiterStatus Status => awaiter.Status;

            public bool GetResult()
            {
                if (awaiter.Status == AwaiterStatus.Canceled)
                {
                    return true;
                }

                awaiter.GetResult();
                return false;
            }

            public void OnCompleted(Action continuation)
            {
                awaiter.OnCompleted(continuation);
            }

            public void UnsafeOnCompleted(Action continuation)
            {
                awaiter.UnsafeOnCompleted(continuation);
            }

            void IAwaiter.GetResult()
            {
                awaiter.GetResult();
            }
        }

        public struct Awaiter: IAwaiter
        {
            private readonly ETTask task;

            [DebuggerHidden]
            public Awaiter(ETTask task)
            {
                this.task = task;
            }

            [DebuggerHidden]
            public bool IsCompleted => task.IsCompleted;

            [DebuggerHidden]
            public AwaiterStatus Status => task.Status;

            [DebuggerHidden]
            public void GetResult()
            {
                task.GetResult();
            }

            [DebuggerHidden]
            public void OnCompleted(Action continuation)
            {
                if (task.awaiter != null)
                {
                    task.awaiter.OnCompleted(continuation);
                }
                else
                {
                    continuation();
                }
            }

            [DebuggerHidden]
            public void UnsafeOnCompleted(Action continuation)
            {
                if (task.awaiter != null)
                {
                    task.awaiter.UnsafeOnCompleted(continuation);
                }
                else
                {
                    continuation();
                }
            }
        }
    }

    /// <summary>
    /// Lightweight unity specified task-like object.
    /// </summary>
    [AsyncMethodBuilder(typeof (AsyncUniTaskMethodBuilder<>))]
    public struct ETTask<T>: IEquatable<ETTask<T>>
    {
        private readonly T result;
        private readonly IAwaiter<T> awaiter;

        [DebuggerHidden]
        public ETTask(T result)
        {
            this.result = result;
            this.awaiter = null;
        }

        [DebuggerHidden]
        public ETTask(IAwaiter<T> awaiter)
        {
            this.result = default;
            this.awaiter = awaiter;
        }

        [DebuggerHidden]
        public ETTask(Func<ETTask<T>> factory)
        {
            this.result = default;
            this.awaiter = new LazyPromise<T>(factory);
        }

        [DebuggerHidden]
        public AwaiterStatus Status => awaiter?.Status ?? AwaiterStatus.Succeeded;

        [DebuggerHidden]
        public bool IsCompleted => awaiter?.IsCompleted ?? true;

        [DebuggerHidden]
        public T Result
        {
            get
            {
                if (awaiter == null)
                {
                    return result;
                }

                return this.awaiter.GetResult();
            }
        }

        [DebuggerHidden]
        public Awaiter GetAwaiter()
        {
            return new Awaiter(this);
        }

        /// <summary>
        /// returns (bool IsCanceled, T Result) instead of throws OperationCanceledException.
        /// </summary>
        public ETTask<(bool IsCanceled, T Result)> SuppressCancellationThrow()
        {
            var status = Status;
            if (status == AwaiterStatus.Succeeded)
            {
                return new ETTask<(bool, T)>((false, Result));
            }

            if (status == AwaiterStatus.Canceled)
            {
                return new ETTask<(bool, T)>((true, default));
            }

            return new ETTask<(bool, T)>(new IsCanceledAwaiter(awaiter));
        }

        public bool Equals(ETTask<T> other)
        {
            if (this.awaiter == null && other.awaiter == null)
            {
                return EqualityComparer<T>.Default.Equals(this.result, other.result);
            }

            if (this.awaiter != null && other.awaiter != null)
            {
                return this.awaiter == other.awaiter;
            }

            return false;
        }

        public override int GetHashCode()
        {
            if (this.awaiter == null)
            {
                if (result == null)
                {
                    return 0;
                }

                return result.GetHashCode();
            }

            return this.awaiter.GetHashCode();
        }

        public override string ToString()
        {
            return this.awaiter == null? result.ToString()
                    : this.awaiter.Status == AwaiterStatus.Succeeded? this.awaiter.GetResult().ToString()
                    : "(" + this.awaiter.Status + ")";
        }

        public static implicit operator ETTask(ETTask<T> task)
        {
            if (task.awaiter != null)
            {
                return new ETTask(task.awaiter);
            }

            return new ETTask();
        }

        private class IsCanceledAwaiter: IAwaiter<(bool, T)>
        {
            private readonly IAwaiter<T> awaiter;

            public IsCanceledAwaiter(IAwaiter<T> awaiter)
            {
                this.awaiter = awaiter;
            }

            public bool IsCompleted => awaiter.IsCompleted;

            public AwaiterStatus Status => awaiter.Status;

            public (bool, T) GetResult()
            {
                if (awaiter.Status == AwaiterStatus.Canceled)
                {
                    return (true, default);
                }

                return (false, awaiter.GetResult());
            }

            public void OnCompleted(Action continuation)
            {
                awaiter.OnCompleted(continuation);
            }

            public void UnsafeOnCompleted(Action continuation)
            {
                awaiter.UnsafeOnCompleted(continuation);
            }

            void IAwaiter.GetResult()
            {
                awaiter.GetResult();
            }
        }

        public struct Awaiter: IAwaiter<T>
        {
            private readonly ETTask<T> task;

            [DebuggerHidden]
            public Awaiter(ETTask<T> task)
            {
                this.task = task;
            }

            [DebuggerHidden]
            public bool IsCompleted => task.IsCompleted;

            [DebuggerHidden]
            public AwaiterStatus Status => task.Status;

            [DebuggerHidden]
            void IAwaiter.GetResult()
            {
                GetResult();
            }

            [DebuggerHidden]
            public T GetResult()
            {
                return task.Result;
            }

            [DebuggerHidden]
            public void OnCompleted(Action continuation)
            {
                if (task.awaiter != null)
                {
                    task.awaiter.OnCompleted(continuation);
                }
                else
                {
                    continuation();
                }
            }

            [DebuggerHidden]
            public void UnsafeOnCompleted(Action continuation)
            {
                if (task.awaiter != null)
                {
                    task.awaiter.UnsafeOnCompleted(continuation);
                }
                else
                {
                    continuation();
                }
            }
        }
    }
}
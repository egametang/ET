using System;
using System.Threading;

namespace ETModel
{
    internal sealed class LazyPromise: IAwaiter
    {
        private Func<ETTask> factory;
        private ETTask value;

        public LazyPromise(Func<ETTask> factory)
        {
            this.factory = factory;
        }

        private void Create()
        {
            var f = Interlocked.Exchange(ref factory, null);
            if (f != null)
            {
                value = f();
            }
        }

        public bool IsCompleted
        {
            get
            {
                Create();
                return value.IsCompleted;
            }
        }

        public AwaiterStatus Status
        {
            get
            {
                Create();
                return value.Status;
            }
        }

        public void GetResult()
        {
            Create();
            value.GetResult();
        }

        void IAwaiter.GetResult()
        {
            GetResult();
        }

        public void UnsafeOnCompleted(Action continuation)
        {
            Create();
            value.GetAwaiter().UnsafeOnCompleted(continuation);
        }

        public void OnCompleted(Action continuation)
        {
            UnsafeOnCompleted(continuation);
        }
    }

    internal sealed class LazyPromise<T>: IAwaiter<T>
    {
        private Func<ETTask<T>> factory;
        private ETTask<T> value;

        public LazyPromise(Func<ETTask<T>> factory)
        {
            this.factory = factory;
        }

        private void Create()
        {
            var f = Interlocked.Exchange(ref factory, null);
            if (f != null)
            {
                value = f();
            }
        }

        public bool IsCompleted
        {
            get
            {
                Create();
                return value.IsCompleted;
            }
        }

        public AwaiterStatus Status
        {
            get
            {
                Create();
                return value.Status;
            }
        }

        public T GetResult()
        {
            Create();
            return value.Result;
        }

        void IAwaiter.GetResult()
        {
            GetResult();
        }

        public void UnsafeOnCompleted(Action continuation)
        {
            Create();
            value.GetAwaiter().UnsafeOnCompleted(continuation);
        }

        public void OnCompleted(Action continuation)
        {
            UnsafeOnCompleted(continuation);
        }
    }
}
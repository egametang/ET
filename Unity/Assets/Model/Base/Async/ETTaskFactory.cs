using System;
using System.Threading;

namespace ET
{
    public partial struct ETTask
    {
        public static ETTask CompletedTask => new ETTask();

        public static ETTask FromException(Exception ex)
        {
            ETTaskCompletionSource tcs = new ETTaskCompletionSource();
            tcs.TrySetException(ex);
            return tcs.Task;
        }

        public static ETTask<T> FromException<T>(Exception ex)
        {
            var tcs = new ETTaskCompletionSource<T>();
            tcs.TrySetException(ex);
            return tcs.Task;
        }

        public static ETTask<T> FromResult<T>(T value)
        {
            return new ETTask<T>(value);
        }

        public static ETTask FromCanceled()
        {
            return CanceledETTaskCache.Task;
        }

        public static ETTask<T> FromCanceled<T>()
        {
            return CanceledETTaskCache<T>.Task;
        }

        public static ETTask FromCanceled(CancellationToken token)
        {
            ETTaskCompletionSource tcs = new ETTaskCompletionSource();
            tcs.TrySetException(new OperationCanceledException(token));
            return tcs.Task;
        }

        public static ETTask<T> FromCanceled<T>(CancellationToken token)
        {
            var tcs = new ETTaskCompletionSource<T>();
            tcs.TrySetException(new OperationCanceledException(token));
            return tcs.Task;
        }
        
        private static class CanceledETTaskCache
        {
            public static readonly ETTask Task;

            static CanceledETTaskCache()
            {
                ETTaskCompletionSource tcs = new ETTaskCompletionSource();
                tcs.TrySetCanceled();
                Task = tcs.Task;
            }
        }

        private static class CanceledETTaskCache<T>
        {
            public static readonly ETTask<T> Task;

            static CanceledETTaskCache()
            {
                var taskCompletionSource = new ETTaskCompletionSource<T>();
                taskCompletionSource.TrySetCanceled();
                Task = taskCompletionSource.Task;
            }
        }
    }

    internal static class CompletedTasks
    {
        public static readonly ETTask<bool> True = ETTask.FromResult(true);
        public static readonly ETTask<bool> False = ETTask.FromResult(false);
        public static readonly ETTask<int> Zero = ETTask.FromResult(0);
        public static readonly ETTask<int> MinusOne = ETTask.FromResult(-1);
        public static readonly ETTask<int> One = ETTask.FromResult(1);
    }
}
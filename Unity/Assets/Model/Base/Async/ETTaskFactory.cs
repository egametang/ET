using System;
using System.Threading;

namespace ETModel
{
    public partial struct ETTask
    {
        private static readonly ETTask CanceledUniTask = new Func<ETTask>(() =>
        {
            var promise = new ETTaskCompletionSource<AsyncUnit>();
            promise.TrySetCanceled();
            return new ETTask(promise);
        })();

        public static ETTask CompletedTask => new ETTask();

        public static ETTask FromException(Exception ex)
        {
            var promise = new ETTaskCompletionSource<AsyncUnit>();
            promise.TrySetException(ex);
            return new ETTask(promise);
        }

        public static ETTask<T> FromException<T>(Exception ex)
        {
            var promise = new ETTaskCompletionSource<T>();
            promise.TrySetException(ex);
            return new ETTask<T>(promise);
        }

        public static ETTask<T> FromResult<T>(T value)
        {
            return new ETTask<T>(value);
        }

        public static ETTask FromCanceled()
        {
            return CanceledUniTask;
        }

        public static ETTask<T> FromCanceled<T>()
        {
            return CanceledUniTaskCache<T>.Task;
        }

        public static ETTask FromCanceled(CancellationToken token)
        {
            var promise = new ETTaskCompletionSource<AsyncUnit>();
            promise.TrySetException(new OperationCanceledException(token));
            return new ETTask(promise);
        }

        public static ETTask<T> FromCanceled<T>(CancellationToken token)
        {
            var promise = new ETTaskCompletionSource<T>();
            promise.TrySetException(new OperationCanceledException(token));
            return new ETTask<T>(promise);
        }

        /// <summary>shorthand of new UniTask[T](Func[UniTask[T]] factory)</summary>
        public static ETTask<T> Lazy<T>(Func<ETTask<T>> factory)
        {
            return new ETTask<T>(factory);
        }

        private static class CanceledUniTaskCache<T>
        {
            public static readonly ETTask<T> Task;

            static CanceledUniTaskCache()
            {
                var promise = new ETTaskCompletionSource<T>();
                promise.TrySetCanceled();
                Task = new ETTask<T>(promise);
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
using System;
using System.Threading;

namespace ETModel
{
    public static class CancellationTokenExtensions
    {
        private static readonly Action<object> cancellationTokenCallback = Callback;

        public static (ETTask, CancellationTokenRegistration) ToUniTask(this CancellationToken cts)
        {
            if (cts.IsCancellationRequested)
            {
                return (ETTask.FromCanceled(cts), default);
            }

            var promise = new ETTaskCompletionSource<AsyncUnit>();
            return (promise.Task, cts.RegisterWithoutCaptureExecutionContext(cancellationTokenCallback, promise));
        }

        private static void Callback(object state)
        {
            var promise = (ETTaskCompletionSource<AsyncUnit>) state;
            promise.TrySetResult(AsyncUnit.Default);
        }

        public static CancellationTokenRegistration RegisterWithoutCaptureExecutionContext(this CancellationToken cancellationToken, Action callback)
        {
            bool restoreFlow = false;
            if (!ExecutionContext.IsFlowSuppressed())
            {
                ExecutionContext.SuppressFlow();
                restoreFlow = true;
            }

            try
            {
                return cancellationToken.Register(callback, false);
            }
            finally
            {
                if (restoreFlow)
                {
                    ExecutionContext.RestoreFlow();
                }
            }
        }

        public static CancellationTokenRegistration RegisterWithoutCaptureExecutionContext(this CancellationToken cancellationToken,
        Action<object> callback, object state)
        {
            bool restoreFlow = false;
            if (!ExecutionContext.IsFlowSuppressed())
            {
                ExecutionContext.SuppressFlow();
                restoreFlow = true;
            }

            try
            {
                return cancellationToken.Register(callback, state, false);
            }
            finally
            {
                if (restoreFlow)
                {
                    ExecutionContext.RestoreFlow();
                }
            }
        }
    }
}
using System;
using YooAsset;
using static Cysharp.Threading.Tasks.Internal.Error;

namespace Cysharp.Threading.Tasks
{
    public static class AsyncOperationBaseExtensions
    {
        public static UniTask.Awaiter GetAwaiter(this AsyncOperationBase handle)
        {
            return ToUniTask(handle).GetAwaiter();
        }

        public static UniTask ToUniTask(this AsyncOperationBase handle,
                                        IProgress<float>        progress = null,
                                        PlayerLoopTiming        timing   = PlayerLoopTiming.Update)
        {
            ThrowArgumentNullException(handle, nameof(handle));

            if(handle.IsDone)
            {
                return UniTask.CompletedTask;
            }

            return new UniTask(
                AsyncOperationBaserConfiguredSource.Create(
                    handle,
                    timing,
                    progress,
                    out var token
                ),
                token
            );
        }

        sealed class AsyncOperationBaserConfiguredSource : IUniTaskSource,
                                                           IPlayerLoopItem,
                                                           ITaskPoolNode<AsyncOperationBaserConfiguredSource>
        {
            private static TaskPool<AsyncOperationBaserConfiguredSource> pool;

            private AsyncOperationBaserConfiguredSource nextNode;

            public ref AsyncOperationBaserConfiguredSource NextNode => ref nextNode;

            static AsyncOperationBaserConfiguredSource()
            {
                TaskPool.RegisterSizeGetter(typeof(AsyncOperationBaserConfiguredSource), () => pool.Size);
            }

            private readonly Action<AsyncOperationBase>             continuationAction;
            private          AsyncOperationBase                     handle;
            private          IProgress<float>                       progress;
            private          bool                                   completed;
            private          UniTaskCompletionSourceCore<AsyncUnit> core;

            AsyncOperationBaserConfiguredSource() { continuationAction = Continuation; }

            public static IUniTaskSource Create(AsyncOperationBase handle,
                                                PlayerLoopTiming   timing,
                                                IProgress<float>   progress,
                                                out short          token)
            {
                if(!pool.TryPop(out var result))
                {
                    result = new AsyncOperationBaserConfiguredSource();
                }

                result.handle    = handle;
                result.progress  = progress;
                result.completed = false;
                TaskTracker.TrackActiveTask(result, 3);

                if(progress != null)
                {
                    PlayerLoopHelper.AddAction(timing, result);
                }

                handle.Completed += result.continuationAction;

                token = result.core.Version;

                return result;
            }

            private void Continuation(AsyncOperationBase _)
            {
                handle.Completed -= continuationAction;

                if(completed)
                {
                    TryReturn();
                }
                else
                {
                    completed = true;
                    if(handle.Status == EOperationStatus.Failed)
                    {
                        core.TrySetException(new Exception(handle.Error));
                    }
                    else
                    {
                        core.TrySetResult(AsyncUnit.Default);
                    }
                }
            }

            bool TryReturn()
            {
                TaskTracker.RemoveTracking(this);
                core.Reset();
                handle   = default;
                progress = default;
                return pool.TryPush(this);
            }

            public UniTaskStatus GetStatus(short token) => core.GetStatus(token);

            public void OnCompleted(Action<object> continuation, object state, short token)
            {
                core.OnCompleted(continuation, state, token);
            }

            public void GetResult(short token) { core.GetResult(token); }

            public UniTaskStatus UnsafeGetStatus() => core.UnsafeGetStatus();

            public bool MoveNext()
            {
                if(completed)
                {
                    TryReturn();
                    return false;
                }

                if(!handle.IsDone)
                {
                    progress?.Report(handle.Progress);
                }

                return true;
            }
        }
    }
}
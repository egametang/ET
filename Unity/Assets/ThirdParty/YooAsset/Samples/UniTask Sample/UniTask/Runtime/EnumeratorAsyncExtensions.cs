﻿#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using System;
using System.Collections;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading;
using Cysharp.Threading.Tasks.Internal;
using UnityEngine;

namespace Cysharp.Threading.Tasks
{
    public static class EnumeratorAsyncExtensions
    {
        public static UniTask.Awaiter GetAwaiter<T>(this T enumerator)
            where T : IEnumerator
        {
            var e = (IEnumerator)enumerator;
            Error.ThrowArgumentNullException(e, nameof(enumerator));
            return new UniTask(EnumeratorPromise.Create(e, PlayerLoopTiming.Update, CancellationToken.None, out var token), token).GetAwaiter();
        }

        public static UniTask WithCancellation(this IEnumerator enumerator, CancellationToken cancellationToken)
        {
            Error.ThrowArgumentNullException(enumerator, nameof(enumerator));
            return new UniTask(EnumeratorPromise.Create(enumerator, PlayerLoopTiming.Update, cancellationToken, out var token), token);
        }

        public static UniTask ToUniTask(this IEnumerator enumerator, PlayerLoopTiming timing = PlayerLoopTiming.Update, CancellationToken cancellationToken = default(CancellationToken))
        {
            Error.ThrowArgumentNullException(enumerator, nameof(enumerator));
            return new UniTask(EnumeratorPromise.Create(enumerator, timing, cancellationToken, out var token), token);
        }

        public static UniTask ToUniTask(this IEnumerator enumerator, MonoBehaviour coroutineRunner)
        {
            var source = AutoResetUniTaskCompletionSource.Create();
            coroutineRunner.StartCoroutine(Core(enumerator, coroutineRunner, source));
            return source.Task;
        }

        static IEnumerator Core(IEnumerator inner, MonoBehaviour coroutineRunner, AutoResetUniTaskCompletionSource source)
        {
            yield return coroutineRunner.StartCoroutine(inner);
            source.TrySetResult();
        }

        sealed class EnumeratorPromise : IUniTaskSource, IPlayerLoopItem, ITaskPoolNode<EnumeratorPromise>
        {
            static TaskPool<EnumeratorPromise> pool;
            EnumeratorPromise nextNode;
            public ref EnumeratorPromise NextNode => ref nextNode;

            static EnumeratorPromise()
            {
                TaskPool.RegisterSizeGetter(typeof(EnumeratorPromise), () => pool.Size);
            }

            IEnumerator innerEnumerator;
            CancellationToken cancellationToken;
            int initialFrame;
            bool loopRunning;
            bool calledGetResult;

            UniTaskCompletionSourceCore<object> core;

            EnumeratorPromise()
            {
            }

            public static IUniTaskSource Create(IEnumerator innerEnumerator, PlayerLoopTiming timing, CancellationToken cancellationToken, out short token)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return AutoResetUniTaskCompletionSource.CreateFromCanceled(cancellationToken, out token);
                }

                if (!pool.TryPop(out var result))
                {
                    result = new EnumeratorPromise();
                }
                TaskTracker.TrackActiveTask(result, 3);

                result.innerEnumerator = ConsumeEnumerator(innerEnumerator);
                result.cancellationToken = cancellationToken;
                result.loopRunning = true;
                result.calledGetResult = false;
                result.initialFrame = -1;

                token = result.core.Version;

                // run immediately.
                if (result.MoveNext())
                {
                    PlayerLoopHelper.AddAction(timing, result);
                }
                
                return result;
            }

            public void GetResult(short token)
            {
                try
                {
                    calledGetResult = true;
                    core.GetResult(token);
                }
                finally
                {
                    if (!loopRunning)
                    {
                        TryReturn();
                    }
                }
            }

            public UniTaskStatus GetStatus(short token)
            {
                return core.GetStatus(token);
            }

            public UniTaskStatus UnsafeGetStatus()
            {
                return core.UnsafeGetStatus();
            }

            public void OnCompleted(Action<object> continuation, object state, short token)
            {
                core.OnCompleted(continuation, state, token);
            }

            public bool MoveNext()
            {
                if (calledGetResult)
                {
                    loopRunning = false;
                    TryReturn();
                    return false;
                }

                if (innerEnumerator == null) // invalid status, returned but loop running?
                {
                    return false;
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    loopRunning = false;
                    core.TrySetCanceled(cancellationToken);
                    return false;
                }

                if (initialFrame == -1)
                {
                    // Time can not touch in threadpool.
                    if (PlayerLoopHelper.IsMainThread)
                    {
                        initialFrame = Time.frameCount;
                    }
                }
                else if (initialFrame == Time.frameCount)
                {
                    return true; // already executed in first frame, skip.
                }

                try
                {
                    if (innerEnumerator.MoveNext())
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    loopRunning = false;
                    core.TrySetException(ex);
                    return false;
                }

                loopRunning = false;
                core.TrySetResult(null);
                return false;
            }

            bool TryReturn()
            {
                TaskTracker.RemoveTracking(this);
                core.Reset();
                innerEnumerator = default;
                cancellationToken = default;

                return pool.TryPush(this);
            }

            // Unwrap YieldInstructions

            static IEnumerator ConsumeEnumerator(IEnumerator enumerator)
            {
                while (enumerator.MoveNext())
                {
                    var current = enumerator.Current;
                    if (current == null)
                    {
                        yield return null;
                    }
                    else if (current is CustomYieldInstruction cyi)
                    {
                        // WWW, WaitForSecondsRealtime
                        while (cyi.keepWaiting)
                        {
                            yield return null;
                        }
                    }
                    else if (current is YieldInstruction)
                    {
                        IEnumerator innerCoroutine = null;
                        switch (current)
                        {
                            case AsyncOperation ao:
                                innerCoroutine = UnwrapWaitAsyncOperation(ao);
                                break;
                            case WaitForSeconds wfs:
                                innerCoroutine = UnwrapWaitForSeconds(wfs);
                                break;
                        }
                        if (innerCoroutine != null)
                        {
                            while (innerCoroutine.MoveNext())
                            {
                                yield return null;
                            }
                        }
                        else
                        {
                            goto WARN;
                        }
                    }
                    else if (current is IEnumerator e3)
                    {
                        var e4 = ConsumeEnumerator(e3);
                        while (e4.MoveNext())
                        {
                            yield return null;
                        }
                    }
                    else
                    {
                        goto WARN;
                    }

                    continue;

                    WARN:
                    // WaitForEndOfFrame, WaitForFixedUpdate, others.
                    UnityEngine.Debug.LogWarning($"yield {current.GetType().Name} is not supported on await IEnumerator or IEnumerator.ToUniTask(), please use ToUniTask(MonoBehaviour coroutineRunner) instead.");
                    yield return null;
                }
            }

            static readonly FieldInfo waitForSeconds_Seconds = typeof(WaitForSeconds).GetField("m_Seconds", BindingFlags.Instance | BindingFlags.GetField | BindingFlags.NonPublic);

            static IEnumerator UnwrapWaitForSeconds(WaitForSeconds waitForSeconds)
            {
                var second = (float)waitForSeconds_Seconds.GetValue(waitForSeconds);
                var elapsed = 0.0f;
                while (true)
                {
                    yield return null;

                    elapsed += Time.deltaTime;
                    if (elapsed >= second)
                    {
                        break;
                    }
                };
            }

            static IEnumerator UnwrapWaitAsyncOperation(AsyncOperation asyncOperation)
            {
                while (!asyncOperation.isDone)
                {
                    yield return null;
                }
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;

namespace ET
{
    [AsyncMethodBuilder(typeof (ETAsyncTaskMethodBuilder))]
    public class ETTask: ICriticalNotifyCompletion
    {
        public static ETTaskCompleted CompletedTask
        {
            get
            {
                return new ETTaskCompleted();
            }
        }

        private static readonly Queue<ETTask> queue = new Queue<ETTask>();

        /// <summary>
        /// 请不要随便使用ETTask的对象池，除非你完全搞懂了ETTask!!!
        /// 假如开启了池,await之后不能再操作ETTask，否则可能操作到再次从池中分配出来的ETTask，产生灾难性的后果
        /// SetResult的时候请现将tcs置空，避免多次对同一个ETTask SetResult
        /// </summary>
        public static ETTask Create(bool fromPool = false)
        {
            if (!fromPool)
            {
                return new ETTask();
            }
            
            if (queue.Count == 0)
            {
                return new ETTask() {fromPool = true};    
            }
            return queue.Dequeue();
        }

        private void Recycle()
        {
            if (!this.fromPool)
            {
                return;
            }
            
            this.state = AwaiterStatus.Pending;
            this.callback = null;
            queue.Enqueue(this);
            // 太多了，回收一下
            if (queue.Count > 1000)
            {
                queue.Clear();
            }
        }

        private bool fromPool;
        private AwaiterStatus state;
        private object callback; // Action or ExceptionDispatchInfo

        private ETTask()
        {
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden]
        private async ETVoid InnerCoroutine()
        {
            await this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden]
        public void Coroutine()
        {
            InnerCoroutine().Coroutine();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden]
        public ETTask GetAwaiter()
        {
            return this;
        }

        
        public bool IsCompleted
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            [DebuggerHidden]
            get
            {
                return this.state != AwaiterStatus.Pending;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden]
        public void UnsafeOnCompleted(Action action)
        {
            if (this.state != AwaiterStatus.Pending)
            {
                action?.Invoke();
                return;
            }

            this.callback = action;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden]
        public void OnCompleted(Action action)
        {
            this.UnsafeOnCompleted(action);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden]
        public void GetResult()
        {
            switch (this.state)
            {
                case AwaiterStatus.Succeeded:
                    this.Recycle();
                    break;
                case AwaiterStatus.Faulted:
                    ExceptionDispatchInfo c = this.callback as ExceptionDispatchInfo;
                    this.callback = null;
                    c?.Throw();
                    this.Recycle();
                    break;
                default:
                    throw new NotSupportedException("ETTask does not allow call GetResult directly when task not completed. Please use 'await'.");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden]
        public void SetResult()
        {
            if (this.state != AwaiterStatus.Pending)
            {
                throw new InvalidOperationException("TaskT_TransitionToFinal_AlreadyCompleted");
            }

            this.state = AwaiterStatus.Succeeded;

            Action c = this.callback as Action;
            this.callback = null;
            c?.Invoke();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden]
        public void SetException(Exception e)
        {
            if (this.state != AwaiterStatus.Pending)
            {
                throw new InvalidOperationException("TaskT_TransitionToFinal_AlreadyCompleted");
            }

            this.state = AwaiterStatus.Faulted;

            Action c = this.callback as Action;
            this.callback = ExceptionDispatchInfo.Capture(e);
            c?.Invoke();
        }
    }

    [AsyncMethodBuilder(typeof (ETAsyncTaskMethodBuilder<>))]
    public class ETTask<T>: ICriticalNotifyCompletion
    {
        private static readonly Queue<ETTask<T>> queue = new Queue<ETTask<T>>();
        
        /// <summary>
        /// 请不要随便使用ETTask的对象池，除非你完全搞懂了ETTask!!!
        /// 假如开启了池,await之后不能再操作ETTask，否则可能操作到再次从池中分配出来的ETTask，产生灾难性的后果
        /// SetResult的时候请现将tcs置空，避免多次对同一个ETTask SetResult
        /// </summary>
        public static ETTask<T> Create(bool fromPool = false)
        {
            if (!fromPool)
            {
                return new ETTask<T>();
            }
            
            if (queue.Count == 0)
            {
                return new ETTask<T>() { fromPool = true };    
            }
            return queue.Dequeue();
        }
        
        private void Recycle()
        {
            if (!this.fromPool)
            {
                return;
            }
            this.callback = null;
            this.value = default;
            this.state = AwaiterStatus.Pending;
            queue.Enqueue(this);
            // 太多了，回收一下
            if (queue.Count > 1000)
            {
                queue.Clear();
            }
        }

        private bool fromPool;
        private AwaiterStatus state;
        private T value;
        private object callback; // Action or ExceptionDispatchInfo

        private ETTask()
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden]
        private async ETVoid InnerCoroutine()
        {
            await this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden]
        public void Coroutine()
        {
            InnerCoroutine().Coroutine();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden]
        public ETTask<T> GetAwaiter()
        {
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden]
        public T GetResult()
        {
            switch (this.state)
            {
                case AwaiterStatus.Succeeded:
                    T v = this.value;
                    this.Recycle();
                    return v;
                case AwaiterStatus.Faulted:
                    ExceptionDispatchInfo c = this.callback as ExceptionDispatchInfo;
                    this.callback = null;
                    c?.Throw();
                    this.Recycle();
                    return default;
                default:
                    throw new NotSupportedException("ETask does not allow call GetResult directly when task not completed. Please use 'await'.");
            }
        }


        public bool IsCompleted
        {
            [DebuggerHidden]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return state != AwaiterStatus.Pending;
            }
        } 

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden]
        public void UnsafeOnCompleted(Action action)
        {
            if (this.state != AwaiterStatus.Pending)
            {
                action?.Invoke();
                return;
            }

            this.callback = action;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden]
        public void OnCompleted(Action action)
        {
            this.UnsafeOnCompleted(action);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden]
        public void SetResult(T result)
        {
            if (this.state != AwaiterStatus.Pending)
            {
                throw new InvalidOperationException("TaskT_TransitionToFinal_AlreadyCompleted");
            }

            this.state = AwaiterStatus.Succeeded;

            this.value = result;

            Action c = this.callback as Action;
            this.callback = null;
            c?.Invoke();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden]
        public void SetException(Exception e)
        {
            if (this.state != AwaiterStatus.Pending)
            {
                throw new InvalidOperationException("TaskT_TransitionToFinal_AlreadyCompleted");
            }

            this.state = AwaiterStatus.Faulted;

            Action c = this.callback as Action;
            this.callback = ExceptionDispatchInfo.Capture(e);
            c?.Invoke();
        }
    }
}
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;

namespace ET
{
    internal static class IETTaskExtension
    {
        internal static void SetContext(this IETTask task, object context)
        {
            while (true)
            {
                if (task.TaskType == TaskType.ContextTask)
                {
                    ((ETTask<object>)task).SetResult(context);
                    break;
                }

                // cancellationToken传下去
                task.TaskType = TaskType.WithContext;
                object child = task.Context;
                task.Context = context;
                task = child as IETTask;
                if (task == null)
                {
                    break;
                }
                //// 传递到WithContext为止，因为可能这一层设置了新的context
                if (task.TaskType == TaskType.WithContext)
                {
                    break;
                }
            }
        }
    }
    
    public enum TaskType: byte
    {
        Common,
        WithContext,
        ContextTask,
    }
    
    public interface IETTask
    {
        public TaskType TaskType { get; set; }
        public object Context { get; set; }
    }
    
    [AsyncMethodBuilder(typeof (ETAsyncTaskMethodBuilder))]
    public class ETTask: ICriticalNotifyCompletion, IETTask
    {
        [StaticField]
        public static Action<Exception> ExceptionHandler;

        [StaticField]
        private static ETTask completedTask;
        
        [StaticField]
        public static ETTask CompletedTask
        {
            get
            {
                return completedTask ??= new ETTask() { state = AwaiterStatus.Succeeded };
            }
        }

        [StaticField]
        private static readonly ConcurrentQueue<ETTask> queue = new();

        /// <summary>
        /// 请不要随便使用ETTask的对象池，除非你完全搞懂了ETTask!!!
        /// 假如开启了池,await之后不能再操作ETTask，否则可能操作到再次从池中分配出来的ETTask，产生灾难性的后果
        /// SetResult的时候请现将tcs置空，避免多次对同一个ETTask SetResult
        /// </summary>
        [DebuggerHidden]
        public static ETTask Create(bool fromPool = false)
        {
            if (!fromPool)
            {
                return new ETTask();
            }
            if (!queue.TryDequeue(out ETTask task))
            {
                return new ETTask() {fromPool = true}; 
            }
            return task;
        }

        [DebuggerHidden]
        private void Recycle()
        {
            if (!this.fromPool)
            {
                return;
            }
            
            this.state = AwaiterStatus.Pending;
            this.callback = null;
            this.Context = null;
            this.TaskType = TaskType.Common;
            // 太多了
            if (queue.Count > 1000)
            {
                return;
            }
            queue.Enqueue(this);
        }

        private bool fromPool;
        private AwaiterStatus state;
        private object callback; // Action or ExceptionDispatchInfo

        [DebuggerHidden]
        private ETTask()
        {
            this.TaskType = TaskType.Common;
        }
        
        [DebuggerHidden]
        private async ETVoid InnerCoroutine()
        {
            await this;
        }

        [DebuggerHidden]
        public void NoContext()
        {
            this.SetContext(null);
            InnerCoroutine().Coroutine();
        }
        
        [DebuggerHidden]
        public void WithContext(object context)
        {
            this.SetContext(context);
            InnerCoroutine().Coroutine();
        }
        
        /// <summary>
        /// 在await的同时可以换一个新的上下文
        /// </summary>
        [DebuggerHidden]
        public async ETTask NewContext(object context)
        {
            this.SetContext(context);
            await this;
        }

        [DebuggerHidden]
        public ETTask GetAwaiter()
        {
            return this;
        }

        
        public bool IsCompleted
        {
            [DebuggerHidden]
            get
            {
                return this.state != AwaiterStatus.Pending;
            }
        }

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

        [DebuggerHidden]
        public void OnCompleted(Action action)
        {
            this.UnsafeOnCompleted(action);
        }

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
                    this.Recycle();
                    c?.Throw();
                    break;
                default:
                    throw new NotSupportedException("ETTask does not allow call GetResult directly when task not completed. Please use 'await'.");
            }
        }

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

        public TaskType TaskType { get; set; }
        public object Context { get; set; }
    }

    [AsyncMethodBuilder(typeof (ETAsyncTaskMethodBuilder<>))]
    public class ETTask<T>: ICriticalNotifyCompletion, IETTask
    {
        [StaticField]
        private static readonly ConcurrentQueue<ETTask<T>> queue = new();
        
        /// <summary>
        /// 请不要随便使用ETTask的对象池，除非你完全搞懂了ETTask!!!
        /// 假如开启了池,await之后不能再操作ETTask，否则可能操作到再次从池中分配出来的ETTask，产生灾难性的后果
        /// SetResult的时候请现将tcs置空，避免多次对同一个ETTask SetResult
        /// </summary>
        [DebuggerHidden]
        public static ETTask<T> Create(bool fromPool = false)
        {
            if (!fromPool)
            {
                return new ETTask<T>();
            }
            
            if (!queue.TryDequeue(out ETTask<T> task))
            {
                return new ETTask<T>() {fromPool = true}; 
            }
            return task;
        }
        
        [DebuggerHidden]
        private void Recycle()
        {
            if (!this.fromPool)
            {
                return;
            }
            this.callback = null;
            this.value = default;
            this.state = AwaiterStatus.Pending;
            this.Context = null;
            this.TaskType = TaskType.Common;
            // 太多了
            if (queue.Count > 1000)
            {
                return;
            }
            queue.Enqueue(this);
        }

        private bool fromPool;
        private AwaiterStatus state;
        private T value;
        private object callback; // Action or ExceptionDispatchInfo

        [DebuggerHidden]
        private ETTask()
        {
            this.TaskType = TaskType.Common;
        }

        [DebuggerHidden]
        private async ETVoid InnerCoroutine()
        {
            await this;
        }

        [DebuggerHidden]
        public void NoContext()
        {
            this.SetContext(null);
            InnerCoroutine().Coroutine();
        }
        
        [DebuggerHidden]
        public void WithContext(object context)
        {
            this.SetContext(context);
            InnerCoroutine().Coroutine();
        }
        
        /// <summary>
        /// 在await的同时可以换一个新的cancellationToken
        /// </summary>
        [DebuggerHidden]
        public async ETTask<T> NewContext(object context)
        {
            this.SetContext(context);
            return await this;
        }

        [DebuggerHidden]
        public ETTask<T> GetAwaiter()
        {
            return this;
        }

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
                    this.Recycle();
                    c?.Throw();
                    return default;
                default:
                    throw new NotSupportedException("ETask does not allow call GetResult directly when task not completed. Please use 'await'.");
            }
        }
        
        public bool IsCompleted
        {
            [DebuggerHidden]
            get
            {
                return state != AwaiterStatus.Pending;
            }
        } 

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

        [DebuggerHidden]
        public void OnCompleted(Action action)
        {
            this.UnsafeOnCompleted(action);
        }

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

        public TaskType TaskType { get; set; }
        public object Context { get; set; }
    }
}
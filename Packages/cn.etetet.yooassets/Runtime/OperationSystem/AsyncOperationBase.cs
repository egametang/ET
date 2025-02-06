using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace YooAsset
{
    public abstract class AsyncOperationBase : IEnumerator, IComparable<AsyncOperationBase>
    {
        // 用户请求的回调
        private Action<AsyncOperationBase> _callback;

        // 是否已经完成
        internal bool IsFinish = false;

        /// <summary>
        /// 所属包裹
        /// </summary>
        public string PackageName { private set; get; }

        /// <summary>
        /// 优先级
        /// </summary>
        public uint Priority { set; get; } = 0;

        /// <summary>
        /// 状态
        /// </summary>
        public EOperationStatus Status { get; protected set; } = EOperationStatus.None;

        /// <summary>
        /// 错误信息
        /// </summary>
        public string Error { get; protected set; }

        /// <summary>
        /// 处理进度
        /// </summary>
        public float Progress { get; protected set; }

        /// <summary>
        /// 是否已经完成
        /// </summary>
        public bool IsDone
        {
            get
            {
                return Status == EOperationStatus.Failed || Status == EOperationStatus.Succeed;
            }
        }

        /// <summary>
        /// 完成事件
        /// </summary>
        public event Action<AsyncOperationBase> Completed
        {
            add
            {
                if (IsDone)
                    value.Invoke(this);
                else
                    _callback += value;
            }
            remove
            {
                _callback -= value;
            }
        }

        /// <summary>
        /// 异步操作任务
        /// </summary>
        public Task Task
        {
            get
            {
                if (_taskCompletionSource == null)
                {
                    _taskCompletionSource = new TaskCompletionSource<object>();
                    if (IsDone)
                        _taskCompletionSource.SetResult(null);
                }
                return _taskCompletionSource.Task;
            }
        }

        internal abstract void InternalOnStart();
        internal abstract void InternalOnUpdate();
        internal virtual void InternalOnAbort() { }

        internal void SetPackageName(string packageName)
        {
            PackageName = packageName;
        }
        internal void SetStart()
        {
            Status = EOperationStatus.Processing;
            InternalOnStart();
        }
        internal void SetFinish()
        {
            IsFinish = true;

            // 进度百分百完成
            Progress = 1f;

            //注意：如果完成回调内发生异常，会导致Task无限期等待
            _callback?.Invoke(this);

            if (_taskCompletionSource != null)
                _taskCompletionSource.TrySetResult(null);
        }
        internal void SetAbort()
        {
            if (IsDone == false)
            {
                Status = EOperationStatus.Failed;
                Error = "user abort";
                YooLogger.Warning($"Async operaiton has been abort : {this.GetType().Name}");
                InternalOnAbort();
            }
        }

        /// <summary>
        /// 清空完成回调
        /// </summary>
        protected void ClearCompletedCallback()
        {
            _callback = null;
        }

        #region 排序接口实现
        public int CompareTo(AsyncOperationBase other)
        {
            return other.Priority.CompareTo(this.Priority);
        }
        #endregion

        #region 异步编程相关
        bool IEnumerator.MoveNext()
        {
            return !IsDone;
        }
        void IEnumerator.Reset()
        {
        }
        object IEnumerator.Current => null;

        private TaskCompletionSource<object> _taskCompletionSource;
        #endregion
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace YooAsset
{
	public abstract class AsyncOperationBase : IEnumerator
	{
		// 用户请求的回调
		private Action<AsyncOperationBase> _callback;

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

		internal abstract void Start();
		internal abstract void Update();
		internal void Finish()
		{
			Progress = 1f;
			_callback?.Invoke(this);
			if (_taskCompletionSource != null)
				_taskCompletionSource.TrySetResult(null);
		}

		/// <summary>
		/// 清空完成回调
		/// </summary>
		protected void ClearCompletedCallback()
		{
			_callback = null;
		}

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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace YooAsset
{
	internal class OperationSystem
	{
		private static readonly List<AsyncOperationBase> _operations = new List<AsyncOperationBase>(100);
		private static readonly List<AsyncOperationBase> _addList = new List<AsyncOperationBase>(100);
		private static readonly List<AsyncOperationBase> _removeList = new List<AsyncOperationBase>(100);

		// 计时器相关
		private static Stopwatch _watch;
		private static long _frameTime;

		/// <summary>
		/// 异步操作的最小时间片段
		/// </summary>
		public static long MaxTimeSlice { set; get; } = long.MaxValue;

		/// <summary>
		/// 处理器是否繁忙
		/// </summary>
		public static bool IsBusy
		{
			get
			{
				return _watch.ElapsedMilliseconds - _frameTime >= MaxTimeSlice;
			}
		}


		/// <summary>
		/// 初始化异步操作系统
		/// </summary>
		public static void Initialize()
		{
			_watch = Stopwatch.StartNew();
		}

		/// <summary>
		/// 更新异步操作系统
		/// </summary>
		public static void Update()
		{
			_frameTime = _watch.ElapsedMilliseconds;

			// 添加新的异步操作
			if (_addList.Count > 0)
			{
				for (int i = 0; i < _addList.Count; i++)
				{
					var operation = _addList[i];
					_operations.Add(operation);
				}
				_addList.Clear();
			}

			// 更新所有的异步操作
			foreach (var operation in _operations)
			{
				if (IsBusy)
					break;

				operation.Update();
				if (operation.IsDone)
				{
					_removeList.Add(operation);
					operation.Finish();
				}
			}

			// 移除已经完成的异步操作
			if (_removeList.Count > 0)
			{
				foreach (var operation in _removeList)
				{
					_operations.Remove(operation);
				}
				_removeList.Clear();
			}
		}

		/// <summary>
		/// 销毁异步操作系统
		/// </summary>
		public static void DestroyAll()
		{
			_operations.Clear();
			_addList.Clear();
			_removeList.Clear();
			_watch = null;
			_frameTime = 0;
			MaxTimeSlice = long.MaxValue;
		}

		/// <summary>
		/// 开始处理异步操作类
		/// </summary>
		public static void StartOperation(AsyncOperationBase operation)
		{
			_addList.Add(operation);
			operation.Start();
		}
	}
}
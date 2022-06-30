using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace YooAsset
{
	internal class OperationSystem
	{
		private static readonly List<AsyncOperationBase> _operations = new List<AsyncOperationBase>(100);

		// 计时器相关
		private static Stopwatch _watch;
		private static long _maxTimeSlice;
		private static long _frameTime;


		/// <summary>
		/// 初始化异步操作系统
		/// </summary>
		public static void Initialize(long maxTimeSlice)
		{
			_maxTimeSlice = maxTimeSlice;
			_watch = Stopwatch.StartNew();
		}

		/// <summary>
		/// 更新异步操作系统
		/// </summary>
		public static void Update()
		{
			_frameTime = _watch.ElapsedMilliseconds;

			for (int i = _operations.Count - 1; i >= 0; i--)
			{
				if (_watch.ElapsedMilliseconds - _frameTime >= _maxTimeSlice)
					return;

				var operation = _operations[i];
				operation.Update();
				if (operation.IsDone)
				{
					_operations.RemoveAt(i);
					operation.Finish();
				}
			}
		}

		/// <summary>
		/// 销毁异步操作系统
		/// </summary>
		public static void DestroyAll()
		{
			_operations.Clear();
			_watch = null;
			_maxTimeSlice = 0;
			_frameTime = 0;
		}

		/// <summary>
		/// 开始处理异步操作类
		/// </summary>
		public static void StartOperaiton(AsyncOperationBase operationBase)
		{
			_operations.Add(operationBase);
			operationBase.Start();
		}
	}
}
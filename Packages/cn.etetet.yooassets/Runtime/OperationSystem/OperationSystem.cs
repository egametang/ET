using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace YooAsset
{
    internal class OperationSystem
    {
        private static readonly List<AsyncOperationBase> _operations = new List<AsyncOperationBase>(1000);
        private static readonly List<AsyncOperationBase> _newList = new List<AsyncOperationBase>(1000);

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

            // 添加新增的异步操作
            if (_newList.Count > 0)
            {
                bool sorting = false;
                foreach (var operation in _newList)
                {
                    if (operation.Priority > 0)
                    {
                        sorting = true;
                        break;
                    }
                }

                _operations.AddRange(_newList);
                _newList.Clear();

                // 重新排序优先级
                if (sorting)
                    _operations.Sort();
            }

            // 更新进行中的异步操作
            for (int i = 0; i < _operations.Count; i++)
            {
                if (IsBusy)
                    break;

                var operation = _operations[i];
                if (operation.IsFinish)
                    continue;

                if (operation.IsDone == false)
                    operation.InternalOnUpdate();

                if (operation.IsDone)
                    operation.SetFinish();
            }

            // 移除已经完成的异步操作
            for (int i = _operations.Count - 1; i >= 0; i--)
            {
                var operation = _operations[i];
                if (operation.IsFinish)
                    _operations.RemoveAt(i);
            }
        }

        /// <summary>
        /// 销毁异步操作系统
        /// </summary>
        public static void DestroyAll()
        {
            _operations.Clear();
            _newList.Clear();
            _watch = null;
            _frameTime = 0;
            MaxTimeSlice = long.MaxValue;
        }

        /// <summary>
        /// 销毁包裹的所有任务
        /// </summary>
        public static void ClearPackageOperation(string packageName)
        {
            // 终止临时队列里的任务
            foreach (var operation in _newList)
            {
                if (operation.PackageName == packageName)
                {
                    operation.SetAbort();
                }
            }

            // 终止正在进行的任务
            foreach (var operation in _operations)
            {
                if (operation.PackageName == packageName)
                {
                    operation.SetAbort();
                }
            }
        }

        /// <summary>
        /// 开始处理异步操作类
        /// </summary>
        public static void StartOperation(string packageName, AsyncOperationBase operation)
        {
            _newList.Add(operation);
            operation.SetPackageName(packageName);
            operation.SetStart();
        }

        /// <summary>
        /// 开始处理异步操作类
        /// </summary>
        public static void StartOperation(AsyncOperationBase operation)
        {
            _newList.Add(operation);
            operation.SetStart();
        }
    }
}
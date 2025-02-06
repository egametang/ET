using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace YooAsset
{
    internal abstract class VerifyCacheFilesOperation : AsyncOperationBase
    {
        public static VerifyCacheFilesOperation CreateOperation(CacheManager cache, List<VerifyCacheFileElement> elements)
        {
#if UNITY_WEBGL
            var operation = new VerifyCacheFilesWithoutThreadOperation(cache, elements);
#else
            var operation = new VerifyCacheFilesWithThreadOperation(cache, elements);
#endif
            return operation;
        }
    }

    /// <summary>
    /// 本地缓存文件验证（线程版）
    /// </summary>
    internal class VerifyCacheFilesWithThreadOperation : VerifyCacheFilesOperation
    {
        private enum ESteps
        {
            None,
            InitVerify,
            UpdateVerify,
            Done,
        }

        private readonly ThreadSyncContext _syncContext = new ThreadSyncContext();
        private readonly CacheManager _cache;
        private List<VerifyCacheFileElement> _waitingList;
        private List<VerifyCacheFileElement> _verifyingList;
        private int _verifyMaxNum;
        private int _verifyTotalCount;
        private float _verifyStartTime;
        private int _succeedCount;
        private int _failedCount;
        private ESteps _steps = ESteps.None;

        public VerifyCacheFilesWithThreadOperation(CacheManager cache, List<VerifyCacheFileElement> elements)
        {
            _cache = cache;
            _waitingList = elements;
        }
        internal override void InternalOnStart()
        {
            _steps = ESteps.InitVerify;
            _verifyStartTime = UnityEngine.Time.realtimeSinceStartup;
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.InitVerify)
            {
                int fileCount = _waitingList.Count;

                // 设置同时验证的最大数
                ThreadPool.GetMaxThreads(out int workerThreads, out int ioThreads);
                YooLogger.Log($"Work threads : {workerThreads}, IO threads : {ioThreads}");
                _verifyMaxNum = Math.Min(workerThreads, ioThreads);
                _verifyTotalCount = fileCount;
                if (_verifyMaxNum < 1)
                    _verifyMaxNum = 1;

                _verifyingList = new List<VerifyCacheFileElement>(_verifyMaxNum);
                _steps = ESteps.UpdateVerify;
            }

            if (_steps == ESteps.UpdateVerify)
            {
                _syncContext.Update();

                Progress = GetProgress();
                if (_waitingList.Count == 0 && _verifyingList.Count == 0)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                    float costTime = UnityEngine.Time.realtimeSinceStartup - _verifyStartTime;
                    YooLogger.Log($"Verify cache files elapsed time {costTime:f1} seconds");
                }

                for (int i = _waitingList.Count - 1; i >= 0; i--)
                {
                    if (OperationSystem.IsBusy)
                        break;

                    if (_verifyingList.Count >= _verifyMaxNum)
                        break;

                    var element = _waitingList[i];
                    if (BeginVerifyFileWithThread(element))
                    {
                        _waitingList.RemoveAt(i);
                        _verifyingList.Add(element);
                    }
                    else
                    {
                        YooLogger.Warning("The thread pool is failed queued.");
                        break;
                    }
                }
            }
        }

        private float GetProgress()
        {
            if (_verifyTotalCount == 0)
                return 1f;
            return (float)(_succeedCount + _failedCount) / _verifyTotalCount;
        }
        private bool BeginVerifyFileWithThread(VerifyCacheFileElement element)
        {
            return ThreadPool.QueueUserWorkItem(new WaitCallback(VerifyInThread), element);
        }
        private void VerifyInThread(object obj)
        {
            VerifyCacheFileElement element = (VerifyCacheFileElement)obj;
            element.Result = CacheHelper.VerifyingCacheFile(element, _cache.BootVerifyLevel);
            _syncContext.Post(VerifyCallback, element);
        }
        private void VerifyCallback(object obj)
        {
            VerifyCacheFileElement element = (VerifyCacheFileElement)obj;
            _verifyingList.Remove(element);

            if (element.Result == EVerifyResult.Succeed)
            {
                _succeedCount++;
                var wrapper = new CacheManager.RecordWrapper(element.InfoFilePath, element.DataFilePath, element.DataFileCRC, element.DataFileSize);
                _cache.Record(element.CacheGUID, wrapper);
            }
            else
            {
                _failedCount++;

                YooLogger.Warning($"Failed verify file and delete files : {element.FileRootPath}");
                element.DeleteFiles();
            }
        }
    }

    /// <summary>
    /// 本地缓存文件验证（非线程版）
    /// </summary>
    internal class VerifyCacheFilesWithoutThreadOperation : VerifyCacheFilesOperation
    {
        private enum ESteps
        {
            None,
            InitVerify,
            UpdateVerify,
            Done,
        }

        private readonly CacheManager _cache;
        private List<VerifyCacheFileElement> _waitingList;
        private List<VerifyCacheFileElement> _verifyingList;
        private int _verifyMaxNum;
        private int _verifyTotalCount;
        private float _verifyStartTime;
        private int _succeedCount;
        private int _failedCount;
        private ESteps _steps = ESteps.None;

        public VerifyCacheFilesWithoutThreadOperation(CacheManager cache, List<VerifyCacheFileElement> elements)
        {
            _cache = cache;
            _waitingList = elements;
        }
        internal override void InternalOnStart()
        {
            _steps = ESteps.InitVerify;
            _verifyStartTime = UnityEngine.Time.realtimeSinceStartup;
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.InitVerify)
            {
                int fileCount = _waitingList.Count;

                // 设置同时验证的最大数
                _verifyMaxNum = fileCount;
                _verifyTotalCount = fileCount;

                _verifyingList = new List<VerifyCacheFileElement>(_verifyMaxNum);
                _steps = ESteps.UpdateVerify;
            }

            if (_steps == ESteps.UpdateVerify)
            {
                Progress = GetProgress();
                if (_waitingList.Count == 0 && _verifyingList.Count == 0)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                    float costTime = UnityEngine.Time.realtimeSinceStartup - _verifyStartTime;
                    YooLogger.Log($"Package verify elapsed time {costTime:f1} seconds");
                }

                for (int i = _waitingList.Count - 1; i >= 0; i--)
                {
                    if (OperationSystem.IsBusy)
                        break;

                    if (_verifyingList.Count >= _verifyMaxNum)
                        break;

                    var element = _waitingList[i];
                    BeginVerifyFileWithoutThread(element);
                    _waitingList.RemoveAt(i);
                    _verifyingList.Add(element);
                }

                // 主线程内验证，可以清空列表
                _verifyingList.Clear();
            }
        }

        private float GetProgress()
        {
            if (_verifyTotalCount == 0)
                return 1f;
            return (float)(_succeedCount + _failedCount) / _verifyTotalCount;
        }
        private void BeginVerifyFileWithoutThread(VerifyCacheFileElement element)
        {
            element.Result = CacheHelper.VerifyingCacheFile(element, _cache.BootVerifyLevel);
            if (element.Result == EVerifyResult.Succeed)
            {
                _succeedCount++;
                var wrapper = new CacheManager.RecordWrapper(element.InfoFilePath, element.DataFilePath, element.DataFileCRC, element.DataFileSize);
                _cache.Record(element.CacheGUID, wrapper);
            }
            else
            {
                _failedCount++;

                YooLogger.Warning($"Failed verify file and delete files : {element.FileRootPath}");
                element.DeleteFiles();
            }
        }
    }
}
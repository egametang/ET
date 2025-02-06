using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset
{
    internal class PackageCachingOperation : AsyncOperationBase
    {
        private enum ESteps
        {
            None,
            FindCacheFiles,
            VerifyCacheFiles,
            Done,
        }

        private readonly PersistentManager _persistent;
        private readonly CacheManager _cache;
        private FindCacheFilesOperation _findCacheFilesOp;
        private VerifyCacheFilesOperation _verifyCacheFilesOp;
        private ESteps _steps = ESteps.None;

        public PackageCachingOperation(PersistentManager persistent, CacheManager cache)
        {
            _persistent = persistent;
            _cache = cache;
        }
        internal override void InternalOnStart()
        {
            _steps = ESteps.FindCacheFiles;
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.FindCacheFiles)
            {
                if (_findCacheFilesOp == null)
                {
                    _findCacheFilesOp = new FindCacheFilesOperation(_persistent, _cache);
                    OperationSystem.StartOperation(_cache.PackageName, _findCacheFilesOp);
                }

                Progress = _findCacheFilesOp.Progress;
                if (_findCacheFilesOp.IsDone == false)
                    return;

                _steps = ESteps.VerifyCacheFiles;
            }

            if (_steps == ESteps.VerifyCacheFiles)
            {
                if (_verifyCacheFilesOp == null)
                {
                    _verifyCacheFilesOp = VerifyCacheFilesOperation.CreateOperation(_cache, _findCacheFilesOp.VerifyElements);
                    OperationSystem.StartOperation(_cache.PackageName, _verifyCacheFilesOp);
                }

                Progress = _verifyCacheFilesOp.Progress;
                if (_verifyCacheFilesOp.IsDone == false)
                    return;

                // 注意：总是返回成功
                _steps = ESteps.Done;
                Status = EOperationStatus.Succeed;

                int totalCount = _cache.GetAllCachedFilesCount();
                YooLogger.Log($"Package '{_cache.PackageName}' cached files count : {totalCount}");
            }
        }
    }
}
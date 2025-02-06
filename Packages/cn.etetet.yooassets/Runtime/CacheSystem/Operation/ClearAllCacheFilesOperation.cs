using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace YooAsset
{
    /// <summary>
    /// 清理本地包裹所有的缓存文件
    /// </summary>
    public sealed class ClearAllCacheFilesOperation : AsyncOperationBase
    {
        private enum ESteps
        {
            None,
            GetAllCacheFiles,
            ClearAllCacheFiles,
            Done,
        }

        private readonly CacheManager _cache;
        private List<string> _allCacheGUIDs;
        private int _fileTotalCount = 0;
        private ESteps _steps = ESteps.None;

        internal ClearAllCacheFilesOperation(CacheManager cache)
        {
            _cache = cache;
        }
        internal override void InternalOnStart()
        {
            _steps = ESteps.GetAllCacheFiles;
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.GetAllCacheFiles)
            {
                _allCacheGUIDs = _cache.GetAllCachedGUIDs();
                _fileTotalCount = _allCacheGUIDs.Count;
                YooLogger.Log($"Found all cache file count : {_fileTotalCount}");
                _steps = ESteps.ClearAllCacheFiles;
            }

            if (_steps == ESteps.ClearAllCacheFiles)
            {
                for (int i = _allCacheGUIDs.Count - 1; i >= 0; i--)
                {
                    string cacheGUID = _allCacheGUIDs[i];
                    _cache.Discard(cacheGUID);
                    _allCacheGUIDs.RemoveAt(i);

                    if (OperationSystem.IsBusy)
                        break;
                }

                if (_fileTotalCount == 0)
                    Progress = 1.0f;
                else
                    Progress = 1.0f - (_allCacheGUIDs.Count / _fileTotalCount);

                if (_allCacheGUIDs.Count == 0)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                }
            }
        }
    }
}
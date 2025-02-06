using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace YooAsset
{
    /// <summary>
    /// 清理本地包裹未使用的缓存文件
    /// </summary>
    public sealed class ClearUnusedCacheFilesOperation : AsyncOperationBase
    {
        private enum ESteps
        {
            None,
            GetUnusedCacheFiles,
            ClearUnusedCacheFiles,
            Done,
        }

        private readonly ResourcePackage _package;
        private readonly CacheManager _cache;
        private List<string> _unusedCacheGUIDs;
        private int _unusedFileTotalCount = 0;
        private ESteps _steps = ESteps.None;

        internal ClearUnusedCacheFilesOperation(ResourcePackage package, CacheManager cache)
        {
            _package = package;
            _cache = cache;
        }
        internal override void InternalOnStart()
        {
            _steps = ESteps.GetUnusedCacheFiles;
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.GetUnusedCacheFiles)
            {
                _unusedCacheGUIDs = GetUnusedCacheGUIDs();
                _unusedFileTotalCount = _unusedCacheGUIDs.Count;
                YooLogger.Log($"Found unused cache file count : {_unusedFileTotalCount}");
                _steps = ESteps.ClearUnusedCacheFiles;
            }

            if (_steps == ESteps.ClearUnusedCacheFiles)
            {
                for (int i = _unusedCacheGUIDs.Count - 1; i >= 0; i--)
                {
                    string cacheGUID = _unusedCacheGUIDs[i];
                    _cache.Discard(cacheGUID);
                    _unusedCacheGUIDs.RemoveAt(i);

                    if (OperationSystem.IsBusy)
                        break;
                }

                if (_unusedFileTotalCount == 0)
                    Progress = 1.0f;
                else
                    Progress = 1.0f - (_unusedCacheGUIDs.Count / _unusedFileTotalCount);

                if (_unusedCacheGUIDs.Count == 0)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                }
            }
        }

        private List<string> GetUnusedCacheGUIDs()
        {
            var allCacheGUIDs = _cache.GetAllCachedGUIDs();
            List<string> result = new List<string>(allCacheGUIDs.Count);
            foreach (var cacheGUID in allCacheGUIDs)
            {
                if (_package.IsIncludeBundleFile(cacheGUID) == false)
                {
                    result.Add(cacheGUID);
                }
            }
            return result;
        }
    }
}
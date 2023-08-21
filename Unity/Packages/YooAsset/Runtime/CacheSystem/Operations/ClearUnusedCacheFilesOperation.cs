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
		private List<string> _unusedCacheGUIDs;
		private int _unusedFileTotalCount = 0;
		private ESteps _steps = ESteps.None;

		internal ClearUnusedCacheFilesOperation(ResourcePackage package)
		{
			_package = package;
		}
		internal override void Start()
		{
			_steps = ESteps.GetUnusedCacheFiles;
		}
		internal override void Update()
		{
			if (_steps == ESteps.None || _steps == ESteps.Done)
				return;

			if (_steps == ESteps.GetUnusedCacheFiles)
			{
				_unusedCacheGUIDs = CacheSystem.GetUnusedCacheGUIDs(_package);
				_unusedFileTotalCount = _unusedCacheGUIDs.Count;
				YooLogger.Log($"Found unused cache file count : {_unusedFileTotalCount}");
				_steps = ESteps.ClearUnusedCacheFiles;
			}

			if (_steps == ESteps.ClearUnusedCacheFiles)
			{
				for (int i = _unusedCacheGUIDs.Count - 1; i >= 0; i--)
				{
					string cacheGUID = _unusedCacheGUIDs[i];
					CacheSystem.DiscardFile(_package.PackageName, cacheGUID);
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
	}
}
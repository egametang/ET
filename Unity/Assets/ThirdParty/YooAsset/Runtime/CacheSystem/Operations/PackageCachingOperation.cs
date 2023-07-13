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

		private readonly string _packageName;
		private FindCacheFilesOperation _findCacheFilesOp;
		private VerifyCacheFilesOperation _verifyCacheFilesOp;
		private ESteps _steps = ESteps.None;

		public PackageCachingOperation(string packageName)
		{
			_packageName = packageName;
		}
		internal override void Start()
		{
			_steps = ESteps.FindCacheFiles;
		}
		internal override void Update()
		{
			if (_steps == ESteps.None || _steps == ESteps.Done)
				return;

			if (_steps == ESteps.FindCacheFiles)
			{
				if (_findCacheFilesOp == null)
				{
					_findCacheFilesOp = new FindCacheFilesOperation(_packageName);
					OperationSystem.StartOperation(_findCacheFilesOp);
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
					_verifyCacheFilesOp = VerifyCacheFilesOperation.CreateOperation(_findCacheFilesOp.VerifyElements);
					OperationSystem.StartOperation(_verifyCacheFilesOp);
				}

				Progress = _verifyCacheFilesOp.Progress;
				if (_verifyCacheFilesOp.IsDone == false)
					return;

				// 注意：总是返回成功
				_steps = ESteps.Done;
				Status = EOperationStatus.Succeed;

				int totalCount = CacheSystem.GetCachedFilesCount(_packageName);
				YooLogger.Log($"Package '{_packageName}' cached files count : {totalCount}");
			}
		}
	}
}
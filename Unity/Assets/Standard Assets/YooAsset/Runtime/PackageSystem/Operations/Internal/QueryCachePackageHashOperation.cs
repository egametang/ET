﻿using System.IO;

namespace YooAsset
{
	internal class QueryCachePackageHashOperation : AsyncOperationBase
	{
		private enum ESteps
		{
			None,
			LoadCachePackageHashFile,
			Done,
		}

		private readonly string _packageName;
		private readonly string _packageVersion;
		private ESteps _steps = ESteps.None;

		/// <summary>
		/// 包裹哈希值
		/// </summary>
		public string PackageHash { private set; get; }


		public QueryCachePackageHashOperation(string packageName, string packageVersion)
		{
			_packageName = packageName;
			_packageVersion = packageVersion;
		}
		internal override void Start()
		{
			_steps = ESteps.LoadCachePackageHashFile;
		}
		internal override void Update()
		{
			if (_steps == ESteps.None || _steps == ESteps.Done)
				return;

			if (_steps == ESteps.LoadCachePackageHashFile)
			{
				string filePath = PersistentTools.GetCachePackageHashFilePath(_packageName, _packageVersion);
				if (File.Exists(filePath) == false)
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = $"Cache package hash file not found : {filePath}";
					return;
				}

				PackageHash = FileUtility.ReadAllText(filePath);
				if (string.IsNullOrEmpty(PackageHash))
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = $"Cache package hash file content is empty !";
				}
				else
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Succeed;
				}
			}
		}
	}
}
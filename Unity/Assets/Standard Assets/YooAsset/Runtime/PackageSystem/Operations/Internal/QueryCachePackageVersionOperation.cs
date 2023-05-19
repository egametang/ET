using System.IO;

namespace YooAsset
{
	internal class QueryCachePackageVersionOperation : AsyncOperationBase
	{
		private enum ESteps
		{
			None,
			LoadCachePackageVersionFile,
			Done,
		}

		private readonly string _packageName;
		private ESteps _steps = ESteps.None;

		/// <summary>
		/// 包裹版本
		/// </summary>
		public string PackageVersion { private set; get; }


		public QueryCachePackageVersionOperation(string packageName)
		{
			_packageName = packageName;
		}
		internal override void Start()
		{
			_steps = ESteps.LoadCachePackageVersionFile;
		}
		internal override void Update()
		{
			if (_steps == ESteps.None || _steps == ESteps.Done)
				return;

			if (_steps == ESteps.LoadCachePackageVersionFile)
			{
				string filePath = PersistentTools.GetCachePackageVersionFilePath(_packageName);
				if (File.Exists(filePath) == false)
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = $"Cache package version file not found : {filePath}";
					return;
				}

				PackageVersion = FileUtility.ReadAllText(filePath);
				if (string.IsNullOrEmpty(PackageVersion))
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = $"Cache package version file content is empty !";
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
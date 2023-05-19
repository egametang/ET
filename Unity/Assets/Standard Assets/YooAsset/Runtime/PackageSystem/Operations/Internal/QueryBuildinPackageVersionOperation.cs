
namespace YooAsset
{
	internal class QueryBuildinPackageVersionOperation : AsyncOperationBase
	{
		private enum ESteps
		{
			None,
			LoadBuildinPackageVersionFile,
			Done,
		}

		private readonly string _packageName;
		private UnityWebDataRequester _downloader;
		private ESteps _steps = ESteps.None;

		/// <summary>
		/// 包裹版本
		/// </summary>
		public string PackageVersion { private set; get; }


		public QueryBuildinPackageVersionOperation(string packageName)
		{
			_packageName = packageName;
		}
		internal override void Start()
		{
			_steps = ESteps.LoadBuildinPackageVersionFile;
		}
		internal override void Update()
		{
			if (_steps == ESteps.None || _steps == ESteps.Done)
				return;

			if (_steps == ESteps.LoadBuildinPackageVersionFile)
			{
				if (_downloader == null)
				{
					string fileName = YooAssetSettingsData.GetPackageVersionFileName(_packageName);
					string filePath = PersistentTools.MakeStreamingLoadPath(fileName);
					string url = PersistentTools.ConvertToWWWPath(filePath);
					_downloader = new UnityWebDataRequester();
					_downloader.SendRequest(url);
				}

				if (_downloader.IsDone() == false)
					return;

				if (_downloader.HasError())
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = _downloader.GetError();
				}
				else
				{
					PackageVersion = _downloader.GetText();
					if (string.IsNullOrEmpty(PackageVersion))
					{
						_steps = ESteps.Done;
						Status = EOperationStatus.Failed;
						Error = $"Buildin package version file content is empty !";
					}
					else
					{
						_steps = ESteps.Done;
						Status = EOperationStatus.Succeed;
					}
				}

				_downloader.Dispose();
			}
		}
	}
}
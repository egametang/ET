using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset
{
	internal class DeserializeManifestOperation : AsyncOperationBase
	{
		private enum ESteps
		{
			None,
			DeserializeFileHeader,
			PrepareAssetList,
			DeserializeAssetList,
			PrepareBundleList,
			DeserializeBundleList,
			Done,
		}
	
		private readonly BufferReader _buffer;
		private int _packageAssetCount;
		private int _packageBundleCount;
		private int _progressTotalValue;
		private ESteps _steps = ESteps.None;

		/// <summary>
		/// 解析的清单实例
		/// </summary>
		public PackageManifest Manifest { private set; get; }

		public DeserializeManifestOperation(byte[] binaryData)
		{
			_buffer = new BufferReader(binaryData);
		}
		internal override void Start()
		{
			_steps = ESteps.DeserializeFileHeader;
		}
		internal override void Update()
		{
			if (_steps == ESteps.None || _steps == ESteps.Done)
				return;

			try
			{
				if (_steps == ESteps.DeserializeFileHeader)
				{
					if (_buffer.IsValid == false)
					{
						_steps = ESteps.Done;
						Status = EOperationStatus.Failed;
						Error = "Buffer is invalid !";
						return;
					}

					// 读取文件标记
					uint fileSign = _buffer.ReadUInt32();
					if (fileSign != YooAssetSettings.ManifestFileSign)
					{
						_steps = ESteps.Done;
						Status = EOperationStatus.Failed;
						Error = "The manifest file format is invalid !";
						return;
					}

					// 读取文件版本
					string fileVersion = _buffer.ReadUTF8();
					if (fileVersion != YooAssetSettings.ManifestFileVersion)
					{
						_steps = ESteps.Done;
						Status = EOperationStatus.Failed;
						Error = $"The manifest file version are not compatible : {fileVersion} != {YooAssetSettings.ManifestFileVersion}";
						return;
					}

					// 读取文件头信息
					Manifest = new PackageManifest();
					Manifest.FileVersion = fileVersion;
					Manifest.EnableAddressable = _buffer.ReadBool();
					Manifest.OutputNameStyle = _buffer.ReadInt32();
					Manifest.PackageName = _buffer.ReadUTF8();
					Manifest.PackageVersion = _buffer.ReadUTF8();

					_steps = ESteps.PrepareAssetList;
				}

				if (_steps == ESteps.PrepareAssetList)
				{
					_packageAssetCount = _buffer.ReadInt32();
					Manifest.AssetList = new List<PackageAsset>(_packageAssetCount);
					Manifest.AssetDic = new Dictionary<string, PackageAsset>(_packageAssetCount);
					_progressTotalValue = _packageAssetCount;
					_steps = ESteps.DeserializeAssetList;
				}
				if (_steps == ESteps.DeserializeAssetList)
				{
					while (_packageAssetCount > 0)
					{
						var packageAsset = new PackageAsset();
						packageAsset.Address = _buffer.ReadUTF8();
						packageAsset.AssetPath = _buffer.ReadUTF8();
						packageAsset.AssetTags = _buffer.ReadUTF8Array();
						packageAsset.BundleID = _buffer.ReadInt32();
						packageAsset.DependIDs = _buffer.ReadInt32Array();
						Manifest.AssetList.Add(packageAsset);

						// 注意：我们不允许原始路径存在重名
						string assetPath = packageAsset.AssetPath;
						if (Manifest.AssetDic.ContainsKey(assetPath))
							throw new System.Exception($"AssetPath have existed : {assetPath}");
						else
							Manifest.AssetDic.Add(assetPath, packageAsset);

						_packageAssetCount--;
						Progress = 1f - _packageAssetCount / _progressTotalValue;
						if (OperationSystem.IsBusy)
							break;
					}

					if (_packageAssetCount <= 0)
					{
						_steps = ESteps.PrepareBundleList;
					}
				}

				if (_steps == ESteps.PrepareBundleList)
				{
					_packageBundleCount = _buffer.ReadInt32();
					Manifest.BundleList = new List<PackageBundle>(_packageBundleCount);
					Manifest.BundleDic = new Dictionary<string, PackageBundle>(_packageBundleCount);
					_progressTotalValue = _packageBundleCount;
					_steps = ESteps.DeserializeBundleList;
				}
				if (_steps == ESteps.DeserializeBundleList)
				{
					while (_packageBundleCount > 0)
					{
						var packageBundle = new PackageBundle();
						packageBundle.BundleName = _buffer.ReadUTF8();
						packageBundle.FileHash = _buffer.ReadUTF8();
						packageBundle.FileCRC = _buffer.ReadUTF8();
						packageBundle.FileSize = _buffer.ReadInt64();
						packageBundle.IsRawFile = _buffer.ReadBool();
						packageBundle.LoadMethod = _buffer.ReadByte();
						packageBundle.Tags = _buffer.ReadUTF8Array();
						packageBundle.ReferenceIDs = _buffer.ReadInt32Array();
						Manifest.BundleList.Add(packageBundle);

						packageBundle.ParseBundle(Manifest.PackageName, Manifest.OutputNameStyle);
						Manifest.BundleDic.Add(packageBundle.BundleName, packageBundle);

						_packageBundleCount--;
						Progress = 1f - _packageBundleCount / _progressTotalValue;
						if (OperationSystem.IsBusy)
							break;
					}

					if (_packageBundleCount <= 0)
					{
						_steps = ESteps.Done;
						Status = EOperationStatus.Succeed;
					}
				}
			}
			catch (System.Exception e)
			{
				Manifest = null;
				_steps = ESteps.Done;
				Status = EOperationStatus.Failed;
				Error = e.Message;
			}
		}
	}
}
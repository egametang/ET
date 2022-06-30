
namespace YooAsset
{
	public class AssetInfo
	{
		private readonly PatchAsset _patchAsset;
		private string _providerGUID;

		/// <summary>
		/// 资源提供者唯一标识符
		/// </summary>
		internal string ProviderGUID
		{
			get
			{
				if (string.IsNullOrEmpty(_providerGUID) == false)
					return _providerGUID;

				if (AssetType == null)
					_providerGUID = $"{AssetPath}[null]";
				else
					_providerGUID = $"{AssetPath}[{AssetType.Name}]";
				return _providerGUID;
			}
		}

		/// <summary>
		/// 身份是否无效
		/// </summary>
		internal bool IsInvalid
		{
			get
			{
				return _patchAsset == null;
			}
		}

		/// <summary>
		/// 错误信息
		/// </summary>
		internal string Error { private set; get; }

		/// <summary>
		/// 可寻址地址
		/// </summary>
		public string Address { private set; get; }

		/// <summary>
		/// 资源路径
		/// </summary>
		public string AssetPath { private set; get; }
		
		/// <summary>
		/// 资源类型
		/// </summary>
		public System.Type AssetType { private set; get; }


		// 注意：这是一个内部类，严格限制外部创建。
		private AssetInfo()
		{
		}
		internal AssetInfo(PatchAsset patchAsset, System.Type assetType)
		{
			if (patchAsset == null)
				throw new System.Exception("Should never get here !");

			_patchAsset = patchAsset;
			AssetType = assetType;
			Address = patchAsset.Address;
			AssetPath = patchAsset.AssetPath;
			Error = string.Empty;
		}
		internal AssetInfo(PatchAsset patchAsset)
		{
			if (patchAsset == null)
				throw new System.Exception("Should never get here !");

			_patchAsset = patchAsset;
			AssetType = null;
			Address = patchAsset.Address;
			AssetPath = patchAsset.AssetPath;
			Error = string.Empty;
		}
		internal AssetInfo(string error)
		{
			_patchAsset = null;
			AssetType = null;
			Address = string.Empty;
			AssetPath = string.Empty;
			Error = error;
		}
	}
}
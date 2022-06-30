using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset.Editor
{
	[Serializable]
	public class ReportBundleInfo
	{
		public class FlagsData
		{
			public bool IsEncrypted { private set; get; }
			public bool IsBuildin { private set; get; }
			public bool IsRawFile { private set; get; }
			public FlagsData(bool isEncrypted, bool isBuildin, bool isRawFile)
			{
				IsEncrypted = isEncrypted;
				IsBuildin = isBuildin;
				IsRawFile = isRawFile;
			}
		}

		private FlagsData _flagData;

		/// <summary>
		/// 资源包名称
		/// </summary>
		public string BundleName;

		/// <summary>
		/// 哈希值
		/// </summary>
		public string Hash;

		/// <summary>
		/// 文件校验码
		/// </summary>
		public string CRC;

		/// <summary>
		/// 文件大小（字节数）
		/// </summary>
		public long SizeBytes;

		/// <summary>
		/// Tags
		/// </summary>
		public string[] Tags;

		/// <summary>
		/// Flags
		/// </summary>
		public int Flags;


		/// <summary>
		/// 获取标志位的解析数据
		/// </summary>
		public FlagsData GetFlagData()
		{
			if (_flagData == null)
			{
				BitMask32 value = Flags;
				bool isEncrypted = value.Test(0);
				bool isBuildin = value.Test(1);
				bool isRawFile = value.Test(2);
				_flagData = new FlagsData(isEncrypted, isBuildin, isRawFile);
			}
			return _flagData;
		}

		/// <summary>
		/// 获取资源分类标签的字符串
		/// </summary>
		public string GetTagsString()
		{
			if (Tags != null)
				return String.Join(";", Tags);
			else
				return string.Empty;
		}

		/// <summary>
		/// 是否为原生文件
		/// </summary>
		public bool IsRawFile()
		{
			if (System.IO.Path.GetExtension(BundleName) == $".{YooAssetSettingsData.Setting.RawFileVariant}")
				return true;
			else
				return false;
		}
	}
}
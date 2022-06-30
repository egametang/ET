using System;
using System.Linq;

namespace YooAsset
{
	[Serializable]
	internal class PatchBundle
	{
		/// <summary>
		/// 资源包名称
		/// </summary>
		public string BundleName;

		/// <summary>
		/// 文件哈希值
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
		/// 资源包的分类标签
		/// </summary>
		public string[] Tags;

		/// <summary>
		/// Flags
		/// </summary>
		public int Flags;


		/// <summary>
		/// 是否为加密文件
		/// </summary>
		public bool IsEncrypted { private set; get; }

		/// <summary>
		/// 是否为内置文件
		/// </summary>
		public bool IsBuildin { private set; get; }

		/// <summary>
		/// 是否为原生文件
		/// </summary>
		public bool IsRawFile { private set; get; }



		public PatchBundle(string bundleName, string hash, string crc, long sizeBytes, string[] tags)
		{
			BundleName = bundleName;
			Hash = hash;
			CRC = crc;
			SizeBytes = sizeBytes;
			Tags = tags;
		}

		/// <summary>
		/// 设置Flags
		/// </summary>
		public void SetFlagsValue(bool isEncrypted, bool isBuildin, bool isRawFile)
		{
			IsEncrypted = isEncrypted;
			IsBuildin = isBuildin;
			IsRawFile = isRawFile;

			BitMask32 mask = new BitMask32(0);
			if (isEncrypted) mask.Open(0);
			if (isBuildin) mask.Open(1);
			if (isRawFile) mask.Open(2);
			Flags = mask;
		}

		/// <summary>
		/// 解析Flags
		/// </summary>
		public void ParseFlagsValue()
		{
			BitMask32 value = Flags;
			IsEncrypted = value.Test(0);
			IsBuildin = value.Test(1);
			IsRawFile = value.Test(2);
		}

		/// <summary>
		/// 是否包含Tag
		/// </summary>
		public bool HasTag(string[] tags)
		{
			if (tags == null || tags.Length == 0)
				return false;
			if (Tags == null || Tags.Length == 0)
				return false;

			foreach (var tag in tags)
			{
				if (Tags.Contains(tag))
					return true;
			}
			return false;
		}

		/// <summary>
		/// 是否为纯内置资源（不带任何Tag的资源）
		/// </summary>
		public bool IsPureBuildin()
		{
			if (Tags == null || Tags.Length == 0)
				return true;
			else
				return false;
		}
	}
}
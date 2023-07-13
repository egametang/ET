using System;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset.Editor
{
	[Serializable]
	public class ReportRedundancyInfo
	{
		/// <summary>
		/// 资源路径
		/// </summary>
		public string AssetPath;

		/// <summary>
		/// 资源类型
		/// </summary>
		public string AssetType;

		/// <summary>
		/// 资源GUID
		/// 说明：Meta文件记录的GUID
		/// </summary>
		public string AssetGUID;

		/// <summary>
		/// 资源文件大小
		/// </summary>
		public long FileSize;

		/// <summary>
		/// 冗余的资源包数量
		/// </summary>
		public int Number;
	}
}
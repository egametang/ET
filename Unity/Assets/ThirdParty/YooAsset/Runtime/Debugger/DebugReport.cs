using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YooAsset
{
	/// <summary>
	/// 资源系统调试信息
	/// </summary>
	[Serializable]
	internal class DebugReport
	{
		public List<DebugProviderInfo> ProviderInfos = new List<DebugProviderInfo>(1000);

		/// <summary>
		/// 游戏帧
		/// </summary>
		public int FrameCount;

		/// <summary>
		/// 资源包总数
		/// </summary>
		public int BundleCount;

		/// <summary>
		/// 资源对象总数
		/// </summary>
		public int AssetCount;


		/// <summary>
		/// 序列化
		/// </summary>
        public static byte[] Serialize(DebugReport debugReport)
        {
            return Encoding.UTF8.GetBytes(JsonUtility.ToJson(debugReport));
        }

		/// <summary>
		/// 反序列化
		/// </summary>
        public static DebugReport Deserialize(byte[] data)
        {
            return JsonUtility.FromJson<DebugReport>(Encoding.UTF8.GetString(data));
        }
    }
}
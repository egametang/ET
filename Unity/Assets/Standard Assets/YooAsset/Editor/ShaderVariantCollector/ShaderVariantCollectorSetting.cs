using UnityEngine;

namespace YooAsset.Editor
{
	[CreateAssetMenu(fileName = "ShaderVariantCollectorSetting", menuName = "YooAsset/Create ShaderVariant Collector Settings")]
	public class ShaderVariantCollectorSetting : ScriptableObject
	{
		/// <summary>
		/// 文件存储路径
		/// </summary>
		public string SavePath = "Assets/MyShaderVariants.shadervariants";

		/// <summary>
		/// 收集的包裹名称
		/// </summary>
		public string CollectPackage = string.Empty;

		/// <summary>
		/// 容器值
		/// </summary>
		public int ProcessCapacity = 1000;
	}
}
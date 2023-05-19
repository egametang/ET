
namespace YooAsset.Editor
{
	public class CollectCommand
	{
		/// <summary>
		/// 构建模式
		/// </summary>
		public EBuildMode BuildMode { private set; get; }

		/// <summary>
		/// 包裹名称
		/// </summary>
		public string PackageName { private set; get; }

		/// <summary>
		/// 是否启用可寻址资源定位
		/// </summary>
		public bool EnableAddressable { private set; get; }

		/// <summary>
		/// 资源包名唯一化
		/// </summary>
		public bool UniqueBundleName { private set; get; }

		/// <summary>
		/// 着色器统一全名称
		/// </summary>
		public string ShadersBundleName { private set; get; }


		public CollectCommand(EBuildMode buildMode, string packageName, bool enableAddressable, bool uniqueBundleName)
		{
			BuildMode = buildMode;
			PackageName = packageName;
			EnableAddressable = enableAddressable;
			UniqueBundleName = uniqueBundleName;

			// 着色器统一全名称
			var packRuleResult = DefaultPackRule.CreateShadersPackRuleResult();
			ShadersBundleName = packRuleResult.GetMainBundleName(packageName, uniqueBundleName);
		}
	}
}
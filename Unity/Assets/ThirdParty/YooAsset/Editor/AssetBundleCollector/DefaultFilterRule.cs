using UnityEngine;
using UnityEditor;
using System.IO;

namespace YooAsset.Editor
{
	/// <summary>
	/// 收集所有资源
	/// </summary>
	public class CollectAll : IFilterRule
	{
		public bool IsCollectAsset(FilterRuleData data)
		{
			return true;
		}
	}

	/// <summary>
	/// 只收集场景
	/// </summary>
	public class CollectScene : IFilterRule
	{
		public bool IsCollectAsset(FilterRuleData data)
		{
			return Path.GetExtension(data.AssetPath) == ".unity";
		}
	}
	
	/// <summary>
	/// 只收集预制体
	/// </summary>
	public class CollectPrefab : IFilterRule
	{
		public bool IsCollectAsset(FilterRuleData data)
		{
			return Path.GetExtension(data.AssetPath) == ".prefab";
		}
	}

	/// <summary>
	/// 只收集精灵类型的资源
	/// </summary>
	public class CollectSprite : IFilterRule
	{
		public bool IsCollectAsset(FilterRuleData data)
		{
			var mainAssetType = AssetDatabase.GetMainAssetTypeAtPath(data.AssetPath);
			if(mainAssetType == typeof(Texture2D))
			{
				var texImporter = AssetImporter.GetAtPath(data.AssetPath) as TextureImporter;
				if (texImporter != null && texImporter.textureType == TextureImporterType.Sprite)
					return true;
				else
					return false;
			}
			else
			{
				return false;
			}
		}
	}
}
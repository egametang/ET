using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;

namespace YooAsset.Editor
{
	[Serializable]
	public class ShaderVariantCollectionManifest
	{
		[Serializable]
		public class ShaderVariantElement
		{
			/// <summary>
			///  Pass type to use in this variant.
			/// </summary>
			public PassType PassType;

			/// <summary>
			/// Array of shader keywords to use in this variant.
			/// </summary>
			public string[] Keywords;
		}

		[Serializable]
		public class ShaderVariantInfo
		{
			/// <summary>
			/// Shader asset path in editor.
			/// </summary>
			public string AssetPath;

			/// <summary>
			/// Shader name.
			/// </summary>
			public string ShaderName;

			/// <summary>
			/// Shader variants elements list.
			/// </summary>
			public List<ShaderVariantElement> ShaderVariantElements = new List<ShaderVariantElement>(1000);
		}


		/// <summary>
		/// Number of shaders in this collection
		/// </summary>
		public int ShaderTotalCount;

		/// <summary>
		/// Number of total varians in this collection
		/// </summary>
		public int VariantTotalCount;

		/// <summary>
		/// Shader variants info list.
		/// </summary>
		public List<ShaderVariantInfo> ShaderVariantInfos = new List<ShaderVariantInfo>(1000);

		/// <summary>
		/// 添加着色器变种信息
		/// </summary>
		public void AddShaderVariant(string assetPath, string shaderName, PassType passType, string[] keywords)
		{
			var info = GetOrCreateShaderVariantInfo(assetPath, shaderName);
			ShaderVariantElement element = new ShaderVariantElement();
			element.PassType = passType;
			element.Keywords = keywords;
			info.ShaderVariantElements.Add(element);
		}
		private ShaderVariantInfo GetOrCreateShaderVariantInfo(string assetPath, string shaderName)
		{
			var selectList = ShaderVariantInfos.Where(t => t.ShaderName == shaderName && t.AssetPath == assetPath).ToList();
			if (selectList.Count == 0)
			{
				ShaderVariantInfo newInfo = new ShaderVariantInfo();
				newInfo.AssetPath = assetPath;
				newInfo.ShaderName = shaderName;
				ShaderVariantInfos.Add(newInfo);
				return newInfo;
			}

			if (selectList.Count != 1)
				throw new Exception("Should never get here !");

			return selectList[0];
		}


		/// <summary>
		/// 解析SVC文件并将数据写入到清单
		/// </summary>
		public static ShaderVariantCollectionManifest Extract(ShaderVariantCollection svc)
		{
			var manifest = new ShaderVariantCollectionManifest();
			manifest.ShaderTotalCount = ShaderVariantCollectionHelper.GetCurrentShaderVariantCollectionShaderCount();
			manifest.VariantTotalCount = ShaderVariantCollectionHelper.GetCurrentShaderVariantCollectionVariantCount();

			using (var so = new SerializedObject(svc))
			{
				var shaderArray = so.FindProperty("m_Shaders.Array");
				if (shaderArray != null && shaderArray.isArray)
				{
					for (int i = 0; i < shaderArray.arraySize; ++i)
					{
						var shaderRef = shaderArray.FindPropertyRelative($"data[{i}].first");
						var shaderVariantsArray = shaderArray.FindPropertyRelative($"data[{i}].second.variants");
						if (shaderRef != null && shaderRef.propertyType == SerializedPropertyType.ObjectReference && shaderVariantsArray != null && shaderVariantsArray.isArray)
						{
							var shader = shaderRef.objectReferenceValue as Shader;
							if (shader == null)
							{
								throw new Exception("Invalid shader in ShaderVariantCollection file.");
							}

							string shaderAssetPath = AssetDatabase.GetAssetPath(shader);
							string shaderName = shader.name;

							// 添加变种信息
							for (int j = 0; j < shaderVariantsArray.arraySize; ++j)
							{
								var propKeywords = shaderVariantsArray.FindPropertyRelative($"Array.data[{j}].keywords");
								var propPassType = shaderVariantsArray.FindPropertyRelative($"Array.data[{j}].passType");
								if (propKeywords != null && propPassType != null && propKeywords.propertyType == SerializedPropertyType.String)
								{
									string[] keywords = propKeywords.stringValue.Split(' ');
									PassType pathType = (PassType)propPassType.intValue;
									manifest.AddShaderVariant(shaderAssetPath, shaderName, pathType, keywords);
								}
							}
						}
					}
				}
			}

			return manifest;
		}
	}
}
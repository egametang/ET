using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;

namespace YooAsset.Editor
{
	public static class ShaderVariantCollectionHelper
	{
		[Serializable]
		public class ShaderVariantWrapper
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
			///  Pass type to use in this variant.
			/// </summary>
			public PassType PassType;

			/// <summary>
			/// Array of shader keywords to use in this variant.
			/// </summary>
			public string[] Keywords;

			public ShaderVariantWrapper(string assetPath, string shaderName, PassType passType, params string[] keywords)
			{
				AssetPath = assetPath;
				ShaderName = shaderName;
				PassType = passType;
				Keywords = keywords;
			}
		}

		[Serializable]
		public class ShaderVariantCollectionWrapper
		{
			/// <summary>
			/// Number of shaders in this collection
			/// </summary>
			public int ShaderCount;

			/// <summary>
			/// Number of total varians in this collection
			/// </summary>
			public int VariantCount;

			/// <summary>
			/// Shader variants list.
			/// </summary>
			public List<ShaderVariantWrapper> ShaderVariants = new List<ShaderVariantWrapper>(1000);

			public void Add(ShaderVariantWrapper variant)
			{
				ShaderVariants.Add(variant);
			}
		}


		public static ShaderVariantCollectionWrapper Extract(ShaderVariantCollection svc)
		{
			var result = new ShaderVariantCollectionWrapper();
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
									result.Add(new ShaderVariantWrapper(shaderAssetPath, shaderName, pathType, keywords));
								}
							}
						}
					}
				}
			}
			return result;
		}
	}
}
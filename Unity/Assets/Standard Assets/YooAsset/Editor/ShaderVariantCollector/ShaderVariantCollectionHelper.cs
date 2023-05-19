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
		public static void ClearCurrentShaderVariantCollection()
		{
			EditorTools.InvokeNonPublicStaticMethod(typeof(ShaderUtil), "ClearCurrentShaderVariantCollection");
		}
		public static void SaveCurrentShaderVariantCollection(string savePath)
		{
			EditorTools.InvokeNonPublicStaticMethod(typeof(ShaderUtil), "SaveCurrentShaderVariantCollection", savePath);
		}
		public static int GetCurrentShaderVariantCollectionShaderCount()
		{
			return (int)EditorTools.InvokeNonPublicStaticMethod(typeof(ShaderUtil), "GetCurrentShaderVariantCollectionShaderCount");
		}
		public static int GetCurrentShaderVariantCollectionVariantCount()
		{
			return (int)EditorTools.InvokeNonPublicStaticMethod(typeof(ShaderUtil), "GetCurrentShaderVariantCollectionVariantCount");
		}

		/// <summary>
		/// 获取着色器的变种总数量
		/// </summary>
		public static string GetShaderVariantCount(string assetPath)
		{
			Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(assetPath);
			var variantCount = EditorTools.InvokeNonPublicStaticMethod(typeof(ShaderUtil), "GetVariantCount", shader, true);
			return variantCount.ToString();
		}
	}
}
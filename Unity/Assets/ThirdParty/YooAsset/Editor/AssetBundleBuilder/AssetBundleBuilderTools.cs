using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

namespace YooAsset.Editor
{
	public static class AssetBundleBuilderTools
	{
		/// <summary>
		/// 检测所有损坏的预制体文件
		/// </summary>
		public static void CheckCorruptionPrefab(List<string> searchDirectorys)
		{
			if (searchDirectorys.Count == 0)
				throw new Exception("路径列表不能为空！");

			// 获取所有资源列表
			int checkCount = 0;
			int invalidCount = 0;
			string[] findAssets = EditorTools.FindAssets(EAssetSearchType.Prefab, searchDirectorys.ToArray());
			foreach (string assetPath in findAssets)
			{
				UnityEngine.Object prefab = AssetDatabase.LoadAssetAtPath(assetPath, typeof(UnityEngine.Object));
				if (prefab == null)
				{
					invalidCount++;
					Debug.LogError($"发现损坏预制件：{assetPath}");
				}
				EditorTools.DisplayProgressBar("检测预制件文件是否损坏", ++checkCount, findAssets.Length);
			}
			EditorTools.ClearProgressBar();

			if (invalidCount == 0)
				Debug.Log($"没有发现损坏预制件");
		}

		/// <summary>
		/// 检测所有动画控制器的冗余状态
		/// </summary>
		public static void FindRedundantAnimationState(List<string> searchDirectorys)
		{
			if (searchDirectorys.Count == 0)
				throw new Exception("路径列表不能为空！");

			// 获取所有资源列表
			int checkCount = 0;
			int findCount = 0;
			string[] findAssets = EditorTools.FindAssets(EAssetSearchType.RuntimeAnimatorController, searchDirectorys.ToArray());
			foreach (string assetPath in findAssets)
			{
				AnimatorController animator= AssetDatabase.LoadAssetAtPath<AnimatorController>(assetPath);
				if (FindRedundantAnimationState(animator))
				{
					findCount++;
					Debug.LogWarning($"发现冗余的动画控制器：{assetPath}");
				}
				EditorTools.DisplayProgressBar("检测冗余的动画控制器", ++checkCount, findAssets.Length);
			}
			EditorTools.ClearProgressBar();

			if (findCount == 0)
				Debug.Log($"没有发现冗余的动画控制器");
			else
				AssetDatabase.SaveAssets();
		}

		/// <summary>
		/// 清理所有材质球的冗余属性
		/// </summary>
		public static void ClearMaterialUnusedProperty(List<string> searchDirectorys)
		{
			if (searchDirectorys.Count == 0)
				throw new Exception("路径列表不能为空！");

			// 获取所有资源列表
			int checkCount = 0;
			int removedCount = 0;
			string[] findAssets = EditorTools.FindAssets(EAssetSearchType.Material, searchDirectorys.ToArray());
			foreach (string assetPath in findAssets)
			{
				Material mat = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
				if (ClearMaterialUnusedProperty(mat))
				{
					removedCount++;
					Debug.LogWarning($"材质球已被处理：{assetPath}");
				}
				EditorTools.DisplayProgressBar("清理冗余的材质球", ++checkCount, findAssets.Length);
			}
			EditorTools.ClearProgressBar();

			if (removedCount == 0)
				Debug.Log($"没有发现冗余的材质球");
			else
				AssetDatabase.SaveAssets();
		}


		/// <summary>
		/// 清理无用的材质球属性
		/// </summary>
		private static bool ClearMaterialUnusedProperty(Material mat)
		{
			bool removeUnused = false;
			SerializedObject so = new SerializedObject(mat);
			SerializedProperty sp = so.FindProperty("m_SavedProperties");

			sp.Next(true);
			do
			{
				if (sp.isArray == false)
					continue;

				for (int i = sp.arraySize - 1; i >= 0; --i)
				{
					var p1 = sp.GetArrayElementAtIndex(i);
					if (p1.isArray)
					{
						for (int ii = p1.arraySize - 1; ii >= 0; --ii)
						{
							var p2 = p1.GetArrayElementAtIndex(ii);
							var val = p2.FindPropertyRelative("first");
							if (mat.HasProperty(val.stringValue) == false)
							{
								Debug.Log($"Material {mat.name} remove unused property : {val.stringValue}");
								p1.DeleteArrayElementAtIndex(ii);
								removeUnused = true;
							}
						}
					}
					else
					{
						var val = p1.FindPropertyRelative("first");
						if (mat.HasProperty(val.stringValue) == false)
						{
							Debug.Log($"Material {mat.name} remove unused property : {val.stringValue}");
							sp.DeleteArrayElementAtIndex(i);
							removeUnused = true;
						}
					}
				}
			}
			while (sp.Next(false));
			so.ApplyModifiedProperties();
			return removeUnused;
		}

		/// <summary>
		/// 查找动画控制器里冗余的动画状态机
		/// </summary>
		private static bool FindRedundantAnimationState(AnimatorController animatorController)
		{
			if (animatorController == null)
				return false;

			string assetPath = AssetDatabase.GetAssetPath(animatorController);

			// 查找使用的状态机名称
			List<string> usedStateNames = new List<string>();
			foreach (var layer in animatorController.layers)
			{
				foreach (var state in layer.stateMachine.states)
				{
					usedStateNames.Add(state.state.name);
				}
			}

			List<string> allLines = new List<string>();
			List<int> stateIndexList = new List<int>();
			using (StreamReader reader = File.OpenText(assetPath))
			{
				string content;
				while (null != (content = reader.ReadLine()))
				{
					allLines.Add(content);
					if (content.StartsWith("AnimatorState:"))
					{
						stateIndexList.Add(allLines.Count - 1);
					}
				}
			}

			List<string> allStateNames = new List<string>();
			foreach (var index in stateIndexList)
			{
				for (int i = index; i < allLines.Count; i++)
				{
					string content = allLines[i];
					content = content.Trim();
					if (content.StartsWith("m_Name"))
					{
						string[] splits = content.Split(':');
						string name = splits[1].TrimStart(' '); //移除前面的空格
						allStateNames.Add(name);
						break;
					}
				}
			}

			bool foundRedundantState = false;
			foreach (var stateName in allStateNames)
			{
				if (usedStateNames.Contains(stateName) == false)
				{
					Debug.LogWarning($"发现冗余的动画文件:{assetPath}={stateName}");
					foundRedundantState = true;
				}
			}
			return foundRedundantState;
		}
	}
}
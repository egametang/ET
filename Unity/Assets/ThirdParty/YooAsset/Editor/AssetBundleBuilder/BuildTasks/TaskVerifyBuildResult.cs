using System;
using System.Linq;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace YooAsset.Editor
{
	[TaskAttribute("验证构建结果")]
	public class TaskVerifyBuildResult : IBuildTask
	{
		void IBuildTask.Run(BuildContext context)
		{
			var buildParametersContext = context.GetContextObject<AssetBundleBuilder.BuildParametersContext>();
			
			// 模拟构建模式下跳过验证
			if (buildParametersContext.Parameters.BuildMode == EBuildMode.SimulateBuild)
				return;

			// 验证构建结果
			if (buildParametersContext.Parameters.VerifyBuildingResult)
			{
				var unityManifestContext = context.GetContextObject<TaskBuilding.UnityManifestContext>();
				VerifyingBuildingResult(context, unityManifestContext.UnityManifest);
			}
		}

		/// <summary>
		/// 验证构建结果
		/// </summary>
		private void VerifyingBuildingResult(BuildContext context, AssetBundleManifest unityManifest)
		{
			var buildParameters = context.GetContextObject<AssetBundleBuilder.BuildParametersContext>();
			var buildMapContext = context.GetContextObject<BuildMapContext>();
			string[] buildedBundles = unityManifest.GetAllAssetBundles();

			// 1. 过滤掉原生Bundle
			List<BuildBundleInfo> expectBundles = new List<BuildBundleInfo>(buildedBundles.Length);
			foreach(var bundleInfo in buildMapContext.BundleInfos)
			{
				if (bundleInfo.IsRawFile == false)
					expectBundles.Add(bundleInfo);
			}

			// 2. 验证数量		
			if (buildedBundles.Length != expectBundles.Count)
			{
				Debug.LogWarning($"构建过程中可能存在无效的资源，导致和预期构建的Bundle数量不一致！");
			}

			// 3. 正向验证Bundle
			foreach (var bundleName in buildedBundles)
			{
				if (buildMapContext.IsContainsBundle(bundleName) == false)
				{
					throw new Exception($"Should never get here !");
				}
			}

			// 4. 反向验证Bundle
			bool isPass = true;
			foreach (var expectBundle in expectBundles)
			{
				bool isMatch = false;
				foreach (var buildedBundle in buildedBundles)
				{
					if (buildedBundle == expectBundle.BundleName)
					{
						isMatch = true;
						break;
					}
				}
				if (isMatch == false)
				{
					isPass = false;
					Debug.LogWarning($"没有找到预期构建的Bundle文件 : {expectBundle.BundleName}");
				}
			}
			if(isPass == false)
			{
				throw new Exception("构建结果验证没有通过，请参考警告日志！");
			}

			// 5. 验证Asset
			var buildMode = buildParameters.Parameters.BuildMode;
			if (buildMode == EBuildMode.ForceRebuild || buildMode == EBuildMode.IncrementalBuild)
			{
				int progressValue = 0;
				foreach (var buildedBundle in buildedBundles)
				{
					string filePath = $"{buildParameters.PipelineOutputDirectory}/{buildedBundle}";
					string[] allBuildinAssetPaths = GetAssetBundleAllAssets(filePath);
					string[] expectBuildinAssetPaths = buildMapContext.GetBuildinAssetPaths(buildedBundle);
					if (expectBuildinAssetPaths.Length != allBuildinAssetPaths.Length)
					{
						Debug.LogWarning($"构建的Bundle文件内的资源对象数量和预期不匹配 : {buildedBundle}");
						isPass = false;
						continue;
					}

					foreach (var buildinAssetPath in allBuildinAssetPaths)
					{
						var guid = AssetDatabase.AssetPathToGUID(buildinAssetPath);
						if (string.IsNullOrEmpty(guid))
						{
							Debug.LogWarning($"无效的资源路径，请检查路径是否带有特殊符号或中文：{buildinAssetPath}");
							isPass = false;
							continue;
						}

						bool isMatch = false;
						foreach (var exceptBuildAssetPath in expectBuildinAssetPaths)
						{
							var guidExcept = AssetDatabase.AssetPathToGUID(exceptBuildAssetPath);
							if (guid == guidExcept)
							{
								isMatch = true;
								break;
							}
						}
						if (isMatch == false)
						{
							Debug.LogWarning($"在构建的Bundle文件里发现了没有匹配的资源对象：{buildinAssetPath}");
							isPass = false;
							continue;
						}
					}

					EditorTools.DisplayProgressBar("验证构建结果", ++progressValue, buildedBundles.Length);
				}
				EditorTools.ClearProgressBar();
				if (isPass == false)
				{
					throw new Exception("构建结果验证没有通过，请参考警告日志！");
				}
			}

			// 卸载所有加载的Bundle
			BuildRunner.Log("构建结果验证成功！");
		}

		/// <summary>
		/// 解析.manifest文件并获取资源列表
		/// </summary>
		private string[] GetAssetBundleAllAssets(string filePath)
		{
			string manifestFilePath = $"{filePath}.manifest";
			List<string> assetLines = new List<string>();
			using (StreamReader reader = File.OpenText(manifestFilePath))
			{
				string content;
				bool findTarget = false;
				while (null != (content = reader.ReadLine()))
				{
					if (content.StartsWith("Dependencies:"))
						break;
					if (findTarget == false && content.StartsWith("Assets:"))
						findTarget = true;
					if (findTarget)
					{
						if (content.StartsWith("- "))
						{
							string assetPath = content.TrimStart("- ".ToCharArray());
							assetLines.Add(assetPath);
						}
					}
				}
			}
			return assetLines.ToArray();
		}
	}
}
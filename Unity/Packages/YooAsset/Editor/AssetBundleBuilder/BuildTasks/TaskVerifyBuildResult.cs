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
			var buildParametersContext = context.GetContextObject<BuildParametersContext>();

			// 模拟构建模式下跳过验证
			if (buildParametersContext.Parameters.BuildMode == EBuildMode.SimulateBuild)
				return;

			// 验证构建结果
			if (buildParametersContext.Parameters.VerifyBuildingResult)
			{
				var buildResultContext = context.GetContextObject<TaskBuilding.BuildResultContext>();
				VerifyingBuildingResult(context, buildResultContext.UnityManifest);
			}
		}

		/// <summary>
		/// 验证构建结果
		/// </summary>
		private void VerifyingBuildingResult(BuildContext context, AssetBundleManifest unityManifest)
		{
			var buildParametersContext = context.GetContextObject<BuildParametersContext>();
			var buildMapContext = context.GetContextObject<BuildMapContext>();
			string[] unityCreateBundles = unityManifest.GetAllAssetBundles();

			// 1. 过滤掉原生Bundle
			string[] mapBundles = buildMapContext.Collection.Where(t => t.IsRawFile == false).Select(t => t.BundleName).ToArray();

			// 2. 验证Bundle
			List<string> exceptBundleList1 = unityCreateBundles.Except(mapBundles).ToList();
			if (exceptBundleList1.Count > 0)
			{
				foreach (var exceptBundle in exceptBundleList1)
				{
					BuildLogger.Warning($"差异资源包: {exceptBundle}");
				}
				throw new System.Exception("存在差异资源包！请查看警告信息！");
			}

			// 3. 验证Bundle
			List<string> exceptBundleList2 = mapBundles.Except(unityCreateBundles).ToList();
			if (exceptBundleList2.Count > 0)
			{
				foreach (var exceptBundle in exceptBundleList2)
				{
					BuildLogger.Warning($"差异资源包: {exceptBundle}");
				}
				throw new System.Exception("存在差异资源包！请查看警告信息！");
			}

			// 4. 验证Asset
			/*
			bool isPass = true;
			var buildMode = buildParametersContext.Parameters.BuildMode;
			if (buildMode == EBuildMode.ForceRebuild || buildMode == EBuildMode.IncrementalBuild)
			{
				int progressValue = 0;
				string pipelineOutputDirectory = buildParametersContext.GetPipelineOutputDirectory();
				foreach (var buildedBundle in buildedBundles)
				{
					string filePath = $"{pipelineOutputDirectory}/{buildedBundle}";
					string[] buildedAssetPaths = GetAssetBundleAllAssets(filePath);
					string[] mapAssetPaths = buildMapContext.GetBuildinAssetPaths(buildedBundle);
					if (mapAssetPaths.Length != buildedAssetPaths.Length)
					{
						BuildLogger.Warning($"构建的Bundle文件内的资源对象数量和预期不匹配 : {buildedBundle}");
						var exceptAssetList1 = mapAssetPaths.Except(buildedAssetPaths).ToList();
						foreach (var excpetAsset in exceptAssetList1)
						{
							BuildLogger.Warning($"构建失败的资源对象路径为 : {excpetAsset}");
						}
						var exceptAssetList2 = buildedAssetPaths.Except(mapAssetPaths).ToList();
						foreach (var excpetAsset in exceptAssetList2)
						{
							BuildLogger.Warning($"构建失败的资源对象路径为 : {excpetAsset}");
						}
						isPass = false;
						continue;
					}
					EditorTools.DisplayProgressBar("验证构建结果", ++progressValue, buildedBundles.Length);
				}
				EditorTools.ClearProgressBar();

				if (isPass == false)
				{
					throw new Exception("构建结果验证没有通过，请参考警告日志！");
				}
			}
			*/

			BuildLogger.Log("构建结果验证成功！");
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
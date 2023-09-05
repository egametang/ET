using System;
using System.Linq;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.Build.Pipeline.Interfaces;

namespace YooAsset.Editor
{
	[TaskAttribute("验证构建结果")]
	public class TaskVerifyBuildResult_SBP : IBuildTask
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
				var buildResultContext = context.GetContextObject<TaskBuilding_SBP.BuildResultContext>();
				VerifyingBuildingResult(context, buildResultContext.Results);
			}
		}

		/// <summary>
		/// 验证构建结果
		/// </summary>
		private void VerifyingBuildingResult(BuildContext context, IBundleBuildResults buildResults)
		{
			var buildParameters = context.GetContextObject<BuildParametersContext>();
			var buildMapContext = context.GetContextObject<BuildMapContext>();
			List<string> unityCreateBundles = buildResults.BundleInfos.Keys.ToList();

			// 1. 过滤掉原生Bundle
			List<string> expectBundles = buildMapContext.Collection.Where(t => t.IsRawFile == false).Select(t => t.BundleName).ToList();

			// 2. 验证Bundle
			List<string> exceptBundleList1 = unityCreateBundles.Except(expectBundles).ToList();
			if (exceptBundleList1.Count > 0)
			{
				foreach (var exceptBundle in exceptBundleList1)
				{
					BuildLogger.Warning($"差异资源包: {exceptBundle}");
				}
				throw new System.Exception("存在差异资源包！请查看警告信息！");
			}

			// 3. 验证Bundle
			List<string> exceptBundleList2 = expectBundles.Except(unityCreateBundles).ToList();
			if (exceptBundleList2.Count > 0)
			{
				foreach (var exceptBundle in exceptBundleList2)
				{
					BuildLogger.Warning($"差异资源包: {exceptBundle}");
				}
				throw new System.Exception("存在差异资源包！请查看警告信息！");
			}

			BuildLogger.Log("构建结果验证成功！");
		}
	}
}
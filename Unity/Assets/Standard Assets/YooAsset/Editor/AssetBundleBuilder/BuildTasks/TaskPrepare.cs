using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace YooAsset.Editor
{
	[TaskAttribute("资源构建准备工作")]
	public class TaskPrepare : IBuildTask
	{
		void IBuildTask.Run(BuildContext context)
		{
			var buildParametersContext = context.GetContextObject<BuildParametersContext>();
			var buildParameters = buildParametersContext.Parameters;

			// 检测构建参数合法性
			if (buildParameters.BuildTarget == BuildTarget.NoTarget)
				throw new Exception("请选择目标平台");
			if (string.IsNullOrEmpty(buildParameters.PackageName))
				throw new Exception("包裹名称不能为空");
			if (string.IsNullOrEmpty(buildParameters.PackageVersion))
				throw new Exception("包裹版本不能为空");

			if (buildParameters.BuildMode != EBuildMode.SimulateBuild)
			{
				// 检测当前是否正在构建资源包
				if (BuildPipeline.isBuildingPlayer)
					throw new Exception("当前正在构建资源包，请结束后再试");

				// 检测是否有未保存场景
				if (EditorTools.HasDirtyScenes())
					throw new Exception("检测到未保存的场景文件");

				// 检测首包资源标签
				if (buildParameters.CopyBuildinFileOption == ECopyBuildinFileOption.ClearAndCopyByTags
					|| buildParameters.CopyBuildinFileOption == ECopyBuildinFileOption.OnlyCopyByTags)
				{
					if (string.IsNullOrEmpty(buildParameters.CopyBuildinFileTags))
						throw new Exception("首包资源标签不能为空！");
				}

				// 检测共享资源打包规则
				if (buildParameters.ShareAssetPackRule == null)
					throw new Exception("共享资源打包规则不能为空！");

#if UNITY_WEBGL
				if (buildParameters.EncryptionServices != null)
				{
					if (buildParameters.EncryptionServices.GetType() != typeof(EncryptionNone))
					{
						throw new Exception("WebGL平台不支持加密！");
					}
				}
#endif

				// 检测包裹输出目录是否存在
				string packageOutputDirectory = buildParametersContext.GetPackageOutputDirectory();
				if (Directory.Exists(packageOutputDirectory))
					throw new Exception($"本次构建的补丁目录已经存在：{packageOutputDirectory}");

				// 保存改动的资源
				AssetDatabase.SaveAssets();
			}

			if (buildParameters.BuildMode == EBuildMode.ForceRebuild)
			{
				// 删除总目录
				string platformDirectory = $"{buildParameters.OutputRoot}/{buildParameters.BuildTarget}/{buildParameters.PackageName}";
				if (EditorTools.DeleteDirectory(platformDirectory))
				{
					BuildLogger.Log($"删除平台总目录：{platformDirectory}");
				}
			}

			// 如果输出目录不存在
			string pipelineOutputDirectory = buildParametersContext.GetPipelineOutputDirectory();
			if (EditorTools.CreateDirectory(pipelineOutputDirectory))
			{
				BuildLogger.Log($"创建输出目录：{pipelineOutputDirectory}");
			}
		}
	}
}
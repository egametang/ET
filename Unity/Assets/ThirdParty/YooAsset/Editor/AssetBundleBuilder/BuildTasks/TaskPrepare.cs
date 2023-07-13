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
				throw new Exception("请选择目标平台！");
			if (string.IsNullOrEmpty(buildParameters.PackageName))
				throw new Exception("包裹名称不能为空！");
			if (string.IsNullOrEmpty(buildParameters.PackageVersion))
				throw new Exception("包裹版本不能为空！");
			if (string.IsNullOrEmpty(buildParameters.BuildOutputRoot))
				throw new Exception("构建输出的根目录为空！");
			if (string.IsNullOrEmpty(buildParameters.StreamingAssetsRoot))
				throw new Exception("内置资源根目录为空！");

			if (buildParameters.BuildPipeline == EBuildPipeline.ScriptableBuildPipeline)
			{
				if (buildParameters.SBPParameters == null)
					throw new Exception($"{nameof(BuildParameters.SBPParameters)} is null !");

				if (buildParameters.BuildMode == EBuildMode.DryRunBuild)
					throw new Exception($"{nameof(EBuildPipeline.ScriptableBuildPipeline)} not support {nameof(EBuildMode.DryRunBuild)} build mode !");

				if (buildParameters.BuildMode == EBuildMode.ForceRebuild)
					throw new Exception($"{nameof(EBuildPipeline.ScriptableBuildPipeline)} not support {nameof(EBuildMode.ForceRebuild)} build mode !");
			}

			if (buildParameters.BuildMode != EBuildMode.SimulateBuild)
			{
#if UNITY_2021_3_OR_NEWER
				if (buildParameters.BuildPipeline == EBuildPipeline.BuiltinBuildPipeline)
				{
					BuildLogger.Warning("推荐使用可编程构建管线（SBP）！");
				}
#endif

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
				if (buildParameters.SharedPackRule == null)
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
				string packageRootDirectory = buildParametersContext.GetPackageRootDirectory();
				if (EditorTools.DeleteDirectory(packageRootDirectory))
				{
					BuildLogger.Log($"删除包裹目录：{packageRootDirectory}");
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
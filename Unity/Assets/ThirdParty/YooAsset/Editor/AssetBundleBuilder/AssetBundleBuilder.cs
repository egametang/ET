using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace YooAsset.Editor
{
	public class AssetBundleBuilder
	{
		public class BuildParametersContext : IContextObject
		{
			private readonly System.Diagnostics.Stopwatch _buildWatch = new System.Diagnostics.Stopwatch();

			/// <summary>
			/// 构建参数
			/// </summary>
			public BuildParameters Parameters { private set; get; }

			/// <summary>
			/// 构建管线的输出目录
			/// </summary>
			public string PipelineOutputDirectory { private set; get; }


			public BuildParametersContext(BuildParameters parameters)
			{
				Parameters = parameters;

				PipelineOutputDirectory = AssetBundleBuilderHelper.MakePipelineOutputDirectory(parameters.OutputRoot, parameters.BuildTarget);
				if (parameters.BuildMode == EBuildMode.DryRunBuild)
					PipelineOutputDirectory += $"_{EBuildMode.DryRunBuild}";
				else if (parameters.BuildMode == EBuildMode.SimulateBuild)
					PipelineOutputDirectory += $"_{EBuildMode.SimulateBuild}";
			}

			/// <summary>
			/// 获取本次构建的补丁目录
			/// </summary>
			public string GetPackageDirectory()
			{
				return $"{Parameters.OutputRoot}/{Parameters.BuildTarget}/{Parameters.BuildVersion}";
			}

			/// <summary>
			/// 获取构建选项
			/// </summary>
			public BuildAssetBundleOptions GetPipelineBuildOptions()
			{
				// For the new build system, unity always need BuildAssetBundleOptions.CollectDependencies and BuildAssetBundleOptions.DeterministicAssetBundle
				// 除非设置ForceRebuildAssetBundle标记，否则会进行增量打包

				BuildAssetBundleOptions opt = BuildAssetBundleOptions.None;
				opt |= BuildAssetBundleOptions.StrictMode; //Do not allow the build to succeed if any errors are reporting during it.

				if (Parameters.BuildMode == EBuildMode.SimulateBuild)
					throw new Exception("Should never get here !");

				if (Parameters.BuildMode == EBuildMode.DryRunBuild)
				{
					opt |= BuildAssetBundleOptions.DryRunBuild;
					return opt;
				}

				if (Parameters.CompressOption == ECompressOption.Uncompressed)
					opt |= BuildAssetBundleOptions.UncompressedAssetBundle;
				else if (Parameters.CompressOption == ECompressOption.LZ4)
					opt |= BuildAssetBundleOptions.ChunkBasedCompression;

				if (Parameters.BuildMode == EBuildMode.ForceRebuild)
					opt |= BuildAssetBundleOptions.ForceRebuildAssetBundle; //Force rebuild the asset bundles
				if (Parameters.DisableWriteTypeTree)
					opt |= BuildAssetBundleOptions.DisableWriteTypeTree; //Do not include type information within the asset bundle (don't write type tree).
				if (Parameters.IgnoreTypeTreeChanges)
					opt |= BuildAssetBundleOptions.IgnoreTypeTreeChanges; //Ignore the type tree changes when doing the incremental build check.

				opt |= BuildAssetBundleOptions.DisableLoadAssetByFileName; //Disables Asset Bundle LoadAsset by file name.
				opt |= BuildAssetBundleOptions.DisableLoadAssetByFileNameWithExtension; //Disables Asset Bundle LoadAsset by file name with extension.			

				return opt;
			}

			/// <summary>
			/// 获取构建的耗时（单位：秒）
			/// </summary>
			public float GetBuildingSeconds()
			{
				float seconds = _buildWatch.ElapsedMilliseconds / 1000f;
				return seconds;
			}
			public void BeginWatch()
			{
				_buildWatch.Start();
			}
			public void StopWatch()
			{
				_buildWatch.Stop();
			}
		}

		private readonly BuildContext _buildContext = new BuildContext();

		/// <summary>
		/// 开始构建
		/// </summary>
		public bool Run(BuildParameters buildParameters)
		{
			// 清空旧数据
			_buildContext.ClearAllContext();

			// 构建参数
			var buildParametersContext = new BuildParametersContext(buildParameters);
			_buildContext.SetContextObject(buildParametersContext);

			// 执行构建流程
			List<IBuildTask> pipeline = new List<IBuildTask>
			{
				new TaskPrepare(), //前期准备工作
				new TaskGetBuildMap(), //获取构建列表
				new TaskBuilding(), //开始执行构建
				new TaskVerifyBuildResult(), //验证构建结果
				new TaskEncryption(), //加密资源文件
				new TaskCreatePatchManifest(), //创建清单文件
				new TaskCreateReport(), //创建报告文件
				new TaskCreatePatchPackage(), //制作补丁包
				new TaskCopyBuildinFiles(), //拷贝内置文件
			};

			if (buildParameters.BuildMode == EBuildMode.SimulateBuild)
				BuildRunner.EnableLog = false;
			else
				BuildRunner.EnableLog = true;

			bool succeed = BuildRunner.Run(pipeline, _buildContext);
			if (succeed)
				Debug.Log($"{buildParameters.BuildMode} pipeline build succeed !");
			else
				Debug.LogWarning($"{buildParameters.BuildMode} pipeline build failed !");
			return succeed;
		}
	}
}
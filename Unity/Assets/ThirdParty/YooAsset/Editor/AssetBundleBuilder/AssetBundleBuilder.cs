using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace YooAsset.Editor
{
	public class AssetBundleBuilder
	{
		private readonly BuildContext _buildContext = new BuildContext();

		/// <summary>
		/// 构建资源包
		/// </summary>
		public BuildResult Run(BuildParameters buildParameters, List<IBuildTask> buildPipeline)
		{
			// 检测构建参数是否为空
			if (buildParameters == null)
				throw new Exception($"{nameof(buildParameters)} is null !");

			// 检测构建参数是否为空
			if (buildPipeline.Count == 0)
				throw new Exception($"Build pipeline is empty !");	

			// 清空旧数据
			_buildContext.ClearAllContext();

			// 构建参数
			var buildParametersContext = new BuildParametersContext(buildParameters);
			_buildContext.SetContextObject(buildParametersContext);

			// 初始化日志
			BuildLogger.InitLogger(buildParameters.EnableLog);

			// 执行构建流程
			var buildResult = BuildRunner.Run(buildPipeline, _buildContext);
			if (buildResult.Success)
			{
				buildResult.OutputPackageDirectory = buildParametersContext.GetPackageOutputDirectory();
				BuildLogger.Log($"{buildParameters.BuildMode} pipeline build succeed !");
			}
			else
			{
				BuildLogger.Warning($"{buildParameters.BuildMode} pipeline build failed !");
				BuildLogger.Error($"Build task failed : {buildResult.FailedTask}");
				BuildLogger.Error(buildResult.ErrorInfo);
			}

			return buildResult;
		}

		/// <summary>
		/// 构建资源包
		/// </summary>
		public BuildResult Run(BuildParameters buildParameters)
		{
			var buildPipeline = GetDefaultBuildPipeline(buildParameters.BuildPipeline);
			return Run(buildParameters, buildPipeline);
		}

		/// <summary>
		/// 获取默认的构建流程
		/// </summary>
		private List<IBuildTask> GetDefaultBuildPipeline(EBuildPipeline buildPipeline)
		{
			// 获取任务节点的属性集合
			if (buildPipeline == EBuildPipeline.BuiltinBuildPipeline)
			{
				List<IBuildTask> pipeline = new List<IBuildTask>
				{
					new TaskPrepare(), //前期准备工作
					new TaskGetBuildMap(), //获取构建列表
					new TaskBuilding(), //开始执行构建
					new TaskCopyRawFile(), //拷贝原生文件
					new TaskVerifyBuildResult(), //验证构建结果
					new TaskEncryption(), //加密资源文件
					new TaskUpdateBundleInfo(), //更新资源包信息
					new TaskCreateManifest(), //创建清单文件
					new TaskCreateReport(), //创建报告文件
					new TaskCreatePackage(), //制作包裹
					new TaskCopyBuildinFiles(), //拷贝内置文件
				};
				return pipeline;
			}
			else if (buildPipeline == EBuildPipeline.ScriptableBuildPipeline)
			{
				List<IBuildTask> pipeline = new List<IBuildTask>
				{
					new TaskPrepare(), //前期准备工作
					new TaskGetBuildMap(), //获取构建列表
					new TaskBuilding_SBP(), //开始执行构建
					new TaskCopyRawFile(), //拷贝原生文件
					new TaskVerifyBuildResult_SBP(), //验证构建结果
					new TaskEncryption(), //加密资源文件
					new TaskUpdateBundleInfo(), //更新补丁信息
					new TaskCreateManifest(), //创建清单文件
					new TaskCreateReport(), //创建报告文件
					new TaskCreatePackage(), //制作补丁包
					new TaskCopyBuildinFiles(), //拷贝内置文件
				};
				return pipeline;
			}
			else
			{
				throw new NotImplementedException();
			}
		}
	}
}
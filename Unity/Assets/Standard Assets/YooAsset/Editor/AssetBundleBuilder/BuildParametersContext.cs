using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace YooAsset.Editor
{
	public class BuildParametersContext : IContextObject
	{
		private string _pipelineOutputDirectory = string.Empty;
		private string _packageOutputDirectory = string.Empty;

		/// <summary>
		/// 构建参数
		/// </summary>
		public BuildParameters Parameters { private set; get; }


		public BuildParametersContext(BuildParameters parameters)
		{
			Parameters = parameters;
		}

		/// <summary>
		/// 获取构建管线的输出目录
		/// </summary>
		/// <returns></returns>
		public string GetPipelineOutputDirectory()
		{
			if (string.IsNullOrEmpty(_pipelineOutputDirectory))
			{
				_pipelineOutputDirectory = $"{Parameters.OutputRoot}/{Parameters.BuildTarget}/{Parameters.PackageName}/{YooAssetSettings.OutputFolderName}";
			}
			return _pipelineOutputDirectory;
		}

		/// <summary>
		/// 获取本次构建的补丁目录
		/// </summary>
		public string GetPackageOutputDirectory()
		{
			if (string.IsNullOrEmpty(_packageOutputDirectory))
			{
				_packageOutputDirectory = $"{Parameters.OutputRoot}/{Parameters.BuildTarget}/{Parameters.PackageName}/{Parameters.PackageVersion}";
			}
			return _packageOutputDirectory;
		}

		/// <summary>
		/// 获取内置构建管线的构建选项
		/// </summary>
		public BuildAssetBundleOptions GetPipelineBuildOptions()
		{
			// For the new build system, unity always need BuildAssetBundleOptions.CollectDependencies and BuildAssetBundleOptions.DeterministicAssetBundle
			// 除非设置ForceRebuildAssetBundle标记，否则会进行增量打包

			if (Parameters.BuildMode == EBuildMode.SimulateBuild)
				throw new Exception("Should never get here !");

			BuildAssetBundleOptions opt = BuildAssetBundleOptions.None;
			opt |= BuildAssetBundleOptions.StrictMode; //Do not allow the build to succeed if any errors are reporting during it.

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
		/// 获取可编程构建管线的构建参数
		/// </summary>
		public UnityEditor.Build.Pipeline.BundleBuildParameters GetSBPBuildParameters()
		{
			if (Parameters.BuildMode == EBuildMode.SimulateBuild)
				throw new Exception("Should never get here !");

			var targetGroup = BuildPipeline.GetBuildTargetGroup(Parameters.BuildTarget);
			var pipelineOutputDirectory = GetPipelineOutputDirectory();
			var buildParams = new UnityEditor.Build.Pipeline.BundleBuildParameters(Parameters.BuildTarget, targetGroup, pipelineOutputDirectory);

			if (Parameters.CompressOption == ECompressOption.Uncompressed)
				buildParams.BundleCompression = UnityEngine.BuildCompression.Uncompressed;
			else if (Parameters.CompressOption == ECompressOption.LZMA)
				buildParams.BundleCompression = UnityEngine.BuildCompression.LZMA;
			else if (Parameters.CompressOption == ECompressOption.LZ4)
				buildParams.BundleCompression = UnityEngine.BuildCompression.LZ4;
			else
				throw new System.NotImplementedException(Parameters.CompressOption.ToString());

			if (Parameters.DisableWriteTypeTree)
				buildParams.ContentBuildFlags |= UnityEditor.Build.Content.ContentBuildFlags.DisableWriteTypeTree;

			buildParams.UseCache = true;
			buildParams.CacheServerHost = Parameters.SBPParameters.CacheServerHost;
			buildParams.CacheServerPort = Parameters.SBPParameters.CacheServerPort;
			buildParams.WriteLinkXML = Parameters.SBPParameters.WriteLinkXML;

			return buildParams;
		}
	}
}
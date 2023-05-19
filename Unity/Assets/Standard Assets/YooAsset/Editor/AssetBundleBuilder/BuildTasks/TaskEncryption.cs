using System;
using System.Linq;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset.Editor
{
	[TaskAttribute("资源包加密")]
	public class TaskEncryption : IBuildTask
	{
		void IBuildTask.Run(BuildContext context)
		{
			var buildParameters = context.GetContextObject<BuildParametersContext>();
			var buildMapContext = context.GetContextObject<BuildMapContext>();

			var buildMode = buildParameters.Parameters.BuildMode;
			if (buildMode == EBuildMode.ForceRebuild || buildMode == EBuildMode.IncrementalBuild)
			{
				EncryptingBundleFiles(buildParameters, buildMapContext);
			}
		}

		/// <summary>
		/// 加密文件
		/// </summary>
		private void EncryptingBundleFiles(BuildParametersContext buildParametersContext, BuildMapContext buildMapContext)
		{
			var encryptionServices = buildParametersContext.Parameters.EncryptionServices;
			if (encryptionServices == null)
				return;

			if (encryptionServices.GetType() == typeof(EncryptionNone))
				return;

			int progressValue = 0;
			string pipelineOutputDirectory = buildParametersContext.GetPipelineOutputDirectory();
			foreach (var bundleInfo in buildMapContext.Collection)
			{
				EncryptFileInfo fileInfo = new EncryptFileInfo();
				fileInfo.BundleName = bundleInfo.BundleName;
				fileInfo.FilePath = $"{pipelineOutputDirectory}/{bundleInfo.BundleName}";

				var encryptResult = encryptionServices.Encrypt(fileInfo);		
				if (encryptResult.LoadMethod != EBundleLoadMethod.Normal)
				{
					// 注意：原生文件不支持加密
					if (bundleInfo.IsRawFile)
					{
						BuildLogger.Warning($"Encryption not support raw file : {bundleInfo.BundleName}");
						continue;
					}

					string filePath = $"{pipelineOutputDirectory}/{bundleInfo.BundleName}.encrypt";
					FileUtility.CreateFile(filePath, encryptResult.EncryptedData);
					bundleInfo.EncryptedFilePath = filePath;
					bundleInfo.LoadMethod = encryptResult.LoadMethod;
					BuildLogger.Log($"Bundle文件加密完成：{filePath}");
				}

				// 进度条
				EditorTools.DisplayProgressBar("加密资源包", ++progressValue, buildMapContext.Collection.Count);
			}
			EditorTools.ClearProgressBar();
		}
	}
}
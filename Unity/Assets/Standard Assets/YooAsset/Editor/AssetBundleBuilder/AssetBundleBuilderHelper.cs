using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace YooAsset.Editor
{
	public static class AssetBundleBuilderHelper
	{
		/// <summary>
		/// 获取默认的输出根路录
		/// </summary>
		public static string GetDefaultOutputRoot()
		{
			string projectPath = EditorTools.GetProjectPath();
			return $"{projectPath}/Bundles";
		}

		/// <summary>
		/// 获取流文件夹路径
		/// </summary>
		public static string GetStreamingAssetsFolderPath()
		{
			return $"{Application.dataPath}/StreamingAssets/{YooAssetSettings.StreamingAssetsBuildinFolder}/";
		}

		/// <summary>
		/// 清空流文件夹
		/// </summary>
		public static void ClearStreamingAssetsFolder()
		{
			string streamingFolderPath = GetStreamingAssetsFolderPath();
			EditorTools.ClearFolder(streamingFolderPath);
		}

		/// <summary>
		/// 删除流文件夹内无关的文件
		/// 删除.manifest文件和.meta文件
		/// </summary>
		public static void DeleteStreamingAssetsIgnoreFiles()
		{
			string streamingFolderPath = GetStreamingAssetsFolderPath();
			if (Directory.Exists(streamingFolderPath))
			{
				string[] files = Directory.GetFiles(streamingFolderPath, "*.manifest", SearchOption.AllDirectories);
				foreach (var file in files)
				{
					FileInfo info = new FileInfo(file);
					info.Delete();
				}

				files = Directory.GetFiles(streamingFolderPath, "*.meta", SearchOption.AllDirectories);
				foreach (var item in files)
				{
					FileInfo info = new FileInfo(item);
					info.Delete();
				}
			}
		}
	}
}
using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace ETModel
{
	public static class BundleHelper
	{
		public static async ETTask DownloadBundle()
		{
			if (Define.IsAsync)
			{
				try
				{
					using (BundleDownloaderComponent bundleDownloaderComponent = Game.Scene.AddComponent<BundleDownloaderComponent>())
					{
						await bundleDownloaderComponent.StartAsync();
						
						Game.EventSystem.Run(EventIdType.LoadingBegin);
						
						await bundleDownloaderComponent.DownloadAsync();
					}
					
					Game.EventSystem.Run(EventIdType.LoadingFinish);
					
					Game.Scene.GetComponent<ResourcesComponent>().LoadOneBundle("StreamingAssets");
					ResourcesComponent.AssetBundleManifestObject = (AssetBundleManifest)Game.Scene.GetComponent<ResourcesComponent>().GetAsset("StreamingAssets", "AssetBundleManifest");
				}
				catch (Exception e)
				{
					Log.Error(e);
				}

			}
		}

		public static string GetBundleMD5(VersionConfig streamingVersionConfig, string bundleName)
		{
			string path = Path.Combine(PathHelper.AppHotfixResPath, bundleName);
			if (File.Exists(path))
			{
				return MD5Helper.FileMD5(path);
			}
			
			if (streamingVersionConfig.FileInfoDict.ContainsKey(bundleName))
			{
				return streamingVersionConfig.FileInfoDict[bundleName].MD5;	
			}

			return "";
		}
	}
}

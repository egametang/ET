using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace ETModel
{
	public static class BundleHelper
	{
		public static async Task DownloadBundle()
		{
			if (Define.IsAsync)
			{
				try
				{
					using (BundleDownloaderComponent bundleDownloaderComponent = Game.Scene.AddComponent<BundleDownloaderComponent>())
					{
                        // 下载Version进行MD5对比
                        await bundleDownloaderComponent.StartAsync();
					    
                        // 根据对比结果进行自动下载更新
                        await bundleDownloaderComponent.DownloadAsync();
					}
                    // 下载成功后读取AssetBundleManifest数据并且缓存起来
                    Game.Scene.GetComponent<ResourcesComponent>().LoadOneBundle("StreamingAssets");
					ResourcesComponent.AssetBundleManifestObject = (AssetBundleManifest)Game.Scene.GetComponent<ResourcesComponent>().GetAsset("StreamingAssets", "AssetBundleManifest");

                    // 完成更新
				    FGUI.GetShowingUI<UI_CheckUpdate>()?.SetState(DownloadState.Done);
				}
				catch (Exception e)
				{
					Log.Error(e);
				}
			}
			else
			{
			    // 编辑器模式直接跳到完成
			    FGUI.GetShowingUI<UI_CheckUpdate>()?.SetState(DownloadState.Done);
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

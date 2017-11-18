using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Model
{
	public static class BundleHelper
	{
		public static async Task DownloadBundle()
		{
			Game.Scene.GetComponent<EventComponent>().Run(EventIdType.LoadingBegin);
			await StartDownLoadResources();
			Game.Scene.GetComponent<EventComponent>().Run(EventIdType.LoadingFinish);
		}
		
		public static async Task StartDownLoadResources()
		{
			if (Define.IsAsync)
			{
				//string url = GlobalConfigComponent.Instance.GlobalProto.GetUrl() + "StreamingAssets/StreamingAssets";
				//try
				//{
				//	using (WWWAsync wwwAsync = EntityFactory.Create<WWWAsync>())
				//	{
				//		await wwwAsync.DownloadAsync(url);
				//		ResourcesComponent.AssetBundleManifestObject = wwwAsync.www.assetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
				//		wwwAsync.www.assetBundle.Unload(false);
				//	}
				//}
				//catch (Exception e)
				//{
				//	Log.Error($"下载错误: {url} {e}");
				//	return;
				//}
				try
				{
					using (BundleDownloaderComponent bundleDownloaderComponent = Game.Scene.AddComponent<BundleDownloaderComponent>())
					{
						await bundleDownloaderComponent.StartAsync();
					}
					Log.Debug("11111111111111111111111111111111");
					Game.Scene.GetComponent<ResourcesComponent>().LoadOneBundle("StreamingAssets");
					ResourcesComponent.AssetBundleManifestObject = Game.Scene.GetComponent<ResourcesComponent>().GetAsset<AssetBundleManifest>("StreamingAssets", "AssetBundleManifest");
					Log.Debug("111111111111111111111111111111112");
				}
				catch (Exception e)
				{
					Log.Error(e.ToString());
				}

			}
		}
	}
}

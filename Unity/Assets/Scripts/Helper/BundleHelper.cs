using System;
using System.Threading.Tasks;
using UnityEngine;

namespace ETModel
{
	public static class BundleHelper
	{
		public static async Task DownloadBundle()
		{
			Game.EventSystem.Run(EventIdType.LoadingBegin);
            bool done = await StartDownLoadResources();
		    Game.EventSystem.Run(EventIdType.LoadingFinish);
            //if (done)
            //{
            //    Game.EventSystem.Run(EventIdType.LoadingFinish);
            //}
            //else
            //{
            //    Game.Scene.GetComponent<UILoadingComponent>().OnLoadFail();
            //}
        }
		
		public static async Task<bool> StartDownLoadResources()
		{
			if (Define.IsAsync)
			{
				try
				{
					using (BundleDownloaderComponent bundleDownloaderComponent = Game.Scene.AddComponent<BundleDownloaderComponent>())
					{
						await bundleDownloaderComponent.StartAsync();
					}
					Game.Scene.GetComponent<ResourcesComponent>().LoadOneBundle("StreamingAssets");
					ResourcesComponent.AssetBundleManifestObject = (AssetBundleManifest)Game.Scene.GetComponent<ResourcesComponent>().GetAsset("StreamingAssets", "AssetBundleManifest");
                    return true;
				}
				catch (Exception e)
				{
					Log.Error(e);
				}
			}
		    return false;
        }
    }
}

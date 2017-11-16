﻿using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace Model
{
	[ObjectEvent]
	public class AssetsLoaderAsyncEvent : ObjectEvent<AssetsLoaderAsync>, IUpdate, IAwake<AssetBundle>
	{
		public void Awake(AssetBundle assetBundle)
		{
			this.Get().Awake(assetBundle);
		}
		
		public void Update()
		{
			this.Get().Update();
		}
	}

	public class AssetsLoaderAsync : Disposer, IUpdate
	{
		private AssetBundle assetBundle;

		private AssetBundleRequest request;

		private TaskCompletionSource<bool> tcs;

		public void Awake(AssetBundle ab)
		{
			this.assetBundle = ab;
		}

		public void Update()
		{
			if (!this.request.isDone)
			{
				return;
			}

			TaskCompletionSource<bool> t = tcs;
			t.SetResult(true);
		}

		public override void Dispose()
		{
			if (this.Id == 0)
			{
				return;
			}
			base.Dispose();

			this.assetBundle = null;
			this.request = null;
		}

		public async Task<UnityEngine.Object[]> LoadAllAssetsAsync()
		{
			await InnerLoadAllAssetsAsync();
			return this.request.allAssets;
		}

		private Task<bool> InnerLoadAllAssetsAsync()
		{
			this.tcs = new TaskCompletionSource<bool>();
			this.request = assetBundle.LoadAllAssetsAsync();
			return this.tcs.Task;
		}
	}
}

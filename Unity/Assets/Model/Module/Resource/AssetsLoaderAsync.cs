using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace ET
{
	
	public class AssetsLoaderAsyncAwakeSystem : AwakeSystem<AssetsLoaderAsync, AssetBundle>
	{
		public override void Awake(AssetsLoaderAsync self, AssetBundle a)
		{
			self.Awake(a);
		}
	}

	
	public class AssetsLoaderAsyncUpdateSystem : UpdateSystem<AssetsLoaderAsync>
	{
		public override void Update(AssetsLoaderAsync self)
		{
			self.Update();
		}
	}

	public class AssetsLoaderAsync : Entity
	{
		private AssetBundle assetBundle;

		private AssetBundleRequest request;

		private ETTaskCompletionSource tcs;

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

			ETTaskCompletionSource t = tcs;
			t.SetResult();
		}

		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}
			base.Dispose();

			this.assetBundle = null;
			this.request = null;
		}

		public async ETTask<UnityEngine.Object[]> LoadAllAssetsAsync()
		{
			await InnerLoadAllAssetsAsync();
			return this.request.allAssets;
		}

		private ETTask InnerLoadAllAssetsAsync()
		{
			this.tcs = new ETTaskCompletionSource();
			this.request = assetBundle.LoadAllAssetsAsync();
			return this.tcs.Task;
		}
	}
}

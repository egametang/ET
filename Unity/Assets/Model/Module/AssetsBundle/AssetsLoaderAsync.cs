using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace ETModel
{
	[ObjectSystem]
	public class AssetsLoaderAsyncAwakeSystem : AwakeSystem<AssetsLoaderAsync, AssetBundle>
	{
		public override void Awake(AssetsLoaderAsync self, AssetBundle a)
		{
			self.Awake(a);
		}
	}

	[ObjectSystem]
	public class AssetsLoaderAsyncUpdateSystem : UpdateSystem<AssetsLoaderAsync>
	{
		public override void Update(AssetsLoaderAsync self)
		{
			self.Update();
		}
	}

	public class AssetsLoaderAsync : Component
	{
		private AssetBundle assetBundle;

		private AssetBundleRequest request;

		private ETTaskCompletionSource<bool> tcs;

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

			ETTaskCompletionSource<bool> t = tcs;
			t.SetResult(true);
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

		private ETTask<bool> InnerLoadAllAssetsAsync()
		{
			this.tcs = new ETTaskCompletionSource<bool>();
			this.request = assetBundle.LoadAllAssetsAsync();
			return this.tcs.Task;
		}
	}
}

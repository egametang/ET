using System.Threading.Tasks;
using UnityEngine;

namespace Model
{
	public class AssetBundleLoaderAsync : Entity
	{
		private AssetBundle assetBundle;

		private AssetBundleRequest request;

		private TaskCompletionSource<bool> tcs;

		public AssetBundleLoaderAsync(AssetBundle assetBundle)
		{
			this.assetBundle = assetBundle;
		}

		public void Update()
		{
			if (!this.request.isDone)
			{
				return;
			}

			TaskCompletionSource<bool> t = this.tcs;
			t.SetResult(true);
		}

		public override void Dispose()
		{
			if (this.Id == 0)
			{
				return;
			}
			base.Dispose();
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

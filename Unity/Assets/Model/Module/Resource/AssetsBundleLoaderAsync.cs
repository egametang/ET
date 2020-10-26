using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace ET
{
	
	public class AssetsBundleLoaderAsyncSystem : UpdateSystem<AssetsBundleLoaderAsync>
	{
		public override void Update(AssetsBundleLoaderAsync self)
		{
			self.Update();
		}
	}

	public class AssetsBundleLoaderAsync : Entity
	{
		private AssetBundleCreateRequest request;

		private ETTaskCompletionSource<AssetBundle> tcs;

		public void Update()
		{
			if (!this.request.isDone)
			{
				return;
			}

			ETTaskCompletionSource<AssetBundle> t = tcs;
			t.SetResult(this.request.assetBundle);
		}

		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}
			base.Dispose();
		}

		public ETTask<AssetBundle> LoadAsync(string path)
		{
			this.tcs = new ETTaskCompletionSource<AssetBundle>();
			this.request = AssetBundle.LoadFromFileAsync(path);
			return this.tcs.Task;
		}
	}
}

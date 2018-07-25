using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace ETModel
{
	[ObjectSystem]
	public class AssetsBundleLoaderAsyncSystem : UpdateSystem<AssetsBundleLoaderAsync>
	{
		public override void Update(AssetsBundleLoaderAsync self)
		{
			self.Update();
		}
	}

	public class AssetsBundleLoaderAsync : Component
	{
		private AssetBundleCreateRequest request;

		private TaskCompletionSource<AssetBundle> tcs;

		public void Update()
		{
			if (!this.request.isDone)
			{
				return;
			}

			TaskCompletionSource<AssetBundle> t = tcs;
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

		public Task<AssetBundle> LoadAsync(string path)
		{
			this.tcs = new TaskCompletionSource<AssetBundle>();
			this.request = AssetBundle.LoadFromFileAsync(path);
			return this.tcs.Task;
		}
	}
}

using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace Model
{
	[ObjectSystem]
	public class AssetsBundleLoaderAsyncSystem : ObjectSystem<AssetsBundleLoaderAsync>, IUpdate
	{
		public void Update()
		{
			this.Get().Update();
		}
	}

	public class AssetsBundleLoaderAsync : Component, IUpdate
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

		public Task<AssetBundle> LoadAsync(string bundleName)
		{
			this.tcs = new TaskCompletionSource<AssetBundle>();
			this.request = AssetBundle.LoadFromFileAsync(Path.Combine(PathHelper.AppHotfixResPath, bundleName));
			return this.tcs.Task;
		}
	}
}

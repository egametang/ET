using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace ETModel
{
	[ObjectSystem]
	public class WwwAsyncUpdateSystem : UpdateSystem<WWWAsync>
	{
		public override void Update(WWWAsync self)
		{
			self.Update();
		}
	}
	
	public class WWWAsync: Entity
	{
		public WWW www;

		public bool isCancel;

		public TaskCompletionSource<bool> tcs;

		public float Progress
		{
			get
			{
				if (this.www == null)
				{
					return 0;
				}
				return this.www.progress;
			}
		}

		public int ByteDownloaded
		{
			get
			{
				if (this.www == null)
				{
					return 0;
				}
				return this.www.bytesDownloaded;
			}
		}

		public void Update()
		{
			if (this.isCancel)
			{
				this.tcs.SetResult(false);
				return;
			}

			if (!this.www.isDone)
			{
				return;
			}

			if (!string.IsNullOrEmpty(this.www.error))
			{
				this.tcs.SetException(new Exception($"WWWAsync error: {this.www.error}"));
				return;
			}

			var t = this.tcs;
			this.tcs = null;
			t?.SetResult(true);
		}

		public Task<bool> LoadFromCacheOrDownload(string url, Hash128 hash)
		{
			url = url.Replace(" ", "%20");
			this.www = WWW.LoadFromCacheOrDownload(url, hash, 0);
			this.tcs = new TaskCompletionSource<bool>();
			return this.tcs.Task;
		}

		public Task<bool> LoadFromCacheOrDownload(string url, Hash128 hash, CancellationToken cancellationToken)
		{
			url = url.Replace(" ", "%20");
			this.www = WWW.LoadFromCacheOrDownload(url, hash, 0);
			this.tcs = new TaskCompletionSource<bool>();
			cancellationToken.Register(() => { this.isCancel = true; });
			return this.tcs.Task;
		}

		public Task<bool> DownloadAsync(string url)
		{
			url = url.Replace(" ", "%20");
			this.www = new WWW(url);
			this.tcs = new TaskCompletionSource<bool>();
			return this.tcs.Task;
		}

		public Task<bool> DownloadAsync(string url, CancellationToken cancellationToken)
		{
			url = url.Replace(" ", "%20");
			this.www = new WWW(url);
			this.tcs = new TaskCompletionSource<bool>();
			cancellationToken.Register(() => { this.isCancel = true; });
			return this.tcs.Task;
		}

		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}

			base.Dispose();

            www?.Dispose();
			this.www = null;
			this.tcs = null;
		}
	}
}

using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Model
{
	[ObjectEvent]
	public class WWWAsyncEvent : ObjectEvent<WWWAsync>, IUpdate
	{
		public void Update()
		{
			this.Get().Update();
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

			this.tcs.SetResult(true);
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
			if (this.Id == 0)
			{
				return;
			}

			base.Dispose();

            www?.Dispose();
			this.www = null;
		}
	}
}

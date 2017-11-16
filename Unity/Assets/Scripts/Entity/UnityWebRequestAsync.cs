using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Model
{
	[ObjectEvent]
	public class UnityWebRequestEvent : ObjectEvent<UnityWebRequestAsync>, IUpdate
	{
		public void Update()
		{
			this.Get().Update();
		}
	}
	
	public class UnityWebRequestAsync : Disposer
	{
		public UnityWebRequest Request;

		public bool isCancel;

		public TaskCompletionSource<bool> tcs;
		
		public override void Dispose()
		{
			if (this.Id == 0)
			{
				return;
			}

			base.Dispose();

			this.Request?.Dispose();
			this.Request = null;
			this.isCancel = false;
		}

		public float Progress
		{
			get
			{
				if (this.Request == null)
				{
					return 0;
				}
				return this.Request.downloadProgress;
			}
		}

		public ulong ByteDownloaded
		{
			get
			{
				if (this.Request == null)
				{
					return 0;
				}
				return this.Request.downloadedBytes;
			}
		}

		public void Update()
		{
			if (this.isCancel)
			{
				this.tcs.SetResult(false);
				return;
			}
			
			if (!this.Request.isDone)
			{
				return;
			}
			if (!string.IsNullOrEmpty(this.Request.error))
			{
				this.tcs.SetException(new Exception($"request error: {this.Request.error}"));
				return;
			}

			this.tcs.SetResult(true);
		}

		public Task<bool> DownloadAsync(string url)
		{
			this.tcs = new TaskCompletionSource<bool>();
			
			url = url.Replace(" ", "%20");
			this.Request = UnityWebRequest.Get(url);
			this.Request.Send();
			
			return this.tcs.Task;
		}
	}
}

﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Model
{
	[ObjectEvent]
	public class UIBundleDownloaderComponentEvent : ObjectEvent<BundleDownloaderComponent>, IAwake
	{
		public void Awake()
		{
			BundleDownloaderComponent self = this.Get();

			self.bundles = new Queue<string>();
			self.downloadedBundles = new HashSet<string>();
			self.downloadingBundle = "";
		}
	}
	
	/// <summary>
	/// 用来对比web端的资源，比较md5，对比下载资源
	/// </summary>
	public class BundleDownloaderComponent: Component
	{
		public VersionConfig VersionConfig { get; private set; }

		public Queue<string> bundles;

		public long TotalSize;

		public HashSet<string> downloadedBundles;

		public string downloadingBundle;

		public UnityWebRequestAsync downloadingRequest;

		public TaskCompletionSource<bool> Tcs;

		public async Task StartAsync()
		{
			using (UnityWebRequestAsync request = EntityFactory.Create<UnityWebRequestAsync>())
			{
				string versionUrl = GlobalConfigComponent.Instance.GlobalProto.GetUrl() + "StreamingAssets/" + "Version.txt";
				Log.Debug(versionUrl);
				await request.DownloadAsync(versionUrl);
				this.VersionConfig = MongoHelper.FromJson<VersionConfig>(request.Request.downloadHandler.text);
				Log.Info(MongoHelper.ToJson(this.VersionConfig));
			}
			
			Log.Debug(MongoHelper.ToJson(this.VersionConfig));

			// 对比本地的Version.txt
			string versionPath = Path.Combine(PathHelper.AppHotfixResPath, "Version.txt");
			if (!File.Exists(versionPath))
			{
				foreach (FileVersionInfo versionInfo in this.VersionConfig.FileVersionInfos)
				{
					if(versionInfo.File == "Version.txt")
					{
						continue;
					}
					this.bundles.Enqueue(versionInfo.File);
					this.TotalSize += versionInfo.Size;
				}
			}
			else
			{

				VersionConfig localVersionConfig = MongoHelper.FromJson<VersionConfig>(File.ReadAllText(versionPath));
				// 先删除服务器端没有的ab
				foreach (FileVersionInfo fileVersionInfo in localVersionConfig.FileVersionInfos)
				{
					if (this.VersionConfig.FileInfoDict.ContainsKey(fileVersionInfo.File))
					{
						continue;
					}
					string abPath = Path.Combine(PathHelper.AppHotfixResPath, fileVersionInfo.File);
					File.Delete(abPath);
				}

				// 再下载
				foreach (FileVersionInfo fileVersionInfo in this.VersionConfig.FileVersionInfos)
				{
					FileVersionInfo localVersionInfo;
					if (localVersionConfig.FileInfoDict.TryGetValue(fileVersionInfo.File, out localVersionInfo))
					{
						if (fileVersionInfo.MD5 == localVersionInfo.MD5)
						{
							continue;
						}
					}

					if(fileVersionInfo.File == "Version.txt")
					{
						continue;
					}
					
					this.bundles.Enqueue(fileVersionInfo.File);
					this.TotalSize += fileVersionInfo.Size;
				}
			}
			
			if (this.bundles.Count == 0)
			{
				return;
			}

			Log.Debug($"need download bundles: {this.bundles.ToList().ListToString()}");
			await this.WaitAsync();
		}

		private async void UpdateAsync()
		{
			while (true)
			{
				if (this.bundles.Count == 0)
				{
					break;
				}
				
				this.downloadingBundle = this.bundles.Dequeue();
				
				while (true)
				{
					try
					{
						using (this.downloadingRequest = EntityFactory.Create<UnityWebRequestAsync>())
						{
							await this.downloadingRequest.DownloadAsync(GlobalConfigComponent.Instance.GlobalProto.GetUrl() + "StreamingAssets/" + this.downloadingBundle);
							byte[] data = this.downloadingRequest.Request.downloadHandler.data;

							string path = Path.Combine(PathHelper.AppHotfixResPath, this.downloadingBundle);
							if (!Directory.Exists(Path.GetDirectoryName(path)))
							{
								Directory.CreateDirectory(Path.GetDirectoryName(path));
							}
							using (FileStream fs = new FileStream(path, FileMode.Create))
							{
								fs.Write(data, 0, data.Length);
							}
						}
					}
					catch(Exception e)
					{
						Log.Error($"download bundle error: {this.downloadingBundle}\n{e}");
						continue;
					}

					break;
				}
				this.downloadedBundles.Add(this.downloadingBundle);
				this.downloadingBundle = "";
				this.downloadingRequest = null;
			}

			using (FileStream fs = new FileStream(Path.Combine(PathHelper.AppHotfixResPath, "Version.txt"), FileMode.Create))
			using (StreamWriter sw = new StreamWriter(fs))
			{
				sw.Write(MongoHelper.ToJson(this.VersionConfig));
			}

			this.Tcs?.SetResult(true);
		}

		public int Progress
		{
			get
			{
				if (this.VersionConfig == null)
				{
					return 0;
				}
				long alreadyDownloadBytes = 0;
				foreach (string downloadedBundle in this.downloadedBundles)
				{
					long size = this.VersionConfig.FileInfoDict[downloadedBundle].Size;
					alreadyDownloadBytes += size;
				}
				if (this.downloadingRequest != null)
				{
					alreadyDownloadBytes += (long)this.downloadingRequest.Request.downloadedBytes;
				}
				return (int)(alreadyDownloadBytes * 100f / this.VersionConfig.TotalSize);
			}
		}

		private Task<bool> WaitAsync()
		{
			if (this.bundles.Count == 0 && this.downloadingBundle == "")
			{
				return Task.FromResult(true);
			}
			
			this.Tcs = new TaskCompletionSource<bool>();

			UpdateAsync();
			
			return this.Tcs.Task;
		}

		public override void Dispose()
		{
			if (this.Id == 0)
			{
				return;
			}

			base.Dispose();
		}
	}
}

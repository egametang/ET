using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ETModel
{
	[ObjectSystem]
	public class UiBundleDownloaderComponentAwakeSystem : AwakeSystem<BundleDownloaderComponent>
	{
		public override void Awake(BundleDownloaderComponent self)
		{
			self.bundles = new Queue<string>();
			self.downloadedBundles = new HashSet<string>();
			self.downloadingBundle = "";
		}
	}

	/// <summary>
	/// 用来对比web端的资源，比较md5，对比下载资源
	/// </summary>
	public class BundleDownloaderComponent : Component
	{
		public VersionConfig VersionConfig { get; private set; }

		public Queue<string> bundles;

		public long TotalSize;

		public HashSet<string> downloadedBundles;

		public string downloadingBundle;

		public UnityWebRequestAsync webRequest;

		public TaskCompletionSource<bool> Tcs;

		public async Task StartAsync()
		{
			using (UnityWebRequestAsync webRequestAsync = ComponentFactory.Create<UnityWebRequestAsync>())
			{
				string versionUrl = GlobalConfigComponent.Instance.GlobalProto.GetUrl() + "StreamingAssets/" + "Version.txt";
				//Log.Debug(versionUrl);
				await webRequestAsync.DownloadAsync(versionUrl);
				this.VersionConfig = JsonHelper.FromJson<VersionConfig>(webRequestAsync.Request.downloadHandler.text);
				//Log.Debug(JsonHelper.ToJson(this.VersionConfig));
			}


			VersionConfig localVersionConfig;
			// 对比本地的Version.txt
			string versionPath = Path.Combine(PathHelper.AppHotfixResPath, "Version.txt");
			if (File.Exists(versionPath))
			{
				localVersionConfig = JsonHelper.FromJson<VersionConfig>(File.ReadAllText(versionPath));
			}
			else
			{
				versionPath = Path.Combine(PathHelper.AppResPath, "Version.txt");
				using (UnityWebRequestAsync request = ComponentFactory.Create<UnityWebRequestAsync>())
				{
					await request.DownloadAsync(versionPath);
					localVersionConfig = JsonHelper.FromJson<VersionConfig>(request.Request.downloadHandler.text);
				}
			}


			// 先删除服务器端没有的ab
			foreach (FileVersionInfo fileVersionInfo in localVersionConfig.FileInfoDict.Values)
			{
				if (this.VersionConfig.FileInfoDict.ContainsKey(fileVersionInfo.File))
				{
					continue;
				}
				string abPath = Path.Combine(PathHelper.AppHotfixResPath, fileVersionInfo.File);
				File.Delete(abPath);
			}

			// 再下载
			foreach (FileVersionInfo fileVersionInfo in this.VersionConfig.FileInfoDict.Values)
			{
				FileVersionInfo localVersionInfo;
				if (localVersionConfig.FileInfoDict.TryGetValue(fileVersionInfo.File, out localVersionInfo))
				{
					if (fileVersionInfo.MD5 == localVersionInfo.MD5)
					{
						continue;
					}
				}

				if (fileVersionInfo.File == "Version.txt")
				{
					continue;
				}

				this.bundles.Enqueue(fileVersionInfo.File);
				this.TotalSize += fileVersionInfo.Size;
			}

			if (this.bundles.Count == 0)
			{
				return;
			}

			//Log.Debug($"need download bundles: {this.bundles.ToList().ListToString()}");
			await this.WaitAsync();
		}

		private async void UpdateAsync()
		{
			try
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
							using (this.webRequest = ComponentFactory.Create<UnityWebRequestAsync>())
							{
								await this.webRequest.DownloadAsync(GlobalConfigComponent.Instance.GlobalProto.GetUrl() + "StreamingAssets/" + this.downloadingBundle);
								byte[] data = this.webRequest.Request.downloadHandler.data;

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
						catch (Exception e)
						{
							Log.Error($"download bundle error: {this.downloadingBundle}\n{e}");
							continue;
						}

						break;
					}
					this.downloadedBundles.Add(this.downloadingBundle);
					this.downloadingBundle = "";
					this.webRequest = null;
				}

				using (FileStream fs = new FileStream(Path.Combine(PathHelper.AppHotfixResPath, "Version.txt"), FileMode.Create))
				using (StreamWriter sw = new StreamWriter(fs))
				{
					sw.Write(JsonHelper.ToJson(this.VersionConfig));
				}

				this.Tcs?.SetResult(true);
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}

		public int Progress
		{
			get
			{
				if (this.VersionConfig == null)
				{
					return 0;
				}

				if (this.TotalSize == 0)
				{
					return 0;
				}

				long alreadyDownloadBytes = 0;
				foreach (string downloadedBundle in this.downloadedBundles)
				{
					long size = this.VersionConfig.FileInfoDict[downloadedBundle].Size;
					alreadyDownloadBytes += size;
				}
				if (this.webRequest != null)
				{
					alreadyDownloadBytes += (long)this.webRequest.Request.downloadedBytes;
				}
				return (int)(alreadyDownloadBytes * 100f / this.TotalSize);
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
	}
}

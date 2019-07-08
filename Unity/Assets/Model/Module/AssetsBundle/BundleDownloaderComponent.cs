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
		private VersionConfig remoteVersionConfig;
		
		public Queue<string> bundles;

		public long TotalSize;

		public HashSet<string> downloadedBundles;

		public string downloadingBundle;

		public UnityWebRequestAsync webRequest;

		public async ETTask StartAsync()
		{
			// 获取远程的Version.txt
			string versionUrl = "";
			try
			{
				using (UnityWebRequestAsync webRequestAsync = ComponentFactory.Create<UnityWebRequestAsync>())
				{
					versionUrl = GlobalConfigComponent.Instance.GlobalProto.GetUrl() + "StreamingAssets/" + "Version.txt";
					//Log.Debug(versionUrl);
					await webRequestAsync.DownloadAsync(versionUrl);
					remoteVersionConfig = JsonHelper.FromJson<VersionConfig>(webRequestAsync.Request.downloadHandler.text);
					//Log.Debug(JsonHelper.ToJson(this.VersionConfig));
				}

			}
			catch (Exception e)
			{
				throw new Exception($"url: {versionUrl}", e);
			}

			// 获取streaming目录的Version.txt
			VersionConfig streamingVersionConfig;
			string versionPath = Path.Combine(PathHelper.AppResPath4Web, "Version.txt");
			using (UnityWebRequestAsync request = ComponentFactory.Create<UnityWebRequestAsync>())
			{
				await request.DownloadAsync(versionPath);
				streamingVersionConfig = JsonHelper.FromJson<VersionConfig>(request.Request.downloadHandler.text);
			}
			
			// 删掉远程不存在的文件
			DirectoryInfo directoryInfo = new DirectoryInfo(PathHelper.AppHotfixResPath);
			if (directoryInfo.Exists)
			{
				FileInfo[] fileInfos = directoryInfo.GetFiles();
				foreach (FileInfo fileInfo in fileInfos)
				{
					if (remoteVersionConfig.FileInfoDict.ContainsKey(fileInfo.Name))
					{
						continue;
					}

					if (fileInfo.Name == "Version.txt")
					{
						continue;
					}
					
					fileInfo.Delete();
				}
			}
			else
			{
				directoryInfo.Create();
			}

			// 对比MD5
			foreach (FileVersionInfo fileVersionInfo in remoteVersionConfig.FileInfoDict.Values)
			{
				// 对比md5
				string localFileMD5 = BundleHelper.GetBundleMD5(streamingVersionConfig, fileVersionInfo.File);
				if (fileVersionInfo.MD5 == localFileMD5)
				{
					continue;
				}
				this.bundles.Enqueue(fileVersionInfo.File);
				this.TotalSize += fileVersionInfo.Size;
			}
		}

		public int Progress
		{
			get
			{
				if (this.TotalSize == 0)
				{
					return 0;
				}

				long alreadyDownloadBytes = 0;
				foreach (string downloadedBundle in this.downloadedBundles)
				{
					long size = this.remoteVersionConfig.FileInfoDict[downloadedBundle].Size;
					alreadyDownloadBytes += size;
				}
				if (this.webRequest != null)
				{
					alreadyDownloadBytes += (long)this.webRequest.Request.downloadedBytes;
				}
				return (int)(alreadyDownloadBytes * 100f / this.TotalSize);
			}
		}

		public async ETTask DownloadAsync()
		{
			if (this.bundles.Count == 0 && this.downloadingBundle == "")
			{
				return;
			}

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
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}
	}
}

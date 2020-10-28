using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ET
{
	
	public class BundleDownloaderComponentAwakeSystem : AwakeSystem<BundleDownloaderComponent>
	{
		public override void Awake(BundleDownloaderComponent self)
		{
			self.bundles = new Queue<string>();
			self.downloadedBundles = new HashSet<string>();
		}
	}

	/// <summary>
	/// 用来对比web端的资源，比较md5，对比下载资源
	/// </summary>
	public class BundleDownloaderComponent : Entity
	{
		private VersionConfig remoteVersionConfig;
		
		public Queue<string> bundles;

		public long TotalSize;

		public HashSet<string> downloadedBundles;

		//多任务同时下载
		public List<UnityWebRequestAsync> webRequests = new List<UnityWebRequestAsync>();
		
		public override void Dispose()
		{
				if (this.IsDisposed)
				{
						return;
				}

				if (this.Parent.IsDisposed)
				{
						return;
				}

				base.Dispose();

				this.remoteVersionConfig = null;
				this.TotalSize = 0;
				this.bundles = null;
				this.downloadedBundles = null;
				foreach (UnityWebRequestAsync webRequest in this.webRequests)
				{
					webRequest.Dispose();
				}
				webRequests.Clear();

				this.Parent.RemoveComponent<BundleDownloaderComponent>();
		}

		public async ETTask StartAsync(string url)
		{
			// 获取远程的Version.txt
			string versionUrl = "";
			try
			{
				using (UnityWebRequestAsync webRequestAsync = EntityFactory.Create<UnityWebRequestAsync>(this.Domain))
				{
					versionUrl = url + "StreamingAssets/" + "Version.txt";
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
			using (UnityWebRequestAsync request = EntityFactory.Create<UnityWebRequestAsync>(this.Domain))
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
				//已经下载完成的
				foreach (string downloadedBundle in this.downloadedBundles)
				{
					alreadyDownloadBytes += this.remoteVersionConfig.FileInfoDict[downloadedBundle].Size;
				}
				//当前正在下载的
				foreach (UnityWebRequestAsync webRequest in webRequests)
				{
					alreadyDownloadBytes += (long) webRequest.Request.downloadedBytes;
				}
				return (int)(alreadyDownloadBytes * 100f / this.TotalSize);
			}
		}

		public async ETTask DownloadAsync(string url)
		{
			if (this.bundles.Count == 0)
			{
				return;
			}
			try
			{
				//正在下载的文件个数
				int downloadingCount = 0;
				//下载单个文件
                async void downloadFile()
                {
                    if (this.bundles.Count == 0)
                        return;
                    downloadingCount++;
                    //取出一个进行下载
                    string downloading = this.bundles.Dequeue();
                    Log.Debug($"开始下载({downloadingCount}):{downloading}");
                    try
                    {

                        using (UnityWebRequestAsync webRequest = EntityFactory.Create<UnityWebRequestAsync>(this.Domain))
                        {
                            webRequests.Add(webRequest);
                            await webRequest.DownloadAsync(url + "StreamingAssets/" + downloading);
                            byte[] data = webRequest.Request.downloadHandler.data;
                            webRequests.Remove(webRequest);
                            string path = Path.Combine(PathHelper.AppHotfixResPath, downloading);
                            using (FileStream fs = new FileStream(path, FileMode.Create))
                            {
	                            fs.Write(data, 0, data.Length);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        //下载异常跳过
                        Log.Error($"download bundle error: {downloading}\n{e}");
                    }
                    finally
                    {
	                    downloadingCount--;
                    }
                    //正常下载
                    this.downloadedBundles.Add(downloading);
                    Log.Debug($"download bundle Finish: {downloading}\n");
                }
                /*
                //最多同时下载n个文件 下载40M(400~500)个文件测试时间(ms)对比
                //等待n个任务同时完成再继续 1~61616 2~44796 3~34377 4~31918 5~27184 6~25564 7~22817 8~22719
                //完成1个补充1个最大n个任务 1~61309 8~11871 9~10843 10~10600 15~9309 20~9146 100~9195
                */
                
                //最大任务数量20 速度从61秒提升到9秒
                int maxCount = 20;
                while (true)
                {
	                await TimerComponent.Instance.WaitAsync(10);
	                //需要下载队列取完 正在下载为0表示完成更新
	                if (this.bundles.Count == 0 && downloadingCount == 0)
		                break;
	                for (int i = downloadingCount; i < maxCount; i++)
		                downloadFile();
                }
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}
	}
}

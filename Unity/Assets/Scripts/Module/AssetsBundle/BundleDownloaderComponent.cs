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

        public Dictionary<string, string[]> MapVersionConfigDict { get; private set; }

        public Dictionary<string, string[]> DBVersionConfigDict { get; private set; }

        public Queue<string> bundles;

        public long TotalSize;

        public long RemaningSize;

        public HashSet<string> downloadedBundles;

        public string downloadingBundle;

        public UnityWebRequestAsync webRequest;

        public TaskCompletionSource<bool> Tcs;

        private string NewMapVersionString;

        private string NewDBVersionString;

        public async Task StartAsync()
        {
            using (UnityWebRequestAsync webRequestAsync = ComponentFactory.Create<UnityWebRequestAsync>())
            {
                string versionUrl = GlobalConfigComponent.Instance.GlobalProto.GetUrl() + "StreamingAssets/" + "Version.txt";
                //Log.Debug(versionUrl);
                await webRequestAsync.DownloadAsync(versionUrl);
                this.VersionConfig = JsonHelper.FromJson<VersionConfig>(webRequestAsync.Request.downloadHandler.text);
                //下载地图Version.txt
                //Log.Debug(GlobalConfigComponent.Instance.GlobalProto.AssetBundleServerUrl + "Map/" + "MapVersion.txt");
                string mapversionUrl = GlobalConfigComponent.Instance.GlobalProto.AssetBundleServerUrl + "Map/" + "MapVersion.txt";
                await webRequestAsync.DownloadAsync(mapversionUrl);
                NewMapVersionString = webRequestAsync.Request.downloadHandler.text;
                string[] array = NewMapVersionString.Split('\n');
                if (array.Length > 0)
                {
                    this.MapVersionConfigDict = new Dictionary<string, string[]>();
                    for (int i = 0; i < array.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(array[i]))
                        {
                            string[] info = array[i].Split('|');
                            this.MapVersionConfigDict.Add(info[0], new[] { info[1], info[2] }); //filename, md5, size
                        }
                    }
                }
                //下载DBVersion.txt
                //Log.Debug(GlobalConfigComponent.Instance.GlobalProto.AssetBundleServerUrl + "GameConfig/" + "GameConfigVersion.txt");
                string dbconfigversionUrl = GlobalConfigComponent.Instance.GlobalProto.AssetBundleServerUrl + "DB/" + "DBVersion.txt";
                await webRequestAsync.DownloadAsync(dbconfigversionUrl);
                NewDBVersionString = webRequestAsync.Request.downloadHandler.text;
                string[] array1 = NewDBVersionString.Split('\n');
                if (array1.Length > 0)
                {
                    this.DBVersionConfigDict = new Dictionary<string, string[]>();
                    for (int i = 0; i < array1.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(array1[i]))
                        {
                            string[] info = array1[i].Split('|');
                            this.DBVersionConfigDict.Add(info[0], new[] { info[1], info[2] }); //filename, md5, size
                        }
                    }
                }
            }

            //------------------------------------------------------------------------------------------------------------------

            // 获得本地的Version.txt
            VersionConfig localVersionConfig;
            string versionPath = Path.Combine(PathHelper.AppHotfixResPath, "Version.txt");
            if (File.Exists(versionPath))
            {
                localVersionConfig = JsonHelper.FromJson<VersionConfig>(File.ReadAllText(versionPath));
            }
            else
            {
                versionPath = Path.Combine(PathHelper.AppResPath4Web, "Version.txt");
                using (UnityWebRequestAsync request = ComponentFactory.Create<UnityWebRequestAsync>())
                {
                    await request.DownloadAsync(versionPath);
                    localVersionConfig = JsonHelper.FromJson<VersionConfig>(request.Request.downloadHandler.text);
                }
            }
            // 获得本地Map的Version.txt
            Dictionary<string, string[]> localmapConfig = new Dictionary<string, string[]>();
            string mapversionPath = Path.Combine(PathHelper.AppHotfixResPath, "MapVersion.txt");
            if (File.Exists(mapversionPath))
            {
                string[] array = File.ReadAllText(mapversionPath).Split('\n');
                if (array.Length > 0)
                {
                    for (int i = 0; i < array.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(array[i]))
                        {
                            string[] info = array[i].Split('|');
                            localmapConfig.Add(info[0], new[] { info[1], info[2] }); //filename, md5, size
                        }
                    }
                }
            }
            // 获得本地DB的Version.txt
            Dictionary<string, string[]> localdbconfigConfig = new Dictionary<string, string[]>();
            string dbversionPath = Path.Combine(PathHelper.AppHotfixResPath, "DBVersion.txt");
            if (File.Exists(dbversionPath))
            {
                string[] array = File.ReadAllText(dbversionPath).Split('\n');
                if (array.Length > 0)
                {
                    for (int i = 0; i < array.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(array[i]))
                        {
                            string[] info = array[i].Split('|');
                            localdbconfigConfig.Add(info[0], new[] { info[1], info[2] }); //filename, md5, size
                        }
                    }
                }
            }

            //------------------------------------------------------------------------------------------------------------------

            // 先删除服务器端没有的 ab
            foreach (FileVersionInfo fileVersionInfo in localVersionConfig.FileInfoDict.Values)
            {
                if (this.VersionConfig.FileInfoDict.ContainsKey(fileVersionInfo.File)) continue;
                string abPath = Path.Combine(PathHelper.AppHotfixResPath, fileVersionInfo.File);
                File.Delete(abPath);
            }
            // 先删除服务器端没有的地图 ab
            foreach (var file in localmapConfig)
            {
                //Log.Debug("Delete: " + file.Key + " " + file.Value[0] + " " + file.Value[1]);
                if (this.MapVersionConfigDict.ContainsKey(file.Key)) continue;
                string abPath = Path.Combine(PathHelper.AppHotfixResPath + "/Map", file.Key);
                File.Delete(abPath);
            }
            // 先删除服务器端没有的config ab
            foreach (var file in localdbconfigConfig)
            {
                //Log.Debug("Delete: " + file.Key + " " + file.Value[0] + " " + file.Value[1]);
                if (this.DBVersionConfigDict.ContainsKey(file.Key)) continue;
                string abPath = Path.Combine(PathHelper.AppHotfixResPath + "/DB", file.Key);
                File.Delete(abPath);
            }

            //------------------------------------------------------------------------------------------------------------------

            // 开始下载地图数据
            foreach (var file in this.MapVersionConfigDict)
            {
                string[] info;
                if (localmapConfig.TryGetValue(file.Key, out info))
                {
                    //对比MD5
                    if (file.Value[0] == info[0]) continue; //filename, md5, size
                }

                if (file.Key == "MapVersion.txt") continue;

                //Log.Debug("Enqueue: " + file.Key + " " + file.Value[0] + " " + file.Value[1]);
                this.bundles.Enqueue(file.Key);
                this.TotalSize += int.Parse(file.Value[1]);
                this.RemaningSize += int.Parse(file.Value[1]);
            }
            // 开始下载DB数据
            foreach (var file in this.DBVersionConfigDict)
            {
                string[] info;
                if (localdbconfigConfig.TryGetValue(file.Key, out info))
                {
                    //对比MD5
                    if (file.Value[0] == info[0]) continue; //filename, md5, size
                }

                if (file.Key == "DBVersion.txt") continue;

                //Log.Debug("Enqueue: " + file.Key + " " + file.Value[0] + " " + file.Value[1]);
                this.bundles.Enqueue(file.Key);
                this.TotalSize += int.Parse(file.Value[1]);
                this.RemaningSize += int.Parse(file.Value[1]);
            }
            // 开始下载UI数据
            foreach (FileVersionInfo fileVersionInfo in this.VersionConfig.FileInfoDict.Values)
            {
                FileVersionInfo localVersionInfo;
                if (localVersionConfig.FileInfoDict.TryGetValue(fileVersionInfo.File, out localVersionInfo))
                {
                    //对比MD5
                    if (fileVersionInfo.MD5 == localVersionInfo.MD5) continue;
                }

                if (fileVersionInfo.File == "Version.txt") continue;

                this.bundles.Enqueue(fileVersionInfo.File);
                this.TotalSize += fileVersionInfo.Size;
                this.RemaningSize += fileVersionInfo.Size;
            }

            //------------------------------------------------------------------------------------------------------------------

            if (this.bundles.Count == 0)
            {
                //写入文本到本地
                using (FileStream fs = new FileStream(Path.Combine(PathHelper.AppHotfixResPath, "Version.txt"), FileMode.Create))
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(JsonHelper.ToJson(this.VersionConfig));
                }

                //就算没新增也强制重写文本到本地
                using (StreamWriter file = new StreamWriter(Path.Combine(PathHelper.AppHotfixResPath, "MapVersion.txt"), false))
                {
                    file.Write(NewMapVersionString);
                }

                //就算没新增也强制重写文本到本地
                using (StreamWriter file = new StreamWriter(Path.Combine(PathHelper.AppHotfixResPath, "DBVersion.txt"), false))
                {
                    file.Write(NewDBVersionString);
                }

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
                    //DebugText._instance.Show("AA: " + this.downloadingBundle);

                    while (true)
                    {
                        try
                        {
                            using (this.webRequest = ComponentFactory.Create<UnityWebRequestAsync>())
                            {
                                string path;
                                if (this.downloadingBundle.Contains(".dat"))
                                {
                                    //地图dat数据
                                    await this.webRequest.DownloadAsync(GlobalConfigComponent.Instance.GlobalProto.AssetBundleServerUrl + "Map/" + this.downloadingBundle);
                                    path = Path.Combine(PathHelper.AppHotfixResPath + "/Map", this.downloadingBundle);
                                    //DebugText._instance.Show("dat: " + path);
                                    //Log.Debug("开始dat下载: " + GlobalConfigComponent.Instance.GlobalProto.AssetBundleServerUrl + "Map/" + this.downloadingBundle);
                                }
                                else if (this.downloadingBundle.Contains(".db"))
                                {
                                    //游戏db数据
                                    await this.webRequest.DownloadAsync(GlobalConfigComponent.Instance.GlobalProto.AssetBundleServerUrl + "DB/" + this.downloadingBundle);
                                    path = Path.Combine(PathHelper.AppHotfixResPath + "/DB", this.downloadingBundle);
                                    //DebugText._instance.Show("db: "+path);
                                    //Log.Debug("开始dat下载: " + GlobalConfigComponent.Instance.GlobalProto.AssetBundleServerUrl + "GameConfig/" + this.downloadingBundle);
                                }
                                else
                                {
                                    //普通资源
                                    await this.webRequest.DownloadAsync(GlobalConfigComponent.Instance.GlobalProto.GetUrl() + "StreamingAssets/" + this.downloadingBundle);
                                    path = Path.Combine(PathHelper.AppHotfixResPath, this.downloadingBundle);
                                }
                                
                                byte[] data = this.webRequest.Request.downloadHandler.data;
                                
                                if (!Directory.Exists(Path.GetDirectoryName(path)))
                                {
                                    //DebugText._instance.Show("目录不存在:"+ Path.GetDirectoryName(path));
                                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                                    //DebugText._instance.Show("目录创建成功:" + path);
                                }

                                //DebugText._instance.Show("开始写入:" + path);
                                using (FileStream fs = new FileStream(path, FileMode.Create))
                                {
                                    fs.Write(data, 0, data.Length);
                                    //DebugText._instance.Show("成功:" + path);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            //DebugText._instance.Show("2:" + e.Message);
                            Log.Error($"download bundle error: {this.downloadingBundle}\n{e}");
                            continue;
                        }

                        break;
                    }

                    this.downloadedBundles.Add(this.downloadingBundle);
                    this.downloadingBundle = "";
                    this.webRequest = null;
                }

                //写入文本到本地
                using (FileStream fs = new FileStream(Path.Combine(PathHelper.AppHotfixResPath, "Version.txt"), FileMode.Create))
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(JsonHelper.ToJson(this.VersionConfig));
                }

                //写入文本到本地
                using (StreamWriter file = new StreamWriter(Path.Combine(PathHelper.AppHotfixResPath, "MapVersion.txt"), false))
                {
                    file.Write(NewMapVersionString);
                }

                //就算没新增也强制重写文本到本地
                using (StreamWriter file = new StreamWriter(Path.Combine(PathHelper.AppHotfixResPath, "DBVersion.txt"), false))
                {
                    file.Write(NewDBVersionString);
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
                if (this.VersionConfig == null || this.MapVersionConfigDict == null || this.MapVersionConfigDict.Count == 0)
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
                    long size = 0;
                    //区分信息类别
                    if (downloadedBundle.Contains(".dat"))
                    {
                        size = long.Parse(this.MapVersionConfigDict[downloadedBundle][1]);
                    }
                    else if (downloadedBundle.Contains(".db"))
                    {
                        size = long.Parse(this.DBVersionConfigDict[downloadedBundle][1]);
                    }
                    else
                    {
                        size = this.VersionConfig.FileInfoDict[downloadedBundle].Size;
                    }

                    alreadyDownloadBytes += size;
                }
                if (this.webRequest != null)
                {
                    alreadyDownloadBytes += (long)this.webRequest.Request.downloadedBytes;
                }
                this.RemaningSize = alreadyDownloadBytes;
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
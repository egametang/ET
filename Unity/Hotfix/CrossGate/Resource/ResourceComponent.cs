using System;
using System.Collections.Generic;
using System.IO;
using ETModel;
using UnityEngine;

namespace ETHotfix
{
    [ObjectSystem]
    public sealed class ResourceComponentAwakeSystem : AwakeSystem<ResourceComponent>
    {
        public override void Awake(ResourceComponent self)
        {
            self.Awake();
        }
    }

    public sealed class ResourceComponent : Component
    {
        public static ResourceComponent Instance { get; private set; }

        //private readonly string url = "http://mylittlebucket.oss-cn-hangzhou.aliyuncs.com/";
        private readonly string url = "http://192.168.0.2:8080/";

        private readonly Dictionary<string, Sprite> spriteDict = new Dictionary<string, Sprite>();
        private readonly Queue<string> downloadQueue = new Queue<string>();
        private readonly string mapItemFolder = "mapitem";
        private readonly string animationFolder = "animation";
        
        private UnityWebRequestAsync webRequest;
        private bool downLoading;
        private string downloadingBundle;
        private string downloadPath;
        private string savePath;
        private byte[] bytes;

        public void Awake()
        {
            Instance = this;
        }

        public Sprite GetMapItemSprite(string pngname)
        {
            //有图片直接返回
            Sprite sprite = GetFromLocal(pngname);
            if (sprite != null) return sprite;

            //加入下载队列
            if (!this.downloadQueue.Contains(pngname))
            {
                this.downloadQueue.Enqueue(pngname);
                if (!downLoading) Download(mapItemFolder);
            }

            return null;
        }

        private Sprite GetFromLocal(string pngname)
        {
            //如果有则直接从字典中取出
            if (this.spriteDict.ContainsKey(pngname))
            {
                return spriteDict[pngname];
            }

            //如果本地存在则加载AB包
            string path = SqlConnectHelper.GetSavePath("Resource/" + mapItemFolder + "/") + pngname.ToLower() + ".so";
            FileInfo file = new FileInfo(path);
            if (file.Exists)
            {
                //如果该AB包正在被加载中则不允许同时加载
                //if (this.loadingBundle == pngname) return null;

                //获得本地AB包
                Sprite sp = LoadFromAB(path);
                AddToDictionary(pngname, sp);
                return sp;
            }

            return null;
        }

        private Sprite LoadFromAB(string path)
        {
            AssetBundle ab = AssetBundle.LoadFromFile(path);
            return (Sprite) ab.mainAsset;
        }

        private void AddToDictionary(string key, Sprite value)
        {
            if (!this.spriteDict.ContainsKey(key))
            {
                this.spriteDict.Add(key, value);
            }
        }

        private async void Download(string subfolderfromserver)
        {
            try
            {
                while (true)
                {
                    if (this.downloadQueue.Count == 0)
                    {
                        //全部下载完成
                        this.downLoading = false;
                        break;
                    }

                    //取出需要下载的物品
                    this.downLoading = true;
                    this.downloadingBundle = this.downloadQueue.Peek();
                    //Log.Debug("开始下载:" + this.downloadingBundle + ".so");

                    try
                    {
                        using (this.webRequest = ETModel.ComponentFactory.Create<UnityWebRequestAsync>())
                        {
                            //生成对应路径
                            this.downloadPath = this.url + subfolderfromserver + "/" + this.downloadingBundle + ".so";
                            this.savePath = SqlConnectHelper.GetSavePath("Resource/" + subfolderfromserver + "/") + this.downloadingBundle.ToLower() + ".so";
                            
                            //开始下载
                            await this.webRequest.DownloadAsync(this.downloadPath);
                            this.bytes = this.webRequest.Request.downloadHandler.data;
                            
                            //创建文件夹路径
                            if (!Directory.Exists(Path.GetDirectoryName(this.savePath)))
                            {
                                Directory.CreateDirectory(Path.GetDirectoryName(this.savePath));
                                //Log.Debug("创建文件夹:" + Path.GetDirectoryName(this.savePath));
                            }

                            //储存到客户端本地
                            using (FileStream fs = new FileStream(this.savePath, FileMode.Create))
                            {
                                fs.Write(this.bytes, 0, this.bytes.Length);
                                //Log.Debug("下载成功:" + this.downloadingBundle + ".so");
                            }

                            //保存到缓存内
                            AddToDictionary(this.downloadingBundle, (Sprite) AssetBundle.LoadFromMemory(this.bytes).mainAsset);

                            //从队列中移除
                            this.downloadQueue.Dequeue();
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error($"download sprite bundle error: {this.downloadingBundle}\n{e}");
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        public override void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }

            base.Dispose();

            Instance = null;

            this.downloadQueue.Clear();
            this.spriteDict.Clear();
            this.downLoading = false;
            this.downloadingBundle = "";
            this.downloadPath = "";
            this.savePath = "";
            this.bytes = null;
        }
    }
}
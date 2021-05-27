using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace ET
{
    public class UnityWebRequestRenewalUpdateSystem: UpdateSystem<UnityWebRequestRenewalAsync>
    {
        public override void Update(UnityWebRequestRenewalAsync self)
        {
            self.Update();
        }
    }

    /// <summary>
    /// 断点续传实现
    /// </summary>
    public class UnityWebRequestRenewalAsync: Entity
    {
        public class AcceptAllCertificate: CertificateHandler
        {
            protected override bool ValidateCertificate(byte[] certificateData)
            {
                return true;
            }
        }

        public static AcceptAllCertificate certificateHandler = new AcceptAllCertificate();

        public UnityWebRequest headRequest;

        public bool isCancel;

        public ETTask tcs;

        //请求资源类型
        private class RequestType
        {
            public const int None = 0; //未开始
            public const int Head = 1; //文件头
            public const int Data = 2; //文件数据
        }

        //当前请求资源类型
        private int requestType = RequestType.None;

        //多任务同时请求
        private readonly List<UnityWebRequest> dataRequests = new List<UnityWebRequest>();

        //当前下载的文件流 边下边存文件流
        private FileStream fileStream;

        //资源地址
        private string Url;

        //每个下载包长度
        private int packageLength = 1000000; //1M

        //同时开启任务最大个数
        private int maxCount = 20;

        //已经写入文件的大小
        private long byteWrites;

        //文件总大小
        private long totalBytes;

        //当前请求的位置
        private long downloadIndex;

        //文件下载是否出错
        private string dataError
        {
            get
            {
                foreach (UnityWebRequest webRequest in this.dataRequests)
                {
                    if (!string.IsNullOrEmpty(webRequest.error))
                    {
                        return webRequest.error;
                    }
                }

                return "";
            }
        }

        //批量开启任务下载
        private void DownloadPackages()
        {
            if (dataRequests.Count >= maxCount || this.downloadIndex == totalBytes - 1)
            {
                return;
            }

            //开启一个下载任务
            void DownloadPackage(long start, long end)
            {
                this.downloadIndex = end;
                Log.Debug($"Request Data ({start}~{end}):{Url}");
                UnityWebRequest request = UnityWebRequest.Get(Url);
                dataRequests.Add(request);
                request.certificateHandler = certificateHandler;
                request.SetRequestHeader("Range", $"bytes={start}-{end}");
                request.SendWebRequest();
            }

            //开启批量下载
            for (int i = dataRequests.Count; i < maxCount; i++)
            {
                long start = this.byteWrites + i * packageLength;
                long end = this.byteWrites + (i + 1) * packageLength - 1;
                if (end > this.totalBytes)
                {
                    end = this.totalBytes - 1;
                }

                DownloadPackage(start, end);
                if (end == this.totalBytes - 1)
                {
                    break;
                }
            }
        }

        //一次批量下载完成后写文件
        private void WritePackages()
        {
            //写入单个包
            void WritePackage(UnityWebRequest webRequest)
            {
                byte[] buff = webRequest.downloadHandler.data;
                if (buff != null && buff.Length > 0)
                {
                    this.fileStream.Write(buff, 0, buff.Length);
                    this.byteWrites += buff.Length;
                }

                Log.Debug($"write file Length:{byteWrites}");
            }

            //从第一个开始顺序写入
            while (this.dataRequests.Count > 0 && dataRequests[0].isDone)
            {
                UnityWebRequest first = dataRequests[0];
                dataRequests.RemoveAt(0);
                WritePackage(first);
                first.Dispose();
            }
        }

        //更新文件体下载
        private void UpdatePackages()
        {
            if (this.isCancel)
            {
                this.tcs.SetException(new Exception($"request data error: {dataError}"));
                return;
            }

            if (!string.IsNullOrEmpty(dataError))
            {
                this.tcs.SetException(new Exception($"request data error: {dataError}"));
                return;
            }

            this.WritePackages();
            if (this.byteWrites == this.totalBytes)
            {
                this.tcs.SetResult();
            }
            else
            {
                this.DownloadPackages();
            }
        }

        //更新文件头下载
        private void UpdateHead()
        {
            if (this.isCancel)
            {
                this.tcs.SetException(new Exception($"request error: {this.headRequest.error}"));
                return;
            }

            if (!this.headRequest.isDone)
            {
                return;
            }

            if (!string.IsNullOrEmpty(this.headRequest.error))
            {
                this.tcs.SetException(new Exception($"request error: {this.headRequest.error}"));
                return;
            }

            this.tcs.SetResult();
        }

        //检测是不是同一个文件
        private bool CheckSameFile(string modifiedTime)
        {
            string cacheValue = PlayerPrefs.GetString(Url);
            string currentValue = this.totalBytes + "|" + modifiedTime;
            if (cacheValue == currentValue)
            {
                return true;
            }

            PlayerPrefs.SetString(Url, currentValue);
            PlayerPrefs.Save();
            Log.Debug($"断点续传下载一个新的文件:{Url} cacheValue:{cacheValue} currentValue:{currentValue}");
            return false;
        }

        /// <summary>
        /// 断点续传入口
        /// </summary>
        /// <param name="url">文件下载地址</param>
        /// <param name="filePath">文件写入路径</param>
        /// <param name="packageLength">单个任务包体字节大小</param>
        /// <param name="maxCount">同时开启最大任务个数</param>
        /// <returns></returns>
        public async ETTask DownloadAsync(string url, string filePath, int packageLength = 1000000, int maxCount = 20)
        {
            try
            {
                url = url.Replace(" ", "%20");
                this.Url = url;
                this.packageLength = packageLength;
                this.maxCount = maxCount;
                Log.Debug("Web Request:" + url);

                #region Download File Header

                this.requestType = RequestType.Head;
                //下载文件头
                Log.Debug($"Request Head: {Url}");
                this.tcs = ETTask.Create(true);
                this.headRequest = UnityWebRequest.Head(Url);
                this.headRequest.SendWebRequest();
                await this.tcs;
                this.totalBytes = long.Parse(this.headRequest.GetResponseHeader("Content-Length"));
                string modifiedTime = this.headRequest.GetResponseHeader("Last-Modified");
                Log.Debug($"totalBytes: {this.totalBytes}");
                this.headRequest?.Dispose();
                this.headRequest = null;

                #endregion

                #region Check Local File

                //打开或创建
                fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
                //获取已下载长度
                this.byteWrites = fileStream.Length;
                //通过本地缓存的服务器文件修改时间和文件总长度检测服务器是否是同一个文件 不是同一个从头开始写入
                if (!CheckSameFile(modifiedTime))
                {
                    this.byteWrites = 0;
                }

                Log.Debug($"byteWrites: {this.byteWrites}");
                if (this.byteWrites == this.totalBytes)
                {
                    Log.Debug("已经下载完成2");
                    return;
                }

                //设置开始写入位置
                fileStream.Seek(this.byteWrites, SeekOrigin.Begin);

                #endregion

                #region Download File Data

                //下载文件数据
                requestType = RequestType.Data;
                Log.Debug($"Request Data: {Url}");
                this.tcs = ETTask.Create(true);
                this.DownloadPackages();
                await this.tcs;

                #endregion
            }
            catch (Exception e)
            {
                Log.Error($"下载:{Url} Exception:{e}");
                throw;
            }
        }

        //下载进度
        public float Progress
        {
            get
            {
                if (this.totalBytes == 0)
                {
                    return 0;
                }

                return (float) ((this.byteWrites + ByteDownloaded) / (double) this.totalBytes);
            }
        }

        //当前任务已经下载的长度
        public long ByteDownloaded
        {
            get
            {
                long length = 0;
                foreach (UnityWebRequest dataRequest in this.dataRequests)
                {
                    length += dataRequest.downloadHandler.data.Length;
                }

                return length;
            }
        }

        public void Update()
        {
            if (this.requestType == RequestType.Head)
            {
                this.UpdateHead();
            }

            if (this.requestType == RequestType.Data)
            {
                this.UpdatePackages();
            }
        }

        public override void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }

            base.Dispose();
            headRequest?.Dispose();
            headRequest = null;
            foreach (UnityWebRequest dataRequest in this.dataRequests)
            {
                dataRequest.Dispose();
            }

            dataRequests.Clear();
            this.fileStream?.Close();
            this.fileStream?.Dispose();
            this.fileStream = null;
            this.isCancel = false;
        }
    }
}
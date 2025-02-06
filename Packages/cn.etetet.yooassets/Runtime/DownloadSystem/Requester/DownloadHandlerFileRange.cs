using System;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace YooAsset
{
    /// <summary>
    /// 支持Unity2018版本的断点续传下载器
    /// </summary>
    internal class DownloadHandlerFileRange : DownloadHandlerScript
    {
        private string _fileSavePath;
        private long _fileTotalSize;
        private UnityWebRequest _webRequest;
        private FileStream _fileStream;

        private long _localFileSize = 0;
        private long _curFileSize = 0;


        public DownloadHandlerFileRange(string fileSavePath, long fileTotalSize, UnityWebRequest webRequest) : base(new byte[1024 * 1024])
        {
            _fileSavePath = fileSavePath;
            _fileTotalSize = fileTotalSize;
            _webRequest = webRequest;

            if (File.Exists(fileSavePath))
            {
                FileInfo fileInfo = new FileInfo(fileSavePath);
                _localFileSize = fileInfo.Length;
            }

            _fileStream = new FileStream(_fileSavePath, FileMode.Append, FileAccess.Write);
            _curFileSize = _localFileSize;
        }
        protected override bool ReceiveData(byte[] data, int dataLength)
        {
            if (data == null || dataLength == 0 || _webRequest.responseCode >= 400)
                return false;

            if (_fileStream == null)
                return false;

            _fileStream.Write(data, 0, dataLength);
            _curFileSize += dataLength;
            return true;
        }

        /// <summary>
        /// UnityWebRequest.downloadHandler.data
        /// </summary>
        protected override byte[] GetData()
        {
            return null;
        }

        /// <summary>
        /// UnityWebRequest.downloadHandler.text
        /// </summary>
        protected override string GetText()
        {
            return null;
        }

        /// <summary>
        /// UnityWebRequest.downloadProgress
        /// </summary>
        protected override float GetProgress()
        {
            return _fileTotalSize == 0 ? 0 : ((float)_curFileSize) / _fileTotalSize;
        }

        /// <summary>
        /// 释放下载句柄
        /// </summary>
        public void Cleanup()
        {
            if (_fileStream != null)
            {
                _fileStream.Flush();
                _fileStream.Dispose();
                _fileStream = null;
            }
        }
    }
}
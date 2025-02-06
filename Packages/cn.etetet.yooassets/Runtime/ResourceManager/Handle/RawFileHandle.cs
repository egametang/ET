using System;
using System.IO;
using System.Text;

namespace YooAsset
{
    public class RawFileHandle : HandleBase, IDisposable
    {
        private System.Action<RawFileHandle> _callback;

        internal RawFileHandle(ProviderBase provider) : base(provider)
        {
        }
        internal override void InvokeCallback()
        {
            _callback?.Invoke(this);
        }

        /// <summary>
        /// 完成委托
        /// </summary>
        public event System.Action<RawFileHandle> Completed
        {
            add
            {
                if (IsValidWithWarning == false)
                    throw new System.Exception($"{nameof(RawFileHandle)} is invalid");
                if (Provider.IsDone)
                    value.Invoke(this);
                else
                    _callback += value;
            }
            remove
            {
                if (IsValidWithWarning == false)
                    throw new System.Exception($"{nameof(RawFileHandle)} is invalid");
                _callback -= value;
            }
        }

        /// <summary>
        /// 等待异步执行完毕
        /// </summary>
        public void WaitForAsyncComplete()
        {
            if (IsValidWithWarning == false)
                return;
            Provider.WaitForAsyncComplete();
        }

        /// <summary>
        /// 释放资源句柄
        /// </summary>
        public void Release()
        {
            this.ReleaseInternal();
        }

        /// <summary>
        /// 释放资源句柄
        /// </summary>
        public void Dispose()
        {
            this.ReleaseInternal();
        }


        /// <summary>
        /// 获取原生文件的二进制数据
        /// </summary>
        public byte[] GetRawFileData()
        {
            if (IsValidWithWarning == false)
                return null;
            string filePath = Provider.RawFilePath;
            return FileUtility.ReadAllBytes(filePath);
        }

        /// <summary>
        /// 获取原生文件的文本数据
        /// </summary>
        public string GetRawFileText()
        {
            if (IsValidWithWarning == false)
                return null;
            string filePath = Provider.RawFilePath;
            return FileUtility.ReadAllText(filePath);
        }

        /// <summary>
        /// 获取原生文件的路径
        /// </summary>
        public string GetRawFilePath()
        {
            if (IsValidWithWarning == false)
                return string.Empty;
            return Provider.RawFilePath;
        }
    }
}
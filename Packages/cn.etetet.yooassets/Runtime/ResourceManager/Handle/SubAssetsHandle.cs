using System;
using System.Collections.Generic;

namespace YooAsset
{
    public sealed class SubAssetsHandle : HandleBase, IDisposable
    {
        private System.Action<SubAssetsHandle> _callback;

        internal SubAssetsHandle(ProviderBase provider) : base(provider)
        {
        }
        internal override void InvokeCallback()
        {
            _callback?.Invoke(this);
        }

        /// <summary>
        /// 完成委托
        /// </summary>
        public event System.Action<SubAssetsHandle> Completed
        {
            add
            {
                if (IsValidWithWarning == false)
                    throw new System.Exception($"{nameof(SubAssetsHandle)} is invalid");
                if (Provider.IsDone)
                    value.Invoke(this);
                else
                    _callback += value;
            }
            remove
            {
                if (IsValidWithWarning == false)
                    throw new System.Exception($"{nameof(SubAssetsHandle)} is invalid");
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
        /// 子资源对象集合
        /// </summary>
        public UnityEngine.Object[] AllAssetObjects
        {
            get
            {
                if (IsValidWithWarning == false)
                    return null;
                return Provider.AllAssetObjects;
            }
        }

        /// <summary>
        /// 获取子资源对象
        /// </summary>
        /// <typeparam name="TObject">子资源对象类型</typeparam>
        /// <param name="assetName">子资源对象名称</param>
        public TObject GetSubAssetObject<TObject>(string assetName) where TObject : UnityEngine.Object
        {
            if (IsValidWithWarning == false)
                return null;

            foreach (var assetObject in Provider.AllAssetObjects)
            {
                if (assetObject.name == assetName)
                    return assetObject as TObject;
            }

            YooLogger.Warning($"Not found sub asset object : {assetName}");
            return null;
        }

        /// <summary>
        /// 获取所有的子资源对象集合
        /// </summary>
        /// <typeparam name="TObject">子资源对象类型</typeparam>
        public TObject[] GetSubAssetObjects<TObject>() where TObject : UnityEngine.Object
        {
            if (IsValidWithWarning == false)
                return null;

            List<TObject> ret = new List<TObject>(Provider.AllAssetObjects.Length);
            foreach (var assetObject in Provider.AllAssetObjects)
            {
                var retObject = assetObject as TObject;
                if (retObject != null)
                    ret.Add(retObject);
            }
            return ret.ToArray();
        }
    }
}
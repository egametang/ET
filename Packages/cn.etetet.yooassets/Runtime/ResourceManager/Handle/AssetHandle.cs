using System;
using System.Collections.Generic;
using UnityEngine;

namespace YooAsset
{
    public sealed class AssetHandle : HandleBase, IDisposable
    {
        private System.Action<AssetHandle> _callback;

        internal AssetHandle(ProviderBase provider) : base(provider)
        {
        }
        internal override void InvokeCallback()
        {
            _callback?.Invoke(this);
        }

        /// <summary>
        /// 完成委托
        /// </summary>
        public event System.Action<AssetHandle> Completed
        {
            add
            {
                if (IsValidWithWarning == false)
                    throw new System.Exception($"{nameof(AssetHandle)} is invalid");
                if (Provider.IsDone)
                    value.Invoke(this);
                else
                    _callback += value;
            }
            remove
            {
                if (IsValidWithWarning == false)
                    throw new System.Exception($"{nameof(AssetHandle)} is invalid");
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
        /// 资源对象
        /// </summary>
        public UnityEngine.Object AssetObject
        {
            get
            {
                if (IsValidWithWarning == false)
                    return null;
                return Provider.AssetObject;
            }
        }

        /// <summary>
        /// 获取资源对象
        /// </summary>
        /// <typeparam name="TAsset">资源类型</typeparam>
        public TAsset GetAssetObject<TAsset>() where TAsset : UnityEngine.Object
        {
            if (IsValidWithWarning == false)
                return null;
            return Provider.AssetObject as TAsset;
        }

        /// <summary>
        /// 同步初始化游戏对象
        /// </summary>
        public GameObject InstantiateSync()
        {
            return InstantiateSyncInternal(false, Vector3.zero, Quaternion.identity, null, false);
        }
        public GameObject InstantiateSync(Transform parent)
        {
            return InstantiateSyncInternal(false, Vector3.zero, Quaternion.identity, parent, false);
        }
        public GameObject InstantiateSync(Transform parent, bool worldPositionStays)
        {
            return InstantiateSyncInternal(false, Vector3.zero, Quaternion.identity, parent, worldPositionStays);
        }
        public GameObject InstantiateSync(Vector3 position, Quaternion rotation)
        {
            return InstantiateSyncInternal(true, position, rotation, null, false);
        }
        public GameObject InstantiateSync(Vector3 position, Quaternion rotation, Transform parent)
        {
            return InstantiateSyncInternal(true, position, rotation, parent, false);
        }

        /// <summary>
        /// 异步初始化游戏对象
        /// </summary>
        public InstantiateOperation InstantiateAsync()
        {
            return InstantiateAsyncInternal(false, Vector3.zero, Quaternion.identity, null, false);
        }
        public InstantiateOperation InstantiateAsync(Transform parent)
        {
            return InstantiateAsyncInternal(false, Vector3.zero, Quaternion.identity, parent, false);
        }
        public InstantiateOperation InstantiateAsync(Transform parent, bool worldPositionStays)
        {
            return InstantiateAsyncInternal(false, Vector3.zero, Quaternion.identity, parent, worldPositionStays);
        }
        public InstantiateOperation InstantiateAsync(Vector3 position, Quaternion rotation)
        {
            return InstantiateAsyncInternal(true, position, rotation, null, false);
        }
        public InstantiateOperation InstantiateAsync(Vector3 position, Quaternion rotation, Transform parent)
        {
            return InstantiateAsyncInternal(true, position, rotation, parent, false);
        }

        private GameObject InstantiateSyncInternal(bool setPositionAndRotation, Vector3 position, Quaternion rotation, Transform parent, bool worldPositionStays)
        {
            if (IsValidWithWarning == false)
                return null;
            if (Provider.AssetObject == null)
                return null;

            return InstantiateOperation.InstantiateInternal(Provider.AssetObject, setPositionAndRotation, position, rotation, parent, worldPositionStays);
        }
        private InstantiateOperation InstantiateAsyncInternal(bool setPositionAndRotation, Vector3 position, Quaternion rotation, Transform parent, bool worldPositionStays)
        {
            string packageName = GetAssetInfo().PackageName;
            InstantiateOperation operation = new InstantiateOperation(this, setPositionAndRotation, position, rotation, parent, worldPositionStays);
            OperationSystem.StartOperation(packageName, operation);
            return operation;
        }
    }
}

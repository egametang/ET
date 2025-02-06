using UnityEngine;

namespace YooAsset
{
    public sealed class InstantiateOperation : AsyncOperationBase
    {
        private enum ESteps
        {
            None,
            Clone,
            Done,
        }

        private readonly AssetHandle _handle;
        private readonly bool _setPositionAndRotation;
        private readonly Vector3 _position;
        private readonly Quaternion _rotation;
        private readonly Transform _parent;
        private readonly bool _worldPositionStays;
        private ESteps _steps = ESteps.None;

        /// <summary>
        /// 实例化的游戏对象
        /// </summary>
        public GameObject Result = null;


        internal InstantiateOperation(AssetHandle handle, bool setPositionAndRotation, Vector3 position, Quaternion rotation, Transform parent, bool worldPositionStays)
        {
            _handle = handle;
            _setPositionAndRotation = setPositionAndRotation;
            _position = position;
            _rotation = rotation;
            _parent = parent;
            _worldPositionStays = worldPositionStays;
        }
        internal override void InternalOnStart()
        {
            _steps = ESteps.Clone;
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.Clone)
            {
                if (_handle.IsValidWithWarning == false)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = $"{nameof(AssetHandle)} is invalid.";
                    return;
                }

                if (_handle.IsDone == false)
                    return;

                if (_handle.AssetObject == null)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = $"{nameof(AssetHandle.AssetObject)} is null.";
                    return;
                }

                // 实例化游戏对象
                Result = InstantiateInternal(_handle.AssetObject, _setPositionAndRotation, _position, _rotation, _parent, _worldPositionStays);

                _steps = ESteps.Done;
                Status = EOperationStatus.Succeed;
            }
        }

        /// <summary>
        /// 取消实例化对象操作
        /// </summary>
        public void Cancel()
        {
            if (IsDone == false)
            {
                _steps = ESteps.Done;
                Status = EOperationStatus.Failed;
                Error = $"User cancelled !";
            }
        }

        /// <summary>
        /// 等待异步实例化结束
        /// </summary>
        public void WaitForAsyncComplete()
        {
            if (_steps == ESteps.Done)
                return;
            _handle.WaitForAsyncComplete();
            InternalOnUpdate();
        }

        internal static GameObject InstantiateInternal(UnityEngine.Object assetObject, bool setPositionAndRotation, Vector3 position, Quaternion rotation, Transform parent, bool worldPositionStays)
        {
            if (assetObject == null)
                return null;

            if (setPositionAndRotation)
            {
                if (parent != null)
                {
                    GameObject clone = UnityEngine.Object.Instantiate(assetObject as GameObject, position, rotation, parent);
                    return clone;
                }
                else
                {
                    GameObject clone = UnityEngine.Object.Instantiate(assetObject as GameObject, position, rotation);
                    return clone;
                }
            }
            else
            {
                if (parent != null)
                {
                    GameObject clone = UnityEngine.Object.Instantiate(assetObject as GameObject, parent, worldPositionStays);
                    return clone;
                }
                else
                {
                    GameObject clone = UnityEngine.Object.Instantiate(assetObject as GameObject);
                    return clone;
                }
            }
        }
    }
}
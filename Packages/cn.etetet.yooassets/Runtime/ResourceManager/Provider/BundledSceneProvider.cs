using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace YooAsset
{
    internal sealed class BundledSceneProvider : ProviderBase
    {
        public readonly LoadSceneMode SceneMode;
        private readonly bool _suspendLoad;
        private AsyncOperation _asyncOperation;

        public BundledSceneProvider(ResourceManager manager, string providerGUID, AssetInfo assetInfo, LoadSceneMode sceneMode, bool suspendLoad) : base(manager, providerGUID, assetInfo)
        {
            SceneMode = sceneMode;
            SceneName = Path.GetFileNameWithoutExtension(assetInfo.AssetPath);
            _suspendLoad = suspendLoad;
        }
        internal override void InternalOnStart()
        {
            DebugBeginRecording();
        }
        internal override void InternalOnUpdate()
        {
            if (IsDone)
                return;

            if (_steps == ESteps.None)
            {
                _steps = ESteps.CheckBundle;
            }

            // 1. 检测资源包
            if (_steps == ESteps.CheckBundle)
            {
                if (DependBundles.IsDone() == false)
                    return;
                if (OwnerBundle.IsDone() == false)
                    return;

                if (DependBundles.IsSucceed() == false)
                {
                    string error = DependBundles.GetLastError();
                    InvokeCompletion(error, EOperationStatus.Failed);
                    return;
                }

                if (OwnerBundle.Status != BundleLoaderBase.EStatus.Succeed)
                {
                    string error = OwnerBundle.LastError;
                    InvokeCompletion(error, EOperationStatus.Failed);
                    return;
                }

                _steps = ESteps.Loading;
            }

            // 2. 加载场景
            if (_steps == ESteps.Loading)
            {
                // 注意：如果场景不存在则返回NULL
                _asyncOperation = SceneManager.LoadSceneAsync(MainAssetInfo.AssetPath, SceneMode);
                if (_asyncOperation != null)
                {
                    _asyncOperation.allowSceneActivation = !_suspendLoad;
                    _asyncOperation.priority = 100;
                    SceneObject = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
                    _steps = ESteps.Checking;
                }
                else
                {
                    string error = $"Failed to load scene : {MainAssetInfo.AssetPath}";
                    YooLogger.Error(error);
                    InvokeCompletion(error, EOperationStatus.Failed);
                }
            }

            // 3. 检测加载结果
            if (_steps == ESteps.Checking)
            {
                Progress = _asyncOperation.progress;
                if (_asyncOperation.isDone)
                {
                    if (SceneObject.IsValid())
                    {
                        InvokeCompletion(string.Empty, EOperationStatus.Succeed);
                    }
                    else
                    {
                        string error = $"The load scene is invalid : {MainAssetInfo.AssetPath}";
                        YooLogger.Error(error);
                        InvokeCompletion(error, EOperationStatus.Failed);
                    }
                }
            }
        }

        /// <summary>
        /// 解除场景加载挂起操作
        /// </summary>
        public bool UnSuspendLoad()
        {
            if (_asyncOperation == null)
                return false;

            _asyncOperation.allowSceneActivation = true;
            return true;
        }
    }
}
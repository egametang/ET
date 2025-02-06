using UnityEngine;
using UnityEngine.SceneManagement;

namespace YooAsset
{
    /// <summary>
    /// 场景卸载异步操作类
    /// </summary>
    public sealed class UnloadSceneOperation : AsyncOperationBase
    {
        private enum ESteps
        {
            None,
            CheckError,
            PrepareDone,
            UnLoadScene,
            Checking,
            Done,
        }

        private ESteps _steps = ESteps.None;
        private readonly string _error;
        private readonly ProviderBase _provider;
        private AsyncOperation _asyncOp;

        internal UnloadSceneOperation(string error)
        {
            _error = error;
        }
        internal UnloadSceneOperation(ProviderBase provider)
        {
            _error = null;
            _provider = provider;
        }
        internal override void InternalOnStart()
        {
            _steps = ESteps.CheckError;
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.CheckError)
            {
                if (string.IsNullOrEmpty(_error) == false)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _error;
                    return;
                }

                _steps = ESteps.PrepareDone;
            }

            if (_steps == ESteps.PrepareDone)
            {
                if (_provider.IsDone == false)
                    return;

                if (_provider.SceneObject.IsValid() == false)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = "Scene is invalid !";
                    return;
                }

                if (_provider.SceneObject.isLoaded == false)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = "Scene is not loaded !";
                    return;
                }

                _steps = ESteps.UnLoadScene;
            }

            if (_steps == ESteps.UnLoadScene)
            {
                _asyncOp = SceneManager.UnloadSceneAsync(_provider.SceneObject);
                _provider.ResourceMgr.UnloadSubScene(_provider.SceneName);
                _provider.ResourceMgr.TryUnloadUnusedAsset(_provider.MainAssetInfo);
                _steps = ESteps.Checking;
            }

            if (_steps == ESteps.Checking)
            {
                Progress = _asyncOp.progress;
                if (_asyncOp.isDone == false)
                    return;

                _steps = ESteps.Done;
                Status = EOperationStatus.Succeed;
            }
        }
    }
}
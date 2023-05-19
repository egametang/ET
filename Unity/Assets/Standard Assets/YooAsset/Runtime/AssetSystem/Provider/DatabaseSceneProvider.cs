using UnityEngine;
using UnityEngine.SceneManagement;

namespace YooAsset
{
	internal sealed class DatabaseSceneProvider : ProviderBase
	{
		public readonly LoadSceneMode SceneMode;
		private readonly bool _activateOnLoad;
		private readonly int _priority;
		private AsyncOperation _asyncOp;

		public DatabaseSceneProvider(AssetSystemImpl impl, string providerGUID, AssetInfo assetInfo, LoadSceneMode sceneMode, bool activateOnLoad, int priority) : base(impl, providerGUID, assetInfo)
		{
			SceneMode = sceneMode;
			_activateOnLoad = activateOnLoad;
			_priority = priority;
		}
		public override void Update()
		{
#if UNITY_EDITOR
			if (IsDone)
				return;

			if (Status == EStatus.None)
			{
				Status = EStatus.CheckBundle;
			}

			// 1. 检测资源包
			if (Status == EStatus.CheckBundle)
			{
				if (IsWaitForAsyncComplete)
				{
					OwnerBundle.WaitForAsyncComplete();
				}

				if (OwnerBundle.IsDone() == false)
					return;

				if (OwnerBundle.Status != BundleLoaderBase.EStatus.Succeed)
				{
					Status = EStatus.Failed;
					LastError = OwnerBundle.LastError;
					InvokeCompletion();
					return;
				}

				Status = EStatus.Loading;
			}

			// 2. 加载资源对象
			if (Status == EStatus.Loading)
			{
				LoadSceneParameters loadSceneParameters = new LoadSceneParameters();
				loadSceneParameters.loadSceneMode = SceneMode;
				_asyncOp = UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(MainAssetInfo.AssetPath, loadSceneParameters);
				if (_asyncOp != null)
				{
					_asyncOp.allowSceneActivation = true;
					_asyncOp.priority = _priority;
					SceneObject = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
					Status = EStatus.Checking;
				}
				else
				{
					Status = EStatus.Failed;
					LastError = $"Failed to load scene : {MainAssetInfo.AssetPath}";
					YooLogger.Error(LastError);
					InvokeCompletion();
				}
			}

			// 3. 检测加载结果
			if (Status == EStatus.Checking)
			{
				Progress = _asyncOp.progress;
				if (_asyncOp.isDone)
				{				
					if (SceneObject.IsValid() && _activateOnLoad)
						SceneManager.SetActiveScene(SceneObject);

					Status = SceneObject.IsValid() ? EStatus.Succeed : EStatus.Failed;
					if (Status == EStatus.Failed)
					{
						LastError = $"The loaded scene is invalid : {MainAssetInfo.AssetPath}";
						YooLogger.Error(LastError);
					}
					InvokeCompletion();
				}
			}
#endif
		}
	}
}
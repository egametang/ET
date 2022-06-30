using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace YooAsset
{
	internal sealed class BundledSceneProvider : BundledProvider
	{
		public readonly LoadSceneMode SceneMode;
		private readonly string _sceneName;
		private readonly bool _activateOnLoad;
		private readonly int _priority;
		private AsyncOperation _asyncOp;
		public override float Progress
		{
			get
			{
				if (_asyncOp == null)
					return 0;
				return _asyncOp.progress;
			}
		}

		public BundledSceneProvider(AssetInfo assetInfo, LoadSceneMode sceneMode, bool activateOnLoad, int priority) : base(assetInfo)
		{
			SceneMode = sceneMode;
			_sceneName = Path.GetFileNameWithoutExtension(assetInfo.AssetPath);
			_activateOnLoad = activateOnLoad;
			_priority = priority;
		}
		public override void Update()
		{
			if (IsDone)
				return;

			if (Status == EStatus.None)
			{
				Status = EStatus.CheckBundle;
			}

			// 1. 检测资源包
			if (Status == EStatus.CheckBundle)
			{
				if (DependBundleGroup.IsDone() == false)
					return;
				if (OwnerBundle.IsDone() == false)
					return;

				if (DependBundleGroup.IsSucceed() == false)
				{
					Status = EStatus.Fail;
					LastError = DependBundleGroup.GetLastError();
					InvokeCompletion();
					return;
				}

				if (OwnerBundle.Status != AssetBundleLoaderBase.EStatus.Succeed)
				{
					Status = EStatus.Fail;
					LastError = OwnerBundle.LastError;
					InvokeCompletion();
					return;
				}

				Status = EStatus.Loading;
			}

			// 2. 加载场景
			if (Status == EStatus.Loading)
			{
				_asyncOp = SceneManager.LoadSceneAsync(_sceneName, SceneMode);
				if (_asyncOp != null)
				{
					_asyncOp.allowSceneActivation = true;
					_asyncOp.priority = _priority;
					Status = EStatus.Checking;
				}
				else
				{
					Status = EStatus.Fail;
					LastError = $"Failed to load scene : {_sceneName}";
					YooLogger.Error(LastError);
					InvokeCompletion();
				}
			}

			// 3. 检测加载结果
			if (Status == EStatus.Checking)
			{
				if (_asyncOp.isDone)
				{
					SceneObject = SceneManager.GetSceneByName(_sceneName);
					if (SceneObject.IsValid() && _activateOnLoad)
						SceneManager.SetActiveScene(SceneObject);

					Status = SceneObject.IsValid() ? EStatus.Success : EStatus.Fail;
					if(Status == EStatus.Fail)
					{
						LastError = $"The load scene is invalid : {MainAssetInfo.AssetPath}";
						YooLogger.Error(LastError);
					}
					InvokeCompletion();
				}
			}
		}
	}
}
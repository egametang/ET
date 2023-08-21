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
		private readonly string _sceneName;
		private readonly bool _suspendLoad;
		private readonly int _priority;
		private AsyncOperation _asyncOperation;

		public BundledSceneProvider(AssetSystemImpl impl, string providerGUID, AssetInfo assetInfo, LoadSceneMode sceneMode, bool suspendLoad, int priority) : base(impl, providerGUID, assetInfo)
		{
			SceneMode = sceneMode;
			_sceneName = Path.GetFileNameWithoutExtension(assetInfo.AssetPath);
			_suspendLoad = suspendLoad;
			_priority = priority;
		}
		public override void Update()
		{
			DebugBeginRecording();

			if (IsDone)
				return;

			if (Status == EStatus.None)
			{
				Status = EStatus.CheckBundle;
			}

			// 1. 检测资源包
			if (Status == EStatus.CheckBundle)
			{
				if (DependBundles.IsDone() == false)
					return;
				if (OwnerBundle.IsDone() == false)
					return;

				if (DependBundles.IsSucceed() == false)
				{
					Status = EStatus.Failed;
					LastError = DependBundles.GetLastError();
					InvokeCompletion();
					return;
				}

				if (OwnerBundle.Status != BundleLoaderBase.EStatus.Succeed)
				{
					Status = EStatus.Failed;
					LastError = OwnerBundle.LastError;
					InvokeCompletion();
					return;
				}

				Status = EStatus.Loading;
			}

			// 2. 加载场景
			if (Status == EStatus.Loading)
			{
				// 注意：如果场景不存在则返回NULL
				_asyncOperation = SceneManager.LoadSceneAsync(MainAssetInfo.AssetPath, SceneMode);
				if (_asyncOperation != null)
				{
					_asyncOperation.allowSceneActivation = !_suspendLoad;
					_asyncOperation.priority = _priority;
					SceneObject = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
					Status = EStatus.Checking;
				}
				else
				{
					Status = EStatus.Failed;
					LastError = $"Failed to load scene : {_sceneName}";
					YooLogger.Error(LastError);
					InvokeCompletion();
				}
			}

			// 3. 检测加载结果
			if (Status == EStatus.Checking)
			{
				Progress = _asyncOperation.progress;
				if (_asyncOperation.isDone)
				{
					Status = SceneObject.IsValid() ? EStatus.Succeed : EStatus.Failed;
					if (Status == EStatus.Failed)
					{
						LastError = $"The load scene is invalid : {MainAssetInfo.AssetPath}";
						YooLogger.Error(LastError);
					}
					InvokeCompletion();
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
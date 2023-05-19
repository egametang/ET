using UnityEngine.SceneManagement;

namespace YooAsset
{
	public class SceneOperationHandle : OperationHandleBase
	{	
		private System.Action<SceneOperationHandle> _callback;
		internal string PackageName { set; get; }

		internal SceneOperationHandle(ProviderBase provider) : base(provider)
		{
		}
		internal override void InvokeCallback()
		{
			_callback?.Invoke(this);
		}

		/// <summary>
		/// 完成委托
		/// </summary>
		public event System.Action<SceneOperationHandle> Completed
		{
			add
			{
				if (IsValidWithWarning == false)
					throw new System.Exception($"{nameof(SceneOperationHandle)} is invalid");
				if (Provider.IsDone)
					value.Invoke(this);
				else
					_callback += value;
			}
			remove
			{
				if (IsValidWithWarning == false)
					throw new System.Exception($"{nameof(SceneOperationHandle)} is invalid");
				_callback -= value;
			}
		}

		/// <summary>
		/// 场景对象
		/// </summary>
		public Scene SceneObject
		{
			get
			{
				if (IsValidWithWarning == false)
					return new Scene();
				return Provider.SceneObject;
			}
		}

		/// <summary>
		/// 激活场景
		/// </summary>
		public bool ActivateScene()
		{
			if (IsValidWithWarning == false)
				return false;

			if (SceneObject.IsValid() && SceneObject.isLoaded)
			{
				return SceneManager.SetActiveScene(SceneObject);
			}
			else
			{
				YooLogger.Warning($"Scene is invalid or not loaded : {SceneObject.name}");
				return false;
			}
		}

		/// <summary>
		/// 是否为主场景
		/// </summary>
		public bool IsMainScene()
		{
			if (IsValidWithWarning == false)
				return false;

			if (Provider is DatabaseSceneProvider)
			{
				var temp = Provider as DatabaseSceneProvider;
				return temp.SceneMode == LoadSceneMode.Single;
			}
			else if (Provider is BundledSceneProvider)
			{
				var temp = Provider as BundledSceneProvider;
				return temp.SceneMode == LoadSceneMode.Single;
			}
			else
			{
				throw new System.NotImplementedException();
			}
		}

		/// <summary>
		/// 异步卸载子场景
		/// </summary>
		public UnloadSceneOperation UnloadAsync()
		{
			// 如果句柄无效
			if (IsValidWithWarning == false)
			{
				string error = $"{nameof(SceneOperationHandle)} is invalid.";
				var operation = new UnloadSceneOperation(error);
				OperationSystem.StartOperation(operation);
				return operation;
			}

			// 如果是主场景
			if (IsMainScene())
			{
				string error = $"Cannot unload main scene. Use {nameof(YooAssets.LoadSceneAsync)} method to change the main scene !";
				YooLogger.Error(error);
				var operation = new UnloadSceneOperation(error);
				OperationSystem.StartOperation(operation);
				return operation;
			}

			// 卸载子场景
			Scene sceneObject = SceneObject;
			Provider.Impl.UnloadSubScene(Provider);
			{
				var operation = new UnloadSceneOperation(sceneObject);
				OperationSystem.StartOperation(operation);
				return operation;
			}
		}
	}
}
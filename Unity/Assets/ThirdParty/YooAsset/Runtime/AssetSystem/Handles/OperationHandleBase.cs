using System.Collections;

namespace YooAsset
{
	public abstract class OperationHandleBase : IEnumerator
	{
		private readonly AssetInfo _assetInfo;
		internal ProviderBase Provider { private set; get; }

		internal OperationHandleBase(ProviderBase provider)
		{
			Provider = provider;
			_assetInfo = provider.MainAssetInfo;
		}
		internal abstract void InvokeCallback();

		/// <summary>
		/// 当前状态
		/// </summary>
		public EOperationStatus Status
		{
			get
			{
				if (IsValid == false)
					return EOperationStatus.None;
				if (Provider.Status == ProviderBase.EStatus.Fail)
					return EOperationStatus.Failed;
				else if (Provider.Status == ProviderBase.EStatus.Success)
					return EOperationStatus.Succeed;
				else
					return EOperationStatus.None;
			}
		}

		/// <summary>
		/// 最近的错误信息
		/// </summary>
		public string LastError
		{
			get
			{
				if (IsValid == false)
					return string.Empty;
				return Provider.LastError;
			}
		}

		/// <summary>
		/// 加载进度
		/// </summary>
		public float Progress
		{
			get
			{
				if (IsValid == false)
					return 0;
				return Provider.Progress;
			}
		}

		/// <summary>
		/// 是否加载完毕
		/// </summary>
		public bool IsDone
		{
			get
			{
				if (IsValid == false)
					return false;
				return Provider.IsDone;
			}
		}

		/// <summary>
		/// 句柄是否有效
		/// </summary>
		public bool IsValid
		{
			get
			{
				if (Provider != null && Provider.IsDestroyed == false)
				{
					return true;
				}
				else
				{
					if (Provider == null)
						YooLogger.Warning($"Operation handle is released : {_assetInfo.AssetPath}");
					else if (Provider.IsDestroyed)
						YooLogger.Warning($"Provider is destroyed : {_assetInfo.AssetPath}");
					return false;
				}
			}
		}

		/// <summary>
		/// 句柄是否有效
		/// </summary>
		public bool IsValidNoWarning
		{
			get
			{
				if (Provider != null && Provider.IsDestroyed == false)
					return true;
				else
					return false;
			}
		}

		/// <summary>
		/// 释放句柄
		/// </summary>
		internal void ReleaseInternal()
		{
			if (IsValid == false)
				return;
			Provider.ReleaseHandle(this);
			Provider = null;
		}

		#region 异步操作相关
		/// <summary>
		/// 异步操作任务
		/// </summary>
		public System.Threading.Tasks.Task Task
		{
			get { return Provider.Task; }
		}

		// 协程相关
		bool IEnumerator.MoveNext()
		{
			return !IsDone;
		}
		void IEnumerator.Reset()
		{
		}
		object IEnumerator.Current
		{
			get { return Provider; }
		}
		#endregion
	}
}
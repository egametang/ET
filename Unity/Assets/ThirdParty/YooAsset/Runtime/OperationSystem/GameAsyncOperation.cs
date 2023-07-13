
namespace YooAsset
{
	public abstract class GameAsyncOperation : AsyncOperationBase
	{
		internal override void Start()
		{
			OnStart();
		}
		internal override void Update()
		{
			OnUpdate();
		}

		/// <summary>
		/// 异步操作开始
		/// </summary>
		protected abstract void OnStart();

		/// <summary>
		/// 异步操作更新
		/// </summary>
		protected abstract void OnUpdate();

		/// <summary>
		/// 异步操作系统是否繁忙
		/// </summary>
		protected bool IsBusy()
		{
			return OperationSystem.IsBusy;
		}
	}
}
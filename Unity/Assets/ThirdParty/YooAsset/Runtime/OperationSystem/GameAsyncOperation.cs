
namespace YooAsset
{
	public class GameAsyncOperation : AsyncOperationBase
	{
		internal override void Start()
		{
			OnStart();
		}
		internal override void Update()
		{
			OnUpdate();
		}

		protected virtual void OnStart() { }
		protected virtual void OnUpdate() { }
	}
}
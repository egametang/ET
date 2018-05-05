using System.Threading.Tasks;

namespace ETModel
{
	public struct ActorTask
	{
		public ActorProxy proxy;
		
		public IActorMessage message;
		
		public TaskCompletionSource<IResponse> Tcs;

		public async Task<IResponse> Run()
		{
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(this.proxy.Address);

			this.message.ActorId = this.proxy.ActorId;
			IResponse response = await session.Call(message);

			if (response.Error != ErrorCode.ERR_NotFoundActor)
			{
				if (this.Tcs != null)
				{
					this.Tcs?.SetResult(response);
				}
			}
			return response;
		}

		public void RunFail(int error)
		{
			this.Tcs?.SetException(new RpcException(error, ""));
		}
	}
}
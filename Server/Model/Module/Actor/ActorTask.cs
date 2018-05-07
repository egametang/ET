using System.Threading.Tasks;

namespace ETModel
{
	public struct ActorTask
	{
		public ActorMessageSender MessageSender;
		
		public IActorMessage message;
		
		public TaskCompletionSource<IResponse> Tcs;

		public async Task<IResponse> Run()
		{
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(this.MessageSender.Address);

			this.message.ActorId = this.MessageSender.ActorId;
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
using System;
using System.Threading.Tasks;

namespace ETModel
{
	public struct ActorTask
	{
		public ActorProxy proxy;
		
		public IMessage message;
		
		public TaskCompletionSource<IResponse> Tcs;

		public async Task<IResponse> Run()
		{
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(this.proxy.Address);
			OpcodeTypeComponent opcodeTypeComponent = session.Network.Entity.GetComponent<OpcodeTypeComponent>();

			ushort opcode = opcodeTypeComponent.GetOpcode(message.GetType());
			byte[] requestBytes = session.Network.MessagePacker.SerializeToByteArray(message);

			ActorRequest actorRequest = new ActorRequest() { Id = this.proxy.Id, Op = opcode, AMessage = requestBytes };

			ActorResponse actorResponse = (ActorResponse)await session.Call(actorRequest, this.proxy.CancellationTokenSource.Token);
			
			if (actorResponse.Error != ErrorCode.ERR_NotFoundActor)
			{
				if (this.Tcs != null)
				{
					Type type = opcodeTypeComponent.GetType(actorResponse.Op);
					IResponse response = (IResponse) session.Network.MessagePacker.DeserializeFrom(type, actorResponse.AMessage);
					this.Tcs?.SetResult(response);
				}
			}
			return actorResponse;
		}

		public void RunFail(int error)
		{
			this.Tcs?.SetException(new RpcException(error, ""));
		}
	}
}
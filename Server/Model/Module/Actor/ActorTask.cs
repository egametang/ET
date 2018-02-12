using System.Threading.Tasks;

namespace Model
{
	public struct ActorTask
	{
		public ActorProxy proxy;
		
		public MessageObject message;
		
		public TaskCompletionSource<IResponse> Tcs;

		public async Task<IResponse> Run()
		{
			ActorRequest request = new ActorRequest() { Id = this.proxy.Id, AMessage = this.message };
			ActorResponse response = (ActorResponse)await this.proxy.RealCall(request, this.proxy.CancellationTokenSource.Token);
			if (response.Error != ErrorCode.ERR_NotFoundActor)
			{
				this.Tcs?.SetResult((IResponse)response.AMessage);
			}
			return response;
		}

		public void RunFail(int error)
		{
			this.Tcs?.SetException(new RpcException(error, ""));
		}
	}
}
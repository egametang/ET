using System.Threading.Tasks;

namespace ETModel
{
	public struct ActorTask
	{
		public IActorRequest ActorRequest;
		
		public TaskCompletionSource<IActorLocationResponse> Tcs;

		public ActorTask(IActorLocationMessage actorRequest)
		{
			this.ActorRequest = actorRequest;
			this.Tcs = null;
		}
		
		public ActorTask(IActorLocationRequest actorRequest, TaskCompletionSource<IActorLocationResponse> tcs)
		{
			this.ActorRequest = actorRequest;
			this.Tcs = tcs;
		}
	}
}
using System.Threading.Tasks;

namespace ETModel
{
	public struct ActorTask
	{
		public IActorRequest ActorRequest;
		
		public TaskCompletionSource<IActorResponse> Tcs;

		public ActorTask(IActorRequest actorRequest)
		{
			this.ActorRequest = actorRequest;
			this.Tcs = null;
		}
		
		public ActorTask(IActorRequest actorRequest, TaskCompletionSource<IActorResponse> tcs)
		{
			this.ActorRequest = actorRequest;
			this.Tcs = tcs;
		}
	}
}
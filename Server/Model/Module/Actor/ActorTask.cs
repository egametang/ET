using System.Threading.Tasks;

namespace ETModel
{
	public struct ActorTask
	{
		public IActorMessage ActorMessage;
		
		public TaskCompletionSource<IResponse> Tcs;

		public ActorTask(IActorMessage actorMessage)
		{
			this.ActorMessage = actorMessage;
			this.Tcs = null;
		}
		
		public ActorTask(IActorMessage actorMessage, TaskCompletionSource<IResponse> tcs)
		{
			this.ActorMessage = actorMessage;
			this.Tcs = tcs;
		}
	}
}
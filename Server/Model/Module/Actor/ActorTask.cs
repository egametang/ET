namespace ETModel
{
	public struct ActorTask
	{
		public IActorRequest ActorRequest;
		
		public ETTaskCompletionSource<IActorLocationResponse> Tcs;

		public ActorTask(IActorLocationMessage actorRequest)
		{
			this.ActorRequest = actorRequest;
			this.Tcs = null;
		}
		
		public ActorTask(IActorLocationRequest actorRequest, ETTaskCompletionSource<IActorLocationResponse> tcs)
		{
			this.ActorRequest = actorRequest;
			this.Tcs = tcs;
		}
	}
}
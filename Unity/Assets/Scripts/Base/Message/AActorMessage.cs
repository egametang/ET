namespace Model
{
	public abstract class AActorMessage : AMessage
	{
	}

	public abstract class AActorRequest : ARequest
	{
	}

	public abstract class AActorResponse : AResponse
	{
	}

	public abstract class AFrameMessage : AActorMessage
	{
		public long Id;
	}
}
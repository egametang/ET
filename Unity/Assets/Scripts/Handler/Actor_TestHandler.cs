namespace Model
{
	[MessageHandler((int)Opcode.Actor_Test)]
	public class Actor_TestHandler : AMHandler<Actor_Test>
	{
		protected override void Run(Actor_Test message)
		{
			Log.Debug(message.Info);
		}
	}
}

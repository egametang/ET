using ETModel;

namespace ETHotfix
{
	[MessageHandler]
	public class Actor_TestHandler : AMHandler<Actor_Test>
	{
		protected override void Run(ETModel.Session session, Actor_Test message)
		{
			Log.Debug(message.Info);
		}
	}
}

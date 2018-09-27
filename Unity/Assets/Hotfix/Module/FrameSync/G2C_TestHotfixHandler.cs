using ETModel;

namespace ETHotfix
{
	[MessageHandler]
	public class G2C_TestHotfixMessageHandler : AMHandler<G2C_TestHotfixMessage>
	{
		protected override void Run(ETModel.Session session, G2C_TestHotfixMessage message)
		{
			Log.Debug(message.Info);
		}
	}
}
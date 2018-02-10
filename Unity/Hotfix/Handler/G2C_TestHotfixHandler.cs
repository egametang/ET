using Model;

namespace Hotfix
{
	[MessageHandler]
	public class G2C_TestHotfixMessageHandler : AMHandler<G2C_TestHotfixMessage>
	{
		protected override void Run(Session session, G2C_TestHotfixMessage message)
		{
			Log.Debug(message.Info);
		}
	}
}
using Model;

namespace Hotfix
{
	[MessageHandler(Opcode.R2C_ServerLog)]
	public class R2C_ServerLogHandler: AMHandler<R2C_ServerLog>
	{
		protected override void Run(Session session, R2C_ServerLog message)
		{
			Log.Debug("111111111111111111111111");
		}
	}
}

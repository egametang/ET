using Base;
using Model;

namespace Controller
{
	[MessageHandler(AppType.Client)]
	public class S2C_ServerLogEvent: AMEvent<S2C_ServerLog>
	{
		protected override void Run(Entity scene, S2C_ServerLog message)
		{
			Log.Debug(message.Log);
		}
	}
}

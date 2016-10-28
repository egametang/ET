using Base;
using Model;

namespace Controller
{
	[MessageHandler(AppType.Client)]
	public class R2C_ServerLogHandler: AMHandler<R2C_ServerLog>
	{
		protected override void Run(Session scene, R2C_ServerLog message)
		{
			Log.Debug($"[{message.AppType}][{message.AppId}] [{message.Type}] {message.Log}");
		}
	}
}

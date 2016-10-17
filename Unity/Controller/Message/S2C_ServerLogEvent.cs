using Base;
using Model;

namespace Controller
{
	public class S2C_ServerLogEvent: AMEvent<S2C_ServerLog>
	{
		protected override void Run(Entity scene, S2C_ServerLog message, uint rpcId)
		{
			Log.Debug(message.Log);
		}
	}
}

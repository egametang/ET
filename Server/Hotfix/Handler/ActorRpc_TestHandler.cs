using System;
using System.Threading.Tasks;
using Model;

namespace Hotfix
{
	[ActorMessageHandler(AppType.Map)]
	public class ActorRpc_TestRequestHandler : AMActorRpcHandler<Unit, ActorRpc_TestRequest, ActorRpc_TestResponse>
	{
		protected override async Task<bool> Run(Unit entity, ActorRpc_TestRequest message, Action<ActorRpc_TestResponse> reply)
		{
			Log.Info(message.request);
			reply(new ActorRpc_TestResponse() {response = "response actor rpc"});
			return true;
		}
	}
}
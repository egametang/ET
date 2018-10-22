using System;
using System.Threading.Tasks;
using ETModel;

namespace ETHotfix
{
	[ActorMessageHandler(AppType.Map)]
	public class C2M_TestRequestHandler : AMActorLocationRpcHandler<Unit, C2M_TestRequest, M2C_TestResponse>
	{
		protected override async ETTask Run(Unit unit, C2M_TestRequest message, Action<M2C_TestResponse> reply)
		{
			await ETTask.CompletedTask;
			reply(new M2C_TestResponse() {Response = "response actor rpc"});
		}
	}
}
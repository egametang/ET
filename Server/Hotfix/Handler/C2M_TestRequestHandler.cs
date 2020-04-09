using System;
using ETModel;

namespace ETHotfix
{
	[ActorMessageHandler(AppType.Map)]
	public class C2M_TestRequestHandler : AMActorLocationRpcHandler<Unit, C2M_TestRequest, M2C_TestResponse>
	{
		protected override async ETTask Run(Unit unit, C2M_TestRequest message, M2C_TestResponse response, Action reply)
		{
			response.Response = "response actor rpc";
			reply();
			await ETTask.CompletedTask;
		}
	}
}
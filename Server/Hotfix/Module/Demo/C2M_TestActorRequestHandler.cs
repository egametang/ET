using System;
using ETModel;

namespace ETHotfix
{
	[ActorMessageHandler(AppType.Map)]
	public class C2M_TestActorRequestHandler : AMActorLocationRpcHandler<Unit, C2M_TestActorRequest, M2C_TestActorResponse>
	{
		protected override async ETTask Run(Unit unit, C2M_TestActorRequest message, M2C_TestActorResponse response, Action reply)
		{
			response.Info = "actor rpc response";
			reply();
			await ETTask.CompletedTask;
		}
	}
}
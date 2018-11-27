﻿using System;
using ETModel;

namespace ETHotfix
{
	[ActorMessageHandler(AppType.Map)]
	public class C2M_TestActorRequestHandler : AMActorLocationRpcHandler<Unit, C2M_TestActorRequest, M2C_TestActorResponse>
	{
		protected override async ETTask Run(Unit unit, C2M_TestActorRequest message, Action<M2C_TestActorResponse> reply)
		{
			reply(new M2C_TestActorResponse(){Info = "actor rpc response"});
			await ETTask.CompletedTask;
		}
	}
}
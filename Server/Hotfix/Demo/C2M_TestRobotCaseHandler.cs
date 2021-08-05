using System;

namespace ET
{
	public class C2M_TestRobotCaseHandler : AMActorLocationRpcHandler<Unit, C2M_TestRobotCase, M2C_TestRobotCase>
	{
		protected override async ETTask Run(Unit unit, C2M_TestRobotCase request, M2C_TestRobotCase response, Action reply)
		{
			response.N = request.N;
			reply();
			await ETTask.CompletedTask;
		}
	}
}
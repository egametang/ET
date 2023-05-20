using System;

namespace ET.Server
{
	public static partial class C2M_TestRobotCaseHandler
	{
		[ActorMessageLocationHandler(SceneType.Map)]
		private static async ETTask Run(Unit unit, C2M_TestRobotCase request, M2C_TestRobotCase response)
		{
			response.N = request.N;
			await ETTask.CompletedTask;
		}
	}
}
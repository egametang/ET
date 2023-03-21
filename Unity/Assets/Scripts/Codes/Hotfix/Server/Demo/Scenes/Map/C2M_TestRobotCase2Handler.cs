using System;

namespace ET.Server
{
	[ActorMessageHandler(SceneType.Map)]
	public class C2M_TestRobotCase2Handler : AMActorLocationHandler<Unit, C2M_TestRobotCase2>
	{
		protected override async ETTask Run(Unit unit, C2M_TestRobotCase2 message)
		{
			MessageHelper.SendToClient(unit, new M2C_TestRobotCase2() {N = message.N});
			await ETTask.CompletedTask;
		}
	}
}
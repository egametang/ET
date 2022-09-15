using System.Collections.Generic;
using Unity.Mathematics;

namespace ET.Server
{
	[ActorMessageHandler(SceneType.Map)]
	public class C2M_PathfindingResultHandler : AMActorLocationHandler<Unit, C2M_PathfindingResult>
	{
		protected override async ETTask Run(Unit unit, C2M_PathfindingResult message)
		{
			float3 target = new float3(message.X, message.Y, message.Z);

			unit.FindPathMoveToAsync(target).Coroutine();
			
			await ETTask.CompletedTask;
		}
	}
}
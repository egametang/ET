
namespace ET.Server
{
	public static partial class C2M_PathfindingResultHandler
	{
		[ActorMessageLocationHandler(SceneType.Map)]
		private static async ETTask Run(Unit unit, C2M_PathfindingResult message)
		{
			unit.FindPathMoveToAsync(message.Position).Coroutine();
			await ETTask.CompletedTask;
		}
	}
}
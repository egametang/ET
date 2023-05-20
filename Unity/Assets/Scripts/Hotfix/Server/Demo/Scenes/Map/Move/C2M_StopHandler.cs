namespace ET.Server
{
	public static partial class C2M_StopHandler
	{
		[ActorMessageLocationHandler(SceneType.Map)]
		private static async ETTask Run(Unit unit, C2M_Stop message)
		{
			unit.Stop(1);
			await ETTask.CompletedTask;
		}
	}
}
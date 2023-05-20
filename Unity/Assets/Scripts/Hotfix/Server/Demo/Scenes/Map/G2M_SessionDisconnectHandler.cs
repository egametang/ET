

namespace ET.Server
{
	public static partial class G2M_SessionDisconnectHandler
	{
		[ActorMessageLocationHandler(SceneType.Map)]
		private static async ETTask Run(Unit unit, G2M_SessionDisconnect message)
		{
			await ETTask.CompletedTask;
		}
	}
}
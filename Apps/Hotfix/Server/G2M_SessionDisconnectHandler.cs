

namespace ET.Server
{
	[ActorMessageHandler(SceneType.Map)]
	public class G2M_SessionDisconnectHandler : AMActorLocationHandler<Unit, G2M_SessionDisconnect>
	{
		protected override async ETTask Run(Unit unit, G2M_SessionDisconnect message)
		{
			await ETTask.CompletedTask;
		}
	}
}
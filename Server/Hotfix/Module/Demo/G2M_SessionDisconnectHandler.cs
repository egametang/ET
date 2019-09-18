using ETModel;

namespace ETHotfix
{
	[ActorMessageHandler(AppType.Map)]
	public class G2M_SessionDisconnectHandler : AMActorLocationHandler<Unit, G2M_SessionDisconnect>
	{
		protected override async ETTask Run(Unit unit, G2M_SessionDisconnect message)
		{
			unit.GetComponent<UnitGateComponent>().IsDisconnect = true;
			await ETTask.CompletedTask;
		}
	}
}
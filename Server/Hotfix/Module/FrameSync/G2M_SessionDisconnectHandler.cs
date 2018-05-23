using System.Threading.Tasks;
using ETModel;

namespace ETHotfix
{
	[ActorMessageHandler(AppType.Map)]
	public class G2M_SessionDisconnectHandler : AMActorHandler<Unit, G2M_SessionDisconnect>
	{
		protected override async Task Run(Unit unit, G2M_SessionDisconnect message)
		{
			unit.GetComponent<UnitGateComponent>().IsDisconnect = true;
			await Task.CompletedTask;
		}
	}
}
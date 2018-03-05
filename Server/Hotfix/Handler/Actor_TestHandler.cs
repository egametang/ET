using System.Threading.Tasks;
using ETModel;

namespace ETHotfix
{
	[ActorMessageHandler(AppType.Map)]
	public class Actor_TestHandler : AMActorHandler<Unit, Actor_Test>
	{
		protected override async Task Run(Unit unit, Actor_Test message)
		{
			Log.Debug(message.Info);
			await Task.CompletedTask;
			unit.GetComponent<UnitGateComponent>().GetActorProxy().Send(message);
		}
	}
}
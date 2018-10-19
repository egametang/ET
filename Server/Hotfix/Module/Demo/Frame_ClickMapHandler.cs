using ETModel;
using PF;
using ABPath = ETModel.ABPath;

namespace ETHotfix
{
	[ActorMessageHandler(AppType.Map)]
	public class Frame_ClickMapHandler : AMActorLocationHandler<Unit, Frame_ClickMap>
	{
		protected override void Run(Unit unit, Frame_ClickMap message)
		{
			Vector3 target = new Vector3(message.X, message.Y, message.Z);
			unit.GetComponent<UnitPathComponent>().MoveTo(target).NoAwait();
			
		}
	}
}
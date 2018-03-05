using ETModel;
using UnityEngine;

namespace ETHotfix
{
	[MessageHandler]
	public class Frame_ClickMapHandler : AMHandler<Frame_ClickMap>
	{
		protected override void Run(Session session, Frame_ClickMap message)
		{
			Unit unit = ETModel.Game.Scene.GetComponent<UnitComponent>().Get(message.Id);
			MoveComponent moveComponent = unit.GetComponent<MoveComponent>();
			Vector3 dest = new Vector3(message.X / 1000f, 0, message.Z / 1000f);
			moveComponent.MoveToDest(dest, 1);
			moveComponent.Turn2D(dest - unit.Position);
		}
	}
}

using UnityEngine;

namespace Model
{
	[MessageHandler(Opcode.Actor_CreateUnits)]
	public class Actor_CreateUnitsHandler : AMHandler<Actor_CreateUnits>
	{
		protected override void Run(Actor_CreateUnits message)
		{
			UnitComponent unitComponent = Game.Scene.GetComponent<UnitComponent>();
			
			foreach (UnitInfo unitInfo in message.Units)
			{
				if (unitComponent.Get(unitInfo.UnitId) != null)
				{
					continue;
				}
				Unit unit = UnitFactory.Create(unitInfo.UnitId);
				unit.Position = new Vector3(unitInfo.X / 1000f, 0, unitInfo.Z / 1000f);
				unit.IntPos = new VInt3(unitInfo.X, 0, unitInfo.Z);

				if (PlayerComponent.Instance.MyPlayer.UnitId == unit.Id)
				{
					Game.Scene.GetComponent<CameraComponent>().Unit = unit;
				}
			}

			Game.Scene.AddComponent<OperaComponent>();
		}
	}
}

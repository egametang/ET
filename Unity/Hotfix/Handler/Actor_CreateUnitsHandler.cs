using Model;
using UnityEngine;

namespace Hotfix
{
	[MessageHandler]
	public class Actor_CreateUnitsHandler : AMHandler<Actor_CreateUnits>
	{
		protected override void Run(Session session, Actor_CreateUnits message)
		{
			// 加载Unit资源
			ResourcesComponent resourcesComponent = Game.Scene.GetComponent<ResourcesComponent>();
			resourcesComponent.LoadBundle($"Unit.unity3d");
			
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

			Hotfix.Scene.AddComponent<OperaComponent>();
		}
	}
}

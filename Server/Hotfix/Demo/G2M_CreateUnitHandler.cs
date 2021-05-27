using System;
using UnityEngine;

namespace ET
{
	[ActorMessageHandler]
	public class G2M_CreateUnitHandler : AMActorRpcHandler<Scene, G2M_CreateUnit, M2G_CreateUnit>
	{
		protected override async ETTask Run(Scene scene, G2M_CreateUnit request, M2G_CreateUnit response, Action reply)
		{
			Unit unit = EntityFactory.CreateWithId<Unit, int>(scene, IdGenerater.Instance.GenerateId(), 1001);
			unit.AddComponent<MoveComponent>();
			unit.Position = new Vector3(-10, 0, -10);
			
			NumericComponent numericComponent = unit.AddComponent<NumericComponent>();
			numericComponent.Set(NumericType.Speed, 6f); // 速度是6米每秒
			
			unit.AddComponent<MailBoxComponent>();
			await unit.AddLocation();
			unit.AddComponent<UnitGateComponent, long>(request.GateSessionId);
			scene.GetComponent<UnitComponent>().Add(unit);
			response.UnitId = unit.Id;
			
			// 把自己广播给周围的人
			M2C_CreateUnits createUnits = new M2C_CreateUnits();
			createUnits.Units.Add(UnitHelper.CreateUnitInfo(unit));
			MessageHelper.Broadcast(unit, createUnits);
			
			// 把周围的人通知给自己
			createUnits.Units.Clear();
			Unit[] units = scene.GetComponent<UnitComponent>().GetAll();
			foreach (Unit u in units)
			{
				createUnits.Units.Add(UnitHelper.CreateUnitInfo(u));
			}
			MessageHelper.SendActor(unit.GetComponent<UnitGateComponent>().GateSessionActorId, createUnits);

			reply();
		}
	}
}
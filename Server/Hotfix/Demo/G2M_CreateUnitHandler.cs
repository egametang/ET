using System;
using UnityEngine;

namespace ET
{
	[ActorMessageHandler]
	public class G2M_CreateUnitHandler : AMActorRpcHandler<Scene, G2M_CreateUnit, M2G_CreateUnit>
	{
		protected override async ETTask Run(Scene scene, G2M_CreateUnit request, M2G_CreateUnit response, Action reply)
		{
			UnitComponent unitComponent = scene.GetComponent<UnitComponent>();
			Unit unit = unitComponent.AddChildWithId<Unit, int>(IdGenerater.Instance.GenerateId(), 1001);
			unit.AddComponent<MoveComponent>();
			unit.AddComponent<PathfindingComponent, string>("solo");
			unit.Position = new Vector3(-10, 0, -10);
			
			NumericComponent numericComponent = unit.AddComponent<NumericComponent>();
			numericComponent.Set(NumericType.Speed, 6f); // 速度是6米每秒
			numericComponent.Set(NumericType.AOI, 15000); // 视野15米
			
			unit.AddComponent<MailBoxComponent>();
			await unit.AddLocation();
			unit.AddComponent<UnitGateComponent, long>(request.GateSessionId);
			unitComponent.Add(unit);
			// 加入aoi
			unit.AddComponent<AOIEntity, int, Vector3>(9 * 1000, unit.Position);

			M2C_CreateUnits m2CCreateUnits = new M2C_CreateUnits();
			m2CCreateUnits.Units.Add(UnitHelper.CreateUnitInfo(unit));
			MessageHelper.SendToClient(unit, m2CCreateUnits);
			
			response.MyId = unit.Id;
			
			reply();
		}
	}
}
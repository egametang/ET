using System;
using UnityEngine;

namespace ET.Server
{
	[ActorMessageHandler(SceneType.Map)]
	public class M2M_UnitTransferRequestHandler : AMActorRpcHandler<Scene, M2M_UnitTransferRequest, M2M_UnitTransferResponse>
	{
		protected override async ETTask Run(Scene scene, M2M_UnitTransferRequest request, M2M_UnitTransferResponse response, Action reply)
		{
			await ETTask.CompletedTask;
			UnitComponent unitComponent = scene.GetComponent<UnitComponent>();
			Unit unit = MongoHelper.FromBson<Unit>(request.Unit);
			
			unitComponent.AddChild(unit);
			unitComponent.Add(unit);

			foreach (byte[] bytes in request.Entitys)
			{
				Entity entity = MongoHelper.FromBson<Entity>(bytes);
				unit.AddComponent(entity);
			}
			
			unit.AddComponent<MoveComponent>();
			unit.AddComponent<PathfindingComponent, string>(scene.Name);
			unit.Position = new Vector3(-10, 0, -10);
			
			unit.AddComponent<MailBoxComponent>();
			
			// 通知客户端创建My Unit
			M2C_CreateMyUnit m2CCreateUnits = new M2C_CreateMyUnit();
			m2CCreateUnits.Unit = Server.UnitHelper.CreateUnitInfo(unit);
			MessageHelper.SendToClient(unit, m2CCreateUnits);
			
			// 加入aoi
			unit.AddComponent<AOIEntity, int, Vector3>(9 * 1000, unit.Position);

			response.NewInstanceId = unit.InstanceId;
			
			reply();
		}
	}
}
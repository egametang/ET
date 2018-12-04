using System;
using ETModel;
using PF;

namespace ETHotfix
{
	[MessageHandler(AppType.Map)]
	public class G2M_CreateUnitHandler : AMRpcHandler<G2M_CreateUnit, M2G_CreateUnit>
	{
		protected override void Run(Session session, G2M_CreateUnit message, Action<M2G_CreateUnit> reply)
		{
			RunAsync(session, message, reply).NoAwait();
		}
		
		protected async ETVoid RunAsync(Session session, G2M_CreateUnit message, Action<M2G_CreateUnit> reply)
		{
			M2G_CreateUnit response = new M2G_CreateUnit();
			try
			{
				Unit unit = ComponentFactory.CreateWithId<Unit>(IdGenerater.GenerateId());
				unit.AddComponent<MoveComponent>();
				unit.AddComponent<UnitPathComponent>();
				unit.Position = new Vector3(-10, 0, -10);
				
				await unit.AddComponent<MailBoxComponent>().AddLocation();
				unit.AddComponent<UnitGateComponent, long>(message.GateSessionId);
				Game.Scene.GetComponent<UnitComponent>().Add(unit);
				response.UnitId = unit.Id;
				
				
				// 广播创建的unit
				M2C_CreateUnits createUnits = new M2C_CreateUnits();
				Unit[] units = Game.Scene.GetComponent<UnitComponent>().GetAll();
				foreach (Unit u in units)
				{
					UnitInfo unitInfo = new UnitInfo();
					unitInfo.X = u.Position.x;
					unitInfo.Y = u.Position.y;
					unitInfo.Z = u.Position.z;
					unitInfo.UnitId = u.Id;
					createUnits.Units.Add(unitInfo);
				}
				MessageHelper.Broadcast(createUnits);
				
				
				reply(response);
			}
			catch (Exception e)
			{
				ReplyError(response, e, reply);
			}
		}
	}
}
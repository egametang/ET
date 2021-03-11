
using Vector3 = UnityEngine.Vector3;

namespace ET
{
	[MessageHandler]
	public class M2C_CreateUnitsHandler : AMHandler<M2C_CreateUnits>
	{
		protected override async ETVoid Run(Session session, M2C_CreateUnits message)
		{	
			UnitComponent unitComponent = session.Domain.GetComponent<UnitComponent>();
			
			foreach (UnitInfo unitInfo in message.Units)
			{
				if (unitComponent.Get(unitInfo.UnitId) != null)
				{
					continue;
				}
				Unit unit = UnitFactory.Create(session.Domain, unitInfo.UnitId);
				unit.Position = new Vector3(unitInfo.X, unitInfo.Y, unitInfo.Z);
			}

			await ETTask.CompletedTask;
		}
	}
}

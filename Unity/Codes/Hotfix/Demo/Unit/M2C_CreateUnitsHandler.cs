namespace ET
{
	[MessageHandler]
	public class M2C_CreateUnitsHandler : AMHandler<M2C_CreateUnits>
	{
		protected override async ETTask Run(Session session, M2C_CreateUnits message)
		{	
			UnitComponent unitComponent = session.Domain.GetComponent<UnitComponent>();
			
			foreach (UnitInfo unitInfo in message.Units)
			{
				if (unitComponent.Get(unitInfo.UnitId) != null)
				{
					continue;
				}
				Unit unit = UnitFactory.Create(session.Domain, unitInfo);
			}

			await ETTask.CompletedTask;
		}
	}
}

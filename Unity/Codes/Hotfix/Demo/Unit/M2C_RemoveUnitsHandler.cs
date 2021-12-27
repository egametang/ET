namespace ET
{
	[MessageHandler]
	public class M2C_RemoveUnitsHandler : AMHandler<M2C_RemoveUnits>
	{
		protected override async ETTask Run(Session session, M2C_RemoveUnits message)
		{	
			UnitComponent unitComponent = session.Domain.GetComponent<UnitComponent>();
			foreach (long unitId in message.Units)
			{
				unitComponent.Remove(unitId);
			}

			await ETTask.CompletedTask;
		}
	}
}

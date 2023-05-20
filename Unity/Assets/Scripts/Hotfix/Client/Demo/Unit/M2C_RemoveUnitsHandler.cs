namespace ET.Client
{
	public static partial class M2C_RemoveUnitsHandler
	{
		[MessageHandler(SceneType.Demo)]
		private static async ETTask Run(Session session, M2C_RemoveUnits message)
		{	
			UnitComponent unitComponent = session.DomainScene().CurrentScene()?.GetComponent<UnitComponent>();
			if (unitComponent == null)
			{
				return;
			}
			foreach (long unitId in message.Units)
			{
				unitComponent.Remove(unitId);
			}

			await ETTask.CompletedTask;
		}
	}
}

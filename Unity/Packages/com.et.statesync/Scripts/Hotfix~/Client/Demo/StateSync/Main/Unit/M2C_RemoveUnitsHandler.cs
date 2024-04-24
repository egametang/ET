namespace ET.Client
{
	[MessageHandler(SceneType.StateSync)]
	public class M2C_RemoveUnitsHandler: MessageHandler<Scene, M2C_RemoveUnits>
	{
		protected override async ETTask Run(Scene root, M2C_RemoveUnits message)
		{	
			UnitComponent unitComponent = root.CurrentScene()?.GetComponent<UnitComponent>();
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

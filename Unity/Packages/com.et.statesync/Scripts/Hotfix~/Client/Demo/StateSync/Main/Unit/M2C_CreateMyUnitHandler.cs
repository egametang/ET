namespace ET.Client
{
	[MessageHandler(SceneType.StateSync)]
	public class M2C_CreateMyUnitHandler: MessageHandler<Scene, M2C_CreateMyUnit>
	{
		protected override async ETTask Run(Scene root, M2C_CreateMyUnit message)
		{
			// 通知场景切换协程继续往下走
			root.GetComponent<ObjectWait>().Notify(new Wait_CreateMyUnit() {Message = message});
			await ETTask.CompletedTask;
		}
	}
}

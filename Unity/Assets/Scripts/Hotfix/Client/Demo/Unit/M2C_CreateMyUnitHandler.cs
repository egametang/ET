namespace ET.Client
{
	[MessageHandler(SceneType.Demo)]
	public class M2C_CreateMyUnitHandler : MessageHandler<M2C_CreateMyUnit>
	{
		protected override async ETTask Run(Session session, M2C_CreateMyUnit message)
		{
			// 通知场景切换协程继续往下走
			session.DomainScene().GetComponent<ObjectWait>().Notify(new Wait_CreateMyUnit() {Message = message});
			await ETTask.CompletedTask;
		}
	}
}

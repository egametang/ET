namespace ET.Server
{
	[FriendOf(typeof(LvExpComponent))]
	[MessageSessionHandler(SceneType.Gate)]
	public class C2G_AddExpHandler : MessageSessionHandler<C2G_AddExp, G2C_AddExp>
	{
		protected override async ETTask Run(Session session, C2G_AddExp request, G2C_AddExp response)
		{
			Player player = session.GetComponent<SessionPlayerComponent>().Player;
			LvExpComponent lvExp = player.GetComponent<LvExpComponent>();
			if (lvExp == null)
			{
				lvExp = player.AddComponent<LvExpComponent>();
			}

			lvExp.Add(request.AddExp);
			response.Lv = lvExp.Lv;
			response.Exp = lvExp.Exp;
			await ETTask.CompletedTask;
		}
	}
}
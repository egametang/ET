using Model;

namespace Hotfix
{
	[Event(EventIdType.InitSceneStart)]
	public class InitSceneStart_CreateLobbyUI
	{
		public void Run()
		{
			UI ui = Game.Scene.GetComponent<UIComponent>().Create(UIType.Lobby);
			//Login();
		}

		public static async void Login()
		{	//
			//try
			//{
			//	Game.Scene.AddComponent<MessageDispatherComponent, AppType>(AppType.Client);
			//	ClientConfig clientConfig = Game.Scene.AddComponent<ClientConfigComponent>().Config.GetComponent<ClientConfig>();
			//	NetOuterComponent networkComponent = Game.Scene.AddComponent<NetOuterComponent>();
			//	using (Session session = networkComponent.Create(clientConfig.Address))
			//	{
			//		R2C_Login s2CLogin = await session.Call<C2R_Login, R2C_Login>(new C2R_Login { Account = "abcdef", Password = "111111" });
			//		networkComponent.Remove(session.Id);
			//
			//		// 连接Gate
			//		using (Session gateSession = networkComponent.Create(s2CLogin.Address))
			//		{
			//			await gateSession.Call<C2G_LoginGate, G2C_LoginGate>(new C2G_LoginGate(s2CLogin.Key));
			//		}
			//
			//		Log.Info("连接Gate验证成功!");
			//	}
			//}
			//catch (Exception e)
			//{
			//	Log.Error(e.ToString());
			//}
		}
	}
}

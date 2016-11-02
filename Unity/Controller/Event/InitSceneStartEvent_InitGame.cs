using System;
using Base;
using Model;

namespace Controller
{
	/// <summary>
	/// 初始化游戏
	/// </summary>
	[Event(EventIdType.InitSceneStart)]
	public class InitSceneStartEvent_InitGame: IEvent
	{
		public async void Run()
		{
			Game.Scene.AddComponent<MessageDispatherComponent, AppType>(AppType.Client);
			ClientConfig clientConfig = Game.Scene.AddComponent<ClientConfigComponent>().Config;
			NetOuterComponent networkComponent = Game.Scene.AddComponent<NetOuterComponent>();
			using (Session session = networkComponent.Create(clientConfig.Address))
			{
				try
				{
					R2C_Login s2CLogin = await session.Call<C2R_Login, R2C_Login>(new C2R_Login { Account = "abcdef", Password = "111111" });
					networkComponent.Remove(session.Id);

					// 连接Gate
					using (Session gateSession = networkComponent.Create(s2CLogin.Address))
					{
						await gateSession.Call<C2G_LoginGate, G2C_LoginGate>(new C2G_LoginGate(s2CLogin.Key));
					}
					
					Log.Info("连接Gate验证成功!");
				}
				catch (RpcException e)
				{
					Log.Error(e.ToString());
				}
				catch (Exception e)
				{
					Log.Error(e.ToString());
				}
			}
		}
	}
}

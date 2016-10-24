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
			Game.Scene.AddComponent<MessageDispatherComponent, string>("Client");
			NetworkComponent networkComponent = Game.Scene.AddComponent<NetOuterComponent>();
			Entity session = networkComponent.Get("192.168.12.188:10003");

			try
			{
				R2C_Login s2CLogin = await session.GetComponent<MessageComponent>().Call<C2R_Login, R2C_Login>(new C2R_Login {Account = "abcdef", Password = "111111"});
				networkComponent.Remove(session.Id);

				// 连接Gate
				Log.Debug(MongoHelper.ToJson(s2CLogin));
				Entity gateSession = networkComponent.Get(s2CLogin.Address);
				await gateSession.GetComponent<MessageComponent>().Call<C2G_LoginGate, G2C_LoginGate>(new C2G_LoginGate(s2CLogin.Key));
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

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
			NetworkComponent networkComponent = Game.Scene.AddComponent<NetworkComponent, NetworkProtocol>(NetworkProtocol.TCP);
			Entity session = networkComponent.Get("127.0.0.1:8888");

			try
			{
				// 订阅服务端日志, 服务端收到这个消息会将之后的日志转发给客户端
				await session.GetComponent<MessageComponent>().Call<C2S_SubscribeLog, S2C_SubscribeLog>(new C2S_SubscribeLog());
				S2C_Login s2CLogin = await session.GetComponent<MessageComponent>().Call<C2S_Login, S2C_Login>(new C2S_Login {Account = "tanghai", Password = "1111111"});
				Log.Info(MongoHelper.ToJson(s2CLogin));
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

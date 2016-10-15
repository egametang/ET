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
			Game.Scene.AddComponent<MessageHandlerComponent, string>("Client");
			NetworkComponent networkComponent = Game.Scene.AddComponent<NetworkComponent, NetworkProtocol>(NetworkProtocol.UDP);
			Entity session = networkComponent.Get("127.0.0.1:8888");

			try
			{
				S2C_Login s2CLogin = await session.GetComponent<MessageComponent>().CallAsync<S2C_Login>(new C2S_Login {Account = "tanghai", Password = "1111111"});
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

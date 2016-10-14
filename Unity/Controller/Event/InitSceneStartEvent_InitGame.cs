using System;
using System.Reflection;
using Base;
using Model;
using UnityEngine;
using Object = Base.Object;

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
			Game.Scene.AddComponent<MessageComponent>();
			Game.Scene.AddComponent<ChildrenComponent>();

			try
			{
				S2C_Login s2CLogin = await Game.Scene.GetComponent<MessageComponent>().CallAsync<S2C_Login>(new C2S_Login {Account = "tanghai", Password = "1111111"});
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

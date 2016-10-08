using System;
using System.Reflection;
using Base;
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
			GameObject code = (GameObject)Resources.Load("Code/Code");
			byte[] assBytes = code.Get<TextAsset>("Controller.dll").bytes;
			byte[] mdbBytes = code.Get<TextAsset>("Controller.dll.mdb").bytes;
			Assembly assembly = Assembly.Load(assBytes, mdbBytes);
			Object.ObjectManager.Register("Controller", assembly);
			Object.ObjectManager.Register("Base", typeof(Game).Assembly);

			Game.Scene.AddComponent<MessageComponent>();
			Game.Scene.AddComponent<ChildrenComponent>();

			try
			{
				S2C_FetchServerTime s2CFetchServerTime = await Game.Scene.GetComponent<MessageComponent>().CallAsync<S2C_FetchServerTime>(new C2S_FetchServerTime());
				Log.Info($"server time is: {s2CFetchServerTime.ServerTime}");
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

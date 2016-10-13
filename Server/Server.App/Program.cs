using System;
using Base;
using Object = Base.Object;

namespace App
{
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				Object.ObjectManager.Register("Model", typeof(Scene).Assembly);
				
				Server.Scene.AddComponent<EventComponent>();
				Server.Scene.AddComponent<TimerComponent>();
				Server.Scene.AddComponent<NetworkComponent, NetworkProtocol>(NetworkProtocol.UDP);

				Server.Scene.AddComponent<Scene, SceneType, string>(SceneType.Realm, "realm");
				Server.Scene.AddComponent<MessageHandlerComponent, SceneType>(Server.Scene.GetComponent<Scene>().SceneType);

				while (true)
				{
					Object.ObjectManager.Update();
				}
			}
			catch (Exception e)
			{
				Log.Error(e.ToString());
			}
		}
	}
}

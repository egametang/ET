using System;
using System.IO;
using System.Reflection;
using Base;
using Model;
using Object = Base.Object;

namespace App
{
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				Log.Info("server start........................");

				Object.ObjectManager.Register("Base", typeof(Game).Assembly);
				Object.ObjectManager.Register("Model", typeof(Opcode).Assembly);
				byte[] dllBytes = File.ReadAllBytes("./Controller.dll");
				byte[] pdbBytes = File.ReadAllBytes("./Controller.pdb");
				Assembly controller = Assembly.Load(dllBytes, pdbBytes);
				Object.ObjectManager.Register("Controller", controller);
				
				Game.Scene.AddComponent<EventComponent>();
				TimeComponent timeComponent = Game.Scene.AddComponent<TimeComponent>();
				Game.Scene.AddComponent<TimerComponent, TimeComponent>(timeComponent);
				Game.Scene.AddComponent<NetworkComponent, NetworkProtocol>(NetworkProtocol.UDP);
				Game.Scene.AddComponent<MessageHandlerComponent, MessageType>(MessageType.Realm);

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

using System;
using System.IO;
using Helper;

namespace World
{
	public class World
	{
		private static readonly World instance = new World();

		private IDispatcher dispatcher;

		private World()
		{
			this.Load();
		}

		private void Load()
		{
			string dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Handler.dll");
			if (!File.Exists(dllPath))
			{
				throw new Exception("not found handler dll!");
			}
			var assembly = LoaderHelper.Load(dllPath);
			this.dispatcher = (IDispatcher)assembly.CreateInstance("Handler.Dispatcher");
		}

		public static World Instance
		{
			get
			{
				return instance;
			}
		}

		public void Reload()
		{
			this.Load();
		}

		public void Dispatcher(short opcode, byte[] content)
		{
			var messageEnv = new MessageEnv();
			messageEnv["world"] = this;
			this.dispatcher.Dispatch(messageEnv, opcode, content);
		}
	}
}

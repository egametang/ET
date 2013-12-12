using System;
using System.IO;
using Component;
using Helper;
using Log;

namespace World
{
	public class World
	{
		private static readonly World instance = new World();

		private ILogicEntry iLogicEntry;

		private readonly Config config;

		private World()
		{
			this.config = Config.Instance;
			this.LoadLogic();
		}

		private void LoadLogic()
		{
			string dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logic.dll");
			if (!File.Exists(dllPath))
			{
				throw new Exception(string.Format("not found logic dll, path: {0}", dllPath));
			}
			var assembly = LoaderHelper.Load(dllPath);
			this.iLogicEntry = (ILogicEntry)assembly.CreateInstance("Logic.LogicEntry");
		}

		public static World Instance
		{
			get
			{
				return instance;
			}
		}

		public void ReloadLogic()
		{
			this.LoadLogic();
		}

		public void ReloadConfig()
		{
			this.config.Reload();
		}

		public void Dispatcher(short opcode, byte[] content)
		{
			try
			{
				var messageEnv = new MessageEnv();
				messageEnv.Set(this);
				this.iLogicEntry.Enter(messageEnv, opcode, content);
			}
			catch (Exception e)
			{
				Logger.Trace("message handle error: {0}", e.Message);
			}
		}

		public Config Config
		{
			get
			{
				return this.config;
			}
		}
	}
}

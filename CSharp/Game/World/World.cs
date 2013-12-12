using System;
using System.IO;
using Component;
using Helper;
using Log;
using Logic;

namespace World
{
	public class World
	{
		private static readonly World instance = new World();

		private readonly LogicEntry logicEntry = LogicEntry.Instance;

		private readonly Config config = Config.Instance;

		public static World Instance
		{
			get
			{
				return instance;
			}
		}

		public void ReloadLogic()
		{
			this.logicEntry.Reload();
		}

		public void ReloadConfig()
		{
			this.config.Reload();
		}

		public void Enter(short opcode, byte[] content)
		{
			try
			{
				var messageEnv = new MessageEnv();
				messageEnv.Set(this);
				this.logicEntry.Enter(messageEnv, opcode, content);
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

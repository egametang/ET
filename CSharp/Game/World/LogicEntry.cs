using System;
using System.Collections.Generic;
using System.IO;
using Component;
using Helper;
using Logic;

namespace World
{
	public class LogicEntry : ILogicEntry
    {
		private static readonly LogicEntry instance = new LogicEntry();

	    private Dictionary<short, IHandler> handlers;

		public static LogicEntry Instance
		{
			get
			{
				return instance;
			}
		}

		public LogicEntry()
		{
			this.Load();
		}

		public void Load()
		{
			this.handlers = new Dictionary<short, IHandler>();
			string dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logic.dll");
			if (!File.Exists(dllPath))
			{
				throw new Exception(string.Format("not found logic dll, path: {0}", dllPath));
			}
			var assembly = LoaderHelper.Load(dllPath);
			Type[] types = assembly.GetTypes();

			foreach (var type in types)
			{
				object[] attrs = type.GetCustomAttributes(typeof(HandlerAttribute), false);
				if (attrs.Length == 0)
				{
					continue;
				}
				var handler = (IHandler)Activator.CreateInstance(type);
				short opcode = ((HandlerAttribute)attrs[0]).Opcode;
				if (this.handlers.ContainsKey(opcode))
				{
					throw new Exception(string.Format(
						"same opcode, opcode: {0}, name: {1}", opcode, type.Name));
				}
				this.handlers[opcode] = handler;
			}
		}

		public void Reload()
		{
			this.Load();
		}

	    public void Enter(MessageEnv messageEnv, short opcode, byte[] content)
	    {
		    IHandler handler = null;
			if (!handlers.TryGetValue(opcode, out handler))
			{
				throw new Exception(string.Format("not found handler opcode {0}", opcode));
			}

			handler.Handle(messageEnv, content);
	    }
	}
}

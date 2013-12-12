using System;
using System.Collections.Generic;
using Component;

namespace Logic
{
	public class LogicEntry : ILogicEntry
    {
	    private readonly Dictionary<short, IHandler> handlers = new Dictionary<short, IHandler>();

		public LogicEntry()
		{
			Type[] types = typeof (LogicEntry).Assembly.GetTypes();
			foreach (var type in types)
			{
				object[] attrs = type.GetCustomAttributes(typeof(HandlerAttribute), false);
				if (attrs.Length == 0)
				{
					continue;
				}
				var handler = (IHandler)Activator.CreateInstance(type);
				short opcode = ((HandlerAttribute)attrs[0]).Opcode;
				if (handlers.ContainsKey(opcode))
				{
					throw new Exception(string.Format(
						"same opcode, opcode: {0}, name: {1}", opcode, type.Name));
				}
				handlers[opcode] = handler;
			}
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

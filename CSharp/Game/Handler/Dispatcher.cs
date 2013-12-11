using System;
using System.Collections.Generic;
using World;

namespace Handler
{
	public class Dispatcher : IDispatcher
    {
	    private readonly Dictionary<short, IHandle> handlers = new Dictionary<short, IHandle>();

		public Dispatcher()
		{
			Type[] types = typeof (Dispatcher).Assembly.GetTypes();
			foreach (var type in types)
			{
				object[] attrs = type.GetCustomAttributes(typeof(HandlerAttribute), false);
				if (attrs.Length == 0)
				{
					continue;
				}
				var handler = (IHandle)Activator.CreateInstance(type);
				short opcode = ((HandlerAttribute)attrs[0]).Opcode;
				if (handlers.ContainsKey(opcode))
				{
					throw new Exception(string.Format("same opcode {0}", opcode));
				}
				handlers[opcode] = handler;
			}
		}

	    public void Dispatch(MessageEnv messageEnv, short opcode, byte[] content)
	    {
		    IHandle handler = null;
			if (!handlers.TryGetValue(opcode, out handler))
			{
				throw new Exception(string.Format("not found handler opcode {0}", opcode));
			}

			handler.Handle(messageEnv, content);
	    }
	}
}

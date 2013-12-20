using System;
using System.Collections.Generic;
using System.IO;
using Helper;
using Log;

namespace Component
{
	public class LogicManager : ILogic
    {
		private static readonly LogicManager instance = new LogicManager();

		private Dictionary<int, IHandler> handlers;

		private Dictionary<EventType, SortedDictionary<int, IEvent>> events;

		public static LogicManager Instance
		{
			get
			{
				return instance;
			}
		}

		private LogicManager()
		{
			this.Load();
		}

		private void Load()
		{
			string dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logic.dll");
			var assembly = LoaderHelper.Load(dllPath);
			Type[] types = assembly.GetTypes();

			// 加载封包处理器
			this.handlers = new Dictionary<int, IHandler>();
			foreach (var type in types)
			{
				object[] attrs = type.GetCustomAttributes(typeof(HandlerAttribute), false);
				if (attrs.Length == 0)
				{
					continue;
				}
				var handler = (IHandler)Activator.CreateInstance(type);
				int opcode = ((HandlerAttribute)attrs[0]).Opcode;
				if (this.handlers.ContainsKey(opcode))
				{
					throw new Exception(string.Format(
						"same handler opcode, opcode: {0}, name: {1}", opcode, type.Name));
				}
				this.handlers[opcode] = handler;
			}

			// 加载事件处理器
			this.events = new Dictionary<EventType, SortedDictionary<int, IEvent>>();
			foreach (var type in types)
			{
				object[] attrs = type.GetCustomAttributes(typeof(EventAttribute), false);
				if (attrs.Length == 0)
				{
					continue;
				}
				var evt = (IEvent)Activator.CreateInstance(type);
				var eventType = ((EventAttribute)attrs[0]).Type;
				var eventNumber = ((EventAttribute)attrs[0]).Number;
				if (!this.events.ContainsKey(eventType))
				{
					this.events[eventType] = new SortedDictionary<int, IEvent>();
				}
				if (this.events[eventType].ContainsKey(eventNumber))
				{
					throw new Exception(string.Format(
						"same event number, type: {0}, number: {1}, name: {2}", 
						eventType, eventNumber, type.Name));
				}
				this.events[eventType][eventNumber] = evt;
			}
		}

		public void Reload()
		{
			this.Load();
		}

		public void Handle(short opcode, byte[] content)
	    {
		    IHandler handler = null;
			if (!handlers.TryGetValue(opcode, out handler))
			{
				throw new Exception(string.Format("not found handler opcode {0}", opcode));
			}

			try
			{
				var messageEnv = new MessageEnv();
				handler.Handle(messageEnv, content);
			}
			catch (Exception e)
			{
				Logger.Trace("message handle error: {0}", e.Message);
			}
	    }

		public void Trigger(MessageEnv messageEnv, EventType type)
		{
			SortedDictionary<int, IEvent> iEventDict = null;
			if (!this.events.TryGetValue(type, out iEventDict))
			{
				return;
			}

			foreach (var iEvent in iEventDict)
			{
				iEvent.Value.Trigger(messageEnv);
			}
		}
	}
}

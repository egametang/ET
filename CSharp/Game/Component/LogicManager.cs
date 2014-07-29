using System;
using System.Collections.Generic;
using System.IO;
using Helper;
using Logger;

namespace Component
{
    public class LogicManager: ILogic
    {
        private static readonly LogicManager instance = new LogicManager();

        private Dictionary<int, Tuple<IHandler, Type>> handlers;

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
            var localHandlers = new Dictionary<int, Tuple<IHandler, Type>>();
            foreach (var type in types)
            {
                object[] attrs = type.GetCustomAttributes(typeof (HandlerAttribute), false);
                if (attrs.Length == 0)
                {
                    continue;
                }
                var handler = (IHandler) Activator.CreateInstance(type);
                int opcode = ((HandlerAttribute) attrs[0]).Opcode;
                Type messageType = ((HandlerAttribute) attrs[0]).Type;
                if (opcode == 0 || messageType == null)
                {
                    throw new Exception(string.Format("not set opcode or type, handler name: {0}",
                            type.Name));
                }
                if (localHandlers.ContainsKey(opcode))
                {
                    throw new Exception(string.Format(
                                                      "same handler opcode, opcode: {0}, name: {1}",
                            opcode, type.Name));
                }
                localHandlers[opcode] = new Tuple<IHandler, Type>(handler, messageType);
            }

            // 加载事件处理器
            var localEvents = new Dictionary<EventType, SortedDictionary<int, IEvent>>();
            foreach (var type in types)
            {
                object[] attrs = type.GetCustomAttributes(typeof (EventAttribute), false);
                if (attrs.Length == 0)
                {
                    continue;
                }
                var evt = (IEvent) Activator.CreateInstance(type);
                EventType eventType = ((EventAttribute) attrs[0]).Type;
                int eventOrder = ((EventAttribute) attrs[0]).Order;

                if (eventOrder == 0 || eventType == EventType.DefaultEvent)
                {
                    throw new Exception(string.Format("not set order or type, event name: {0}",
                            type.Name));
                }

                if (!localEvents.ContainsKey(eventType))
                {
                    localEvents[eventType] = new SortedDictionary<int, IEvent>();
                }
                if (localEvents[eventType].ContainsKey(eventOrder))
                {
                    throw new Exception(
                            string.Format("same event number, type: {0}, number: {1}, name: {2}",
                                    eventType, eventOrder, type.Name));
                }
                localEvents[eventType][eventOrder] = evt;
            }

            // 
            this.handlers = localHandlers;
            this.events = localEvents;
        }

        public void Reload()
        {
            this.Load();
        }

        public void Handle(short opcode, byte[] content)
        {
            Tuple<IHandler, Type> tuple = null;
            if (!this.handlers.TryGetValue(opcode, out tuple))
            {
                throw new Exception(string.Format("not found handler opcode {0}", opcode));
            }

            try
            {
                object message = MongoHelper.FromBson(content, tuple.Item2);
                var messageEnv = new MessageEnv();
                messageEnv[KeyDefine.KMessage] = message;
                tuple.Item1.Handle(messageEnv);
            }
            catch (Exception e)
            {
                Log.Trace("message handle error: {0}", e.Message);
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
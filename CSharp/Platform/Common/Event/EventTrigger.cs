using System;
using System.Collections.Generic;

namespace Common.Event
{
    public class EventTrigger<T> where T : AEventAttribute
    {
        private readonly Dictionary<int, List<IEvent>> events;

        public EventTrigger()
        {
            this.events = new Dictionary<int, List<IEvent>>();

            Type type = typeof (T);
            var types = type.Assembly.GetTypes();
            foreach (var t in types)
            {
                object[] attrs = t.GetCustomAttributes(type, false);
                if (attrs.Length == 0)
                {
                    continue;
                }
                object obj = Activator.CreateInstance(t);
                IEvent iEvent = obj as IEvent;
                if (iEvent == null)
                {
                    throw new Exception(string.Format("event not inherit IEvent interface: {0}",
                            obj.GetType().FullName));
                }

                AEventAttribute iEventAttribute = (T) attrs[0];

                if (!this.events.ContainsKey(iEventAttribute.Type))
                {
                    this.events.Add(iEventAttribute.Type, new List<IEvent>());
                }
                this.events[iEventAttribute.Type].Add(iEvent);
            }
        }

        public void Trigger(int type, Env env)
        {
            List<IEvent> iEventDict = null;
            if (!this.events.TryGetValue(type, out iEventDict))
            {
                return;
            }

            foreach (var iEvent in iEventDict)
            {
                iEvent.Trigger(env);
            }
        }
    }
}
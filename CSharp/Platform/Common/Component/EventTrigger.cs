using System;
using System.Collections.Generic;

namespace Common.Component
{
    public class EventTrigger<T> where T : IEventAttribute
    {
        private readonly Dictionary<int, SortedDictionary<int, IEvent>> events;

        public EventTrigger()
        {
            this.events = new Dictionary<int, SortedDictionary<int, IEvent>>();

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
                    throw new Exception(string.Format("event not inherit IEvent interface: {0}", obj.GetType().FullName));
                }

                IEventAttribute iEventAttribute = (T) attrs[0];

                if (!this.events.ContainsKey(iEventAttribute.Type))
                {
                    this.events.Add(iEventAttribute.Type, new SortedDictionary<int, IEvent>());
                }
                this.events[iEventAttribute.Type].Add(iEventAttribute.Order, iEvent);
            }
        }

        public void Trigger(int type, Env env)
        {
            SortedDictionary<int, IEvent> iEventDict = null;
            if (!this.events.TryGetValue(type, out iEventDict))
            {
                return;
            }

            foreach (var iEvent in iEventDict)
            {
                iEvent.Value.Trigger(env);
            }
        }
    }
}

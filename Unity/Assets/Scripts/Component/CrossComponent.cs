using System;
using System.Collections.Generic;

namespace Model
{
    [ObjectEvent]
    public class CrossComponentEvent : ObjectEvent<CrossComponent>, IAwake, ILoad
    {
        public void Awake()
        {
            this.Get().Awake();
        }

        public void Load()
        {
            this.Get().Load();
        }
    }

    /// <summary>
    /// 事件分发,可以将事件分发到IL层
    /// </summary>
    public class CrossComponent : Component
    {
        private Dictionary<int, List<IInstanceMethod>> allEvents;

        public void Awake()
        {
            this.Load();
        }

        public void Load()
        {
            this.allEvents = new Dictionary<int, List<IInstanceMethod>>();

            Type[] types = DllHelper.GetHotfixTypes();
            foreach (Type type in types)
            {
                object[] attrs = type.GetCustomAttributes(typeof(CrossEventAttribute), false);

                foreach (object attr in attrs)
                {
                    CrossEventAttribute aEventAttribute = (CrossEventAttribute)attr;
#if ILRuntime
          IInstanceMethod method = new ILInstanceMethod(type, "Run"); 
          if (!this.allEvents.ContainsKey(aEventAttribute.Type)) 
#else
                    IInstanceMethod method = new MonoInstanceMethod(type, "Run");
#endif
                    if (!this.allEvents.ContainsKey(aEventAttribute.Type))
                    {
                        this.allEvents.Add(aEventAttribute.Type, new List<IInstanceMethod>());
                    }
                    this.allEvents[aEventAttribute.Type].Add(method);
                }
            }
        }

        public void Run(int type)
        {
            List<IInstanceMethod> iEvents = null;
            if (!this.allEvents.TryGetValue(type, out iEvents))
            {
                return;
            }

            foreach (IInstanceMethod obj in iEvents)
            {
                try
                {
                    obj.Run();
                }
                catch (Exception err)
                {
                    Log.Error(err.ToString());
                }
            }
        }

        public void Run<A>(CrossIdType type, A a)
        {
            List<IInstanceMethod> iEvents = null;
            if (!this.allEvents.TryGetValue((int)type, out iEvents))
            {
                return;
            }

            foreach (IInstanceMethod obj in iEvents)
            {
                try
                {
                    obj.Run(a);
                }
                catch (Exception err)
                {
                    Log.Error(err.ToString());
                }
            }
        }

        public void Run<A, B>(CrossIdType type, A a, B b)
        {
            List<IInstanceMethod> iEvents = null;
            if (!this.allEvents.TryGetValue((int)type, out iEvents))
            {
                return;
            }

            foreach (IInstanceMethod obj in iEvents)
            {
                try
                {
                    obj.Run(a, b);
                }
                catch (Exception err)
                {
                    Log.Error(err.ToString());
                }
            }
        }

        public void Run<A, B, C>(CrossIdType type, A a, B b, C c)
        {
            List<IInstanceMethod> iEvents = null;
            if (!this.allEvents.TryGetValue((int)type, out iEvents))
            {
                return;
            }

            foreach (IInstanceMethod obj in iEvents)
            {
                try
                {
                    obj.Run(a, b, c);
                }
                catch (Exception err)
                {
                    Log.Error(err.ToString());
                }
            }
        }
    }
}
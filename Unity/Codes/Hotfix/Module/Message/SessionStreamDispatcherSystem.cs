using System;
using System.Collections.Generic;
using System.IO;

namespace ET
{
    [ObjectSystem]
    public class SessionStreamDispatcherAwakeSystem: AwakeSystem<SessionStreamDispatcher>
    {
        public override void Awake(SessionStreamDispatcher self)
        {
            SessionStreamDispatcher.Instance = self;
            self.Load();
        }
    }

    [ObjectSystem]
    public class SessionStreamDispatcherLoadSystem: LoadSystem<SessionStreamDispatcher>
    {
        public override void Load(SessionStreamDispatcher self)
        {
            self.Load();
        }
    }

    [ObjectSystem]
    public class SessionStreamDispatcherDestroySystem: DestroySystem<SessionStreamDispatcher>
    {
        public override void Destroy(SessionStreamDispatcher self)
        {
            SessionStreamDispatcher.Instance = null;
        }
    }
    
    [FriendClass(typeof(SessionStreamDispatcher))]
    public static class SessionStreamDispatcherSystem
    {
        public static void Load(this SessionStreamDispatcher self)
        {
            self.Dispatchers = new ISessionStreamDispatcher[100];
            
            List<Type> types = Game.EventSystem.GetTypes(typeof (SessionStreamDispatcherAttribute));

            foreach (Type type in types)
            {
                object[] attrs = type.GetCustomAttributes(typeof (SessionStreamDispatcherAttribute), false);
                if (attrs.Length == 0)
                {
                    continue;
                }
                
                SessionStreamDispatcherAttribute sessionStreamDispatcherAttribute = attrs[0] as SessionStreamDispatcherAttribute;
                if (sessionStreamDispatcherAttribute == null)
                {
                    continue;
                }

                if (sessionStreamDispatcherAttribute.Type >= 100)
                {
                    Log.Error("session dispatcher type must < 100");
                    continue;
                }
                
                ISessionStreamDispatcher iSessionStreamDispatcher = Activator.CreateInstance(type) as ISessionStreamDispatcher;
                if (iSessionStreamDispatcher == null)
                {
                    Log.Error($"sessionDispatcher {type.Name} 需要继承 ISessionDispatcher");
                    continue;
                }

                self.Dispatchers[sessionStreamDispatcherAttribute.Type] = iSessionStreamDispatcher;
            }
        }

        public static void Dispatch(this SessionStreamDispatcher self, int type, Session session, MemoryStream memoryStream)
        {
            ISessionStreamDispatcher sessionStreamDispatcher = self.Dispatchers[type];
            if (sessionStreamDispatcher == null)
            {
                throw new Exception("maybe your NetInnerComponent or NetOuterComponent not set SessionStreamDispatcherType");
            }
            sessionStreamDispatcher.Dispatch(session, memoryStream);
        }
    }
}
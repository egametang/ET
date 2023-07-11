using System.Collections.Generic;
using System;

namespace ET
{
    public class MessageDispatcherInfo
    {
        public SceneType SceneType { get; }
        public IMHandler IMHandler { get; }

        public MessageDispatcherInfo(SceneType sceneType, IMHandler imHandler)
        {
            this.SceneType = sceneType;
            this.IMHandler = imHandler;
        }
    }
    
    public class MessageDispatcherComponent: SingletonLock<MessageDispatcherComponent>, ISingletonAwake
    {
        private readonly Dictionary<ushort, List<MessageDispatcherInfo>> handlers = new();
        
        public void Awake()
        {
            HashSet<Type> types = EventSystem.Instance.GetTypes(typeof (MessageHandlerAttribute));

            foreach (Type type in types)
            {
                IMHandler iMHandler = Activator.CreateInstance(type) as IMHandler;
                if (iMHandler == null)
                {
                    Log.Error($"message handle {type.Name} 需要继承 IMHandler");
                    continue;
                }

                object[] attrs = type.GetCustomAttributes(typeof(MessageHandlerAttribute), true);
                
                foreach (object attr in attrs)
                {
                    MessageHandlerAttribute messageHandlerAttribute = attr as MessageHandlerAttribute;
                    
                    Type messageType = iMHandler.GetMessageType();
                    
                    ushort opcode = OpcodeType.Instance.GetOpcode(messageType);
                    if (opcode == 0)
                    {
                        Log.Error($"消息opcode为0: {messageType.Name}");
                        continue;
                    }

                    MessageDispatcherInfo messageDispatcherInfo = new (messageHandlerAttribute.SceneType, iMHandler);
                    this.RegisterHandler(opcode, messageDispatcherInfo);
                }
            }
        }
        
        public override void Load()
        {
            World.Instance.AddSingleton<MessageDispatcherComponent>(true);
        }
        
        private void RegisterHandler(ushort opcode, MessageDispatcherInfo handler)
        {
            if (!this.handlers.ContainsKey(opcode))
            {
                this.handlers.Add(opcode, new List<MessageDispatcherInfo>());
            }

            this.handlers[opcode].Add(handler);
        }

        public void Handle(Session session, object message)
        {
            List<MessageDispatcherInfo> actions;
            ushort opcode = OpcodeType.Instance.GetOpcode(message.GetType());
            if (!this.handlers.TryGetValue(opcode, out actions))
            {
                Log.Error($"消息没有处理: {opcode} {message}");
                return;
            }

            SceneType sceneType = session.IScene.SceneType;
            foreach (MessageDispatcherInfo ev in actions)
            {
                if (!ev.SceneType.HasSameFlag(sceneType))
                {
                    continue;
                }
                
                try
                {
                    ev.IMHandler.Handle(session, message);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }
    }
}
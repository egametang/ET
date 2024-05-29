using System.Collections.Generic;
using System;

namespace ET
{
    public struct MessageSessionDispatcherInfo
    {
        public int SceneType { get; }
        public IMessageSessionHandler IMHandler { get; }

        public MessageSessionDispatcherInfo(int sceneType, IMessageSessionHandler imHandler)
        {
            this.SceneType = sceneType;
            this.IMHandler = imHandler;
        }
    }
    
    [CodeProcess]
    public class MessageSessionDispatcher: Singleton<MessageSessionDispatcher>, ISingletonAwake
    {
        private readonly Dictionary<ushort, List<MessageSessionDispatcherInfo>> handlers = new();
        
        public void Awake()
        {
            HashSet<Type> types = CodeTypes.Instance.GetTypes(typeof (MessageSessionHandlerAttribute));

            foreach (Type type in types)
            {
                IMessageSessionHandler iMessageSessionHandler = Activator.CreateInstance(type) as IMessageSessionHandler;
                if (iMessageSessionHandler == null)
                {
                    Log.Error($"message handle {type.Name} 需要继承 IMHandler");
                    continue;
                }

                object[] attrs = type.GetCustomAttributes(typeof(MessageSessionHandlerAttribute), true);
                
                foreach (object attr in attrs)
                {
                    MessageSessionHandlerAttribute messageSessionHandlerAttribute = attr as MessageSessionHandlerAttribute;
                    
                    Type messageType = iMessageSessionHandler.GetMessageType();
                    
                    ushort opcode = OpcodeType.Instance.GetOpcode(messageType);
                    if (opcode == 0)
                    {
                        Log.Error($"消息opcode为0: {messageType.Name}");
                        continue;
                    }

                    MessageSessionDispatcherInfo messageSessionDispatcherInfo = new (messageSessionHandlerAttribute.SceneType, iMessageSessionHandler);
                    this.RegisterHandler(opcode, messageSessionDispatcherInfo);
                }
            }
        }
        
        private void RegisterHandler(ushort opcode, MessageSessionDispatcherInfo handler)
        {
            if (!this.handlers.ContainsKey(opcode))
            {
                this.handlers.Add(opcode, new List<MessageSessionDispatcherInfo>());
            }

            this.handlers[opcode].Add(handler);
        }

        public void Handle(Session session, object message)
        {
            List<MessageSessionDispatcherInfo> actions;
            ushort opcode = OpcodeType.Instance.GetOpcode(message.GetType());
            if (!this.handlers.TryGetValue(opcode, out actions))
            {
                Log.Error($"消息没有处理: {opcode} {message}");
                return;
            }

            int sceneType = session.IScene.SceneType;
            foreach (MessageSessionDispatcherInfo ev in actions)
            {
                if (!SceneTypeSingleton.IsSame(ev.SceneType, sceneType))
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
using System;
using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 消息分发组件
    /// </summary>
    [FriendOf(typeof(MessageDispatcherComponent))]
    public static class MessageDispatcherComponentHelper
    {
        [ObjectSystem]
        public class MessageDispatcherComponentAwakeSystem: AwakeSystem<MessageDispatcherComponent>
        {
            protected override void Awake(MessageDispatcherComponent self)
            {
                MessageDispatcherComponent.Instance = self;
                self.Load();
            }
        }

        [ObjectSystem]
        public class MessageDispatcherComponentLoadSystem: LoadSystem<MessageDispatcherComponent>
        {
            protected override void Load(MessageDispatcherComponent self)
            {
                self.Load();
            }
        }

        [ObjectSystem]
        public class MessageDispatcherComponentDestroySystem: DestroySystem<MessageDispatcherComponent>
        {
            protected override void Destroy(MessageDispatcherComponent self)
            {
                MessageDispatcherComponent.Instance = null;
                self.Handlers.Clear();
            }
        }

        private static void Load(this MessageDispatcherComponent self)
        {
            self.Handlers.Clear();

            HashSet<Type> types = EventSystem.Instance.GetTypes(typeof (MessageHandlerAttribute));

            foreach (Type type in types)
            {
                IMHandler iMHandler = Activator.CreateInstance(type) as IMHandler;
                if (iMHandler == null)
                {
                    Log.Error($"message handle {type.Name} 需要继承 IMHandler");
                    continue;
                }

                object[] attrs = type.GetCustomAttributes(typeof(MessageHandlerAttribute), false);
                
                foreach (object attr in attrs)
                {
                    MessageHandlerAttribute messageHandlerAttribute = attr as MessageHandlerAttribute;
                    
                    Type messageType = iMHandler.GetMessageType();
                    
                    ushort opcode = NetServices.Instance.GetOpcode(messageType);
                    if (opcode == 0)
                    {
                        Log.Error($"消息opcode为0: {messageType.Name}");
                        continue;
                    }

                    MessageDispatcherInfo messageDispatcherInfo = new (messageHandlerAttribute.SceneType, iMHandler);
                    self.RegisterHandler(opcode, messageDispatcherInfo);
                }
            }
        }

        private static void RegisterHandler(this MessageDispatcherComponent self, ushort opcode, MessageDispatcherInfo handler)
        {
            if (!self.Handlers.ContainsKey(opcode))
            {
                self.Handlers.Add(opcode, new List<MessageDispatcherInfo>());
            }

            self.Handlers[opcode].Add(handler);
        }

        public static void Handle(this MessageDispatcherComponent self, Session session, object message)
        {
            List<MessageDispatcherInfo> actions;
            ushort opcode = NetServices.Instance.GetOpcode(message.GetType());
            if (!self.Handlers.TryGetValue(opcode, out actions))
            {
                Log.Error($"消息没有处理: {opcode} {message}");
                return;
            }

            SceneType sceneType = session.DomainScene().SceneType;
            foreach (MessageDispatcherInfo ev in actions)
            {
                if (ev.SceneType != sceneType)
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
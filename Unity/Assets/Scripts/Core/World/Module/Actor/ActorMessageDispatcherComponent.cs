using System;
using System.Collections.Generic;

namespace ET
{
    public class ActorMessageDispatcherInfo
    {
        public SceneType SceneType { get; }
        
        public IMActorHandler IMActorHandler { get; }

        public ActorMessageDispatcherInfo(SceneType sceneType, IMActorHandler imActorHandler)
        {
            this.SceneType = sceneType;
            this.IMActorHandler = imActorHandler;
        }
    }
    
    /// <summary>
    /// Actor消息分发组件
    /// </summary>
    public class ActorMessageDispatcherComponent: SingletonLock<ActorMessageDispatcherComponent>, ISingletonAwake
    {
        private readonly Dictionary<Type, List<ActorMessageDispatcherInfo>> ActorMessageHandlers = new();

        public void Awake()
        {
            HashSet<Type> types = EventSystem.Instance.GetTypes(typeof (ActorMessageHandlerAttribute));
            
            foreach (Type type in types)
            {
                this.Register(type);
            }
            
            HashSet<Type> types2 = EventSystem.Instance.GetTypes(typeof (ActorMessageLocationHandlerAttribute));
            
            foreach (Type type in types2)
            {
                this.Register(type);
            }
        }
        
        public override void Load()
        {
            World.Instance.AddSingleton<ActorMessageDispatcherComponent>(true);
        }
        
        private void Register(Type type)
        {
            object obj = Activator.CreateInstance(type);

            IMActorHandler imHandler = obj as IMActorHandler;
            if (imHandler == null)
            {
                throw new Exception($"message handler not inherit IMActorHandler abstract class: {obj.GetType().FullName}");
            }
                
            object[] attrs = type.GetCustomAttributes(typeof(ActorMessageHandlerAttribute), true);

            foreach (object attr in attrs)
            {
                ActorMessageHandlerAttribute actorMessageHandlerAttribute = attr as ActorMessageHandlerAttribute;

                Type messageType = imHandler.GetRequestType();

                Type handleResponseType = imHandler.GetResponseType();
                if (handleResponseType != null)
                {
                    Type responseType = OpcodeType.Instance.GetResponseType(messageType);
                    if (handleResponseType != responseType)
                    {
                        throw new Exception($"message handler response type error: {messageType.FullName}");
                    }
                }

                ActorMessageDispatcherInfo actorMessageDispatcherInfo = new(actorMessageHandlerAttribute.SceneType, imHandler);

                this.RegisterHandler(messageType, actorMessageDispatcherInfo);
            }
        }
        
        private void RegisterHandler(Type type, ActorMessageDispatcherInfo handler)
        {
            if (!this.ActorMessageHandlers.ContainsKey(type))
            {
                this.ActorMessageHandlers.Add(type, new List<ActorMessageDispatcherInfo>());
            }

            this.ActorMessageHandlers[type].Add(handler);
        }

        public async ETTask Handle(Entity entity, Address fromAddress, MessageObject message)
        {
            List<ActorMessageDispatcherInfo> list;
            if (!this.ActorMessageHandlers.TryGetValue(message.GetType(), out list))
            {
                throw new Exception($"not found message handler: {message} {entity.GetType().FullName}");
            }

            SceneType sceneType = entity.IScene.SceneType;
            foreach (ActorMessageDispatcherInfo actorMessageDispatcherInfo in list)
            {
                if (!actorMessageDispatcherInfo.SceneType.HasSameFlag(sceneType))
                {
                    continue;
                }
                await actorMessageDispatcherInfo.IMActorHandler.Handle(entity, fromAddress, message);   
            }
        }
    }
}
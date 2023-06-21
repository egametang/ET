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
    public class ActorMessageDispatcherComponent: Singleton<ActorMessageDispatcherComponent>, ISingletonAwake, ISingletonLoad
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
        
        
        public void Load()
        {
            World.Instance.AddSingleton<ActorMessageDispatcherComponent>();
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

        public async ETTask Handle(Entity entity, ActorId actorId, object message)
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
                await actorMessageDispatcherInfo.IMActorHandler.Handle(entity, actorId, message);   
            }
        }

/*
        /// <summary>
        /// 分发actor消息
        /// </summary>
        public async ETTask HandleIActorRequest(ActorId actorId, IActorRequest iActorRequest)
        {
            Entity entity = self.Get(actorId.InstanceId);
            if (entity == null)
            {
                IActorResponse response = ActorHelper.CreateResponse(iActorRequest, ErrorCore.ERR_NotFoundActor);
                ActorHandleHelper.Reply(actorId, response);
                return;
            }
            
            Log.Debug(iActorRequest.ToJson());

            MailBoxComponent mailBoxComponent = entity.GetComponent<MailBoxComponent>();
            if (mailBoxComponent == null)
            {
                Log.Warning($"actor not found mailbox: {entity.GetType().FullName} {actorId} {iActorRequest}");
                IActorResponse response = ActorHelper.CreateResponse(iActorRequest, ErrorCore.ERR_NotFoundActor);
                ActorHandleHelper.Reply(actorId, response);
                return;
            }
            
            switch (mailBoxComponent.MailboxType)
            {
                case MailboxType.OrderedMessage:
                {
                    using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Mailbox, actorId.InstanceId))
                    {
                        if (entity.InstanceId != actorId.InstanceId)
                        {
                            IActorResponse response = ActorHelper.CreateResponse(iActorRequest, ErrorCore.ERR_NotFoundActor);
                            ActorHandleHelper.Reply(actorId, response);
                            break;
                        }
                        await self.Handle(entity, actorId, iActorRequest);
                    }
                    break;
                }
                case MailboxType.UnOrderMessageDispatcher:
                {
                    await self.Handle(entity, actorId, iActorRequest);
                    break;
                }
                default:
                    throw new Exception($"no mailboxtype: {mailBoxComponent.MailboxType} {iActorRequest}");
            }
        }
        
        /// <summary>
        /// 分发actor消息
        /// </summary>
        [EnableAccessEntiyChild]
        public static async ETTask HandleIActorMessage(this ActorMessageDispatcherComponent self, ActorId actorId, IActorMessage iActorMessage)
        {
            Entity entity = self.Get(actorId.InstanceId);
            if (entity == null)
            {
                Log.Error($"not found actor: {actorId} {iActorMessage}");
                return;
            }
            
            MailBoxComponent mailBoxComponent = entity.GetComponent<MailBoxComponent>();
            if (mailBoxComponent == null)
            {
                Log.Error($"actor not found mailbox: {entity.GetType().FullName} {actorId} {iActorMessage}");
                return;
            }

            switch (mailBoxComponent.MailboxType)
            {
                case MailboxType.OrderedMessage:
                {
                    using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Mailbox, actorId.InstanceId))
                    {
                        if (entity.InstanceId != actorId.InstanceId)
                        {
                            break;
                        }
                        await self.Handle(entity, actorId, iActorMessage);
                    }
                    break;
                }
                case MailboxType.UnOrderMessageDispatcher:
                {
                    await self.Handle(entity, actorId, iActorMessage);
                    break;
                }
                case MailboxType.GateSession:
                {
                    if (entity is PlayerSessionComponent playerSessionComponent)
                    {
                        playerSessionComponent.Session?.Send(iActorMessage);
                    }
                    break;
                }
                default:
                    throw new Exception($"no mailboxtype: {mailBoxComponent.MailboxType} {iActorMessage}");
            }
        }
        */
    }
}
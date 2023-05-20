using System;
using System.Collections.Generic;
using System.Linq;

namespace ET.Server
{
    /// <summary>
    /// Actor消息分发组件
    /// </summary>
    [FriendOf(typeof(ActorMessageDispatcherComponent))]
    public static partial class ActorMessageDispatcherComponentHelper
    {
        [EntitySystem]
        private static void Awake(this ActorMessageDispatcherComponent self)
        {
            ActorMessageDispatcherComponent.Instance = self;
            self.Load();
        }
        
        [EntitySystem]
        private static void Destroy(this ActorMessageDispatcherComponent self)
        {
            self.ActorMessageHandlers.Clear();
            ActorMessageDispatcherComponent.Instance = null;
        }
        
        [EntitySystem]
        private static void Load(this ActorMessageDispatcherComponent self)
        {
            self.ActorMessageHandlers.Clear();

            HashSet<Type> types = EventSystem.Instance.GetTypes(typeof (ActorMessageHandlerAttribute));
            
            foreach (Type type in types)
            {
                self.Register(type);
            }
            
            HashSet<Type> types2 = EventSystem.Instance.GetTypes(typeof (ActorMessageLocationHandlerAttribute));
            
            foreach (Type type in types2)
            {
                self.Register(type);
            }
        }

        private static void Register(this ActorMessageDispatcherComponent self, Type type)
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
                    Type responseType = OpcodeTypeComponent.Instance.GetResponseType(messageType);
                    if (handleResponseType != responseType)
                    {
                        throw new Exception($"message handler response type error: {messageType.FullName}");
                    }
                }

                ActorMessageDispatcherInfo actorMessageDispatcherInfo = new(actorMessageHandlerAttribute.SceneType, imHandler);

                self.RegisterHandler(messageType, actorMessageDispatcherInfo);
            }
        }
        
        private static void RegisterHandler(this ActorMessageDispatcherComponent self, Type type, ActorMessageDispatcherInfo handler)
        {
            if (!self.ActorMessageHandlers.ContainsKey(type))
            {
                self.ActorMessageHandlers.Add(type, new List<ActorMessageDispatcherInfo>());
            }

            self.ActorMessageHandlers[type].Add(handler);
        }

        private static async ETTask Handle(this ActorMessageDispatcherComponent self, Entity entity, int fromProcess, object message)
        {
            List<ActorMessageDispatcherInfo> list;
            if (!self.ActorMessageHandlers.TryGetValue(message.GetType(), out list))
            {
                throw new Exception($"not found message handler: {message} {entity.GetType().FullName}");
            }

            SceneType sceneType = entity.Domain.SceneType;
            foreach (ActorMessageDispatcherInfo actorMessageDispatcherInfo in list)
            {
                if (!actorMessageDispatcherInfo.SceneType.HasSameFlag(sceneType))
                {
                    continue;
                }
                await actorMessageDispatcherInfo.IMActorHandler.Handle(entity, fromProcess, message);   
            }
        }


        /// <summary>
        /// 分发actor消息
        /// </summary>
        [EnableAccessEntiyChild]
        public static async ETTask HandleIActorRequest(this ActorMessageDispatcherComponent self, int fromProcess, long actorId, IActorRequest iActorRequest)
        {
            Entity entity = self.Get(actorId);
            if (entity == null)
            {
                IActorResponse response = ActorHelper.CreateResponse(iActorRequest, ErrorCore.ERR_NotFoundActor);
                ActorHandleHelper.Reply(fromProcess, response);
                return;
            }
            
            self.LogMsg(iActorRequest);

            MailBoxComponent mailBoxComponent = entity.GetComponent<MailBoxComponent>();
            if (mailBoxComponent == null)
            {
                Log.Warning($"actor not found mailbox: {entity.GetType().FullName} {actorId} {iActorRequest}");
                IActorResponse response = ActorHelper.CreateResponse(iActorRequest, ErrorCore.ERR_NotFoundActor);
                ActorHandleHelper.Reply(fromProcess, response);
                return;
            }
            
            switch (mailBoxComponent.MailboxType)
            {
                case MailboxType.OrderedMessage:
                {
                    using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Mailbox, actorId))
                    {
                        if (entity.InstanceId != actorId)
                        {
                            IActorResponse response = ActorHelper.CreateResponse(iActorRequest, ErrorCore.ERR_NotFoundActor);
                            ActorHandleHelper.Reply(fromProcess, response);
                            break;
                        }
                        await self.Handle(entity, fromProcess, iActorRequest);
                    }
                    break;
                }
                case MailboxType.UnOrderMessageDispatcher:
                {
                    await self.Handle(entity, fromProcess, iActorRequest);
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
        public static async ETTask HandleIActorMessage(this ActorMessageDispatcherComponent self, int fromProcess, long actorId, IActorMessage iActorMessage)
        {
            Entity entity = self.Get(actorId);
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
                    using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Mailbox, actorId))
                    {
                        if (entity.InstanceId != actorId)
                        {
                            break;
                        }
                        await self.Handle(entity, fromProcess, iActorMessage);
                    }
                    break;
                }
                case MailboxType.UnOrderMessageDispatcher:
                {
                    await self.Handle(entity, fromProcess, iActorMessage);
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
        
        
        public static void Add(this ActorMessageDispatcherComponent self, Entity entity)
        {
            self.mailboxEntities.Add(entity.InstanceId, entity);
        }
        
        public static void Remove(this ActorMessageDispatcherComponent self, long instanceId)
        {
            self.mailboxEntities.Remove(instanceId);
        }

        private static Entity Get(this ActorMessageDispatcherComponent self, long instanceId)
        {
            Entity component = null;
            self.mailboxEntities.TryGetValue(instanceId, out component);
            return component;
        }
    }
}
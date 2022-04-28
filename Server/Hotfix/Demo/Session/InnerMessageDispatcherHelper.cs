using System;
using System.IO;

namespace ET
{
    [FriendClass(typeof(MailBoxComponent))]
    public static class InnerMessageDispatcherHelper
    {
        public static void HandleIActorResponse(ushort opcode, long actorId, IActorResponse iActorResponse)
        {
            ActorMessageSenderComponent.Instance.RunMessage(actorId, iActorResponse);
        }

        public static void HandleIActorRequest(ushort opcode, long actorId, IActorRequest iActorRequest, Action<IActorResponse> reply)
        {
            Entity entity = Game.EventSystem.Get(actorId);
            if (entity == null)
            {
                FailResponse(iActorRequest, ErrorCore.ERR_NotFoundActor, reply);
                return;
            }

            MailBoxComponent mailBoxComponent = entity.GetComponent<MailBoxComponent>();
            if (mailBoxComponent == null)
            {
                Log.Warning($"actor not found mailbox: {entity.GetType().Name} {actorId} {iActorRequest}");
                FailResponse(iActorRequest, ErrorCore.ERR_NotFoundActor, reply);
                return;
            }

            switch (mailBoxComponent.MailboxType)
            {
                case MailboxType.MessageDispatcher:
                {
                    async ETTask MessageDispatcherHandler()
                    {
                        long instanceId = entity.InstanceId;
                        using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Mailbox, actorId))
                        {
                            if (entity.InstanceId != instanceId)
                            {
                                FailResponse(iActorRequest, ErrorCore.ERR_NotFoundActor, reply);
                                return;
                            }

                            await ActorMessageDispatcherComponent.Instance.Handle(entity, iActorRequest, reply);
                        }
                    }
                    
                    MessageDispatcherHandler().Coroutine();
                    break;
                }
                case MailboxType.UnOrderMessageDispatcher:
                {
                    ActorMessageDispatcherComponent.Instance.Handle(entity, iActorRequest, reply).Coroutine();
                    break;
                }
            }
        }

        private static void FailResponse(IActorRequest iActorRequest, int error, Action<IActorResponse> reply)
        {
            IActorResponse response = ActorHelper.CreateResponse(iActorRequest, error);
            reply.Invoke(response);
        }


        public static void HandleIActorMessage(ushort opcode, long actorId, IActorMessage iActorMessage)
        {
            OpcodeHelper.LogMsg(opcode, actorId, iActorMessage);

            Entity entity = Game.EventSystem.Get(actorId);
            if (entity == null)
            {
                Log.Error($"not found actor: {actorId} {iActorMessage}");
                return;
            }

            MailBoxComponent mailBoxComponent = entity.GetComponent<MailBoxComponent>();
            if (mailBoxComponent == null)
            {
                Log.Error($"actor not found mailbox: {entity.GetType().Name} {actorId} {iActorMessage}");
                return;
            }

            switch (mailBoxComponent.MailboxType)
            {
                
                case MailboxType.MessageDispatcher:
                {
                    async ETTask MessageDispatcherHandler()
                    {
                        long instanceId = entity.InstanceId;
                        using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Mailbox, actorId))
                        {
                            if (entity.InstanceId != instanceId)
                            {
                                return;
                            }

                            await ActorMessageDispatcherComponent.Instance.Handle(entity, iActorMessage, null);
                        }
                    }
                    MessageDispatcherHandler().Coroutine();
                    break;
                }
                case MailboxType.UnOrderMessageDispatcher:
                {
                    ActorMessageDispatcherComponent.Instance.Handle(entity, iActorMessage, null).Coroutine();
                    break;
                }
            }
        }
    }
}
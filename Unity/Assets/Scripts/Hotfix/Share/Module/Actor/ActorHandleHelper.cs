using System;

namespace ET
{
    public static partial class ActorHandleHelper
    {
        [EnableAccessEntiyChild]
        public static async ETTask HandleMessage(Fiber fiber, ActorId actorId, MessageObject message)
        {
            await ETTask.CompletedTask;
            /*
            // 收到actor消息,放入actor队列
            switch (message)
            {
                case IActorRequest iActorRequest:
                {


                    switch (mailBoxComponent.MailboxType)
                    {
                        case MailboxType.OrderedMessage:
                        {
                            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Mailbox, actorId.InstanceId))
                            {
                                if (entity.InstanceId != actorId.InstanceId)
                                {
                                    IActorResponse response = ActorHelper.CreateResponse(iActorRequest, ErrorCore.ERR_NotFoundActor);
                                    ActorMessageSenderComponent.Instance.Reply(actorId, response);
                                    break;
                                }

                                await ActorMessageDispatcherComponent.Instance.Handle(entity, actorId, iActorRequest);
                            }

                            break;
                        }
                        case MailboxType.UnOrderMessageDispatcher:
                        {
                            await ActorMessageDispatcherComponent.Instance.Handle(entity, actorId, iActorRequest);
                            break;
                        }
                        default:
                            throw new Exception($"no mailboxtype: {mailBoxComponent.MailboxType} {iActorRequest}");
                    }
                    break;
                }
                case IActorMessage iActorMessage:
                {
                    mailBoxComponent = Fiber.Instance.Mailboxes.Get(actorId.InstanceId);
                    if (mailBoxComponent == null)
                    {
                        Log.Error($"actor not found mailbox: {actorId} {iActorMessage}");
                        return;
                    }
                    mailBoxComponent.Add(iActorMessage as MessageObject);

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

                                await ActorMessageDispatcherComponent.Instance.Handle(entity, actorId, iActorMessage);
                            }

                            break;
                        }
                        case MailboxType.UnOrderMessageDispatcher:
                        {
                            await ActorMessageDispatcherComponent.Instance.Handle(entity, actorId, iActorMessage);
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

                    break;
                    
                }
                
            }
                        */
        }
    }
}
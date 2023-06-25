using System;

namespace ET.Server
{
    [Event(SceneType.Process)]
    public class NetInnerComponentOnReadEvent: AEvent<Scene, NetInnerComponentOnRead>
    {
        [EnableAccessEntiyChild]
        protected override async ETTask Run(Scene scene, NetInnerComponentOnRead args)
        {
            await ETTask.CompletedTask;
            try
            {
                ActorId actorId = args.ActorId;
                MessageObject message = args.Message as MessageObject;
                if (actorId.Address != scene.Fiber.Address)
                {
                    ActorMessageQueue.Instance.Send(actorId, message);
                    return;
                }

                actorId.Address = scene.Fiber.Address;

                if (message is IActorResponse iActorResponse)
                {
                    ActorMessageSenderComponent.Instance.HandleIActorResponse(iActorResponse);
                    return;
                }

                // 收到actor消息,放入actor队列
                switch (message)
                {
                    case IActorRequest iActorRequest:
                    {
                        Entity entity = Fiber.Instance.ActorEntities.Get(actorId);
                        if (entity == null)
                        {
                            IActorResponse response = ActorHelper.CreateResponse(iActorRequest, ErrorCore.ERR_NotFoundActor);
                            ActorHandleHelper.Reply(actorId, response);
                            return;
                        }
            
                        Log.Debug(message.ToJson());

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
                        Entity entity = Fiber.Instance.ActorEntities.Get(actorId);
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
            }
            catch (Exception e)
            {
                Log.Error($"InnerMessageDispatcher error: {args.Message.GetType().FullName}\n{e}");
            }
        }
    }
}
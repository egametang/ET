using System;

namespace ET.Server
{
    [Event(SceneType.Process)]
    public class NetInnerComponentOnReadEvent: AEvent<NetInnerComponentOnRead>
    {
        [EnableAccessEntiyChild]
        protected override async ETTask Run(Scene scene, NetInnerComponentOnRead args)
        {
            try
            {
                long actorId = args.ActorId;
                object message = args.Message;

                InstanceIdStruct instanceIdStruct = new(actorId);
                int fromProcess = instanceIdStruct.Process;
                instanceIdStruct.Process = Options.Instance.Process;
                long realActorId = instanceIdStruct.ToLong();
                
                // 收到actor消息,放入actor队列
                switch (message)
                {
                    case IActorResponse iActorResponse:
                    {
                        ActorMessageSenderComponent.Instance.RunMessage(realActorId, iActorResponse);
                        break;
                    }
                    case IActorRequest iActorRequest:
                    {
                        void Reply(IActorResponse response)
                        {
                            if (fromProcess == Options.Instance.Process) // 返回消息是同一个进程
                            {
                                // NetInnerComponent.Instance.HandleMessage(realActorId, response); 等同于直接调用下面这句
                                ActorMessageSenderComponent.Instance.RunMessage(realActorId, response);
                                return;
                            }
                            
                            Session replySession = NetInnerComponent.Instance.Get(fromProcess);
                            // 发回真实的actorId 做查问题使用
                            replySession.Send(realActorId, response);
                        }
                        
                        Entity entity = Root.Instance.Get(realActorId);
                        if (entity == null)
                        {
                            IActorResponse response = ActorHelper.CreateResponse(iActorRequest, ErrorCore.ERR_NotFoundActor);
                            Reply(response);
                            break;
                        }

                        MailBoxComponent mailBoxComponent = entity.GetComponent<MailBoxComponent>();
                        if (mailBoxComponent == null)
                        {
                            Log.Warning($"actor not found mailbox: {entity.GetType().Name} {realActorId} {iActorRequest}");
                            IActorResponse response = ActorHelper.CreateResponse(iActorRequest, ErrorCore.ERR_NotFoundActor);
                            Reply(response);
                            break;
                        }

                        switch (mailBoxComponent.MailboxType)
                        {
                            case MailboxType.MessageDispatcher:
                            {
                                using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Mailbox, realActorId))
                                {
                                    if (entity.InstanceId != realActorId)
                                    {
                                        IActorResponse response = ActorHelper.CreateResponse(iActorRequest, ErrorCore.ERR_NotFoundActor);
                                        Reply(response);
                                        break;
                                    }
                                    await ActorMessageDispatcherComponent.Instance.Handle(entity, iActorRequest, Reply);
                                }
                                break;
                            }
                            case MailboxType.UnOrderMessageDispatcher:
                            {
                                await ActorMessageDispatcherComponent.Instance.Handle(entity, iActorRequest, Reply);
                                break;
                            }
                        }
                        break;
                    }
                    case IActorMessage iActorMessage:
                    {
                        Entity entity = Root.Instance.Get(realActorId);
                        if (entity == null)
                        {
                            Log.Error($"not found actor: {scene.Name} {realActorId} {message}");
                            break;
                        }
                        
                        MailBoxComponent mailBoxComponent = entity.GetComponent<MailBoxComponent>();
                        if (mailBoxComponent == null)
                        {
                            Log.Error($"actor not found mailbox: {entity.GetType().Name} {realActorId} {iActorMessage}");
                            break;
                        }

                        switch (mailBoxComponent.MailboxType)
                        {
                            case MailboxType.MessageDispatcher:
                            {
                                using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Mailbox, realActorId))
                                {
                                    if (entity.InstanceId != realActorId)
                                    {
                                        break;
                                    }
                                    await ActorMessageDispatcherComponent.Instance.Handle(entity, iActorMessage, null);
                                }
                                break;
                            }
                            case MailboxType.UnOrderMessageDispatcher:
                            {
                                await ActorMessageDispatcherComponent.Instance.Handle(entity, iActorMessage, null);
                                break;
                            }
                            case MailboxType.GateSession:
                            {
                                if (entity is Session gateSession)
                                {
                                    // 发送给客户端
                                    gateSession.Send(0, iActorMessage);
                                }
                                break;
                            }
                        }
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error($"InnerMessageDispatcher error: {args.Message.GetType().Name}\n{e}");
            }
        }
    }
}
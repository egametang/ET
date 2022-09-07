using System;

namespace ET.Server
{
    [Event(SceneType.Process)]
    public class NetInnerComponentOnReadEvent: AEvent<NetInnerComponentOnRead>
    {
        protected override async ETTask Run(Scene scene, NetInnerComponentOnRead args)
        {
            ushort opcode = 0;
            try
            {
                Session session = args.Session;
                long actorId = args.ActorId;
                object message = args.Message;

                opcode = NetServices.Instance.GetOpcode(message.GetType());
                InstanceIdStruct instanceIdStruct = new InstanceIdStruct(actorId);
                int fromProcess = instanceIdStruct.Process;
                instanceIdStruct.Process = Options.Instance.Process;
                long realActorId = instanceIdStruct.ToLong();

                // 收到actor消息,放入actor队列
                switch (message)
                {
                    case IActorRequest iActorRequest:
                    {
                        void Reply(IActorResponse response)
                        {
                            Session replySession = NetInnerComponent.Instance.Get(fromProcess);
                            // 发回真实的actorId 做查问题使用
                            replySession.Send(realActorId, response);
                        }
                        InnerMessageDispatcherHelper.HandleIActorRequest(opcode, realActorId, iActorRequest, Reply);
                        return;
                    }
                    case IActorResponse iActorResponse:
                    {
                        InnerMessageDispatcherHelper.HandleIActorResponse(opcode, realActorId, iActorResponse);
                        return;
                    }
                    case IActorMessage iactorMessage:
                    {
                        // 内网收到外网消息，有可能是gateUnit消息，还有可能是gate广播消息
                        if (OpcodeTypeComponent.Instance.IsOutrActorMessage(opcode))
                        {
                            Entity entity = EventSystem.Instance.Get(realActorId);
                            if (entity == null)
                            {
                                Log.Error($"not found actor: {session.DomainScene().Name}  {opcode} {realActorId} {message}");
                                return;
                            }
                    
                            if (entity is Session gateSession)
                            {
                                // 发送给客户端
                                gateSession.Send(0, iactorMessage);
                                return;
                            }
                        }
                        
                        InnerMessageDispatcherHelper.HandleIActorMessage(opcode, realActorId, iactorMessage);
                        return;
                    }
                    case IResponse iResponse:
                    {
                        session.OnResponse(iResponse);
                        return;
                    }
                    default:
                    {
                        MessageDispatcherComponent.Instance.Handle(session, message);
                        break;
                    }
                }

                await ETTask.CompletedTask;
            }
            catch (Exception e)
            {
                Log.Error($"InnerMessageDispatcher error: {opcode}\n{e}");
            }
        }
    }
}
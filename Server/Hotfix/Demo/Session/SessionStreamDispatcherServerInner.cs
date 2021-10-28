using System;
using System.IO;

namespace ET
{
    [SessionStreamDispatcher(SessionStreamDispatcherType.SessionStreamDispatcherServerInner)]
    public class SessionStreamDispatcherServerInner: ISessionStreamDispatcher
    {
        public void Dispatch(Session session, MemoryStream memoryStream)
        {
            ushort opcode = 0;
            try
            {
                long actorId = BitConverter.ToInt64(memoryStream.GetBuffer(), Packet.ActorIdIndex);
                opcode = BitConverter.ToUInt16(memoryStream.GetBuffer(), Packet.OpcodeIndex);
                Type type = null;
                object message = null;
#if SERVER   

                // 内网收到外网消息，有可能是gateUnit消息，还有可能是gate广播消息
                if (OpcodeTypeComponent.Instance.IsOutrActorMessage(opcode))
                {
                    InstanceIdStruct instanceIdStruct = new InstanceIdStruct(actorId);
                    instanceIdStruct.Process = Game.Options.Process;
                    long realActorId = instanceIdStruct.ToLong();
                    
                    
                    Entity entity = Game.EventSystem.Get(realActorId);
                    if (entity == null)
                    {
                        type = OpcodeTypeComponent.Instance.GetType(opcode);
                        message = MessageSerializeHelper.DeserializeFrom(opcode, type, memoryStream);
                        Log.Error($"not found actor: {session.DomainScene().Name}  {opcode} {realActorId} {message}");
                        return;
                    }
                    
                    if (entity is Session gateSession)
                    {
                        // 发送给客户端
                        memoryStream.Seek(Packet.OpcodeIndex, SeekOrigin.Begin);
                        gateSession.Send(0, memoryStream);
                        return;
                    }
                }
#endif
                        
                        
                type = OpcodeTypeComponent.Instance.GetType(opcode);
                message = MessageSerializeHelper.DeserializeFrom(opcode, type, memoryStream);

                if (message is IResponse iResponse && !(message is IActorResponse))
                {
                    session.OnRead(opcode, iResponse);
                    return;
                }

                OpcodeHelper.LogMsg(session.DomainZone(), opcode, message);

                // 收到actor消息,放入actor队列
                switch (message)
                {
                    case IActorRequest iActorRequest:
                    {
                        InstanceIdStruct instanceIdStruct = new InstanceIdStruct(actorId);
                        int fromProcess = instanceIdStruct.Process;
                        instanceIdStruct.Process = Game.Options.Process;
                        long realActorId = instanceIdStruct.ToLong();
                        
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
                        InstanceIdStruct instanceIdStruct = new InstanceIdStruct(actorId);
                        instanceIdStruct.Process = Game.Options.Process;
                        long realActorId = instanceIdStruct.ToLong();
                        InnerMessageDispatcherHelper.HandleIActorResponse(opcode, realActorId, iActorResponse);
                        return;
                    }
                    case IActorMessage iactorMessage:
                    {
                        InstanceIdStruct instanceIdStruct = new InstanceIdStruct(actorId);
                        instanceIdStruct.Process = Game.Options.Process;
                        long realActorId = instanceIdStruct.ToLong();
                        InnerMessageDispatcherHelper.HandleIActorMessage(opcode, realActorId, iactorMessage);
                        return;
                    }
                    default:
                    {
                        MessageDispatcherComponent.Instance.Handle(session, opcode, message);
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error($"InnerMessageDispatcher error: {opcode}\n{e}");
            }
        }
    }
}
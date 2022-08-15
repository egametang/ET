using System;
using System.IO;

namespace ET.Client
{
    [Callback(SessionStreamCallbackId.SessionStreamDispatcherClientOuter)]
    public class SessionStreamDispatcherClientOuter: ACallbackHandler<SessionStreamCallback>
    {
        public override void Handle(SessionStreamCallback sessionStreamCallback)
        {
            ushort opcode = BitConverter.ToUInt16(sessionStreamCallback.MemoryStream.GetBuffer(), Packet.KcpOpcodeIndex);
            Type type = OpcodeTypeComponent.Instance.GetType(opcode);
            object message = MessageSerializeHelper.DeserializeFrom(opcode, type, sessionStreamCallback.MemoryStream);
            
            if (message is IResponse response)
            {
                sessionStreamCallback.Session.OnRead(opcode, response);
                return;
            }

            OpcodeHelper.LogMsg(sessionStreamCallback.Session.DomainZone(), opcode, message);
            // 普通消息或者是Rpc请求消息
            MessageDispatcherComponent.Instance.Handle(sessionStreamCallback.Session, opcode, message);
        }
    }
}
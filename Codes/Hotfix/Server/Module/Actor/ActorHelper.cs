using System;
using System.IO;

namespace ET.Server
{
    public static class ActorHelper
    {
        public static IActorResponse CreateResponse(IActorRequest iActorRequest, int error)
        {
            Type responseType = OpcodeTypeComponent.Instance.GetResponseType(iActorRequest.GetType());
            IActorResponse response = (IActorResponse)Activator.CreateInstance(responseType);
            response.Error = error;
            response.RpcId = iActorRequest.RpcId;
            return response;
        }

        public static object ToActorMessage(this MemoryStream memoryStream)
        {
            ushort opcode = BitConverter.ToUInt16(memoryStream.GetBuffer(), 8);
            Type type = OpcodeTypeComponent.Instance.GetType(opcode);

            if (opcode < OpcodeRangeDefine.PbMaxOpcode)
            {
                return ProtobufHelper.FromBytes(type, memoryStream.GetBuffer(), 10, (int)memoryStream.Length - 10);
            }

            if (opcode >= OpcodeRangeDefine.JsonMinOpcode)
            {
                return JsonHelper.FromJson(type, memoryStream.GetBuffer().ToStr(10, (int)(memoryStream.Length - 10)));
            }

            return MongoHelper.FromBson(type, memoryStream.GetBuffer(), 10, (int)memoryStream.Length - 10);
        }
    }
}
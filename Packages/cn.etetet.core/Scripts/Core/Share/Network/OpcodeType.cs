using System;
using System.Collections.Generic;

namespace ET
{
    public class OpcodeType: Singleton<OpcodeType>, ISingletonAwake
    {
        // 初始化后不变，所以主线程，网络线程都可以读
        private readonly DoubleMap<Type, ushort> typeOpcode = new();
        
        private readonly Dictionary<Type, Type> requestResponse = new();
        
        public void Awake()
        {
            HashSet<Type> types = CodeTypes.Instance.GetTypes(typeof (MessageAttribute));
            foreach (Type type in types)
            {
                object[] att = type.GetCustomAttributes(typeof (MessageAttribute), false);
                if (att.Length == 0)
                {
                    continue;
                }

                MessageAttribute messageAttribute = att[0] as MessageAttribute;
                if (messageAttribute == null)
                {
                    continue;
                }

                ushort opcode = messageAttribute.Opcode;
                if (opcode != 0)
                {
                    this.typeOpcode.Add(type, opcode);
                }

                // 检查request response
                if (typeof (IRequest).IsAssignableFrom(type))
                {
                    if (typeof (ILocationMessage).IsAssignableFrom(type))
                    {
                        this.requestResponse.Add(type, typeof (MessageResponse));
                        continue;
                    }

                    var attrs = type.GetCustomAttributes(typeof (ResponseTypeAttribute), false);
                    if (attrs.Length == 0)
                    {
                        Log.Error($"not found responseType: {type}");
                        continue;
                    }

                    ResponseTypeAttribute responseTypeAttribute = attrs[0] as ResponseTypeAttribute;
                    this.requestResponse.Add(type, CodeTypes.Instance.GetType($"ET.{responseTypeAttribute.Type}"));
                }
            }
        }
        
        public ushort GetOpcode(Type type)
        {
            return this.typeOpcode.GetValueByKey(type);
        }

        public Type GetType(ushort opcode)
        {
            Type type = this.typeOpcode.GetKeyByValue(opcode);
            if (type == null)
            {
                throw new Exception($"OpcodeType not found type: {opcode}");
            }
            return type;
        }

        public Type GetResponseType(Type request)
        {
            if (!this.requestResponse.TryGetValue(request, out Type response))
            {
                throw new Exception($"not found response type, request type: {request.FullName}");
            }

            return response;
        }
    }
}
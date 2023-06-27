using System;
using System.Collections.Generic;

namespace ET
{
    public class OpcodeType: Singleton<OpcodeType>, ISingletonAwake, ISingletonLoad
    {
        // 初始化后不变，所以主线程，网络线程都可以读
        private readonly DoubleMap<Type, ushort> typeOpcode = new();
        
        private readonly HashSet<ushort> outrActorMessage = new HashSet<ushort>();

        private readonly Dictionary<Type, Type> requestResponse = new Dictionary<Type, Type>();
        
        public void Awake()
        {
            HashSet<Type> types = EventSystem.Instance.GetTypes(typeof (MessageAttribute));
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
                
                this.typeOpcode.Add(type, opcode);

                if (OpcodeHelper.IsOuterMessage(opcode) && typeof (IActorMessage).IsAssignableFrom(type))
                {
                    this.outrActorMessage.Add(opcode);
                }

                // 检查request response
                if (typeof (IRequest).IsAssignableFrom(type))
                {
                    if (typeof (IActorLocationMessage).IsAssignableFrom(type))
                    {
                        this.requestResponse.Add(type, typeof (ActorResponse));
                        continue;
                    }

                    var attrs = type.GetCustomAttributes(typeof (ResponseTypeAttribute), false);
                    if (attrs.Length == 0)
                    {
                        Log.Error($"not found responseType: {type}");
                        continue;
                    }

                    ResponseTypeAttribute responseTypeAttribute = attrs[0] as ResponseTypeAttribute;
                    this.requestResponse.Add(type, EventSystem.Instance.GetType($"ET.{responseTypeAttribute.Type}"));
                }
            }
        }
        
        public ushort GetOpcode(Type type)
        {
            return this.typeOpcode.GetValueByKey(type);
        }

        public Type GetType(ushort opcode)
        {
            return this.typeOpcode.GetKeyByValue(opcode);
        }

        public bool IsOutrActorMessage(ushort opcode)
        {
            return this.outrActorMessage.Contains(opcode);
        }

        public Type GetResponseType(Type request)
        {
            if (!this.requestResponse.TryGetValue(request, out Type response))
            {
                throw new Exception($"not found response type, request type: {request.FullName}");
            }

            return response;
        }

        public void Load()
        {
            World.Instance.AddSingleton<OpcodeType>();
        }
    }
}
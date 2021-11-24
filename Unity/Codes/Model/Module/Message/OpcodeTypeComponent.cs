using System;
using System.Collections.Generic;

namespace ET
{
    [ObjectSystem]
    public class OpcodeTypeComponentAwakeSystem: AwakeSystem<OpcodeTypeComponent>
    {
        public override void Awake(OpcodeTypeComponent self)
        {
            OpcodeTypeComponent.Instance = self;
            self.Awake();
        }
    }

    [ObjectSystem]
    public class OpcodeTypeComponentDestroySystem: DestroySystem<OpcodeTypeComponent>
    {
        public override void Destroy(OpcodeTypeComponent self)
        {
            OpcodeTypeComponent.Instance = null;
        }
    }

    public class OpcodeTypeComponent: Entity
    {
        public static OpcodeTypeComponent Instance;
        
        private HashSet<ushort> outrActorMessage = new HashSet<ushort>();
        
        private readonly Dictionary<ushort, Type> opcodeTypes = new Dictionary<ushort, Type>();
        private readonly Dictionary<Type, ushort> typeOpcodes = new Dictionary<Type, ushort>();
        
        private readonly Dictionary<Type, Type> requestResponse = new Dictionary<Type, Type>();
        
        public void Awake()
        {
            this.opcodeTypes.Clear();
            this.typeOpcodes.Clear();
            this.requestResponse.Clear();

            HashSet<Type> types = Game.EventSystem.GetTypes(typeof (MessageAttribute));
            foreach (Type type in types)
            {
                object[] attrs = type.GetCustomAttributes(typeof (MessageAttribute), false);
                if (attrs.Length == 0)
                {
                    continue;
                }

                MessageAttribute messageAttribute = attrs[0] as MessageAttribute;
                if (messageAttribute == null)
                {
                    continue;
                }
                

                this.opcodeTypes.Add(messageAttribute.Opcode, type);
                this.typeOpcodes.Add(type, messageAttribute.Opcode);

                if (OpcodeHelper.IsOuterMessage(messageAttribute.Opcode) && typeof (IActorMessage).IsAssignableFrom(type))
                {
                    this.outrActorMessage.Add(messageAttribute.Opcode);
                }
                
                // 检查request response
                if (typeof (IRequest).IsAssignableFrom(type))
                {
                    if (typeof (IActorLocationMessage).IsAssignableFrom(type))
                    {
                        this.requestResponse.Add(type, typeof(ActorResponse));
                        continue;
                    }
                    
                    attrs = type.GetCustomAttributes(typeof (ResponseTypeAttribute), false);
                    if (attrs.Length == 0)
                    {
                        Log.Error($"not found responseType: {type}");
                        continue;
                    }

                    ResponseTypeAttribute responseTypeAttribute = attrs[0] as ResponseTypeAttribute;
                    this.requestResponse.Add(type, Game.EventSystem.GetType($"ET.{responseTypeAttribute.Type}"));
                }
            }
        }

        public bool IsOutrActorMessage(ushort opcode)
        {
            return this.outrActorMessage.Contains(opcode);
        }

        public ushort GetOpcode(Type type)
        {
            return this.typeOpcodes[type];
        }

        public Type GetType(ushort opcode)
        {
            return this.opcodeTypes[opcode];
        }

        public Type GetResponseType(Type request)
        {
            if (!this.requestResponse.TryGetValue(request, out Type response))
            {
                throw new Exception($"not found response type, request type: {request.GetType().Name}");
            }
            return response;
        }
    }
}
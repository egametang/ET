#if FEAT_COMPILER && !(FX11 || FEAT_IKVM)
using System;
using ProtoBuf.Meta;



namespace ProtoBuf.Serializers
{
    sealed class CompiledSerializer : IProtoTypeSerializer
    {
        bool IProtoTypeSerializer.HasCallbacks(TypeModel.CallbackType callbackType)
        {
            return head.HasCallbacks(callbackType); // these routes only used when bits of the model not compiled
        }
        bool IProtoTypeSerializer.CanCreateInstance()
        {
            return head.CanCreateInstance();
        }
        object IProtoTypeSerializer.CreateInstance(ProtoReader source)
        {
            return head.CreateInstance(source);
        }
        public void Callback(object value, TypeModel.CallbackType callbackType, SerializationContext context)
        {
            head.Callback(value, callbackType, context); // these routes only used when bits of the model not compiled
        }
        public static CompiledSerializer Wrap(IProtoTypeSerializer head, TypeModel model)
        {
            CompiledSerializer result = head as CompiledSerializer;
            if (result == null)
            {
                result = new CompiledSerializer(head, model);
                Helpers.DebugAssert(((IProtoTypeSerializer)result).ExpectedType == head.ExpectedType);
            }
            return result;
        }
        private readonly IProtoTypeSerializer head;
        private readonly Compiler.ProtoSerializer serializer;
        private readonly Compiler.ProtoDeserializer deserializer;
        private CompiledSerializer(IProtoTypeSerializer head, TypeModel model)
        {
            this.head = head;
            serializer = Compiler.CompilerContext.BuildSerializer(head, model);
            deserializer = Compiler.CompilerContext.BuildDeserializer(head, model);
        }
        bool IProtoSerializer.RequiresOldValue { get { return head.RequiresOldValue; } }
        bool IProtoSerializer.ReturnsValue { get { return head.ReturnsValue; } }

        Type IProtoSerializer.ExpectedType { get { return head.ExpectedType; } }

        void IProtoSerializer.Write(object value, ProtoWriter dest)
        {
            serializer(value, dest);
        }
        object IProtoSerializer.Read(object value, ProtoReader source)
        {
            return deserializer(value, source);
        }

        void IProtoSerializer.EmitWrite(Compiler.CompilerContext ctx, Compiler.Local valueFrom)
        {
            head.EmitWrite(ctx, valueFrom);
        }
        void IProtoSerializer.EmitRead(Compiler.CompilerContext ctx, Compiler.Local valueFrom)
        {
            head.EmitRead(ctx, valueFrom);
        }
        void IProtoTypeSerializer.EmitCallback(Compiler.CompilerContext ctx, Compiler.Local valueFrom, TypeModel.CallbackType callbackType)
        {
            head.EmitCallback(ctx, valueFrom, callbackType);
        }
        void IProtoTypeSerializer.EmitCreateInstance(Compiler.CompilerContext ctx)
        {
            head.EmitCreateInstance(ctx);
        }
    }
}
#endif
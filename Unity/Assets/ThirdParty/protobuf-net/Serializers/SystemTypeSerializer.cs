using System;

#if !NO_RUNTIME

namespace ProtoBuf.Serializers
{
    sealed class SystemTypeSerializer : IProtoSerializer
    {
        static readonly Type expectedType = typeof(Type);

        public SystemTypeSerializer(ProtoBuf.Meta.TypeModel model)
        {

        }

        public Type ExpectedType => expectedType;

        void IProtoSerializer.Write(object value, ProtoWriter dest)
        {
            ProtoWriter.WriteType((Type)value, dest);
        }

        object IProtoSerializer.Read(object value, ProtoReader source)
        {
            Helpers.DebugAssert(value == null); // since replaces
            return source.ReadType();
        }

        bool IProtoSerializer.RequiresOldValue => false;

        bool IProtoSerializer.ReturnsValue => true;

#if FEAT_COMPILER
        void IProtoSerializer.EmitWrite(Compiler.CompilerContext ctx, Compiler.Local valueFrom)
        {
            ctx.EmitBasicWrite("WriteType", valueFrom);
        }
        void IProtoSerializer.EmitRead(Compiler.CompilerContext ctx, Compiler.Local valueFrom)
        {
            ctx.EmitBasicRead("ReadType", ExpectedType);
        }
#endif
    }
}

#endif

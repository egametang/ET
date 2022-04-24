#if !NO_RUNTIME
using System;

namespace ProtoBuf.Serializers
{
    sealed class UInt64Serializer : IProtoSerializer
    {
        static readonly Type expectedType = typeof(ulong);

        public UInt64Serializer(ProtoBuf.Meta.TypeModel model)
        {

        }
        public Type ExpectedType => expectedType;

        bool IProtoSerializer.RequiresOldValue => false;

        bool IProtoSerializer.ReturnsValue => true;

        public object Read(object value, ProtoReader source)
        {
            Helpers.DebugAssert(value == null); // since replaces
            return source.ReadUInt64();
        }

        public void Write(object value, ProtoWriter dest)
        {
            ProtoWriter.WriteUInt64((ulong)value, dest);
        }

#if FEAT_COMPILER
        void IProtoSerializer.EmitWrite(Compiler.CompilerContext ctx, Compiler.Local valueFrom)
        {
            ctx.EmitBasicWrite("WriteUInt64", valueFrom);
        }
        void IProtoSerializer.EmitRead(Compiler.CompilerContext ctx, Compiler.Local valueFrom)
        {
            ctx.EmitBasicRead("ReadUInt64", ExpectedType);
        }
#endif
    }
}
#endif
#if !NO_RUNTIME
using System;

namespace ProtoBuf.Serializers
{
    sealed class ByteSerializer : IProtoSerializer
    {
        public Type ExpectedType { get { return expectedType; } }

        static readonly Type expectedType = typeof(byte);

        public ByteSerializer(ProtoBuf.Meta.TypeModel model) { }

        bool IProtoSerializer.RequiresOldValue => false;

        bool IProtoSerializer.ReturnsValue => true;

        public void Write(object value, ProtoWriter dest)
        {
            ProtoWriter.WriteByte((byte)value, dest);
        }

        public object Read(object value, ProtoReader source)
        {
            Helpers.DebugAssert(value == null); // since replaces
            return source.ReadByte();
        }

#if FEAT_COMPILER
        void IProtoSerializer.EmitWrite(Compiler.CompilerContext ctx, Compiler.Local valueFrom)
        {
            ctx.EmitBasicWrite("WriteByte", valueFrom);
        }

        void IProtoSerializer.EmitRead(Compiler.CompilerContext ctx, Compiler.Local valueFrom)
        {
            ctx.EmitBasicRead("ReadByte", ExpectedType);
        }
#endif
    }
}
#endif
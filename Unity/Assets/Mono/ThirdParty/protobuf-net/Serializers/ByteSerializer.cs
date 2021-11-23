#if !NO_RUNTIME
using System;

#if FEAT_IKVM
using Type = IKVM.Reflection.Type;
#endif



namespace ProtoBuf.Serializers
{
    sealed class ByteSerializer : IProtoSerializer
    {
        public Type ExpectedType { get { return expectedType; } }

#if FEAT_IKVM
        readonly Type expectedType;
#else
        static readonly Type expectedType = typeof(byte);
#endif
        public ByteSerializer(ProtoBuf.Meta.TypeModel model)
        {
#if FEAT_IKVM
            expectedType = model.MapType(typeof(byte));
#endif
        }
        bool IProtoSerializer.RequiresOldValue { get { return false; } }
        bool IProtoSerializer.ReturnsValue { get { return true; } }
#if !FEAT_IKVM
        public void Write(object value, ProtoWriter dest)
        {
            ProtoWriter.WriteByte((byte)value, dest);
        }
        public object Read(object value, ProtoReader source)
        {
            Helpers.DebugAssert(value == null); // since replaces
            return source.ReadByte();
        }
#endif

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
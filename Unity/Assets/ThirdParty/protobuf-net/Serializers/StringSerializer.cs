#if !NO_RUNTIME
using System;

namespace ProtoBuf.Serializers
{
    sealed class StringSerializer : IProtoSerializer
    {
        static readonly Type expectedType = typeof(string);

        public StringSerializer(ProtoBuf.Meta.TypeModel model)
        {
        }

        public Type ExpectedType => expectedType;

        public void Write(object value, ProtoWriter dest)
        {
            ProtoWriter.WriteString((string)value, dest);
        }
        bool IProtoSerializer.RequiresOldValue => false;

        bool IProtoSerializer.ReturnsValue => true;

        public object Read(object value, ProtoReader source)
        {
            Helpers.DebugAssert(value == null); // since replaces
            return source.ReadString();
        }
#if FEAT_COMPILER
        void IProtoSerializer.EmitWrite(Compiler.CompilerContext ctx, Compiler.Local valueFrom)
        {
            ctx.EmitBasicWrite("WriteString", valueFrom);
        }
        void IProtoSerializer.EmitRead(Compiler.CompilerContext ctx, Compiler.Local valueFrom)
        {
            ctx.EmitBasicRead("ReadString", ExpectedType);
        }
#endif
    }
}
#endif
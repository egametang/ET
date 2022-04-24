#if !NO_RUNTIME
using System;

namespace ProtoBuf.Serializers
{
    class UInt16Serializer : IProtoSerializer
    {
        static readonly Type expectedType = typeof(ushort);

        public UInt16Serializer(ProtoBuf.Meta.TypeModel model)
        {
        }

        public virtual Type ExpectedType => expectedType;

        bool IProtoSerializer.RequiresOldValue => false;

        bool IProtoSerializer.ReturnsValue => true;

        public virtual object Read(object value, ProtoReader source)
        {
            Helpers.DebugAssert(value == null); // since replaces
            return source.ReadUInt16();
        }

        public virtual void Write(object value, ProtoWriter dest)
        {
            ProtoWriter.WriteUInt16((ushort)value, dest);
        }

#if FEAT_COMPILER
        void IProtoSerializer.EmitWrite(Compiler.CompilerContext ctx, Compiler.Local valueFrom)
        {
            ctx.EmitBasicWrite("WriteUInt16", valueFrom);
        }
        void IProtoSerializer.EmitRead(Compiler.CompilerContext ctx, Compiler.Local valueFrom)
        {
            ctx.EmitBasicRead("ReadUInt16", ctx.MapType(typeof(ushort)));
        }
#endif
    }
}
#endif
#if !NO_RUNTIME
using System;
#if COREFX
using System.Reflection;
#endif
#if FEAT_COMPILER
using System.Reflection.Emit;
#endif

#if FEAT_IKVM
using Type = IKVM.Reflection.Type;
#endif


namespace ProtoBuf.Serializers
{
    sealed class BlobSerializer : IProtoSerializer
    {
        public Type ExpectedType { get { return expectedType; } }

#if FEAT_IKVM
        readonly Type expectedType;
#else
        static readonly Type expectedType = typeof(byte[]);
#endif
        public BlobSerializer(ProtoBuf.Meta.TypeModel model, bool overwriteList)
        {
#if FEAT_IKVM
            expectedType = model.MapType(typeof(byte[]));
#endif
            this.overwriteList = overwriteList;
        }
        private readonly bool overwriteList;
#if !FEAT_IKVM
        public object Read(object value, ProtoReader source)
        {
            return ProtoReader.AppendBytes(overwriteList ? null : (byte[])value, source);
        }
        public void Write(object value, ProtoWriter dest)
        {
            ProtoWriter.WriteBytes((byte[])value, dest);
        }
#endif
        bool IProtoSerializer.RequiresOldValue { get { return !overwriteList; } }
        bool IProtoSerializer.ReturnsValue { get { return true; } }
#if FEAT_COMPILER
        void IProtoSerializer.EmitWrite(Compiler.CompilerContext ctx, Compiler.Local valueFrom)
        {
            ctx.EmitBasicWrite("WriteBytes", valueFrom);
        }
        void IProtoSerializer.EmitRead(Compiler.CompilerContext ctx, Compiler.Local valueFrom)
        {
            if (overwriteList)
            {
                ctx.LoadNullRef();
            }
            else
            {
                ctx.LoadValue(valueFrom);
            }
            ctx.LoadReaderWriter();
            ctx.EmitCall(ctx.MapType(typeof(ProtoReader))
               .GetMethod("AppendBytes"));
        }
#endif
    }
}
#endif
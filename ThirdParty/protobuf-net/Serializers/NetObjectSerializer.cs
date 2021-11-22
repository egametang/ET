#if !NO_RUNTIME
using System;
using System.Reflection;
using ProtoBuf.Meta;

namespace ProtoBuf.Serializers
{
    sealed class NetObjectSerializer : IProtoSerializer
    {
        private readonly int key;
        private readonly Type type;

        private readonly BclHelpers.NetObjectOptions options;

        public NetObjectSerializer(TypeModel model, Type type, int key, BclHelpers.NetObjectOptions options)
        {
            bool dynamicType = (options & BclHelpers.NetObjectOptions.DynamicType) != 0;
            this.key = dynamicType ? -1 : key;
            this.type = dynamicType ? model.MapType(typeof(object)) : type;
            this.options = options;
        }

        public Type ExpectedType => type;

        public bool ReturnsValue => true;

        public bool RequiresOldValue => true;

        public object Read(object value, ProtoReader source)
        {
            return BclHelpers.ReadNetObject(value, source, key, type == typeof(object) ? null : type, options);
        }

        public void Write(object value, ProtoWriter dest)
        {
            BclHelpers.WriteNetObject(value, dest, key, options);
        }

#if FEAT_COMPILER
        public void EmitRead(Compiler.CompilerContext ctx, Compiler.Local valueFrom)
        {
            ctx.LoadValue(valueFrom);
            ctx.CastToObject(type);
            ctx.LoadReaderWriter();
            ctx.LoadValue(ctx.MapMetaKeyToCompiledKey(key));
            if (type == ctx.MapType(typeof(object))) ctx.LoadNullRef();
            else ctx.LoadValue(type);
            ctx.LoadValue((int)options);
            ctx.EmitCall(ctx.MapType(typeof(BclHelpers)).GetMethod("ReadNetObject"));
            ctx.CastFromObject(type);
        }
        public void EmitWrite(Compiler.CompilerContext ctx, Compiler.Local valueFrom)
        {
            ctx.LoadValue(valueFrom);
            ctx.CastToObject(type);
            ctx.LoadReaderWriter();
            ctx.LoadValue(ctx.MapMetaKeyToCompiledKey(key));
            ctx.LoadValue((int)options);
            ctx.EmitCall(ctx.MapType(typeof(BclHelpers)).GetMethod("WriteNetObject"));
        }
#endif
    }
}
#endif
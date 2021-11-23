#if !NO_RUNTIME
using System;

#if FEAT_IKVM
using Type = IKVM.Reflection.Type;
using IKVM.Reflection;
#else

#endif

namespace ProtoBuf.Serializers
{
    sealed class DateTimeSerializer : IProtoSerializer
    {
#if FEAT_IKVM
        readonly Type expectedType;
#else
        static readonly Type expectedType = typeof(DateTime);
#endif
        public Type ExpectedType => expectedType;

        bool IProtoSerializer.RequiresOldValue => false;
        bool IProtoSerializer.ReturnsValue => true;

        private readonly bool includeKind, wellKnown;
        public DateTimeSerializer(DataFormat dataFormat, ProtoBuf.Meta.TypeModel model)
        {
#if FEAT_IKVM
            expectedType = model.MapType(typeof(DateTime));
#endif
            wellKnown = dataFormat == DataFormat.WellKnown;
            includeKind = model != null && model.SerializeDateTimeKind();
        }
#if !FEAT_IKVM
        public object Read(object value, ProtoReader source)
        {
            if(wellKnown)
            {
                return BclHelpers.ReadTimestamp(source);
            }
            else
            {
                Helpers.DebugAssert(value == null); // since replaces
                return BclHelpers.ReadDateTime(source);
            }            
        }
        public void Write(object value, ProtoWriter dest)
        {
            if(wellKnown)
                BclHelpers.WriteTimestamp((DateTime)value, dest);
            else if(includeKind)
                BclHelpers.WriteDateTimeWithKind((DateTime)value, dest);
            else
                BclHelpers.WriteDateTime((DateTime)value, dest);
        }
#endif
#if FEAT_COMPILER
        void IProtoSerializer.EmitWrite(Compiler.CompilerContext ctx, Compiler.Local valueFrom)
        {
            ctx.EmitWrite(ctx.MapType(typeof(BclHelpers)),
                wellKnown ? nameof(BclHelpers.WriteTimestamp)
                : includeKind ? nameof(BclHelpers.WriteDateTimeWithKind) : nameof(BclHelpers.WriteDateTime), valueFrom);
        }
        void IProtoSerializer.EmitRead(Compiler.CompilerContext ctx, Compiler.Local valueFrom)
        {
            if (wellKnown) ctx.LoadValue(valueFrom);
            ctx.EmitBasicRead(ctx.MapType(typeof(BclHelpers)),
                wellKnown ? nameof(BclHelpers.ReadTimestamp) : nameof(BclHelpers.ReadDateTime),
                ExpectedType);
        }
#endif

    }
}
#endif
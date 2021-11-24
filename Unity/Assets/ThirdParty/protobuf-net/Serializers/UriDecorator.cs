#if !NO_RUNTIME
using System;

//using ProtoBuf.Compiler;
#if FEAT_IKVM
using Type = IKVM.Reflection.Type;
using IKVM.Reflection;
#else

#endif

namespace ProtoBuf.Serializers
{
    sealed class UriDecorator : ProtoDecoratorBase
    {
#if FEAT_IKVM
        readonly Type expectedType;
#else
        static readonly Type expectedType = typeof(Uri);
#endif
        public UriDecorator(ProtoBuf.Meta.TypeModel model, IProtoSerializer tail) : base(tail)
        {
#if FEAT_IKVM
            expectedType = model.MapType(typeof(Uri));
#endif
        }
        public override Type ExpectedType { get { return expectedType; } }
        public override bool RequiresOldValue { get { return false; } }
        public override bool ReturnsValue { get { return true; } }
        

#if !FEAT_IKVM
        public override void Write(object value, ProtoWriter dest)
        {
            Tail.Write(((Uri)value).OriginalString, dest);
        }

        public override object Read(object value, ProtoReader source)
        {
            Helpers.DebugAssert(value == null); // not expecting incoming
            string s = (string)Tail.Read(null, source);
            return s.Length == 0 ? null : new Uri(s, UriKind.RelativeOrAbsolute);
        }
#endif

#if FEAT_COMPILER
        protected override void EmitWrite(Compiler.CompilerContext ctx, Compiler.Local valueFrom)
        {
            ctx.LoadValue(valueFrom);
            ctx.LoadValue(typeof(Uri).GetProperty("OriginalString"));
            Tail.EmitWrite(ctx, null);
        }
        protected override void EmitRead(Compiler.CompilerContext ctx, Compiler.Local valueFrom)
        {
            Tail.EmitRead(ctx, valueFrom);
            ctx.CopyValue();
            Compiler.CodeLabel @nonEmpty = ctx.DefineLabel(), @end = ctx.DefineLabel();
            ctx.LoadValue(typeof(string).GetProperty("Length"));
            ctx.BranchIfTrue(@nonEmpty, true);
            ctx.DiscardValue();
            ctx.LoadNullRef();
            ctx.Branch(@end, true);
            ctx.MarkLabel(@nonEmpty);
            ctx.LoadValue((int)UriKind.RelativeOrAbsolute);
            ctx.EmitCtor(ctx.MapType(typeof(Uri)), ctx.MapType(typeof(string)), ctx.MapType(typeof(UriKind)));
            ctx.MarkLabel(@end);
        }
#endif 
    }
}
#endif

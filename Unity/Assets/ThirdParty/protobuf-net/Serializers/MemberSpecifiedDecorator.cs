#if !NO_RUNTIME
using System;
using System.Reflection;

namespace ProtoBuf.Serializers
{
    sealed class MemberSpecifiedDecorator : ProtoDecoratorBase
    {
        public override Type ExpectedType => Tail.ExpectedType;

        public override bool RequiresOldValue => Tail.RequiresOldValue;

        public override bool ReturnsValue => Tail.ReturnsValue;

        private readonly MethodInfo getSpecified, setSpecified;
        public MemberSpecifiedDecorator(MethodInfo getSpecified, MethodInfo setSpecified, IProtoSerializer tail)
            : base(tail)
        {
            if (getSpecified == null && setSpecified == null) throw new InvalidOperationException();
            this.getSpecified = getSpecified;
            this.setSpecified = setSpecified;
        }

        public override void Write(object value, ProtoWriter dest)
        {
            if (getSpecified == null || (bool)getSpecified.Invoke(value, null))
            {
                Tail.Write(value, dest);
            }
        }

        public override object Read(object value, ProtoReader source)
        {
            object result = Tail.Read(value, source);
            if (setSpecified != null) setSpecified.Invoke(value, new object[] { true });
            return result;
        }

#if FEAT_COMPILER
        protected override void EmitWrite(Compiler.CompilerContext ctx, Compiler.Local valueFrom)
        {
            if (getSpecified == null)
            {
                Tail.EmitWrite(ctx, valueFrom);
                return;
            }
            using (Compiler.Local loc = ctx.GetLocalWithValue(ExpectedType, valueFrom))
            {
                ctx.LoadAddress(loc, ExpectedType);
                ctx.EmitCall(getSpecified);
                Compiler.CodeLabel done = ctx.DefineLabel();
                ctx.BranchIfFalse(done, false);
                Tail.EmitWrite(ctx, loc);
                ctx.MarkLabel(done);
            }

        }
        protected override void EmitRead(Compiler.CompilerContext ctx, Compiler.Local valueFrom)
        {
            if (setSpecified == null)
            {
                Tail.EmitRead(ctx, valueFrom);
                return;
            }
            using (Compiler.Local loc = ctx.GetLocalWithValue(ExpectedType, valueFrom))
            {
                Tail.EmitRead(ctx, loc);
                ctx.LoadAddress(loc, ExpectedType);
                ctx.LoadValue(1); // true
                ctx.EmitCall(setSpecified);
            }
        }
#endif
    }
}
#endif
#if !NO_RUNTIME
using System;

using ProtoBuf.Meta;

#if FEAT_IKVM
using Type = IKVM.Reflection.Type;
using IKVM.Reflection;
#else

#endif

namespace ProtoBuf.Serializers
{
    sealed class NullDecorator : ProtoDecoratorBase
    {
        private readonly Type expectedType;
        public const int Tag = 1;
        public NullDecorator(TypeModel model, IProtoSerializer tail) : base(tail)
        {
            if (!tail.ReturnsValue)
                throw new NotSupportedException("NullDecorator only supports implementations that return values");

            Type tailType = tail.ExpectedType;
            if (Helpers.IsValueType(tailType))
            {
#if NO_GENERICS
                throw new NotSupportedException("NullDecorator cannot be used with a struct without generics support");
#else
                expectedType = model.MapType(typeof(Nullable<>)).MakeGenericType(tailType);
#endif
            }
            else
            {
                expectedType = tailType;
            }

        }

        public override Type ExpectedType
        {
            get { return expectedType; }
        }
        public override bool ReturnsValue
        {
            get { return true; }
        }
        public override bool RequiresOldValue
        {
            get { return true; }
        }
#if FEAT_COMPILER
        protected override void EmitRead(Compiler.CompilerContext ctx, Compiler.Local valueFrom)
        {
            using (Compiler.Local oldValue = ctx.GetLocalWithValue(expectedType, valueFrom))
            using (Compiler.Local token = new Compiler.Local(ctx, ctx.MapType(typeof(SubItemToken))))
            using (Compiler.Local field = new Compiler.Local(ctx, ctx.MapType(typeof(int))))
            {
                ctx.LoadReaderWriter();
                ctx.EmitCall(ctx.MapType(typeof(ProtoReader)).GetMethod("StartSubItem"));
                ctx.StoreValue(token);

                Compiler.CodeLabel next = ctx.DefineLabel(), processField = ctx.DefineLabel(), end = ctx.DefineLabel();

                ctx.MarkLabel(next);

                ctx.EmitBasicRead("ReadFieldHeader", ctx.MapType(typeof(int)));
                ctx.CopyValue();
                ctx.StoreValue(field);
                ctx.LoadValue(Tag); // = 1 - process
                ctx.BranchIfEqual(processField, true);
                ctx.LoadValue(field);
                ctx.LoadValue(1); // < 1 - exit
                ctx.BranchIfLess(end, false);

                // default: skip
                ctx.LoadReaderWriter();
                ctx.EmitCall(ctx.MapType(typeof(ProtoReader)).GetMethod("SkipField"));
                ctx.Branch(next, true);

                // process
                ctx.MarkLabel(processField);
                if (Tail.RequiresOldValue)
                {
                    if (Helpers.IsValueType(expectedType))
                    {
                        ctx.LoadAddress(oldValue, expectedType);
                        ctx.EmitCall(expectedType.GetMethod("GetValueOrDefault", Helpers.EmptyTypes));
                    }
                    else
                    {
                        ctx.LoadValue(oldValue);
                    }
                }
                Tail.EmitRead(ctx, null);
                // note we demanded always returns a value
                if (Helpers.IsValueType(expectedType))
                {
                    ctx.EmitCtor(expectedType, Tail.ExpectedType); // re-nullable<T> it
                }
                ctx.StoreValue(oldValue);
                ctx.Branch(next, false);

                // outro
                ctx.MarkLabel(end);
               
                ctx.LoadValue(token);
                ctx.LoadReaderWriter();
                ctx.EmitCall(ctx.MapType(typeof(ProtoReader)).GetMethod("EndSubItem"));
                ctx.LoadValue(oldValue); // load the old value
            }
        }
        protected override void EmitWrite(Compiler.CompilerContext ctx, Compiler.Local valueFrom)
        {
            using(Compiler.Local valOrNull = ctx.GetLocalWithValue(expectedType, valueFrom))
            using(Compiler.Local token = new Compiler.Local(ctx, ctx.MapType(typeof(SubItemToken))))
            {
                ctx.LoadNullRef();
                ctx.LoadReaderWriter();
                ctx.EmitCall(ctx.MapType(typeof(ProtoWriter)).GetMethod("StartSubItem"));
                ctx.StoreValue(token);
                
                if (Helpers.IsValueType(expectedType))
                {
                    ctx.LoadAddress(valOrNull, expectedType);
                    ctx.LoadValue(expectedType.GetProperty("HasValue"));
                }
                else
                {
                    ctx.LoadValue(valOrNull);
                }
                Compiler.CodeLabel @end = ctx.DefineLabel();
                ctx.BranchIfFalse(@end, false);
                if (Helpers.IsValueType(expectedType))
                {
                    ctx.LoadAddress(valOrNull, expectedType);
                    ctx.EmitCall(expectedType.GetMethod("GetValueOrDefault", Helpers.EmptyTypes));
                }
                else
                {
                    ctx.LoadValue(valOrNull);
                }
                Tail.EmitWrite(ctx, null);

                ctx.MarkLabel(@end);

                ctx.LoadValue(token);
                ctx.LoadReaderWriter();
                ctx.EmitCall(ctx.MapType(typeof(ProtoWriter)).GetMethod("EndSubItem"));
            }
        }
#endif

#if !FEAT_IKVM
        public override object Read(object value, ProtoReader source)
        {
            SubItemToken tok = ProtoReader.StartSubItem(source);
            int field;
            while((field = source.ReadFieldHeader()) > 0)
            {
                if(field == Tag) {
                    value = Tail.Read(value, source);
                } else {
                    source.SkipField();
                }
            }
            ProtoReader.EndSubItem(tok, source);
            return value;
        }
        public override void Write(object value, ProtoWriter dest)
        {
            SubItemToken token = ProtoWriter.StartSubItem(null, dest);
            if(value != null)
            {
                Tail.Write(value, dest);
            }
            ProtoWriter.EndSubItem(token, dest);
        }
#endif
    }
}
#endif
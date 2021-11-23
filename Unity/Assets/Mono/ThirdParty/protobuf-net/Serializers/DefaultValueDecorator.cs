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
    sealed class DefaultValueDecorator : ProtoDecoratorBase
    {

        public override Type ExpectedType { get { return Tail.ExpectedType; } }
        public override bool RequiresOldValue { get { return Tail.RequiresOldValue; } }
        public override bool ReturnsValue { get { return Tail.ReturnsValue; } }
        private readonly object defaultValue;
        public DefaultValueDecorator(TypeModel model, object defaultValue, IProtoSerializer tail) : base(tail)
        {
            if (defaultValue == null) throw new ArgumentNullException("defaultValue");
            Type type = model.MapType(defaultValue.GetType());
            if (type != tail.ExpectedType
#if FEAT_IKVM // in IKVM, we'll have the default value as an underlying type
                && !(tail.ExpectedType.IsEnum && type == tail.ExpectedType.GetEnumUnderlyingType())
#endif
                )
            {
                //ILRuntime enum 转int需要忽略
                if (!Helpers.IsEnum(tail.ExpectedType) && !(tail.ExpectedType is ILRuntime.Reflection.ILRuntimeType))
                {
                    throw new ArgumentException("Default value is of incorrect type", "defaultValue");
                }
            }
            this.defaultValue = defaultValue;
        }
#if !FEAT_IKVM
        public override void Write(object value, ProtoWriter dest)
        {
            if (!object.Equals(value, defaultValue))
            {
                Tail.Write(value, dest);
            }
        }
        public override object Read(object value, ProtoReader source)
        {
            return Tail.Read(value, source);
        }
#endif

#if FEAT_COMPILER
        protected override void EmitWrite(Compiler.CompilerContext ctx, Compiler.Local valueFrom)
        {
            Compiler.CodeLabel done = ctx.DefineLabel();
            if (valueFrom == null)
            {
                ctx.CopyValue(); // on the stack
                Compiler.CodeLabel needToPop = ctx.DefineLabel();
                EmitBranchIfDefaultValue(ctx, needToPop);
                Tail.EmitWrite(ctx, null);
                ctx.Branch(done, true);
                ctx.MarkLabel(needToPop);
                ctx.DiscardValue();
            }
            else
            {
                ctx.LoadValue(valueFrom); // variable/parameter
                EmitBranchIfDefaultValue(ctx, done);
                Tail.EmitWrite(ctx, valueFrom);
            }
            ctx.MarkLabel(done);
        }
        private void EmitBeq(Compiler.CompilerContext ctx, Compiler.CodeLabel label, Type type)
        {
            switch (Helpers.GetTypeCode(type))
            {
                case ProtoTypeCode.Boolean:
                case ProtoTypeCode.Byte:
                case ProtoTypeCode.Char:
                case ProtoTypeCode.Double:
                case ProtoTypeCode.Int16:
                case ProtoTypeCode.Int32:
                case ProtoTypeCode.Int64:
                case ProtoTypeCode.SByte:
                case ProtoTypeCode.Single:
                case ProtoTypeCode.UInt16:
                case ProtoTypeCode.UInt32:
                case ProtoTypeCode.UInt64:
                    ctx.BranchIfEqual(label, false);
                    break;
                default:
#if COREFX
                    MethodInfo method = type.GetMethod("op_Equality", new Type[] { type, type });
                    if (method == null || !method.IsPublic || !method.IsStatic) method = null;
#else
                    MethodInfo method = type.GetMethod("op_Equality", BindingFlags.Public | BindingFlags.Static,
                        null, new Type[] { type, type}, null);
#endif
                    if (method == null || method.ReturnType != ctx.MapType(typeof(bool)))
                    {
                        throw new InvalidOperationException("No suitable equality operator found for default-values of type: " + type.FullName);
                    }
                    ctx.EmitCall(method);
                    ctx.BranchIfTrue(label, false);
                    break;

            }
        }
        private void EmitBranchIfDefaultValue(Compiler.CompilerContext ctx, Compiler.CodeLabel label)
        {
            Type expected = ExpectedType;
            switch (Helpers.GetTypeCode(expected))
            {
                case ProtoTypeCode.Boolean:
                    if ((bool)defaultValue)
                    {
                        ctx.BranchIfTrue(label, false);
                    }
                    else
                    {
                        ctx.BranchIfFalse(label, false);
                    }
                    break;
                case ProtoTypeCode.Byte:
                    if ((byte)defaultValue == (byte)0)
                    {
                        ctx.BranchIfFalse(label, false);
                    }
                    else
                    {
                        ctx.LoadValue((int)(byte)defaultValue);
                        EmitBeq(ctx, label, expected);
                    }
                    break;
                case ProtoTypeCode.SByte:
                    if ((sbyte)defaultValue == (sbyte)0)
                    {
                        ctx.BranchIfFalse(label, false);
                    }
                    else
                    {
                        ctx.LoadValue((int)(sbyte)defaultValue);
                        EmitBeq(ctx, label, expected);
                    }
                    break;
                case ProtoTypeCode.Int16:
                    if ((short)defaultValue == (short)0)
                    {
                        ctx.BranchIfFalse(label, false);
                    }
                    else
                    {
                        ctx.LoadValue((int)(short)defaultValue);
                        EmitBeq(ctx, label, expected);
                    }
                    break;
                case ProtoTypeCode.UInt16:
                    if ((ushort)defaultValue == (ushort)0)
                    {
                        ctx.BranchIfFalse(label, false);
                    }
                    else
                    {
                        ctx.LoadValue((int)(ushort)defaultValue);
                        EmitBeq(ctx, label, expected);
                    }
                    break;
                case ProtoTypeCode.Int32:
                    if ((int)defaultValue == (int)0)
                    {
                        ctx.BranchIfFalse(label, false);
                    }
                    else
                    {
                        ctx.LoadValue((int)defaultValue);
                        EmitBeq(ctx, label, expected);
                    }
                    break;
                case ProtoTypeCode.UInt32:
                    if ((uint)defaultValue == (uint)0)
                    {
                        ctx.BranchIfFalse(label, false);
                    }
                    else
                    {
                        ctx.LoadValue((int)(uint)defaultValue);
                        EmitBeq(ctx, label, expected);
                    }
                    break;
                case ProtoTypeCode.Char:
                    if ((char)defaultValue == (char)0)
                    {
                        ctx.BranchIfFalse(label, false);
                    }
                    else
                    {
                        ctx.LoadValue((int)(char)defaultValue);
                        EmitBeq(ctx, label, expected);
                    }
                    break;
                case ProtoTypeCode.Int64:
                    ctx.LoadValue((long)defaultValue);
                    EmitBeq(ctx, label, expected);
                    break;
                case ProtoTypeCode.UInt64:
                    ctx.LoadValue((long)(ulong)defaultValue);
                    EmitBeq(ctx, label, expected);
                    break;
                case ProtoTypeCode.Double:
                    ctx.LoadValue((double)defaultValue);
                    EmitBeq(ctx, label, expected);
                    break;
                case ProtoTypeCode.Single:
                    ctx.LoadValue((float)defaultValue);
                    EmitBeq(ctx, label, expected);
                    break;
                case ProtoTypeCode.String:
                    ctx.LoadValue((string)defaultValue);
                    EmitBeq(ctx, label, expected);
                    break;
                case ProtoTypeCode.Decimal:
                    {
                        decimal d = (decimal)defaultValue;
                        ctx.LoadValue(d);
                        EmitBeq(ctx, label, expected);
                    }
                    break;
                case ProtoTypeCode.TimeSpan:
                    {
                        TimeSpan ts = (TimeSpan)defaultValue;
                        if (ts == TimeSpan.Zero)
                        {
                            ctx.LoadValue(typeof(TimeSpan).GetField("Zero"));
                        }
                        else
                        {
                            ctx.LoadValue(ts.Ticks);
                            ctx.EmitCall(ctx.MapType(typeof(TimeSpan)).GetMethod("FromTicks"));
                        }
                        EmitBeq(ctx, label, expected);
                        break;
                    }
                case ProtoTypeCode.Guid:
                    {
                        ctx.LoadValue((Guid)defaultValue);
                        EmitBeq(ctx, label, expected);
                        break;
                    }
                case ProtoTypeCode.DateTime:
                    {
#if FX11
                        ctx.LoadValue(((DateTime)defaultValue).ToFileTime());
                        ctx.EmitCall(typeof(DateTime).GetMethod("FromFileTime"));                      
#else
                        ctx.LoadValue(((DateTime)defaultValue).ToBinary());
                        ctx.EmitCall(ctx.MapType(typeof(DateTime)).GetMethod("FromBinary"));
#endif

                        EmitBeq(ctx, label, expected);
                        break;
                    }
                default:
                    throw new NotSupportedException("Type cannot be represented as a default value: " + expected.FullName);
            }
        }
        protected override void EmitRead(Compiler.CompilerContext ctx, Compiler.Local valueFrom)
        {
            Tail.EmitRead(ctx, valueFrom);
        }
#endif
            }
}
#endif
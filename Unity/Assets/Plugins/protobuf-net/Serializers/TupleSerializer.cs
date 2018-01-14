#if !NO_RUNTIME
using System;
using ProtoBuf.Meta;

#if FEAT_IKVM
using Type = IKVM.Reflection.Type;
using IKVM.Reflection;
#else
using System.Reflection;
#endif

namespace ProtoBuf.Serializers
{
    sealed class TupleSerializer : IProtoTypeSerializer
    {
        private readonly MemberInfo[] members;
        private readonly ConstructorInfo ctor;
        private IProtoSerializer[] tails;
        public TupleSerializer(RuntimeTypeModel model, ConstructorInfo ctor, MemberInfo[] members)
        {
            if (ctor == null) throw new ArgumentNullException("ctor");
            if (members == null) throw new ArgumentNullException("members");
            this.ctor = ctor;
            this.members = members;
            this.tails = new IProtoSerializer[members.Length];

            ParameterInfo[] parameters = ctor.GetParameters();
            for(int i = 0 ; i < members.Length ; i++)
            {
                WireType wireType;
                Type finalType = parameters[i].ParameterType;

                Type itemType = null, defaultType = null;

                MetaType.ResolveListTypes(model, finalType, ref itemType, ref defaultType);
                Type tmp = itemType == null ? finalType : itemType;

                bool asReference = false;
                int typeIndex = model.FindOrAddAuto(tmp, false, true, false);
                if (typeIndex >= 0)
                {
                    asReference = model[tmp].AsReferenceDefault;
                }
                IProtoSerializer tail = ValueMember.TryGetCoreSerializer(model, DataFormat.Default, tmp, out wireType, asReference, false, false, true), serializer;
                if (tail == null)
                {
                    throw new InvalidOperationException("No serializer defined for type: " + tmp.FullName);
                }

                tail = new TagDecorator(i + 1, wireType, false, tail);
                if(itemType == null)
                {
                    serializer = tail;
                }
                else
                {
                    if (finalType.IsArray)
                    {
                        serializer = new ArrayDecorator(model, tail, i + 1, false, wireType, finalType, false, false);
                    }
                    else
                    {
                        serializer = ListDecorator.Create(model, finalType, defaultType, tail, i + 1, false, wireType, true, false, false);
                    }
                }
                tails[i] = serializer;
            }
        }
        public bool HasCallbacks(Meta.TypeModel.CallbackType callbackType)
        {
            return false;
        }

#if FEAT_COMPILER
        public void EmitCallback(Compiler.CompilerContext ctx, Compiler.Local valueFrom, Meta.TypeModel.CallbackType callbackType){}
#endif
        public Type ExpectedType
        {
            get { return ctor.DeclaringType; }
        }


        
#if !FEAT_IKVM
        void IProtoTypeSerializer.Callback(object value, Meta.TypeModel.CallbackType callbackType, SerializationContext context) { }
        object IProtoTypeSerializer.CreateInstance(ProtoReader source) { throw new NotSupportedException(); }
        private object GetValue(object obj, int index)
        {
            PropertyInfo prop;
            FieldInfo field;
            
            if ((prop = members[index] as PropertyInfo) != null)
            {
                if (obj == null)
                    return Helpers.IsValueType(prop.PropertyType) ? Activator.CreateInstance(prop.PropertyType) : null;
                return prop.GetValue(obj, null);
            }
            else if ((field = members[index] as FieldInfo) != null)
            {
                if (obj == null)
                    return Helpers.IsValueType(field.FieldType) ? Activator.CreateInstance(field.FieldType) : null;
                return field.GetValue(obj);
            }
            else
            {
                throw new InvalidOperationException();
            }          
        }
        public object Read(object value, ProtoReader source)
        {
            object[] values = new object[members.Length];
            bool invokeCtor = false;
            if (value == null)
            {
                invokeCtor = true;
            }
            for (int i = 0; i < values.Length; i++)
                    values[i] = GetValue(value, i);
            int field;
            while((field = source.ReadFieldHeader()) > 0)
            {
                invokeCtor = true;
                if(field <= tails.Length)
                {
                    IProtoSerializer tail = tails[field - 1];
                    values[field - 1] = tails[field - 1].Read(tail.RequiresOldValue ? values[field - 1] : null, source);
                }
                else
                {
                    source.SkipField();
                }
            }
            return invokeCtor ? ctor.Invoke(values) : value;
        }
        public void Write(object value, ProtoWriter dest)
        {
            for (int i = 0; i < tails.Length; i++)
            {
                object val = GetValue(value, i);
                if (val != null) tails[i].Write(val, dest);
            }
        }
#endif
        public bool RequiresOldValue
        {
            get { return true; }
        }

        public bool ReturnsValue
        {
            get { return false; }
        }
        Type GetMemberType(int index)
        {
            Type result = Helpers.GetMemberType(members[index]);
            if (result == null) throw new InvalidOperationException();
            return result;
        }
        bool IProtoTypeSerializer.CanCreateInstance() { return false; }

#if FEAT_COMPILER
        public void EmitWrite(Compiler.CompilerContext ctx, Compiler.Local valueFrom)
        {
            using (Compiler.Local loc = ctx.GetLocalWithValue(ctor.DeclaringType, valueFrom))
            {
                for (int i = 0; i < tails.Length; i++)
                {
                    Type type = GetMemberType(i);
                    ctx.LoadAddress(loc, ExpectedType);
                    switch(members[i].MemberType)
                    {
                        case MemberTypes.Field:
                            ctx.LoadValue((FieldInfo)members[i]);
                            break;
                        case MemberTypes.Property:
                            ctx.LoadValue((PropertyInfo)members[i]);
                            break;
                    }
                    ctx.WriteNullCheckedTail(type, tails[i], null);
                }
            }
        }

        void IProtoTypeSerializer.EmitCreateInstance(Compiler.CompilerContext ctx) { throw new NotSupportedException(); }

        public void EmitRead(Compiler.CompilerContext ctx, Compiler.Local incoming)
        {
            using (Compiler.Local objValue = ctx.GetLocalWithValue(ExpectedType, incoming))
            {
                Compiler.Local[] locals = new Compiler.Local[members.Length];
                try
                {
                    for (int i = 0; i < locals.Length; i++)
                    {
                        Type type = GetMemberType(i);
                        bool store = true;
                        locals[i] = new Compiler.Local(ctx, type);
                        if (!ExpectedType.IsValueType)
                        {
                            // value-types always read the old value
                            if (type.IsValueType)
                            {
                                switch (Helpers.GetTypeCode(type))
                                {
                                    case ProtoTypeCode.Boolean:
                                    case ProtoTypeCode.Byte:
                                    case ProtoTypeCode.Int16:
                                    case ProtoTypeCode.Int32:
                                    case ProtoTypeCode.SByte:
                                    case ProtoTypeCode.UInt16:
                                    case ProtoTypeCode.UInt32:
                                        ctx.LoadValue(0);
                                        break;
                                    case ProtoTypeCode.Int64:
                                    case ProtoTypeCode.UInt64:
                                        ctx.LoadValue(0L);
                                        break;
                                    case ProtoTypeCode.Single:
                                        ctx.LoadValue(0.0F);
                                        break;
                                    case ProtoTypeCode.Double:
                                        ctx.LoadValue(0.0D);
                                        break;
                                    case ProtoTypeCode.Decimal:
                                        ctx.LoadValue(0M);
                                        break;
                                    case ProtoTypeCode.Guid:
                                        ctx.LoadValue(Guid.Empty);
                                        break;
                                    default:
                                        ctx.LoadAddress(locals[i], type);
                                        ctx.EmitCtor(type);
                                        store = false;
                                        break;
                                }
                            }
                            else
                            {
                                ctx.LoadNullRef();
                            }
                            if (store)
                            {
                                ctx.StoreValue(locals[i]);
                            }
                        }
                    }

                    Compiler.CodeLabel skipOld = ExpectedType.IsValueType
                                                        ? new Compiler.CodeLabel()
                                                        : ctx.DefineLabel();
                    if (!ExpectedType.IsValueType)
                    {
                        ctx.LoadAddress(objValue, ExpectedType);
                        ctx.BranchIfFalse(skipOld, false);
                    }
                    for (int i = 0; i < members.Length; i++)
                    {
                        ctx.LoadAddress(objValue, ExpectedType);
                        switch (members[i].MemberType)
                        {
                            case MemberTypes.Field:
                                ctx.LoadValue((FieldInfo) members[i]);
                                break;
                            case MemberTypes.Property:
                                ctx.LoadValue((PropertyInfo) members[i]);
                                break;
                        }
                        ctx.StoreValue(locals[i]);
                    }

                    if (!ExpectedType.IsValueType) ctx.MarkLabel(skipOld);

                    using (Compiler.Local fieldNumber = new Compiler.Local(ctx, ctx.MapType(typeof (int))))
                    {
                        Compiler.CodeLabel @continue = ctx.DefineLabel(),
                                           processField = ctx.DefineLabel(),
                                           notRecognised = ctx.DefineLabel();
                        ctx.Branch(@continue, false);

                        Compiler.CodeLabel[] handlers = new Compiler.CodeLabel[members.Length];
                        for (int i = 0; i < members.Length; i++)
                        {
                            handlers[i] = ctx.DefineLabel();
                        }

                        ctx.MarkLabel(processField);

                        ctx.LoadValue(fieldNumber);
                        ctx.LoadValue(1);
                        ctx.Subtract(); // jump-table is zero-based
                        ctx.Switch(handlers);

                        // and the default:
                        ctx.Branch(notRecognised, false);
                        for (int i = 0; i < handlers.Length; i++)
                        {
                            ctx.MarkLabel(handlers[i]);
                            IProtoSerializer tail = tails[i];
                            Compiler.Local oldValIfNeeded = tail.RequiresOldValue ? locals[i] : null;
                            ctx.ReadNullCheckedTail(locals[i].Type, tail, oldValIfNeeded);
                            if (tail.ReturnsValue)
                            {
                                if (locals[i].Type.IsValueType)
                                {
                                    ctx.StoreValue(locals[i]);
                                }
                                else
                                {
                                    Compiler.CodeLabel hasValue = ctx.DefineLabel(), allDone = ctx.DefineLabel();

                                    ctx.CopyValue();
                                    ctx.BranchIfTrue(hasValue, true); // interpret null as "don't assign"
                                    ctx.DiscardValue();
                                    ctx.Branch(allDone, true);
                                    ctx.MarkLabel(hasValue);
                                    ctx.StoreValue(locals[i]);
                                    ctx.MarkLabel(allDone);
                                }
                            }
                            ctx.Branch(@continue, false);
                        }

                        ctx.MarkLabel(notRecognised);
                        ctx.LoadReaderWriter();
                        ctx.EmitCall(ctx.MapType(typeof (ProtoReader)).GetMethod("SkipField"));

                        ctx.MarkLabel(@continue);
                        ctx.EmitBasicRead("ReadFieldHeader", ctx.MapType(typeof (int)));
                        ctx.CopyValue();
                        ctx.StoreValue(fieldNumber);
                        ctx.LoadValue(0);
                        ctx.BranchIfGreater(processField, false);
                    }
                    for (int i = 0; i < locals.Length; i++)
                    {
                        ctx.LoadValue(locals[i]);
                    }

                    ctx.EmitCtor(ctor);
                    ctx.StoreValue(objValue);
                }
                finally
                {
                    for (int i = 0; i < locals.Length; i++)
                    {
                        if (locals[i] != null)
                            locals[i].Dispose(); // release for re-use
                    }
                }
            }

        }
#endif
    }
}

#endif
#if !NO_RUNTIME
using System;
using System.Collections;
using ProtoBuf.Meta;

#if FEAT_IKVM
using Type = IKVM.Reflection.Type;
using IKVM.Reflection;
#else
using System.Reflection;
#endif

namespace ProtoBuf.Serializers
{
    class ListDecorator : ProtoDecoratorBase
    {
        internal static bool CanPack(WireType wireType)
        {
            switch (wireType)
            {
                case WireType.Fixed32:
                case WireType.Fixed64:
                case WireType.SignedVariant:
                case WireType.Variant:
                    return true;
                default:
                    return false;
            }
        }
        private readonly byte options;

        private const byte OPTIONS_IsList = 1,
                           OPTIONS_SuppressIList = 2,
                           OPTIONS_WritePacked = 4,
                           OPTIONS_ReturnList = 8,
                           OPTIONS_OverwriteList = 16,
                           OPTIONS_SupportNull = 32;

        private readonly Type declaredType, concreteType;

        private readonly MethodInfo add;

        private readonly int fieldNumber;

        private bool IsList { get { return (options & OPTIONS_IsList) != 0; } }
        private bool SuppressIList { get { return (options & OPTIONS_SuppressIList) != 0; } }
        private bool WritePacked { get { return (options & OPTIONS_WritePacked) != 0; } }
        private bool SupportNull { get { return (options & OPTIONS_SupportNull) != 0; } }
        private bool ReturnList { get { return (options & OPTIONS_ReturnList) != 0; } }
        protected readonly WireType packedWireType;


        internal static ListDecorator Create(TypeModel model, Type declaredType, Type concreteType, IProtoSerializer tail, int fieldNumber, bool writePacked, WireType packedWireType, bool returnList, bool overwriteList, bool supportNull)
        {
#if !NO_GENERICS
            MethodInfo builderFactory, add, addRange, finish;
            if (returnList && ImmutableCollectionDecorator.IdentifyImmutable(model, declaredType, out builderFactory, out add, out addRange, out finish))
            {
                return new ImmutableCollectionDecorator(
                    model, declaredType, concreteType, tail, fieldNumber, writePacked, packedWireType, returnList, overwriteList, supportNull,
                    builderFactory, add, addRange, finish);
            }

#endif
            return new ListDecorator(model, declaredType, concreteType, tail, fieldNumber, writePacked, packedWireType, returnList, overwriteList, supportNull);
        }

        protected ListDecorator(TypeModel model, Type declaredType, Type concreteType, IProtoSerializer tail, int fieldNumber, bool writePacked, WireType packedWireType, bool returnList, bool overwriteList, bool supportNull)
            : base(tail)
        {
            if (returnList) options |= OPTIONS_ReturnList;
            if (overwriteList) options |= OPTIONS_OverwriteList;
            if (supportNull) options |= OPTIONS_SupportNull;
            if ((writePacked || packedWireType != WireType.None) && fieldNumber <= 0) throw new ArgumentOutOfRangeException("fieldNumber");
            if (!CanPack(packedWireType))
            {
                if (writePacked) throw new InvalidOperationException("Only simple data-types can use packed encoding");
                packedWireType = WireType.None;
            }            

            this.fieldNumber = fieldNumber;
            if (writePacked) options |= OPTIONS_WritePacked;
            this.packedWireType = packedWireType;
            if (declaredType == null) throw new ArgumentNullException("declaredType");
            if (declaredType.IsArray) throw new ArgumentException("Cannot treat arrays as lists", "declaredType");
            this.declaredType = declaredType;
            this.concreteType = concreteType;
            
            // look for a public list.Add(typedObject) method
            if (RequireAdd)
            {
                bool isList;
                add = TypeModel.ResolveListAdd(model, declaredType, tail.ExpectedType, out isList);
                if (isList)
                {
                    options |= OPTIONS_IsList;
                    string fullName = declaredType.FullName;
                    if (fullName != null && fullName.StartsWith("System.Data.Linq.EntitySet`1[["))
                    { // see http://stackoverflow.com/questions/6194639/entityset-is-there-a-sane-reason-that-ilist-add-doesnt-set-assigned
                        options |= OPTIONS_SuppressIList;
                    }
                }
                if (add == null) throw new InvalidOperationException("Unable to resolve a suitable Add method for " + declaredType.FullName);
            }

        }
        protected virtual bool RequireAdd { get { return true; } }

        public override Type ExpectedType { get { return declaredType;  } }
        public override bool RequiresOldValue { get { return AppendToCollection; } }
        public override bool ReturnsValue { get { return ReturnList; } }

        protected bool AppendToCollection
        {
            get { return (options & OPTIONS_OverwriteList) == 0; }
        }

#if FEAT_COMPILER
        protected override void EmitRead(ProtoBuf.Compiler.CompilerContext ctx, ProtoBuf.Compiler.Local valueFrom)
        {
            /* This looks more complex than it is. Look at the non-compiled Read to
             * see what it is trying to do, but note that it needs to cope with a
             * few more scenarios. Note that it picks the **most specific** Add,
             * unlike the runtime version that uses IList when possible. The core
             * is just a "do {list.Add(readValue())} while {thereIsMore}"
             * 
             * The complexity is due to:
             *  - value types vs reference types (boxing etc)
             *  - initialization if we need to pass in a value to the tail
             *  - handling whether or not the tail *returns* the value vs updates the input
             */
            bool returnList = ReturnList;
            
            using (Compiler.Local list = AppendToCollection ? ctx.GetLocalWithValue(ExpectedType, valueFrom) : new Compiler.Local(ctx, declaredType))
            using (Compiler.Local origlist = (returnList && AppendToCollection && !Helpers.IsValueType(ExpectedType)) ? new Compiler.Local(ctx, ExpectedType) : null)
            {
                if (!AppendToCollection)
                { // always new
                    ctx.LoadNullRef();
                    ctx.StoreValue(list);
                }
                else if (returnList && origlist != null)
                { // need a copy
                    ctx.LoadValue(list);
                    ctx.StoreValue(origlist);
                }
                if (concreteType != null)
                {
                    ctx.LoadValue(list);
                    Compiler.CodeLabel notNull = ctx.DefineLabel();
                    ctx.BranchIfTrue(notNull, true);
                    ctx.EmitCtor(concreteType);
                    ctx.StoreValue(list);
                    ctx.MarkLabel(notNull);
                }

                bool castListForAdd = !add.DeclaringType.IsAssignableFrom(declaredType);
                EmitReadList(ctx, list, Tail, add, packedWireType, castListForAdd);

                if (returnList)
                {
                    if (AppendToCollection && origlist != null)
                    {
                        // remember ^^^^ we had a spare copy of the list on the stack; now we'll compare
                        ctx.LoadValue(origlist);
                        ctx.LoadValue(list); // [orig] [new-value]
                        Compiler.CodeLabel sameList = ctx.DefineLabel(), allDone = ctx.DefineLabel();
                        ctx.BranchIfEqual(sameList, true);
                        ctx.LoadValue(list);
                        ctx.Branch(allDone, true);
                        ctx.MarkLabel(sameList);
                        ctx.LoadNullRef();
                        ctx.MarkLabel(allDone);
                    }
                    else
                    {
                        ctx.LoadValue(list);
                    }
                }
            }
        }

        internal static void EmitReadList(ProtoBuf.Compiler.CompilerContext ctx, Compiler.Local list, IProtoSerializer tail, MethodInfo add, WireType packedWireType, bool castListForAdd)
        {
            using (Compiler.Local fieldNumber = new Compiler.Local(ctx, ctx.MapType(typeof(int))))
            {
                Compiler.CodeLabel readPacked = packedWireType == WireType.None ? new Compiler.CodeLabel() : ctx.DefineLabel();                                   
                if (packedWireType != WireType.None)
                {
                    ctx.LoadReaderWriter();
                    ctx.LoadValue(typeof(ProtoReader).GetProperty("WireType"));
                    ctx.LoadValue((int)WireType.String);
                    ctx.BranchIfEqual(readPacked, false);
                }
                ctx.LoadReaderWriter();
                ctx.LoadValue(typeof(ProtoReader).GetProperty("FieldNumber"));
                ctx.StoreValue(fieldNumber);

                Compiler.CodeLabel @continue = ctx.DefineLabel();
                ctx.MarkLabel(@continue);

                EmitReadAndAddItem(ctx, list, tail, add, castListForAdd);

                ctx.LoadReaderWriter();
                ctx.LoadValue(fieldNumber);
                ctx.EmitCall(ctx.MapType(typeof(ProtoReader)).GetMethod("TryReadFieldHeader"));
                ctx.BranchIfTrue(@continue, false);

                if (packedWireType != WireType.None)
                {
                    Compiler.CodeLabel allDone = ctx.DefineLabel();
                    ctx.Branch(allDone, false);
                    ctx.MarkLabel(readPacked);

                    ctx.LoadReaderWriter();
                    ctx.EmitCall(ctx.MapType(typeof(ProtoReader)).GetMethod("StartSubItem"));

                    Compiler.CodeLabel testForData = ctx.DefineLabel(), noMoreData = ctx.DefineLabel();
                    ctx.MarkLabel(testForData);
                    ctx.LoadValue((int)packedWireType);
                    ctx.LoadReaderWriter();
                    ctx.EmitCall(ctx.MapType(typeof(ProtoReader)).GetMethod("HasSubValue"));
                    ctx.BranchIfFalse(noMoreData, false);

                    EmitReadAndAddItem(ctx, list, tail, add, castListForAdd);
                    ctx.Branch(testForData, false);

                    ctx.MarkLabel(noMoreData);
                    ctx.LoadReaderWriter();
                    ctx.EmitCall(ctx.MapType(typeof(ProtoReader)).GetMethod("EndSubItem"));
                    ctx.MarkLabel(allDone);
                }


                
            }
        }

        private static void EmitReadAndAddItem(Compiler.CompilerContext ctx, Compiler.Local list, IProtoSerializer tail, MethodInfo add, bool castListForAdd)
        {
            ctx.LoadAddress(list, list.Type); // needs to be the reference in case the list is value-type (static-call)
            if (castListForAdd) ctx.Cast(add.DeclaringType);

            Type itemType = tail.ExpectedType;
            bool tailReturnsValue = tail.ReturnsValue;
            if (tail.RequiresOldValue)
            {
                if (Helpers.IsValueType(itemType) || !tailReturnsValue)
                {
                    // going to need a variable
                    using (Compiler.Local item = new Compiler.Local(ctx, itemType))
                    {
                        if (Helpers.IsValueType(itemType))
                        {   // initialise the struct
                            ctx.LoadAddress(item, itemType);
                            ctx.EmitCtor(itemType);
                        }
                        else
                        {   // assign null
                            ctx.LoadNullRef();
                            ctx.StoreValue(item);
                        }
                        tail.EmitRead(ctx, item);
                        if (!tailReturnsValue) { ctx.LoadValue(item); }
                    }
                }
                else
                {    // no variable; pass the null on the stack and take the value *off* the stack
                    ctx.LoadNullRef();
                    tail.EmitRead(ctx, null);
                }
            }
            else
            {
                if (tailReturnsValue)
                {   // out only (on the stack); just emit it
                    tail.EmitRead(ctx, null);
                }
                else
                {   // doesn't take anything in nor return anything! WTF?
                    throw new InvalidOperationException();
                }
            }
            // our "Add" is chosen either to take the correct type, or to take "object";
            // we may need to box the value
                
            Type addParamType = add.GetParameters()[0].ParameterType;
            if(addParamType != itemType) {
                if (addParamType == ctx.MapType(typeof(object)))
                {
                    ctx.CastToObject(itemType);
                }
#if !NO_GENERICS
                else if(Helpers.GetUnderlyingType(addParamType) == itemType)
                { // list is nullable
                    ConstructorInfo ctor = Helpers.GetConstructor(addParamType, new Type[] {itemType}, false);
                    ctx.EmitCtor(ctor); // the itemType on the stack is now a Nullable<ItemType>
                }
#endif
                else
                {
                    throw new InvalidOperationException("Conflicting item/add type");
                }
            }
            ctx.EmitCall(add, list.Type);
            if (add.ReturnType != ctx.MapType(typeof(void)))
            {
                ctx.DiscardValue();
            }
        }
#endif

#if WINRT || COREFX
        private static readonly TypeInfo ienumeratorType = typeof(IEnumerator).GetTypeInfo(), ienumerableType = typeof (IEnumerable).GetTypeInfo();
#else
        private static readonly System.Type ienumeratorType = typeof (IEnumerator), ienumerableType = typeof (IEnumerable);
#endif
        protected MethodInfo GetEnumeratorInfo(TypeModel model, out MethodInfo moveNext, out MethodInfo current)
            => GetEnumeratorInfo(model, ExpectedType, Tail.ExpectedType, out moveNext, out current);
        internal static MethodInfo GetEnumeratorInfo(TypeModel model, Type expectedType, Type itemType, out MethodInfo moveNext, out MethodInfo current)
        {

#if WINRT || COREFX
            TypeInfo enumeratorType = null, iteratorType;
#else
            Type enumeratorType = null, iteratorType;
#endif

            // try a custom enumerator
            MethodInfo getEnumerator = Helpers.GetInstanceMethod(expectedType, "GetEnumerator", null);

            Type getReturnType = null;
            if (getEnumerator != null)
            {
                getReturnType = getEnumerator.ReturnType;
                iteratorType = getReturnType
#if WINRT || COREFX || COREFX
                    .GetTypeInfo()
#endif
                    ;
                moveNext = Helpers.GetInstanceMethod(iteratorType, "MoveNext", null);
                PropertyInfo prop = Helpers.GetProperty(iteratorType, "Current", false);
                current = prop == null ? null : Helpers.GetGetMethod(prop, false, false);
                if (moveNext == null && (model.MapType(ienumeratorType).IsAssignableFrom(iteratorType)))
                {
                    moveNext = Helpers.GetInstanceMethod(model.MapType(ienumeratorType), "MoveNext", null);
                }
                // fully typed
                if (moveNext != null && moveNext.ReturnType == model.MapType(typeof(bool))
                    && current != null && current.ReturnType == itemType)
                {
                    return getEnumerator;
                }
                moveNext = current = getEnumerator = null;
            }
            
#if !NO_GENERICS
            // try IEnumerable<T>
            Type tmp = model.MapType(typeof(System.Collections.Generic.IEnumerable<>), false);
            
            if (tmp != null)
            {
                tmp = tmp.MakeGenericType(itemType);

#if WINRT || COREFX
                enumeratorType = tmp.GetTypeInfo();
#else
                enumeratorType = tmp;
#endif
            }
;
            if (enumeratorType != null && enumeratorType.IsAssignableFrom(expectedType
#if WINRT || COREFX
                .GetTypeInfo()
#endif
                ))
            {
                getEnumerator = Helpers.GetInstanceMethod(enumeratorType, "GetEnumerator");
                getReturnType = getEnumerator.ReturnType;

#if WINRT || COREFX
                iteratorType = getReturnType.GetTypeInfo();
#else
                iteratorType = getReturnType;
#endif

                moveNext = Helpers.GetInstanceMethod(model.MapType(ienumeratorType), "MoveNext");
                current = Helpers.GetGetMethod(Helpers.GetProperty(iteratorType, "Current", false), false, false);
                return getEnumerator;
            }
#endif
            // give up and fall-back to non-generic IEnumerable
            enumeratorType = model.MapType(ienumerableType);
            getEnumerator = Helpers.GetInstanceMethod(enumeratorType, "GetEnumerator");
            getReturnType = getEnumerator.ReturnType;
            iteratorType = getReturnType
#if WINRT || COREFX
                .GetTypeInfo()
#endif
                ;
            moveNext = Helpers.GetInstanceMethod(iteratorType, "MoveNext");
            current = Helpers.GetGetMethod(Helpers.GetProperty(iteratorType,"Current", false), false, false);
            return getEnumerator;
        }
#if FEAT_COMPILER
        protected override void EmitWrite(ProtoBuf.Compiler.CompilerContext ctx, ProtoBuf.Compiler.Local valueFrom)
        {
            using (Compiler.Local list = ctx.GetLocalWithValue(ExpectedType, valueFrom))
            {
                MethodInfo moveNext, current, getEnumerator = GetEnumeratorInfo(ctx.Model, out moveNext, out current);
                Helpers.DebugAssert(moveNext != null);
                Helpers.DebugAssert(current != null);
                Helpers.DebugAssert(getEnumerator != null);
                Type enumeratorType = getEnumerator.ReturnType;
                bool writePacked = WritePacked;
                using (Compiler.Local iter = new Compiler.Local(ctx, enumeratorType))
                using (Compiler.Local token = writePacked ? new Compiler.Local(ctx, ctx.MapType(typeof(SubItemToken))) : null)
                {
                    if (writePacked)
                    {
                        ctx.LoadValue(fieldNumber);
                        ctx.LoadValue((int)WireType.String);
                        ctx.LoadReaderWriter();
                        ctx.EmitCall(ctx.MapType(typeof(ProtoWriter)).GetMethod("WriteFieldHeader"));

                        ctx.LoadValue(list);
                        ctx.LoadReaderWriter();
                        ctx.EmitCall(ctx.MapType(typeof(ProtoWriter)).GetMethod("StartSubItem"));
                        ctx.StoreValue(token);

                        ctx.LoadValue(fieldNumber);
                        ctx.LoadReaderWriter();
                        ctx.EmitCall(ctx.MapType(typeof(ProtoWriter)).GetMethod("SetPackedField"));
                    }

                    ctx.LoadAddress(list, ExpectedType);
                    ctx.EmitCall(getEnumerator, ExpectedType);
                    ctx.StoreValue(iter);
                    using (ctx.Using(iter))
                    {
                        Compiler.CodeLabel body = ctx.DefineLabel(), next = ctx.DefineLabel();
                        ctx.Branch(next, false);
                        
                        ctx.MarkLabel(body);

                        ctx.LoadAddress(iter, enumeratorType);
                        ctx.EmitCall(current, enumeratorType);
                        Type itemType = Tail.ExpectedType;
                        if (itemType != ctx.MapType(typeof(object)) && current.ReturnType == ctx.MapType(typeof(object)))
                        {
                            ctx.CastFromObject(itemType);
                        }
                        Tail.EmitWrite(ctx, null);

                        ctx.MarkLabel(@next);
                        ctx.LoadAddress(iter, enumeratorType);
                        ctx.EmitCall(moveNext, enumeratorType);
                        ctx.BranchIfTrue(body, false);
                    }

                    if (writePacked)
                    {
                        ctx.LoadValue(token);
                        ctx.LoadReaderWriter();
                        ctx.EmitCall(ctx.MapType(typeof(ProtoWriter)).GetMethod("EndSubItem"));
                    }                    
                }
            }
        }
#endif

#if !FEAT_IKVM
        public override void Write(object value, ProtoWriter dest)
        {
            SubItemToken token;
            bool writePacked = WritePacked;
            bool fixedSizePacked = writePacked & CanUsePackedPrefix(value) && value is ICollection;
            if (writePacked)
            {
                ProtoWriter.WriteFieldHeader(fieldNumber, WireType.String, dest);
                if (fixedSizePacked)
                {
                    ProtoWriter.WritePackedPrefix(((ICollection)value).Count, packedWireType, dest);
                    token = default(SubItemToken);
                }
                else
                {
                    token = ProtoWriter.StartSubItem(value, dest);
                }
                ProtoWriter.SetPackedField(fieldNumber, dest);
            }
            else
            {
                token = new SubItemToken(); // default
            }
            bool checkForNull = !SupportNull;
            foreach (object subItem in (IEnumerable)value)
            {
                if (checkForNull && subItem == null) { throw new NullReferenceException(); }
                Tail.Write(subItem, dest);
            }
            if (writePacked)
            {
                if (fixedSizePacked)
                {
                    ProtoWriter.ClearPackedField(fieldNumber, dest);
                }
                else
                {
                    ProtoWriter.EndSubItem(token, dest);
                }
            }
        }

		private bool CanUsePackedPrefix(object obj) {
			return ArrayDecorator.CanUsePackedPrefix (packedWireType, Tail.ExpectedType);
		}

        public override object Read(object value, ProtoReader source)
        {
            try
            {
                int field = source.FieldNumber;
                object origValue = value;
                if (value == null) value = Activator.CreateInstance(concreteType);
                bool isList = IsList && !SuppressIList;
                if (packedWireType != WireType.None && source.WireType == WireType.String)
                {
                    SubItemToken token = ProtoReader.StartSubItem(source);
                    if (isList)
                    {
                        IList list = (IList)value;
                        while (ProtoReader.HasSubValue(packedWireType, source))
                        {
                            list.Add(Tail.Read(null, source));
                        }
                    }
                    else
                    {
                        object[] args = new object[1];
                        while (ProtoReader.HasSubValue(packedWireType, source))
                        {
                            args[0] = Tail.Read(null, source);
                            add.Invoke(value, args);
                        }
                    }
                    ProtoReader.EndSubItem(token, source);
                }
                else
                {
                    if (isList)
                    {
                        IList list = (IList)value;
                        do
                        {
                            list.Add(Tail.Read(null, source));
                        } while (source.TryReadFieldHeader(field));
                    }
                    else
                    {
                        object[] args = new object[1];
                        do
                        {
                            args[0] = Tail.Read(null, source);
                            add.Invoke(value, args);
                        } while (source.TryReadFieldHeader(field));
                    }
                }
                return origValue == value ? null : value;
            } catch(TargetInvocationException tie)
            {
                if (tie.InnerException != null) throw tie.InnerException;
                throw;
            }
        }
#endif

    }
}
#endif
#if FEAT_COMPILER
//#define DEBUG_COMPILE
using System;
using System.Threading;
using ProtoBuf.Meta;
using ProtoBuf.Serializers;

#if FEAT_IKVM
using Type = IKVM.Reflection.Type;
using IKVM.Reflection;
using IKVM.Reflection.Emit;
#else
using System.Reflection;
using System.Reflection.Emit;
#endif


namespace ProtoBuf.Compiler
{
    internal struct CodeLabel
    {
        public readonly Label Value;
        public readonly int Index;
        public CodeLabel(Label value, int index)
        {
            this.Value = value;
            this.Index = index;
        }
    }
    internal sealed class CompilerContext
    {
        public TypeModel Model { get { return model; } }

#if !(FX11 || FEAT_IKVM)
        readonly DynamicMethod method;
        static int next;
#endif

        internal CodeLabel DefineLabel()
        {
            CodeLabel result = new CodeLabel(il.DefineLabel(), nextLabel++);
            return result;
        }
#if DEBUG_COMPILE
        static readonly string traceCompilePath;
        static CompilerContext()
        {
            traceCompilePath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(),
                "TraceCompile.txt");
            Console.WriteLine("DEBUG_COMPILE enabled; writing to " + traceCompilePath);
        }
#endif
        [System.Diagnostics.Conditional("DEBUG_COMPILE")]
        private void TraceCompile(string value)
        {
#if DEBUG_COMPILE
            if (!string.IsNullOrWhiteSpace(value))
            {
                using (System.IO.StreamWriter sw = System.IO.File.AppendText(traceCompilePath))
                {
                    sw.WriteLine(value);
                }
            }
#endif
        }
        internal void MarkLabel(CodeLabel label)
        {
            il.MarkLabel(label.Value);
            TraceCompile("#: " + label.Index);
        }

#if !(FX11 || FEAT_IKVM)
        public static ProtoSerializer BuildSerializer(IProtoSerializer head, TypeModel model)
        {
            Type type = head.ExpectedType;
            try
            {
                CompilerContext ctx = new CompilerContext(type, true, true, model, typeof(object));
                ctx.LoadValue(ctx.InputValue);
                ctx.CastFromObject(type);
                ctx.WriteNullCheckedTail(type, head, null);
                ctx.Emit(OpCodes.Ret);
                return (ProtoSerializer)ctx.method.CreateDelegate(
                    typeof(ProtoSerializer));
            }
            catch (Exception ex)
            {
                string name = type.FullName;
                if(string.IsNullOrEmpty(name)) name = type.Name;
                throw new InvalidOperationException("It was not possible to prepare a serializer for: " + name, ex);
            }
        }
        /*public static ProtoCallback BuildCallback(IProtoTypeSerializer head)
        {
            Type type = head.ExpectedType;
            CompilerContext ctx = new CompilerContext(type, true, true);
            using (Local typedVal = new Local(ctx, type))
            {
                ctx.LoadValue(Local.InputValue);
                ctx.CastFromObject(type);
                ctx.StoreValue(typedVal);
                CodeLabel[] jumpTable = new CodeLabel[4];
                for(int i = 0 ; i < jumpTable.Length ; i++) {
                    jumpTable[i] = ctx.DefineLabel();
                }
                ctx.LoadReaderWriter();
                ctx.Switch(jumpTable);
                ctx.Return();
                for(int i = 0 ; i < jumpTable.Length ; i++) {
                    ctx.MarkLabel(jumpTable[i]);
                    if (head.HasCallbacks((TypeModel.CallbackType)i))
                    {
                        head.EmitCallback(ctx, typedVal, (TypeModel.CallbackType)i);
                    }
                    ctx.Return();
                }                
            }
            
            ctx.Emit(OpCodes.Ret);
            return (ProtoCallback)ctx.method.CreateDelegate(
                typeof(ProtoCallback));
        }*/
        public static ProtoDeserializer BuildDeserializer(IProtoSerializer head, TypeModel model)
        {
            Type type = head.ExpectedType;
            CompilerContext ctx = new CompilerContext(type, false, true, model, typeof(object));
            
            using (Local typedVal = new Local(ctx, type))
            {
                if (!Helpers.IsValueType(type))
                {
                    ctx.LoadValue(ctx.InputValue);
                    ctx.CastFromObject(type);
                    ctx.StoreValue(typedVal);
                }
                else
                {   
                    ctx.LoadValue(ctx.InputValue);
                    CodeLabel notNull = ctx.DefineLabel(), endNull = ctx.DefineLabel();
                    ctx.BranchIfTrue(notNull, true);

                    ctx.LoadAddress(typedVal, type);
                    ctx.EmitCtor(type);
                    ctx.Branch(endNull, true);

                    ctx.MarkLabel(notNull);
                    ctx.LoadValue(ctx.InputValue);
                    ctx.CastFromObject(type);
                    ctx.StoreValue(typedVal);

                    ctx.MarkLabel(endNull);
                }
                head.EmitRead(ctx, typedVal);

                if (head.ReturnsValue) {
                    ctx.StoreValue(typedVal);
                }

                ctx.LoadValue(typedVal);
                ctx.CastToObject(type);
            }
            ctx.Emit(OpCodes.Ret);
            return (ProtoDeserializer)ctx.method.CreateDelegate(
                typeof(ProtoDeserializer));
        }
#endif
        internal void Return()
        {
            Emit(OpCodes.Ret);
        }

        static bool IsObject(Type type)
        {
#if FEAT_IKVM
            return type.FullName == "System.Object";
#else
            return type == typeof(object);
#endif
        }
        internal void CastToObject(Type type)
        {
            if(IsObject(type))
            { }
            else if (Helpers.IsValueType(type))
            {
                il.Emit(OpCodes.Box, type);
                TraceCompile(OpCodes.Box + ": " + type);
            }
            else
            {
                il.Emit(OpCodes.Castclass, MapType(typeof(object)));
                TraceCompile(OpCodes.Castclass + ": " + type);
            }
        }

        internal void CastFromObject(Type type)
        {
            if (IsObject(type))
            { }
            else if (Helpers.IsValueType(type))
            {
                switch (MetadataVersion)
                {
                    case ILVersion.Net1:
                        il.Emit(OpCodes.Unbox, type);
                        il.Emit(OpCodes.Ldobj, type);
                        TraceCompile(OpCodes.Unbox + ": " + type);
                        TraceCompile(OpCodes.Ldobj + ": " + type);
                        break;
                    default:
#if FX11
                        throw new NotSupportedException();
#else
                        il.Emit(OpCodes.Unbox_Any, type);
                        TraceCompile(OpCodes.Unbox_Any + ": " + type);
                        break;
#endif
                }
            }
            else
            {
                il.Emit(OpCodes.Castclass, type);
                TraceCompile(OpCodes.Castclass + ": " + type);
            }
        }
        private readonly bool isStatic;
#if !SILVERLIGHT
        private readonly RuntimeTypeModel.SerializerPair[] methodPairs;

        internal MethodBuilder GetDedicatedMethod(int metaKey, bool read)
        {
            if (methodPairs == null) return null;
            // but if we *do* have pairs, we demand that we find a match...
            for (int i = 0; i < methodPairs.Length; i++ )
            {
                if (methodPairs[i].MetaKey == metaKey) { return read ? methodPairs[i].Deserialize : methodPairs[i].Serialize; }
            }
            throw new ArgumentException("Meta-key not found", "metaKey");
        }

        internal int MapMetaKeyToCompiledKey(int metaKey)
        {
            if (metaKey < 0 || methodPairs == null) return metaKey; // all meta, or a dummy/wildcard key

            for (int i = 0; i < methodPairs.Length; i++)
            {
                if (methodPairs[i].MetaKey == metaKey) return i;
            }
            throw new ArgumentException("Key could not be mapped: " + metaKey.ToString(), "metaKey");
        }
#else
        internal int MapMetaKeyToCompiledKey(int metaKey)
        {
            return metaKey;
        }
#endif

        private readonly bool isWriter;
#if FX11 || FEAT_IKVM
        internal bool NonPublic { get { return false; } }
#else
        private readonly bool nonPublic;
        internal bool NonPublic { get { return nonPublic; } }
#endif


        private readonly Local inputValue;
        public Local InputValue { get { return inputValue; } }
#if !(SILVERLIGHT || PHONE8)
        private readonly string assemblyName;
        internal CompilerContext(ILGenerator il, bool isStatic, bool isWriter, RuntimeTypeModel.SerializerPair[] methodPairs, TypeModel model, ILVersion metadataVersion, string assemblyName, Type inputType, string traceName)
        {
            if (il == null) throw new ArgumentNullException("il");
            if (methodPairs == null) throw new ArgumentNullException("methodPairs");
            if (model == null) throw new ArgumentNullException("model");
            if (Helpers.IsNullOrEmpty(assemblyName)) throw new ArgumentNullException("assemblyName");
            this.assemblyName = assemblyName;
            this.isStatic = isStatic;
            this.methodPairs = methodPairs;
            this.il = il;
            // nonPublic = false; <== implicit
            this.isWriter = isWriter;
            this.model = model;
            this.metadataVersion = metadataVersion;
            if (inputType != null) this.inputValue = new Local(null, inputType);
            TraceCompile(">> " + traceName);
        }
#endif
#if !(FX11 || FEAT_IKVM)
        private CompilerContext(Type associatedType, bool isWriter, bool isStatic, TypeModel model, Type inputType)
        {
            if (model == null) throw new ArgumentNullException("model");
#if FX11
            metadataVersion = ILVersion.Net1;
#else
            metadataVersion = ILVersion.Net2;
#endif
            this.isStatic = isStatic;
            this.isWriter = isWriter;
            this.model = model;
            nonPublic = true;
            Type[] paramTypes;
            Type returnType;
            if (isWriter)
            {
                returnType = typeof(void);
                paramTypes = new Type[] { typeof(object), typeof(ProtoWriter) };
            }
            else
            {
                returnType = typeof(object);
                paramTypes = new Type[] { typeof(object), typeof(ProtoReader) };
            }
            int uniqueIdentifier;
#if PLAT_NO_INTERLOCKED
            uniqueIdentifier = ++next;
#else
            uniqueIdentifier = Interlocked.Increment(ref next);
#endif
            method = new DynamicMethod("proto_" + uniqueIdentifier.ToString(), returnType, paramTypes, associatedType
#if COREFX
                .GetTypeInfo()
#endif
                .IsInterface ? typeof(object) : associatedType, true);
            this.il = method.GetILGenerator();
            if (inputType != null) this.inputValue = new Local(null, inputType);
            TraceCompile(">> " + method.Name);
        }

#endif
        private readonly ILGenerator il;

        private void Emit(OpCode opcode)
        {
            il.Emit(opcode);
            TraceCompile(opcode.ToString());
        }
        public void LoadValue(string value)
        {
            if (value == null)
            {
                LoadNullRef();
            }
            else
            {
                il.Emit(OpCodes.Ldstr, value);
                TraceCompile(OpCodes.Ldstr + ": " + value);
            }
        }
        public void LoadValue(float value)
        {
            il.Emit(OpCodes.Ldc_R4, value);
            TraceCompile(OpCodes.Ldc_R4 + ": " + value);
        }
        public void LoadValue(double value)
        {
            il.Emit(OpCodes.Ldc_R8, value);
            TraceCompile(OpCodes.Ldc_R8 + ": " + value);
        }
        public void LoadValue(long value)
        {
            il.Emit(OpCodes.Ldc_I8, value);
            TraceCompile(OpCodes.Ldc_I8 + ": " + value);
        }
        public void LoadValue(int value)
        {
            switch (value)
            {
                case 0: Emit(OpCodes.Ldc_I4_0); break;
                case 1: Emit(OpCodes.Ldc_I4_1); break;
                case 2: Emit(OpCodes.Ldc_I4_2); break;
                case 3: Emit(OpCodes.Ldc_I4_3); break;
                case 4: Emit(OpCodes.Ldc_I4_4); break;
                case 5: Emit(OpCodes.Ldc_I4_5); break;
                case 6: Emit(OpCodes.Ldc_I4_6); break;
                case 7: Emit(OpCodes.Ldc_I4_7); break;
                case 8: Emit(OpCodes.Ldc_I4_8); break;
                case -1: Emit(OpCodes.Ldc_I4_M1); break;
                default:
                    if (value >= -128 && value <= 127)
                    {
                        il.Emit(OpCodes.Ldc_I4_S, (sbyte)value);
                        TraceCompile(OpCodes.Ldc_I4_S + ": " + value);
                    }
                    else
                    {
                        il.Emit(OpCodes.Ldc_I4, value);
                        TraceCompile(OpCodes.Ldc_I4 + ": " + value);
                    }
                    break;

            }
        }

        MutableList locals = new MutableList();
        internal LocalBuilder GetFromPool(Type type)
        {
            int count = locals.Count;
            for (int i = 0; i < count; i++)
            {
                LocalBuilder item = (LocalBuilder)locals[i];
                if (item != null && item.LocalType == type)
                {
                    locals[i] = null; // remove from pool
                    return item;
                }
            }
            LocalBuilder result = il.DeclareLocal(type);
            TraceCompile("$ " + result + ": " + type);
            return result;
        }
        //
        internal void ReleaseToPool(LocalBuilder value)
        {
            int count = locals.Count;
            for (int i = 0; i < count; i++)
            {
                if (locals[i] == null)
                {
                    locals[i] = value; // released into existing slot
                    return;
                }
            }
            locals.Add(value); // create a new slot
        }
        public void LoadReaderWriter()
        {
            Emit(isStatic ? OpCodes.Ldarg_1 : OpCodes.Ldarg_2);
        }
        public void StoreValue(Local local)
        {
            if (local == this.InputValue)
            {
                byte b = isStatic ? (byte) 0 : (byte)1;
                il.Emit(OpCodes.Starg_S, b);
                TraceCompile(OpCodes.Starg_S + ": $" + b);
            }
            else
            {
#if !FX11
                switch (local.Value.LocalIndex)
                {
                    case 0: Emit(OpCodes.Stloc_0); break;
                    case 1: Emit(OpCodes.Stloc_1); break;
                    case 2: Emit(OpCodes.Stloc_2); break;
                    case 3: Emit(OpCodes.Stloc_3); break;
                    default:
#endif
                        OpCode code = UseShortForm(local) ? OpCodes.Stloc_S : OpCodes.Stloc;
                        il.Emit(code, local.Value);
                        TraceCompile(code + ": $" + local.Value);
#if !FX11
                        break;
                }
#endif
            }
        }
        public void LoadValue(Local local)
        {
            if (local == null) { /* nothing to do; top of stack */}
            else if (local == this.InputValue)
            {
                Emit(isStatic ? OpCodes.Ldarg_0 : OpCodes.Ldarg_1);
            }
            else
            {
#if !FX11
                switch (local.Value.LocalIndex)
                {
                    case 0: Emit(OpCodes.Ldloc_0); break;
                    case 1: Emit(OpCodes.Ldloc_1); break;
                    case 2: Emit(OpCodes.Ldloc_2); break;
                    case 3: Emit(OpCodes.Ldloc_3); break;
                    default:
#endif             
                        OpCode code = UseShortForm(local) ? OpCodes.Ldloc_S :  OpCodes.Ldloc;
                        il.Emit(code, local.Value);
                        TraceCompile(code + ": $" + local.Value);
#if !FX11
                        break;
                }
#endif
            }
        }
        public Local GetLocalWithValue(Type type, Compiler.Local fromValue)
        {
            if (fromValue != null)
            {
                if (fromValue.Type == type) return fromValue.AsCopy();
                // otherwise, load onto the stack and let the default handling (below) deal with it
                LoadValue(fromValue);
                if (!Helpers.IsValueType(type) && (fromValue.Type == null || !type.IsAssignableFrom(fromValue.Type)))
                { // need to cast
                    Cast(type);
                }
            }
            // need to store the value from the stack
            Local result = new Local(this, type);
            StoreValue(result);
            return result;
        }
        internal void EmitBasicRead(string methodName, Type expectedType)
        {
            MethodInfo method = MapType(typeof(ProtoReader)).GetMethod(
                methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null || method.ReturnType != expectedType
                || method.GetParameters().Length != 0) throw new ArgumentException("methodName");
            LoadReaderWriter();
            EmitCall(method);            
        }
        internal void EmitBasicRead(Type helperType, string methodName, Type expectedType)
        {
            MethodInfo method = helperType.GetMethod(
                methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (method == null || method.ReturnType != expectedType
                || method.GetParameters().Length != 1) throw new ArgumentException("methodName");
            LoadReaderWriter();
            EmitCall(method);
        }
        internal void EmitBasicWrite(string methodName, Compiler.Local fromValue)
        {
            if (Helpers.IsNullOrEmpty(methodName)) throw new ArgumentNullException("methodName");
            LoadValue(fromValue);
            LoadReaderWriter();
            EmitCall(GetWriterMethod(methodName));
        }
        private MethodInfo GetWriterMethod(string methodName)
        {
            Type writerType = MapType(typeof(ProtoWriter));
            MethodInfo[] methods = writerType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            foreach (MethodInfo method in methods)
            {
                if(method.Name != methodName) continue;
                ParameterInfo[] pis = method.GetParameters();
                if (pis.Length == 2 && pis[1].ParameterType == writerType) return method;
            }
            throw new ArgumentException("No suitable method found for: " + methodName, "methodName");
        }
        internal void EmitWrite(Type helperType, string methodName, Compiler.Local valueFrom)
        {
            if (Helpers.IsNullOrEmpty(methodName)) throw new ArgumentNullException("methodName");
            MethodInfo method = helperType.GetMethod(
                methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (method == null || method.ReturnType != MapType(typeof(void))) throw new ArgumentException("methodName");
            LoadValue(valueFrom);
            LoadReaderWriter();
            EmitCall(method);
        }
        public void EmitCall(MethodInfo method) { EmitCall(method, null); }
        public void EmitCall(MethodInfo method, Type targetType)
        {
            Helpers.DebugAssert(method != null);
            MemberInfo member = method;
            CheckAccessibility(ref member);
            OpCode opcode;
            if (method.IsStatic || Helpers.IsValueType(method.DeclaringType))
            {
                opcode = OpCodes.Call; 
            }
            else
            {
                opcode = OpCodes.Callvirt;
                if (targetType != null && Helpers.IsValueType(targetType) && !Helpers.IsValueType(method.DeclaringType))
                {
                    Constrain(targetType);
                }
            }
            il.EmitCall(opcode, method, null);
            TraceCompile(opcode + ": " + method + " on " + method.DeclaringType + (targetType == null ? "" : (" via " + targetType)));
        }
        /// <summary>
        /// Pushes a null reference onto the stack. Note that this should only
        /// be used to return a null (or set a variable to null); for null-tests
        /// use BranchIfTrue / BranchIfFalse.
        /// </summary>
        public void LoadNullRef()
        {
            Emit(OpCodes.Ldnull);
        }

        private int nextLabel;

        internal void WriteNullCheckedTail(Type type, IProtoSerializer tail, Compiler.Local valueFrom)
        {
            if (Helpers.IsValueType(type))
            {
                Type underlyingType = null;
#if !FX11
                underlyingType = Helpers.GetUnderlyingType(type);
#endif
                if (underlyingType == null)
                { // not a nullable T; can invoke directly
                    tail.EmitWrite(this, valueFrom);
                }
                else
                { // nullable T; check HasValue
                    using (Compiler.Local valOrNull = GetLocalWithValue(type, valueFrom))
                    {
                        LoadAddress(valOrNull, type);
                        LoadValue(type.GetProperty("HasValue"));
                        CodeLabel @end = DefineLabel();
                        BranchIfFalse(@end, false);
                        LoadAddress(valOrNull, type);
                        EmitCall(type.GetMethod("GetValueOrDefault", Helpers.EmptyTypes));
                        tail.EmitWrite(this, null);
                        MarkLabel(@end);
                    }
                }
            }
            else
            { // ref-type; do a null-check
                LoadValue(valueFrom);
                CopyValue();
                CodeLabel hasVal = DefineLabel(), @end = DefineLabel();
                BranchIfTrue(hasVal, true);
                DiscardValue();
                Branch(@end, false);
                MarkLabel(hasVal);
                tail.EmitWrite(this, null);
                MarkLabel(@end);
            }
        }

        internal void ReadNullCheckedTail(Type type, IProtoSerializer tail, Compiler.Local valueFrom)
        {
#if !FX11
            Type underlyingType;
            
            if (Helpers.IsValueType(type) && (underlyingType = Helpers.GetUnderlyingType(type)) != null)
            {
                if(tail.RequiresOldValue)
                {
                    // we expect the input value to be in valueFrom; need to unpack it from T?
                    using (Local loc = GetLocalWithValue(type, valueFrom))
                    {
                        LoadAddress(loc, type);
                        EmitCall(type.GetMethod("GetValueOrDefault", Helpers.EmptyTypes));
                    }
                }
                else
                {
                    Helpers.DebugAssert(valueFrom == null); // not expecting a valueFrom in this case
                }
                tail.EmitRead(this, null); // either unwrapped on the stack or not provided
                if (tail.ReturnsValue)
                {
                    // now re-wrap the value
                    EmitCtor(type, underlyingType);
                }
                return;
            }
#endif
            // either a ref-type of a non-nullable struct; treat "as is", even if null
            // (the type-serializer will handle the null case; it needs to allow null
            // inputs to perform the correct type of subclass creation)
            tail.EmitRead(this, valueFrom);
        }

        public void EmitCtor(Type type)
        {
            EmitCtor(type, Helpers.EmptyTypes);
        }

        public void EmitCtor(ConstructorInfo ctor)
        {
            if (ctor == null) throw new ArgumentNullException("ctor");
            MemberInfo ctorMember = ctor;
            CheckAccessibility(ref ctorMember);
            il.Emit(OpCodes.Newobj, ctor);
            TraceCompile(OpCodes.Newobj + ": " + ctor.DeclaringType);
        }

        public void InitLocal(Type type, Compiler.Local target)
        {
            LoadAddress(target, type, evenIfClass: true); // for class, initobj is a load-null, store-indirect
            il.Emit(OpCodes.Initobj, type);
            TraceCompile(OpCodes.Initobj + ": " + type);
        }
        public void EmitCtor(Type type, params Type[] parameterTypes)
        {
            Helpers.DebugAssert(type != null);
            Helpers.DebugAssert(parameterTypes != null);
            if (Helpers.IsValueType(type) && parameterTypes.Length == 0)
            {
                il.Emit(OpCodes.Initobj, type);
                TraceCompile(OpCodes.Initobj + ": " + type);
            }
            else
            {
                ConstructorInfo ctor =  Helpers.GetConstructor(type
#if COREFX
                .GetTypeInfo()
#endif
                    , parameterTypes, true);
                if (ctor == null) throw new InvalidOperationException("No suitable constructor found for " + type.FullName);
                EmitCtor(ctor);
            }
        }
#if !(PHONE8 || SILVERLIGHT || FX11)
        BasicList knownTrustedAssemblies, knownUntrustedAssemblies;
#endif
        bool InternalsVisible(Assembly assembly)
        {
#if PHONE8 || SILVERLIGHT || FX11
            return false;
#else
            if (Helpers.IsNullOrEmpty(assemblyName)) return false;
            if (knownTrustedAssemblies != null)
            {
                if (knownTrustedAssemblies.IndexOfReference(assembly) >= 0)
                {
                    return true;
                }
            }
            if (knownUntrustedAssemblies != null)
            {
                if (knownUntrustedAssemblies.IndexOfReference(assembly) >= 0)
                {
                    return false;
                }
            }
            bool isTrusted = false;
            Type attributeType = MapType(typeof(System.Runtime.CompilerServices.InternalsVisibleToAttribute));
            if(attributeType == null) return false;
#if FEAT_IKVM
            foreach (CustomAttributeData attrib in assembly.__GetCustomAttributes(attributeType, false))
            {
                if (attrib.ConstructorArguments.Count == 1)
                {
                    string privelegedAssembly = attrib.ConstructorArguments[0].Value as string;
                    if (privelegedAssembly == assemblyName || privelegedAssembly.StartsWith(assemblyName + ","))
                    {
                        isTrusted = true;
                        break;
                    }
                }
            }
#else

#if COREFX
            foreach (System.Runtime.CompilerServices.InternalsVisibleToAttribute attrib in assembly.GetCustomAttributes(attributeType))
#else
            foreach (System.Runtime.CompilerServices.InternalsVisibleToAttribute attrib in assembly.GetCustomAttributes(attributeType, false))
#endif
            {
                if (attrib.AssemblyName == assemblyName || attrib.AssemblyName.StartsWith(assemblyName + ","))
                {
                    isTrusted = true;
                    break;
                }
            }
#endif
            if (isTrusted)
            {
                if (knownTrustedAssemblies == null) knownTrustedAssemblies = new BasicList();
                knownTrustedAssemblies.Add(assembly);
            }
            else
            {
                if (knownUntrustedAssemblies == null) knownUntrustedAssemblies = new BasicList();
                knownUntrustedAssemblies.Add(assembly);
            }
            return isTrusted;
#endif
        }
        internal void CheckAccessibility(ref MemberInfo member)
        {
            if (member == null)
            {
                throw new ArgumentNullException("member");
            }
#if ! COREFX
            Type type;
#endif
            if (!NonPublic)
            {
                if(member is FieldInfo && member.Name.StartsWith("<") & member.Name.EndsWith(">k__BackingField"))
                {
                    var propName = member.Name.Substring(1, member.Name.Length - 17);
                    var prop = member.DeclaringType.GetProperty(propName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
                    if (prop != null) member = prop;
                }
                bool isPublic;
#if COREFX
                if (member is TypeInfo)
                {
                    TypeInfo ti = (TypeInfo)member;
                    do
                    {
                        isPublic = ti.IsNestedPublic || ti.IsPublic || ((ti.IsNested || ti.IsNestedAssembly || ti.IsNestedFamORAssem) && InternalsVisible(ti.Assembly));
                    } while (isPublic && ti.IsNested && (ti = ti.DeclaringType.GetTypeInfo()) != null);
                }
                else if (member is FieldInfo)
                {
                    FieldInfo field = ((FieldInfo)member);
                    isPublic = field.IsPublic || ((field.IsAssembly || field.IsFamilyOrAssembly) && InternalsVisible(Helpers.GetAssembly(field.DeclaringType)));
                }
                else if (member is PropertyInfo)
                {
                    isPublic = true; // defer to get/set
                }
                else if (member is ConstructorInfo)
                {
                    ConstructorInfo ctor = ((ConstructorInfo)member);
                    isPublic = ctor.IsPublic || ((ctor.IsAssembly || ctor.IsFamilyOrAssembly) && InternalsVisible(Helpers.GetAssembly(ctor.DeclaringType)));
                }
                else if (member is MethodInfo)
                {
                    MethodInfo method = ((MethodInfo)member);
                    isPublic = method.IsPublic || ((method.IsAssembly || method.IsFamilyOrAssembly) && InternalsVisible(Helpers.GetAssembly(method.DeclaringType)));
                    if (!isPublic)
                    {
                        // allow calls to TypeModel protected methods, and methods we are in the process of creating
                        if (
                                member is MethodBuilder ||
                                member.DeclaringType == MapType(typeof(TypeModel)))
                            isPublic = true;
                    }
                }
                else
                {
                    throw new NotSupportedException(member.GetType().Name);
                }
#else
                MemberTypes memberType = member.MemberType;
                switch (memberType)
                {
                    case MemberTypes.TypeInfo:
                        // top-level type
                        type = (Type)member;
                        isPublic = type.IsPublic || InternalsVisible(type.Assembly);
                        break;
                    case MemberTypes.NestedType:
                        type = (Type)member;
                        do
                        {
                            isPublic = type.IsNestedPublic || type.IsPublic || ((type.DeclaringType == null || type.IsNestedAssembly || type.IsNestedFamORAssem) && InternalsVisible(type.Assembly));
                        } while (isPublic && (type = type.DeclaringType) != null); // ^^^ !type.IsNested, but not all runtimes have that
                        break;
                    case MemberTypes.Field:
                        FieldInfo field = ((FieldInfo)member);
                        isPublic = field.IsPublic || ((field.IsAssembly || field.IsFamilyOrAssembly) && InternalsVisible(field.DeclaringType.Assembly));
                        break;
                    case MemberTypes.Constructor:
                        ConstructorInfo ctor = ((ConstructorInfo)member);
                        isPublic = ctor.IsPublic || ((ctor.IsAssembly || ctor.IsFamilyOrAssembly) && InternalsVisible(ctor.DeclaringType.Assembly));
                        break;
                    case MemberTypes.Method:
                        MethodInfo method = ((MethodInfo)member);
                        isPublic = method.IsPublic || ((method.IsAssembly || method.IsFamilyOrAssembly) && InternalsVisible(method.DeclaringType.Assembly));
                        if (!isPublic)
                        {
                            // allow calls to TypeModel protected methods, and methods we are in the process of creating
                            if(
#if !SILVERLIGHT
                                member is MethodBuilder ||
#endif
                                member.DeclaringType == MapType(typeof(TypeModel))) isPublic = true; 
                        }
                        break;
                    case MemberTypes.Property:
                        isPublic = true; // defer to get/set
                        break;
                    default:
                        throw new NotSupportedException(memberType.ToString());
                }
#endif
                if (!isPublic)
                {
#if COREFX
                    if (member is TypeInfo)
                    {
                        throw new InvalidOperationException("Non-public type cannot be used with full dll compilation: " +
                                ((TypeInfo)member).FullName);
                    }
                    else
                    {
                        throw new InvalidOperationException("Non-public member cannot be used with full dll compilation: " +
                                member.DeclaringType.FullName + "." + member.Name);
                    }

#else
                    switch (memberType)
                    {
                        case MemberTypes.TypeInfo:
                        case MemberTypes.NestedType:
                            throw new InvalidOperationException("Non-public type cannot be used with full dll compilation: " +
                                ((Type)member).FullName);
                        default:
                            throw new InvalidOperationException("Non-public member cannot be used with full dll compilation: " +
                                member.DeclaringType.FullName + "." + member.Name);
                    }
#endif

                }
            }
        }

        public void LoadValue(FieldInfo field)
        {
            MemberInfo member = field;
            CheckAccessibility(ref member);
            if (member is PropertyInfo)
            {
                LoadValue((PropertyInfo)member);
            }
            else
            {
                OpCode code = field.IsStatic ? OpCodes.Ldsfld : OpCodes.Ldfld;
                il.Emit(code, field);
                TraceCompile(code + ": " + field + " on " + field.DeclaringType);
            }
        }
#if FEAT_IKVM
        public void StoreValue(System.Reflection.FieldInfo field)
        {
            StoreValue(MapType(field.DeclaringType).GetField(field.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance));
        }
        public void StoreValue(System.Reflection.PropertyInfo property)
        {
            StoreValue(MapType(property.DeclaringType).GetProperty(property.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance));
        }
        public void LoadValue(System.Reflection.FieldInfo field)
        {
            LoadValue(MapType(field.DeclaringType).GetField(field.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance));
        }
        public void LoadValue(System.Reflection.PropertyInfo property)
        {
            LoadValue(MapType(property.DeclaringType).GetProperty(property.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance));
        }
#endif
        public void StoreValue(FieldInfo field)
        {
            MemberInfo member = field;
            CheckAccessibility(ref member);
            if (member is PropertyInfo)
            {
                StoreValue((PropertyInfo)member);
            }
            else
            {
                OpCode code = field.IsStatic ? OpCodes.Stsfld : OpCodes.Stfld;
                il.Emit(code, field);
                TraceCompile(code + ": " + field + " on " + field.DeclaringType);
            }
        }
        
        public void LoadValue(PropertyInfo property)
        {
            MemberInfo member = property;
            CheckAccessibility(ref member);
            EmitCall(Helpers.GetGetMethod(property, true, true));
        }
        public void StoreValue(PropertyInfo property)
        {
            MemberInfo member = property;
            CheckAccessibility(ref member);
            EmitCall(Helpers.GetSetMethod(property, true, true));
        }

        //internal void EmitInstance()
        //{
        //    if (isStatic) throw new InvalidOperationException();
        //    Emit(OpCodes.Ldarg_0);
        //}

        internal static void LoadValue(ILGenerator il, int value)
        {
            switch (value)
            {
                case 0: il.Emit(OpCodes.Ldc_I4_0); break;
                case 1: il.Emit(OpCodes.Ldc_I4_1); break;
                case 2: il.Emit(OpCodes.Ldc_I4_2); break;
                case 3: il.Emit(OpCodes.Ldc_I4_3); break;
                case 4: il.Emit(OpCodes.Ldc_I4_4); break;
                case 5: il.Emit(OpCodes.Ldc_I4_5); break;
                case 6: il.Emit(OpCodes.Ldc_I4_6); break;
                case 7: il.Emit(OpCodes.Ldc_I4_7); break;
                case 8: il.Emit(OpCodes.Ldc_I4_8); break;
                case -1: il.Emit(OpCodes.Ldc_I4_M1); break;
                default: il.Emit(OpCodes.Ldc_I4, value); break;
            }
        }

        private bool UseShortForm(Local local)
        {
#if FX11
            return locals.Count < 256;
#else
            return local.Value.LocalIndex < 256;
#endif
        }
#if FEAT_IKVM
        internal void LoadAddress(Local local, System.Type type)
        {
            LoadAddress(local, MapType(type));
        }
#endif
        internal void LoadAddress(Local local, Type type, bool evenIfClass = false)
        {
            if (evenIfClass || Helpers.IsValueType(type))
            {
                if (local == null)
                {
                    throw new InvalidOperationException("Cannot load the address of the head of the stack");
                }

                if (local == this.InputValue)
                {
                    il.Emit(OpCodes.Ldarga_S, (isStatic ? (byte)0 : (byte)1));
                    TraceCompile(OpCodes.Ldarga_S + ": $" + (isStatic ? 0 : 1));
                }
                else
                {
                    OpCode code = UseShortForm(local) ? OpCodes.Ldloca_S : OpCodes.Ldloca;
                    il.Emit(code, local.Value);
                    TraceCompile(code + ": $" + local.Value);
                }

            }
            else
            {   // reference-type; already *is* the address; just load it
                LoadValue(local);
            }
        }
        internal void Branch(CodeLabel label, bool @short)
        {
            OpCode code = @short ? OpCodes.Br_S : OpCodes.Br;
            il.Emit(code, label.Value);
            TraceCompile(code + ": " + label.Index);
        }
        internal void BranchIfFalse(CodeLabel label, bool @short)
        {
            OpCode code = @short ? OpCodes.Brfalse_S :  OpCodes.Brfalse;
            il.Emit(code, label.Value);
            TraceCompile(code + ": " + label.Index);
        }


        internal void BranchIfTrue(CodeLabel label, bool @short)
        {
            OpCode code = @short ? OpCodes.Brtrue_S : OpCodes.Brtrue;
            il.Emit(code, label.Value);
            TraceCompile(code + ": " + label.Index);
        }
        internal void BranchIfEqual(CodeLabel label, bool @short)
        {
            OpCode code = @short ? OpCodes.Beq_S : OpCodes.Beq;
            il.Emit(code, label.Value);
            TraceCompile(code + ": " + label.Index);
        }
        //internal void TestEqual()
        //{
        //    Emit(OpCodes.Ceq);
        //}


        internal void CopyValue()
        {
            Emit(OpCodes.Dup);
        }

        internal void BranchIfGreater(CodeLabel label, bool @short)
        {
            OpCode code = @short ? OpCodes.Bgt_S : OpCodes.Bgt;
            il.Emit(code, label.Value);
            TraceCompile(code + ": " + label.Index);
        }

        internal void BranchIfLess(CodeLabel label, bool @short)
        {
            OpCode code = @short ? OpCodes.Blt_S : OpCodes.Blt;
            il.Emit(code, label.Value);
            TraceCompile(code + ": " + label.Index);
        }

        internal void DiscardValue()
        {
            Emit(OpCodes.Pop);
        }

        public void Subtract()
        {
            Emit(OpCodes.Sub);
        }



        public void Switch(CodeLabel[] jumpTable)
        {
            const int MAX_JUMPS = 128;

            if (jumpTable.Length <= MAX_JUMPS)
            {
                // simple case
                Label[] labels = new Label[jumpTable.Length];
                for (int i = 0; i < labels.Length; i++)
                {
                    labels[i] = jumpTable[i].Value;
                }
                TraceCompile(OpCodes.Switch.ToString());
                il.Emit(OpCodes.Switch, labels);
            }
            else
            {
                // too many to jump easily (especially on Android) - need to split up (note: uses a local pulled from the stack)
                using (Local val = GetLocalWithValue(MapType(typeof(int)), null))
                {
                    int count = jumpTable.Length, offset = 0;
                    int blockCount = count / MAX_JUMPS;
                    if ((count % MAX_JUMPS) != 0) blockCount++;

                    Label[] blockLabels = new Label[blockCount];
                    for (int i = 0; i < blockCount; i++)
                    {
                        blockLabels[i] = il.DefineLabel();
                    }
                    CodeLabel endOfSwitch = DefineLabel();
                    
                    LoadValue(val);
                    LoadValue(MAX_JUMPS);
                    Emit(OpCodes.Div);
                    TraceCompile(OpCodes.Switch.ToString());
                    il.Emit(OpCodes.Switch, blockLabels);
                    Branch(endOfSwitch, false);

                    Label[] innerLabels = new Label[MAX_JUMPS];
                    for (int blockIndex = 0; blockIndex < blockCount; blockIndex++)
                    {
                        il.MarkLabel(blockLabels[blockIndex]);

                        int itemsThisBlock = Math.Min(MAX_JUMPS, count);
                        count -= itemsThisBlock;
                        if (innerLabels.Length != itemsThisBlock) innerLabels = new Label[itemsThisBlock];

                        int subtract = offset;
                        for (int j = 0; j < itemsThisBlock; j++)
                        {
                            innerLabels[j] = jumpTable[offset++].Value;
                        }
                        LoadValue(val);
                        if (subtract != 0) // switches are always zero-based
                        {
                            LoadValue(subtract);
                            Emit(OpCodes.Sub);
                        }
                        TraceCompile(OpCodes.Switch.ToString());
                        il.Emit(OpCodes.Switch, innerLabels);
                        if (count != 0)
                        { // force default to the very bottom
                            Branch(endOfSwitch, false);
                        }
                    }
                    Helpers.DebugAssert(count == 0, "Should use exactly all switch items");
                    MarkLabel(endOfSwitch);
                }
            }
        }

        internal void EndFinally()
        {
            il.EndExceptionBlock();
            TraceCompile("EndExceptionBlock");
        }

        internal void BeginFinally()
        {
            il.BeginFinallyBlock();
            TraceCompile("BeginFinallyBlock");
        }

        internal void EndTry(CodeLabel label, bool @short)
        {
            OpCode code = @short ? OpCodes.Leave_S : OpCodes.Leave;
            il.Emit(code, label.Value);
            TraceCompile(code + ": " + label.Index);
        }

        internal CodeLabel BeginTry()
        {
            CodeLabel label = new CodeLabel(il.BeginExceptionBlock(), nextLabel++);
            TraceCompile("BeginExceptionBlock: " + label.Index);
            return label;
        }

        internal void Constrain(Type type)
        {
#if FX11
            throw new NotSupportedException("This operation requires a constrained call, which is not available on this platform");
#else
            il.Emit(OpCodes.Constrained, type);
            TraceCompile(OpCodes.Constrained + ": " + type);
#endif
        }


        internal void TryCast(Type type)
        {
            il.Emit(OpCodes.Isinst, type);
            TraceCompile(OpCodes.Isinst + ": " + type);
        }

        internal void Cast(Type type)
        {
            il.Emit(OpCodes.Castclass, type);
            TraceCompile(OpCodes.Castclass + ": " + type);
        }
        public IDisposable Using(Local local)
        {
            return new UsingBlock(this, local);
        }
        private sealed class UsingBlock : IDisposable{
            private Local local;
            CompilerContext ctx;
            CodeLabel label;
            /// <summary>
            /// Creates a new "using" block (equivalent) around a variable;
            /// the variable must exist, and note that (unlike in C#) it is
            /// the variables *final* value that gets disposed. If you need
            /// *original* disposal, copy your variable first.
            /// 
            /// It is the callers responsibility to ensure that the variable's
            /// scope fully-encapsulates the "using"; if not, the variable
            /// may be re-used (and thus re-assigned) unexpectedly.
            /// </summary>
            public UsingBlock(CompilerContext ctx, Local local)
            {
                if (ctx == null) throw new ArgumentNullException("ctx");
                if (local == null) throw new ArgumentNullException("local");

                Type type = local.Type;
                // check if **never** disposable
                if ((Helpers.IsValueType(type) || Helpers.IsSealed(type)) &&
                    !ctx.MapType(typeof(IDisposable)).IsAssignableFrom(type))
                {
                    return; // nothing to do! easiest "using" block ever
                    // (note that C# wouldn't allow this as a "using" block,
                    // but we'll be generous and simply not do anything)
                }
                this.local = local;
                this.ctx = ctx;
                label = ctx.BeginTry();
                
            }
            public void Dispose()
            {
                if (local == null || ctx == null) return;

                ctx.EndTry(label, false);
                ctx.BeginFinally();
                Type disposableType = ctx.MapType(typeof (IDisposable));
                MethodInfo dispose = disposableType.GetMethod("Dispose");
                Type type = local.Type;
                // remember that we've already (in the .ctor) excluded the case
                // where it *cannot* be disposable
                if (Helpers.IsValueType(type))
                {
                    ctx.LoadAddress(local, type);
                    switch (ctx.MetadataVersion)
                    {
                        case ILVersion.Net1:
                            ctx.LoadValue(local);
                            ctx.CastToObject(type);
                            break;
                        default:
#if FX11
                            throw new NotSupportedException();
#else
                            ctx.Constrain(type);
                            break;
#endif
                    }
                    ctx.EmitCall(dispose);                    
                }
                else
                {
                    Compiler.CodeLabel @null = ctx.DefineLabel();
                    if (disposableType.IsAssignableFrom(type))
                    {   // *known* to be IDisposable; just needs a null-check                            
                        ctx.LoadValue(local);
                        ctx.BranchIfFalse(@null, true);
                        ctx.LoadAddress(local, type);
                    }
                    else
                    {   // *could* be IDisposable; test via "as"
                        using (Compiler.Local disp = new Compiler.Local(ctx, disposableType))
                        {
                            ctx.LoadValue(local);
                            ctx.TryCast(disposableType);
                            ctx.CopyValue();
                            ctx.StoreValue(disp);
                            ctx.BranchIfFalse(@null, true);
                            ctx.LoadAddress(disp, disposableType);
                        }
                    }
                    ctx.EmitCall(dispose);
                    ctx.MarkLabel(@null);
                }
                ctx.EndFinally();
                this.local = null;
                this.ctx = null;
                label = new CodeLabel(); // default
            }
        }

        internal void Add()
        {
            Emit(OpCodes.Add);
        }

        internal void LoadLength(Local arr, bool zeroIfNull)
        {
            Helpers.DebugAssert(arr.Type.IsArray && arr.Type.GetArrayRank() == 1);

            if (zeroIfNull)
            {
                Compiler.CodeLabel notNull = DefineLabel(), done = DefineLabel();
                LoadValue(arr);
                CopyValue(); // optimised for non-null case
                BranchIfTrue(notNull, true);
                DiscardValue();
                LoadValue(0);
                Branch(done, true);
                MarkLabel(notNull);
                Emit(OpCodes.Ldlen);
                Emit(OpCodes.Conv_I4);
                MarkLabel(done);
            }
            else
            {
                LoadValue(arr);
                Emit(OpCodes.Ldlen);
                Emit(OpCodes.Conv_I4);
            }
        }

        internal void CreateArray(Type elementType, Local length)
        {
            LoadValue(length);
            il.Emit(OpCodes.Newarr, elementType);
            TraceCompile(OpCodes.Newarr + ": " + elementType);
        }

        internal void LoadArrayValue(Local arr, Local i)
        {
            Type type = arr.Type;
            Helpers.DebugAssert(type.IsArray && arr.Type.GetArrayRank() == 1);
            type = type.GetElementType();
            Helpers.DebugAssert(type != null, "Not an array: " + arr.Type.FullName);
            LoadValue(arr);
            LoadValue(i);
            switch(Helpers.GetTypeCode(type)) {
                case ProtoTypeCode.SByte: Emit(OpCodes.Ldelem_I1); break;
                case ProtoTypeCode.Int16: Emit(OpCodes.Ldelem_I2); break;
                case ProtoTypeCode.Int32: Emit(OpCodes.Ldelem_I4); break;
                case ProtoTypeCode.Int64: Emit(OpCodes.Ldelem_I8); break;

                case ProtoTypeCode.Byte: Emit(OpCodes.Ldelem_U1); break;
                case ProtoTypeCode.UInt16: Emit(OpCodes.Ldelem_U2); break;
                case ProtoTypeCode.UInt32: Emit(OpCodes.Ldelem_U4); break;
                case ProtoTypeCode.UInt64: Emit(OpCodes.Ldelem_I8); break; // odd, but this is what C# does...

                case ProtoTypeCode.Single: Emit(OpCodes.Ldelem_R4); break;
                case ProtoTypeCode.Double: Emit(OpCodes.Ldelem_R8); break;
                default:
                    if (Helpers.IsValueType(type))
                    {
                        il.Emit(OpCodes.Ldelema, type);
                        il.Emit(OpCodes.Ldobj, type);
                        TraceCompile(OpCodes.Ldelema + ": " + type);
                        TraceCompile(OpCodes.Ldobj + ": " + type);
                    }
                    else
                    {
                        Emit(OpCodes.Ldelem_Ref);
                    }
             
                    break;
            }
            
        }



        internal void LoadValue(Type type)
        {
            il.Emit(OpCodes.Ldtoken, type);
            TraceCompile(OpCodes.Ldtoken + ": " + type);
            EmitCall(MapType(typeof(System.Type)).GetMethod("GetTypeFromHandle"));
        }

        internal void ConvertToInt32(ProtoTypeCode typeCode, bool uint32Overflow)
        {
            switch (typeCode)
            {
                case ProtoTypeCode.Byte:
                case ProtoTypeCode.SByte:
                case ProtoTypeCode.Int16:
                case ProtoTypeCode.UInt16:
                    Emit(OpCodes.Conv_I4);
                    break;
                case ProtoTypeCode.Int32:
                    break;
                case ProtoTypeCode.Int64:
                    Emit(OpCodes.Conv_Ovf_I4);
                    break;
                case ProtoTypeCode.UInt32:
                    Emit(uint32Overflow ? OpCodes.Conv_Ovf_I4_Un : OpCodes.Conv_Ovf_I4);
                    break;
                case ProtoTypeCode.UInt64:
                    Emit(OpCodes.Conv_Ovf_I4_Un);
                    break;
                default:
                    throw new InvalidOperationException("ConvertToInt32 not implemented for: " + typeCode.ToString());
            }
        }

        internal void ConvertFromInt32(ProtoTypeCode typeCode, bool uint32Overflow)
        {
            switch (typeCode)
            {
                case ProtoTypeCode.SByte: Emit(OpCodes.Conv_Ovf_I1); break;
                case ProtoTypeCode.Byte: Emit(OpCodes.Conv_Ovf_U1); break;
                case ProtoTypeCode.Int16: Emit(OpCodes.Conv_Ovf_I2); break;
                case ProtoTypeCode.UInt16: Emit(OpCodes.Conv_Ovf_U2); break;
                case ProtoTypeCode.Int32: break;
                case ProtoTypeCode.UInt32: Emit(uint32Overflow ? OpCodes.Conv_Ovf_U4 : OpCodes.Conv_U4); break;
                case ProtoTypeCode.Int64: Emit(OpCodes.Conv_I8); break;
                case ProtoTypeCode.UInt64: Emit(OpCodes.Conv_U8); break;
                default: throw new InvalidOperationException();
            }
        }

        internal void LoadValue(decimal value)
        {
            if (value == 0M)
            {
                LoadValue(typeof(decimal).GetField("Zero"));
            }
            else
            {
                int[] bits = decimal.GetBits(value);
                LoadValue(bits[0]); // lo
                LoadValue(bits[1]); // mid
                LoadValue(bits[2]); // hi
                LoadValue((int)(((uint)bits[3]) >> 31)); // isNegative (bool, but int for CLI purposes)
                LoadValue((bits[3] >> 16) & 0xFF); // scale (byte, but int for CLI purposes)

                EmitCtor(MapType(typeof(decimal)), new Type[] { MapType(typeof(int)), MapType(typeof(int)), MapType(typeof(int)), MapType(typeof(bool)), MapType(typeof(byte)) });
            }
        }

        internal void LoadValue(Guid value)
        {
            if (value == Guid.Empty)
            {
                LoadValue(typeof(Guid).GetField("Empty"));
            }
            else
            { // note we're adding lots of shorts/bytes here - but at the IL level they are I4, not I1/I2 (which barely exist)
                byte[] bytes = value.ToByteArray();
                int i = (bytes[0]) | (bytes[1] << 8) | (bytes[2] << 16) | (bytes[3] << 24);
                LoadValue(i);
                short s = (short)((bytes[4]) | (bytes[5] << 8));
                LoadValue(s);
                s = (short)((bytes[6]) | (bytes[7] << 8));
                LoadValue(s);
                for (i = 8; i <= 15; i++)
                {
                    LoadValue(bytes[i]);
                }
                EmitCtor(MapType(typeof(Guid)), new Type[] { MapType(typeof(int)), MapType(typeof(short)), MapType(typeof(short)),
                            MapType(typeof(byte)), MapType(typeof(byte)), MapType(typeof(byte)), MapType(typeof(byte)), MapType(typeof(byte)), MapType(typeof(byte)), MapType(typeof(byte)), MapType(typeof(byte)) });
            }
        }

        //internal void LoadValue(bool value)
        //{
        //    Emit(value ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
        //}

        internal void LoadSerializationContext()
        {
            LoadReaderWriter();
            LoadValue((isWriter ? typeof(ProtoWriter) : typeof(ProtoReader)).GetProperty("Context"));
        }

        private readonly TypeModel model;

        internal Type MapType(System.Type type)
        {
            return model.MapType(type);
        }

        private readonly ILVersion metadataVersion;
        public ILVersion MetadataVersion { get { return metadataVersion; } }
        public enum ILVersion
        {
            Net1, Net2
        }

        internal bool AllowInternal(PropertyInfo property)
        {
            return NonPublic ? true : InternalsVisible(Helpers.GetAssembly(property.DeclaringType));
        }
    }
}
#endif
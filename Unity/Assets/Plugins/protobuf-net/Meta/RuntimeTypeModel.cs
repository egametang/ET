#if !NO_RUNTIME
using System;
using System.Collections;
using System.Text;

#if FEAT_IKVM
using Type = IKVM.Reflection.Type;
using IKVM.Reflection;
using IKVM.Reflection.Emit;
#else
using System.Reflection;
#if FEAT_COMPILER
using System.Reflection.Emit;
#endif
#endif

using ProtoBuf.Serializers;
using System.Threading;
using System.IO;


namespace ProtoBuf.Meta
{
    /// <summary>
    /// Provides protobuf serialization support for a number of types that can be defined at runtime
    /// </summary>
    public sealed class RuntimeTypeModel : TypeModel
    {
        private ushort options;
        private const ushort
           OPTIONS_InferTagFromNameDefault = 1,
           OPTIONS_IsDefaultModel = 2,
           OPTIONS_Frozen = 4,
           OPTIONS_AutoAddMissingTypes = 8,
#if FEAT_COMPILER && !FX11
           OPTIONS_AutoCompile = 16,
#endif
           OPTIONS_UseImplicitZeroDefaults = 32,
           OPTIONS_AllowParseableTypes = 64,
           OPTIONS_AutoAddProtoContractTypesOnly = 128,
           OPTIONS_IncludeDateTimeKind = 256;
        private bool GetOption(ushort option)
        {
            return (options & option) == option;
        }
        private void SetOption(ushort option, bool value)
        {
            if (value) options |= option;
            else options &= (ushort)~option;
        }
        /// <summary>
        /// Global default that
        /// enables/disables automatic tag generation based on the existing name / order
        /// of the defined members. See <seealso cref="ProtoContractAttribute.InferTagFromName"/>
        /// for usage and <b>important warning</b> / explanation.
        /// You must set the global default before attempting to serialize/deserialize any
        /// impacted type.
        /// </summary>
        public bool InferTagFromNameDefault
        {
            get { return GetOption(OPTIONS_InferTagFromNameDefault); }
            set { SetOption(OPTIONS_InferTagFromNameDefault, value); }
        }

        /// <summary>
        /// Global default that determines whether types are considered serializable
        /// if they have [DataContract] / [XmlType]. With this enabled, <b>ONLY</b>
        /// types marked as [ProtoContract] are added automatically.
        /// </summary>
        public bool AutoAddProtoContractTypesOnly
        {
            get { return GetOption(OPTIONS_AutoAddProtoContractTypesOnly); }
            set { SetOption(OPTIONS_AutoAddProtoContractTypesOnly, value); }
        }

        /// <summary>
        /// Global switch that enables or disables the implicit
        /// handling of "zero defaults"; meanning: if no other default is specified,
        /// it assumes bools always default to false, integers to zero, etc.
        /// 
        /// If this is disabled, no such assumptions are made and only *explicit*
        /// default values are processed. This is enabled by default to 
        /// preserve similar logic to v1.
        /// </summary>
        public bool UseImplicitZeroDefaults
        {
            get {return GetOption(OPTIONS_UseImplicitZeroDefaults);}
            set {
                if (!value && GetOption(OPTIONS_IsDefaultModel))
                {
                    throw new InvalidOperationException("UseImplicitZeroDefaults cannot be disabled on the default model");
                }
                SetOption(OPTIONS_UseImplicitZeroDefaults, value);
            }
        }

        /// <summary>
        /// Global switch that determines whether types with a <c>.ToString()</c> and a <c>Parse(string)</c>
        /// should be serialized as strings.
        /// </summary>
        public bool AllowParseableTypes
        {
            get { return GetOption(OPTIONS_AllowParseableTypes); }
            set { SetOption(OPTIONS_AllowParseableTypes, value); }
        }
        /// <summary>
        /// Global switch that determines whether DateTime serialization should include the <c>Kind</c> of the date/time.
        /// </summary>
        public bool IncludeDateTimeKind
        {
            get { return GetOption(OPTIONS_IncludeDateTimeKind); }
            set { SetOption(OPTIONS_IncludeDateTimeKind, value); }
        }

        /// <summary>
        /// Should the <c>Kind</c> be included on date/time values?
        /// </summary>
        protected internal override bool SerializeDateTimeKind()
        {
            return GetOption(OPTIONS_IncludeDateTimeKind);
        }
        

        private sealed class Singleton
        {
            private Singleton() { }
            internal static readonly RuntimeTypeModel Value = new RuntimeTypeModel(true);
        }
        /// <summary>
        /// The default model, used to support ProtoBuf.Serializer
        /// </summary>
        public static RuntimeTypeModel Default
        {
            get { return Singleton.Value; }
        }
        /// <summary>
        /// Returns a sequence of the Type instances that can be
        /// processed by this model.
        /// </summary>
        public IEnumerable GetTypes() { return types; }

        /// <summary>
        /// Suggest a .proto definition for the given type
        /// </summary>
        /// <param name="type">The type to generate a .proto definition for, or <c>null</c> to generate a .proto that represents the entire model</param>
        /// <returns>The .proto definition as a string</returns>
        public override string GetSchema(Type type)
        {
            BasicList requiredTypes = new BasicList();
            MetaType primaryType = null;
            bool isInbuiltType = false;
            if (type == null)
            { // generate for the entire model
                foreach(MetaType meta in types)
                {
                    MetaType tmp = meta.GetSurrogateOrBaseOrSelf(false);
                    if (!requiredTypes.Contains(tmp))
                    { // ^^^ note that the type might have been added as a descendent
                        requiredTypes.Add(tmp);
                        CascadeDependents(requiredTypes, tmp);
                    }
                }
            }
            else
            {
                Type tmp = Helpers.GetUnderlyingType(type);
                if (tmp != null) type = tmp;

                WireType defaultWireType;
                isInbuiltType = (ValueMember.TryGetCoreSerializer(this, DataFormat.Default, type, out defaultWireType, false, false, false, false) != null);
                if (!isInbuiltType)
                {
                    //Agenerate just relative to the supplied type
                    int index = FindOrAddAuto(type, false, false, false);
                    if (index < 0) throw new ArgumentException("The type specified is not a contract-type", "type");

                    // get the required types
                    primaryType = ((MetaType)types[index]).GetSurrogateOrBaseOrSelf(false);
                    requiredTypes.Add(primaryType);
                    CascadeDependents(requiredTypes, primaryType);
                }
            }

            // use the provided type's namespace for the "package"
            StringBuilder headerBuilder = new StringBuilder();
            string package = null;

            if (!isInbuiltType)
            {
                IEnumerable typesForNamespace = primaryType == null ? types : requiredTypes;
                foreach (MetaType meta in typesForNamespace)
                {
                    if (meta.IsList) continue;
                    string tmp = meta.Type.Namespace;
                    if (!Helpers.IsNullOrEmpty(tmp))
                    {
                        if (tmp.StartsWith("System.")) continue;
                        if (package == null)
                        { // haven't seen any suggestions yet
                            package = tmp;
                        }
                        else if (package == tmp)
                        { // that's fine; a repeat of the one we already saw
                        }
                        else
                        { // something else; have confliucting suggestions; abort
                            package = null;
                            break;
                        }
                    }
                }
            }

            if (!Helpers.IsNullOrEmpty(package))
            {
                headerBuilder.Append("package ").Append(package).Append(';');
                Helpers.AppendLine(headerBuilder);
            }

            bool requiresBclImport = false;
            StringBuilder bodyBuilder = new StringBuilder();
            // sort them by schema-name
            MetaType[] metaTypesArr = new MetaType[requiredTypes.Count];
            requiredTypes.CopyTo(metaTypesArr, 0);
            Array.Sort(metaTypesArr, MetaType.Comparer.Default);

            // write the messages
            if (isInbuiltType)
            {
                Helpers.AppendLine(bodyBuilder).Append("message ").Append(type.Name).Append(" {");
                MetaType.NewLine(bodyBuilder, 1).Append("optional ").Append(GetSchemaTypeName(type, DataFormat.Default, false, false, ref requiresBclImport))
                    .Append(" value = 1;");
                Helpers.AppendLine(bodyBuilder).Append('}');
            }
            else
            {
                for (int i = 0; i < metaTypesArr.Length; i++)
                {
                    MetaType tmp = metaTypesArr[i];
                    if (tmp.IsList && tmp != primaryType) continue;
                    tmp.WriteSchema(bodyBuilder, 0, ref requiresBclImport);
                }
            }
            if (requiresBclImport)
            {
                headerBuilder.Append("import \"bcl.proto\"; // schema for protobuf-net's handling of core .NET types");
                Helpers.AppendLine(headerBuilder);
            }
            return Helpers.AppendLine(headerBuilder.Append(bodyBuilder)).ToString();
        }
        private void CascadeDependents(BasicList list, MetaType metaType)
        {
            MetaType tmp;
            if (metaType.IsList)
            {
                Type itemType = TypeModel.GetListItemType(this, metaType.Type);
                WireType defaultWireType;
                IProtoSerializer coreSerializer = ValueMember.TryGetCoreSerializer(this, DataFormat.Default, itemType, out defaultWireType, false, false, false, false);
                if (coreSerializer == null)
                {
                    int index = FindOrAddAuto(itemType, false, false, false);
                    if (index >= 0)
                    {
                        tmp = ((MetaType)types[index]).GetSurrogateOrBaseOrSelf(false);
                        if (!list.Contains(tmp))
                        { // could perhaps also implement as a queue, but this should work OK for sane models
                            list.Add(tmp);
                            CascadeDependents(list, tmp);
                        }
                    }
                }
            }
            else
            {
                if (metaType.IsAutoTuple)
                {
                    MemberInfo[] mapping;
                    if(MetaType.ResolveTupleConstructor(metaType.Type, out mapping) != null)
                    {
                        for (int i = 0; i < mapping.Length; i++)
                        {
                            Type type = null;
                            if (mapping[i] is PropertyInfo) type = ((PropertyInfo)mapping[i]).PropertyType;
                            else if (mapping[i] is FieldInfo) type = ((FieldInfo)mapping[i]).FieldType;

                            WireType defaultWireType;
                            IProtoSerializer coreSerializer = ValueMember.TryGetCoreSerializer(this, DataFormat.Default, type, out defaultWireType, false, false, false, false);
                            if (coreSerializer == null)
                            {
                                int index = FindOrAddAuto(type, false, false, false);
                                if (index >= 0)
                                {
                                    tmp = ((MetaType)types[index]).GetSurrogateOrBaseOrSelf(false);
                                    if (!list.Contains(tmp))
                                    { // could perhaps also implement as a queue, but this should work OK for sane models
                                        list.Add(tmp);
                                        CascadeDependents(list, tmp);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    foreach (ValueMember member in metaType.Fields)
                    {
                        Type type = member.ItemType;
                        if (type == null) type = member.MemberType;
                        WireType defaultWireType;
                        IProtoSerializer coreSerializer = ValueMember.TryGetCoreSerializer(this, DataFormat.Default, type, out defaultWireType, false, false, false, false);
                        if (coreSerializer == null)
                        {
                            // is an interesting type
                            int index = FindOrAddAuto(type, false, false, false);
                            if (index >= 0)
                            {
                                tmp = ((MetaType)types[index]).GetSurrogateOrBaseOrSelf(false);
                                if (!list.Contains(tmp))
                                { // could perhaps also implement as a queue, but this should work OK for sane models
                                    list.Add(tmp);
                                    CascadeDependents(list, tmp);
                                }
                            }
                        }
                    }
                }
                if (metaType.HasSubtypes)
                {
                    foreach (SubType subType in metaType.GetSubtypes())
                    {
                        tmp = subType.DerivedType.GetSurrogateOrSelf(); // note: exclude base-types!
                        if (!list.Contains(tmp))
                        {
                            list.Add(tmp);
                            CascadeDependents(list, tmp);
                        }
                    }
                }
                tmp = metaType.BaseType;
                if (tmp != null) tmp = tmp.GetSurrogateOrSelf(); // note: already walking base-types; exclude base
                if (tmp != null && !list.Contains(tmp))
                {
                    list.Add(tmp);
                    CascadeDependents(list, tmp);
                }
            }
        }


        internal RuntimeTypeModel(bool isDefault)
        {
#if FEAT_IKVM
            universe = new IKVM.Reflection.Universe();
            universe.EnableMissingMemberResolution(); // needed to avoid TypedReference issue on WinRT
#endif
            AutoAddMissingTypes = true;
            UseImplicitZeroDefaults = true;
            SetOption(OPTIONS_IsDefaultModel, isDefault);
#if FEAT_COMPILER && !FX11 && !DEBUG
            AutoCompile = true;
#endif
        }

#if FEAT_IKVM
        readonly IKVM.Reflection.Universe universe;
        /// <summary>
        /// Load an assembly into the model's universe
        /// </summary>
        public Assembly Load(string path)
        {
            return universe.LoadFile(path);
        }
        /// <summary>
        /// Gets the IKVM Universe that relates to this model
        /// </summary>
        public Universe Universe { get { return universe; } }

        /// <summary>
        /// Adds support for an additional type in this model, optionally
        /// appplying inbuilt patterns. If the type is already known to the
        /// model, the existing type is returned **without** applying
        /// any additional behaviour.
        /// </summary>
        public MetaType Add(string assemblyQualifiedTypeName, bool applyDefaults)
        {
            Type type = universe.GetType(assemblyQualifiedTypeName, true);
            return Add(type, applyDefaults);
        }
        /// <summary>
        /// Adds support for an additional type in this model, optionally
        /// appplying inbuilt patterns. If the type is already known to the
        /// model, the existing type is returned **without** applying
        /// any additional behaviour.
        /// </summary>
        public MetaType Add(System.Type type, bool applyDefaultBehaviour)
        {
            return Add(MapType(type), applyDefaultBehaviour);
        }
        /// <summary>
        /// Obtains the MetaType associated with a given Type for the current model,
        /// allowing additional configuration.
        /// </summary>
        public MetaType this[System.Type type] { get { return this[MapType(type)]; } }
        
#endif

        /// <summary>
        /// Obtains the MetaType associated with a given Type for the current model,
        /// allowing additional configuration.
        /// </summary>
        public MetaType this[Type type] { get { return (MetaType)types[FindOrAddAuto(type, true, false, false)]; } }
        
        internal MetaType FindWithoutAdd(Type type)
        {
            // this list is thread-safe for reading
            foreach (MetaType metaType in types)
            {
                if (metaType.Type == type)
                {
                    if (metaType.Pending) WaitOnLock(metaType);
                    return metaType;
                }
            }
            // if that failed, check for a proxy
            Type underlyingType = ResolveProxies(type);
            return underlyingType == null ? null : FindWithoutAdd(underlyingType);
        }

        static readonly BasicList.MatchPredicate
            MetaTypeFinder = new BasicList.MatchPredicate(MetaTypeFinderImpl),
            BasicTypeFinder = new BasicList.MatchPredicate(BasicTypeFinderImpl);
        static bool MetaTypeFinderImpl(object value, object ctx)
        {
            return ((MetaType)value).Type == (Type)ctx;
        }
        static bool BasicTypeFinderImpl(object value, object ctx)
        {
            return ((BasicType)value).Type == (Type)ctx;
        }

        private void WaitOnLock(MetaType type)
        {
            int opaqueToken = 0;
            try
            {
                TakeLock(ref opaqueToken);
            }
            finally
            {
                ReleaseLock(opaqueToken);
            }
        }
        BasicList basicTypes = new BasicList();

        sealed class BasicType
        {
            private readonly Type type;
            public Type Type { get { return type; } }
            private readonly IProtoSerializer serializer;
            public IProtoSerializer Serializer { get { return serializer; } }
            public BasicType(Type type, IProtoSerializer serializer)
            {
                this.type = type;
                this.serializer = serializer;
            }
        }
        internal IProtoSerializer TryGetBasicTypeSerializer(Type type)
        {
            int idx = basicTypes.IndexOf(BasicTypeFinder, type);

            if (idx >= 0) return ((BasicType)basicTypes[idx]).Serializer;

            lock(basicTypes)
            { // don't need a full model lock for this

                // double-checked
                idx = basicTypes.IndexOf(BasicTypeFinder, type);
                if (idx >= 0) return ((BasicType)basicTypes[idx]).Serializer;

                WireType defaultWireType;
                MetaType.AttributeFamily family = MetaType.GetContractFamily(this, type, null);
                IProtoSerializer ser = family == MetaType.AttributeFamily.None
                    ? ValueMember.TryGetCoreSerializer(this, DataFormat.Default, type, out defaultWireType, false, false, false, false)
                    : null;

                if(ser != null) basicTypes.Add(new BasicType(type, ser));
                return ser;
            }

        }

        internal int FindOrAddAuto(Type type, bool demand, bool addWithContractOnly, bool addEvenIfAutoDisabled)
        {
            int key = types.IndexOf(MetaTypeFinder, type);
            MetaType metaType;

            // the fast happy path: meta-types we've already seen
            if (key >= 0)
            {
                metaType = (MetaType)types[key];
                if (metaType.Pending)
                {
                    WaitOnLock(metaType);
                }
                return key;
            }

            // the fast fail path: types that will never have a meta-type
            bool shouldAdd = AutoAddMissingTypes || addEvenIfAutoDisabled;

            if (!Helpers.IsEnum(type) && TryGetBasicTypeSerializer(type) != null)
            {
                if (shouldAdd && !addWithContractOnly) throw MetaType.InbuiltType(type);
                return -1; // this will never be a meta-type
            }

            // otherwise: we don't yet know

            // check for proxy types
            Type underlyingType = ResolveProxies(type);
            if (underlyingType != null)
            {
                key = types.IndexOf(MetaTypeFinder, underlyingType);
                type = underlyingType; // if new added, make it reflect the underlying type
            }

            if (key < 0)
            {
                int opaqueToken = 0;
                try
                {
                    TakeLock(ref opaqueToken);
                    // try to recognise a few familiar patterns...
                    if ((metaType = RecogniseCommonTypes(type)) == null)
                    { // otherwise, check if it is a contract
                        MetaType.AttributeFamily family = MetaType.GetContractFamily(this, type, null);
                        if (family == MetaType.AttributeFamily.AutoTuple)
                        {
                            shouldAdd = addEvenIfAutoDisabled = true; // always add basic tuples, such as KeyValuePair
                        }

                        if (!shouldAdd || (
                            !Helpers.IsEnum(type) && addWithContractOnly && family == MetaType.AttributeFamily.None)
                            )
                        {
                            if (demand) ThrowUnexpectedType(type);
                            return key;
                        }
                        metaType = Create(type);
                    }
                    metaType.Pending = true;                    
                    bool weAdded = false;

                    // double-checked
                    int winner = types.IndexOf(MetaTypeFinder, type);
                    if (winner < 0)
                    {
                        ThrowIfFrozen();
                        key = types.Add(metaType);
                        weAdded = true;
                    }
                    else
                    {
                        key = winner;
                    }
                    if (weAdded)
                    {
                        metaType.ApplyDefaultBehaviour();
                        metaType.Pending = false;
                    }
                }
                finally
                {
                    ReleaseLock(opaqueToken);
                }
            }
            return key;
        }

        private MetaType RecogniseCommonTypes(Type type)
        {
//#if !NO_GENERICS
//            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(System.Collections.Generic.KeyValuePair<,>))
//            {
//                MetaType mt = new MetaType(this, type);

//                Type surrogate = typeof (KeyValuePairSurrogate<,>).MakeGenericType(type.GetGenericArguments());

//                mt.SetSurrogate(surrogate);
//                mt.IncludeSerializerMethod = false;
//                mt.Freeze();

//                MetaType surrogateMeta = (MetaType)types[FindOrAddAuto(surrogate, true, true, true)]; // this forcibly adds it if needed
//                if(surrogateMeta.IncludeSerializerMethod)
//                { // don't blindly set - it might be frozen
//                    surrogateMeta.IncludeSerializerMethod = false;
//                }
//                surrogateMeta.Freeze();
//                return mt;
//            }
//#endif
            return null;
        }
        private MetaType Create(Type type)
        {
            ThrowIfFrozen();
            return new MetaType(this, type, defaultFactory);
        }

        /// <summary>
        /// Adds support for an additional type in this model, optionally
        /// appplying inbuilt patterns. If the type is already known to the
        /// model, the existing type is returned **without** applying
        /// any additional behaviour.
        /// </summary>
        /// <remarks>Inbuilt patterns include:
        /// [ProtoContract]/[ProtoMember(n)]
        /// [DataContract]/[DataMember(Order=n)]
        /// [XmlType]/[XmlElement(Order=n)]
        /// [On{Des|S}erializ{ing|ed}]
        /// ShouldSerialize*/*Specified
        /// </remarks>
        /// <param name="type">The type to be supported</param>
        /// <param name="applyDefaultBehaviour">Whether to apply the inbuilt configuration patterns (via attributes etc), or
        /// just add the type with no additional configuration (the type must then be manually configured).</param>
        /// <returns>The MetaType representing this type, allowing
        /// further configuration.</returns>
        public MetaType Add(Type type, bool applyDefaultBehaviour)
        {
            if (type == null) throw new ArgumentNullException("type");
            MetaType newType = FindWithoutAdd(type);
            if (newType != null) return newType; // return existing
            int opaqueToken = 0;
            
#if WINRT
            System.Reflection.TypeInfo typeInfo = System.Reflection.IntrospectionExtensions.GetTypeInfo(type);
            if (typeInfo.IsInterface && MetaType.ienumerable.IsAssignableFrom(typeInfo)
#else
            if (type.IsInterface && MapType(MetaType.ienumerable).IsAssignableFrom(type)
#endif
                    && GetListItemType(this, type) == null)
            {
                throw new ArgumentException("IEnumerable[<T>] data cannot be used as a meta-type unless an Add method can be resolved");
            }
            try
            {
                newType = RecogniseCommonTypes(type);
                if(newType != null)
                {
                    if(!applyDefaultBehaviour) {
                        throw new ArgumentException(
                            "Default behaviour must be observed for certain types with special handling; " + type.FullName,
                            "applyDefaultBehaviour");
                    }
                    // we should assume that type is fully configured, though; no need to re-run:
                    applyDefaultBehaviour = false;
                }
                if(newType == null) newType = Create(type);
                newType.Pending = true;
                TakeLock(ref opaqueToken);
                // double checked
                if (FindWithoutAdd(type) != null) throw new ArgumentException("Duplicate type", "type");
                ThrowIfFrozen();
                types.Add(newType);
                if (applyDefaultBehaviour) { newType.ApplyDefaultBehaviour(); }
                newType.Pending = false;
            }
            finally
            {
                ReleaseLock(opaqueToken);
            }
            
            return newType;
        }

#if FEAT_COMPILER && !FX11
        /// <summary>
        /// Should serializers be compiled on demand? It may be useful
        /// to disable this for debugging purposes.
        /// </summary>
        public bool AutoCompile
        {
            get { return GetOption(OPTIONS_AutoCompile); }
            set { SetOption(OPTIONS_AutoCompile, value); }
        }
#endif
        /// <summary>
        /// Should support for unexpected types be added automatically?
        /// If false, an exception is thrown when unexpected types
        /// are encountered.
        /// </summary>
        public bool AutoAddMissingTypes
        {
            get { return GetOption(OPTIONS_AutoAddMissingTypes); }
            set {
                if (!value && GetOption(OPTIONS_IsDefaultModel))
                {
                    throw new InvalidOperationException("The default model must allow missing types");
                }
                ThrowIfFrozen();
                SetOption(OPTIONS_AutoAddMissingTypes, value);
            }
        }
        /// <summary>
        /// Verifies that the model is still open to changes; if not, an exception is thrown
        /// </summary>
        private void ThrowIfFrozen()
        {
            if (GetOption(OPTIONS_Frozen)) throw new InvalidOperationException("The model cannot be changed once frozen");
        }
        /// <summary>
        /// Prevents further changes to this model
        /// </summary>
        public void Freeze()
        {
            if (GetOption(OPTIONS_IsDefaultModel)) throw new InvalidOperationException("The default model cannot be frozen");
            SetOption(OPTIONS_Frozen, true);
        }

        private readonly BasicList types = new BasicList();

        /// <summary>
        /// Provides the key that represents a given type in the current model.
        /// </summary>
        protected override int GetKeyImpl(Type type)
        {
            return GetKey(type, false, true);
        }
        internal int GetKey(Type type, bool demand, bool getBaseKey)
        {
            Helpers.DebugAssert(type != null);
            try
            {
                int typeIndex = FindOrAddAuto(type, demand, true, false);
                if (typeIndex >= 0)
                {
                    MetaType mt = (MetaType)types[typeIndex];
                    if (getBaseKey)
                    {
                        mt = MetaType.GetRootType(mt);
                        typeIndex = FindOrAddAuto(mt.Type, true, true, false);                        
                    }
                }
                return typeIndex;
            }
            catch (NotSupportedException)
            {
                throw; // re-surface "as-is"
            }
            catch (Exception ex)
            {
                if (ex.Message.IndexOf(type.FullName) >= 0) throw;  // already enough info
                throw new ProtoException(ex.Message + " (" + type.FullName + ")", ex);
            }
        }
        /// <summary>
        /// Writes a protocol-buffer representation of the given instance to the supplied stream.
        /// </summary>
        /// <param name="key">Represents the type (including inheritance) to consider.</param>
        /// <param name="value">The existing instance to be serialized (cannot be null).</param>
        /// <param name="dest">The destination stream to write to.</param>
        protected internal override void Serialize(int key, object value, ProtoWriter dest)
        {
#if FEAT_IKVM
            throw new NotSupportedException();
#else
            //Helpers.DebugWriteLine("Serialize", value);
            ((MetaType)types[key]).Serializer.Write(value, dest);
#endif
        }
        /// <summary>
        /// Applies a protocol-buffer stream to an existing instance (which may be null).
        /// </summary>
        /// <param name="key">Represents the type (including inheritance) to consider.</param>
        /// <param name="value">The existing instance to be modified (can be null).</param>
        /// <param name="source">The binary stream to apply to the instance (cannot be null).</param>
        /// <returns>The updated instance; this may be different to the instance argument if
        /// either the original instance was null, or the stream defines a known sub-type of the
        /// original instance.</returns>
        protected internal override object Deserialize(int key, object value, ProtoReader source)
        {
#if FEAT_IKVM
            throw new NotSupportedException();
#else
            //Helpers.DebugWriteLine("Deserialize", value);
            IProtoSerializer ser = ((MetaType)types[key]).Serializer;
            if (value == null && Helpers.IsValueType(ser.ExpectedType)) {
                if(ser.RequiresOldValue) value = Activator.CreateInstance(ser.ExpectedType);
                return ser.Read(value, source);
            } else {
                return ser.Read(value, source);
            }
#endif
        }

#if FEAT_COMPILER
        // this is used by some unit-tests; do not remove
        internal Compiler.ProtoSerializer GetSerializer(IProtoSerializer serializer, bool compiled)
        {
#if FEAT_IKVM
            throw new NotSupportedException();
#else
            if (serializer == null) throw new ArgumentNullException("serializer");
#if FEAT_COMPILER && !FX11
            if (compiled) return Compiler.CompilerContext.BuildSerializer(serializer, this);
#endif
            return new Compiler.ProtoSerializer(serializer.Write);
#endif
        }

#if !FX11
        /// <summary>
        /// Compiles the serializers individually; this is *not* a full
        /// standalone compile, but can significantly boost performance
        /// while allowing additional types to be added.
        /// </summary>
        /// <remarks>An in-place compile can access non-public types / members</remarks>
        public void CompileInPlace()
        {
            foreach (MetaType type in types)
            {
                type.CompileInPlace();
            }
        }
#endif
#endif
        //internal override IProtoSerializer GetTypeSerializer(Type type)
        //{   // this list is thread-safe for reading
        //    .Serializer;
        //}
        //internal override IProtoSerializer GetTypeSerializer(int key)
        //{   // this list is thread-safe for reading
        //    MetaType type = (MetaType)types.TryGet(key);
        //    if (type != null) return type.Serializer;
        //    throw new KeyNotFoundException();

        //}

        
#if FEAT_COMPILER
        private void BuildAllSerializers()
        {
            // note that types.Count may increase during this operation, as some serializers
            // bring other types into play
            for (int i = 0; i < types.Count; i++)
            {
                // the primary purpose of this is to force the creation of the Serializer
                MetaType mt = (MetaType)types[i];
                if (mt.Serializer == null)
                    throw new InvalidOperationException("No serializer available for " + mt.Type.Name);
            }
        }
#if !SILVERLIGHT
        internal sealed class SerializerPair : IComparable
        {
            int IComparable.CompareTo(object obj)
            {
                if (obj == null) throw new ArgumentException("obj");
                SerializerPair other = (SerializerPair)obj;

                // we want to bunch all the items with the same base-type together, but we need the items with a
                // different base **first**.
                if (this.BaseKey == this.MetaKey)
                {
                    if (other.BaseKey == other.MetaKey)
                    { // neither is a subclass
                        return this.MetaKey.CompareTo(other.MetaKey);
                    }
                    else
                    { // "other" (only) is involved in inheritance; "other" should be first
                        return 1;
                    }
                }
                else
                {
                    if (other.BaseKey == other.MetaKey)
                    { // "this" (only) is involved in inheritance; "this" should be first
                        return -1;
                    }
                    else
                    { // both are involved in inheritance
                        int result = this.BaseKey.CompareTo(other.BaseKey);
                        if (result == 0) result = this.MetaKey.CompareTo(other.MetaKey);
                        return result;
                    }
                }
            }
            public readonly int MetaKey, BaseKey;
            public readonly MetaType Type;
            public readonly MethodBuilder Serialize, Deserialize;
            public readonly ILGenerator SerializeBody, DeserializeBody;
            public SerializerPair(int metaKey, int baseKey, MetaType type, MethodBuilder serialize, MethodBuilder deserialize,
                ILGenerator serializeBody, ILGenerator deserializeBody)
            {
                this.MetaKey = metaKey;
                this.BaseKey = baseKey;
                this.Serialize = serialize;
                this.Deserialize = deserialize;
                this.SerializeBody = serializeBody;
                this.DeserializeBody = deserializeBody;
                this.Type = type;
            }
        }

        /// <summary>
        /// Fully compiles the current model into a static-compiled model instance
        /// </summary>
        /// <remarks>A full compilation is restricted to accessing public types / members</remarks>
        /// <returns>An instance of the newly created compiled type-model</returns>
        public TypeModel Compile()
        {
            return Compile(null, null);
        }
        static ILGenerator Override(TypeBuilder type, string name)
        {
            MethodInfo baseMethod = type.BaseType.GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance);

            ParameterInfo[] parameters = baseMethod.GetParameters();
            Type[] paramTypes = new Type[parameters.Length];
            for(int i = 0 ; i < paramTypes.Length ; i++) {
                paramTypes[i] = parameters[i].ParameterType;
            }
            MethodBuilder newMethod = type.DefineMethod(baseMethod.Name,
                (baseMethod.Attributes & ~MethodAttributes.Abstract) | MethodAttributes.Final, baseMethod.CallingConvention, baseMethod.ReturnType, paramTypes);
            ILGenerator il = newMethod.GetILGenerator();
            type.DefineMethodOverride(newMethod, baseMethod);
            return il;
        }

#if FEAT_IKVM
        /// <summary>
        /// Inspect the model, and resolve all related types
        /// </summary>
        public void Cascade()
        {
            BuildAllSerializers();
        }
        /// <summary>
        /// Translate a System.Type into the universe's type representation
        /// </summary>
        protected internal override Type MapType(System.Type type, bool demand)
        {
            if (type == null) return null;
#if DEBUG
            if (type.Assembly == typeof(IKVM.Reflection.Type).Assembly)
            {
                throw new InvalidOperationException(string.Format(
                    "Somebody is passing me IKVM types! {0} should be fully-qualified at the call-site",
                    type.Name));
            }
#endif
            Type result = universe.GetType(type.AssemblyQualifiedName);
            
            if(result == null)
            {
                // things also tend to move around... *a lot* - especially in WinRT; search all as a fallback strategy
                foreach (Assembly a in universe.GetAssemblies())
                {
                    result = a.GetType(type.FullName);
                    if (result != null) break;
                }
                if (result == null && demand)
                {
                    throw new InvalidOperationException("Unable to map type: " + type.AssemblyQualifiedName);
                }
            }
            return result;
        }
#endif
        /// <summary>
        /// Represents configuration options for compiling a model to 
        /// a standalone assembly.
        /// </summary>
        public sealed class CompilerOptions
        {
            /// <summary>
            /// Import framework options from an existing type
            /// </summary>
            public void SetFrameworkOptions(MetaType from)
            {
                if (from == null) throw new ArgumentNullException("from");
                AttributeMap[] attribs = AttributeMap.Create(from.Model, from.Type.Assembly);
                foreach (AttributeMap attrib in attribs)
                {
                    if (attrib.AttributeType.FullName == "System.Runtime.Versioning.TargetFrameworkAttribute")
                    {
                        object tmp;
                        if (attrib.TryGet("FrameworkName", out tmp)) TargetFrameworkName = (string)tmp;
                        if (attrib.TryGet("FrameworkDisplayName", out tmp)) TargetFrameworkDisplayName = (string)tmp;
                        break;
                    }
                }
            }

            private string targetFrameworkName, targetFrameworkDisplayName, typeName, outputPath, imageRuntimeVersion;
            private int metaDataVersion;
            /// <summary>
            /// The TargetFrameworkAttribute FrameworkName value to burn into the generated assembly
            /// </summary>
            public string TargetFrameworkName { get { return targetFrameworkName; } set { targetFrameworkName = value; } }

            /// <summary>
            /// The TargetFrameworkAttribute FrameworkDisplayName value to burn into the generated assembly
            /// </summary>
            public string TargetFrameworkDisplayName { get { return targetFrameworkDisplayName; } set { targetFrameworkDisplayName = value; } }
            /// <summary>
            /// The name of the TypeModel class to create
            /// </summary>
            public string TypeName { get { return typeName; } set { typeName = value; } }
            /// <summary>
            /// The path for the new dll
            /// </summary>
            public string OutputPath { get { return outputPath; } set { outputPath = value; } }
            /// <summary>
            /// The runtime version for the generated assembly
            /// </summary>
            public string ImageRuntimeVersion { get { return imageRuntimeVersion; } set { imageRuntimeVersion = value; } }
            /// <summary>
            /// The runtime version for the generated assembly
            /// </summary>
            public int MetaDataVersion { get { return metaDataVersion; } set { metaDataVersion = value; } }


            private Accessibility accessibility = Accessibility.Public;
            /// <summary>
            /// The acecssibility of the generated serializer
            /// </summary>
            public Accessibility Accessibility { get { return accessibility; } set { accessibility = value; } }

#if FEAT_IKVM
            /// <summary>
            /// The name of the container that holds the key pair.
            /// </summary>
            public string KeyContainer { get; set; }
            /// <summary>
            /// The path to a file that hold the key pair.
            /// </summary>
            public string KeyFile { get; set; }

            /// <summary>
            /// The public  key to sign the file with.
            /// </summary>
            public string PublicKey { get; set; }
#endif
        }
        /// <summary>
        /// Type accessibility
        /// </summary>
        public enum Accessibility
        {
            /// <summary>
            /// Available to all callers
            /// </summary>
            Public,
            /// <summary>
            /// Available to all callers in the same assembly, or assemblies specified via [InternalsVisibleTo(...)]
            /// </summary>
            Internal
        }
        /// <summary>
        /// Fully compiles the current model into a static-compiled serialization dll
        /// (the serialization dll still requires protobuf-net for support services).
        /// </summary>
        /// <remarks>A full compilation is restricted to accessing public types / members</remarks>
        /// <param name="name">The name of the TypeModel class to create</param>
        /// <param name="path">The path for the new dll</param>
        /// <returns>An instance of the newly created compiled type-model</returns>
        public TypeModel Compile(string name, string path)
        {
            CompilerOptions options = new CompilerOptions();
            options.TypeName = name;
            options.OutputPath = path;
            return Compile(options);
        }

        /// <summary>
        /// Fully compiles the current model into a static-compiled serialization dll
        /// (the serialization dll still requires protobuf-net for support services).
        /// </summary>
        /// <remarks>A full compilation is restricted to accessing public types / members</remarks>
        /// <returns>An instance of the newly created compiled type-model</returns>
        public TypeModel Compile(CompilerOptions options)
        {
            if (options == null) throw new ArgumentNullException("options");
            string typeName = options.TypeName;
            string path = options.OutputPath;
            BuildAllSerializers();
            Freeze();
            bool save = !Helpers.IsNullOrEmpty(path);
            if (Helpers.IsNullOrEmpty(typeName))
            {
                if (save) throw new ArgumentNullException("typeName");
                typeName = Guid.NewGuid().ToString();
            }


            string assemblyName, moduleName;
            if(path == null)
            {
                assemblyName = typeName;
                moduleName = assemblyName + ".dll";
            }
            else
            {
                assemblyName = new System.IO.FileInfo(System.IO.Path.GetFileNameWithoutExtension(path)).Name;
                moduleName = assemblyName + System.IO.Path.GetExtension(path);
            }

#if FEAT_IKVM
            IKVM.Reflection.AssemblyName an = new IKVM.Reflection.AssemblyName();
            an.Name = assemblyName;
            AssemblyBuilder asm = universe.DefineDynamicAssembly(an, AssemblyBuilderAccess.Save);
            if (!Helpers.IsNullOrEmpty(options.KeyFile))
            {
                asm.__SetAssemblyKeyPair(new StrongNameKeyPair(File.OpenRead(options.KeyFile)));
            }
            else if (!Helpers.IsNullOrEmpty(options.KeyContainer))
            {
                asm.__SetAssemblyKeyPair(new StrongNameKeyPair(options.KeyContainer));
            }
            else if (!Helpers.IsNullOrEmpty(options.PublicKey))
            {
                asm.__SetAssemblyPublicKey(FromHex(options.PublicKey));
            }
            if(!Helpers.IsNullOrEmpty(options.ImageRuntimeVersion) && options.MetaDataVersion != 0)
            {
                asm.__SetImageRuntimeVersion(options.ImageRuntimeVersion, options.MetaDataVersion);
            }
            ModuleBuilder module = asm.DefineDynamicModule(moduleName, path);
#else
            AssemblyName an = new AssemblyName();
            an.Name = assemblyName;
            AssemblyBuilder asm = AppDomain.CurrentDomain.DefineDynamicAssembly(an,
                (save ? AssemblyBuilderAccess.RunAndSave : AssemblyBuilderAccess.Run)
                );
            ModuleBuilder module = save ? asm.DefineDynamicModule(moduleName, path)
                                        : asm.DefineDynamicModule(moduleName);
#endif

            WriteAssemblyAttributes(options, assemblyName, asm);

            TypeBuilder type = WriteBasicTypeModel(options, typeName, module);

            int index;
            bool hasInheritance;
            SerializerPair[] methodPairs;
            Compiler.CompilerContext.ILVersion ilVersion;
            WriteSerializers(options, assemblyName, type, out index, out hasInheritance, out methodPairs, out ilVersion);

            ILGenerator il;
            int knownTypesCategory;
            FieldBuilder knownTypes;
            Type knownTypesLookupType;
            WriteGetKeyImpl(type, hasInheritance, methodPairs, ilVersion, assemblyName, out il, out knownTypesCategory, out knownTypes, out knownTypesLookupType);

            // trivial flags
            il = Override(type, "SerializeDateTimeKind");
            il.Emit(IncludeDateTimeKind ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Ret);
            // end: trivial flags

            Compiler.CompilerContext ctx = WriteSerializeDeserialize(assemblyName, type, methodPairs, ilVersion, ref il);

            WriteConstructors(type, ref index, methodPairs, ref il, knownTypesCategory, knownTypes, knownTypesLookupType, ctx);
            


            Type finalType = type.CreateType();
            if(!Helpers.IsNullOrEmpty(path))
            {
                asm.Save(path);
                Helpers.DebugWriteLine("Wrote dll:" + path);
            }
#if FEAT_IKVM
            return null;
#else
            return (TypeModel)Activator.CreateInstance(finalType);
#endif
        }
#if FEAT_IKVM
        private byte[] FromHex(string value)
        {
            if (Helpers.IsNullOrEmpty(value)) throw new ArgumentNullException("value");
            int len = value.Length / 2;
            byte[] result = new byte[len];
            for(int i = 0 ; i < len ; i++)
            {
                result[i] = Convert.ToByte(value.Substring(i * 2, 2), 16);
            }
            return result;
        }
#endif
        private void WriteConstructors(TypeBuilder type, ref int index, SerializerPair[] methodPairs, ref ILGenerator il, int knownTypesCategory, FieldBuilder knownTypes, Type knownTypesLookupType, Compiler.CompilerContext ctx)
        {
            type.DefineDefaultConstructor(MethodAttributes.Public);
            il = type.DefineTypeInitializer().GetILGenerator();
            switch (knownTypesCategory)
            {
                case KnownTypes_Array:
                    {
                        Compiler.CompilerContext.LoadValue(il, types.Count);
                        il.Emit(OpCodes.Newarr, ctx.MapType(typeof(System.Type)));
                        index = 0;
                        foreach (SerializerPair pair in methodPairs)
                        {
                            il.Emit(OpCodes.Dup);
                            Compiler.CompilerContext.LoadValue(il, index);
                            il.Emit(OpCodes.Ldtoken, pair.Type.Type);
                            il.EmitCall(OpCodes.Call, ctx.MapType(typeof(System.Type)).GetMethod("GetTypeFromHandle"), null);
                            il.Emit(OpCodes.Stelem_Ref);
                            index++;
                        }
                        il.Emit(OpCodes.Stsfld, knownTypes);
                        il.Emit(OpCodes.Ret);
                    }
                    break;
                case KnownTypes_Dictionary:
                    {
                        Compiler.CompilerContext.LoadValue(il, types.Count);
                        //LocalBuilder loc = il.DeclareLocal(knownTypesLookupType);
                        il.Emit(OpCodes.Newobj, knownTypesLookupType.GetConstructor(new Type[] { MapType(typeof(int)) }));
                        il.Emit(OpCodes.Stsfld, knownTypes);
                        int typeIndex = 0;
                        foreach (SerializerPair pair in methodPairs)
                        {
                            il.Emit(OpCodes.Ldsfld, knownTypes);
                            il.Emit(OpCodes.Ldtoken, pair.Type.Type);
                            il.EmitCall(OpCodes.Call, ctx.MapType(typeof(System.Type)).GetMethod("GetTypeFromHandle"), null);
                            int keyIndex = typeIndex++, lastKey = pair.BaseKey;
                            if (lastKey != pair.MetaKey) // not a base-type; need to give the index of the base-type
                            {
                                keyIndex = -1; // assume epic fail
                                for (int j = 0; j < methodPairs.Length; j++)
                                {
                                    if (methodPairs[j].BaseKey == lastKey && methodPairs[j].MetaKey == lastKey)
                                    {
                                        keyIndex = j;
                                        break;
                                    }
                                }
                            }
                            Compiler.CompilerContext.LoadValue(il, keyIndex);
                            il.EmitCall(OpCodes.Callvirt, knownTypesLookupType.GetMethod("Add", new Type[] { MapType(typeof(System.Type)), MapType(typeof(int)) }), null);
                        }
                        il.Emit(OpCodes.Ret);
                    }
                    break;
                case KnownTypes_Hashtable:
                    {
                        Compiler.CompilerContext.LoadValue(il, types.Count);
                        il.Emit(OpCodes.Newobj, knownTypesLookupType.GetConstructor(new Type[] { MapType(typeof(int)) }));
                        il.Emit(OpCodes.Stsfld, knownTypes);
                        int typeIndex = 0;
                        foreach (SerializerPair pair in methodPairs)
                        {
                            il.Emit(OpCodes.Ldsfld, knownTypes);
                            il.Emit(OpCodes.Ldtoken, pair.Type.Type);
                            il.EmitCall(OpCodes.Call, ctx.MapType(typeof(System.Type)).GetMethod("GetTypeFromHandle"), null);
                            int keyIndex = typeIndex++, lastKey = pair.BaseKey;
                            if (lastKey != pair.MetaKey) // not a base-type; need to give the index of the base-type
                            {
                                keyIndex = -1; // assume epic fail
                                for (int j = 0; j < methodPairs.Length; j++)
                                {
                                    if (methodPairs[j].BaseKey == lastKey && methodPairs[j].MetaKey == lastKey)
                                    {
                                        keyIndex = j;
                                        break;
                                    }
                                }
                            }
                            Compiler.CompilerContext.LoadValue(il, keyIndex);
                            il.Emit(OpCodes.Box, MapType(typeof(int)));
                            il.EmitCall(OpCodes.Callvirt, knownTypesLookupType.GetMethod("Add", new Type[] { MapType(typeof(object)), MapType(typeof(object)) }), null);
                        }
                        il.Emit(OpCodes.Ret);
                    }
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }

        private Compiler.CompilerContext WriteSerializeDeserialize(string assemblyName, TypeBuilder type, SerializerPair[] methodPairs, Compiler.CompilerContext.ILVersion ilVersion, ref ILGenerator il)
        {
            il = Override(type, "Serialize");
            Compiler.CompilerContext ctx = new Compiler.CompilerContext(il, false, true, methodPairs, this, ilVersion, assemblyName, MapType(typeof(object)));
            // arg0 = this, arg1 = key, arg2=obj, arg3=dest
            Compiler.CodeLabel[] jumpTable = new Compiler.CodeLabel[types.Count];
            for (int i = 0; i < jumpTable.Length; i++)
            {
                jumpTable[i] = ctx.DefineLabel();
            }
            il.Emit(OpCodes.Ldarg_1);
            ctx.Switch(jumpTable);
            ctx.Return();
            for (int i = 0; i < jumpTable.Length; i++)
            {
                SerializerPair pair = methodPairs[i];
                ctx.MarkLabel(jumpTable[i]);
                il.Emit(OpCodes.Ldarg_2);
                ctx.CastFromObject(pair.Type.Type);
                il.Emit(OpCodes.Ldarg_3);
                il.EmitCall(OpCodes.Call, pair.Serialize, null);
                ctx.Return();
            }

            il = Override(type, "Deserialize");
            ctx = new Compiler.CompilerContext(il, false, false, methodPairs, this, ilVersion, assemblyName, MapType(typeof(object)));
            // arg0 = this, arg1 = key, arg2=obj, arg3=source
            for (int i = 0; i < jumpTable.Length; i++)
            {
                jumpTable[i] = ctx.DefineLabel();
            }
            il.Emit(OpCodes.Ldarg_1);
            ctx.Switch(jumpTable);
            ctx.LoadNullRef();
            ctx.Return();
            for (int i = 0; i < jumpTable.Length; i++)
            {
                SerializerPair pair = methodPairs[i];
                ctx.MarkLabel(jumpTable[i]);
                Type keyType = pair.Type.Type;
                if (keyType.IsValueType)
                {
                    il.Emit(OpCodes.Ldarg_2);
                    il.Emit(OpCodes.Ldarg_3);
                    il.EmitCall(OpCodes.Call, EmitBoxedSerializer(type, i, keyType, methodPairs, this, ilVersion, assemblyName), null);
                    ctx.Return();
                }
                else
                {
                    il.Emit(OpCodes.Ldarg_2);
                    ctx.CastFromObject(keyType);
                    il.Emit(OpCodes.Ldarg_3);
                    il.EmitCall(OpCodes.Call, pair.Deserialize, null);
                    ctx.Return();
                }
            }
            return ctx;
        }

        private const int KnownTypes_Array = 1, KnownTypes_Dictionary = 2, KnownTypes_Hashtable = 3, KnownTypes_ArrayCutoff = 20;
        private void WriteGetKeyImpl(TypeBuilder type, bool hasInheritance, SerializerPair[] methodPairs, Compiler.CompilerContext.ILVersion ilVersion, string assemblyName, out ILGenerator il, out int knownTypesCategory, out FieldBuilder knownTypes, out Type knownTypesLookupType)
        {

            il = Override(type, "GetKeyImpl");
            Compiler.CompilerContext ctx = new Compiler.CompilerContext(il, false, false, methodPairs, this, ilVersion, assemblyName, MapType(typeof(System.Type), true));
            
            if (types.Count <= KnownTypes_ArrayCutoff)
            {
                knownTypesCategory = KnownTypes_Array;
                knownTypesLookupType = MapType(typeof(System.Type[]), true);
            }
            else
            {
#if NO_GENERICS
                knownTypesLookupType = null;
#else
                knownTypesLookupType = MapType(typeof(System.Collections.Generic.Dictionary<System.Type, int>), false);
#endif
                if (knownTypesLookupType == null)
                {
                    knownTypesLookupType = MapType(typeof(Hashtable), true);
                    knownTypesCategory = KnownTypes_Hashtable;
                }
                else
                {
                    knownTypesCategory = KnownTypes_Dictionary;
                }
            }
            knownTypes = type.DefineField("knownTypes", knownTypesLookupType, FieldAttributes.Private | FieldAttributes.InitOnly | FieldAttributes.Static);

            switch (knownTypesCategory)
            {
                case KnownTypes_Array:
                    {
                        il.Emit(OpCodes.Ldsfld, knownTypes);
                        il.Emit(OpCodes.Ldarg_1);
                        // note that Array.IndexOf is not supported under CF
                        il.EmitCall(OpCodes.Callvirt, MapType(typeof(IList)).GetMethod(
                            "IndexOf", new Type[] { MapType(typeof(object)) }), null);
                        if (hasInheritance)
                        {
                            il.DeclareLocal(MapType(typeof(int))); // loc-0
                            il.Emit(OpCodes.Dup);
                            il.Emit(OpCodes.Stloc_0);

                            BasicList getKeyLabels = new BasicList();
                            int lastKey = -1;
                            for (int i = 0; i < methodPairs.Length; i++)
                            {
                                if (methodPairs[i].MetaKey == methodPairs[i].BaseKey) break;
                                if (lastKey == methodPairs[i].BaseKey)
                                {   // add the last label again
                                    getKeyLabels.Add(getKeyLabels[getKeyLabels.Count - 1]);
                                }
                                else
                                {   // add a new unique label
                                    getKeyLabels.Add(ctx.DefineLabel());
                                    lastKey = methodPairs[i].BaseKey;
                                }
                            }
                            Compiler.CodeLabel[] subtypeLabels = new Compiler.CodeLabel[getKeyLabels.Count];
                            getKeyLabels.CopyTo(subtypeLabels, 0);

                            ctx.Switch(subtypeLabels);
                            il.Emit(OpCodes.Ldloc_0); // not a sub-type; use the original value
                            il.Emit(OpCodes.Ret);

                            lastKey = -1;
                            // now output the different branches per sub-type (not derived type)
                            for (int i = subtypeLabels.Length - 1; i >= 0; i--)
                            {
                                if (lastKey != methodPairs[i].BaseKey)
                                {
                                    lastKey = methodPairs[i].BaseKey;
                                    // find the actual base-index for this base-key (i.e. the index of
                                    // the base-type)
                                    int keyIndex = -1;
                                    for (int j = subtypeLabels.Length; j < methodPairs.Length; j++)
                                    {
                                        if (methodPairs[j].BaseKey == lastKey && methodPairs[j].MetaKey == lastKey)
                                        {
                                            keyIndex = j;
                                            break;
                                        }
                                    }
                                    ctx.MarkLabel(subtypeLabels[i]);
                                    Compiler.CompilerContext.LoadValue(il, keyIndex);
                                    il.Emit(OpCodes.Ret);
                                }
                            }
                        }
                        else
                        {
                            il.Emit(OpCodes.Ret);
                        }
                    }
                    break;
                case KnownTypes_Dictionary:
                    {
                        LocalBuilder result = il.DeclareLocal(MapType(typeof(int)));
                        Label otherwise = il.DefineLabel();
                        il.Emit(OpCodes.Ldsfld, knownTypes);
                        il.Emit(OpCodes.Ldarg_1);
                        il.Emit(OpCodes.Ldloca_S, result);
                        il.EmitCall(OpCodes.Callvirt, knownTypesLookupType.GetMethod("TryGetValue", BindingFlags.Instance | BindingFlags.Public), null);
                        il.Emit(OpCodes.Brfalse_S, otherwise);
                        il.Emit(OpCodes.Ldloc_S, result);
                        il.Emit(OpCodes.Ret);
                        il.MarkLabel(otherwise);
                        il.Emit(OpCodes.Ldc_I4_M1);
                        il.Emit(OpCodes.Ret);
                    }
                    break;
                case KnownTypes_Hashtable:
                    {
                        Label otherwise = il.DefineLabel();
                        il.Emit(OpCodes.Ldsfld, knownTypes);
                        il.Emit(OpCodes.Ldarg_1);
                        il.EmitCall(OpCodes.Callvirt, knownTypesLookupType.GetProperty("Item").GetGetMethod(), null);
                        il.Emit(OpCodes.Dup);
                        il.Emit(OpCodes.Brfalse_S, otherwise);
#if FX11
                        il.Emit(OpCodes.Unbox, MapType(typeof(int)));
                        il.Emit(OpCodes.Ldobj, MapType(typeof(int)));
#else
                        if (ilVersion == Compiler.CompilerContext.ILVersion.Net1)
                        {
                            il.Emit(OpCodes.Unbox, MapType(typeof(int)));
                            il.Emit(OpCodes.Ldobj, MapType(typeof(int)));
                        }
                        else
                        {
                            il.Emit(OpCodes.Unbox_Any, MapType(typeof(int)));
                        }
#endif
                        il.Emit(OpCodes.Ret);
                        il.MarkLabel(otherwise);
                        il.Emit(OpCodes.Pop);
                        il.Emit(OpCodes.Ldc_I4_M1);
                        il.Emit(OpCodes.Ret);
                    }
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }

        private void WriteSerializers(CompilerOptions options, string assemblyName, TypeBuilder type, out int index, out bool hasInheritance, out SerializerPair[] methodPairs, out Compiler.CompilerContext.ILVersion ilVersion)
        {
            Compiler.CompilerContext ctx;

            index = 0;
            hasInheritance = false;
            methodPairs = new SerializerPair[types.Count];
            foreach (MetaType metaType in types)
            {
                MethodBuilder writeMethod = type.DefineMethod("Write"
#if DEBUG
 + metaType.Type.Name
#endif
,
                    MethodAttributes.Private | MethodAttributes.Static, CallingConventions.Standard,
                    MapType(typeof(void)), new Type[] { metaType.Type, MapType(typeof(ProtoWriter)) });

                MethodBuilder readMethod = type.DefineMethod("Read"
#if DEBUG
 + metaType.Type.Name
#endif
,
                    MethodAttributes.Private | MethodAttributes.Static, CallingConventions.Standard,
                    metaType.Type, new Type[] { metaType.Type, MapType(typeof(ProtoReader)) });

                SerializerPair pair = new SerializerPair(
                    GetKey(metaType.Type, true, false), GetKey(metaType.Type, true, true), metaType,
                    writeMethod, readMethod, writeMethod.GetILGenerator(), readMethod.GetILGenerator());
                methodPairs[index++] = pair;
                if (pair.MetaKey != pair.BaseKey) hasInheritance = true;
            }

            if (hasInheritance)
            {
                Array.Sort(methodPairs);
            }

            ilVersion = Compiler.CompilerContext.ILVersion.Net2;
            if (options.MetaDataVersion == 0x10000)
            {
                ilVersion = Compiler.CompilerContext.ILVersion.Net1; // old-school!
            }
            for (index = 0; index < methodPairs.Length; index++)
            {
                SerializerPair pair = methodPairs[index];
                ctx = new Compiler.CompilerContext(pair.SerializeBody, true, true, methodPairs, this, ilVersion, assemblyName, pair.Type.Type);
                ctx.CheckAccessibility(pair.Deserialize.ReturnType);
                pair.Type.Serializer.EmitWrite(ctx, ctx.InputValue);
                ctx.Return();

                ctx = new Compiler.CompilerContext(pair.DeserializeBody, true, false, methodPairs, this, ilVersion, assemblyName, pair.Type.Type);
                pair.Type.Serializer.EmitRead(ctx, ctx.InputValue);
                if (!pair.Type.Serializer.ReturnsValue)
                {
                    ctx.LoadValue(ctx.InputValue);
                }
                ctx.Return();
            }
        }

        private TypeBuilder WriteBasicTypeModel(CompilerOptions options, string typeName, ModuleBuilder module)
        {
            Type baseType = MapType(typeof(TypeModel));
            TypeAttributes typeAttributes = (baseType.Attributes & ~TypeAttributes.Abstract) | TypeAttributes.Sealed;
            if (options.Accessibility == Accessibility.Internal)
            {
                typeAttributes &= ~TypeAttributes.Public;
            }

            TypeBuilder type = module.DefineType(typeName, typeAttributes, baseType);
            return type;
        }

        private void WriteAssemblyAttributes(CompilerOptions options, string assemblyName, AssemblyBuilder asm)
        {
            if (!Helpers.IsNullOrEmpty(options.TargetFrameworkName))
            {
                // get [TargetFramework] from mscorlib/equivalent and burn into the new assembly
                Type versionAttribType = null;
                try
                { // this is best-endeavours only
                    versionAttribType = GetType("System.Runtime.Versioning.TargetFrameworkAttribute", MapType(typeof(string)).Assembly);
                }
                catch { /* don't stress */ }
                if (versionAttribType != null)
                {
                    PropertyInfo[] props;
                    object[] propValues;
                    if (Helpers.IsNullOrEmpty(options.TargetFrameworkDisplayName))
                    {
                        props = new PropertyInfo[0];
                        propValues = new object[0];
                    }
                    else
                    {
                        props = new PropertyInfo[1] { versionAttribType.GetProperty("FrameworkDisplayName") };
                        propValues = new object[1] { options.TargetFrameworkDisplayName };
                    }
                    CustomAttributeBuilder builder = new CustomAttributeBuilder(
                        versionAttribType.GetConstructor(new Type[] { MapType(typeof(string)) }),
                        new object[] { options.TargetFrameworkName },
                        props,
                        propValues);
                    asm.SetCustomAttribute(builder);
                }
            }

            // copy assembly:InternalsVisibleTo
            Type internalsVisibleToAttribType = null;
#if !FX11
            try
            {
                internalsVisibleToAttribType = MapType(typeof(System.Runtime.CompilerServices.InternalsVisibleToAttribute));
            }
            catch { /* best endeavors only */ }
#endif
            if (internalsVisibleToAttribType != null)
            {
                BasicList internalAssemblies = new BasicList(), consideredAssemblies = new BasicList();
                foreach (MetaType metaType in types)
                {
                    Assembly assembly = metaType.Type.Assembly;
                    if (consideredAssemblies.IndexOfReference(assembly) >= 0) continue;
                    consideredAssemblies.Add(assembly);

                    AttributeMap[] assemblyAttribsMap = AttributeMap.Create(this, assembly);
                    for (int i = 0; i < assemblyAttribsMap.Length; i++)
                    {

                        if (assemblyAttribsMap[i].AttributeType != internalsVisibleToAttribType) continue;

                        object privelegedAssemblyObj;
                        assemblyAttribsMap[i].TryGet("AssemblyName", out privelegedAssemblyObj);
                        string privelegedAssemblyName = privelegedAssemblyObj as string;
                        if (privelegedAssemblyName == assemblyName || Helpers.IsNullOrEmpty(privelegedAssemblyName)) continue; // ignore

                        if (internalAssemblies.IndexOfString(privelegedAssemblyName) >= 0) continue; // seen it before
                        internalAssemblies.Add(privelegedAssemblyName);

                        CustomAttributeBuilder builder = new CustomAttributeBuilder(
                            internalsVisibleToAttribType.GetConstructor(new Type[] { MapType(typeof(string)) }),
                            new object[] { privelegedAssemblyName });
                        asm.SetCustomAttribute(builder);
                    }
                }
            }
        }

        private static MethodBuilder EmitBoxedSerializer(TypeBuilder type, int i, Type valueType, SerializerPair[] methodPairs, TypeModel model, Compiler.CompilerContext.ILVersion ilVersion, string assemblyName)
        {
            MethodInfo dedicated = methodPairs[i].Deserialize;
            MethodBuilder boxedSerializer = type.DefineMethod("_" + i.ToString(), MethodAttributes.Static, CallingConventions.Standard,
                model.MapType(typeof(object)), new Type[] { model.MapType(typeof(object)), model.MapType(typeof(ProtoReader)) });
            Compiler.CompilerContext ctx = new Compiler.CompilerContext(boxedSerializer.GetILGenerator(), true, false, methodPairs, model, ilVersion, assemblyName, model.MapType(typeof(object)));
            ctx.LoadValue(ctx.InputValue);
            Compiler.CodeLabel @null = ctx.DefineLabel();
            ctx.BranchIfFalse(@null, true);

            Type mappedValueType = valueType;
            ctx.LoadValue(ctx.InputValue);
            ctx.CastFromObject(mappedValueType);
            ctx.LoadReaderWriter();
            ctx.EmitCall(dedicated);
            ctx.CastToObject(mappedValueType);
            ctx.Return();

            ctx.MarkLabel(@null);
            using (Compiler.Local typedVal = new Compiler.Local(ctx, mappedValueType))
            {
                // create a new valueType
                ctx.LoadAddress(typedVal, mappedValueType);
                ctx.EmitCtor(mappedValueType);
                ctx.LoadValue(typedVal);
                ctx.LoadReaderWriter();
                ctx.EmitCall(dedicated);
                ctx.CastToObject(mappedValueType);
                ctx.Return();
            }
            return boxedSerializer;
        }
        
#endif
#endif
        //internal bool IsDefined(Type type, int fieldNumber)
        //{
        //    return FindWithoutAdd(type).IsDefined(fieldNumber);
        //}

        // note that this is used by some of the unit tests
        internal bool IsPrepared(Type type)
        {
            MetaType meta = FindWithoutAdd(type);
            return meta != null && meta.IsPrepared();
        }

        internal EnumSerializer.EnumPair[] GetEnumMap(Type type)
        {
            int index = FindOrAddAuto(type, false, false, false);
            return index < 0 ? null : ((MetaType)types[index]).GetEnumMap();
        }

        private int metadataTimeoutMilliseconds = 5000;
        /// <summary>
        /// The amount of time to wait if there are concurrent metadata access operations
        /// </summary>
        public int MetadataTimeoutMilliseconds
        {
            get { return metadataTimeoutMilliseconds; }
            set
            {
                if (value <= 0) throw new ArgumentOutOfRangeException("MetadataTimeoutMilliseconds");
                metadataTimeoutMilliseconds = value;
            }
        }

#if DEBUG
        int lockCount;
        /// <summary>
        /// Gets how many times a model lock was taken
        /// </summary>
        public int LockCount { get { return lockCount; } }
#endif
        internal void TakeLock(ref int opaqueToken)
        {
            const string message = "Timeout while inspecting metadata; this may indicate a deadlock. This can often be avoided by preparing necessary serializers during application initialization, rather than allowing multiple threads to perform the initial metadata inspection; please also see the LockContended event";
            opaqueToken = 0;
#if PORTABLE
            if(!Monitor.TryEnter(types)) throw new TimeoutException(message); // yes, we have to do this immediately - I'm not creating a "hot" loop, just because Sleep() doesn't exist...
            opaqueToken = Interlocked.CompareExchange(ref contentionCounter, 0, 0); // just fetch current value (starts at 1)
#elif CF2 || CF35
            int remaining = metadataTimeoutMilliseconds;
            bool lockTaken;
            do {
                lockTaken = Monitor.TryEnter(types);
                if(!lockTaken)
                {
                    if(remaining <= 0) throw new TimeoutException(message);
                    remaining -= 50;
                    Thread.Sleep(50);
                }
            } while(!lockTaken);
            opaqueToken = Interlocked.CompareExchange(ref contentionCounter, 0, 0); // just fetch current value (starts at 1)
#else
            if (Monitor.TryEnter(types, metadataTimeoutMilliseconds))
            {
                opaqueToken = GetContention(); // just fetch current value (starts at 1)
            }
            else
            {
                AddContention();
#if FX11
                throw new InvalidOperationException(message);
#else
                throw new TimeoutException(message);
#endif
            }
#endif

#if DEBUG // note that here, through all code-paths: we have the lock
            lockCount++;
#endif
        }

        private int contentionCounter = 1;
#if PLAT_NO_INTERLOCKED
        private readonly object contentionLock = new object();
#endif
        private int GetContention()
        {
#if PLAT_NO_INTERLOCKED
            lock(contentionLock)
            {
                return contentionCounter;
            }
#else
            return Interlocked.CompareExchange(ref contentionCounter, 0, 0);
#endif
        }
        private void AddContention()
        {
#if PLAT_NO_INTERLOCKED
            lock(contentionLock)
            {
                contentionCounter++;
            }
#else
            Interlocked.Increment(ref contentionCounter);
#endif
        }

        internal void ReleaseLock(int opaqueToken)
        {
            if (opaqueToken != 0)
            {
                Monitor.Exit(types);
                if(opaqueToken != GetContention()) // contention-count changes since we looked!
                {
                    LockContentedEventHandler handler = LockContended;
                    if (handler != null)
                    {
                        // not hugely elegant, but this is such a far-corner-case that it doesn't need to be slick - I'll settle for cross-platform
                        string stackTrace;
                        try
                        {
                            throw new ProtoException();
                        }
                        catch(Exception ex)
                        {
                            stackTrace = ex.StackTrace;
                        }
                        
                        handler(this, new LockContentedEventArgs(stackTrace));
                    }
                }
            }
        }
        /// <summary>
        /// If a lock-contention is detected, this event signals the *owner* of the lock responsible for the blockage, indicating
        /// what caused the problem; this is only raised if the lock-owning code successfully completes.
        /// </summary>
        public event LockContentedEventHandler LockContended;

        internal void ResolveListTypes(Type type, ref Type itemType, ref Type defaultType)
        {
            if (type == null) return;
            if(Helpers.GetTypeCode(type) != ProtoTypeCode.Unknown) return; // don't try this[type] for inbuilts
            if(this[type].IgnoreListHandling) return;

            // handle arrays
            if (type.IsArray)
            {
                if (type.GetArrayRank() != 1)
                {
                    throw new NotSupportedException("Multi-dimension arrays are supported");
                }
                itemType = type.GetElementType();
                if (itemType == MapType(typeof(byte)))
                {
                    defaultType = itemType = null;
                }
                else
                {
                    defaultType = type;
                }
            }
            // handle lists
            if (itemType == null) { itemType = TypeModel.GetListItemType(this, type); }

            // check for nested data (not allowed)
            if (itemType != null)
            {
                Type nestedItemType = null, nestedDefaultType = null;
                ResolveListTypes(itemType, ref nestedItemType, ref nestedDefaultType);
                if (nestedItemType != null)
                {
                    throw TypeModel.CreateNestedListsNotSupported();
                }
            }

            if (itemType != null && defaultType == null)
            {
#if WINRT
                System.Reflection.TypeInfo typeInfo = System.Reflection.IntrospectionExtensions.GetTypeInfo(type);
                if (typeInfo.IsClass && !typeInfo.IsAbstract && Helpers.GetConstructor(typeInfo, Helpers.EmptyTypes, true) != null)
#else
                if (type.IsClass && !type.IsAbstract && Helpers.GetConstructor(type, Helpers.EmptyTypes, true) != null)
#endif
                {
                    defaultType = type;
                }
                if (defaultType == null)
                {
#if WINRT
                    if (typeInfo.IsInterface)
#else
                    if (type.IsInterface)
#endif
                    {
#if NO_GENERICS
                        defaultType = typeof(ArrayList);
#else
                        Type[] genArgs;
#if WINRT
                        if (typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(System.Collections.Generic.IDictionary<,>)
                            && itemType == typeof(System.Collections.Generic.KeyValuePair<,>).MakeGenericType(genArgs = typeInfo.GenericTypeArguments))
#else
                        if (type.IsGenericType && type.GetGenericTypeDefinition() == MapType(typeof(System.Collections.Generic.IDictionary<,>))
                            && itemType == MapType(typeof(System.Collections.Generic.KeyValuePair<,>)).MakeGenericType(genArgs = type.GetGenericArguments()))
#endif
                        {
                            defaultType = MapType(typeof(System.Collections.Generic.Dictionary<,>)).MakeGenericType(genArgs);
                        }
                        else
                        {
                            defaultType = MapType(typeof(System.Collections.Generic.List<>)).MakeGenericType(itemType);
                        }
#endif
                    }
                }
                // verify that the default type is appropriate
                if (defaultType != null && !Helpers.IsAssignableFrom(type, defaultType)) { defaultType = null; }
            }
        }

      
#if FEAT_IKVM
        internal override Type GetType(string fullName, Assembly context)
        {
            if (context != null)
            {
                Type found = universe.GetType(context, fullName, false);
                if (found != null) return found;
            }
            return universe.GetType(fullName, false);
        }
#endif

        internal string GetSchemaTypeName(Type effectiveType, DataFormat dataFormat, bool asReference, bool dynamicType, ref bool requiresBclImport)
        {
            Type tmp = Helpers.GetUnderlyingType(effectiveType);
            if (tmp != null) effectiveType = tmp;

            if (effectiveType == this.MapType(typeof(byte[]))) return "bytes";

            WireType wireType;
            IProtoSerializer ser = ValueMember.TryGetCoreSerializer(this, dataFormat, effectiveType, out wireType, false, false, false, false);
            if (ser == null)
            {   // model type
                if (asReference || dynamicType)
                {
                    requiresBclImport = true;
                    return "bcl.NetObjectProxy";
                }
                return this[effectiveType].GetSurrogateOrBaseOrSelf(true).GetSchemaTypeName();
            }
            else
            {
                if (ser is ParseableSerializer)
                {
                    if (asReference) requiresBclImport = true;
                    return asReference ? "bcl.NetObjectProxy" : "string";
                }

                switch (Helpers.GetTypeCode(effectiveType))
                {
                    case ProtoTypeCode.Boolean: return "bool";
                    case ProtoTypeCode.Single: return "float";
                    case ProtoTypeCode.Double: return "double";
                    case ProtoTypeCode.String:
                        if (asReference) requiresBclImport = true;
                        return asReference ? "bcl.NetObjectProxy" : "string";
                    case ProtoTypeCode.Byte:
                    case ProtoTypeCode.Char:
                    case ProtoTypeCode.UInt16:
                    case ProtoTypeCode.UInt32:
                        switch (dataFormat)
                        {
                            case DataFormat.FixedSize: return "fixed32";
                            default: return "uint32";
                        }
                    case ProtoTypeCode.SByte:
                    case ProtoTypeCode.Int16:
                    case ProtoTypeCode.Int32:
                        switch (dataFormat)
                        {
                            case DataFormat.ZigZag: return "sint32";
                            case DataFormat.FixedSize: return "sfixed32";
                            default: return "int32";
                        }
                    case ProtoTypeCode.UInt64:
                        switch (dataFormat)
                        {
                            case DataFormat.FixedSize: return "fixed64";
                            default: return "uint64";
                        }
                    case ProtoTypeCode.Int64:
                        switch (dataFormat)
                        {
                            case DataFormat.ZigZag: return "sint64";
                            case DataFormat.FixedSize: return "sfixed64";
                            default: return "int64";
                        }
                    case ProtoTypeCode.DateTime: requiresBclImport = true; return "bcl.DateTime";
                    case ProtoTypeCode.TimeSpan: requiresBclImport = true; return "bcl.TimeSpan";
                    case ProtoTypeCode.Decimal: requiresBclImport = true; return "bcl.Decimal";
                    case ProtoTypeCode.Guid: requiresBclImport = true; return "bcl.Guid";
                    default: throw new NotSupportedException("No .proto map found for: " + effectiveType.FullName);
                }
            }

        }

        /// <summary>
        /// Designate a factory-method to use to create instances of any type; note that this only affect types seen by the serializer *after* setting the factory.
        /// </summary>
        public void SetDefaultFactory(MethodInfo methodInfo)
        {
            VerifyFactory(methodInfo, null);
            defaultFactory = methodInfo;
        }
        private MethodInfo defaultFactory;

        internal void VerifyFactory(MethodInfo factory, Type type)
        {
            if (factory != null)
            {
                if (type != null && Helpers.IsValueType(type)) throw new InvalidOperationException();
                if (!factory.IsStatic) throw new ArgumentException("A factory-method must be static", "factory");
                if ((type != null && factory.ReturnType != type) && factory.ReturnType != MapType(typeof(object))) throw new ArgumentException("The factory-method must return object" + (type == null ? "" : (" or " + type.FullName)), "factory");

                if (!CallbackSet.CheckCallbackParameters(this, factory)) throw new ArgumentException("Invalid factory signature in " + factory.DeclaringType.FullName + "." + factory.Name, "factory");
            }
        }

    }
    /// <summary>
    /// Contains the stack-trace of the owning code when a lock-contention scenario is detected
    /// </summary>
    public sealed class LockContentedEventArgs : EventArgs
    {
        private readonly string ownerStackTrace;
        internal LockContentedEventArgs(string ownerStackTrace)
        {
            this.ownerStackTrace = ownerStackTrace;
        }
        /// <summary>
        /// The stack-trace of the code that owned the lock when a lock-contention scenario occurred
        /// </summary>
        public string OwnerStackTrace { get { return ownerStackTrace; } }
    }
    /// <summary>
    /// Event-type that is raised when a lock-contention scenario is detected
    /// </summary>
    public delegate void LockContentedEventHandler(object sender, LockContentedEventArgs args);


}
#endif
﻿#if !NO_RUNTIME
using System;
using System.Collections;
using System.Text;
using ProtoBuf.Serializers;
using System.Reflection;
using System.Collections.Generic;

#if PROFILE259
using System.Linq;
#endif

namespace ProtoBuf.Meta
{
    /// <summary>
    /// Represents a type at runtime for use with protobuf, allowing the field mappings (etc) to be defined
    /// </summary>
    public class MetaType : ISerializerProxy
    {
        internal sealed class Comparer : IComparer, IComparer<MetaType>
        {
            public static readonly Comparer Default = new Comparer();
            public int Compare(object x, object y)
            {
                return Compare(x as MetaType, y as MetaType);
            }
            public int Compare(MetaType x, MetaType y)
            {
                if (ReferenceEquals(x, y)) return 0;
                if (x == null) return -1;
                if (y == null) return 1;

                return string.Compare(x.GetSchemaTypeName(), y.GetSchemaTypeName(), StringComparison.Ordinal);
            }
        }
        /// <summary>
        /// Get the name of the type being represented
        /// </summary>
        public override string ToString()
        {
            return type.ToString();
        }

        IProtoSerializer ISerializerProxy.Serializer => Serializer;
        private MetaType baseType;

        /// <summary>
        /// Gets the base-type for this type
        /// </summary>
        public MetaType BaseType => baseType;

        internal TypeModel Model => model;

        /// <summary>
        /// When used to compile a model, should public serialization/deserialzation methods
        /// be included for this type?
        /// </summary>
        public bool IncludeSerializerMethod
        {   // negated to minimize common-case / initializer
            get { return !HasFlag(OPTIONS_PrivateOnApi); }
            set { SetFlag(OPTIONS_PrivateOnApi, !value, true); }
        }

        /// <summary>
        /// Should this type be treated as a reference by default?
        /// </summary>
        public bool AsReferenceDefault
        {
            get { return HasFlag(OPTIONS_AsReferenceDefault); }
            set { SetFlag(OPTIONS_AsReferenceDefault, value, true); }
        }

        private BasicList subTypes;
        private bool IsValidSubType(Type subType)
        {
#if COREFX || PROFILE259
            return typeInfo.IsAssignableFrom(subType.GetTypeInfo());
#else
            return type.IsAssignableFrom(subType);
#endif
        }
        /// <summary>
        /// Adds a known sub-type to the inheritance model
        /// </summary>
        public MetaType AddSubType(int fieldNumber, Type derivedType)
        {
            return AddSubType(fieldNumber, derivedType, DataFormat.Default);
        }
        /// <summary>
        /// Adds a known sub-type to the inheritance model
        /// </summary>
        public MetaType AddSubType(int fieldNumber, Type derivedType, DataFormat dataFormat)
        {
            if (derivedType == null) throw new ArgumentNullException("derivedType");
            if (fieldNumber < 1) throw new ArgumentOutOfRangeException("fieldNumber");
#if COREFX || COREFX || PROFILE259
			if (!(typeInfo.IsClass || typeInfo.IsInterface) || typeInfo.IsSealed) {
#else
            if (!(type.IsClass || type.IsInterface) || type.IsSealed)
            {
#endif
                throw new InvalidOperationException("Sub-types can only be added to non-sealed classes");
            }
            if (!IsValidSubType(derivedType))
            {
                throw new ArgumentException(derivedType.Name + " is not a valid sub-type of " + type.Name, "derivedType");
            }
            MetaType derivedMeta = model[derivedType];
            ThrowIfFrozen();
            derivedMeta.ThrowIfFrozen();
            SubType subType = new SubType(fieldNumber, derivedMeta, dataFormat);
            ThrowIfFrozen();

            derivedMeta.SetBaseType(this); // includes ThrowIfFrozen
            if (subTypes == null) subTypes = new BasicList();
            subTypes.Add(subType);
            model.ResetKeyCache();
            return this;
        }
#if COREFX || PROFILE259
		internal static readonly TypeInfo ienumerable = typeof(IEnumerable).GetTypeInfo();
#else
        internal static readonly Type ienumerable = typeof(IEnumerable);
#endif
        private void SetBaseType(MetaType baseType)
        {
            if (baseType == null) throw new ArgumentNullException("baseType");
            if (this.baseType == baseType) return;
            if (this.baseType != null) throw new InvalidOperationException($"Type '{this.baseType.Type.FullName}' can only participate in one inheritance hierarchy");

            MetaType type = baseType;
            while (type != null)
            {
                if (ReferenceEquals(type, this)) throw new InvalidOperationException($"Cyclic inheritance of '{this.baseType.Type.FullName}' is not allowed");
                type = type.baseType;
            }
            this.baseType = baseType;
        }

        private CallbackSet callbacks;

        /// <summary>
        /// Indicates whether the current type has defined callbacks 
        /// </summary>
        public bool HasCallbacks => callbacks != null && callbacks.NonTrivial;

        /// <summary>
        /// Indicates whether the current type has defined subtypes
        /// </summary>
        public bool HasSubtypes => subTypes != null && subTypes.Count != 0;

        /// <summary>
        /// Returns the set of callbacks defined for this type
        /// </summary>
        public CallbackSet Callbacks
        {
            get
            {
                if (callbacks == null) callbacks = new CallbackSet(this);
                return callbacks;
            }
        }

        private bool IsValueType
        {
            get
            {
#if COREFX || PROFILE259
				return typeInfo.IsValueType;
#else
                return type.IsValueType;
#endif
            }
        }
        /// <summary>
        /// Assigns the callbacks to use during serialiation/deserialization.
        /// </summary>
        /// <param name="beforeSerialize">The method (or null) called before serialization begins.</param>
        /// <param name="afterSerialize">The method (or null) called when serialization is complete.</param>
        /// <param name="beforeDeserialize">The method (or null) called before deserialization begins (or when a new instance is created during deserialization).</param>
        /// <param name="afterDeserialize">The method (or null) called when deserialization is complete.</param>
        /// <returns>The set of callbacks.</returns>
        public MetaType SetCallbacks(MethodInfo beforeSerialize, MethodInfo afterSerialize, MethodInfo beforeDeserialize, MethodInfo afterDeserialize)
        {
            CallbackSet callbacks = Callbacks;
            callbacks.BeforeSerialize = beforeSerialize;
            callbacks.AfterSerialize = afterSerialize;
            callbacks.BeforeDeserialize = beforeDeserialize;
            callbacks.AfterDeserialize = afterDeserialize;
            return this;
        }
        /// <summary>
        /// Assigns the callbacks to use during serialiation/deserialization.
        /// </summary>
        /// <param name="beforeSerialize">The name of the method (or null) called before serialization begins.</param>
        /// <param name="afterSerialize">The name of the method (or null) called when serialization is complete.</param>
        /// <param name="beforeDeserialize">The name of the method (or null) called before deserialization begins (or when a new instance is created during deserialization).</param>
        /// <param name="afterDeserialize">The name of the method (or null) called when deserialization is complete.</param>
        /// <returns>The set of callbacks.</returns>
        public MetaType SetCallbacks(string beforeSerialize, string afterSerialize, string beforeDeserialize, string afterDeserialize)
        {
            if (IsValueType) throw new InvalidOperationException();
            CallbackSet callbacks = Callbacks;
            callbacks.BeforeSerialize = ResolveMethod(beforeSerialize, true);
            callbacks.AfterSerialize = ResolveMethod(afterSerialize, true);
            callbacks.BeforeDeserialize = ResolveMethod(beforeDeserialize, true);
            callbacks.AfterDeserialize = ResolveMethod(afterDeserialize, true);
            return this;
        }

        /// <summary>
        /// Returns the public Type name of this Type used in serialization
        /// </summary>
        public string GetSchemaTypeName()
        {
            if (surrogate != null) return model[surrogate].GetSchemaTypeName();

            if (!string.IsNullOrEmpty(name)) return name;

            string typeName = type.Name;
            if (type
#if COREFX || PROFILE259
				.GetTypeInfo()
#endif
                .IsGenericType)
            {
                var sb = new StringBuilder(typeName);
                int split = typeName.IndexOf('`');
                if (split >= 0) sb.Length = split;
                foreach (Type arg in type
#if COREFX || PROFILE259
					.GetTypeInfo().GenericTypeArguments
#else
                    .GetGenericArguments()
#endif
                    )
                {
                    sb.Append('_');
                    Type tmp = arg;
                    int key = model.GetKey(ref tmp);
                    MetaType mt;
                    if (key >= 0 && (mt = model[tmp]) != null && mt.surrogate == null) // <=== need to exclude surrogate to avoid chance of infinite loop
                    {

                        sb.Append(mt.GetSchemaTypeName());
                    }
                    else
                    {
                        sb.Append(tmp.Name);
                    }
                }
                return sb.ToString();
            }

            return typeName;
        }

        private string name;

        /// <summary>
        /// Gets or sets the name of this contract.
        /// </summary>
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                ThrowIfFrozen();
                name = value;
            }
        }

        private MethodInfo factory;
        /// <summary>
        /// Designate a factory-method to use to create instances of this type
        /// </summary>
        public MetaType SetFactory(MethodInfo factory)
        {
            model.VerifyFactory(factory, type);
            ThrowIfFrozen();
            this.factory = factory;
            return this;
        }

        /// <summary>
        /// Designate a factory-method to use to create instances of this type
        /// </summary>
        public MetaType SetFactory(string factory)
        {
            return SetFactory(ResolveMethod(factory, false));
        }

        private MethodInfo ResolveMethod(string name, bool instance)
        {
            if (string.IsNullOrEmpty(name)) return null;
#if COREFX
            return instance ? Helpers.GetInstanceMethod(typeInfo, name) : Helpers.GetStaticMethod(typeInfo, name);
#else
            return instance ? Helpers.GetInstanceMethod(type, name) : Helpers.GetStaticMethod(type, name);
#endif
        }

        private readonly RuntimeTypeModel model;

        internal static Exception InbuiltType(Type type)
        {
            return new ArgumentException("Data of this type has inbuilt behaviour, and cannot be added to a model in this way: " + type.FullName);
        }

        internal MetaType(RuntimeTypeModel model, Type type, MethodInfo factory)
        {
            this.factory = factory;
            if (model == null) throw new ArgumentNullException("model");
            if (type == null) throw new ArgumentNullException("type");

            if (type.IsArray) throw InbuiltType(type);
            IProtoSerializer coreSerializer = model.TryGetBasicTypeSerializer(type);
            if (coreSerializer != null)
            {
                throw InbuiltType(type);
            }

            this.type = type;
#if COREFX || PROFILE259
			this.typeInfo = type.GetTypeInfo();
#endif
            this.model = model;

            if (Helpers.IsEnum(type))
            {
#if COREFX || PROFILE259
				EnumPassthru = typeInfo.IsDefined(typeof(FlagsAttribute), false);
#else
                EnumPassthru = type.IsDefined(model.MapType(typeof(FlagsAttribute)), false);
#endif
            }
        }
#if COREFX || PROFILE259
		private readonly TypeInfo typeInfo;
#endif
        /// <summary>
        /// Throws an exception if the type has been made immutable
        /// </summary>
        protected internal void ThrowIfFrozen()
        {
            if ((flags & OPTIONS_Frozen) != 0) throw new InvalidOperationException("The type cannot be changed once a serializer has been generated for " + type.FullName);
        }

        // internal void Freeze() { flags |= OPTIONS_Frozen; }

        private readonly Type type;
        /// <summary>
        /// The runtime type that the meta-type represents
        /// </summary>
        public Type Type => type;

        private IProtoTypeSerializer serializer;
        internal IProtoTypeSerializer Serializer
        {
            get
            {
                if (serializer == null)
                {
                    int opaqueToken = 0;
                    try
                    {
                        model.TakeLock(ref opaqueToken);
                        if (serializer == null)
                        { // double-check, but our main purpse with this lock is to ensure thread-safety with
                            // serializers needing to wait until another thread has finished adding the properties
                            SetFlag(OPTIONS_Frozen, true, false);
                            serializer = BuildSerializer();
#if FEAT_COMPILER
                            if (model.AutoCompile) CompileInPlace();
#endif
                        }
                    }
                    finally
                    {
                        model.ReleaseLock(opaqueToken);
                    }
                }
                return serializer;
            }
        }
        internal bool IsList
        {
            get
            {
                Type itemType = IgnoreListHandling ? null : TypeModel.GetListItemType(model, type);
                return itemType != null;
            }
        }
        private IProtoTypeSerializer BuildSerializer()
        {
            if (Helpers.IsEnum(type))
            {
                return new TagDecorator(ProtoBuf.Serializer.ListItemTag, WireType.Variant, false, new EnumSerializer(type, GetEnumMap()));
            }
            Type itemType = IgnoreListHandling ? null : TypeModel.GetListItemType(model, type);
            if (itemType != null)
            {
                if (surrogate != null)
                {
                    throw new ArgumentException("Repeated data (a list, collection, etc) has inbuilt behaviour and cannot use a surrogate");
                }
                if (subTypes != null && subTypes.Count != 0)
                {
                    throw new ArgumentException("Repeated data (a list, collection, etc) has inbuilt behaviour and cannot be subclassed");
                }
                Type defaultType = null;
                ResolveListTypes(model, type, ref itemType, ref defaultType);
                ValueMember fakeMember = new ValueMember(model, ProtoBuf.Serializer.ListItemTag, type, itemType, defaultType, DataFormat.Default);
                return new TypeSerializer(model, type, new int[] { ProtoBuf.Serializer.ListItemTag }, new IProtoSerializer[] { fakeMember.Serializer }, null, true, true, null, constructType, factory);
            }
            if (surrogate != null)
            {
                MetaType mt = model[surrogate], mtBase;
                while ((mtBase = mt.baseType) != null) { mt = mtBase; }
                return new SurrogateSerializer(model, type, surrogate, mt.Serializer);
            }
            if (IsAutoTuple)
            {
                ConstructorInfo ctor = ResolveTupleConstructor(type, out MemberInfo[] mapping);
                if (ctor == null) throw new InvalidOperationException();
                return new TupleSerializer(model, ctor, mapping);
            }

            fields.Trim();
            int fieldCount = fields.Count;
            int subTypeCount = subTypes == null ? 0 : subTypes.Count;
            int[] fieldNumbers = new int[fieldCount + subTypeCount];
            IProtoSerializer[] serializers = new IProtoSerializer[fieldCount + subTypeCount];
            int i = 0;
            if (subTypeCount != 0)
            {
                foreach (SubType subType in subTypes)
                {
#if COREFX || PROFILE259
					if (!subType.DerivedType.IgnoreListHandling && ienumerable.IsAssignableFrom(subType.DerivedType.Type.GetTypeInfo()))
#else
                    if (!subType.DerivedType.IgnoreListHandling && model.MapType(ienumerable).IsAssignableFrom(subType.DerivedType.Type))
#endif
                    {
                        throw new ArgumentException("Repeated data (a list, collection, etc) has inbuilt behaviour and cannot be used as a subclass");
                    }
                    fieldNumbers[i] = subType.FieldNumber;
                    serializers[i++] = subType.Serializer;
                }
            }
            if (fieldCount != 0)
            {
                foreach (ValueMember member in fields)
                {
                    fieldNumbers[i] = member.FieldNumber;
                    serializers[i++] = member.Serializer;
                }
            }

            BasicList baseCtorCallbacks = null;
            MetaType tmp = BaseType;

            while (tmp != null)
            {
                MethodInfo method = tmp.HasCallbacks ? tmp.Callbacks.BeforeDeserialize : null;
                if (method != null)
                {
                    if (baseCtorCallbacks == null) baseCtorCallbacks = new BasicList();
                    baseCtorCallbacks.Add(method);
                }
                tmp = tmp.BaseType;
            }
            MethodInfo[] arr = null;
            if (baseCtorCallbacks != null)
            {
                arr = new MethodInfo[baseCtorCallbacks.Count];
                baseCtorCallbacks.CopyTo(arr, 0);
                Array.Reverse(arr);
            }
            return new TypeSerializer(model, type, fieldNumbers, serializers, arr, baseType == null, UseConstructor, callbacks, constructType, factory);
        }

        [Flags]
        internal enum AttributeFamily
        {
            None = 0, ProtoBuf = 1, DataContractSerialier = 2, XmlSerializer = 4, AutoTuple = 8
        }
        static Type GetBaseType(MetaType type)
        {
#if COREFX || PROFILE259
			return type.typeInfo.BaseType;
#else
            return type.type.BaseType;
#endif
        }
        internal static bool GetAsReferenceDefault(RuntimeTypeModel model, Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (Helpers.IsEnum(type)) return false; // never as-ref
            AttributeMap[] typeAttribs = AttributeMap.Create(model, type, false);
            for (int i = 0; i < typeAttribs.Length; i++)
            {
                if (typeAttribs[i].AttributeType.FullName == "ProtoBuf.ProtoContractAttribute")
                {
                    if (typeAttribs[i].TryGet("AsReferenceDefault", out object tmp)) return (bool)tmp;
                }
            }
            return false;
        }

        internal void ApplyDefaultBehaviour()
        {
            TypeAddedEventArgs args = null; // allows us to share the event-args between events
            RuntimeTypeModel.OnBeforeApplyDefaultBehaviour(this, ref args);
            if (args == null || args.ApplyDefaultBehaviour) ApplyDefaultBehaviourImpl();
            RuntimeTypeModel.OnAfterApplyDefaultBehaviour(this, ref args);
        }

        internal void ApplyDefaultBehaviourImpl()
        {
            Type baseType = GetBaseType(this);
            if (baseType != null && model.FindWithoutAdd(baseType) == null
                && GetContractFamily(model, baseType, null) != MetaType.AttributeFamily.None)
            {
                model.FindOrAddAuto(baseType, true, false, false);
            }

            AttributeMap[] typeAttribs = AttributeMap.Create(model, type, false);
            AttributeFamily family = GetContractFamily(model, type, typeAttribs);
            if (family == AttributeFamily.AutoTuple)
            {
                SetFlag(OPTIONS_AutoTuple, true, true);
            }
            bool isEnum = !EnumPassthru && Helpers.IsEnum(type);
            if (family == AttributeFamily.None && !isEnum) return; // and you'd like me to do what, exactly?

            bool enumShouldUseImplicitPassThru = isEnum;
            BasicList partialIgnores = null, partialMembers = null;
            int dataMemberOffset = 0, implicitFirstTag = 1;
            bool inferTagByName = model.InferTagFromNameDefault;
            ImplicitFields implicitMode = ImplicitFields.None;
            string name = null;
            for (int i = 0; i < typeAttribs.Length; i++)
            {
                AttributeMap item = (AttributeMap)typeAttribs[i];
                object tmp;
                string fullAttributeTypeName = item.AttributeType.FullName;
                if (!isEnum && fullAttributeTypeName == "ProtoBuf.ProtoIncludeAttribute")
                {
                    int tag = 0;
                    if (item.TryGet("tag", out tmp)) tag = (int)tmp;
                    DataFormat dataFormat = DataFormat.Default;
                    if (item.TryGet("DataFormat", out tmp))
                    {
                        dataFormat = (DataFormat)(int)tmp;
                    }
                    Type knownType = null;
                    try
                    {
                        if (item.TryGet("knownTypeName", out tmp)) knownType = model.GetType((string)tmp, type
#if COREFX || PROFILE259
							.GetTypeInfo()
#endif
                            .Assembly);
                        else if (item.TryGet("knownType", out tmp)) knownType = (Type)tmp;
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException("Unable to resolve sub-type of: " + type.FullName, ex);
                    }
                    if (knownType == null)
                    {
                        throw new InvalidOperationException("Unable to resolve sub-type of: " + type.FullName);
                    }
                    if (IsValidSubType(knownType)) AddSubType(tag, knownType, dataFormat);
                }

                if (fullAttributeTypeName == "ProtoBuf.ProtoPartialIgnoreAttribute")
                {
                    if (item.TryGet(nameof(ProtoPartialIgnoreAttribute.MemberName), out tmp) && tmp != null)
                    {
                        if (partialIgnores == null) partialIgnores = new BasicList();
                        partialIgnores.Add((string)tmp);
                    }
                }
                if (!isEnum && fullAttributeTypeName == "ProtoBuf.ProtoPartialMemberAttribute")
                {
                    if (partialMembers == null) partialMembers = new BasicList();
                    partialMembers.Add(item);
                }

                if (fullAttributeTypeName == "ProtoBuf.ProtoContractAttribute")
                {
                    if (item.TryGet(nameof(ProtoContractAttribute.Name), out tmp)) name = (string)tmp;
                    if (Helpers.IsEnum(type)) // note this is subtly different to isEnum; want to do this even if [Flags]
                    {
                        if (item.TryGet(nameof(ProtoContractAttribute.EnumPassthruHasValue), false, out tmp) && (bool)tmp)
                        {
                            if (item.TryGet(nameof(ProtoContractAttribute.EnumPassthru), out tmp))
                            {
                                EnumPassthru = (bool)tmp;
                                enumShouldUseImplicitPassThru = false;
                                if (EnumPassthru) isEnum = false; // no longer treated as an enum
                            }
                        }
                    }
                    else
                    {
                        if (item.TryGet(nameof(ProtoContractAttribute.DataMemberOffset), out tmp)) dataMemberOffset = (int)tmp;

                        if (item.TryGet(nameof(ProtoContractAttribute.InferTagFromNameHasValue), false, out tmp) && (bool)tmp)
                        {
                            if (item.TryGet(nameof(ProtoContractAttribute.InferTagFromName), out tmp)) inferTagByName = (bool)tmp;
                        }

                        if (item.TryGet(nameof(ProtoContractAttribute.ImplicitFields), out tmp) && tmp != null)
                        {
                            implicitMode = (ImplicitFields)(int)tmp; // note that this uses the bizarre unboxing rules of enums/underlying-types
                        }

                        if (item.TryGet(nameof(ProtoContractAttribute.SkipConstructor), out tmp)) UseConstructor = !(bool)tmp;
                        if (item.TryGet(nameof(ProtoContractAttribute.IgnoreListHandling), out tmp)) IgnoreListHandling = (bool)tmp;
                        if (item.TryGet(nameof(ProtoContractAttribute.AsReferenceDefault), out tmp)) AsReferenceDefault = (bool)tmp;
                        if (item.TryGet(nameof(ProtoContractAttribute.ImplicitFirstTag), out tmp) && (int)tmp > 0) implicitFirstTag = (int)tmp;
                        if (item.TryGet(nameof(ProtoContractAttribute.IsGroup), out tmp)) IsGroup = (bool)tmp;

                        if (item.TryGet(nameof(ProtoContractAttribute.Surrogate), out tmp))
                        {
                            SetSurrogate((Type)tmp);
                        }
                    }
                }

                if (fullAttributeTypeName == "System.Runtime.Serialization.DataContractAttribute")
                {
                    if (name == null && item.TryGet("Name", out tmp)) name = (string)tmp;
                }
                if (fullAttributeTypeName == "System.Xml.Serialization.XmlTypeAttribute")
                {
                    if (name == null && item.TryGet("TypeName", out tmp)) name = (string)tmp;
                }
            }
            if (!string.IsNullOrEmpty(name)) Name = name;
            if (implicitMode != ImplicitFields.None)
            {
                family &= AttributeFamily.ProtoBuf; // with implicit fields, **only** proto attributes are important
            }
            MethodInfo[] callbacks = null;

            BasicList members = new BasicList();

#if PROFILE259
			IEnumerable<MemberInfo> foundList;
            if(isEnum) {
                foundList = type.GetRuntimeFields();
            }
            else
            {
                List<MemberInfo> list = new List<MemberInfo>();
                foreach(PropertyInfo prop in type.GetRuntimeProperties()) {
                    MethodInfo getter = Helpers.GetGetMethod(prop, false, false);
                    if(getter != null && !getter.IsStatic) list.Add(prop);
                }
                foreach(FieldInfo fld in type.GetRuntimeFields()) if(fld.IsPublic && !fld.IsStatic) list.Add(fld);
                foreach(MethodInfo mthd in type.GetRuntimeMethods()) if(mthd.IsPublic && !mthd.IsStatic) list.Add(mthd);
                foundList = list;
            }
#else
            MemberInfo[] foundList = type.GetMembers(isEnum ? BindingFlags.Public | BindingFlags.Static
                : BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
#endif
            bool hasConflictingEnumValue = false;
            foreach (MemberInfo member in foundList)
            {
                if (member.DeclaringType != type) continue;
                if (member.IsDefined(model.MapType(typeof(ProtoIgnoreAttribute)), true)) continue;
                if (partialIgnores != null && partialIgnores.Contains(member.Name)) continue;

                bool forced = false, isPublic, isField;
                Type effectiveType;

                if (member is PropertyInfo property)
                {
                    if (isEnum) continue; // wasn't expecting any props!
                    MemberInfo backingField = null;
                    if (!property.CanWrite)
                    {
                        // roslyn automatically implemented properties, in particular for get-only properties: <{Name}>k__BackingField;
                        var backingFieldName = $"<{property.Name}>k__BackingField";
                        foreach (var fieldMemeber in foundList)
                        {
                            if ((fieldMemeber as FieldInfo != null) && fieldMemeber.Name == backingFieldName)
                            {
                                backingField = fieldMemeber;
                                break;
                            }
                        }
                    }
                    effectiveType = property.PropertyType;
                    isPublic = Helpers.GetGetMethod(property, false, false) != null;
                    isField = false;
                    ApplyDefaultBehaviour_AddMembers(model, family, isEnum, partialMembers, dataMemberOffset, inferTagByName, implicitMode, members, member, ref forced, isPublic, isField, ref effectiveType, ref hasConflictingEnumValue, backingField);
                }
                else if (member is FieldInfo field)
                {
                    effectiveType = field.FieldType;
                    isPublic = field.IsPublic;
                    isField = true;
                    if (isEnum && !field.IsStatic)
                    { // only care about static things on enums; WinRT has a __value instance field!
                        continue;
                    }
                    ApplyDefaultBehaviour_AddMembers(model, family, isEnum, partialMembers, dataMemberOffset, inferTagByName, implicitMode, members, member, ref forced, isPublic, isField, ref effectiveType, ref hasConflictingEnumValue);
                }
                else if (member is MethodInfo method)
                {
                    if (isEnum) continue;
                    AttributeMap[] memberAttribs = AttributeMap.Create(model, method, false);
                    if (memberAttribs != null && memberAttribs.Length > 0)
                    {
                        CheckForCallback(method, memberAttribs, "ProtoBuf.ProtoBeforeSerializationAttribute", ref callbacks, 0);
                        CheckForCallback(method, memberAttribs, "ProtoBuf.ProtoAfterSerializationAttribute", ref callbacks, 1);
                        CheckForCallback(method, memberAttribs, "ProtoBuf.ProtoBeforeDeserializationAttribute", ref callbacks, 2);
                        CheckForCallback(method, memberAttribs, "ProtoBuf.ProtoAfterDeserializationAttribute", ref callbacks, 3);
                        CheckForCallback(method, memberAttribs, "System.Runtime.Serialization.OnSerializingAttribute", ref callbacks, 4);
                        CheckForCallback(method, memberAttribs, "System.Runtime.Serialization.OnSerializedAttribute", ref callbacks, 5);
                        CheckForCallback(method, memberAttribs, "System.Runtime.Serialization.OnDeserializingAttribute", ref callbacks, 6);
                        CheckForCallback(method, memberAttribs, "System.Runtime.Serialization.OnDeserializedAttribute", ref callbacks, 7);
                    }
                }
            }

            if (isEnum && enumShouldUseImplicitPassThru && !hasConflictingEnumValue)
            {
                EnumPassthru = true;
                // but leave isEnum alone
            }
            var arr = new ProtoMemberAttribute[members.Count];
            members.CopyTo(arr, 0);

            if (inferTagByName || implicitMode != ImplicitFields.None)
            {
                Array.Sort(arr);
                int nextTag = implicitFirstTag;
                foreach (ProtoMemberAttribute normalizedAttribute in arr)
                {
                    if (!normalizedAttribute.TagIsPinned) // if ProtoMember etc sets a tag, we'll trust it
                    {
                        normalizedAttribute.Rebase(nextTag++);
                    }
                }
            }

            foreach (ProtoMemberAttribute normalizedAttribute in arr)
            {
                ValueMember vm = ApplyDefaultBehaviour(isEnum, normalizedAttribute);
                if (vm != null)
                {
                    Add(vm);
                }
            }

            if (callbacks != null)
            {
                SetCallbacks(Coalesce(callbacks, 0, 4), Coalesce(callbacks, 1, 5),
                    Coalesce(callbacks, 2, 6), Coalesce(callbacks, 3, 7));
            }
        }

        private static void ApplyDefaultBehaviour_AddMembers(TypeModel model, AttributeFamily family, bool isEnum, BasicList partialMembers, int dataMemberOffset, bool inferTagByName, ImplicitFields implicitMode, BasicList members, MemberInfo member, ref bool forced, bool isPublic, bool isField, ref Type effectiveType, ref bool hasConflictingEnumValue, MemberInfo backingMember = null)
        {
            switch (implicitMode)
            {
                case ImplicitFields.AllFields:
                    if (isField) forced = true;
                    break;
                case ImplicitFields.AllPublic:
                    if (isPublic) forced = true;
                    break;
            }

            // we just don't like delegate types ;p
#if COREFX || PROFILE259
			if (effectiveType.GetTypeInfo().IsSubclassOf(typeof(Delegate))) effectiveType = null;
#else
            if (effectiveType.IsSubclassOf(model.MapType(typeof(Delegate)))) effectiveType = null;
#endif
            if (effectiveType != null)
            {
                ProtoMemberAttribute normalizedAttribute = NormalizeProtoMember(model, member, family, forced, isEnum, partialMembers, dataMemberOffset, inferTagByName, ref hasConflictingEnumValue, backingMember);
                if (normalizedAttribute != null) members.Add(normalizedAttribute);
            }
        }

        static MethodInfo Coalesce(MethodInfo[] arr, int x, int y)
        {
            MethodInfo mi = arr[x];
            if (mi == null) mi = arr[y];
            return mi;
        }

        internal static AttributeFamily GetContractFamily(RuntimeTypeModel model, Type type, AttributeMap[] attributes)
        {
            AttributeFamily family = AttributeFamily.None;

            if (attributes == null) attributes = AttributeMap.Create(model, type, false);

            for (int i = 0; i < attributes.Length; i++)
            {
                switch (attributes[i].AttributeType.FullName)
                {
                    case "ProtoBuf.ProtoContractAttribute":
                        bool tmp = false;
                        GetFieldBoolean(ref tmp, attributes[i], "UseProtoMembersOnly");
                        if (tmp) return AttributeFamily.ProtoBuf;
                        family |= AttributeFamily.ProtoBuf;
                        break;
                    case "System.Xml.Serialization.XmlTypeAttribute":
                        if (!model.AutoAddProtoContractTypesOnly)
                        {
                            family |= AttributeFamily.XmlSerializer;
                        }
                        break;
                    case "System.Runtime.Serialization.DataContractAttribute":
                        if (!model.AutoAddProtoContractTypesOnly)
                        {
                            family |= AttributeFamily.DataContractSerialier;
                        }
                        break;
                }
            }
            if (family == AttributeFamily.None)
            { // check for obvious tuples
                if (ResolveTupleConstructor(type, out MemberInfo[] mapping) != null)
                {
                    family |= AttributeFamily.AutoTuple;
                }
            }
            return family;
        }
        internal static ConstructorInfo ResolveTupleConstructor(Type type, out MemberInfo[] mappedMembers)
        {
            mappedMembers = null;
            if (type == null) throw new ArgumentNullException(nameof(type));
#if COREFX || PROFILE259
			TypeInfo typeInfo = type.GetTypeInfo();
            if (typeInfo.IsAbstract) return null; // as if!
            ConstructorInfo[] ctors = Helpers.GetConstructors(typeInfo, false);
#else
            if (type.IsAbstract) return null; // as if!
            ConstructorInfo[] ctors = Helpers.GetConstructors(type, false);
#endif
            // need to have an interesting constructor to bother even checking this stuff
            if (ctors.Length == 0 || (ctors.Length == 1 && ctors[0].GetParameters().Length == 0)) return null;

            MemberInfo[] fieldsPropsUnfiltered = Helpers.GetInstanceFieldsAndProperties(type, true);
            BasicList memberList = new BasicList();
            // for most types we'll enforce that you need readonly, because that is what protobuf-net
            // always did historically; but: if you smell so much like a Tuple that it is *in your name*,
            // we'll let you past that
            bool demandReadOnly = type.Name.IndexOf("Tuple", StringComparison.OrdinalIgnoreCase) < 0;
            for (int i = 0; i < fieldsPropsUnfiltered.Length; i++)
            {
                if (fieldsPropsUnfiltered[i] is PropertyInfo prop)
                {
                    if (!prop.CanRead) return null; // no use if can't read
                    if (demandReadOnly && prop.CanWrite && Helpers.GetSetMethod(prop, false, false) != null) return null; // don't allow a public set (need to allow non-public to handle Mono's KeyValuePair<,>)
                    memberList.Add(prop);
                }
                else
                {
                    if (fieldsPropsUnfiltered[i] is FieldInfo field)
                    {
                        if (demandReadOnly && !field.IsInitOnly) return null; // all public fields must be readonly to be counted a tuple
                        memberList.Add(field);
                    }
                }
            }
            if (memberList.Count == 0)
            {
                return null;
            }

            MemberInfo[] members = new MemberInfo[memberList.Count];
            memberList.CopyTo(members, 0);

            int[] mapping = new int[members.Length];
            int found = 0;
            ConstructorInfo result = null;
            mappedMembers = new MemberInfo[mapping.Length];
            for (int i = 0; i < ctors.Length; i++)
            {
                ParameterInfo[] parameters = ctors[i].GetParameters();

                if (parameters.Length != members.Length) continue;

                // reset the mappings to test
                for (int j = 0; j < mapping.Length; j++) mapping[j] = -1;

                for (int j = 0; j < parameters.Length; j++)
                {
                    for (int k = 0; k < members.Length; k++)
                    {
                        if (string.Compare(parameters[j].Name, members[k].Name, StringComparison.OrdinalIgnoreCase) != 0) continue;
                        Type memberType = Helpers.GetMemberType(members[k]);
                        if (memberType != parameters[j].ParameterType) continue;

                        mapping[j] = k;
                    }
                }
                // did we map all?
                bool notMapped = false;
                for (int j = 0; j < mapping.Length; j++)
                {
                    if (mapping[j] < 0)
                    {
                        notMapped = true;
                        break;
                    }
                    mappedMembers[j] = members[mapping[j]];
                }

                if (notMapped) continue;
                found++;
                result = ctors[i];

            }
            return found == 1 ? result : null;
        }

        private static void CheckForCallback(MethodInfo method, AttributeMap[] attributes, string callbackTypeName, ref MethodInfo[] callbacks, int index)
        {
            for (int i = 0; i < attributes.Length; i++)
            {
                if (attributes[i].AttributeType.FullName == callbackTypeName)
                {
                    if (callbacks == null) { callbacks = new MethodInfo[8]; }
                    else if (callbacks[index] != null)
                    {
#if COREFX || PROFILE259
						Type reflected = method.DeclaringType;
#else
                        Type reflected = method.ReflectedType;
#endif
                        throw new ProtoException("Duplicate " + callbackTypeName + " callbacks on " + reflected.FullName);
                    }
                    callbacks[index] = method;
                }
            }
        }
        private static bool HasFamily(AttributeFamily value, AttributeFamily required)
        {
            return (value & required) == required;
        }

        private static ProtoMemberAttribute NormalizeProtoMember(TypeModel model, MemberInfo member, AttributeFamily family, bool forced, bool isEnum, BasicList partialMembers, int dataMemberOffset, bool inferByTagName, ref bool hasConflictingEnumValue, MemberInfo backingMember = null)
        {
            if (member == null || (family == AttributeFamily.None && !isEnum)) return null; // nix
            int fieldNumber = int.MinValue, minAcceptFieldNumber = inferByTagName ? -1 : 1;
            string name = null;
            bool isPacked = false, ignore = false, done = false, isRequired = false, asReference = false, asReferenceHasValue = false, dynamicType = false, tagIsPinned = false, overwriteList = false;
            DataFormat dataFormat = DataFormat.Default;
            if (isEnum) forced = true;
            AttributeMap[] attribs = AttributeMap.Create(model, member, true);
            AttributeMap attrib;

            if (isEnum)
            {
                attrib = GetAttribute(attribs, "ProtoBuf.ProtoIgnoreAttribute");
                if (attrib != null)
                {
                    ignore = true;
                }
                else
                {
                    attrib = GetAttribute(attribs, "ProtoBuf.ProtoEnumAttribute");
#if PORTABLE || CF || COREFX || PROFILE259
					fieldNumber = Convert.ToInt32(((FieldInfo)member).GetValue(null));
#else
                    fieldNumber = Convert.ToInt32(((FieldInfo)member).GetRawConstantValue());
#endif
                    if (attrib != null)
                    {
                        GetFieldName(ref name, attrib, nameof(ProtoEnumAttribute.Name));

                        if ((bool)Helpers.GetInstanceMethod(attrib.AttributeType
#if COREFX || PROFILE259
							 .GetTypeInfo()
#endif
                            , nameof(ProtoEnumAttribute.HasValue)).Invoke(attrib.Target, null))
                        {
                            if (attrib.TryGet(nameof(ProtoEnumAttribute.Value), out object tmp))
                            {
                                if (fieldNumber != (int)tmp)
                                {
                                    hasConflictingEnumValue = true;
                                }
                                fieldNumber = (int)tmp;
                            }
                        }
                    }

                }
                done = true;
            }

            if (!ignore && !done) // always consider ProtoMember 
            {
                attrib = GetAttribute(attribs, "ProtoBuf.ProtoMemberAttribute");
                GetIgnore(ref ignore, attrib, attribs, "ProtoBuf.ProtoIgnoreAttribute");

                if (!ignore && attrib != null)
                {
                    GetFieldNumber(ref fieldNumber, attrib, "Tag");
                    GetFieldName(ref name, attrib, "Name");
                    GetFieldBoolean(ref isRequired, attrib, "IsRequired");
                    GetFieldBoolean(ref isPacked, attrib, "IsPacked");
                    GetFieldBoolean(ref overwriteList, attrib, "OverwriteList");
                    GetDataFormat(ref dataFormat, attrib, "DataFormat");
                    GetFieldBoolean(ref asReferenceHasValue, attrib, "AsReferenceHasValue", false);

                    if (asReferenceHasValue)
                    {
                        asReferenceHasValue = GetFieldBoolean(ref asReference, attrib, "AsReference", true);
                    }
                    GetFieldBoolean(ref dynamicType, attrib, "DynamicType");
                    done = tagIsPinned = fieldNumber > 0; // note minAcceptFieldNumber only applies to non-proto
                }

                if (!done && partialMembers != null)
                {
                    foreach (AttributeMap ppma in partialMembers)
                    {
                        if (ppma.TryGet("MemberName", out object tmp) && (string)tmp == member.Name)
                        {
                            GetFieldNumber(ref fieldNumber, ppma, "Tag");
                            GetFieldName(ref name, ppma, "Name");
                            GetFieldBoolean(ref isRequired, ppma, "IsRequired");
                            GetFieldBoolean(ref isPacked, ppma, "IsPacked");
                            GetFieldBoolean(ref overwriteList, attrib, "OverwriteList");
                            GetDataFormat(ref dataFormat, ppma, "DataFormat");
                            GetFieldBoolean(ref asReferenceHasValue, attrib, "AsReferenceHasValue", false);

                            if (asReferenceHasValue)
                            {
                                asReferenceHasValue = GetFieldBoolean(ref asReference, ppma, "AsReference", true);
                            }
                            GetFieldBoolean(ref dynamicType, ppma, "DynamicType");
                            if (done = tagIsPinned = fieldNumber > 0) break; // note minAcceptFieldNumber only applies to non-proto
                        }
                    }
                }
            }

            if (!ignore && !done && HasFamily(family, AttributeFamily.DataContractSerialier))
            {
                attrib = GetAttribute(attribs, "System.Runtime.Serialization.DataMemberAttribute");
                if (attrib != null)
                {
                    GetFieldNumber(ref fieldNumber, attrib, "Order");
                    GetFieldName(ref name, attrib, "Name");
                    GetFieldBoolean(ref isRequired, attrib, "IsRequired");
                    done = fieldNumber >= minAcceptFieldNumber;
                    if (done) fieldNumber += dataMemberOffset; // dataMemberOffset only applies to DCS flags, to allow us to "bump" WCF by a notch
                }
            }
            if (!ignore && !done && HasFamily(family, AttributeFamily.XmlSerializer))
            {
                attrib = GetAttribute(attribs, "System.Xml.Serialization.XmlElementAttribute");
                if (attrib == null) attrib = GetAttribute(attribs, "System.Xml.Serialization.XmlArrayAttribute");
                GetIgnore(ref ignore, attrib, attribs, "System.Xml.Serialization.XmlIgnoreAttribute");
                if (attrib != null && !ignore)
                {
                    GetFieldNumber(ref fieldNumber, attrib, "Order");
                    GetFieldName(ref name, attrib, "ElementName");
                    done = fieldNumber >= minAcceptFieldNumber;
                }
            }
            if (!ignore && !done)
            {
                if (GetAttribute(attribs, "System.NonSerializedAttribute") != null) ignore = true;
            }
            if (ignore || (fieldNumber < minAcceptFieldNumber && !forced)) return null;
            ProtoMemberAttribute result = new ProtoMemberAttribute(fieldNumber, forced || inferByTagName)
            {
                AsReference = asReference,
                AsReferenceHasValue = asReferenceHasValue,
                DataFormat = dataFormat,
                DynamicType = dynamicType,
                IsPacked = isPacked,
                OverwriteList = overwriteList,
                IsRequired = isRequired,
                Name = string.IsNullOrEmpty(name) ? member.Name : name,
                Member = member,
                BackingMember = backingMember,
                TagIsPinned = tagIsPinned
            };
            return result;
        }

        private ValueMember ApplyDefaultBehaviour(bool isEnum, ProtoMemberAttribute normalizedAttribute)
        {
            MemberInfo member;
            if (normalizedAttribute == null || (member = normalizedAttribute.Member) == null) return null; // nix

            Type effectiveType = Helpers.GetMemberType(member);


            Type itemType = null;
            Type defaultType = null;

            // check for list types
            ResolveListTypes(model, effectiveType, ref itemType, ref defaultType);
            bool ignoreListHandling = false;
            // but take it back if it is explicitly excluded
            if (itemType != null)
            { // looks like a list, but double check for IgnoreListHandling
                int idx = model.FindOrAddAuto(effectiveType, false, true, false);
                if (idx >= 0 && (ignoreListHandling = model[effectiveType].IgnoreListHandling))
                {
                    itemType = null;
                    defaultType = null;
                }
            }
            AttributeMap[] attribs = AttributeMap.Create(model, member, true);
            AttributeMap attrib;

            object defaultValue = null;
            // implicit zero default
            if (model.UseImplicitZeroDefaults)
            {
                switch (Helpers.GetTypeCode(effectiveType))
                {
                    case ProtoTypeCode.Boolean: defaultValue = false; break;
                    case ProtoTypeCode.Decimal: defaultValue = (decimal)0; break;
                    case ProtoTypeCode.Single: defaultValue = (float)0; break;
                    case ProtoTypeCode.Double: defaultValue = (double)0; break;
                    case ProtoTypeCode.Byte: defaultValue = (byte)0; break;
                    case ProtoTypeCode.Char: defaultValue = (char)0; break;
                    case ProtoTypeCode.Int16: defaultValue = (short)0; break;
                    case ProtoTypeCode.Int32: defaultValue = (int)0; break;
                    case ProtoTypeCode.Int64: defaultValue = (long)0; break;
                    case ProtoTypeCode.SByte: defaultValue = (sbyte)0; break;
                    case ProtoTypeCode.UInt16: defaultValue = (ushort)0; break;
                    case ProtoTypeCode.UInt32: defaultValue = (uint)0; break;
                    case ProtoTypeCode.UInt64: defaultValue = (ulong)0; break;
                    case ProtoTypeCode.TimeSpan: defaultValue = TimeSpan.Zero; break;
                    case ProtoTypeCode.Guid: defaultValue = Guid.Empty; break;
                }
            }
            if ((attrib = GetAttribute(attribs, "System.ComponentModel.DefaultValueAttribute")) != null)
            {
                if (attrib.TryGet("Value", out object tmp)) defaultValue = tmp;
            }
            ValueMember vm = ((isEnum || normalizedAttribute.Tag > 0))
                ? new ValueMember(model, type, normalizedAttribute.Tag, member, effectiveType, itemType, defaultType, normalizedAttribute.DataFormat, defaultValue)
                    : null;
            if (vm != null)
            {
                vm.BackingMember = normalizedAttribute.BackingMember;
#if COREFX || PROFILE259
				TypeInfo finalType = typeInfo;
#else
                Type finalType = type;
#endif
                PropertyInfo prop = Helpers.GetProperty(finalType, member.Name + "Specified", true);
                MethodInfo getMethod = Helpers.GetGetMethod(prop, true, true);
                if (getMethod == null || getMethod.IsStatic) prop = null;
                if (prop != null)
                {
                    vm.SetSpecified(getMethod, Helpers.GetSetMethod(prop, true, true));
                }
                else
                {
                    MethodInfo method = Helpers.GetInstanceMethod(finalType, "ShouldSerialize" + member.Name, Helpers.EmptyTypes);
                    if (method != null && method.ReturnType == model.MapType(typeof(bool)))
                    {
                        vm.SetSpecified(method, null);
                    }
                }
                if (!string.IsNullOrEmpty(normalizedAttribute.Name)) vm.SetName(normalizedAttribute.Name);
                vm.IsPacked = normalizedAttribute.IsPacked;
                vm.IsRequired = normalizedAttribute.IsRequired;
                vm.OverwriteList = normalizedAttribute.OverwriteList;
                if (normalizedAttribute.AsReferenceHasValue)
                {
                    vm.AsReference = normalizedAttribute.AsReference;
                }
                vm.DynamicType = normalizedAttribute.DynamicType;

                vm.IsMap = ignoreListHandling ? false : vm.ResolveMapTypes(out var _, out var _, out var _);
                if (vm.IsMap) // is it even *allowed* to be a map?
                {
                    if ((attrib = GetAttribute(attribs, "ProtoBuf.ProtoMapAttribute")) != null)
                    {
                        if (attrib.TryGet(nameof(ProtoMapAttribute.DisableMap), out object tmp) && (bool)tmp)
                        {
                            vm.IsMap = false;
                        }
                        else
                        {
                            if (attrib.TryGet(nameof(ProtoMapAttribute.KeyFormat), out tmp)) vm.MapKeyFormat = (DataFormat)tmp;
                            if (attrib.TryGet(nameof(ProtoMapAttribute.ValueFormat), out tmp)) vm.MapValueFormat = (DataFormat)tmp;
                        }
                    }
                }

            }
            return vm;
        }

        private static void GetDataFormat(ref DataFormat value, AttributeMap attrib, string memberName)
        {
            if ((attrib == null) || (value != DataFormat.Default)) return;
            if (attrib.TryGet(memberName, out object obj) && obj != null) value = (DataFormat)obj;
        }

        private static void GetIgnore(ref bool ignore, AttributeMap attrib, AttributeMap[] attribs, string fullName)
        {
            if (ignore || attrib == null) return;
            ignore = GetAttribute(attribs, fullName) != null;
            return;
        }

        private static void GetFieldBoolean(ref bool value, AttributeMap attrib, string memberName)
        {
            GetFieldBoolean(ref value, attrib, memberName, true);
        }
        private static bool GetFieldBoolean(ref bool value, AttributeMap attrib, string memberName, bool publicOnly)
        {
            if (attrib == null) return false;
            if (value) return true;
            if (attrib.TryGet(memberName, publicOnly, out object obj) && obj != null)
            {
                value = (bool)obj;
                return true;
            }
            return false;
        }

        private static void GetFieldNumber(ref int value, AttributeMap attrib, string memberName)
        {
            if (attrib == null || value > 0) return;
            if (attrib.TryGet(memberName, out object obj) && obj != null) value = (int)obj;
        }

        private static void GetFieldName(ref string name, AttributeMap attrib, string memberName)
        {
            if (attrib == null || !string.IsNullOrEmpty(name)) return;
            if (attrib.TryGet(memberName, out object obj) && obj != null) name = (string)obj;
        }

        private static AttributeMap GetAttribute(AttributeMap[] attribs, string fullName)
        {
            for (int i = 0; i < attribs.Length; i++)
            {
                AttributeMap attrib = attribs[i];
                if (attrib != null && attrib.AttributeType.FullName == fullName) return attrib;
            }
            return null;
        }

        /// <summary>
        /// Adds a member (by name) to the MetaType
        /// </summary>        
        public MetaType Add(int fieldNumber, string memberName)
        {
            AddField(fieldNumber, memberName, null, null, null);
            return this;
        }

        /// <summary>
        /// Adds a member (by name) to the MetaType, returning the ValueMember rather than the fluent API.
        /// This is otherwise identical to Add.
        /// </summary>
        public ValueMember AddField(int fieldNumber, string memberName)
        {
            return AddField(fieldNumber, memberName, null, null, null);
        }

        /// <summary>
        /// Gets or sets whether the type should use a parameterless constructor (the default),
        /// or whether the type should skip the constructor completely. This option is not supported
        /// on compact-framework.
        /// </summary>
        public bool UseConstructor
        { // negated to have defaults as flat zero
            get { return !HasFlag(OPTIONS_SkipConstructor); }
            set { SetFlag(OPTIONS_SkipConstructor, !value, true); }
        }

        /// <summary>
        /// The concrete type to create when a new instance of this type is needed; this may be useful when dealing
        /// with dynamic proxies, or with interface-based APIs
        /// </summary>
        public Type ConstructType
        {
            get { return constructType; }
            set
            {
                ThrowIfFrozen();
                constructType = value;
            }
        }

        private Type constructType;
        /// <summary>
        /// Adds a member (by name) to the MetaType
        /// </summary>     
        public MetaType Add(string memberName)
        {
            Add(GetNextFieldNumber(), memberName);
            return this;
        }

        Type surrogate;
        /// <summary>
        /// Performs serialization of this type via a surrogate; all
        /// other serialization options are ignored and handled
        /// by the surrogate's configuration.
        /// </summary>
        public void SetSurrogate(Type surrogateType)
        {
            if (surrogateType == type) surrogateType = null;
            if (surrogateType != null)
            {
                // note that BuildSerializer checks the **CURRENT TYPE** is OK to be surrogated
                if (surrogateType != null && Helpers.IsAssignableFrom(model.MapType(typeof(IEnumerable)), surrogateType))
                {
                    throw new ArgumentException("Repeated data (a list, collection, etc) has inbuilt behaviour and cannot be used as a surrogate");
                }
            }
            ThrowIfFrozen();
            this.surrogate = surrogateType;
            // no point in offering chaining; no options are respected
        }

        internal MetaType GetSurrogateOrSelf()
        {
            if (surrogate != null) return model[surrogate];
            return this;
        }

        internal MetaType GetSurrogateOrBaseOrSelf(bool deep)
        {
            if (surrogate != null) return model[surrogate];
            MetaType snapshot = this.baseType;
            if (snapshot != null)
            {
                if (deep)
                {
                    MetaType tmp;
                    do
                    {
                        tmp = snapshot;
                        snapshot = snapshot.baseType;
                    } while (snapshot != null);
                    return tmp;
                }
                return snapshot;
            }
            return this;
        }

        private int GetNextFieldNumber()
        {
            int maxField = 0;
            foreach (ValueMember member in fields)
            {
                if (member.FieldNumber > maxField) maxField = member.FieldNumber;
            }
            if (subTypes != null)
            {
                foreach (SubType subType in subTypes)
                {
                    if (subType.FieldNumber > maxField) maxField = subType.FieldNumber;
                }
            }
            return maxField + 1;
        }

        /// <summary>
        /// Adds a set of members (by name) to the MetaType
        /// </summary>     
        public MetaType Add(params string[] memberNames)
        {
            if (memberNames == null) throw new ArgumentNullException("memberNames");
            int next = GetNextFieldNumber();
            for (int i = 0; i < memberNames.Length; i++)
            {
                Add(next++, memberNames[i]);
            }
            return this;
        }

        /// <summary>
        /// Adds a member (by name) to the MetaType
        /// </summary>        
        public MetaType Add(int fieldNumber, string memberName, object defaultValue)
        {
            AddField(fieldNumber, memberName, null, null, defaultValue);
            return this;
        }

        /// <summary>
        /// Adds a member (by name) to the MetaType, including an itemType and defaultType for representing lists
        /// </summary>
        public MetaType Add(int fieldNumber, string memberName, Type itemType, Type defaultType)
        {
            AddField(fieldNumber, memberName, itemType, defaultType, null);
            return this;
        }

        /// <summary>
        /// Adds a member (by name) to the MetaType, including an itemType and defaultType for representing lists, returning the ValueMember rather than the fluent API.
        /// This is otherwise identical to Add.
        /// </summary>
        public ValueMember AddField(int fieldNumber, string memberName, Type itemType, Type defaultType)
        {
            return AddField(fieldNumber, memberName, itemType, defaultType, null);
        }

        private ValueMember AddField(int fieldNumber, string memberName, Type itemType, Type defaultType, object defaultValue)
        {
            MemberInfo mi = null;
#if PROFILE259
			mi = Helpers.IsEnum(type) ? type.GetTypeInfo().GetDeclaredField(memberName) : Helpers.GetInstanceMember(type.GetTypeInfo(), memberName);

#else
            MemberInfo[] members = type.GetMember(memberName, Helpers.IsEnum(type) ? BindingFlags.Static | BindingFlags.Public : BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (members != null && members.Length == 1) mi = members[0];
#endif
            if (mi == null) throw new ArgumentException("Unable to determine member: " + memberName, "memberName");

            Type miType;
            PropertyInfo pi = null;
            FieldInfo fi = null;
#if PORTABLE || COREFX || PROFILE259
			pi = mi as PropertyInfo;
            if (pi == null)
            {
                fi = mi as FieldInfo;
                if (fi == null)
                {
                    throw new NotSupportedException(mi.GetType().Name);
                }
                else
                {
                    miType = fi.FieldType;
                }
            }
            else
            {
                miType = pi.PropertyType;
            }
#else
            switch (mi.MemberType)
            {
                case MemberTypes.Field:
                    fi = (FieldInfo)mi;
                    miType = fi.FieldType; break;
                case MemberTypes.Property:
                    pi = (PropertyInfo)mi;
                    miType = pi.PropertyType; break;
                default:
                    throw new NotSupportedException(mi.MemberType.ToString());
            }
#endif
            ResolveListTypes(model, miType, ref itemType, ref defaultType);

            MemberInfo backingField = null;
            if (pi?.CanWrite == false)
            {
                string name = $"<{((PropertyInfo)mi).Name}>k__BackingField";
#if PROFILE259
				var backingMembers = type.GetTypeInfo().DeclaredMembers;
	            var memberInfos = backingMembers as MemberInfo[] ?? backingMembers.ToArray();
	            if (memberInfos.Count() == 1)
	            {
		            MemberInfo first = memberInfos.FirstOrDefault();
		            if (first is FieldInfo)
		            {
			            backingField = first;
		            }
	            }
#else
                var backingMembers = type.GetMember($"<{((PropertyInfo)mi).Name}>k__BackingField", Helpers.IsEnum(type) ? BindingFlags.Static | BindingFlags.Public : BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (backingMembers != null && backingMembers.Length == 1 && (backingMembers[0] as FieldInfo) != null)
                    backingField = backingMembers[0];
#endif
            }
            ValueMember newField = new ValueMember(model, type, fieldNumber, backingField ?? mi, miType, itemType, defaultType, DataFormat.Default, defaultValue);
            if (backingField != null)
                newField.SetName(mi.Name);
            Add(newField);
            return newField;
        }

        internal static void ResolveListTypes(TypeModel model, Type type, ref Type itemType, ref Type defaultType)
        {
            if (type == null) return;
            // handle arrays
            if (type.IsArray)
            {
                if (type.GetArrayRank() != 1)
                {
                    throw new NotSupportedException("Multi-dimensional arrays are not supported");
                }
                itemType = type.GetElementType();
                if (itemType == model.MapType(typeof(byte)))
                {
                    defaultType = itemType = null;
                }
                else
                {
                    defaultType = type;
                }
            }
            // handle lists
            if (itemType == null) { itemType = TypeModel.GetListItemType(model, type); }

            // check for nested data (not allowed)
            if (itemType != null)
            {
                Type nestedItemType = null, nestedDefaultType = null;
                ResolveListTypes(model, itemType, ref nestedItemType, ref nestedDefaultType);
                if (nestedItemType != null)
                {
                    throw TypeModel.CreateNestedListsNotSupported(type);
                }
            }

            if (itemType != null && defaultType == null)
            {
#if COREFX || PROFILE259
				TypeInfo typeInfo = type.GetTypeInfo();
                if (typeInfo.IsClass && !typeInfo.IsAbstract && Helpers.GetConstructor(typeInfo, Helpers.EmptyTypes, true) != null)
#else
                if (type.IsClass && !type.IsAbstract && Helpers.GetConstructor(type, Helpers.EmptyTypes, true) != null)
#endif
                {
                    defaultType = type;
                }
                if (defaultType == null)
                {
#if COREFX || PROFILE259
					if (typeInfo.IsInterface)
#else
                    if (type.IsInterface)
#endif
                    {

                        Type[] genArgs;
#if COREFX || PROFILE259
						if (typeInfo.IsGenericType && type.GetGenericTypeDefinition() == typeof(System.Collections.Generic.IDictionary<,>)
                            && itemType == typeof(System.Collections.Generic.KeyValuePair<,>).MakeGenericType(genArgs = typeInfo.GenericTypeArguments))
#else
                        if (type.IsGenericType && type.GetGenericTypeDefinition() == model.MapType(typeof(System.Collections.Generic.IDictionary<,>))
                            && itemType == model.MapType(typeof(System.Collections.Generic.KeyValuePair<,>)).MakeGenericType(genArgs = type.GetGenericArguments()))
#endif
                        {
                            defaultType = model.MapType(typeof(System.Collections.Generic.Dictionary<,>)).MakeGenericType(genArgs);
                        }
                        else
                        {
                            defaultType = model.MapType(typeof(System.Collections.Generic.List<>)).MakeGenericType(itemType);
                        }
                    }
                }
                // verify that the default type is appropriate
                if (defaultType != null && !Helpers.IsAssignableFrom(type, defaultType)) { defaultType = null; }
            }
        }

        private void Add(ValueMember member)
        {
            int opaqueToken = 0;
            try
            {
                model.TakeLock(ref opaqueToken);
                ThrowIfFrozen();
                fields.Add(member);
            }
            finally
            {
                model.ReleaseLock(opaqueToken);
            }
        }

        /// <summary>
        /// Returns the ValueMember that matchs a given field number, or null if not found
        /// </summary>
        public ValueMember this[int fieldNumber]
        {
            get
            {
                foreach (ValueMember member in fields)
                {
                    if (member.FieldNumber == fieldNumber) return member;
                }
                return null;
            }
        }
        /// <summary>
        /// Returns the ValueMember that matchs a given member (property/field), or null if not found
        /// </summary>
        public ValueMember this[MemberInfo member]
        {
            get
            {
                if (member == null) return null;
                foreach (ValueMember x in fields)
                {
                    if (x.Member == member || x.BackingMember == member) return x;
                }
                return null;
            }
        }
        private readonly BasicList fields = new BasicList();

        /// <summary>
        /// Returns the ValueMember instances associated with this type
        /// </summary>
        public ValueMember[] GetFields()
        {
            ValueMember[] arr = new ValueMember[fields.Count];
            fields.CopyTo(arr, 0);
            Array.Sort(arr, ValueMember.Comparer.Default);
            return arr;
        }

        /// <summary>
        /// Returns the SubType instances associated with this type
        /// </summary>
        public SubType[] GetSubtypes()
        {
            if (subTypes == null || subTypes.Count == 0) return new SubType[0];
            SubType[] arr = new SubType[subTypes.Count];
            subTypes.CopyTo(arr, 0);
            Array.Sort(arr, SubType.Comparer.Default);
            return arr;
        }

        internal IEnumerable<Type> GetAllGenericArguments()
        {
            return GetAllGenericArguments(type);
        }

        private static IEnumerable<Type> GetAllGenericArguments(Type type)
        {

#if PROFILE259
	        var genericArguments = type.GetGenericTypeDefinition().GenericTypeArguments;
#else
            var genericArguments = type.GetGenericArguments();
#endif
            foreach (var arg in genericArguments)
            {
                yield return arg;
                foreach (var inner in GetAllGenericArguments(arg))
                {
                    yield return inner;
                }
            }
        }

#if FEAT_COMPILER
        /// <summary>
        /// Compiles the serializer for this type; this is *not* a full
        /// standalone compile, but can significantly boost performance
        /// while allowing additional types to be added.
        /// </summary>
        /// <remarks>An in-place compile can access non-public types / members</remarks>
        public void CompileInPlace()
        {
            serializer = CompiledSerializer.Wrap(Serializer, model);
        }
#endif

        internal bool IsDefined(int fieldNumber)
        {
            foreach (ValueMember field in fields)
            {
                if (field.FieldNumber == fieldNumber) return true;
            }
            return false;
        }

        internal int GetKey(bool demand, bool getBaseKey)
        {
            return model.GetKey(type, demand, getBaseKey);
        }

        internal EnumSerializer.EnumPair[] GetEnumMap()
        {
            if (HasFlag(OPTIONS_EnumPassThru)) return null;
            EnumSerializer.EnumPair[] result = new EnumSerializer.EnumPair[fields.Count];
            for (int i = 0; i < result.Length; i++)
            {
                ValueMember member = (ValueMember)fields[i];
                int wireValue = member.FieldNumber;
                object value = member.GetRawEnumValue();
                result[i] = new EnumSerializer.EnumPair(wireValue, value, member.MemberType);
            }
            return result;
        }

        /// <summary>
        /// Gets or sets a value indicating that an enum should be treated directly as an int/short/etc, rather
        /// than enforcing .proto enum rules. This is useful *in particul* for [Flags] enums.
        /// </summary>
        public bool EnumPassthru
        {
            get { return HasFlag(OPTIONS_EnumPassThru); }
            set { SetFlag(OPTIONS_EnumPassThru, value, true); }
        }

        /// <summary>
        /// Gets or sets a value indicating that this type should NOT be treated as a list, even if it has
        /// familiar list-like characteristics (enumerable, add, etc)
        /// </summary>
        public bool IgnoreListHandling
        {
            get { return HasFlag(OPTIONS_IgnoreListHandling); }
            set { SetFlag(OPTIONS_IgnoreListHandling, value, true); }
        }

        internal bool Pending
        {
            get { return HasFlag(OPTIONS_Pending); }
            set { SetFlag(OPTIONS_Pending, value, false); }
        }

        private const ushort
            OPTIONS_Pending = 1,
            OPTIONS_EnumPassThru = 2,
            OPTIONS_Frozen = 4,
            OPTIONS_PrivateOnApi = 8,
            OPTIONS_SkipConstructor = 16,
            OPTIONS_AsReferenceDefault = 32,
            OPTIONS_AutoTuple = 64,
            OPTIONS_IgnoreListHandling = 128,
            OPTIONS_IsGroup = 256;

        private volatile ushort flags;
        private bool HasFlag(ushort flag) { return (flags & flag) == flag; }
        private void SetFlag(ushort flag, bool value, bool throwIfFrozen)
        {
            if (throwIfFrozen && HasFlag(flag) != value)
            {
                ThrowIfFrozen();
            }
            if (value)
                flags |= flag;
            else
                flags = (ushort)(flags & ~flag);
        }

        internal static MetaType GetRootType(MetaType source)
        {
            while (source.serializer != null)
            {
                MetaType tmp = source.baseType;
                if (tmp == null) return source;
                source = tmp; // else loop until we reach something that isn't generated, or is the root
            }

            // now we get into uncertain territory
            RuntimeTypeModel model = source.model;
            int opaqueToken = 0;
            try
            {
                model.TakeLock(ref opaqueToken);

                MetaType tmp;
                while ((tmp = source.baseType) != null) source = tmp;
                return source;

            }
            finally
            {
                model.ReleaseLock(opaqueToken);
            }
        }

        internal bool IsPrepared()
        {
#if FEAT_COMPILER
            return serializer is CompiledSerializer;
#else
            return false;
#endif
        }

        internal IEnumerable Fields => this.fields;

        internal static StringBuilder NewLine(StringBuilder builder, int indent)
        {
            return Helpers.AppendLine(builder).Append(' ', indent * 3);
        }

        internal bool IsAutoTuple => HasFlag(OPTIONS_AutoTuple);

        /// <summary>
        /// Indicates whether this type should always be treated as a "group" (rather than a string-prefixed sub-message)
        /// </summary>
        public bool IsGroup
        {
            get { return HasFlag(OPTIONS_IsGroup); }
            set { SetFlag(OPTIONS_IsGroup, value, true); }
        }

        internal void WriteSchema(StringBuilder builder, int indent, ref RuntimeTypeModel.CommonImports imports, ProtoSyntax syntax)
        {
            if (surrogate != null) return; // nothing to write

            ValueMember[] fieldsArr = new ValueMember[fields.Count];
            fields.CopyTo(fieldsArr, 0);
            Array.Sort(fieldsArr, ValueMember.Comparer.Default);

            if (IsList)
            {
                string itemTypeName = model.GetSchemaTypeName(TypeModel.GetListItemType(model, type), DataFormat.Default, false, false, ref imports);
                NewLine(builder, indent).Append("message ").Append(GetSchemaTypeName()).Append(" {");
                NewLine(builder, indent + 1).Append("repeated ").Append(itemTypeName).Append(" items = 1;");
                NewLine(builder, indent).Append('}');
            }
            else if (IsAutoTuple)
            { // key-value-pair etc

                if (ResolveTupleConstructor(type, out MemberInfo[] mapping) != null)
                {
                    NewLine(builder, indent).Append("message ").Append(GetSchemaTypeName()).Append(" {");
                    for (int i = 0; i < mapping.Length; i++)
                    {
                        Type effectiveType;
                        if (mapping[i] is PropertyInfo property)
                        {
                            effectiveType = property.PropertyType;
                        }
                        else if (mapping[i] is FieldInfo field)
                        {
                            effectiveType = field.FieldType;
                        }
                        else
                        {
                            throw new NotSupportedException("Unknown member type: " + mapping[i].GetType().Name);
                        }
                        NewLine(builder, indent + 1).Append(syntax == ProtoSyntax.Proto2 ? "optional " : "").Append(model.GetSchemaTypeName(effectiveType, DataFormat.Default, false, false, ref imports).Replace('.', '_'))
                            .Append(' ').Append(mapping[i].Name).Append(" = ").Append(i + 1).Append(';');
                    }
                    NewLine(builder, indent).Append('}');
                }
            }
            else if (Helpers.IsEnum(type))
            {
                NewLine(builder, indent).Append("enum ").Append(GetSchemaTypeName()).Append(" {");
                if (fieldsArr.Length == 0 && EnumPassthru)
                {
                    if (type
#if COREFX || PROFILE259
					.GetTypeInfo()
#endif
.IsDefined(model.MapType(typeof(FlagsAttribute)), false))
                    {
                        NewLine(builder, indent + 1).Append("// this is a composite/flags enumeration");
                    }
                    else
                    {
                        NewLine(builder, indent + 1).Append("// this enumeration will be passed as a raw value");
                    }
                    foreach (FieldInfo field in
#if PROFILE259
						type.GetRuntimeFields()
#else
                        type.GetFields()
#endif

                        )
                    {
                        if (field.IsStatic && field.IsLiteral)
                        {
                            object enumVal;
#if PORTABLE || CF || NETSTANDARD1_3 || NETSTANDARD1_4 || PROFILE259 || UAP
							enumVal = Convert.ChangeType(field.GetValue(null), Enum.GetUnderlyingType(field.FieldType), System.Globalization.CultureInfo.InvariantCulture);
#else
                            enumVal = field.GetRawConstantValue();
#endif
                            NewLine(builder, indent + 1).Append(field.Name).Append(" = ").Append(enumVal).Append(";");
                        }
                    }

                }
                else
                {
                    Dictionary<int, int> countByField = new Dictionary<int, int>(fieldsArr.Length);
                    bool needsAlias = false;
                    foreach (var field in fieldsArr)
                    {
                        if (countByField.ContainsKey(field.FieldNumber))
                        {  // no point actually counting; that's enough to know we have a problem
                            needsAlias = true;
                            break;
                        }
                        countByField.Add(field.FieldNumber, 1);
                    }
                    if (needsAlias)
                    {   // duplicated value requires allow_alias
                        NewLine(builder, indent + 1).Append("option allow_alias = true;");
                    }

                    bool haveWrittenZero = false;
                    // write zero values **first**
                    foreach (ValueMember member in fieldsArr)
                    {
                        if (member.FieldNumber == 0)
                        {
                            NewLine(builder, indent + 1).Append(member.Name).Append(" = ").Append(member.FieldNumber).Append(';');
                            haveWrittenZero = true;
                        }
                    }
                    if (syntax == ProtoSyntax.Proto3 && !haveWrittenZero)
                    {
                        NewLine(builder, indent + 1).Append("ZERO = 0; // proto3 requires a zero value as the first item (it can be named anything)");
                    }
                    // note array is already sorted, so zero would already be first
                    foreach (ValueMember member in fieldsArr)
                    {
                        if (member.FieldNumber == 0) continue;
                        NewLine(builder, indent + 1).Append(member.Name).Append(" = ").Append(member.FieldNumber).Append(';');
                    }
                }
                NewLine(builder, indent).Append('}');
            }
            else
            {
                NewLine(builder, indent).Append("message ").Append(GetSchemaTypeName()).Append(" {");
                foreach (ValueMember member in fieldsArr)
                {
                    string schemaTypeName;
                    bool hasOption = false;
                    if (member.IsMap)
                    {
                        member.ResolveMapTypes(out var _, out var keyType, out var valueType);

                        var keyTypeName = model.GetSchemaTypeName(keyType, member.MapKeyFormat, false, false, ref imports);
                        schemaTypeName = model.GetSchemaTypeName(valueType, member.MapKeyFormat, member.AsReference, member.DynamicType, ref imports);
                        NewLine(builder, indent + 1).Append("map<").Append(keyTypeName).Append(",").Append(schemaTypeName).Append("> ")
                            .Append(member.Name).Append(" = ").Append(member.FieldNumber).Append(";");
                    }
                    else
                    {
                        string ordinality = member.ItemType != null ? "repeated " : (syntax == ProtoSyntax.Proto2 ? (member.IsRequired ? "required " : "optional ") : "");
                        NewLine(builder, indent + 1).Append(ordinality);
                        if (member.DataFormat == DataFormat.Group) builder.Append("group ");
                        schemaTypeName = member.GetSchemaTypeName(true, ref imports);
                        builder.Append(schemaTypeName).Append(" ")
                             .Append(member.Name).Append(" = ").Append(member.FieldNumber);

                        if (syntax == ProtoSyntax.Proto2 && member.DefaultValue != null && member.IsRequired == false)
                        {
                            if (member.DefaultValue is string)
                            {
                                AddOption(builder, ref hasOption).Append("default = \"").Append(member.DefaultValue).Append("\"");
                            }
                            else if (member.DefaultValue is TimeSpan)
                            {
                                // ignore
                            }
                            else if (member.DefaultValue is bool)
                            {   // need to be lower case (issue 304)
                                AddOption(builder, ref hasOption).Append((bool)member.DefaultValue ? "default = true" : "default = false");
                            }
                            else
                            {
                                AddOption(builder, ref hasOption).Append("default = ").Append(member.DefaultValue);
                            }
                        }
                        if (CanPack(member.ItemType))
                        {
                            if (syntax == ProtoSyntax.Proto2)
                            {
                                if (member.IsPacked) AddOption(builder, ref hasOption).Append("packed = true"); // disabled by default
                            }
                            else
                            {
                                if (!member.IsPacked) AddOption(builder, ref hasOption).Append("packed = false"); // enabled by default
                            }
                        }
                        if (member.AsReference)
                        {
                            imports |= RuntimeTypeModel.CommonImports.Protogen;
                            AddOption(builder, ref hasOption).Append("(.protobuf_net.fieldopt).asRef = true");
                        }
                        if (member.DynamicType)
                        {
                            imports |= RuntimeTypeModel.CommonImports.Protogen;
                            AddOption(builder, ref hasOption).Append("(.protobuf_net.fieldopt).dynamicType = true");
                        }
                        CloseOption(builder, ref hasOption).Append(';');
                        if (syntax != ProtoSyntax.Proto2 && member.DefaultValue != null && !member.IsRequired)
                        {
                            if (IsImplicitDefault(member.DefaultValue))
                            {
                                // don't emit; we're good
                            }
                            else
                            {
                                builder.Append(" // default value could not be applied: ").Append(member.DefaultValue);
                            }
                        }
                    }
                    if (schemaTypeName == ".bcl.NetObjectProxy" && member.AsReference && !member.DynamicType) // we know what it is; tell the user
                    {
                        builder.Append(" // reference-tracked ").Append(member.GetSchemaTypeName(false, ref imports));
                    }
                }
                if (subTypes != null && subTypes.Count != 0)
                {
                    SubType[] subTypeArr = new SubType[subTypes.Count];
                    subTypes.CopyTo(subTypeArr, 0);
                    Array.Sort(subTypeArr, SubType.Comparer.Default);
                    string[] fieldNames = new string[subTypeArr.Length];
                    for(int i = 0; i < subTypeArr.Length;i++)
                        fieldNames[i] = subTypeArr[i].DerivedType.GetSchemaTypeName();

                    string fieldName = "subtype";
                    while (Array.IndexOf(fieldNames, fieldName) >= 0)
                        fieldName = "_" + fieldName;

                    NewLine(builder, indent + 1).Append("oneof ").Append(fieldName).Append(" {");
                    for(int i = 0; i < subTypeArr.Length; i++)
                    {
                        var subTypeName = fieldNames[i];
                        NewLine(builder, indent + 2).Append(subTypeName)
                               .Append(" ").Append(subTypeName).Append(" = ").Append(subTypeArr[i].FieldNumber).Append(';');
                    }
                    NewLine(builder, indent + 1).Append("}");
                }
                NewLine(builder, indent).Append('}');
            }
        }

        private static StringBuilder AddOption(StringBuilder builder, ref bool hasOption)
        {
            if (hasOption)
                return builder.Append(", ");
            hasOption = true;
            return builder.Append(" [");
        }

        private static StringBuilder CloseOption(StringBuilder builder, ref bool hasOption)
        {
            if (hasOption)
            {
                hasOption = false;
                return builder.Append("]");
            }
            return builder;
        }

        private static bool IsImplicitDefault(object value)
        {
            try
            {
                if (value == null) return false;
                switch (Helpers.GetTypeCode(value.GetType()))
                {
                    case ProtoTypeCode.Boolean: return ((bool)value) == false;
                    case ProtoTypeCode.Byte: return ((byte)value) == (byte)0;
                    case ProtoTypeCode.Char: return ((char)value) == (char)0;
                    case ProtoTypeCode.DateTime: return ((DateTime)value) == default;
                    case ProtoTypeCode.Decimal: return ((decimal)value) == 0M;
                    case ProtoTypeCode.Double: return ((double)value) == (double)0;
                    case ProtoTypeCode.Int16: return ((short)value) == (short)0;
                    case ProtoTypeCode.Int32: return ((int)value) == (int)0;
                    case ProtoTypeCode.Int64: return ((long)value) == (long)0;
                    case ProtoTypeCode.SByte: return ((sbyte)value) == (sbyte)0;
                    case ProtoTypeCode.Single: return ((float)value) == (float)0;
                    case ProtoTypeCode.String: return ((string)value) == "";
                    case ProtoTypeCode.TimeSpan: return ((TimeSpan)value) == TimeSpan.Zero;
                    case ProtoTypeCode.UInt16: return ((ushort)value) == (ushort)0;
                    case ProtoTypeCode.UInt32: return ((uint)value) == (uint)0;
                    case ProtoTypeCode.UInt64: return ((ulong)value) == (ulong)0;
                }
            }
            catch { }
            return false;
        }

        private static bool CanPack(Type type)
        {
            if (type == null) return false;
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
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Apply a shift to all fields (and sub-types) on this type
        /// </summary>
        /// <param name="offset">The change in field number to apply</param>
        /// <remarks>The resultant field numbers must still all be considered valid</remarks>
#if !(NETSTANDARD1_0 || NETSTANDARD1_3 || UAP)
        [System.ComponentModel.Browsable(false)]
#endif
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Advanced)]
        public void ApplyFieldOffset(int offset)
        {
            if (Helpers.IsEnum(type)) throw new InvalidOperationException("Cannot apply field-offset to an enum");
            if (offset == 0) return; // nothing to do
            int opaqueToken = 0;
            try
            {
                model.TakeLock(ref opaqueToken);
                ThrowIfFrozen();

                if (fields != null)
                {
                    foreach(ValueMember field in fields)
                        AssertValidFieldNumber(field.FieldNumber + offset);
                }
                if (subTypes != null)
                {
                    foreach (SubType subType in subTypes)
                        AssertValidFieldNumber(subType.FieldNumber + offset);
                }

                // we've checked the ranges are all OK; since we're moving everything, we can't overlap ourselves
                // so: we can just move
                if (fields != null)
                {
                    foreach (ValueMember field in fields)
                        field.FieldNumber += offset;
                }
                if (subTypes != null)
                {
                    foreach (SubType subType in subTypes)
                        subType.FieldNumber += offset;
                }
            }
            finally
            {
                model.ReleaseLock(opaqueToken);
            }
        }

        internal static void AssertValidFieldNumber(int fieldNumber)
        {
            if (fieldNumber < 1) throw new ArgumentOutOfRangeException(nameof(fieldNumber));
        }
    }
}
#endif

﻿#if !NO_RUNTIME
using System;

using ProtoBuf.Serializers;
using System.Globalization;
using System.Collections.Generic;

#if FEAT_IKVM
using Type = IKVM.Reflection.Type;
using IKVM.Reflection;
#else
using System.Reflection;
#endif

namespace ProtoBuf.Meta
{
    /// <summary>
    /// Represents a member (property/field) that is mapped to a protobuf field
    /// </summary>
    public class ValueMember
    {
        private readonly int fieldNumber;
        /// <summary>
        /// The number that identifies this member in a protobuf stream
        /// </summary>
        public int FieldNumber { get { return fieldNumber; } }
        private readonly MemberInfo originalMember;
        private MemberInfo backingMember;
        /// <summary>
        /// Gets the member (field/property) which this member relates to.
        /// </summary>
        public MemberInfo Member { get { return originalMember; } }
        /// <summary>
        /// Gets the backing member (field/property) which this member relates to
        /// </summary>
        public MemberInfo BackingMember
        {
            get { return backingMember; }
            set
            {
                if(backingMember != value)
                {
                    ThrowIfFrozen();
                    backingMember = value;
                }
            }
        }


        private readonly Type parentType, itemType, defaultType, memberType;
        private object defaultValue;
        /// <summary>
        /// Within a list / array / etc, the type of object for each item in the list (especially useful with ArrayList)
        /// </summary>
        public Type ItemType { get { return itemType; } }
        /// <summary>
        /// The underlying type of the member
        /// </summary>
        public Type MemberType { get { return memberType; } }
        /// <summary>
        /// For abstract types (IList etc), the type of concrete object to create (if required)
        /// </summary>
        public Type DefaultType { get { return defaultType; } }
        /// <summary>
        /// The type the defines the member
        /// </summary>
        public Type ParentType { get { return parentType; } }

        /// <summary>
        /// The default value of the item (members with this value will not be serialized)
        /// </summary>
        public object DefaultValue
        {
            get { return defaultValue; }
            set {
                if (defaultValue != value)
                {
                    ThrowIfFrozen();
                    defaultValue = value;
                }
            }
        }

        private readonly RuntimeTypeModel model;
        /// <summary>
        /// Creates a new ValueMember instance
        /// </summary>
        public ValueMember(RuntimeTypeModel model, Type parentType, int fieldNumber, MemberInfo member, Type memberType, Type itemType, Type defaultType, DataFormat dataFormat, object defaultValue) 
            : this(model, fieldNumber,memberType, itemType, defaultType, dataFormat)
        {
            if (member == null) throw new ArgumentNullException("member");
            if (parentType == null) throw new ArgumentNullException("parentType");
            if (fieldNumber < 1 && !Helpers.IsEnum(parentType)) throw new ArgumentOutOfRangeException("fieldNumber");

            this.originalMember = member;
            this.parentType = parentType;
            if (fieldNumber < 1 && !Helpers.IsEnum(parentType)) throw new ArgumentOutOfRangeException("fieldNumber");
//#if WINRT
            if (defaultValue != null && model.MapType(defaultValue.GetType()) != memberType)
//#else
//            if (defaultValue != null && !memberType.IsInstanceOfType(defaultValue))
//#endif
            {
                defaultValue = ParseDefaultValue(memberType, defaultValue);
            }
            this.defaultValue = defaultValue;

            MetaType type = model.FindWithoutAdd(memberType);
            if (type != null)
            {
                AsReference = type.AsReferenceDefault;
            }
            else
            { // we need to scan the hard way; can't risk recursion by fully walking it
                AsReference = MetaType.GetAsReferenceDefault(model, memberType);
            }
        }
        /// <summary>
        /// Creates a new ValueMember instance
        /// </summary>
        internal ValueMember(RuntimeTypeModel model, int fieldNumber, Type memberType, Type itemType, Type defaultType, DataFormat dataFormat) 
        {
            if (memberType == null) throw new ArgumentNullException("memberType");
            if (model == null) throw new ArgumentNullException("model");
            this.fieldNumber = fieldNumber;
            this.memberType = memberType;
            this.itemType = itemType;
            this.defaultType = defaultType;

            this.model = model;
            this.dataFormat = dataFormat;
        }
        internal object GetRawEnumValue()
        {
#if WINRT || PORTABLE || CF || FX11 || COREFX
            object value = ((FieldInfo)originalMember).GetValue(null);
            switch(Helpers.GetTypeCode(Enum.GetUnderlyingType(((FieldInfo)originalMember).FieldType)))
            {
                case ProtoTypeCode.SByte: return (sbyte)value;
                case ProtoTypeCode.Byte: return (byte)value;
                case ProtoTypeCode.Int16: return (short)value;
                case ProtoTypeCode.UInt16: return (ushort)value;
                case ProtoTypeCode.Int32: return (int)value;
                case ProtoTypeCode.UInt32: return (uint)value;
                case ProtoTypeCode.Int64: return (long)value;
                case ProtoTypeCode.UInt64: return (ulong)value;
                default:
                    throw new InvalidOperationException();
            }
#else
            return ((FieldInfo)originalMember).GetRawConstantValue();
#endif
        }
        private static object ParseDefaultValue(Type type, object value)
        {
			if(true)
            {
                Type tmp = Helpers.GetUnderlyingType(type);
                if (tmp != null) type = tmp;
            }
			switch (Helpers.GetTypeCode(type))
			{
			case ProtoTypeCode.Boolean: 
			case ProtoTypeCode.Byte: 
			case ProtoTypeCode.Char: // char.Parse missing on CF/phone7
			case ProtoTypeCode.DateTime: 
			case ProtoTypeCode.Decimal: 
			case ProtoTypeCode.Double: 
			case ProtoTypeCode.Int16: 
			case ProtoTypeCode.Int32: 
			case ProtoTypeCode.Int64: 
			case ProtoTypeCode.SByte: 
			case ProtoTypeCode.Single: 
			case ProtoTypeCode.String:
			case ProtoTypeCode.UInt16:
			case ProtoTypeCode.UInt32:
			case ProtoTypeCode.UInt64:
			case ProtoTypeCode.TimeSpan:
			case ProtoTypeCode.Uri:
			case ProtoTypeCode.Guid:
				{
					value = value + "";
				}
				break;
			}
            if (value is string)
            {
                string s = (string)value;
                if (Helpers.IsEnum(type)) return Helpers.ParseEnum(type, s);

                switch (Helpers.GetTypeCode(type))
                {
                    case ProtoTypeCode.Boolean: return bool.Parse(s);
                    case ProtoTypeCode.Byte: return byte.Parse(s, NumberStyles.Integer, CultureInfo.InvariantCulture);
                    case ProtoTypeCode.Char: // char.Parse missing on CF/phone7
                        if (s.Length == 1) return s[0];
                        throw new FormatException("Single character expected: \"" + s + "\"");
                    case ProtoTypeCode.DateTime: return DateTime.Parse(s, CultureInfo.InvariantCulture);
                    case ProtoTypeCode.Decimal: return decimal.Parse(s, NumberStyles.Any, CultureInfo.InvariantCulture);
                    case ProtoTypeCode.Double: return double.Parse(s, NumberStyles.Any, CultureInfo.InvariantCulture);
                    case ProtoTypeCode.Int16: return short.Parse(s, NumberStyles.Any, CultureInfo.InvariantCulture);
                    case ProtoTypeCode.Int32: return int.Parse(s, NumberStyles.Any, CultureInfo.InvariantCulture);
                    case ProtoTypeCode.Int64: return long.Parse(s, NumberStyles.Any, CultureInfo.InvariantCulture);
                    case ProtoTypeCode.SByte: return sbyte.Parse(s, NumberStyles.Integer, CultureInfo.InvariantCulture);
                    case ProtoTypeCode.Single: return float.Parse(s, NumberStyles.Any, CultureInfo.InvariantCulture);
                    case ProtoTypeCode.String: return s;
                    case ProtoTypeCode.UInt16: return ushort.Parse(s, NumberStyles.Any, CultureInfo.InvariantCulture);
                    case ProtoTypeCode.UInt32: return uint.Parse(s, NumberStyles.Any, CultureInfo.InvariantCulture);
                    case ProtoTypeCode.UInt64: return ulong.Parse(s, NumberStyles.Any, CultureInfo.InvariantCulture);
                    case ProtoTypeCode.TimeSpan: return TimeSpan.Parse(s);
                    case ProtoTypeCode.Uri: return s; // Uri is decorated as string
                    case ProtoTypeCode.Guid: return new Guid(s);
                }
            }
#if FEAT_IKVM
            if (Helpers.IsEnum(type)) return value; // return the underlying type instead
            System.Type convertType = null;
            switch(Helpers.GetTypeCode(type))
            {
                case ProtoTypeCode.SByte: convertType = typeof(sbyte); break;
                case ProtoTypeCode.Int16: convertType = typeof(short); break;
                case ProtoTypeCode.Int32: convertType = typeof(int); break;
                case ProtoTypeCode.Int64: convertType = typeof(long); break;
                case ProtoTypeCode.Byte: convertType = typeof(byte); break;
                case ProtoTypeCode.UInt16: convertType = typeof(ushort); break;
                case ProtoTypeCode.UInt32: convertType = typeof(uint); break;
                case ProtoTypeCode.UInt64: convertType = typeof(ulong); break;
                case ProtoTypeCode.Single: convertType = typeof(float); break;
                case ProtoTypeCode.Double: convertType = typeof(double); break;
                case ProtoTypeCode.Decimal: convertType = typeof(decimal); break;
            }
            if(convertType != null) return Convert.ChangeType(value, convertType, CultureInfo.InvariantCulture);
            throw new ArgumentException("Unable to process default value: " + value + ", " + type.FullName);
#else
            if (Helpers.IsEnum(type)) return Enum.ToObject(type, value);
            return Convert.ChangeType(value, type, CultureInfo.InvariantCulture);
#endif
        }

        private IProtoSerializer serializer;
        internal IProtoSerializer Serializer
        {
            get
            {
                if (serializer == null) serializer = BuildSerializer();
                return serializer;
            }
        }

        private DataFormat dataFormat;
        /// <summary>
        /// Specifies the rules used to process the field; this is used to determine the most appropriate
        /// wite-type, but also to describe subtypes <i>within</i> that wire-type (such as SignedVariant)
        /// </summary>
        public DataFormat DataFormat {
            get { return dataFormat; }
            set
            {
                if (value != dataFormat)
                {
                    ThrowIfFrozen();
                    this.dataFormat = value;
                }
            }
        }

        /// <summary>
        /// Indicates whether this field should follow strict encoding rules; this means (for example) that if a "fixed32"
        /// is encountered when "variant" is defined, then it will fail (throw an exception) when parsing. Note that
        /// when serializing the defined type is always used.
        /// </summary>
        public bool IsStrict
        {
            get { return HasFlag(OPTIONS_IsStrict); }
            set { SetFlag(OPTIONS_IsStrict, value, true); }
        }

        /// <summary>
        /// Indicates whether this field should use packed encoding (which can save lots of space for repeated primitive values).
        /// This option only applies to list/array data of primitive types (int, double, etc).
        /// </summary>
        public bool IsPacked
        {
            get { return HasFlag(OPTIONS_IsPacked); }
            set { SetFlag(OPTIONS_IsPacked, value, true); }
        }

        /// <summary>
        /// Indicates whether this field should *repace* existing values (the default is false, meaning *append*).
        /// This option only applies to list/array data.
        /// </summary>
        public bool OverwriteList
        {
            get { return HasFlag(OPTIONS_OverwriteList); }
            set { SetFlag(OPTIONS_OverwriteList, value, true); }
        }

        /// <summary>
        /// Indicates whether this field is mandatory.
        /// </summary>
        public bool IsRequired
        {
            get { return HasFlag(OPTIONS_IsRequired); }
            set { SetFlag(OPTIONS_IsRequired, value, true); }
        }

        /// <summary>
        /// Enables full object-tracking/full-graph support.
        /// </summary>
        public bool AsReference
        {
            get { return HasFlag(OPTIONS_AsReference); }
            set { SetFlag(OPTIONS_AsReference, value, true); }
        }

        /// <summary>
        /// Embeds the type information into the stream, allowing usage with types not known in advance.
        /// </summary>
        public bool DynamicType
        {
            get { return HasFlag(OPTIONS_DynamicType); }
            set { SetFlag(OPTIONS_DynamicType, value, true); }
        }

        /// <summary>
        /// Indicates that the member should be treated as a protobuf Map
        /// </summary>
        public bool IsMap
        {
            get { return HasFlag(OPTIONS_IsMap); }
            set { SetFlag(OPTIONS_IsMap, value, true); }
        }

        private DataFormat mapKeyFormat, mapValueFormat;
        /// <summary>
        /// Specifies the data-format that should be used for the key, when IsMap is enabled
        /// </summary>
        public DataFormat MapKeyFormat
        {
            get { return mapKeyFormat; }
            set
            {
                if(mapKeyFormat != value)
                {
                    ThrowIfFrozen();
                    mapKeyFormat = value;
                }
            }
        }
        /// <summary>
        /// Specifies the data-format that should be used for the value, when IsMap is enabled
        /// </summary>
        public DataFormat MapValueFormat
        {
            get { return mapValueFormat; }
            set
            {
                if (mapValueFormat != value)
                {
                    ThrowIfFrozen();
                    mapValueFormat = value;
                }
            }
        }

		private string mapKeyTypeName;
		public string MapKeyTypeName{
			get{ 
				return mapKeyTypeName;
			}
			set{
				mapKeyTypeName = value;
			}
		}

		private string mapValueTypeName;
		public string MapValueTypeName{
			get{ 
				return mapValueTypeName;
			}
			set{
				mapValueTypeName = value;
			}
		}

        private MethodInfo getSpecified, setSpecified;
        /// <summary>
        /// Specifies methods for working with optional data members.
        /// </summary>
        /// <param name="getSpecified">Provides a method (null for none) to query whether this member should
        /// be serialized; it must be of the form "bool {Method}()". The member is only serialized if the
        /// method returns true.</param>
        /// <param name="setSpecified">Provides a method (null for none) to indicate that a member was
        /// deserialized; it must be of the form "void {Method}(bool)", and will be called with "true"
        /// when data is found.</param>
        public void SetSpecified(MethodInfo getSpecified, MethodInfo setSpecified)
        {
            if (this.getSpecified != getSpecified || this.setSpecified != setSpecified)
            {
                if (getSpecified != null)
                {
                    if (getSpecified.ReturnType != model.MapType(typeof(bool))
                        || getSpecified.IsStatic
                        || getSpecified.GetParameters().Length != 0)
                    {
                        throw new ArgumentException("Invalid pattern for checking member-specified", "getSpecified");
                    }
                }
                if (setSpecified != null)
                {
                    ParameterInfo[] args;
                    if (setSpecified.ReturnType != model.MapType(typeof(void))
                        || setSpecified.IsStatic
                        || (args = setSpecified.GetParameters()).Length != 1
                        || args[0].ParameterType != model.MapType(typeof(bool)))
                    {
                        throw new ArgumentException("Invalid pattern for setting member-specified", "setSpecified");
                    }
                }

                ThrowIfFrozen();
                this.getSpecified = getSpecified;
                this.setSpecified = setSpecified;
            }
        }
        private void ThrowIfFrozen()
        {
            if (serializer != null) throw new InvalidOperationException("The type cannot be changed once a serializer has been generated");
        }
        internal bool ResolveMapTypes(out Type dictionaryType, out Type keyType, out Type valueType)
        {
            dictionaryType = keyType = valueType = null;

#if WINRT || COREFX
            var info = memberType.GetTypeInfo();
#else
            var info = memberType;
#endif
			MethodInfo b, a, ar, f;
            if(ImmutableCollectionDecorator.IdentifyImmutable(model, MemberType, out b, out a, out ar, out f))
            {
                return false;
            }
            if (info.IsInterface && info.IsGenericType && info.GetGenericTypeDefinition() == typeof(IDictionary<,>))
            {
                var typeArgs = memberType.GetGenericArguments();

                if (IsValidMapKeyType(typeArgs[0]))
                {
                    keyType = typeArgs[0];
                    valueType = typeArgs[1];
                    dictionaryType = memberType;
                }
                return false;
            }

            foreach (var iType in memberType.GetInterfaces())
            {
#if WINRT || COREFX
                info = iType.GetTypeInfo();
#else
                info = iType;
#endif
                if (info.IsGenericType && info.GetGenericTypeDefinition() == typeof(IDictionary<,>))
                {
                    if (dictionaryType != null) throw new InvalidOperationException("Multiple dictionary interfaces implemented by type: " + memberType.FullName);
                    var typeArgs = iType.GetGenericArguments();

                    if (IsValidMapKeyType(typeArgs[0]))
                    {
                        keyType = typeArgs[0];
                        valueType = typeArgs[1];
                        dictionaryType = memberType;
                    }
                }
            }
            if (dictionaryType == null) return false;

            // (note we checked the key type already)
            // not a map if value is repeated
            Type itemType = null, defaultType = null;
            model.ResolveListTypes(valueType, ref itemType, ref defaultType);
            if (itemType != null) return false;

            return dictionaryType != null;
        }

        static bool IsValidMapKeyType(Type type)
        {
            if (type == null) return false;
            switch(Helpers.GetTypeCode(type))
            {
                case ProtoTypeCode.Boolean:
                case ProtoTypeCode.Byte:
                case ProtoTypeCode.Char:
                case ProtoTypeCode.Int16:
                case ProtoTypeCode.Int32:
                case ProtoTypeCode.Int64:
                case ProtoTypeCode.String:

                case ProtoTypeCode.SByte:
                case ProtoTypeCode.UInt16:
                case ProtoTypeCode.UInt32:
                case ProtoTypeCode.UInt64:
                    return true;
            }
            return false;
        }
        private IProtoSerializer BuildSerializer()
        {
            int opaqueToken = 0;
            try
            {
                model.TakeLock(ref opaqueToken);// check nobody is still adding this type
                var member = backingMember ?? originalMember;
                IProtoSerializer ser;
                if (IsMap)
                {
					Type dictionaryType,keyType,valueType;
                    ResolveMapTypes(out dictionaryType, out keyType, out valueType);

					var keyType2 = keyType;
					var valueType2 = valueType;
					if (keyType2 != null && keyType2.FullName == "ILRuntime.Runtime.Intepreter.ILTypeInstance") {
						keyType2 = PType.FindType (mapKeyTypeName);
					}
					if (valueType2 != null && valueType2.FullName == "ILRuntime.Runtime.Intepreter.ILTypeInstance") {
						valueType2 = PType.FindType (mapValueTypeName);
					}
					if(dictionaryType != null){
						dictionaryType = typeof(System.Collections.Generic.Dictionary<,>).MakeGenericType(new Type[]{ keyType, valueType});
					}

                    if (dictionaryType == null)
                    {
                        throw new InvalidOperationException("Unable to resolve map type for type: " + memberType.FullName);
                    }
                    var concreteType = defaultType;
                    if(concreteType == null && Helpers.IsClass(memberType))
                    {
                        concreteType = memberType;
                    }
					WireType keyWireType;
                    var keySer = TryGetCoreSerializer(model, MapKeyFormat, keyType2, out keyWireType, false, false, false, false);
                    if(!AsReference)
                    {
                        AsReference = MetaType.GetAsReferenceDefault(model, valueType);
                    }
					WireType valueWireType;
                    var valueSer = TryGetCoreSerializer(model, MapValueFormat, valueType2, out valueWireType, AsReference, DynamicType, false, true);

                    var ctors = typeof(MapDecorator<,,>).MakeGenericType(new Type[] { dictionaryType, keyType, valueType }).GetConstructors(
                        BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                    if (ctors.Length != 1) throw new InvalidOperationException("Unable to resolve MapDecorator constructor");
                    ser = (IProtoSerializer)ctors[0].Invoke(new object[] {model, concreteType, keySer, valueSer, fieldNumber,
                        DataFormat == DataFormat.Group ? WireType.StartGroup : WireType.String, keyWireType, valueWireType, OverwriteList });
                }
                else
                {
                    WireType wireType;
                    Type finalType = itemType == null ? memberType : itemType;
                    ser = TryGetCoreSerializer(model, dataFormat, finalType, out wireType, AsReference, DynamicType, OverwriteList, true);
                    if (ser == null)
                    {
                        throw new InvalidOperationException("No serializer defined for type: " + finalType.FullName);
                    }

                    // apply tags
                    if (itemType != null && SupportNull)
                    {
                        if (IsPacked)
                        {
                            throw new NotSupportedException("Packed encodings cannot support null values");
                        }
                        ser = new TagDecorator(NullDecorator.Tag, wireType, IsStrict, ser);
                        ser = new NullDecorator(model, ser);
                        ser = new TagDecorator(fieldNumber, WireType.StartGroup, false, ser);
                    }
                    else
                    {
                        ser = new TagDecorator(fieldNumber, wireType, IsStrict, ser);
                    }
                    // apply lists if appropriate
                    if (itemType != null)
                    {
#if NO_GENERICS
                    Type underlyingItemType = itemType;
#else
                        Type underlyingItemType = SupportNull ? itemType : Helpers.GetUnderlyingType(itemType) ?? itemType;
#endif
                        Helpers.DebugAssert(underlyingItemType == ser.ExpectedType
                            || (ser.ExpectedType == model.MapType(typeof(object)) && !Helpers.IsValueType(underlyingItemType))
                            , "Wrong type in the tail; expected {0}, received {1}", ser.ExpectedType, underlyingItemType);
                        if (memberType.IsArray)
                        {
                            ser = new ArrayDecorator(model, ser, fieldNumber, IsPacked, wireType, memberType, OverwriteList, SupportNull);
                        }
                        else
                        {
                            ser = ListDecorator.Create(model, memberType, defaultType, ser, fieldNumber, IsPacked, wireType, member != null && PropertyDecorator.CanWrite(model, member), OverwriteList, SupportNull);
                        }
                    }
                    else if (defaultValue != null && !IsRequired && getSpecified == null)
                    {   // note: "ShouldSerialize*" / "*Specified" / etc ^^^^ take precedence over defaultValue,
                        // as does "IsRequired"
                        ser = new DefaultValueDecorator(model, defaultValue, ser);
                    }
                    if (memberType == model.MapType(typeof(Uri)))
                    {
                        ser = new UriDecorator(model, ser);
                    }
#if PORTABLE
                    else if(memberType.FullName == typeof(Uri).FullName)
                    {
                        // In PCLs, the Uri type may not match (WinRT uses Internal/Uri, .Net uses System/Uri)
                        ser = new ReflectedUriDecorator(memberType, model, ser);
                    }
#endif
                }
                if (member != null)
                {
                    PropertyInfo prop = member as PropertyInfo;
                    if (prop != null)
                    {
                        ser = new PropertyDecorator(model, parentType, (PropertyInfo)member, ser);
                    }
                    else
                    {
                        FieldInfo fld = member as FieldInfo;
                        if (fld != null)
                        {
                            ser = new FieldDecorator(parentType, (FieldInfo)member, ser);
                        }
                        else
                        {
                            throw new InvalidOperationException();
                        }
                    }
                    if (getSpecified != null || setSpecified != null)
                    {
                        ser = new MemberSpecifiedDecorator(getSpecified, setSpecified, ser);
                    }
                }
                return ser;
            }
            finally
            {
                model.ReleaseLock(opaqueToken);
            }
        }

        private static WireType GetIntWireType(DataFormat format, int width) {
            switch(format) {
                case DataFormat.ZigZag: return WireType.SignedVariant;
                case DataFormat.FixedSize: return width == 32 ? WireType.Fixed32 : WireType.Fixed64;
                case DataFormat.TwosComplement:
                case DataFormat.Default: return WireType.Variant;
                default: throw new InvalidOperationException();
            }
        }
        private static WireType GetDateTimeWireType(DataFormat format)
        {
            switch (format)
            {
                
                case DataFormat.Group: return WireType.StartGroup;
                case DataFormat.FixedSize: return WireType.Fixed64;
                case DataFormat.WellKnown:
                case DataFormat.Default:
                    return WireType.String;
                default: throw new InvalidOperationException();
            }
        }

        internal static IProtoSerializer TryGetCoreSerializer(RuntimeTypeModel model, DataFormat dataFormat, Type type, out WireType defaultWireType,
            bool asReference, bool dynamicType, bool overwriteList, bool allowComplexTypes)
        {
#if !NO_GENERICS
            {
                Type tmp = Helpers.GetUnderlyingType(type);
                if (tmp != null) type = tmp;
            }
#endif
            if (Helpers.IsEnum(type))
            {
                if (allowComplexTypes && model != null)
                {
                    // need to do this before checking the typecode; an int enum will report Int32 etc
                    defaultWireType = WireType.Variant;
                    return new EnumSerializer(type, model.GetEnumMap(type));
                }
                else
                { // enum is fine for adding as a meta-type
                    defaultWireType = WireType.None;
                    return null;
                }
            }
            ProtoTypeCode code = Helpers.GetTypeCode(type);
            switch (code)
            {
                case ProtoTypeCode.Int32:
                    defaultWireType = GetIntWireType(dataFormat, 32);
                    return new Int32Serializer(model);
                case ProtoTypeCode.UInt32:
                    defaultWireType = GetIntWireType(dataFormat, 32);
                    return new UInt32Serializer(model);
                case ProtoTypeCode.Int64:
                    defaultWireType = GetIntWireType(dataFormat, 64);
                    return new Int64Serializer(model);
                case ProtoTypeCode.UInt64:
                    defaultWireType = GetIntWireType(dataFormat, 64);
                    return new UInt64Serializer(model);
                case ProtoTypeCode.String:
                    defaultWireType = WireType.String;
                    if (asReference)
                    {
                        return new NetObjectSerializer(model, model.MapType(typeof(string)), 0, BclHelpers.NetObjectOptions.AsReference);
                    }
                    return new StringSerializer(model);
                case ProtoTypeCode.Single:
                    defaultWireType = WireType.Fixed32;
                    return new SingleSerializer(model);
                case ProtoTypeCode.Double:
                    defaultWireType = WireType.Fixed64;
                    return new DoubleSerializer(model);
                case ProtoTypeCode.Boolean:
                    defaultWireType = WireType.Variant;
                    return new BooleanSerializer(model);
                case ProtoTypeCode.DateTime:
                    defaultWireType = GetDateTimeWireType(dataFormat);
                    return new DateTimeSerializer(dataFormat, model);
                case ProtoTypeCode.Decimal:
                    defaultWireType = WireType.String;
                    return new DecimalSerializer(model);
                case ProtoTypeCode.Byte:
                    defaultWireType = GetIntWireType(dataFormat, 32);
                    return new ByteSerializer(model);
                case ProtoTypeCode.SByte:
                    defaultWireType = GetIntWireType(dataFormat, 32);
                    return new SByteSerializer(model);
                case ProtoTypeCode.Char:
                    defaultWireType = WireType.Variant;
                    return new CharSerializer(model);
                case ProtoTypeCode.Int16:
                    defaultWireType = GetIntWireType(dataFormat, 32);
                    return new Int16Serializer(model);
                case ProtoTypeCode.UInt16:
                    defaultWireType = GetIntWireType(dataFormat, 32);
                    return new UInt16Serializer(model);
                case ProtoTypeCode.TimeSpan:
                    defaultWireType = GetDateTimeWireType(dataFormat);
                    return new TimeSpanSerializer(dataFormat, model);
                case ProtoTypeCode.Guid:
                    defaultWireType = dataFormat == DataFormat.Group ? WireType.StartGroup : WireType.String;
                    return new GuidSerializer(model);
                case ProtoTypeCode.Uri:
                    defaultWireType = WireType.String;
                    return new StringSerializer(model);
                case ProtoTypeCode.ByteArray:
                    defaultWireType = WireType.String;
                    return new BlobSerializer(model, overwriteList);
                case ProtoTypeCode.Type:
                    defaultWireType = WireType.String;
                    return new SystemTypeSerializer(model);
            }
            IProtoSerializer parseable = model.AllowParseableTypes ? ParseableSerializer.TryCreate(type, model) : null;
            if (parseable != null)
            {
                defaultWireType = WireType.String;
                return parseable;
            }
            if (allowComplexTypes && model != null)
            {
                int key = model.GetKey(type, false, true);
                MetaType meta = null;
                if (key >= 0)
                {
                    meta = model[type];
                    if(dataFormat == DataFormat.Default && meta.IsGroup)
                    {
                        dataFormat = DataFormat.Group;
                    }
                }

                if (asReference || dynamicType)
                {
                    BclHelpers.NetObjectOptions options = BclHelpers.NetObjectOptions.None;
                    if (asReference) options |= BclHelpers.NetObjectOptions.AsReference;
                    if (dynamicType) options |= BclHelpers.NetObjectOptions.DynamicType;
                    if (meta != null)
                    { // exists
                        if (asReference && Helpers.IsValueType(type))
                        {
                            string message = "AsReference cannot be used with value-types";

                            if (type.Name == "KeyValuePair`2")
                            {
                                message += "; please see http://stackoverflow.com/q/14436606/";
                            }
                            else
                            {
                                message += ": " + type.FullName;
                            }
                            throw new InvalidOperationException(message);
                        }
                        
                        if (asReference && meta.IsAutoTuple) options |= BclHelpers.NetObjectOptions.LateSet;                        
                        if (meta.UseConstructor) options |= BclHelpers.NetObjectOptions.UseConstructor;
                    }
                    defaultWireType = dataFormat == DataFormat.Group ? WireType.StartGroup : WireType.String;
                    return new NetObjectSerializer(model, type, key, options);
                }
                if (key >= 0)
                {
                    defaultWireType = dataFormat == DataFormat.Group ? WireType.StartGroup : WireType.String;
                    return new SubItemSerializer(type, key, meta, true);
                }
            }
            defaultWireType = WireType.None;
            return null;
        }


        private string name;
        internal void SetName(string name)
        {
            if (name != this.name)
            {
                ThrowIfFrozen();
                this.name = name;
            }
        }
        /// <summary>
        /// Gets the logical name for this member in the schema (this is not critical for binary serialization, but may be used
        /// when inferring a schema).
        /// </summary>
        public string Name
        {
            get { return Helpers.IsNullOrEmpty(name) ? originalMember.Name : name; }
            set { SetName(value); }
        }

        private const byte
           OPTIONS_IsStrict = 1,
           OPTIONS_IsPacked = 2,
           OPTIONS_IsRequired = 4,
           OPTIONS_OverwriteList = 8,
           OPTIONS_SupportNull = 16,
           OPTIONS_AsReference = 32,
           OPTIONS_IsMap = 64,
           OPTIONS_DynamicType = 128;

        private byte flags;
        private bool HasFlag(byte flag) { return (flags & flag) == flag; }
        private void SetFlag(byte flag, bool value, bool throwIfFrozen)
        {
            if (throwIfFrozen && HasFlag(flag) != value)
            {
                ThrowIfFrozen();
            }
            if (value)
                flags |= flag;
            else
                flags = (byte)(flags & ~flag);
        }

        /// <summary>
        /// Should lists have extended support for null values? Note this makes the serialization less efficient.
        /// </summary>
        public bool SupportNull
        {
            get { return HasFlag(OPTIONS_SupportNull); }
            set { SetFlag(OPTIONS_SupportNull, value, true);}
        }

        internal string GetSchemaTypeName(bool applyNetObjectProxy, ref RuntimeTypeModel.CommonImports imports)
        {
            Type effectiveType = ItemType;
            if (effectiveType == null) effectiveType = MemberType;
            return model.GetSchemaTypeName(effectiveType, DataFormat, applyNetObjectProxy && AsReference, applyNetObjectProxy && DynamicType, ref imports);
        }

        
        internal sealed class Comparer : System.Collections.IComparer
#if !NO_GENERICS
, System.Collections.Generic.IComparer<ValueMember>
#endif
        {
            public static readonly Comparer Default = new Comparer();
            public int Compare(object x, object y)
            {
                return Compare(x as ValueMember, y as ValueMember);
            }
            public int Compare(ValueMember x, ValueMember y)
            {
                if (ReferenceEquals(x, y)) return 0;
                if (x == null) return -1;
                if (y == null) return 1;

                return x.FieldNumber.CompareTo(y.FieldNumber);
            }
        }
    }
}
#endif
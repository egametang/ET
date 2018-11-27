/* Copyright 2010-2016 MongoDB Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.Linq.Expressions;
using System.Reflection;
#if !ENABLE_IL2CPP
using System.Reflection.Emit;		
#endif
using MongoDB.Bson.Serialization.Serializers;

namespace MongoDB.Bson.Serialization
{
    /// <summary>
    /// Represents the mapping between a field or property and a BSON element.
    /// </summary>
    public class BsonMemberMap
    {
        // private fields
        private readonly BsonClassMap _classMap;
        private readonly MemberInfo _memberInfo;
        private readonly Type _memberType;
        private readonly bool _memberTypeIsBsonValue;

        private string _elementName;
        private bool _frozen; // once a class map has been frozen no further changes are allowed
        private int _order;
        private Func<object, object> _getter;
        private Action<object, object> _setter;
        private volatile IBsonSerializer _serializer;
        private IIdGenerator _idGenerator;
        private bool _isRequired;
        private Func<object, bool> _shouldSerializeMethod;
        private bool _ignoreIfDefault;
        private bool _ignoreIfNull;
        private object _defaultValue;
        private Func<object> _defaultValueCreator;
        private bool _defaultValueSpecified;

        // constructors
        /// <summary>
        /// Initializes a new instance of the BsonMemberMap class.
        /// </summary>
        /// <param name="classMap">The class map this member map belongs to.</param>
        /// <param name="memberInfo">The member info.</param>
        public BsonMemberMap(BsonClassMap classMap, MemberInfo memberInfo)
        {
            _classMap = classMap;
            _memberInfo = memberInfo;
            _memberType = BsonClassMap.GetMemberInfoType(memberInfo);
            _memberTypeIsBsonValue = typeof(BsonValue).GetTypeInfo().IsAssignableFrom(_memberType);

            Reset();
        }

        // public properties
        /// <summary>
        /// Gets the class map that this member map belongs to.
        /// </summary>
        public BsonClassMap ClassMap
        {
            get { return _classMap; }
        }

        /// <summary>
        /// Gets the name of the member.
        /// </summary>
        public string MemberName
        {
            get { return _memberInfo.Name; }
        }

        /// <summary>
        /// Gets the type of the member.
        /// </summary>
        public Type MemberType
        {
            get { return _memberType; }
        }

        /// <summary>
        /// Gets whether the member type is a BsonValue.
        /// </summary>
        public bool MemberTypeIsBsonValue
        {
            get { return _memberTypeIsBsonValue; }
        }

        /// <summary>
        /// Gets the name of the element.
        /// </summary>
        public string ElementName
        {
            get { return _elementName; }
        }

        /// <summary>
        /// Gets the serialization order.
        /// </summary>
        public int Order
        {
            get { return _order; }
        }

        /// <summary>
        /// Gets the member info.
        /// </summary>
        public MemberInfo MemberInfo
        {
            get { return _memberInfo; }
        }

        /// <summary>
        /// Gets the getter function.
        /// </summary>
        public Func<object, object> Getter
        {
            get
            {
                if (_getter == null)
                {
                    _getter = GetGetter();
                }
                return _getter;
            }
        }

        /// <summary>
        /// Gets the setter function.
        /// </summary>
        public Action<object, object> Setter
        {
            get
            {
                if (_setter == null)
                {
                    if (_memberInfo is FieldInfo)
                    {
                        _setter = GetFieldSetter();
                    }
                    else
                    {
                        _setter = GetPropertySetter();
                    }
                }
                return _setter;
            }
        }

        /// <summary>
        /// Gets the Id generator.
        /// </summary>
        public IIdGenerator IdGenerator
        {
            get { return _idGenerator; }
        }

        /// <summary>
        /// Gets whether a default value was specified.
        /// </summary>
        public bool IsDefaultValueSpecified
        {
            get { return _defaultValueSpecified; }
        }

        /// <summary>
        /// Gets whether an element is required for this member when deserialized.
        /// </summary>
        public bool IsRequired
        {
            get { return _isRequired; }
        }

        /// <summary>
        /// Gets the method that will be called to determine whether the member should be serialized.
        /// </summary>
        public Func<object, bool> ShouldSerializeMethod
        {
            get { return _shouldSerializeMethod; }
        }

        /// <summary>
        /// Gets whether default values should be ignored when serialized.
        /// </summary>
        public bool IgnoreIfDefault
        {
            get { return _ignoreIfDefault; }
        }

        /// <summary>
        /// Gets whether null values should be ignored when serialized.
        /// </summary>
        public bool IgnoreIfNull
        {
            get { return _ignoreIfNull; }
        }

        /// <summary>
        /// Gets the default value.
        /// </summary>
        public object DefaultValue
        {
            get { return _defaultValueCreator != null ? _defaultValueCreator() : _defaultValue; }
        }

        /// <summary>
        /// Gets whether the member is readonly.
        /// </summary>
        /// <remarks>
        /// Readonly indicates that the member is written to the database, but not read from the database.
        /// </remarks>
        public bool IsReadOnly
        {
            get
            {
                if (_memberInfo is FieldInfo)
                {
                    var field = (FieldInfo)_memberInfo;
                    return field.IsInitOnly || field.IsLiteral;
                }
                else if (_memberInfo is PropertyInfo)
                {
                    var property = (PropertyInfo)_memberInfo;
                    return !property.CanWrite;
                }
                else
                {
                    throw new NotSupportedException(
                       string.Format("Only fields and properties are supported by BsonMemberMap. The member {0} of class {1} is a {2}.",
                       _memberInfo.Name,
                       _memberInfo.DeclaringType.Name,
                       _memberInfo is FieldInfo ? "field" : "property"));
                }
            }
        }

        // public methods
        /// <summary>
        /// Applies the default value to the member of an object.
        /// </summary>
        /// <param name="obj">The object.</param>
        public void ApplyDefaultValue(object obj)
        {
            if (_defaultValueSpecified)
            {
                this.Setter(obj, DefaultValue);
            }
        }

        /// <summary>
        /// Freezes this instance.
        /// </summary>
        public void Freeze()
        {
            _frozen = true;
        }

        /// <summary>
        /// Gets the serializer.
        /// </summary>
        /// <returns>The serializer.</returns>
        public IBsonSerializer GetSerializer()
        {
            if (_serializer == null)
            {
                // return special serializer for BsonValue members that handles the _csharpnull representation
                if (_memberTypeIsBsonValue)
                {
                    var wrappedSerializer = BsonSerializer.LookupSerializer(_memberType);
                    var isBsonArraySerializer = wrappedSerializer is IBsonArraySerializer;
                    var isBsonDocumentSerializer = wrappedSerializer is IBsonDocumentSerializer;

                    Type csharpNullSerializerDefinition;
                    if (isBsonArraySerializer && isBsonDocumentSerializer)
                    {
                        csharpNullSerializerDefinition = typeof(BsonValueCSharpNullArrayAndDocumentSerializer<>);
                    }
                    else if (isBsonArraySerializer)
                    {
                        csharpNullSerializerDefinition = typeof(BsonValueCSharpNullArraySerializer<>);
                    }
                    else if (isBsonDocumentSerializer)
                    {
                        csharpNullSerializerDefinition = typeof(BsonValueCSharpNullDocumentSerializer<>);
                    }
                    else
                    {
                        csharpNullSerializerDefinition = typeof(BsonValueCSharpNullSerializer<>);
                    }

                    var csharpNullSerializerType = csharpNullSerializerDefinition.MakeGenericType(_memberType);
                    var csharpNullSerializer = (IBsonSerializer)Activator.CreateInstance(csharpNullSerializerType, wrappedSerializer);
                    _serializer = csharpNullSerializer;
                }
                else
                {
                    _serializer = BsonSerializer.LookupSerializer(_memberType);
                }
            }
            return _serializer;
        }

        /// <summary>
        /// Resets the member map back to its initial state.
        /// </summary>
        /// <returns>The member map.</returns>
        public BsonMemberMap Reset()
        {
            if (_frozen) { ThrowFrozenException(); }

            _defaultValue = GetDefaultValue(_memberType);
            _defaultValueCreator = null;
            _defaultValueSpecified = false;
            _elementName = _memberInfo.Name;
            _idGenerator = null;
            _ignoreIfDefault = false;
            _ignoreIfNull = false;
            _isRequired = false;
            _order = int.MaxValue;
            _serializer = null;
            _shouldSerializeMethod = null;

            return this;
        }

        /// <summary>
        /// Sets the default value creator.
        /// </summary>
        /// <param name="defaultValueCreator">The default value creator (note: the supplied delegate must be thread safe).</param>
        /// <returns>The member map.</returns>
        public BsonMemberMap SetDefaultValue(Func<object> defaultValueCreator)
        {
            if (defaultValueCreator == null)
            {
                throw new ArgumentNullException("defaultValueCreator");
            }
            if (_frozen) { ThrowFrozenException(); }
            _defaultValue = defaultValueCreator(); // need an instance to compare against
            _defaultValueCreator = defaultValueCreator;
            _defaultValueSpecified = true;
            return this;
        }

        /// <summary>
        /// Sets the default value.
        /// </summary>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The member map.</returns>
        public BsonMemberMap SetDefaultValue(object defaultValue)
        {
            if (_frozen) { ThrowFrozenException(); }
            _defaultValue = defaultValue;
            _defaultValueCreator = null;
            _defaultValueSpecified = true;
            return this;
        }

        /// <summary>
        /// Sets the name of the element.
        /// </summary>
        /// <param name="elementName">The name of the element.</param>
        /// <returns>The member map.</returns>
        public BsonMemberMap SetElementName(string elementName)
        {
            if (elementName == null)
            {
                throw new ArgumentNullException("elementName");
            }
            if (elementName.IndexOf('\0') != -1)
            {
                throw new ArgumentException("Element names cannot contain nulls.", "elementName");
            }
            if (_frozen) { ThrowFrozenException(); }

            _elementName = elementName;
            return this;
        }

        /// <summary>
        /// Sets the Id generator.
        /// </summary>
        /// <param name="idGenerator">The Id generator.</param>
        /// <returns>The member map.</returns>
        public BsonMemberMap SetIdGenerator(IIdGenerator idGenerator)
        {
            if (_frozen) { ThrowFrozenException(); }
            _idGenerator = idGenerator;
            return this;
        }

        /// <summary>
        /// Sets whether default values should be ignored when serialized.
        /// </summary>
        /// <param name="ignoreIfDefault">Whether default values should be ignored when serialized.</param>
        /// <returns>The member map.</returns>
        public BsonMemberMap SetIgnoreIfDefault(bool ignoreIfDefault)
        {
            if (_frozen) { ThrowFrozenException(); }
            if (ignoreIfDefault && _ignoreIfNull)
            {
                throw new InvalidOperationException("IgnoreIfDefault and IgnoreIfNull are mutually exclusive. Choose one or the other.");
            }

            _ignoreIfDefault = ignoreIfDefault;
            return this;
        }

        /// <summary>
        /// Sets whether null values should be ignored when serialized.
        /// </summary>
        /// <param name="ignoreIfNull">Wether null values should be ignored when serialized.</param>
        /// <returns>The member map.</returns>
        public BsonMemberMap SetIgnoreIfNull(bool ignoreIfNull)
        {
            if (_frozen) { ThrowFrozenException(); }

            if (ignoreIfNull && _ignoreIfDefault)
            {
                throw new InvalidOperationException("IgnoreIfDefault and IgnoreIfNull are mutually exclusive. Choose one or the other.");
            }
            _ignoreIfNull = ignoreIfNull;
            return this;
        }

        /// <summary>
        /// Sets whether an element is required for this member when deserialized
        /// </summary>
        /// <param name="isRequired">Whether an element is required for this member when deserialized</param>
        /// <returns>The member map.</returns>
        public BsonMemberMap SetIsRequired(bool isRequired)
        {
            if (_frozen) { ThrowFrozenException(); }
            _isRequired = isRequired;
            return this;
        }

        /// <summary>
        /// Sets the serialization order.
        /// </summary>
        /// <param name="order">The serialization order.</param>
        /// <returns>The member map.</returns>
        public BsonMemberMap SetOrder(int order)
        {
            if (_frozen) { ThrowFrozenException(); }
            _order = order;
            return this;
        }

        /// <summary>
        /// Sets the serializer.
        /// </summary>
        /// <param name="serializer">The serializer.</param>
        /// <returns>
        /// The member map.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">serializer</exception>
        /// <exception cref="System.ArgumentException">serializer</exception>
        public BsonMemberMap SetSerializer(IBsonSerializer serializer)
        {
            if (serializer == null)
            {
                throw new ArgumentNullException("serializer");
            }
            if (serializer.ValueType != _memberType)
            {
                var message = string.Format("Value type of serializer is {0} and does not match member type {1}.", serializer.ValueType.FullName, _memberType.FullName);
                throw new ArgumentException(message, "serializer");
            }

            if (_frozen) { ThrowFrozenException(); }
            _serializer = serializer;
            return this;
        }

        /// <summary>
        /// Sets the method that will be called to determine whether the member should be serialized.
        /// </summary>
        /// <param name="shouldSerializeMethod">The method.</param>
        /// <returns>The member map.</returns>
        public BsonMemberMap SetShouldSerializeMethod(Func<object, bool> shouldSerializeMethod)
        {
            if (_frozen) { ThrowFrozenException(); }
            _shouldSerializeMethod = shouldSerializeMethod;
            return this;
        }

        /// <summary>
        /// Determines whether a value should be serialized
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="value">The value.</param>
        /// <returns>True if the value should be serialized.</returns>
        public bool ShouldSerialize(object obj, object value)
        {
            if (_ignoreIfNull)
            {
                if (value == null)
                {
                    return false; // don't serialize null
                }
            }

            if (_ignoreIfDefault)
            {
                if (object.Equals(_defaultValue, value))
                {
                    return false; // don't serialize default value
                }
            }

            if (_shouldSerializeMethod != null && !_shouldSerializeMethod(obj))
            {
                // the _shouldSerializeMethod determined that the member shouldn't be serialized
                return false;
            }

            return true;
        }

        // private methods
        private static object GetDefaultValue(Type type)
        {
            var typeInfo = type.GetTypeInfo();
            if (typeInfo.IsEnum)
            {
                return Enum.ToObject(type, 0);
            }

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Empty:
#if NET45
                case TypeCode.DBNull:
#endif
                case TypeCode.String:
                    break;
                case TypeCode.Object:
                    if (typeInfo.IsValueType)
                    {
                        return Activator.CreateInstance(type);
                    }
                    break;
                case TypeCode.Boolean: return false;
                case TypeCode.Char: return '\0';
                case TypeCode.SByte: return (sbyte)0;
                case TypeCode.Byte: return (byte)0;
                case TypeCode.Int16: return (short)0;
                case TypeCode.UInt16: return (ushort)0;
                case TypeCode.Int32: return 0;
                case TypeCode.UInt32: return 0U;
                case TypeCode.Int64: return 0L;
                case TypeCode.UInt64: return 0UL;
                case TypeCode.Single: return 0F;
                case TypeCode.Double: return 0D;
                case TypeCode.Decimal: return 0M;
                case TypeCode.DateTime: return DateTime.MinValue;
            }
            return null;
        }

        private Action<object, object> GetFieldSetter()
        {
            var fieldInfo = (FieldInfo)_memberInfo;

            if (IsReadOnly)
            {
                var message = string.Format(
                    "The field '{0} {1}' of class '{2}' is readonly. To avoid this exception, call IsReadOnly to ensure that setting a value is allowed.",
                    fieldInfo.FieldType.FullName, fieldInfo.Name, fieldInfo.DeclaringType.FullName);
                throw new BsonSerializationException(message);
            }

	        return (obj, value) => { fieldInfo.SetValue(obj, value); };
		}

		private Func<object, object> GetGetter()
        {
#if ENABLE_IL2CPP
            PropertyInfo propertyInfo = _memberInfo as PropertyInfo;
            if (propertyInfo != null)
            {
                MethodInfo getMethodInfo = propertyInfo.GetGetMethod();
                if (getMethodInfo == null)
                {
                    var message = string.Format(
                        "The property '{0} {1}' of class '{2}' has no 'get' accessor.",
                        propertyInfo.PropertyType.FullName, propertyInfo.Name, propertyInfo.DeclaringType.FullName);
                    throw new BsonSerializationException(message);
                }
                return (obj) => { return getMethodInfo.Invoke(obj, null); };
            }

            FieldInfo fieldInfo = _memberInfo as FieldInfo;
            return (obj) => { return fieldInfo.GetValue(obj); };
#else
			var propertyInfo = _memberInfo as PropertyInfo;
            if (propertyInfo != null)
            {
                var getMethodInfo = propertyInfo.GetMethod;
                if (getMethodInfo == null)
                {
                    var message = string.Format(
                        "The property '{0} {1}' of class '{2}' has no 'get' accessor.",
                        propertyInfo.PropertyType.FullName, propertyInfo.Name, propertyInfo.DeclaringType.FullName);
                    throw new BsonSerializationException(message);
                }
            }

            // lambdaExpression = (obj) => (object) ((TClass) obj).Member
            var objParameter = Expression.Parameter(typeof(object), "obj");
            var lambdaExpression = Expression.Lambda<Func<object, object>>(
                Expression.Convert(
                    Expression.MakeMemberAccess(
                        Expression.Convert(objParameter, _memberInfo.DeclaringType),
                        _memberInfo
                    ),
                    typeof(object)
                ),
                objParameter
            );

            return lambdaExpression.Compile();
#endif
        }


        private Action<object, object> GetPropertySetter()
        {
#if ENABLE_IL2CPP
            var propertyInfo = (PropertyInfo) _memberInfo;

            return (obj, value) => { propertyInfo.SetValue(obj, value); };
#else
			var propertyInfo = (PropertyInfo)_memberInfo;
            var setMethodInfo = propertyInfo.SetMethod;
            if (IsReadOnly)
            {
                var message = string.Format(
                    "The property '{0} {1}' of class '{2}' has no 'set' accessor. To avoid this exception, call IsReadOnly to ensure that setting a value is allowed.",
                    propertyInfo.PropertyType.FullName, propertyInfo.Name, propertyInfo.DeclaringType.FullName);
                throw new BsonSerializationException(message);
            }

            // lambdaExpression = (obj, value) => ((TClass) obj).SetMethod((TMember) value)
            var objParameter = Expression.Parameter(typeof(object), "obj");
            var valueParameter = Expression.Parameter(typeof(object), "value");
            var lambdaExpression = Expression.Lambda<Action<object, object>>(
                Expression.Call(
                    Expression.Convert(objParameter, _memberInfo.DeclaringType),
                    setMethodInfo,
                    Expression.Convert(valueParameter, _memberType)
                ),
                objParameter,
                valueParameter
            );

            return lambdaExpression.Compile();
#endif
        }

        private void ThrowFrozenException()
        {
            var message = string.Format("Member map for {0}.{1} has been frozen and no further changes are allowed.", _classMap.ClassType.FullName, _memberInfo.Name);
            throw new InvalidOperationException(message);
        }
    }
}

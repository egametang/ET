/* Copyright 2010-2014 MongoDB Inc.
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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Net;
using MongoDB.Bson.Serialization.Serializers;

namespace MongoDB.Bson.Serialization
{
    /// <summary>
    /// Represents the default serialization provider.
    /// </summary>
    internal class BsonDefaultSerializationProvider : IBsonSerializationProvider
    {
        // private static fields
        private static Dictionary<Type, Type> __serializers;
        private static Dictionary<Type, Type> __genericSerializerDefinitions;

        // static constructor
        static BsonDefaultSerializationProvider()
        {
            __serializers = new Dictionary<Type, Type>
            {
                { typeof(BitArray), typeof(BitArraySerializer) },
                { typeof(Boolean), typeof(BooleanSerializer) },
                { typeof(BsonArray), typeof(BsonArraySerializer) },
                { typeof(BsonBinaryData), typeof(BsonBinaryDataSerializer) },
                { typeof(BsonBoolean), typeof(BsonBooleanSerializer) },
                { typeof(BsonDateTime), typeof(BsonDateTimeSerializer) },
                { typeof(BsonDocument), typeof(BsonDocumentSerializer) },
                { typeof(BsonDocumentWrapper), typeof(BsonDocumentWrapperSerializer) },
                { typeof(BsonDouble), typeof(BsonDoubleSerializer) },
                { typeof(BsonInt32), typeof(BsonInt32Serializer) },
                { typeof(BsonInt64), typeof(BsonInt64Serializer) },
                { typeof(BsonJavaScript), typeof(BsonJavaScriptSerializer) },
                { typeof(BsonJavaScriptWithScope), typeof(BsonJavaScriptWithScopeSerializer) },
                { typeof(BsonMaxKey), typeof(BsonMaxKeySerializer) },
                { typeof(BsonMinKey), typeof(BsonMinKeySerializer) },
                { typeof(BsonNull), typeof(BsonNullSerializer) },
                { typeof(BsonObjectId), typeof(BsonObjectIdSerializer) },
                { typeof(BsonRegularExpression), typeof(BsonRegularExpressionSerializer) },
                { typeof(BsonString), typeof(BsonStringSerializer) },
                { typeof(BsonSymbol), typeof(BsonSymbolSerializer) },
                { typeof(BsonTimestamp), typeof(BsonTimestampSerializer) },
                { typeof(BsonUndefined), typeof(BsonUndefinedSerializer) },
                { typeof(BsonValue), typeof(BsonValueSerializer) },
                { typeof(Byte), typeof(ByteSerializer) },
                { typeof(Byte[]), typeof(ByteArraySerializer) },
                { typeof(Char), typeof(CharSerializer) },
                { typeof(CultureInfo), typeof(CultureInfoSerializer) },
                { typeof(DateTime), typeof(DateTimeSerializer) },
                { typeof(DateTimeOffset), typeof(DateTimeOffsetSerializer) },
                { typeof(Decimal), typeof(DecimalSerializer) },
                { typeof(Double), typeof(DoubleSerializer) },
                { typeof(Guid), typeof(GuidSerializer) },
                { typeof(Int16), typeof(Int16Serializer) },
                { typeof(Int32), typeof(Int32Serializer) },
                { typeof(Int64), typeof(Int64Serializer) },
                { typeof(IPAddress), typeof(IPAddressSerializer) },
                { typeof(IPEndPoint), typeof(IPEndPointSerializer) },
                { typeof(Object), typeof(ObjectSerializer) },
                { typeof(ObjectId), typeof(ObjectIdSerializer) },
                { typeof(Queue), typeof(QueueSerializer) },
                { typeof(SByte), typeof(SByteSerializer) },
                { typeof(Single), typeof(SingleSerializer) },
                { typeof(Stack), typeof(StackSerializer) },
                { typeof(String), typeof(StringSerializer) },
                { typeof(TimeSpan), typeof(TimeSpanSerializer) },
                { typeof(UInt16), typeof(UInt16Serializer) },
                { typeof(UInt32), typeof(UInt32Serializer) },
                { typeof(UInt64), typeof(UInt64Serializer) },
                { typeof(Uri), typeof(UriSerializer) },
                { typeof(Version), typeof(VersionSerializer) }
            };

            __genericSerializerDefinitions = new Dictionary<Type, Type>
            {
                { typeof(KeyValuePair<,>), typeof(KeyValuePairSerializer<,>) },
                { typeof(Nullable<>), typeof(NullableSerializer<>) },
                { typeof(Queue<>), typeof(QueueSerializer<>) },
                { typeof(Stack<>), typeof(StackSerializer<>) }
            };
        }

        // constructors
        /// <summary>
        /// Initializes a new instance of the BsonDefaultSerializer class.
        /// </summary>
        public BsonDefaultSerializationProvider()
        {
        }

        // public methods
        /// <summary>
        /// Gets the serializer for a type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The serializer.</returns>
        public IBsonSerializer GetSerializer(Type type)
        {
            Type serializerType;
            if (__serializers.TryGetValue(type, out serializerType))
            {
                return (IBsonSerializer)Activator.CreateInstance(serializerType);
            }

            // use BsonDocumentSerializer for all subclasses of BsonDocument also
            if (typeof(BsonDocument).IsAssignableFrom(type))
            {
                return BsonDocumentSerializer.Instance;
            }

            // use BsonIBsonSerializableSerializer for all classes that implement IBsonSerializable
            if (typeof(IBsonSerializable).IsAssignableFrom(type))
            {
                return BsonIBsonSerializableSerializer.Instance;
            }

            if (type.IsGenericType)
            {
                var genericTypeDefinition = type.GetGenericTypeDefinition();
                Type genericSerializerDefinition;
                if (__genericSerializerDefinitions.TryGetValue(genericTypeDefinition, out genericSerializerDefinition))
                {
                    var genericSerializerType = genericSerializerDefinition.MakeGenericType(type.GetGenericArguments());
                    return (IBsonSerializer)Activator.CreateInstance(genericSerializerType);
                }
            }

            if (type.IsArray)
            {
                var elementType = type.GetElementType();
                switch (type.GetArrayRank())
                {
                    case 1:
                        var arraySerializerDefinition = typeof(ArraySerializer<>);
                        var arraySerializerType = arraySerializerDefinition.MakeGenericType(elementType);
                        return (IBsonSerializer)Activator.CreateInstance(arraySerializerType);
                    case 2:
                        var twoDimensionalArraySerializerDefinition = typeof(TwoDimensionalArraySerializer<>);
                        var twoDimensionalArraySerializerType = twoDimensionalArraySerializerDefinition.MakeGenericType(elementType);
                        return (IBsonSerializer)Activator.CreateInstance(twoDimensionalArraySerializerType);
                    case 3:
                        var threeDimensionalArraySerializerDefinition = typeof(ThreeDimensionalArraySerializer<>);
                        var threeDimensionalArraySerializerType = threeDimensionalArraySerializerDefinition.MakeGenericType(elementType);
                        return (IBsonSerializer)Activator.CreateInstance(threeDimensionalArraySerializerType);
                    default:
                        var message = string.Format("No serializer found for array for rank {0}.", type.GetArrayRank());
                        throw new BsonSerializationException(message);
                }
            }

            if (type.IsEnum)
            {
                return new EnumSerializer();
            }

            // classes that implement IDictionary or IEnumerable are serialized using either DictionarySerializer or EnumerableSerializer
            // this does mean that any additional public properties the class might have won't be serialized (just like the XmlSerializer)
            var collectionSerializer = GetCollectionSerializer(type);
            if (collectionSerializer != null)
            {
                return collectionSerializer;
            }

            // we'll try our best by attempting to find a discriminator hoping it points
            // us to a concrete type with a serializer.
            if (type.IsInterface)
            {
                return InterfaceSerializer.Instance;
            }

            return null;
        }

        // private methods
        private IBsonSerializer GetCollectionSerializer(Type type)
        {
            Type implementedGenericDictionaryInterface = null;
            Type implementedGenericEnumerableInterface = null;
            Type implementedDictionaryInterface = null;
            Type implementedEnumerableInterface = null;

            var implementedInterfaces = new List<Type>(type.GetInterfaces());
            if (type.IsInterface)
            {
                implementedInterfaces.Add(type);
            }
            foreach (var implementedInterface in implementedInterfaces)
            {
                if (implementedInterface.IsGenericType)
                {
                    var genericInterfaceDefinition = implementedInterface.GetGenericTypeDefinition();
                    if (genericInterfaceDefinition == typeof(IDictionary<,>))
                    {
                        implementedGenericDictionaryInterface = implementedInterface;
                    }
                    if (genericInterfaceDefinition == typeof(IEnumerable<>))
                    {
                        implementedGenericEnumerableInterface = implementedInterface;
                    }
                }
                else
                {
                    if (implementedInterface == typeof(IDictionary))
                    {
                        implementedDictionaryInterface = implementedInterface;
                    }
                    if (implementedInterface == typeof(IEnumerable))
                    {
                        implementedEnumerableInterface = implementedInterface;
                    }
                }
            }

            // the order of the tests is important
            if (implementedGenericDictionaryInterface != null)
            {
                var keyType = implementedGenericDictionaryInterface.GetGenericArguments()[0];
                var valueType = implementedGenericDictionaryInterface.GetGenericArguments()[1];
                var genericSerializerDefinition = typeof(DictionarySerializer<,>);
                var genericSerializerType = genericSerializerDefinition.MakeGenericType(keyType, valueType);
                return (IBsonSerializer)Activator.CreateInstance(genericSerializerType);
            }
            else if (implementedDictionaryInterface != null)
            {
                return new DictionarySerializer();
            }
            else if (implementedGenericEnumerableInterface != null)
            {
                var valueType = implementedGenericEnumerableInterface.GetGenericArguments()[0];
                var readOnlyCollectionType = typeof(ReadOnlyCollection<>).MakeGenericType(valueType);
                Type genericSerializerDefinition;
                if (readOnlyCollectionType.IsAssignableFrom(type))
                {
                    genericSerializerDefinition = typeof(ReadOnlyCollectionSerializer<>);
                    if (type != readOnlyCollectionType)
                    {
                        BsonSerializer.RegisterDiscriminator(type, type.Name);
                    }
                }
                else
                {
                    genericSerializerDefinition = typeof(EnumerableSerializer<>);
                }
                var genericSerializerType = genericSerializerDefinition.MakeGenericType(valueType);
                return (IBsonSerializer)Activator.CreateInstance(genericSerializerType);
            }
            else if (implementedEnumerableInterface != null)
            {
                return new EnumerableSerializer();
            }

            return null;
        }
    }
}

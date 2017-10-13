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
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Reflection;
using MongoDB.Bson.Serialization.Serializers;

namespace MongoDB.Bson.Serialization
{
    /// <summary>
    /// Provides serializers for primitive types.
    /// </summary>
    public class PrimitiveSerializationProvider : BsonSerializationProviderBase
    {
        private static readonly Dictionary<Type, Type> __serializersTypes;

        static PrimitiveSerializationProvider()
        {
            __serializersTypes = new Dictionary<Type, Type>
            {
                { typeof(Boolean), typeof(BooleanSerializer) },
                { typeof(Byte), typeof(ByteSerializer) },
                { typeof(Byte[]), typeof(ByteArraySerializer) },
                { typeof(Char), typeof(CharSerializer) },
                { typeof(CultureInfo), typeof(CultureInfoSerializer) },
                { typeof(DateTime), typeof(DateTimeSerializer) },
                { typeof(DateTimeOffset), typeof(DateTimeOffsetSerializer) },
                { typeof(Decimal), typeof(DecimalSerializer) },
                { typeof(Decimal128), typeof(Decimal128Serializer) },
                { typeof(Double), typeof(DoubleSerializer) },
                { typeof(Guid), typeof(GuidSerializer) },
                { typeof(Int16), typeof(Int16Serializer) },
                { typeof(Int32), typeof(Int32Serializer) },
                { typeof(Int64), typeof(Int64Serializer) },
                { typeof(IPAddress), typeof(IPAddressSerializer) },
                { typeof(IPEndPoint), typeof(IPEndPointSerializer) },
                { typeof(KeyValuePair<,>), typeof(KeyValuePairSerializer<,>) },
                { typeof(Nullable<>), typeof(NullableSerializer<>) },
                { typeof(Object), typeof(ObjectSerializer) },
                { typeof(ObjectId), typeof(ObjectIdSerializer) },
                { typeof(SByte), typeof(SByteSerializer) },
                { typeof(Single), typeof(SingleSerializer) },
                { typeof(String), typeof(StringSerializer) },
                { typeof(TimeSpan), typeof(TimeSpanSerializer) },
                { typeof(Tuple<>), typeof(TupleSerializer<>) },
                { typeof(Tuple<,>), typeof(TupleSerializer<,>) },
                { typeof(Tuple<,,>), typeof(TupleSerializer<,,>) },
                { typeof(Tuple<,,,>), typeof(TupleSerializer<,,,>) },
                { typeof(Tuple<,,,,>), typeof(TupleSerializer<,,,,>) },
                { typeof(Tuple<,,,,,>), typeof(TupleSerializer<,,,,,>) },
                { typeof(Tuple<,,,,,,>), typeof(TupleSerializer<,,,,,,>) },
                { typeof(Tuple<,,,,,,,>), typeof(TupleSerializer<,,,,,,,>) },
                { typeof(UInt16), typeof(UInt16Serializer) },
                { typeof(UInt32), typeof(UInt32Serializer) },
                { typeof(UInt64), typeof(UInt64Serializer) },
                { typeof(Uri), typeof(UriSerializer) },
                { typeof(Version), typeof(VersionSerializer) }
            };
        }

        /// <inheritdoc/>
        public override IBsonSerializer GetSerializer(Type type, IBsonSerializerRegistry serializerRegistry)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            var typeInfo = type.GetTypeInfo();
            if (typeInfo.IsGenericType && typeInfo.ContainsGenericParameters)
            {
                var message = string.Format("Generic type {0} has unassigned type parameters.", BsonUtils.GetFriendlyTypeName(type));
                throw new ArgumentException(message, "type");
            }

            Type serializerType;
            if (__serializersTypes.TryGetValue(type, out serializerType))
            {
                return CreateSerializer(serializerType, serializerRegistry);
            }

            if (typeInfo.IsGenericType && !typeInfo.ContainsGenericParameters)
            {
                Type serializerTypeDefinition;
                if (__serializersTypes.TryGetValue(type.GetGenericTypeDefinition(), out serializerTypeDefinition))
                {
                    return CreateGenericSerializer(serializerTypeDefinition, type.GetTypeInfo().GetGenericArguments(), serializerRegistry);
                }
            }

            if (typeInfo.IsEnum)
            {
                return CreateGenericSerializer(typeof(EnumSerializer<>), new[] { type }, serializerRegistry);
            }

            return null;
        }
    }
}
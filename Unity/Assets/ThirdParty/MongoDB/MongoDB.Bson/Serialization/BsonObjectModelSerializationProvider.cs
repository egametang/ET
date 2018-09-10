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
using System.Reflection;
using MongoDB.Bson.Serialization.Serializers;

namespace MongoDB.Bson.Serialization
{
    /// <summary>
    /// Provides serializers for BsonValue and its derivations.
    /// </summary>
    public class BsonObjectModelSerializationProvider : BsonSerializationProviderBase
    {
        private static readonly Dictionary<Type, IBsonSerializer> __serializers;

        static BsonObjectModelSerializationProvider()
        {
            __serializers = new Dictionary<Type, IBsonSerializer>
            {
                { typeof(BsonArray), BsonArraySerializer.Instance },
                { typeof(BsonBinaryData), BsonBinaryDataSerializer.Instance },
                { typeof(BsonBoolean), BsonBooleanSerializer.Instance },
                { typeof(BsonDateTime), BsonDateTimeSerializer.Instance },
                { typeof(BsonDecimal128), BsonDecimal128Serializer.Instance },
                { typeof(BsonDocument), BsonDocumentSerializer.Instance },
                { typeof(BsonDocumentWrapper), BsonDocumentWrapperSerializer.Instance },
                { typeof(BsonDouble), BsonDoubleSerializer.Instance },
                { typeof(BsonInt32), BsonInt32Serializer.Instance },
                { typeof(BsonInt64), BsonInt64Serializer.Instance },
                { typeof(BsonJavaScript), BsonJavaScriptSerializer.Instance },
                { typeof(BsonJavaScriptWithScope), BsonJavaScriptWithScopeSerializer.Instance },
                { typeof(BsonMaxKey), BsonMaxKeySerializer.Instance },
                { typeof(BsonMinKey), BsonMinKeySerializer.Instance },
                { typeof(BsonNull), BsonNullSerializer.Instance },
                { typeof(BsonObjectId), BsonObjectIdSerializer.Instance },
                { typeof(BsonRegularExpression), BsonRegularExpressionSerializer.Instance },
                { typeof(BsonString), BsonStringSerializer.Instance },
                { typeof(BsonSymbol), BsonSymbolSerializer.Instance },
                { typeof(BsonTimestamp), BsonTimestampSerializer.Instance },
                { typeof(BsonUndefined), BsonUndefinedSerializer.Instance },
                { typeof(BsonValue), BsonValueSerializer.Instance }
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

            IBsonSerializer serializer;
            if (__serializers.TryGetValue(type, out serializer))
            {
                return serializer;
            }

            return null;
        }
    }
}
/* Copyright 2015-present MongoDB Inc.
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
using System.Reflection;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Support;

namespace MongoDB.Driver.Linq.Expressions
{
    internal interface ISerializationExpression
    {
        IBsonSerializer Serializer { get; }

        Type Type { get; }
    }

    internal static class ISerializationExpressionExtensions
    {
        public static string AppendFieldName(this ISerializationExpression node, string suffix)
        {
            var field = node as IFieldExpression;
            return CombineFieldNames(field == null ? null : field.FieldName, suffix);
        }

        public static string PrependFieldName(this ISerializationExpression node, string prefix)
        {
            var field = node as IFieldExpression;
            return CombineFieldNames(prefix, field == null ? null : field.FieldName);
        }

        public static BsonValue SerializeValue(this ISerializationExpression field, Type valueType, object value)
        {
            Ensure.IsNotNull(field, nameof(field));

            var valueSerializer = FieldValueSerializerHelper.GetSerializerForValueType(field.Serializer, BsonSerializer.SerializerRegistry, valueType, value);

            var tempDocument = new BsonDocument();
            using (var bsonWriter = new BsonDocumentWriter(tempDocument))
            {
                var context = BsonSerializationContext.CreateRoot(bsonWriter);
                bsonWriter.WriteStartDocument();
                bsonWriter.WriteName("value");
                valueSerializer.Serialize(context, value);
                bsonWriter.WriteEndDocument();
                return tempDocument[0];
            }
        }

        public static BsonArray SerializeValues(this ISerializationExpression field, Type itemType, IEnumerable values)
        {
            Ensure.IsNotNull(field, nameof(field));
            Ensure.IsNotNull(itemType, nameof(itemType));
            Ensure.IsNotNull(values, nameof(values));

            var itemSerializer = FieldValueSerializerHelper.GetSerializerForValueType(field.Serializer, BsonSerializer.SerializerRegistry, itemType);

            var tempDocument = new BsonDocument();
            using (var bsonWriter = new BsonDocumentWriter(tempDocument))
            {
                var context = BsonSerializationContext.CreateRoot(bsonWriter);
                bsonWriter.WriteStartDocument();
                bsonWriter.WriteName("values");
                bsonWriter.WriteStartArray();
                foreach (var value in values)
                {
                    itemSerializer.Serialize(context, value);
                }
                bsonWriter.WriteEndArray();
                bsonWriter.WriteEndDocument();

                return (BsonArray)tempDocument[0];
            }
        }

        private static string CombineFieldNames(string prefix, string suffix)
        {
            if (prefix == null)
            {
                return suffix;
            }
            if (suffix == null)
            {
                return prefix;
            }

            return prefix + "." + suffix;
        }
    }
}
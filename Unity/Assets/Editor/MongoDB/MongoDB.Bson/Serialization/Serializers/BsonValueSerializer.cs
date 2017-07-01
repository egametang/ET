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
using MongoDB.Bson.IO;

namespace MongoDB.Bson.Serialization.Serializers
{
    /// <summary>
    /// Represents a serializer for BsonValues.
    /// </summary>
    public class BsonValueSerializer : BsonBaseSerializer, IBsonDocumentSerializer, IBsonArraySerializer
    {
        // private static fields
        private static BsonValueSerializer __instance = new BsonValueSerializer();

        // constructors
        /// <summary>
        /// Initializes a new instance of the BsonValueSerializer class.
        /// </summary>
        public BsonValueSerializer()
        {
        }

        // public static properties
        /// <summary>
        /// Gets an instance of the BsonValueSerializer class.
        /// </summary>
        public static BsonValueSerializer Instance
        {
            get { return __instance; }
        }

        // public methods
        /// <summary>
        /// Deserializes an object from a BsonReader.
        /// </summary>
        /// <param name="bsonReader">The BsonReader.</param>
        /// <param name="nominalType">The nominal type of the object.</param>
        /// <param name="actualType">The actual type of the object.</param>
        /// <param name="options">The serialization options.</param>
        /// <returns>An object.</returns>
        public override object Deserialize(BsonReader bsonReader, Type nominalType, Type actualType, // ignored
            IBsonSerializationOptions options)
        {
            var bsonType = bsonReader.GetCurrentBsonType();
            switch (bsonType)
            {
                case BsonType.Array: return (BsonValue)BsonArraySerializer.Instance.Deserialize(bsonReader, typeof(BsonArray), options);
                case BsonType.Binary: return (BsonValue)BsonBinaryDataSerializer.Instance.Deserialize(bsonReader, typeof(BsonBinaryData), options);
                case BsonType.Boolean: return (BsonValue)BsonBooleanSerializer.Instance.Deserialize(bsonReader, typeof(BsonBoolean), options);
                case BsonType.DateTime: return (BsonValue)BsonDateTimeSerializer.Instance.Deserialize(bsonReader, typeof(BsonDateTime), options);
                case BsonType.Document: return (BsonValue)BsonDocumentSerializer.Instance.Deserialize(bsonReader, typeof(BsonDocument), options);
                case BsonType.Double: return (BsonValue)BsonDoubleSerializer.Instance.Deserialize(bsonReader, typeof(BsonDouble), options);
                case BsonType.Int32: return (BsonValue)BsonInt32Serializer.Instance.Deserialize(bsonReader, typeof(BsonInt32), options);
                case BsonType.Int64: return (BsonValue)BsonInt64Serializer.Instance.Deserialize(bsonReader, typeof(BsonInt64), options);
                case BsonType.JavaScript: return (BsonValue)BsonJavaScriptSerializer.Instance.Deserialize(bsonReader, typeof(BsonJavaScript), options);
                case BsonType.JavaScriptWithScope: return (BsonValue)BsonJavaScriptWithScopeSerializer.Instance.Deserialize(bsonReader, typeof(BsonJavaScriptWithScope), options);
                case BsonType.MaxKey: return (BsonValue)BsonMaxKeySerializer.Instance.Deserialize(bsonReader, typeof(BsonMaxKey), options);
                case BsonType.MinKey: return (BsonValue)BsonMinKeySerializer.Instance.Deserialize(bsonReader, typeof(BsonMinKey), options);
                case BsonType.Null: return (BsonValue)BsonNullSerializer.Instance.Deserialize(bsonReader, typeof(BsonNull), options);
                case BsonType.ObjectId: return (BsonValue)BsonObjectIdSerializer.Instance.Deserialize(bsonReader, typeof(BsonObjectId), options);
                case BsonType.RegularExpression: return (BsonValue)BsonRegularExpressionSerializer.Instance.Deserialize(bsonReader, typeof(BsonRegularExpression), options);
                case BsonType.String: return (BsonValue)BsonStringSerializer.Instance.Deserialize(bsonReader, typeof(BsonString), options);
                case BsonType.Symbol: return (BsonValue)BsonSymbolSerializer.Instance.Deserialize(bsonReader, typeof(BsonSymbol), options);
                case BsonType.Timestamp: return (BsonValue)BsonTimestampSerializer.Instance.Deserialize(bsonReader, typeof(BsonTimestamp), options);
                case BsonType.Undefined: return (BsonValue)BsonUndefinedSerializer.Instance.Deserialize(bsonReader, typeof(BsonUndefined), options);
                default:
                    var message = string.Format("Invalid BsonType {0}.", bsonType);
                    throw new BsonInternalException(message);
            }
        }

        /// <summary>
        /// Gets the serialization info for a member.
        /// </summary>
        /// <param name="memberName">The member name.</param>
        /// <returns>
        /// The serialization info for the member.
        /// </returns>
        public BsonSerializationInfo GetMemberSerializationInfo(string memberName)
        {
            return new BsonSerializationInfo(
                memberName,
                BsonValueSerializer.Instance,
                typeof(BsonValue),
                BsonValueSerializer.Instance.GetDefaultSerializationOptions());
        }

        /// <summary>
        /// Gets the serialization info for individual items of the array.
        /// </summary>
        /// <returns>
        /// The serialization info for the items.
        /// </returns>
        public BsonSerializationInfo GetItemSerializationInfo()
        {
            return new BsonSerializationInfo(
                null,
                BsonValueSerializer.Instance,
                typeof(BsonValue),
                BsonValueSerializer.Instance.GetDefaultSerializationOptions());
        }

        /// <summary>
        /// Serializes an object to a BsonWriter.
        /// </summary>
        /// <param name="bsonWriter">The BsonWriter.</param>
        /// <param name="nominalType">The nominal type.</param>
        /// <param name="value">The object.</param>
        /// <param name="options">The serialization options.</param>
        public override void Serialize(
            BsonWriter bsonWriter,
            Type nominalType,
            object value,
            IBsonSerializationOptions options)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            var bsonValue = (BsonValue)value;
            switch (bsonValue.BsonType)
            {
                case BsonType.Array: BsonArraySerializer.Instance.Serialize(bsonWriter, typeof(BsonArray), bsonValue, options); break;
                case BsonType.Binary: BsonBinaryDataSerializer.Instance.Serialize(bsonWriter, typeof(BsonBinaryData), bsonValue, options); break;
                case BsonType.Boolean: BsonBooleanSerializer.Instance.Serialize(bsonWriter, typeof(BsonBoolean), bsonValue, options); break;
                case BsonType.DateTime: BsonDateTimeSerializer.Instance.Serialize(bsonWriter, typeof(BsonDateTime), bsonValue, options); break;
                case BsonType.Document: BsonDocumentSerializer.Instance.Serialize(bsonWriter, typeof(BsonDocument), bsonValue, options); break;
                case BsonType.Double: BsonDoubleSerializer.Instance.Serialize(bsonWriter, typeof(BsonDouble), bsonValue, options); break;
                case BsonType.Int32: BsonInt32Serializer.Instance.Serialize(bsonWriter, typeof(BsonInt32), bsonValue, options); break;
                case BsonType.Int64: BsonInt64Serializer.Instance.Serialize(bsonWriter, typeof(BsonInt64), bsonValue, options); break;
                case BsonType.JavaScript: BsonJavaScriptSerializer.Instance.Serialize(bsonWriter, typeof(BsonJavaScript), bsonValue, options); break;
                case BsonType.JavaScriptWithScope: BsonJavaScriptWithScopeSerializer.Instance.Serialize(bsonWriter, typeof(BsonJavaScriptWithScope), bsonValue, options); break;
                case BsonType.MaxKey: BsonMaxKeySerializer.Instance.Serialize(bsonWriter, typeof(BsonMaxKey), bsonValue, options); break;
                case BsonType.MinKey: BsonMinKeySerializer.Instance.Serialize(bsonWriter, typeof(BsonMinKey), bsonValue, options); break;
                case BsonType.Null: BsonNullSerializer.Instance.Serialize(bsonWriter, typeof(BsonNull), bsonValue, options); break;
                case BsonType.ObjectId: BsonObjectIdSerializer.Instance.Serialize(bsonWriter, typeof(BsonObjectId), bsonValue, options); break;
                case BsonType.RegularExpression: BsonRegularExpressionSerializer.Instance.Serialize(bsonWriter, typeof(BsonRegularExpression), bsonValue, options); break;
                case BsonType.String: BsonStringSerializer.Instance.Serialize(bsonWriter, typeof(BsonString), bsonValue, options); break;
                case BsonType.Symbol: BsonSymbolSerializer.Instance.Serialize(bsonWriter, typeof(BsonSymbol), bsonValue, options); break;
                case BsonType.Timestamp: BsonTimestampSerializer.Instance.Serialize(bsonWriter, typeof(BsonTimestamp), bsonValue, options); break;
                case BsonType.Undefined: BsonUndefinedSerializer.Instance.Serialize(bsonWriter, typeof(BsonUndefined), bsonValue, options); break;
                default: throw new BsonInternalException("Invalid BsonType.");
            }
        }
    }
}

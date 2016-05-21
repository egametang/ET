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
using System.IO;
using System.Xml;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization.Options;

namespace MongoDB.Bson.Serialization.Serializers
{
    /// <summary>
    /// Represents a serializer for Booleans.
    /// </summary>
    public class BooleanSerializer : BsonBaseSerializer
    {
        // private static fields
        private static BooleanSerializer __instance = new BooleanSerializer();

        // constructors
        /// <summary>
        /// Initializes a new instance of the BooleanSerializer class.
        /// </summary>
        public BooleanSerializer()
            : base(new RepresentationSerializationOptions(BsonType.Boolean))
        {
        }

        // public static properties
        /// <summary>
        /// Gets an instance of the BooleanSerializer class.
        /// </summary>
        [Obsolete("Use constructor instead.")]
        public static BooleanSerializer Instance
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
        public override object Deserialize(
            BsonReader bsonReader,
            Type nominalType,
            Type actualType,
            IBsonSerializationOptions options)
        {
            VerifyTypes(nominalType, actualType, typeof(bool));

            var bsonType = bsonReader.GetCurrentBsonType();
            switch (bsonType)
            {
                case BsonType.Boolean:
                    return bsonReader.ReadBoolean();
                case BsonType.Double:
                    return bsonReader.ReadDouble() != 0.0;
                case BsonType.Int32:
                    return bsonReader.ReadInt32() != 0;
                case BsonType.Int64:
                    return bsonReader.ReadInt64() != 0;
                case BsonType.Null:
                    bsonReader.ReadNull();
                    return false;
                case BsonType.String:
                    return XmlConvert.ToBoolean(bsonReader.ReadString().ToLower());
                default:
                    var message = string.Format("Cannot deserialize Boolean from BsonType {0}.", bsonType);
                    throw new Exception(message);
            }
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
            var boolValue = (bool)value;
            var representationSerializationOptions = EnsureSerializationOptions<RepresentationSerializationOptions>(options);

            switch (representationSerializationOptions.Representation)
            {
                case BsonType.Boolean:
                    bsonWriter.WriteBoolean(boolValue);
                    break;
                case BsonType.Double:
                    bsonWriter.WriteDouble(boolValue ? 1.0 : 0.0);
                    break;
                case BsonType.Int32:
                    bsonWriter.WriteInt32(boolValue ? 1 : 0);
                    break;
                case BsonType.Int64:
                    bsonWriter.WriteInt64(boolValue ? 1 : 0);
                    break;
                case BsonType.String:
                    bsonWriter.WriteString(XmlConvert.ToString(boolValue));
                    break;
                default:
                    var message = string.Format("'{0}' is not a valid Boolean representation.", representationSerializationOptions.Representation);
                    throw new BsonSerializationException(message);
            }
        }
    }
}

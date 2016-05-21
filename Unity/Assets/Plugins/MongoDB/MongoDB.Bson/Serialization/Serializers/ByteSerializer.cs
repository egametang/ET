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
using System.Globalization;
using System.IO;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization.Options;

namespace MongoDB.Bson.Serialization.Serializers
{
    /// <summary>
    /// Represents a serializer for Bytes.
    /// </summary>
    public class ByteSerializer : BsonBaseSerializer
    {
        // private static fields
        private static ByteSerializer __instance = new ByteSerializer();

        // constructors
        /// <summary>
        /// Initializes a new instance of the ByteSerializer class.
        /// </summary>
        public ByteSerializer()
            : base(new RepresentationSerializationOptions(BsonType.Int32))
        {
        }

        // public static properties
        /// <summary>
        /// Gets an instance of the ByteSerializer class.
        /// </summary>
        [Obsolete("Use constructor instead.")]
        public static ByteSerializer Instance
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
            VerifyTypes(nominalType, actualType, typeof(byte));

            var bsonType = bsonReader.GetCurrentBsonType();
            byte value;
            var lostData = false;
            switch (bsonType)
            {
                case BsonType.Binary:
                    var bytes = bsonReader.ReadBytes();
                    if (bytes.Length != 1)
                    {
                        throw new Exception("Binary data for Byte must be exactly one byte long.");
                    }
                    value = bytes[0];
                    break;
                case BsonType.Int32:
                    var int32Value = bsonReader.ReadInt32();
                    value = (byte)int32Value;
                    lostData = (int)value != int32Value;
                    break;
                case BsonType.Int64:
                    var int64Value = bsonReader.ReadInt64();
                    value = (byte)int64Value;
                    lostData = (int)value != int64Value;
                    break;
                case BsonType.String:
                    var s = bsonReader.ReadString();
                    if (s.Length == 1)
                    {
                        s = "0" + s;
                    }
                    value = byte.Parse(s, NumberStyles.HexNumber);
                    break;
                default:
                    var message = string.Format("Cannot deserialize Byte from BsonType {0}.", bsonType);
                    throw new Exception(message);
            }
            if (lostData)
            {
                var message = string.Format("Data loss occurred when trying to convert from {0} to Byte.", bsonType);
                throw new Exception(message);
            }

            return value;
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
            var byteValue = (byte)value;
            var representationSerializationOptions = EnsureSerializationOptions<RepresentationSerializationOptions>(options);

            switch (representationSerializationOptions.Representation)
            {
                case BsonType.Binary:
                    bsonWriter.WriteBytes(new byte[] { byteValue });
                    break;
                case BsonType.Int32:
                    bsonWriter.WriteInt32(byteValue);
                    break;
                case BsonType.Int64:
                    bsonWriter.WriteInt64(byteValue);
                    break;
                case BsonType.String:
                    bsonWriter.WriteString(string.Format("{0:x2}", byteValue));
                    break;
                default:
                    var message = string.Format("'{0}' is not a valid Byte representation.", representationSerializationOptions.Representation);
                    throw new BsonSerializationException(message);
            }
        }
    }
}

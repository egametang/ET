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
    /// Represents a serializer for SBytes.
    /// </summary>
    public class SByteSerializer : BsonBaseSerializer
    {
        // private static fields
        private static SByteSerializer __instance = new SByteSerializer();

        // constructors
        /// <summary>
        /// Initializes a new instance of the SByteSerializer class.
        /// </summary>
        public SByteSerializer()
            : base(new RepresentationSerializationOptions(BsonType.Int32))
        {
        }

        // public static properties
        /// <summary>
        /// Gets an instance of the SByteSerializer class.
        /// </summary>
        [Obsolete("Use constructor instead.")]
        public static SByteSerializer Instance
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
            VerifyTypes(nominalType, actualType, typeof(sbyte));

            var bsonType = bsonReader.GetCurrentBsonType();
            var lostData = false;
            sbyte value;
            switch (bsonType)
            {
                case BsonType.Binary:
                    var bytes = bsonReader.ReadBytes();
                    if (bytes.Length != 1)
                    {
                        throw new Exception("Binary data for SByte must be exactly one byte long.");
                    }
                    value = (sbyte)bytes[0];
                    break;
                case BsonType.Int32:
                    var int32Value = bsonReader.ReadInt32();
                    value = (sbyte)int32Value;
                    lostData = (int)value != int32Value;
                    break;
                case BsonType.Int64:
                    var int64Value = bsonReader.ReadInt64();
                    value = (sbyte)int64Value;
                    lostData = (int)value != int64Value;
                    break;
                case BsonType.String:
                    var s = bsonReader.ReadString();
                    if (s.Length == 1)
                    {
                        s = "0" + s;
                    }
                    value = (sbyte)byte.Parse(s, NumberStyles.HexNumber);
                    break;
                default:
                    var message = string.Format("Cannot deserialize SByte from BsonType {0}.", bsonType);
                    throw new Exception(message);
            }
            if (lostData)
            {
                var message = string.Format("Data loss occurred when trying to convert from {0} to SByte.", bsonType);
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
            var sbyteValue = (sbyte)value;
            var representationSerializationOptions = EnsureSerializationOptions<RepresentationSerializationOptions>(options);

            switch (representationSerializationOptions.Representation)
            {
                case BsonType.Binary:
                    bsonWriter.WriteBytes(new byte[] { (byte)sbyteValue });
                    break;
                case BsonType.Int32:
                    bsonWriter.WriteInt32(sbyteValue);
                    break;
                case BsonType.Int64:
                    bsonWriter.WriteInt64(sbyteValue);
                    break;
                case BsonType.String:
                    bsonWriter.WriteString(string.Format("{0:x2}", (byte)sbyteValue));
                    break;
                default:
                    var message = string.Format("'{0}' is not a valid SByte representation.", representationSerializationOptions.Representation);
                    throw new BsonSerializationException(message);
            }
        }
    }
}

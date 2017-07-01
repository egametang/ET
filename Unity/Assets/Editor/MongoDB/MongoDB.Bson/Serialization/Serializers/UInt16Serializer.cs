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
    /// Represents a serializer for UInt16s.
    /// </summary>
    public class UInt16Serializer : BsonBaseSerializer
    {
        // private static fields
        private static UInt16Serializer __instance = new UInt16Serializer();

        // constructors
        /// <summary>
        /// Initializes a new instance of the UInt16Serializer class.
        /// </summary>
        public UInt16Serializer()
            : base(new RepresentationSerializationOptions(BsonType.Int32))
        {
        }

        // public static properties
        /// <summary>
        /// Gets an instance of the UInt16Serializer class.
        /// </summary>
        [Obsolete("Use constructor instead.")]
        public static UInt16Serializer Instance
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
            VerifyTypes(nominalType, actualType, typeof(ushort));
            var representationSerializationOptions = EnsureSerializationOptions<RepresentationSerializationOptions>(options);

            var bsonType = bsonReader.GetCurrentBsonType();
            switch (bsonType)
            {
                case BsonType.Double:
                    return representationSerializationOptions.ToUInt16(bsonReader.ReadDouble());
                case BsonType.Int32:
                    return representationSerializationOptions.ToUInt16(bsonReader.ReadInt32());
                case BsonType.Int64:
                    return representationSerializationOptions.ToUInt16(bsonReader.ReadInt64());
                case BsonType.String:
                    return XmlConvert.ToUInt16(bsonReader.ReadString());
                default:
                    var message = string.Format("Cannot deserialize uInt16 from BsonType {0}.", bsonType);
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
            var uint16Value = (ushort)value;
            var representationSerializationOptions = EnsureSerializationOptions<RepresentationSerializationOptions>(options);

            switch (representationSerializationOptions.Representation)
            {
                case BsonType.Double:
                    bsonWriter.WriteDouble(representationSerializationOptions.ToDouble(uint16Value));
                    break;
                case BsonType.Int32:
                    bsonWriter.WriteInt32(representationSerializationOptions.ToInt32(uint16Value));
                    break;
                case BsonType.Int64:
                    bsonWriter.WriteInt64(representationSerializationOptions.ToInt64(uint16Value));
                    break;
                case BsonType.String:
                    bsonWriter.WriteString(XmlConvert.ToString(uint16Value));
                    break;
                default:
                    var message = string.Format("'{0}' is not a valid representation for type 'UInt16'", representationSerializationOptions.Representation);
                    throw new BsonSerializationException(message);
            }
        }
    }
}

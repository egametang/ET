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
using MongoDB.Bson.IO;

namespace MongoDB.Bson.Serialization.Serializers
{
    /// <summary>
    /// Represents a serializer for BsonBinaryDatas.
    /// </summary>
    public class BsonBinaryDataSerializer : BsonBaseSerializer
    {
        // private static fields
        private static BsonBinaryDataSerializer __instance = new BsonBinaryDataSerializer();

        // constructors
        /// <summary>
        /// Initializes a new instance of the BsonBinaryDataSerializer class.
        /// </summary>
        public BsonBinaryDataSerializer()
        {
        }

        // public static properties
        /// <summary>
        /// Gets an instance of the BsonBinaryDataSerializer class.
        /// </summary>
        public static BsonBinaryDataSerializer Instance
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
            VerifyTypes(nominalType, actualType, typeof(BsonBinaryData));

            var bsonType = bsonReader.GetCurrentBsonType();
            switch (bsonType)
            {
                case BsonType.Binary:
                    return bsonReader.ReadBinaryData();
                default:
                    var message = string.Format("Cannot deserialize BsonBinaryData from BsonType {0}.", bsonType);
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
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            var binaryData = (BsonBinaryData)value;

            var subType = binaryData.SubType;
            if (subType == BsonBinarySubType.UuidStandard || subType == BsonBinarySubType.UuidLegacy)
            {
                var writerGuidRepresentation = bsonWriter.Settings.GuidRepresentation;
                if (writerGuidRepresentation != GuidRepresentation.Unspecified)
                {
                    var bytes = binaryData.Bytes;
                    var guidRepresentation = binaryData.GuidRepresentation;

                    if (guidRepresentation == GuidRepresentation.Unspecified)
                    {
                        var message = string.Format(
                            "Cannot serialize BsonBinaryData with GuidRepresentation Unspecified to destination with GuidRepresentation {0}.",
                            writerGuidRepresentation);
                        throw new BsonSerializationException(message);
                    }
                    if (guidRepresentation != writerGuidRepresentation)
                    {
                        var guid = GuidConverter.FromBytes(bytes, guidRepresentation);
                        bytes = GuidConverter.ToBytes(guid, writerGuidRepresentation);
                        subType = (writerGuidRepresentation == GuidRepresentation.Standard) ? BsonBinarySubType.UuidStandard : BsonBinarySubType.UuidLegacy;
                        guidRepresentation = writerGuidRepresentation;
                        binaryData = new BsonBinaryData(bytes, subType, guidRepresentation);
                    }
                }
            }

            bsonWriter.WriteBinaryData(binaryData);
        }
    }
}

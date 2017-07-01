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
using MongoDB.Bson.Serialization.Options;

namespace MongoDB.Bson.Serialization.Serializers
{
    /// <summary>
    /// Represents a serializer for Guids.
    /// </summary>
    public class GuidSerializer : BsonBaseSerializer
    {
        // private static fields
        private static GuidSerializer __instance = new GuidSerializer();

        // constructors
        /// <summary>
        /// Initializes a new instance of the GuidSerializer class.
        /// </summary>
        public GuidSerializer()
            : base(new RepresentationSerializationOptions(BsonType.Binary))
        {
        }

        // public static properties
        /// <summary>
        /// Gets an instance of the GuidSerializer class.
        /// </summary>
        [Obsolete("Use constructor instead.")]
        public static GuidSerializer Instance
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
            VerifyTypes(nominalType, actualType, typeof(Guid));

            var bsonType = bsonReader.GetCurrentBsonType();
            string message;
            switch (bsonType)
            {
                case BsonType.Binary:
                    var binaryData = bsonReader.ReadBinaryData();
                    var bytes = binaryData.Bytes;
                    var subType = binaryData.SubType;
                    var guidRepresentation = binaryData.GuidRepresentation;
                    if (bytes.Length != 16)
                    {
                        message = string.Format("Expected length to be 16, not {0}.", bytes.Length);
                        throw new Exception(message);
                    }
                    if (subType != BsonBinarySubType.UuidStandard && subType != BsonBinarySubType.UuidLegacy)
                    {
                        message = string.Format("Expected binary sub type to be UuidStandard or UuidLegacy, not {0}.", subType);
                        throw new Exception(message);
                    }
                    if (guidRepresentation == GuidRepresentation.Unspecified)
                    {
                        throw new BsonSerializationException("GuidSerializer cannot deserialize a Guid when GuidRepresentation is Unspecified.");
                    }
                    return GuidConverter.FromBytes(bytes, guidRepresentation);
                case BsonType.String:
                    return new Guid(bsonReader.ReadString());
                default:
                    message = string.Format("Cannot deserialize Guid from BsonType {0}.", bsonType);
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
            var guid = (Guid)value;
            var representationSerializationOptions = EnsureSerializationOptions<RepresentationSerializationOptions>(options);

            switch (representationSerializationOptions.Representation)
            {
                case BsonType.Binary:
                    var writerGuidRepresentation = bsonWriter.Settings.GuidRepresentation;
                    if (writerGuidRepresentation == GuidRepresentation.Unspecified)
                    {
                        throw new BsonSerializationException("GuidSerializer cannot serialize a Guid when GuidRepresentation is Unspecified.");
                    }
                    var bytes = GuidConverter.ToBytes(guid, writerGuidRepresentation);
                    var subType = (writerGuidRepresentation == GuidRepresentation.Standard) ? BsonBinarySubType.UuidStandard : BsonBinarySubType.UuidLegacy;
                    bsonWriter.WriteBinaryData(new BsonBinaryData(bytes, subType, writerGuidRepresentation));
                    break;
                case BsonType.String:
                    bsonWriter.WriteString(guid.ToString());
                    break;
                default:
                    var message = string.Format("'{0}' is not a valid Guid representation.", representationSerializationOptions.Representation);
                    throw new BsonSerializationException(message);
            }
        }
    }
}

/* Copyright 2010-2015 MongoDB Inc.
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
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;

namespace MongoDB.Bson.Serialization.Serializers
{
    /// <summary>
    /// Represents a serializer for Guids.
    /// </summary>
    public class GuidSerializer : StructSerializerBase<Guid>, IRepresentationConfigurable<GuidSerializer>
    {
        // private fields
        private readonly BsonType _representation;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="GuidSerializer"/> class.
        /// </summary>
        public GuidSerializer()
            : this(BsonType.Binary)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GuidSerializer"/> class.
        /// </summary>
        /// <param name="representation">The representation.</param>
        public GuidSerializer(BsonType representation)
        {
            switch (representation)
            {
                case BsonType.Binary:
                case BsonType.String:
                    break;

                default:
                    var message = string.Format("{0} is not a valid representation for a GuidSerializer.", representation);
                    throw new ArgumentException(message);
            }

            _representation = representation;
        }

        // public properties
        /// <summary>
        /// Gets the representation.
        /// </summary>
        /// <value>
        /// The representation.
        /// </value>
        public BsonType Representation
        {
            get { return _representation; }
        }

        // public methods
        /// <summary>
        /// Deserializes a value.
        /// </summary>
        /// <param name="context">The deserialization context.</param>
        /// <param name="args">The deserialization args.</param>
        /// <returns>A deserialized value.</returns>
        public override Guid Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var bsonReader = context.Reader;
            string message;

            var bsonType = bsonReader.GetCurrentBsonType();
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
                        throw new FormatException(message);
                    }
                    if (subType != BsonBinarySubType.UuidStandard && subType != BsonBinarySubType.UuidLegacy)
                    {
                        message = string.Format("Expected binary sub type to be UuidStandard or UuidLegacy, not {0}.", subType);
                        throw new FormatException(message);
                    }
                    if (guidRepresentation == GuidRepresentation.Unspecified)
                    {
                        throw new BsonSerializationException("GuidSerializer cannot deserialize a Guid when GuidRepresentation is Unspecified.");
                    }
                    return GuidConverter.FromBytes(bytes, guidRepresentation);

                case BsonType.String:
                    return new Guid(bsonReader.ReadString());

                default:
                    throw CreateCannotDeserializeFromBsonTypeException(bsonType);
            }
        }

        /// <summary>
        /// Serializes a value.
        /// </summary>
        /// <param name="context">The serialization context.</param>
        /// <param name="args">The serialization args.</param>
        /// <param name="value">The object.</param>
        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Guid value)
        {
            var bsonWriter = context.Writer;

            switch (_representation)
            {
                case BsonType.Binary:
                    var writerGuidRepresentation = bsonWriter.Settings.GuidRepresentation;
                    if (writerGuidRepresentation == GuidRepresentation.Unspecified)
                    {
                        throw new BsonSerializationException("GuidSerializer cannot serialize a Guid when GuidRepresentation is Unspecified.");
                    }
                    var bytes = GuidConverter.ToBytes(value, writerGuidRepresentation);
                    var subType = (writerGuidRepresentation == GuidRepresentation.Standard) ? BsonBinarySubType.UuidStandard : BsonBinarySubType.UuidLegacy;
                    bsonWriter.WriteBinaryData(new BsonBinaryData(bytes, subType, writerGuidRepresentation));
                    break;

                case BsonType.String:
                    bsonWriter.WriteString(value.ToString());
                    break;

                default:
                    var message = string.Format("'{0}' is not a valid Guid representation.", _representation);
                    throw new BsonSerializationException(message);
            }
        }

        /// <summary>
        /// Returns a serializer that has been reconfigured with the specified representation.
        /// </summary>
        /// <param name="representation">The representation.</param>
        /// <returns>The reconfigured serializer.</returns>
        public GuidSerializer WithRepresentation(BsonType representation)
        {
            if (representation == _representation)
            {
                return this;
            }
            else
            {
                return new GuidSerializer(representation);
            }
        }

        // explicit interface implementations
        IBsonSerializer IRepresentationConfigurable.WithRepresentation(BsonType representation)
        {
            return WithRepresentation(representation);
        }
    }
}

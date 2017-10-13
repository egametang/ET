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
using System.Globalization;
using System.IO;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;

namespace MongoDB.Bson.Serialization.Serializers
{
    /// <summary>
    /// Represents a serializer for SBytes.
    /// </summary>
    [CLSCompliant(false)]
    public class SByteSerializer : StructSerializerBase<sbyte>, IRepresentationConfigurable<SByteSerializer>
    {
        // private fields
        private readonly BsonType _representation;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="SByteSerializer"/> class.
        /// </summary>
        public SByteSerializer()
            : this(BsonType.Int32)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SByteSerializer"/> class.
        /// </summary>
        /// <param name="representation">The representation.</param>
        public SByteSerializer(BsonType representation)
        {
            switch (representation)
            {
                case BsonType.Binary:
                case BsonType.Int32:
                case BsonType.Int64:
                case BsonType.String:
                    break;

                default:
                    var message = string.Format("{0} is not a valid representation for an SByteSerializer.", representation);
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
        public override sbyte Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var bsonReader = context.Reader;
            sbyte value;
            var lostData = false;

            var bsonType = bsonReader.GetCurrentBsonType();
            switch (bsonType)
            {
                case BsonType.Binary:
                    var bytes = bsonReader.ReadBytes();
                    if (bytes.Length != 1)
                    {
                        throw new FormatException("Binary data for SByte must be exactly one byte long.");
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
                    throw CreateCannotDeserializeFromBsonTypeException(bsonType);
            }

            if (lostData)
            {
                var message = string.Format("Data loss occurred when trying to convert from {0} to SByte.", bsonType);
                throw new FormatException(message);
            }

            return value;
        }

        /// <summary>
        /// Serializes a value.
        /// </summary>
        /// <param name="context">The serialization context.</param>
        /// <param name="args">The serialization args.</param>
        /// <param name="value">The object.</param>
        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, sbyte value)
        {
            var bsonWriter = context.Writer;

            switch (_representation)
            {
                case BsonType.Binary:
                    bsonWriter.WriteBytes(new byte[] { (byte)value });
                    break;

                case BsonType.Int32:
                    bsonWriter.WriteInt32(value);
                    break;

                case BsonType.Int64:
                    bsonWriter.WriteInt64(value);
                    break;

                case BsonType.String:
                    bsonWriter.WriteString(string.Format("{0:x2}", (byte)value));
                    break;

                default:
                    var message = string.Format("'{0}' is not a valid SByte representation.", _representation);
                    throw new BsonSerializationException(message);
            }
        }

        /// <summary>
        /// Returns a serializer that has been reconfigured with the specified representation.
        /// </summary>
        /// <param name="representation">The representation.</param>
        /// <returns>The reconfigured serializer.</returns>
        public SByteSerializer WithRepresentation(BsonType representation)
        {
            if (representation == _representation)
            {
                return this;
            }
            else
            {
                return new SByteSerializer(representation);
            }
        }

        // explicit interface implementations
        IBsonSerializer IRepresentationConfigurable.WithRepresentation(BsonType representation)
        {
            return WithRepresentation(representation);
        }
    }
}

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
    /// Represents a serializer for Booleans.
    /// </summary>
    public class BooleanSerializer : StructSerializerBase<bool>, IRepresentationConfigurable<BooleanSerializer>
    {
        // private fields
        private readonly BsonType _representation;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="BooleanSerializer"/> class.
        /// </summary>
        public BooleanSerializer()
            : this(BsonType.Boolean)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BooleanSerializer"/> class.
        /// </summary>
        /// <param name="representation">The representation.</param>
        public BooleanSerializer(BsonType representation)
        {
            switch (representation)
            {
                case BsonType.Boolean:
                case BsonType.Decimal128:
                case BsonType.Double:
                case BsonType.Int32:
                case BsonType.Int64:
                case BsonType.String:
                    break;

                default:
                    var message = string.Format("{0} is not a valid representation for a BooleanSerializer.", representation);
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
        public override bool Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var bsonReader = context.Reader;

            var bsonType = bsonReader.GetCurrentBsonType();
            switch (bsonType)
            {
                case BsonType.Boolean:
                    return bsonReader.ReadBoolean();

                case BsonType.Decimal128:
                    return bsonReader.ReadDecimal128() != Decimal128.Zero;

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
                    return JsonConvert.ToBoolean(bsonReader.ReadString().ToLower());

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
        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, bool value)
        {
            var bsonWriter = context.Writer;

            switch (_representation)
            {
                case BsonType.Boolean:
                    bsonWriter.WriteBoolean(value);
                    break;

                case BsonType.Decimal128:
                    bsonWriter.WriteDecimal128(value ? Decimal128.One : Decimal128.Zero);
                    break;

                case BsonType.Double:
                    bsonWriter.WriteDouble(value ? 1.0 : 0.0);
                    break;

                case BsonType.Int32:
                    bsonWriter.WriteInt32(value ? 1 : 0);
                    break;

                case BsonType.Int64:
                    bsonWriter.WriteInt64(value ? 1 : 0);
                    break;

                case BsonType.String:
                    bsonWriter.WriteString(JsonConvert.ToString(value));
                    break;

                default:
                    var message = string.Format("'{0}' is not a valid Boolean representation.", _representation);
                    throw new BsonSerializationException(message);
            }
        }

        /// <summary>
        /// Returns a serializer that has been reconfigured with the specified representation.
        /// </summary>
        /// <param name="representation">The representation.</param>
        /// <returns>The reconfigured serializer.</returns>
        public BooleanSerializer WithRepresentation(BsonType representation)
        {
            if (representation == _representation)
            {
                return this;
            }
            else
            {
                return new BooleanSerializer(representation);
            }
        }

        // explicit interface implementations
        IBsonSerializer IRepresentationConfigurable.WithRepresentation(BsonType representation)
        {
            return WithRepresentation(representation);
        }
    }
}

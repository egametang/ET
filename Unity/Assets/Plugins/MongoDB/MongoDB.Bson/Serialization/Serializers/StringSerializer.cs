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

namespace MongoDB.Bson.Serialization.Serializers
{
    /// <summary>
    /// Represents a serializer for Strings.
    /// </summary>
    public class StringSerializer : SealedClassSerializerBase<string>, IRepresentationConfigurable<StringSerializer>
    {
        // private fields
        private readonly BsonType _representation;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="StringSerializer"/> class.
        /// </summary>
        public StringSerializer()
            : this(BsonType.String)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StringSerializer"/> class.
        /// </summary>
        /// <param name="representation">The representation.</param>
        public StringSerializer(BsonType representation)
        {
            switch (representation)
            {
                case BsonType.ObjectId:
                case BsonType.String:
                case BsonType.Symbol:
                    break;

                default:
                    var message = string.Format("{0} is not a valid representation for a StringSerializer.", representation);
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
        protected override string DeserializeValue(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var bsonReader = context.Reader;

            var bsonType = bsonReader.GetCurrentBsonType();
            switch (bsonType)
            {
                case BsonType.ObjectId:
                    if (_representation == BsonType.ObjectId)
                    {
                        return bsonReader.ReadObjectId().ToString();
                    }
                    else
                    {
                        goto default;
                    }

                case BsonType.String:
                    return bsonReader.ReadString();

                case BsonType.Symbol:
                    return bsonReader.ReadSymbol();

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
        protected override void SerializeValue(BsonSerializationContext context, BsonSerializationArgs args, string value)
        {
            var bsonWriter = context.Writer;

            switch (_representation)
            {
                case BsonType.ObjectId:
                    bsonWriter.WriteObjectId(ObjectId.Parse(value));
                    break;

                case BsonType.String:
                    bsonWriter.WriteString(value);
                    break;

                case BsonType.Symbol:
                    bsonWriter.WriteSymbol(value);
                    break;

                default:
                    var message = string.Format("'{0}' is not a valid String representation.", _representation);
                    throw new BsonSerializationException(message);
            }
        }

        /// <summary>
        /// Returns a serializer that has been reconfigured with the specified representation.
        /// </summary>
        /// <param name="representation">The representation.</param>
        /// <returns>The reconfigured serializer.</returns>
        public StringSerializer WithRepresentation(BsonType representation)
        {
            if (representation == _representation)
            {
                return this;
            }
            else
            {
                return new StringSerializer(representation);
            }
        }

        // explicit interface implementations
        IBsonSerializer IRepresentationConfigurable.WithRepresentation(BsonType representation)
        {
            return WithRepresentation(representation);
        }
    }
}

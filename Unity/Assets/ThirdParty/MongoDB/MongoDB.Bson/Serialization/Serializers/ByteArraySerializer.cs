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
using System.Text;

namespace MongoDB.Bson.Serialization.Serializers
{
    /// <summary>
    /// Represents a serializer for ByteArrays.
    /// </summary>
    public class ByteArraySerializer : SealedClassSerializerBase<byte[]>, IRepresentationConfigurable<ByteArraySerializer>
    {
        // private fields
        private readonly BsonType _representation;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ByteArraySerializer"/> class.
        /// </summary>
        public ByteArraySerializer()
            : this(BsonType.Binary)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ByteArraySerializer"/> class.
        /// </summary>
        /// <param name="representation">The representation.</param>
        public ByteArraySerializer(BsonType representation)
        {
            switch (representation)
            {
                case BsonType.Binary:
                case BsonType.String:
                    break;

                default:
                    var message = string.Format("{0} is not a valid representation for a ByteArraySerializer.", representation);
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
#pragma warning disable 618 // about obsolete BsonBinarySubType.OldBinary
        /// <summary>
        /// Deserializes a value.
        /// </summary>
        /// <param name="context">The deserialization context.</param>
        /// <param name="args">The deserialization args.</param>
        /// <returns>A deserialized value.</returns>
        protected override byte[] DeserializeValue(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var bsonReader = context.Reader;

            BsonType bsonType = bsonReader.GetCurrentBsonType();
            switch (bsonType)
            {
                case BsonType.Binary:
                    return bsonReader.ReadBytes();

                case BsonType.String:
                    var s = bsonReader.ReadString();
                    if ((s.Length % 2) != 0)
                    {
                        s = "0" + s; // prepend a zero to make length even
                    }
                    var bytes = new byte[s.Length / 2];
                    for (int i = 0; i < s.Length; i += 2)
                    {
                        var hex = s.Substring(i, 2);
                        var b = byte.Parse(hex, NumberStyles.HexNumber);
                        bytes[i / 2] = b;
                    }
                    return bytes;

                default:
                    throw CreateCannotDeserializeFromBsonTypeException(bsonType);
            }
        }
#pragma warning restore 618

        /// <summary>
        /// Serializes a value.
        /// </summary>
        /// <param name="context">The serialization context.</param>
        /// <param name="args">The serialization args.</param>
        /// <param name="value">The object.</param>
        protected override void SerializeValue(BsonSerializationContext context, BsonSerializationArgs args, byte[] value)
        {
            var bsonWriter = context.Writer;

            switch (_representation)
            {
                case BsonType.Binary:
                    bsonWriter.WriteBytes(value);
                    break;

                case BsonType.String:
                    var sb = new StringBuilder(value.Length * 2);
                    for (int i = 0; i < value.Length; i++)
                    {
                        sb.Append(string.Format("{0:x2}", value[i]));
                    }
                    bsonWriter.WriteString(sb.ToString());
                    break;

                default:
                    var message = string.Format("'{0}' is not a valid Byte[] representation.", _representation);
                    throw new BsonSerializationException(message);
            }
        }

        /// <summary>
        /// Returns a serializer that has been reconfigured with the specified representation.
        /// </summary>
        /// <param name="representation">The representation.</param>
        /// <returns>The reconfigured serializer.</returns>
        public ByteArraySerializer WithRepresentation(BsonType representation)
        {
            if (representation == _representation)
            {
                return this;
            }
            else
            {
                return new ByteArraySerializer(representation);
            }
        }

        // explicit interface implementations
        IBsonSerializer IRepresentationConfigurable.WithRepresentation(BsonType representation)
        {
            return WithRepresentation(representation);
        }
    }
}

/* Copyright 2015-2016 MongoDB Inc.
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
using System.Reflection;
using MongoDB.Bson.IO;

namespace MongoDB.Bson.Serialization.Serializers
{
    /// <summary>
    /// Represents a serializer for a BsonDocument with some parts raw.
    /// </summary>
    public class PartiallyRawBsonDocumentSerializer : SerializerBase<BsonDocument>
    {
        // private fields
        private readonly string _name;
        private readonly IBsonSerializer _rawSerializer;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="PartiallyRawBsonDocumentSerializer"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="rawSerializer">The raw serializer.</param>
        public PartiallyRawBsonDocumentSerializer(string name, IBsonSerializer rawSerializer)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (rawSerializer == null)
            {
                throw new ArgumentNullException("rawSerializer");
            }
            if (!typeof(BsonValue).GetTypeInfo().IsAssignableFrom(rawSerializer.ValueType))
            {
                throw new ArgumentException("RawSerializer ValueType must be a BsonValue.", "rawSerializer");
            }

            _name = name;
            _rawSerializer = rawSerializer;
        }

        // public methods
        /// <inheritdoc/>
        public override BsonDocument Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var document = new BsonDocument();

            var reader = context.Reader;
            reader.ReadStartDocument();
            while (reader.ReadBsonType() != 0)
            {
                var name = reader.ReadName();
                var serializer = ChooseSerializer(name);
                var value = (BsonValue)serializer.Deserialize(context);
                document[name] = value;
            }
            reader.ReadEndDocument();

            return document;
        }

        private IBsonSerializer ChooseSerializer(string name)
        {
            if (name == _name)
            {
                return _rawSerializer;
            }
            else
            {
                return BsonValueSerializer.Instance;
            }
        }
    }
}

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
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoDB.Bson.Serialization.Serializers
{
    /// <summary>
    /// Represents a serializer for nullable values.
    /// </summary>
    /// <typeparam name="T">The underlying type.</typeparam>
    public class NullableSerializer<T> :
        SerializerBase<Nullable<T>>,
        IChildSerializerConfigurable
            where T : struct
    {
        // private fields
        private Lazy<IBsonSerializer<T>> _lazySerializer;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="NullableSerializer{T}"/> class.
        /// </summary>
        public NullableSerializer()
            : this(BsonSerializer.SerializerRegistry)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NullableSerializer{T}"/> class.
        /// </summary>
        /// <param name="serializer">The serializer.</param>
        public NullableSerializer(IBsonSerializer<T> serializer)
        {
            if (serializer == null)
            {
                throw new ArgumentNullException("serializer");
            }

            _lazySerializer = new Lazy<IBsonSerializer<T>>(() => serializer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NullableSerializer{T}" /> class.
        /// </summary>
        /// <param name="serializerRegistry">The serializer registry.</param>
        public NullableSerializer(IBsonSerializerRegistry serializerRegistry)
        {
            if (serializerRegistry == null)
            {
                throw new ArgumentNullException("serializerRegistry");
            }

            _lazySerializer = new Lazy<IBsonSerializer<T>>(() => serializerRegistry.GetSerializer<T>());
        }

        // public methods
        /// <summary>
        /// Deserializes a value.
        /// </summary>
        /// <param name="context">The deserialization context.</param>
        /// <param name="args">The deserialization args.</param>
        /// <returns>A deserialized value.</returns>
        public override T? Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var bsonReader = context.Reader;

            var bsonType = bsonReader.GetCurrentBsonType();
            if (bsonType == BsonType.Null)
            {
                bsonReader.ReadNull();
                return null;
            }
            else
            {
                return _lazySerializer.Value.Deserialize(context);
            }
        }

        /// <summary>
        /// Serializes a value.
        /// </summary>
        /// <param name="context">The serialization context.</param>
        /// <param name="args">The serialization args.</param>
        /// <param name="value">The object.</param>
        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, T? value)
        {
            var bsonWriter = context.Writer;

            if (value == null)
            {
                bsonWriter.WriteNull();
            }
            else
            {
                _lazySerializer.Value.Serialize(context, value.Value);
            }
        }

        /// <summary>
        /// Returns a serializer that has been reconfigured with the specified serializer.
        /// </summary>
        /// <param name="serializer">The serializer.</param>
        /// <returns>
        /// The reconfigured serializer.
        /// </returns>
        public NullableSerializer<T> WithSerializer(IBsonSerializer<T> serializer)
        {
            if (serializer == _lazySerializer.Value)
            {
                return this;
            }
            else
            {
                return new NullableSerializer<T>(serializer);
            }
        }

        // explicit interface implementations
        IBsonSerializer IChildSerializerConfigurable.ChildSerializer
        {
            get { return _lazySerializer.Value; }
        }

        IBsonSerializer IChildSerializerConfigurable.WithChildSerializer(IBsonSerializer childSerializer)
        {
            return WithSerializer((IBsonSerializer<T>)childSerializer);
        }
    }
}

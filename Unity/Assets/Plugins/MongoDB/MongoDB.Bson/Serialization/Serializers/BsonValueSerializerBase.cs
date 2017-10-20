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


namespace MongoDB.Bson.Serialization.Serializers
{
    /// <summary>
    /// Represents a base class for BsonValue serializers.
    /// </summary>
    /// <typeparam name="TBsonValue">The type of the BsonValue.</typeparam>
    public abstract class BsonValueSerializerBase<TBsonValue> : SerializerBase<TBsonValue> where TBsonValue : BsonValue
    {
        // private fields
        private readonly BsonType? _bsonType;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="BsonValueSerializerBase{TBsonValue}"/> class.
        /// </summary>
        /// <param name="bsonType">The Bson type.</param>
        protected BsonValueSerializerBase(BsonType? bsonType)
        {
            _bsonType = bsonType;
        }

        // public methods
        /// <summary>
        /// Deserializes a value.
        /// </summary>
        /// <param name="context">The deserialization context.</param>
        /// <param name="args">The deserialization args.</param>
        /// <returns>A deserialized value.</returns>
        public override TBsonValue Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            if (_bsonType.HasValue)
            {
                EnsureBsonTypeEquals(context.Reader, _bsonType.Value);
            }
            return DeserializeValue(context, args);
        }

        /// <summary>
        /// Serializes a value.
        /// </summary>
        /// <param name="context">The serialization context.</param>
        /// <param name="args">The serialization args.</param>
        /// <param name="value">The object.</param>
        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, TBsonValue value)
        {
            if (value == null)
            {
                var message = string.Format(
                    "C# null values of type '{0}' cannot be serialized using a serializer of type '{1}'.",
                    BsonUtils.GetFriendlyTypeName(ValueType),
                    BsonUtils.GetFriendlyTypeName(GetType()));
                throw new BsonSerializationException(message);
            }

            var actualType = value.GetType();
            if (actualType != ValueType && !args.SerializeAsNominalType)
            {
                var serializer = BsonSerializer.LookupSerializer(actualType);
                serializer.Serialize(context, value);
                return;
            }

            SerializeValue(context, args, value);
        }

        // protected methods
        /// <summary>
        /// Deserializes a value.
        /// </summary>
        /// <param name="context">The deserialization context.</param>
        /// <param name="args">The deserialization args.</param>
        /// <returns>A deserialized value.</returns>
        protected abstract TBsonValue DeserializeValue(BsonDeserializationContext context, BsonDeserializationArgs args);

        /// <summary>
        /// Serializes a value.
        /// </summary>
        /// <param name="context">The serialization context.</param>
        /// <param name="args">The serialization args.</param>
        /// <param name="value">The object.</param>
        protected abstract void SerializeValue(BsonSerializationContext context, BsonSerializationArgs args, TBsonValue value);
    }
}

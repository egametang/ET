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
    /// Represents an abstract base class for sealed class serializers.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    public abstract class SealedClassSerializerBase<TValue> : SerializerBase<TValue> where TValue : class
    {
        // public methods
        /// <summary>
        /// Deserializes a value.
        /// </summary>
        /// <param name="context">The deserialization context.</param>
        /// <param name="args">The deserialization args.</param>
        /// <returns>A deserialized value.</returns>
        public override TValue Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var bsonReader = context.Reader;
            if (bsonReader.GetCurrentBsonType() == BsonType.Null)
            {
                bsonReader.ReadNull();
                return null;
            }
            else
            {
                return DeserializeValue(context, args);
            }
        }

        /// <summary>
        /// Serializes a value.
        /// </summary>
        /// <param name="context">The serialization context.</param>
        /// <param name="args">The serialization args.</param>
        /// <param name="value">The value.</param>
        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, TValue value)
        {
            if (value == null)
            {
                var bsonWriter = context.Writer;
                bsonWriter.WriteNull();
            }
            else
            {
                SerializeValue(context, args, value);
            }
        }

        // protected methods
        /// <summary>
        /// Deserializes a class.
        /// </summary>
        /// <param name="context">The deserialization context.</param>
        /// <param name="args">The deserialization args.</param>
        /// <returns>A deserialized value.</returns>
        protected virtual TValue DeserializeValue(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            throw CreateCannotBeDeserializedException();
        }

        /// <summary>
        /// Serializes a value of type {TValue}.
        /// </summary>
        /// <param name="context">The serialization context.</param>
        /// <param name="args">The serialization args.</param>
        /// <param name="value">The value.</param>
        protected virtual void SerializeValue(BsonSerializationContext context, BsonSerializationArgs args, TValue value)
        {
            throw CreateCannotBeSerializedException();
        }
    }
}

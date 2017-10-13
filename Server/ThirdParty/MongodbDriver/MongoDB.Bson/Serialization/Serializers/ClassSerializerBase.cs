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
    /// Represents an abstract base class for class serializers.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    public abstract class ClassSerializerBase<TValue> : SerializerBase<TValue> where TValue : class
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
                var actualType = GetActualType(context);
                if (actualType == typeof(TValue))
                {
                    return DeserializeValue(context, args);
                }
                else
                {
                    var serializer = BsonSerializer.LookupSerializer(actualType);
                    return (TValue)serializer.Deserialize(context, args);
                }
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
                var actualType = value.GetType();
                if (actualType == typeof(TValue) || args.SerializeAsNominalType)
                {
                    SerializeValue(context, args, value);
                }
                else
                {
                    var serializer = BsonSerializer.LookupSerializer(actualType);
                    serializer.Serialize(context, value);
                }
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
        /// Gets the actual type.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>The actual type.</returns>
        protected virtual Type GetActualType(BsonDeserializationContext context)
        {
            var discriminatorConvention = BsonSerializer.LookupDiscriminatorConvention(typeof(TValue));
            return discriminatorConvention.GetActualType(context.Reader, typeof(TValue));
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

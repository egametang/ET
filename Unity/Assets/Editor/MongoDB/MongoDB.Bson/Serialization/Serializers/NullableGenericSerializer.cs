/* Copyright 2010-2014 MongoDB Inc.
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

namespace MongoDB.Bson.Serialization.Serializers
{
    /// <summary>
    /// Represents a serializer for nullable values.
    /// </summary>
    /// <typeparam name="T">The underlying type.</typeparam>
    public class NullableSerializer<T> : BsonBaseSerializer where T : struct
    {
        // private fields
        private IBsonSerializer _serializer;

        // constructors
        /// <summary>
        /// Initializes a new instance of the NullableSerializer class.
        /// </summary>
        public NullableSerializer()
        {
            _serializer = BsonSerializer.LookupSerializer(typeof(T));
        }

        // public methods
        /// <summary>
        /// Deserializes an object from a BsonReader.
        /// </summary>
        /// <param name="bsonReader">The BsonReader.</param>
        /// <param name="nominalType">The nominal type of the object.</param>
        /// <param name="actualType">The actual type of the object.</param>
        /// <param name="options">The serialization options.</param>
        /// <returns>An object.</returns>
        public override object Deserialize(
           BsonReader bsonReader,
           Type nominalType,
           Type actualType,
           IBsonSerializationOptions options)
        {
            VerifyTypes(nominalType, actualType, typeof(Nullable<T>));

            var bsonType = bsonReader.GetCurrentBsonType();
            if (bsonType == BsonType.Null)
            {
                bsonReader.ReadNull();
                return null;
            }
            else
            {
                return _serializer.Deserialize(bsonReader, typeof(T), options);
            }
        }

        /// <summary>
        /// Gets the default serialization options for this serializer.
        /// </summary>
        /// <returns>The default serialization options for this serializer.</returns>
        public override IBsonSerializationOptions GetDefaultSerializationOptions()
        {
            return _serializer.GetDefaultSerializationOptions();
        }

        /// <summary>
        /// Serializes an object to a BsonWriter.
        /// </summary>
        /// <param name="bsonWriter">The BsonWriter.</param>
        /// <param name="nominalType">The nominal type.</param>
        /// <param name="value">The object.</param>
        /// <param name="options">The serialization options.</param>
        public override void Serialize(
           BsonWriter bsonWriter,
           Type nominalType,
           object value,
           IBsonSerializationOptions options)
        {
            if (value == null)
            {
                bsonWriter.WriteNull();
            }
            else
            {
                _serializer.Serialize(bsonWriter, typeof(T), value, options);
            }
        }
    }
}

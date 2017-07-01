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
using System.IO;
using MongoDB.Bson.IO;

namespace MongoDB.Bson.Serialization.Serializers
{
    /// <summary>
    /// Represents a serializer for BsonArrays.
    /// </summary>
    public class BsonArraySerializer : BsonBaseSerializer, IBsonArraySerializer
    {
        // private static fields
        private static BsonArraySerializer __instance = new BsonArraySerializer();

        // constructors
        /// <summary>
        /// Initializes a new instance of the BsonArraySerializer class.
        /// </summary>
        public BsonArraySerializer()
        {
        }

        // public static properties
        /// <summary>
        /// Gets an instance of the BsonArraySerializer class.
        /// </summary>
        public static BsonArraySerializer Instance
        {
            get { return __instance; }
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
            VerifyTypes(nominalType, actualType, typeof(BsonArray));

            var bsonType = bsonReader.GetCurrentBsonType();
            switch (bsonType)
            {
                case BsonType.Array:
                    bsonReader.ReadStartArray();
                    var array = new BsonArray();
                    while (bsonReader.ReadBsonType() != BsonType.EndOfDocument)
                    {
                        var value = (BsonValue)BsonValueSerializer.Instance.Deserialize(bsonReader, typeof(BsonValue), null);
                        array.Add(value);
                    }
                    bsonReader.ReadEndArray();
                    return array;
                default:
                    var message = string.Format("Cannot deserialize BsonArray from BsonType {0}.", bsonType);
                    throw new Exception(message);
            }
        }

        /// <summary>
        /// Gets the serialization info for individual items of the array.
        /// </summary>
        /// <returns>
        /// The serialization info for the items.
        /// </returns>
        public BsonSerializationInfo GetItemSerializationInfo()
        {
            return new BsonSerializationInfo(
                null,
                BsonValueSerializer.Instance,
                typeof(BsonValue),
                BsonValueSerializer.Instance.GetDefaultSerializationOptions());
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
                throw new ArgumentNullException("value");
            }

            var rawBsonArray = value as RawBsonArray;
            if (rawBsonArray != null)
            {
                RawBsonArraySerializer.Instance.Serialize(bsonWriter, nominalType, value, options);
                return;
            }

            var array = (BsonArray)value;
            bsonWriter.WriteStartArray();
            for (int i = 0; i < array.Count; i++)
            {
                BsonValueSerializer.Instance.Serialize(bsonWriter, typeof(BsonValue), array[i], null);
            }
            bsonWriter.WriteEndArray();
        }
    }
}

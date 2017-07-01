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
using System.Collections.Generic;
using System.IO;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization.Options;

namespace MongoDB.Bson.Serialization.Serializers
{
    /// <summary>
    /// Represents a serializer for two-dimensional arrays.
    /// </summary>
    /// <typeparam name="T">The type of the elements.</typeparam>
    public class TwoDimensionalArraySerializer<T> : BsonBaseSerializer
    {
        // constructors
        /// <summary>
        /// Initializes a new instance of the TwoDimensionalArraySerializer class.
        /// </summary>
        public TwoDimensionalArraySerializer()
            : base(new ArraySerializationOptions())
        {
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
            VerifyTypes(nominalType, actualType, typeof(T[,]));
            var arraySerializationOptions = EnsureSerializationOptions<ArraySerializationOptions>(options);
            var itemSerializationOptions = arraySerializationOptions.ItemSerializationOptions;

            var bsonType = bsonReader.GetCurrentBsonType();
            string message;
            switch (bsonType)
            {
                case BsonType.Null:
                    bsonReader.ReadNull();
                    return null;
                case BsonType.Array:
                    var itemNominalType = typeof(T);
                    var itemNominalTypeIsValueType = itemNominalType.IsValueType;
                    var itemNominalTypeSerializer = BsonSerializer.LookupSerializer(itemNominalType);
                    var itemDiscriminatorConvention = BsonSerializer.LookupDiscriminatorConvention(itemNominalType);
                    Type lastItemType = null;
                    IBsonSerializer lastItemSerializer = null;

                    // if itemNominalType is a value type then these assignments are final
                    var itemActualType = itemNominalType;
                    var itemActualTypeSerializer = itemNominalTypeSerializer;

                    bsonReader.ReadStartArray();
                    var outerList = new List<List<T>>();
                    while (bsonReader.ReadBsonType() != BsonType.EndOfDocument)
                    {
                        bsonReader.ReadStartArray();
                        var innerList = new List<T>();
                        while (bsonReader.ReadBsonType() != BsonType.EndOfDocument)
                        {
                            if (!itemNominalTypeIsValueType)
                            {
                                itemActualType = itemDiscriminatorConvention.GetActualType(bsonReader, itemNominalType);
                                if (itemActualType == itemNominalType)
                                {
                                    itemActualTypeSerializer = itemNominalTypeSerializer;
                                }
                                else if (itemActualType == lastItemType)
                                {
                                    itemActualTypeSerializer = lastItemSerializer;
                                }
                                else
                                {
                                    itemActualTypeSerializer = BsonSerializer.LookupSerializer(itemActualType);
                                    lastItemType = itemActualType;
                                    lastItemSerializer = itemActualTypeSerializer;
                                }
                            }
                            var item = (T)itemActualTypeSerializer.Deserialize(bsonReader, itemNominalType, itemActualType, itemSerializationOptions);
                            innerList.Add(item);
                        }
                        bsonReader.ReadEndArray();
                        outerList.Add(innerList);
                    }
                    bsonReader.ReadEndArray();

                    var length1 = outerList.Count;
                    var length2 = (length1 == 0) ? 0 : outerList[0].Count;
                    var array = new T[length1, length2];
                    for (int i = 0; i < length1; i++)
                    {
                        var innerList = outerList[i];
                        if (innerList.Count != length2)
                        {
                            message = string.Format("Inner list {0} is of length {1} but should be of length {2}.", i, innerList.Count, length2);
                            throw new Exception(message);
                        }
                        for (int j = 0; j < length2; j++)
                        {
                            array[i, j] = innerList[j];
                        }
                    }

                    return array;
                case BsonType.Document:
                    bsonReader.ReadStartDocument();
                    bsonReader.ReadString("_t"); // skip over discriminator
                    bsonReader.ReadName("_v");
                    var value = Deserialize(bsonReader, actualType, actualType, options);
                    bsonReader.ReadEndDocument();
                    return value;
                default:
                    message = string.Format("Can't deserialize a {0} from BsonType {1}.", actualType.FullName, bsonType);
                    throw new Exception(message);
            }
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
                var actualType = value.GetType();
                VerifyTypes(nominalType, actualType, typeof(T[,]));

                if (nominalType == typeof(object))
                {
                    bsonWriter.WriteStartDocument();
                    bsonWriter.WriteString("_t", TypeNameDiscriminator.GetDiscriminator(actualType));
                    bsonWriter.WriteName("_v");
                    Serialize(bsonWriter, actualType, value, options);
                    bsonWriter.WriteEndDocument();
                    return;
                }

                var array = (T[,])value;
                var arraySerializationOptions = EnsureSerializationOptions<ArraySerializationOptions>(options);
                var itemSerializationOptions = arraySerializationOptions.ItemSerializationOptions;

                var itemNominalType = typeof(T);
                var itemNominalTypeIsValueType = itemNominalType.IsValueType;
                var itemNominalTypeSerializer = BsonSerializer.LookupSerializer(itemNominalType);
                BsonSerializer.LookupDiscriminatorConvention(itemNominalType);
                Type lastItemType = null;
                IBsonSerializer lastItemSerializer = null;

                // if itemNominalType is a value type then these assignments are final
                var itemActualType = itemNominalType;
                var itemActualTypeSerializer = itemNominalTypeSerializer;

                bsonWriter.WriteStartArray();
                var length1 = array.GetLength(0);
                var length2 = array.GetLength(1);
                for (int i = 0; i < length1; i++)
                {
                    bsonWriter.WriteStartArray();
                    for (int j = 0; j < length2; j++)
                    {
                        var item = array[i, j];
                        if (!itemNominalTypeIsValueType)
                        {
                            itemActualType = item == null ? itemNominalType : item.GetType();
                            if (itemActualType == itemNominalType)
                            {
                                itemActualTypeSerializer = itemNominalTypeSerializer;
                            }
                            else if (itemActualType == lastItemType)
                            {
                                itemActualTypeSerializer = lastItemSerializer;
                            }
                            else
                            {
                                itemActualTypeSerializer = BsonSerializer.LookupSerializer(itemActualType);
                                lastItemType = itemActualType;
                                lastItemSerializer = itemActualTypeSerializer;
                            }
                        }
                        itemActualTypeSerializer.Serialize(bsonWriter, itemNominalType, item, itemSerializationOptions);
                    }
                    bsonWriter.WriteEndArray();
                }
                bsonWriter.WriteEndArray();
            }
        }
    }
}

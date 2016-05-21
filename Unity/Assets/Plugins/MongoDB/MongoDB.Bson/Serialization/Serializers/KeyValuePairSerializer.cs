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
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Options;

namespace MongoDB.Bson.Serialization.Serializers
{
    /// <summary>
    /// Represents a serializer for KeyValuePairs.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    public class KeyValuePairSerializer<TKey, TValue> : BsonBaseSerializer
    {
        // private static fields
        private static readonly BsonTrie<bool> __bsonTrie = BuildBsonTrie();

        // private fields
        private volatile IDiscriminatorConvention _cachedKeyDiscriminatorConvention;
        private volatile IDiscriminatorConvention _cachedValueDiscriminatorConvention;
        private volatile IBsonSerializer _cachedKeySerializer;
        private volatile IBsonSerializer _cachedValueSerializer;

        // constructors
        /// <summary>
        /// Initializes a new instance of the KeyValuePairSerializer class.
        /// </summary>
        public KeyValuePairSerializer()
            : base(new KeyValuePairSerializationOptions { Representation = BsonType.Document })
        {
        }

        /// <summary>
        /// Initializes a new instance of the KeyValuePairSerializer class.
        /// </summary>
        /// <param name="defaultSerializationOptions">The default serialization options for this serializer.</param>
        public KeyValuePairSerializer(IBsonSerializationOptions defaultSerializationOptions)
            : base(defaultSerializationOptions)
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
            VerifyTypes(nominalType, actualType, typeof(KeyValuePair<TKey, TValue>));
            var keyValuePairSerializationOptions = EnsureSerializationOptions<KeyValuePairSerializationOptions>(options);

            var keyDiscriminatorConvention = GetKeyDiscriminatorConvention();
            var valueDiscriminatorConvention = GetValueDiscriminatorConvention();
            TKey key;
            TValue value;            

            var bsonType = bsonReader.GetCurrentBsonType();
            if (bsonType == BsonType.Array)
            {
                bsonReader.ReadStartArray();
                bsonReader.ReadBsonType();
                var keyType = keyDiscriminatorConvention.GetActualType(bsonReader, typeof(TKey));
                var keySerializer = GetKeySerializer(keyType);
                key = (TKey)keySerializer.Deserialize(bsonReader, typeof(TKey), keyType, keyValuePairSerializationOptions.KeySerializationOptions);
                bsonReader.ReadBsonType();
                var valueType = valueDiscriminatorConvention.GetActualType(bsonReader, typeof(TValue));
                var valueSerializer = GetValueSerializer(valueType);
                value = (TValue)valueSerializer.Deserialize(bsonReader, typeof(TValue), valueType, keyValuePairSerializationOptions.ValueSerializationOptions);
                bsonReader.ReadEndArray();
            }
            else if (bsonType == BsonType.Document)
            {
                bsonReader.ReadStartDocument();
                key = default(TKey);
                value = default(TValue);
                bool keyFound = false, valueFound = false;
                bool elementFound;
                bool elementIsKey;
                while (bsonReader.ReadBsonType(__bsonTrie, out elementFound, out elementIsKey) != BsonType.EndOfDocument)
                {
                    var name = bsonReader.ReadName();
                    if (elementFound)
                    {
                        if (elementIsKey)
                        {
                            var keyType = keyDiscriminatorConvention.GetActualType(bsonReader, typeof(TKey));
                            var keySerializer = GetValueSerializer(keyType);
                            key = (TKey)keySerializer.Deserialize(bsonReader, typeof(TKey), keyType, keyValuePairSerializationOptions.KeySerializationOptions);
                            keyFound = true;
                        }
                        else
                        {
                            var valueType = valueDiscriminatorConvention.GetActualType(bsonReader, typeof(TValue));
                            var valueSerializer = GetValueSerializer(valueType);
                            value = (TValue)valueSerializer.Deserialize(bsonReader, typeof(TValue), valueType, keyValuePairSerializationOptions.ValueSerializationOptions);
                            valueFound = true;
                        }
                    }
                    else
                    {
                        var message = string.Format("Element '{0}' is not valid for KeyValuePairs (expecting 'k' or 'v').", name);
                        throw new BsonSerializationException(message);
                    }
                }
                bsonReader.ReadEndDocument();

                if (!keyFound)
                {
                    throw new Exception("KeyValuePair item was missing the 'k' element.");
                }
                if (!valueFound)
                {
                    throw new Exception("KeyValuePair item was missing the 'v' element.");
                }
            }
            else
            {
                var message = string.Format(
                    "Cannot deserialize '{0}' from BsonType {1}.",
                    BsonUtils.GetFriendlyTypeName(typeof(KeyValuePair<TKey, TValue>)),
                    bsonType);
                throw new Exception(message);
            }

            return new KeyValuePair<TKey, TValue>(key, value);
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
            var keyValuePair = (KeyValuePair<TKey, TValue>)value;
            var keyValuePairSerializationOptions = EnsureSerializationOptions<KeyValuePairSerializationOptions>(options);

            var keyType = (keyValuePair.Key == null) ? typeof(TKey) : keyValuePair.Key.GetType();
            var keySerializer = GetKeySerializer(keyType);
            var valueType = (keyValuePair.Value == null) ? typeof(TValue) : keyValuePair.Value.GetType();
            var valueSerializer = GetValueSerializer(valueType);

            switch (keyValuePairSerializationOptions.Representation)
            {
                case BsonType.Array:
                    bsonWriter.WriteStartArray();
                    keySerializer.Serialize(bsonWriter, typeof(TKey), keyValuePair.Key, keyValuePairSerializationOptions.KeySerializationOptions);
                    valueSerializer.Serialize(bsonWriter, typeof(TValue), keyValuePair.Value, keyValuePairSerializationOptions.ValueSerializationOptions);
                    bsonWriter.WriteEndArray();
                    break;
                case BsonType.Document:
                    bsonWriter.WriteStartDocument();
                    bsonWriter.WriteName("k");
                    keySerializer.Serialize(bsonWriter, typeof(TKey), keyValuePair.Key, keyValuePairSerializationOptions.KeySerializationOptions);
                    bsonWriter.WriteName("v");
                    valueSerializer.Serialize(bsonWriter, typeof(TValue), keyValuePair.Value, keyValuePairSerializationOptions.ValueSerializationOptions);
                    bsonWriter.WriteEndDocument();
                    break;
                default:
                    var message = string.Format(
                        "'{0}' is not a valid {1} representation.",
                        keyValuePairSerializationOptions.Representation,
                        BsonUtils.GetFriendlyTypeName(typeof(KeyValuePair<TKey, TValue>)));
                    throw new BsonSerializationException(message);
            }
        }

        // private static methods
        /// <summary>
        /// Builds the bson decoding trie.
        /// </summary>
        /// <returns>A BsonTrie.</returns>
        private static BsonTrie<bool> BuildBsonTrie()
        {
            var bsonTrie = new BsonTrie<bool>();
            bsonTrie.Add("k", true); // is key
            bsonTrie.Add("v", false);
            return bsonTrie;
        }

        // private methods
        /// <summary>
        /// Gets the discriminator convention for keys.
        /// </summary>
        /// <returns>The discriminator convention for the class.</returns>
        private IDiscriminatorConvention GetKeyDiscriminatorConvention()
        {
            // return a cached discriminator convention when possible
            var discriminatorConvention = _cachedKeyDiscriminatorConvention;
            if (discriminatorConvention == null)
            {
                // it's possible but harmless for multiple threads to do the initial lookup at the same time
                discriminatorConvention = BsonSerializer.LookupDiscriminatorConvention(typeof(TKey));
                _cachedKeyDiscriminatorConvention = discriminatorConvention;
            }
            return discriminatorConvention;
        }

        /// <summary>
        /// Gets the key serializer.
        /// </summary>
        /// <param name="actualType">The actual type of the key.</param>
        /// <returns>The serializer for the key type.</returns>
        private IBsonSerializer GetKeySerializer(Type actualType)
        {
            // return a cached serializer when possible
            if (actualType == typeof(TKey))
            {
                var serializer = _cachedKeySerializer;
                if (serializer == null)
                {
                    // it's possible but harmless for multiple threads to do the initial lookup at the same time
                    serializer = BsonSerializer.LookupSerializer(typeof(TKey));
                    _cachedKeySerializer = serializer;
                }
                return serializer;
            }
            else
            {
                return BsonSerializer.LookupSerializer(actualType);
            }
        }

        /// <summary>
        /// Gets the discriminator convention for values.
        /// </summary>
        /// <returns>The discriminator convention for the class.</returns>
        private IDiscriminatorConvention GetValueDiscriminatorConvention()
        {
            // return a cached discriminator convention when possible
            var discriminatorConvention = _cachedValueDiscriminatorConvention;
            if (discriminatorConvention == null)
            {
                // it's possible but harmless for multiple threads to do the initial lookup at the same time
                discriminatorConvention = BsonSerializer.LookupDiscriminatorConvention(typeof(TValue));
                _cachedValueDiscriminatorConvention = discriminatorConvention;
            }
            return discriminatorConvention;
        }

        /// <summary>
        /// Gets the value serializer.
        /// </summary>
        /// <param name="actualType">The actual type of the value.</param>
        /// <returns>The serializer for the value type.</returns>
        private IBsonSerializer GetValueSerializer(Type actualType)
        {
            // return a cached serializer when possible
            if (actualType == typeof(TValue))
            {
                var serializer = _cachedValueSerializer;
                if (serializer == null)
                {
                    // it's possible but harmless for multiple threads to do the initial lookup at the same time
                    serializer = BsonSerializer.LookupSerializer(typeof(TValue));
                    _cachedValueSerializer = serializer;
                }
                return serializer;
            }
            else
            {
                return BsonSerializer.LookupSerializer(actualType);
            }
        }
    }
}

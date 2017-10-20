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
    public class KeyValuePairSerializer<TKey, TValue> :
        StructSerializerBase<KeyValuePair<TKey, TValue>>,
        IBsonDocumentSerializer
    {
        // private constants
        private static class Flags
        {
            public const long Key = 1;
            public const long Value = 2;
        }

        // private fields
        private readonly SerializerHelper _helper;
        private readonly Lazy<IBsonSerializer<TKey>> _lazyKeySerializer;
        private readonly BsonType _representation;
        private readonly Lazy<IBsonSerializer<TValue>> _lazyValueSerializer;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValuePairSerializer{TKey, TValue}"/> class.
        /// </summary>
        public KeyValuePairSerializer()
            : this(BsonType.Document)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValuePairSerializer{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="representation">The representation.</param>
        public KeyValuePairSerializer(BsonType representation)
            : this(representation, BsonSerializer.SerializerRegistry)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValuePairSerializer{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="representation">The representation.</param>
        /// <param name="keySerializer">The key serializer.</param>
        /// <param name="valueSerializer">The value serializer.</param>
        public KeyValuePairSerializer(BsonType representation, IBsonSerializer<TKey> keySerializer, IBsonSerializer<TValue> valueSerializer)
            : this(
                representation,
                new Lazy<IBsonSerializer<TKey>>(() => keySerializer),
                new Lazy<IBsonSerializer<TValue>>(() => valueSerializer))
        {
            if (keySerializer == null)
            {
                throw new ArgumentNullException("keySerializer");
            }
            if (valueSerializer == null)
            {
                throw new ArgumentNullException("valueSerializer");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValuePairSerializer{TKey, TValue}" /> class.
        /// </summary>
        /// <param name="representation">The representation.</param>
        /// <param name="serializerRegistry">The serializer registry.</param>
        public KeyValuePairSerializer(BsonType representation, IBsonSerializerRegistry serializerRegistry)
            : this(
                representation,
                new Lazy<IBsonSerializer<TKey>>(() => serializerRegistry.GetSerializer<TKey>()),
                new Lazy<IBsonSerializer<TValue>>(() => serializerRegistry.GetSerializer<TValue>()))
        {
            if (serializerRegistry == null)
            {
                throw new ArgumentNullException("serializerRegistry");
            }
        }

        private KeyValuePairSerializer(BsonType representation, Lazy<IBsonSerializer<TKey>> lazyKeySerializer, Lazy<IBsonSerializer<TValue>> lazyValueSerializer)
        {
            switch (representation)
            {
                case BsonType.Array:
                case BsonType.Document:
                    break;

                default:
                    var message = string.Format("{0} is not a valid representation for a KeyValuePairSerializer.", representation);
                    throw new ArgumentException(message);
            }

            _representation = representation;
            _lazyKeySerializer = lazyKeySerializer;
            _lazyValueSerializer = lazyValueSerializer;

            _helper = new SerializerHelper
            (
                new SerializerHelper.Member("k", Flags.Key),
                new SerializerHelper.Member("v", Flags.Value)
            );
        }

        // public properties
        /// <summary>
        /// Gets the key serializer.
        /// </summary>
        /// <value>
        /// The key serializer.
        /// </value>
        public IBsonSerializer<TKey> KeySerializer
        {
            get { return _lazyKeySerializer.Value; }
        }

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

        /// <summary>
        /// Gets the value serializer.
        /// </summary>
        /// <value>
        /// The value serializer.
        /// </value>
        public IBsonSerializer<TValue> ValueSerializer
        {
            get { return _lazyValueSerializer.Value; }
        }

        // public methods
        /// <summary>
        /// Deserializes a value.
        /// </summary>
        /// <param name="context">The deserialization context.</param>
        /// <param name="args">The deserialization args.</param>
        /// <returns>A deserialized value.</returns>
        public override KeyValuePair<TKey, TValue> Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var bsonReader = context.Reader;
            var bsonType = bsonReader.GetCurrentBsonType();
            switch (bsonType)
            {
                case BsonType.Array:
                    return DeserializeArrayRepresentation(context);
                case BsonType.Document:
                    return DeserializeDocumentRepresentation(context);
                default:
                    throw CreateCannotDeserializeFromBsonTypeException(bsonType);
            }
        }

        /// <summary>
        /// Serializes a value.
        /// </summary>
        /// <param name="context">The serialization context.</param>
        /// <param name="args">The serialization args.</param>
        /// <param name="value">The object.</param>
        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, KeyValuePair<TKey, TValue> value)
        {
            switch (_representation)
            {
                case BsonType.Array:
                    SerializeArrayRepresentation(context, value);
                    break;

                case BsonType.Document:
                    SerializeDocumentRepresentation(context, value);
                    break;

                default:
                    var message = string.Format(
                        "'{0}' is not a valid {1} representation.",
                        _representation,
                        BsonUtils.GetFriendlyTypeName(typeof(KeyValuePair<TKey, TValue>)));
                    throw new BsonSerializationException(message);
            }
        }

        /// <inheritdoc />
        public bool TryGetMemberSerializationInfo(string memberName, out BsonSerializationInfo serializationInfo)
        {
            if (_representation != BsonType.Document)
            {
                serializationInfo = null;
                return false;
            }

            switch (memberName)
            {
                case "Key":
                    serializationInfo = new BsonSerializationInfo("k", _lazyKeySerializer.Value, _lazyKeySerializer.Value.ValueType);
                    return true;
                case "Value":
                    serializationInfo = new BsonSerializationInfo("v", _lazyValueSerializer.Value, _lazyValueSerializer.Value.ValueType);
                    return true;
            }

            serializationInfo = null;
            return false;
        }

        // private methods
        private KeyValuePair<TKey, TValue> DeserializeArrayRepresentation(BsonDeserializationContext context)
        {
            var bsonReader = context.Reader;
            bsonReader.ReadStartArray();
            var key = _lazyKeySerializer.Value.Deserialize(context);
            var value = _lazyValueSerializer.Value.Deserialize(context);
            bsonReader.ReadEndArray();
            return new KeyValuePair<TKey, TValue>(key, value);
        }

        private KeyValuePair<TKey, TValue> DeserializeDocumentRepresentation(BsonDeserializationContext context)
        {
            var key = default(TKey);
            var value = default(TValue);
            _helper.DeserializeMembers(context, (elementName, flag) =>
            {
                switch (flag)
                {
                    case Flags.Key: key = _lazyKeySerializer.Value.Deserialize(context); break;
                    case Flags.Value: value = _lazyValueSerializer.Value.Deserialize(context); break;
                }
            });
            return new KeyValuePair<TKey, TValue>(key, value);
        }

        private void SerializeArrayRepresentation(BsonSerializationContext context, KeyValuePair<TKey, TValue> value)
        {
            var bsonWriter = context.Writer;
            bsonWriter.WriteStartArray();
            _lazyKeySerializer.Value.Serialize(context, value.Key);
            _lazyValueSerializer.Value.Serialize(context, value.Value);
            bsonWriter.WriteEndArray();
        }

        private void SerializeDocumentRepresentation(BsonSerializationContext context, KeyValuePair<TKey, TValue> value)
        {
            var bsonWriter = context.Writer;
            bsonWriter.WriteStartDocument();
            bsonWriter.WriteName("k");
            _lazyKeySerializer.Value.Serialize(context, value.Key);
            bsonWriter.WriteName("v");
            _lazyValueSerializer.Value.Serialize(context, value.Value);
            bsonWriter.WriteEndDocument();
        }
    }
}

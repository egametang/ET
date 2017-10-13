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
using System.Collections;
using System.Collections.Generic;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization.Options;

namespace MongoDB.Bson.Serialization.Serializers
{
    /// <summary>
    /// Represents a serializer for dictionaries.
    /// </summary>
    /// <typeparam name="TDictionary">The type of the dictionary.</typeparam>
    public abstract class DictionarySerializerBase<TDictionary> :
        ClassSerializerBase<TDictionary>,
        IBsonDocumentSerializer,
        IBsonDictionarySerializer
        where TDictionary : class, IDictionary
    {
        // private constants
        private static class Flags
        {
            public const long Key = 1;
            public const long Value = 2;
        }

        // private fields
        private readonly DictionaryRepresentation _dictionaryRepresentation;
        private readonly SerializerHelper _helper;
        private readonly IBsonSerializer _keySerializer;
        private readonly IBsonSerializer _valueSerializer;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="DictionarySerializerBase{TDictionary}"/> class.
        /// </summary>
        public DictionarySerializerBase()
            : this(DictionaryRepresentation.Document)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DictionarySerializerBase{TDictionary}"/> class.
        /// </summary>
        /// <param name="dictionaryRepresentation">The dictionary representation.</param>
        public DictionarySerializerBase(DictionaryRepresentation dictionaryRepresentation)
            : this(dictionaryRepresentation, new ObjectSerializer(), new ObjectSerializer())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DictionarySerializerBase{TDictionary}"/> class.
        /// </summary>
        /// <param name="dictionaryRepresentation">The dictionary representation.</param>
        /// <param name="keySerializer">The key serializer.</param>
        /// <param name="valueSerializer">The value serializer.</param>
        public DictionarySerializerBase(DictionaryRepresentation dictionaryRepresentation, IBsonSerializer keySerializer, IBsonSerializer valueSerializer)
        {
            _dictionaryRepresentation = dictionaryRepresentation;
            _keySerializer = keySerializer;
            _valueSerializer = valueSerializer;

            _helper = new SerializerHelper
            (
                new SerializerHelper.Member("k", Flags.Key),
                new SerializerHelper.Member("v", Flags.Value)
            );
        }

        // public properties
        /// <summary>
        /// Gets the dictionary representation.
        /// </summary>
        /// <value>
        /// The dictionary representation.
        /// </value>
        public DictionaryRepresentation DictionaryRepresentation
        {
            get { return _dictionaryRepresentation; }
        }

        /// <summary>
        /// Gets the key serializer.
        /// </summary>
        /// <value>
        /// The key serializer.
        /// </value>
        public IBsonSerializer KeySerializer
        {
            get { return _keySerializer; }
        }

        /// <summary>
        /// Gets the value serializer.
        /// </summary>
        /// <value>
        /// The value serializer.
        /// </value>
        public IBsonSerializer ValueSerializer
        {
            get { return _valueSerializer; }
        }

        // public methods        
        /// <inheritdoc/>
        public bool TryGetMemberSerializationInfo(string memberName, out BsonSerializationInfo serializationInfo)
        {
            if (_dictionaryRepresentation != DictionaryRepresentation.Document)
            {
                serializationInfo = null;
                return false;
            }

            serializationInfo = new BsonSerializationInfo(
                memberName,
                _valueSerializer,
                _valueSerializer.ValueType);
            return true;
        }

        // protected methods
        /// <summary>
        /// Deserializes a value.
        /// </summary>
        /// <param name="context">The deserialization context.</param>
        /// <param name="args">The deserialization args.</param>
        /// <returns>A deserialized value.</returns>
        protected override TDictionary DeserializeValue(BsonDeserializationContext context, BsonDeserializationArgs args)
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
        protected override void SerializeValue(BsonSerializationContext context, BsonSerializationArgs args, TDictionary value)
        {
            var bsonWriter = context.Writer;

            switch (_dictionaryRepresentation)
            {
                case DictionaryRepresentation.Document:
                    SerializeDocumentRepresentation(context, value);
                    break;

                case DictionaryRepresentation.ArrayOfArrays:
                    SerializeArrayOfArraysRepresentation(context, value);
                    break;

                case DictionaryRepresentation.ArrayOfDocuments:
                    SerializeArrayOfDocumentsRepresentation(context, value);
                    break;

                default:
                    var message = string.Format("'{0}' is not a valid IDictionary representation.", _dictionaryRepresentation);
                    throw new BsonSerializationException(message);
            }
        }

        // protected methods
        /// <summary>
        /// Creates the instance.
        /// </summary>
        /// <returns>The instance.</returns>
        protected abstract TDictionary CreateInstance();

        // private methods
        private TDictionary DeserializeArrayRepresentation(BsonDeserializationContext context)
        {
            var dictionary = CreateInstance();

            var bsonReader = context.Reader;
            bsonReader.ReadStartArray();
            while (bsonReader.ReadBsonType() != BsonType.EndOfDocument)
            {
                object key;
                object value;

                var bsonType = bsonReader.GetCurrentBsonType();
                switch (bsonType)
                {
                    case BsonType.Array:
                        bsonReader.ReadStartArray();
                        key = _keySerializer.Deserialize(context);
                        value = _valueSerializer.Deserialize(context);
                        bsonReader.ReadEndArray();
                        break;

                    case BsonType.Document:
                        key = null;
                        value = null;
                        _helper.DeserializeMembers(context, (elementName, flag) =>
                        {
                            switch (flag)
                            {
                                case Flags.Key: key = _keySerializer.Deserialize(context); break;
                                case Flags.Value: value = _valueSerializer.Deserialize(context); break;
                            }
                        });
                        break;

                    default:
                        throw CreateCannotDeserializeFromBsonTypeException(bsonType);
                }

                dictionary.Add(key, value);
            }
            bsonReader.ReadEndArray();

            return dictionary;
        }

        private TDictionary DeserializeDocumentRepresentation(BsonDeserializationContext context)
        {
            var dictionary = CreateInstance();
            var bsonReader = context.Reader;
            bsonReader.ReadStartDocument();
            while (bsonReader.ReadBsonType() != BsonType.EndOfDocument)
            {
                var key = DeserializeKeyString(bsonReader.ReadName());
                var value = _valueSerializer.Deserialize(context);
                dictionary.Add(key, value);
            }
            bsonReader.ReadEndDocument();
            return dictionary;
        }

        private object DeserializeKeyString(string keyString)
        {
            var keyDocument = new BsonDocument("k", keyString);
            using (var keyReader = new BsonDocumentReader(keyDocument))
            {
                var context = BsonDeserializationContext.CreateRoot(keyReader);
                keyReader.ReadStartDocument();
                keyReader.ReadName("k");
                var key = _keySerializer.Deserialize(context);
                keyReader.ReadEndDocument();
                return key;
            }
        }

        private void SerializeArrayOfArraysRepresentation(BsonSerializationContext context, TDictionary value)
        {
            var bsonWriter = context.Writer;
            bsonWriter.WriteStartArray();
            foreach (DictionaryEntry dictionaryEntry in value)
            {
                bsonWriter.WriteStartArray();
                _keySerializer.Serialize(context, dictionaryEntry.Key);
                _valueSerializer.Serialize(context, dictionaryEntry.Value);
                bsonWriter.WriteEndArray();
            }
            bsonWriter.WriteEndArray();
        }

        private void SerializeArrayOfDocumentsRepresentation(BsonSerializationContext context, TDictionary value)
        {
            var bsonWriter = context.Writer;
            bsonWriter.WriteStartArray();
            foreach (DictionaryEntry dictionaryEntry in value)
            {
                bsonWriter.WriteStartDocument();
                bsonWriter.WriteName("k");
                _keySerializer.Serialize(context, dictionaryEntry.Key);
                bsonWriter.WriteName("v");
                _valueSerializer.Serialize(context, dictionaryEntry.Value);
                bsonWriter.WriteEndDocument();
            }
            bsonWriter.WriteEndArray();
        }

        private void SerializeDocumentRepresentation(BsonSerializationContext context, TDictionary value)
        {
            var bsonWriter = context.Writer;
            bsonWriter.WriteStartDocument();
            foreach (DictionaryEntry dictionaryEntry in value)
            {
                bsonWriter.WriteName(SerializeKeyString(dictionaryEntry.Key));
                _valueSerializer.Serialize(context, dictionaryEntry.Value);
            }
            bsonWriter.WriteEndDocument();
        }

        private string SerializeKeyString(object key)
        {
            var keyDocument = new BsonDocument();
            using (var keyWriter = new BsonDocumentWriter(keyDocument))
            {
                var context = BsonSerializationContext.CreateRoot(keyWriter);
                keyWriter.WriteStartDocument();
                keyWriter.WriteName("k");
                _keySerializer.Serialize(context, key);
                keyWriter.WriteEndDocument();
            }

            var keyValue = keyDocument["k"];
            if (keyValue.BsonType != BsonType.String)
            {
                throw new BsonSerializationException("When using DictionaryRepresentation.Document key values must serialize as strings.");
            }

            return (string)keyValue;
        }
    }

    /// <summary>
    /// Represents a serializer for dictionaries.
    /// </summary>
    /// <typeparam name="TDictionary">The type of the dictionary.</typeparam>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    public abstract class DictionarySerializerBase<TDictionary, TKey, TValue> :
        ClassSerializerBase<TDictionary>,
        IBsonArraySerializer,
        IBsonDocumentSerializer,
        IBsonDictionarySerializer
        where TDictionary : class, IDictionary<TKey, TValue>
    {
        // private constants
        private static class Flags
        {
            public const long Key = 1;
            public const long Value = 2;
        }

        // private fields
        private readonly DictionaryRepresentation _dictionaryRepresentation;
        private readonly SerializerHelper _helper;
        private readonly Lazy<IBsonSerializer<TKey>> _lazyKeySerializer;
        private readonly Lazy<IBsonSerializer<TValue>> _lazyValueSerializer;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="DictionarySerializerBase{TDictionary, TKey, TValue}"/> class.
        /// </summary>
        public DictionarySerializerBase()
            : this(DictionaryRepresentation.Document)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DictionarySerializerBase{TDictionary, TKey, TValue}" /> class.
        /// </summary>
        /// <param name="dictionaryRepresentation">The dictionary representation.</param>
        public DictionarySerializerBase(DictionaryRepresentation dictionaryRepresentation)
            : this(dictionaryRepresentation, BsonSerializer.SerializerRegistry)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DictionarySerializerBase{TDictionary, TKey, TValue}" /> class.
        /// </summary>
        /// <param name="dictionaryRepresentation">The dictionary representation.</param>
        /// <param name="keySerializer">The key serializer.</param>
        /// <param name="valueSerializer">The value serializer.</param>
        public DictionarySerializerBase(DictionaryRepresentation dictionaryRepresentation, IBsonSerializer<TKey> keySerializer, IBsonSerializer<TValue> valueSerializer)
            : this(
                dictionaryRepresentation,
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
        /// Initializes a new instance of the <see cref="DictionarySerializerBase{TDictionary, TKey, TValue}" /> class.
        /// </summary>
        /// <param name="dictionaryRepresentation">The dictionary representation.</param>
        /// <param name="serializerRegistry">The serializer registry.</param>
        public DictionarySerializerBase(DictionaryRepresentation dictionaryRepresentation, IBsonSerializerRegistry serializerRegistry)
            : this(
                dictionaryRepresentation,
                new Lazy<IBsonSerializer<TKey>>(() => serializerRegistry.GetSerializer<TKey>()),
                new Lazy<IBsonSerializer<TValue>>(() => serializerRegistry.GetSerializer<TValue>()))
        {
            if (serializerRegistry == null)
            {
                throw new ArgumentNullException("serializerRegistry");
            }
        }

        private DictionarySerializerBase(
            DictionaryRepresentation dictionaryRepresentation,
            Lazy<IBsonSerializer<TKey>> lazyKeySerializer,
            Lazy<IBsonSerializer<TValue>> lazyValueSerializer)
        {
            _dictionaryRepresentation = dictionaryRepresentation;
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
        /// Gets the dictionary representation.
        /// </summary>
        /// <value>
        /// The dictionary representation.
        /// </value>
        public DictionaryRepresentation DictionaryRepresentation
        {
            get { return _dictionaryRepresentation; }
        }

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
        /// <inheritdoc/>
        public bool TryGetItemSerializationInfo(out BsonSerializationInfo serializationInfo)
        {
            if (_dictionaryRepresentation != DictionaryRepresentation.ArrayOfDocuments)
            {
                serializationInfo = null;
                return false;
            }

            var serializer = new KeyValuePairSerializer<TKey, TValue>(
                BsonType.Document,
                _lazyKeySerializer.Value,
                _lazyValueSerializer.Value);
            serializationInfo = new BsonSerializationInfo(
                null,
                serializer,
                serializer.ValueType);
            return true;
        }

        /// <inheritdoc/>
        public bool TryGetMemberSerializationInfo(string memberName, out BsonSerializationInfo serializationInfo)
        {
            if (_dictionaryRepresentation != DictionaryRepresentation.Document)
            {
                serializationInfo = null;
                return false;
            }

            serializationInfo = new BsonSerializationInfo(
                memberName,
                _lazyValueSerializer.Value,
                _lazyValueSerializer.Value.ValueType);
            return true;
        }

        // protected methods
        /// <summary>
        /// Deserializes a value.
        /// </summary>
        /// <param name="context">The deserialization context.</param>
        /// <param name="args">The deserialization args.</param>
        /// <returns>A deserialized value.</returns>
        protected override TDictionary DeserializeValue(BsonDeserializationContext context, BsonDeserializationArgs args)
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
        protected override void SerializeValue(BsonSerializationContext context, BsonSerializationArgs args, TDictionary value)
        {
            var bsonWriter = context.Writer;

            switch (_dictionaryRepresentation)
            {
                case DictionaryRepresentation.Document:
                    SerializeDocumentRepresentation(context, value);
                    break;

                case DictionaryRepresentation.ArrayOfArrays:
                    SerializeArrayOfArraysRepresentation(context, value);
                    break;

                case DictionaryRepresentation.ArrayOfDocuments:
                    SerializeArrayOfDocumentsRepresentation(context, value);
                    break;

                default:
                    var message = string.Format("'{0}' is not a valid IDictionary<{1}, {2}> representation.",
                        _dictionaryRepresentation,
                        BsonUtils.GetFriendlyTypeName(typeof(TKey)),
                        BsonUtils.GetFriendlyTypeName(typeof(TValue)));
                    throw new BsonSerializationException(message);
            }
        }

        // protected methods
        /// <summary>
        /// Creates the instance.
        /// </summary>
        /// <returns>The instance.</returns>
        protected abstract TDictionary CreateInstance();

        // private methods
        private TDictionary DeserializeArrayRepresentation(BsonDeserializationContext context)
        {
            var dictionary = CreateInstance();

            var bsonReader = context.Reader;
            bsonReader.ReadStartArray();
            while (bsonReader.ReadBsonType() != BsonType.EndOfDocument)
            {
                TKey key;
                TValue value;

                var bsonType = bsonReader.GetCurrentBsonType();
                switch (bsonType)
                {
                    case BsonType.Array:
                        bsonReader.ReadStartArray();
                        key = _lazyKeySerializer.Value.Deserialize(context);
                        value = _lazyValueSerializer.Value.Deserialize(context);
                        bsonReader.ReadEndArray();
                        break;

                    case BsonType.Document:
                        key = default(TKey);
                        value = default(TValue);
                        _helper.DeserializeMembers(context, (elementName, flag) =>
                        {
                            switch (flag)
                            {
                                case Flags.Key: key = _lazyKeySerializer.Value.Deserialize(context); break;
                                case Flags.Value: value = _lazyValueSerializer.Value.Deserialize(context); break;
                            }
                        });
                        break;

                    default:
                        throw CreateCannotDeserializeFromBsonTypeException(bsonType);
                }

                dictionary.Add(key, value);
            }
            bsonReader.ReadEndArray();

            return dictionary;
        }

        private TDictionary DeserializeDocumentRepresentation(BsonDeserializationContext context)
        {
            var dictionary = CreateInstance();

            var bsonReader = context.Reader;
            bsonReader.ReadStartDocument();
            while (bsonReader.ReadBsonType() != BsonType.EndOfDocument)
            {
                var key = DeserializeKeyString(bsonReader.ReadName());
                var value = _lazyValueSerializer.Value.Deserialize(context);
                dictionary.Add(key, value);
            }
            bsonReader.ReadEndDocument();

            return dictionary;
        }

        private TKey DeserializeKeyString(string keyString)
        {
            var keyDocument = new BsonDocument("k", keyString);
            using (var keyReader = new BsonDocumentReader(keyDocument))
            {
                var context = BsonDeserializationContext.CreateRoot(keyReader);
                keyReader.ReadStartDocument();
                keyReader.ReadName("k");
                var key = _lazyKeySerializer.Value.Deserialize(context);
                keyReader.ReadEndDocument();
                return key;
            }
        }

        private void SerializeArrayOfArraysRepresentation(BsonSerializationContext context, TDictionary value)
        {
            var bsonWriter = context.Writer;
            bsonWriter.WriteStartArray();
            foreach (var keyValuePair in value)
            {
                bsonWriter.WriteStartArray();
                _lazyKeySerializer.Value.Serialize(context, keyValuePair.Key);
                _lazyValueSerializer.Value.Serialize(context, keyValuePair.Value);
                bsonWriter.WriteEndArray();
            }
            bsonWriter.WriteEndArray();
        }

        private void SerializeArrayOfDocumentsRepresentation(BsonSerializationContext context, TDictionary value)
        {
            var bsonWriter = context.Writer;
            bsonWriter.WriteStartArray();
            foreach (var keyValuePair in value)
            {
                bsonWriter.WriteStartDocument();
                bsonWriter.WriteName("k");
                _lazyKeySerializer.Value.Serialize(context, keyValuePair.Key);
                bsonWriter.WriteName("v");
                _lazyValueSerializer.Value.Serialize(context, keyValuePair.Value);
                bsonWriter.WriteEndDocument();
            }
            bsonWriter.WriteEndArray();
        }

        private void SerializeDocumentRepresentation(BsonSerializationContext context, TDictionary value)
        {
            var bsonWriter = context.Writer;
            bsonWriter.WriteStartDocument();
            foreach (var keyValuePair in value)
            {
                bsonWriter.WriteName(SerializeKeyString(keyValuePair.Key));
                _lazyValueSerializer.Value.Serialize(context, keyValuePair.Value);
            }
            bsonWriter.WriteEndDocument();
        }

        private string SerializeKeyString(TKey key)
        {
            var keyDocument = new BsonDocument();
            using (var keyWriter = new BsonDocumentWriter(keyDocument))
            {
                var context = BsonSerializationContext.CreateRoot(keyWriter);
                keyWriter.WriteStartDocument();
                keyWriter.WriteName("k");
                _lazyKeySerializer.Value.Serialize(context, key);
                keyWriter.WriteEndDocument();
            }

            var keyValue = keyDocument["k"];
            if (keyValue.BsonType != BsonType.String)
            {
                throw new BsonSerializationException("When using DictionaryRepresentation.Document key values must serialize as strings.");
            }

            return (string)keyValue;
        }

        // explicit interface implementations
        IBsonSerializer IBsonDictionarySerializer.KeySerializer
        {
            get { return KeySerializer; }
        }

        IBsonSerializer IBsonDictionarySerializer.ValueSerializer
        {
            get { return ValueSerializer; }
        }
    }
}

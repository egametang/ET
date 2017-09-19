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
using MongoDB.Bson.Serialization.Conventions;

namespace MongoDB.Bson.Serialization.Serializers
{
    /// <summary>
    /// Represents a base serializer for enumerable values.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    public abstract class EnumerableSerializerBase<TValue> : SerializerBase<TValue>, IBsonArraySerializer where TValue : class, IEnumerable
    {
        // private fields
        private readonly IDiscriminatorConvention _discriminatorConvention = new ScalarDiscriminatorConvention("_t");
        private readonly Lazy<IBsonSerializer> _lazyItemSerializer;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="EnumerableSerializerBase{TValue}"/> class.
        /// </summary>
        protected EnumerableSerializerBase()
            : this(BsonSerializer.SerializerRegistry)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumerableSerializerBase{TValue}"/> class.
        /// </summary>
        /// <param name="itemSerializer">The item serializer.</param>
        protected EnumerableSerializerBase(IBsonSerializer itemSerializer)
        {
            if (itemSerializer == null)
            {
                throw new ArgumentNullException("itemSerializer");
            }

            _lazyItemSerializer = new Lazy<IBsonSerializer>(() => itemSerializer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumerableSerializerBase{TValue}" /> class.
        /// </summary>
        /// <param name="serializerRegistry">The serializer registry.</param>
        protected EnumerableSerializerBase(IBsonSerializerRegistry serializerRegistry)
        {
            if (serializerRegistry == null)
            {
                throw new ArgumentNullException("serializerRegistry");
            }

            _lazyItemSerializer = new Lazy<IBsonSerializer>(() => serializerRegistry.GetSerializer(typeof(object)));
        }

        /// <summary>
        /// Gets the item serializer.
        /// </summary>
        /// <value>
        /// The item serializer.
        /// </value>
        public IBsonSerializer ItemSerializer
        {
            get { return _lazyItemSerializer.Value; }
        }

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

            var bsonType = bsonReader.GetCurrentBsonType();
            switch (bsonType)
            {
                case BsonType.Null:
                    bsonReader.ReadNull();
                    return null;

                case BsonType.Array:
                    bsonReader.ReadStartArray();
                    var accumulator = CreateAccumulator();
                    while (bsonReader.ReadBsonType() != BsonType.EndOfDocument)
                    {
                        var item = _lazyItemSerializer.Value.Deserialize(context);
                        AddItem(accumulator, item);
                    }
                    bsonReader.ReadEndArray();
                    return FinalizeResult(accumulator);

                case BsonType.Document:
                    var serializer = new DiscriminatedWrapperSerializer<TValue>(_discriminatorConvention, this);
                    if (serializer.IsPositionedAtDiscriminatedWrapper(context))
                    {
                        return (TValue)serializer.Deserialize(context);
                    }
                    else
                    {
                        goto default;
                    }

                default:
                    throw CreateCannotDeserializeFromBsonTypeException(bsonType);
            }
        }

        /// <summary>
        /// Tries to get the serialization info for the individual items of the array.
        /// </summary>
        /// <param name="serializationInfo">The serialization information.</param>
        /// <returns>
        ///   <c>true</c> if the serialization info exists; otherwise <c>false</c>.
        /// </returns>
        public bool TryGetItemSerializationInfo(out BsonSerializationInfo serializationInfo)
        {
            var itemSerializer = _lazyItemSerializer.Value;
            serializationInfo = new BsonSerializationInfo(null, itemSerializer, itemSerializer.ValueType);
            return true;
        }

        /// <summary>
        /// Serializes a value.
        /// </summary>
        /// <param name="context">The serialization context.</param>
        /// <param name="args">The serialization args.</param>
        /// <param name="value">The object.</param>
        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, TValue value)
        {
            var bsonWriter = context.Writer;

            if (value == null)
            {
                bsonWriter.WriteNull();
            }
            else
            {
                var actualType = value.GetType();
                if (actualType == args.NominalType || args.SerializeAsNominalType)
                {
                    bsonWriter.WriteStartArray();
                    foreach (var item in EnumerateItemsInSerializationOrder(value))
                    {
                        _lazyItemSerializer.Value.Serialize(context, item);
                    }
                    bsonWriter.WriteEndArray();
                }
                else
                {
                    var serializer = new DiscriminatedWrapperSerializer<TValue>(_discriminatorConvention, this);
                    serializer.Serialize(context, value);
                }
            }
        }

        // protected methods
        /// <summary>
        /// Adds the item.
        /// </summary>
        /// <param name="accumulator">The accumulator.</param>
        /// <param name="item">The item.</param>
        protected abstract void AddItem(object accumulator, object item);

        /// <summary>
        /// Creates the accumulator.
        /// </summary>
        /// <returns>The accumulator.</returns>
        protected abstract object CreateAccumulator();

        /// <summary>
        /// Enumerates the items in serialization order.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The items.</returns>
        protected abstract IEnumerable EnumerateItemsInSerializationOrder(TValue value);

        /// <summary>
        /// Finalizes the result.
        /// </summary>
        /// <param name="accumulator">The accumulator.</param>
        /// <returns>The final result.</returns>
        protected abstract TValue FinalizeResult(object accumulator);
    }

    /// <summary>
    /// Represents a serializer for enumerable values.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <typeparam name="TItem">The type of the items.</typeparam>
    public abstract class EnumerableSerializerBase<TValue, TItem> : SerializerBase<TValue>, IBsonArraySerializer where TValue : class, IEnumerable<TItem>
    {
        // private fields
        private readonly IDiscriminatorConvention _discriminatorConvention = new ScalarDiscriminatorConvention("_t");
        private readonly Lazy<IBsonSerializer<TItem>> _lazyItemSerializer;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="EnumerableSerializerBase{TValue, TItem}"/> class.
        /// </summary>
        protected EnumerableSerializerBase()
            : this(BsonSerializer.SerializerRegistry)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumerableSerializerBase{TValue, TItem}"/> class.
        /// </summary>
        /// <param name="itemSerializer">The item serializer.</param>
        protected EnumerableSerializerBase(IBsonSerializer<TItem> itemSerializer)
        {
            if (itemSerializer == null)
            {
                throw new ArgumentNullException("itemSerializer");
            }

            _lazyItemSerializer = new Lazy<IBsonSerializer<TItem>>(() => itemSerializer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumerableSerializerBase{TValue, TItem}" /> class.
        /// </summary>
        /// <param name="serializerRegistry">The serializer registry.</param>
        protected EnumerableSerializerBase(IBsonSerializerRegistry serializerRegistry)
        {
            if (serializerRegistry == null)
            {
                throw new ArgumentNullException("serializerRegistry");
            }

            _lazyItemSerializer = new Lazy<IBsonSerializer<TItem>>(() => serializerRegistry.GetSerializer<TItem>());
        }

        // public properties
        /// <summary>
        /// Gets the item serializer.
        /// </summary>
        /// <value>
        /// The item serializer.
        /// </value>
        public IBsonSerializer<TItem> ItemSerializer
        {
            get { return _lazyItemSerializer.Value; }
        }

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

            var bsonType = bsonReader.GetCurrentBsonType();
            switch (bsonType)
            {
                case BsonType.Null:
                    bsonReader.ReadNull();
                    return null;

                case BsonType.Array:
                    bsonReader.ReadStartArray();
                    var accumulator = CreateAccumulator();
                    while (bsonReader.ReadBsonType() != BsonType.EndOfDocument)
                    {
                        var item = _lazyItemSerializer.Value.Deserialize(context);
                        AddItem(accumulator, item);
                    }
                    bsonReader.ReadEndArray();
                    return FinalizeResult(accumulator);

                case BsonType.Document:
                    var serializer = new DiscriminatedWrapperSerializer<TValue>(_discriminatorConvention, this);
                    if (serializer.IsPositionedAtDiscriminatedWrapper(context))
                    {
                        return (TValue)serializer.Deserialize(context);
                    }
                    else
                    {
                        goto default;
                    }

                default:
                    throw CreateCannotDeserializeFromBsonTypeException(bsonType);
            }

        }

        /// <summary>
        /// Tries to get the serialization info for the individual items of the array.
        /// </summary>
        /// <param name="serializationInfo">The serialization information.</param>
        /// <returns>
        /// The serialization info for the items.
        /// </returns>
        public bool TryGetItemSerializationInfo(out BsonSerializationInfo serializationInfo)
        {
            var serializer = _lazyItemSerializer.Value;
            serializationInfo = new BsonSerializationInfo(null, serializer, serializer.ValueType);
            return true;
        }

        /// <summary>
        /// Serializes a value.
        /// </summary>
        /// <param name="context">The serialization context.</param>
        /// <param name="args">The serialization args.</param>
        /// <param name="value">The object.</param>
        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, TValue value)
        {
            var bsonWriter = context.Writer;

            if (value == null)
            {
                bsonWriter.WriteNull();
            }
            else
            {
                var actualType = value.GetType();
                if (actualType == args.NominalType)
                {
                    bsonWriter.WriteStartArray();
                    foreach (var item in EnumerateItemsInSerializationOrder(value))
                    {
                        _lazyItemSerializer.Value.Serialize(context, item);
                    }
                    bsonWriter.WriteEndArray();
                }
                else
                {
                    var serializer = new DiscriminatedWrapperSerializer<TValue>(_discriminatorConvention, this);
                    serializer.Serialize(context, value);
                }
            }
        }

        // protected methods
        /// <summary>
        /// Adds the item.
        /// </summary>
        /// <param name="accumulator">The accumulator.</param>
        /// <param name="item">The item.</param>
        protected abstract void AddItem(object accumulator, TItem item);

        /// <summary>
        /// Creates the accumulator.
        /// </summary>
        /// <returns>The accumulator.</returns>
        protected abstract object CreateAccumulator();

        /// <summary>
        /// Enumerates the items in serialization order.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The items.</returns>
        protected abstract IEnumerable<TItem> EnumerateItemsInSerializationOrder(TValue value);

        /// <summary>
        /// Finalizes the result.
        /// </summary>
        /// <param name="accumulator">The accumulator.</param>
        /// <returns>The result.</returns>
        protected abstract TValue FinalizeResult(object accumulator);
    }
}


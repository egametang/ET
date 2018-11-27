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
    /// Represents a serializer for a <see cref="Tuple{T1}"/>.
    /// </summary>
    /// <typeparam name="T1">The type of item 1.</typeparam>
    public class TupleSerializer<T1> : SealedClassSerializerBase<Tuple<T1>>
    {
        // private fields
        private readonly Lazy<IBsonSerializer<T1>> _lazyItem1Serializer;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="TupleSerializer{T1}"/> class.
        /// </summary>
        public TupleSerializer()
            : this(BsonSerializer.SerializerRegistry)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TupleSerializer{T1}"/> class.
        /// </summary>
        /// <param name="item1Serializer">The Item1 serializer.</param>
        public TupleSerializer(
            IBsonSerializer<T1> item1Serializer)
        {
            if (item1Serializer == null) { throw new ArgumentNullException("item1Serializer"); }

            _lazyItem1Serializer = new Lazy<IBsonSerializer<T1>>(() => item1Serializer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TupleSerializer{T1}" /> class.
        /// </summary>
        /// <param name="serializerRegistry">The serializer registry.</param>
        public TupleSerializer(IBsonSerializerRegistry serializerRegistry)
        {
            if (serializerRegistry == null) { throw new ArgumentNullException("serializerRegistry"); }

            _lazyItem1Serializer = new Lazy<IBsonSerializer<T1>>(() => serializerRegistry.GetSerializer<T1>());
        }

        // public properties
        /// <summary>
        /// Gets the Item1 serializer.
        /// </summary>
        public IBsonSerializer<T1> Item1Serializer
        {
            get { return _lazyItem1Serializer.Value; }
        }

        // public methods
        /// <summary>
        /// Deserializes the value.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="args">The deserialization args.</param>
        /// <returns>A deserialized value.</returns>
        protected override Tuple<T1> DeserializeValue(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            context.Reader.ReadStartArray();
            var item1 = _lazyItem1Serializer.Value.Deserialize(context);
            context.Reader.ReadEndArray();

            return new Tuple<T1>(item1);
        }

        /// <summary>
        /// Serializes the value.
        /// </summary>
        /// <param name="context">The serialization context.</param>
        /// <param name="args">The serialization args.</param>
        /// <param name="value">The value.</param>
        protected override void SerializeValue(BsonSerializationContext context, BsonSerializationArgs args, Tuple<T1> value)
        {
            context.Writer.WriteStartArray();
            _lazyItem1Serializer.Value.Serialize(context, value.Item1);
            context.Writer.WriteEndArray();
        }
    }

    /// <summary>
    /// Represents a serializer for a <see cref="Tuple{T1, T2}"/>.
    /// </summary>
    /// <typeparam name="T1">The type of item 1.</typeparam>
    /// <typeparam name="T2">The type of item 2.</typeparam>
    public class TupleSerializer<T1, T2> : SealedClassSerializerBase<Tuple<T1, T2>>
    {
        // private fields
        private readonly Lazy<IBsonSerializer<T1>> _lazyItem1Serializer;
        private readonly Lazy<IBsonSerializer<T2>> _lazyItem2Serializer;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="TupleSerializer{T1, T2}"/> class.
        /// </summary>
        public TupleSerializer()
            : this(BsonSerializer.SerializerRegistry)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TupleSerializer{T1, T2}"/> class.
        /// </summary>
        /// <param name="item1Serializer">The Item1 serializer.</param>
        /// <param name="item2Serializer">The Item2 serializer.</param>
        public TupleSerializer(
            IBsonSerializer<T1> item1Serializer,
            IBsonSerializer<T2> item2Serializer)
        {
            if (item1Serializer == null) { throw new ArgumentNullException("item1Serializer"); }
            if (item2Serializer == null) { throw new ArgumentNullException("item2Serializer"); }

            _lazyItem1Serializer = new Lazy<IBsonSerializer<T1>>(() => item1Serializer);
            _lazyItem2Serializer = new Lazy<IBsonSerializer<T2>>(() => item2Serializer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TupleSerializer{T1, T2}" /> class.
        /// </summary>
        /// <param name="serializerRegistry">The serializer registry.</param>
        public TupleSerializer(IBsonSerializerRegistry serializerRegistry)
        {
            if (serializerRegistry == null) { throw new ArgumentNullException("serializerRegistry"); }

            _lazyItem1Serializer = new Lazy<IBsonSerializer<T1>>(() => serializerRegistry.GetSerializer<T1>());
            _lazyItem2Serializer = new Lazy<IBsonSerializer<T2>>(() => serializerRegistry.GetSerializer<T2>());
        }

        // public properties
        /// <summary>
        /// Gets the Item1 serializer.
        /// </summary>
        public IBsonSerializer<T1> Item1Serializer
        {
            get { return _lazyItem1Serializer.Value; }
        }

        /// <summary>
        /// Gets the Item2 serializer.
        /// </summary>
        public IBsonSerializer<T2> Item2Serializer
        {
            get { return _lazyItem2Serializer.Value; }
        }

        // public methods
        /// <summary>
        /// Deserializes the value.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="args">The deserialization args.</param>
        /// <returns>A deserialized value.</returns>
        protected override Tuple<T1, T2> DeserializeValue(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            context.Reader.ReadStartArray();
            var item1 = _lazyItem1Serializer.Value.Deserialize(context);
            var item2 = _lazyItem2Serializer.Value.Deserialize(context);
            context.Reader.ReadEndArray();

            return new Tuple<T1, T2>(item1, item2);
        }

        /// <summary>
        /// Serializes the value.
        /// </summary>
        /// <param name="context">The serialization context.</param>
        /// <param name="args">The serialization args.</param>
        /// <param name="value">The value.</param>
        protected override void SerializeValue(BsonSerializationContext context, BsonSerializationArgs args, Tuple<T1, T2> value)
        {
            context.Writer.WriteStartArray();
            _lazyItem1Serializer.Value.Serialize(context, value.Item1);
            _lazyItem2Serializer.Value.Serialize(context, value.Item2);
            context.Writer.WriteEndArray();
        }
    }

    /// <summary>
    /// Represents a serializer for a <see cref="Tuple{T1, T2, T3}"/>.
    /// </summary>
    /// <typeparam name="T1">The type of item 1.</typeparam>
    /// <typeparam name="T2">The type of item 2.</typeparam>
    /// <typeparam name="T3">The type of item 3.</typeparam>
    public class TupleSerializer<T1, T2, T3> : SealedClassSerializerBase<Tuple<T1, T2, T3>>
    {
        // private fields
        private readonly Lazy<IBsonSerializer<T1>> _lazyItem1Serializer;
        private readonly Lazy<IBsonSerializer<T2>> _lazyItem2Serializer;
        private readonly Lazy<IBsonSerializer<T3>> _lazyItem3Serializer;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="TupleSerializer{T1, T2, T3}"/> class.
        /// </summary>
        public TupleSerializer()
            : this(BsonSerializer.SerializerRegistry)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TupleSerializer{T1, T2, T3}"/> class.
        /// </summary>
        /// <param name="item1Serializer">The Item1 serializer.</param>
        /// <param name="item2Serializer">The Item2 serializer.</param>
        /// <param name="item3Serializer">The Item3 serializer.</param>
        public TupleSerializer(
            IBsonSerializer<T1> item1Serializer,
            IBsonSerializer<T2> item2Serializer,
            IBsonSerializer<T3> item3Serializer)
        {
            if (item1Serializer == null) { throw new ArgumentNullException("item1Serializer"); }
            if (item2Serializer == null) { throw new ArgumentNullException("item2Serializer"); }
            if (item3Serializer == null) { throw new ArgumentNullException("item3Serializer"); }

            _lazyItem1Serializer = new Lazy<IBsonSerializer<T1>>(() => item1Serializer);
            _lazyItem2Serializer = new Lazy<IBsonSerializer<T2>>(() => item2Serializer);
            _lazyItem3Serializer = new Lazy<IBsonSerializer<T3>>(() => item3Serializer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TupleSerializer{T1, T2, T3}" /> class.
        /// </summary>
        /// <param name="serializerRegistry">The serializer registry.</param>
        public TupleSerializer(IBsonSerializerRegistry serializerRegistry)
        {
            if (serializerRegistry == null) { throw new ArgumentNullException("serializerRegistry"); }

            _lazyItem1Serializer = new Lazy<IBsonSerializer<T1>>(() => serializerRegistry.GetSerializer<T1>());
            _lazyItem2Serializer = new Lazy<IBsonSerializer<T2>>(() => serializerRegistry.GetSerializer<T2>());
            _lazyItem3Serializer = new Lazy<IBsonSerializer<T3>>(() => serializerRegistry.GetSerializer<T3>());
        }

        // public properties
        /// <summary>
        /// Gets the Item1 serializer.
        /// </summary>
        public IBsonSerializer<T1> Item1Serializer
        {
            get { return _lazyItem1Serializer.Value; }
        }

        /// <summary>
        /// Gets the Item2 serializer.
        /// </summary>
        public IBsonSerializer<T2> Item2Serializer
        {
            get { return _lazyItem2Serializer.Value; }
        }

        /// <summary>
        /// Gets the Item3 serializer.
        /// </summary>
        public IBsonSerializer<T3> Item3Serializer
        {
            get { return _lazyItem3Serializer.Value; }
        }

        // public methods
        /// <summary>
        /// Deserializes the value.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="args">The deserialization args.</param>
        /// <returns>A deserialized value.</returns>
        protected override Tuple<T1, T2, T3> DeserializeValue(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            context.Reader.ReadStartArray();
            var item1 = _lazyItem1Serializer.Value.Deserialize(context);
            var item2 = _lazyItem2Serializer.Value.Deserialize(context);
            var item3 = _lazyItem3Serializer.Value.Deserialize(context);
            context.Reader.ReadEndArray();

            return new Tuple<T1, T2, T3>(item1, item2, item3);
        }

        /// <summary>
        /// Serializes the value.
        /// </summary>
        /// <param name="context">The serialization context.</param>
        /// <param name="args">The serialization args.</param>
        /// <param name="value">The value.</param>
        protected override void SerializeValue(BsonSerializationContext context, BsonSerializationArgs args, Tuple<T1, T2, T3> value)
        {
            context.Writer.WriteStartArray();
            _lazyItem1Serializer.Value.Serialize(context, value.Item1);
            _lazyItem2Serializer.Value.Serialize(context, value.Item2);
            _lazyItem3Serializer.Value.Serialize(context, value.Item3);
            context.Writer.WriteEndArray();
        }
    }

    /// <summary>
    /// Represents a serializer for a <see cref="Tuple{T1, T2, T3, T4}"/>.
    /// </summary>
    /// <typeparam name="T1">The type of item 1.</typeparam>
    /// <typeparam name="T2">The type of item 2.</typeparam>
    /// <typeparam name="T3">The type of item 3.</typeparam>
    /// <typeparam name="T4">The type of item 4.</typeparam>
    public class TupleSerializer<T1, T2, T3, T4> : SealedClassSerializerBase<Tuple<T1, T2, T3, T4>>
    {
        // private fields
        private readonly Lazy<IBsonSerializer<T1>> _lazyItem1Serializer;
        private readonly Lazy<IBsonSerializer<T2>> _lazyItem2Serializer;
        private readonly Lazy<IBsonSerializer<T3>> _lazyItem3Serializer;
        private readonly Lazy<IBsonSerializer<T4>> _lazyItem4Serializer;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="TupleSerializer{T1, T2, T3, T4}"/> class.
        /// </summary>
        public TupleSerializer()
            : this(BsonSerializer.SerializerRegistry)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TupleSerializer{T1, T2, T3, T4}"/> class.
        /// </summary>
        /// <param name="item1Serializer">The Item1 serializer.</param>
        /// <param name="item2Serializer">The Item2 serializer.</param>
        /// <param name="item3Serializer">The Item3 serializer.</param>
        /// <param name="item4Serializer">The Item4 serializer.</param>
        public TupleSerializer(
            IBsonSerializer<T1> item1Serializer,
            IBsonSerializer<T2> item2Serializer,
            IBsonSerializer<T3> item3Serializer,
            IBsonSerializer<T4> item4Serializer)
        {
            if (item1Serializer == null) { throw new ArgumentNullException("item1Serializer"); }
            if (item2Serializer == null) { throw new ArgumentNullException("item2Serializer"); }
            if (item3Serializer == null) { throw new ArgumentNullException("item3Serializer"); }
            if (item4Serializer == null) { throw new ArgumentNullException("item4Serializer"); }

            _lazyItem1Serializer = new Lazy<IBsonSerializer<T1>>(() => item1Serializer);
            _lazyItem2Serializer = new Lazy<IBsonSerializer<T2>>(() => item2Serializer);
            _lazyItem3Serializer = new Lazy<IBsonSerializer<T3>>(() => item3Serializer);
            _lazyItem4Serializer = new Lazy<IBsonSerializer<T4>>(() => item4Serializer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TupleSerializer{T1, T2, T3, T4}" /> class.
        /// </summary>
        /// <param name="serializerRegistry">The serializer registry.</param>
        public TupleSerializer(IBsonSerializerRegistry serializerRegistry)
        {
            if (serializerRegistry == null) { throw new ArgumentNullException("serializerRegistry"); }

            _lazyItem1Serializer = new Lazy<IBsonSerializer<T1>>(() => serializerRegistry.GetSerializer<T1>());
            _lazyItem2Serializer = new Lazy<IBsonSerializer<T2>>(() => serializerRegistry.GetSerializer<T2>());
            _lazyItem3Serializer = new Lazy<IBsonSerializer<T3>>(() => serializerRegistry.GetSerializer<T3>());
            _lazyItem4Serializer = new Lazy<IBsonSerializer<T4>>(() => serializerRegistry.GetSerializer<T4>());
        }

        // public properties
        /// <summary>
        /// Gets the Item1 serializer.
        /// </summary>
        public IBsonSerializer<T1> Item1Serializer
        {
            get { return _lazyItem1Serializer.Value; }
        }

        /// <summary>
        /// Gets the Item2 serializer.
        /// </summary>
        public IBsonSerializer<T2> Item2Serializer
        {
            get { return _lazyItem2Serializer.Value; }
        }

        /// <summary>
        /// Gets the Item3 serializer.
        /// </summary>
        public IBsonSerializer<T3> Item3Serializer
        {
            get { return _lazyItem3Serializer.Value; }
        }

        /// <summary>
        /// Gets the Item4 serializer.
        /// </summary>
        public IBsonSerializer<T4> Item4Serializer
        {
            get { return _lazyItem4Serializer.Value; }
        }

        // public methods
        /// <summary>
        /// Deserializes the value.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="args">The deserialization args.</param>
        /// <returns>A deserialized value.</returns>
        protected override Tuple<T1, T2, T3, T4> DeserializeValue(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            context.Reader.ReadStartArray();
            var item1 = _lazyItem1Serializer.Value.Deserialize(context);
            var item2 = _lazyItem2Serializer.Value.Deserialize(context);
            var item3 = _lazyItem3Serializer.Value.Deserialize(context);
            var item4 = _lazyItem4Serializer.Value.Deserialize(context);
            context.Reader.ReadEndArray();

            return new Tuple<T1, T2, T3, T4>(item1, item2, item3, item4);
        }

        /// <summary>
        /// Serializes the value.
        /// </summary>
        /// <param name="context">The serialization context.</param>
        /// <param name="args">The serialization args.</param>
        /// <param name="value">The value.</param>
        protected override void SerializeValue(BsonSerializationContext context, BsonSerializationArgs args, Tuple<T1, T2, T3, T4> value)
        {
            context.Writer.WriteStartArray();
            _lazyItem1Serializer.Value.Serialize(context, value.Item1);
            _lazyItem2Serializer.Value.Serialize(context, value.Item2);
            _lazyItem3Serializer.Value.Serialize(context, value.Item3);
            _lazyItem4Serializer.Value.Serialize(context, value.Item4);
            context.Writer.WriteEndArray();
        }
    }

    /// <summary>
    /// Represents a serializer for a <see cref="Tuple{T1, T2, T3, T4, T5}"/>.
    /// </summary>
    /// <typeparam name="T1">The type of item 1.</typeparam>
    /// <typeparam name="T2">The type of item 2.</typeparam>
    /// <typeparam name="T3">The type of item 3.</typeparam>
    /// <typeparam name="T4">The type of item 4.</typeparam>
    /// <typeparam name="T5">The type of item 5.</typeparam>
    public class TupleSerializer<T1, T2, T3, T4, T5> : SealedClassSerializerBase<Tuple<T1, T2, T3, T4, T5>>
    {
        // private fields
        private readonly Lazy<IBsonSerializer<T1>> _lazyItem1Serializer;
        private readonly Lazy<IBsonSerializer<T2>> _lazyItem2Serializer;
        private readonly Lazy<IBsonSerializer<T3>> _lazyItem3Serializer;
        private readonly Lazy<IBsonSerializer<T4>> _lazyItem4Serializer;
        private readonly Lazy<IBsonSerializer<T5>> _lazyItem5Serializer;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="TupleSerializer{T1, T2, T3, T4, T5}"/> class.
        /// </summary>
        public TupleSerializer()
            : this(BsonSerializer.SerializerRegistry)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TupleSerializer{T1, T2, T3, T4, T5}"/> class.
        /// </summary>
        /// <param name="item1Serializer">The Item1 serializer.</param>
        /// <param name="item2Serializer">The Item2 serializer.</param>
        /// <param name="item3Serializer">The Item3 serializer.</param>
        /// <param name="item4Serializer">The Item4 serializer.</param>
        /// <param name="item5Serializer">The Item5 serializer.</param>
        public TupleSerializer(
            IBsonSerializer<T1> item1Serializer,
            IBsonSerializer<T2> item2Serializer,
            IBsonSerializer<T3> item3Serializer,
            IBsonSerializer<T4> item4Serializer,
            IBsonSerializer<T5> item5Serializer)
        {
            if (item1Serializer == null) { throw new ArgumentNullException("item1Serializer"); }
            if (item2Serializer == null) { throw new ArgumentNullException("item2Serializer"); }
            if (item3Serializer == null) { throw new ArgumentNullException("item3Serializer"); }
            if (item4Serializer == null) { throw new ArgumentNullException("item4Serializer"); }
            if (item5Serializer == null) { throw new ArgumentNullException("item5Serializer"); }

            _lazyItem1Serializer = new Lazy<IBsonSerializer<T1>>(() => item1Serializer);
            _lazyItem2Serializer = new Lazy<IBsonSerializer<T2>>(() => item2Serializer);
            _lazyItem3Serializer = new Lazy<IBsonSerializer<T3>>(() => item3Serializer);
            _lazyItem4Serializer = new Lazy<IBsonSerializer<T4>>(() => item4Serializer);
            _lazyItem5Serializer = new Lazy<IBsonSerializer<T5>>(() => item5Serializer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TupleSerializer{T1, T2, T3, T4, T5}" /> class.
        /// </summary>
        /// <param name="serializerRegistry">The serializer registry.</param>
        public TupleSerializer(IBsonSerializerRegistry serializerRegistry)
        {
            if (serializerRegistry == null) { throw new ArgumentNullException("serializerRegistry"); }

            _lazyItem1Serializer = new Lazy<IBsonSerializer<T1>>(() => serializerRegistry.GetSerializer<T1>());
            _lazyItem2Serializer = new Lazy<IBsonSerializer<T2>>(() => serializerRegistry.GetSerializer<T2>());
            _lazyItem3Serializer = new Lazy<IBsonSerializer<T3>>(() => serializerRegistry.GetSerializer<T3>());
            _lazyItem4Serializer = new Lazy<IBsonSerializer<T4>>(() => serializerRegistry.GetSerializer<T4>());
            _lazyItem5Serializer = new Lazy<IBsonSerializer<T5>>(() => serializerRegistry.GetSerializer<T5>());
        }

        // public properties
        /// <summary>
        /// Gets the Item1 serializer.
        /// </summary>
        public IBsonSerializer<T1> Item1Serializer
        {
            get { return _lazyItem1Serializer.Value; }
        }

        /// <summary>
        /// Gets the Item2 serializer.
        /// </summary>
        public IBsonSerializer<T2> Item2Serializer
        {
            get { return _lazyItem2Serializer.Value; }
        }

        /// <summary>
        /// Gets the Item3 serializer.
        /// </summary>
        public IBsonSerializer<T3> Item3Serializer
        {
            get { return _lazyItem3Serializer.Value; }
        }

        /// <summary>
        /// Gets the Item4 serializer.
        /// </summary>
        public IBsonSerializer<T4> Item4Serializer
        {
            get { return _lazyItem4Serializer.Value; }
        }

        /// <summary>
        /// Gets the Item5 serializer.
        /// </summary>
        public IBsonSerializer<T5> Item5Serializer
        {
            get { return _lazyItem5Serializer.Value; }
        }

        // public methods
        /// <summary>
        /// Deserializes the value.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="args">The deserialization args.</param>
        /// <returns>A deserialized value.</returns>
        protected override Tuple<T1, T2, T3, T4, T5> DeserializeValue(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            context.Reader.ReadStartArray();
            var item1 = _lazyItem1Serializer.Value.Deserialize(context);
            var item2 = _lazyItem2Serializer.Value.Deserialize(context);
            var item3 = _lazyItem3Serializer.Value.Deserialize(context);
            var item4 = _lazyItem4Serializer.Value.Deserialize(context);
            var item5 = _lazyItem5Serializer.Value.Deserialize(context);
            context.Reader.ReadEndArray();

            return new Tuple<T1, T2, T3, T4, T5>(item1, item2, item3, item4, item5);
        }

        /// <summary>
        /// Serializes the value.
        /// </summary>
        /// <param name="context">The serialization context.</param>
        /// <param name="args">The serialization args.</param>
        /// <param name="value">The value.</param>
        protected override void SerializeValue(BsonSerializationContext context, BsonSerializationArgs args, Tuple<T1, T2, T3, T4, T5> value)
        {
            context.Writer.WriteStartArray();
            _lazyItem1Serializer.Value.Serialize(context, value.Item1);
            _lazyItem2Serializer.Value.Serialize(context, value.Item2);
            _lazyItem3Serializer.Value.Serialize(context, value.Item3);
            _lazyItem4Serializer.Value.Serialize(context, value.Item4);
            _lazyItem5Serializer.Value.Serialize(context, value.Item5);
            context.Writer.WriteEndArray();
        }
    }

    /// <summary>
    /// Represents a serializer for a <see cref="Tuple{T1, T2, T3, T4, T5, T6}"/>.
    /// </summary>
    /// <typeparam name="T1">The type of item 1.</typeparam>
    /// <typeparam name="T2">The type of item 2.</typeparam>
    /// <typeparam name="T3">The type of item 3.</typeparam>
    /// <typeparam name="T4">The type of item 4.</typeparam>
    /// <typeparam name="T5">The type of item 5.</typeparam>
    /// <typeparam name="T6">The type of item 6.</typeparam>
    public class TupleSerializer<T1, T2, T3, T4, T5, T6> : SealedClassSerializerBase<Tuple<T1, T2, T3, T4, T5, T6>>
    {
        // private fields
        private readonly Lazy<IBsonSerializer<T1>> _lazyItem1Serializer;
        private readonly Lazy<IBsonSerializer<T2>> _lazyItem2Serializer;
        private readonly Lazy<IBsonSerializer<T3>> _lazyItem3Serializer;
        private readonly Lazy<IBsonSerializer<T4>> _lazyItem4Serializer;
        private readonly Lazy<IBsonSerializer<T5>> _lazyItem5Serializer;
        private readonly Lazy<IBsonSerializer<T6>> _lazyItem6Serializer;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="TupleSerializer{T1, T2, T3, T4, T5, T6}"/> class.
        /// </summary>
        public TupleSerializer()
            : this(BsonSerializer.SerializerRegistry)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TupleSerializer{T1, T2, T3, T4, T5, T6}"/> class.
        /// </summary>
        /// <param name="item1Serializer">The Item1 serializer.</param>
        /// <param name="item2Serializer">The Item2 serializer.</param>
        /// <param name="item3Serializer">The Item3 serializer.</param>
        /// <param name="item4Serializer">The Item4 serializer.</param>
        /// <param name="item5Serializer">The Item5 serializer.</param>
        /// <param name="item6Serializer">The Item6 serializer.</param>
        public TupleSerializer(
            IBsonSerializer<T1> item1Serializer,
            IBsonSerializer<T2> item2Serializer,
            IBsonSerializer<T3> item3Serializer,
            IBsonSerializer<T4> item4Serializer,
            IBsonSerializer<T5> item5Serializer,
            IBsonSerializer<T6> item6Serializer)
        {
            if (item1Serializer == null) { throw new ArgumentNullException("item1Serializer"); }
            if (item2Serializer == null) { throw new ArgumentNullException("item2Serializer"); }
            if (item3Serializer == null) { throw new ArgumentNullException("item3Serializer"); }
            if (item4Serializer == null) { throw new ArgumentNullException("item4Serializer"); }
            if (item5Serializer == null) { throw new ArgumentNullException("item5Serializer"); }
            if (item6Serializer == null) { throw new ArgumentNullException("item6Serializer"); }

            _lazyItem1Serializer = new Lazy<IBsonSerializer<T1>>(() => item1Serializer);
            _lazyItem2Serializer = new Lazy<IBsonSerializer<T2>>(() => item2Serializer);
            _lazyItem3Serializer = new Lazy<IBsonSerializer<T3>>(() => item3Serializer);
            _lazyItem4Serializer = new Lazy<IBsonSerializer<T4>>(() => item4Serializer);
            _lazyItem5Serializer = new Lazy<IBsonSerializer<T5>>(() => item5Serializer);
            _lazyItem6Serializer = new Lazy<IBsonSerializer<T6>>(() => item6Serializer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TupleSerializer{T1, T2, T3, T4, T5, T6}" /> class.
        /// </summary>
        /// <param name="serializerRegistry">The serializer registry.</param>
        public TupleSerializer(IBsonSerializerRegistry serializerRegistry)
        {
            if (serializerRegistry == null) { throw new ArgumentNullException("serializerRegistry"); }

            _lazyItem1Serializer = new Lazy<IBsonSerializer<T1>>(() => serializerRegistry.GetSerializer<T1>());
            _lazyItem2Serializer = new Lazy<IBsonSerializer<T2>>(() => serializerRegistry.GetSerializer<T2>());
            _lazyItem3Serializer = new Lazy<IBsonSerializer<T3>>(() => serializerRegistry.GetSerializer<T3>());
            _lazyItem4Serializer = new Lazy<IBsonSerializer<T4>>(() => serializerRegistry.GetSerializer<T4>());
            _lazyItem5Serializer = new Lazy<IBsonSerializer<T5>>(() => serializerRegistry.GetSerializer<T5>());
            _lazyItem6Serializer = new Lazy<IBsonSerializer<T6>>(() => serializerRegistry.GetSerializer<T6>());
        }

        // public properties
        /// <summary>
        /// Gets the Item1 serializer.
        /// </summary>
        public IBsonSerializer<T1> Item1Serializer
        {
            get { return _lazyItem1Serializer.Value; }
        }

        /// <summary>
        /// Gets the Item2 serializer.
        /// </summary>
        public IBsonSerializer<T2> Item2Serializer
        {
            get { return _lazyItem2Serializer.Value; }
        }

        /// <summary>
        /// Gets the Item3 serializer.
        /// </summary>
        public IBsonSerializer<T3> Item3Serializer
        {
            get { return _lazyItem3Serializer.Value; }
        }

        /// <summary>
        /// Gets the Item4 serializer.
        /// </summary>
        public IBsonSerializer<T4> Item4Serializer
        {
            get { return _lazyItem4Serializer.Value; }
        }

        /// <summary>
        /// Gets the Item5 serializer.
        /// </summary>
        public IBsonSerializer<T5> Item5Serializer
        {
            get { return _lazyItem5Serializer.Value; }
        }

        /// <summary>
        /// Gets the Item6 serializer.
        /// </summary>
        public IBsonSerializer<T6> Item6Serializer
        {
            get { return _lazyItem6Serializer.Value; }
        }

        // public methods
        /// <summary>
        /// Deserializes the value.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="args">The deserialization args.</param>
        /// <returns>A deserialized value.</returns>
        protected override Tuple<T1, T2, T3, T4, T5, T6> DeserializeValue(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            context.Reader.ReadStartArray();
            var item1 = _lazyItem1Serializer.Value.Deserialize(context);
            var item2 = _lazyItem2Serializer.Value.Deserialize(context);
            var item3 = _lazyItem3Serializer.Value.Deserialize(context);
            var item4 = _lazyItem4Serializer.Value.Deserialize(context);
            var item5 = _lazyItem5Serializer.Value.Deserialize(context);
            var item6 = _lazyItem6Serializer.Value.Deserialize(context);
            context.Reader.ReadEndArray();

            return new Tuple<T1, T2, T3, T4, T5, T6>(item1, item2, item3, item4, item5, item6);
        }

        /// <summary>
        /// Serializes the value.
        /// </summary>
        /// <param name="context">The serialization context.</param>
        /// <param name="args">The serialization args.</param>
        /// <param name="value">The value.</param>
        protected override void SerializeValue(BsonSerializationContext context, BsonSerializationArgs args, Tuple<T1, T2, T3, T4, T5, T6> value)
        {
            context.Writer.WriteStartArray();
            _lazyItem1Serializer.Value.Serialize(context, value.Item1);
            _lazyItem2Serializer.Value.Serialize(context, value.Item2);
            _lazyItem3Serializer.Value.Serialize(context, value.Item3);
            _lazyItem4Serializer.Value.Serialize(context, value.Item4);
            _lazyItem5Serializer.Value.Serialize(context, value.Item5);
            _lazyItem6Serializer.Value.Serialize(context, value.Item6);
            context.Writer.WriteEndArray();
        }
    }

    /// <summary>
    /// Represents a serializer for a <see cref="Tuple{T1, T2, T3, T4, T5, T6, T7}"/>.
    /// </summary>
    /// <typeparam name="T1">The type of item 1.</typeparam>
    /// <typeparam name="T2">The type of item 2.</typeparam>
    /// <typeparam name="T3">The type of item 3.</typeparam>
    /// <typeparam name="T4">The type of item 4.</typeparam>
    /// <typeparam name="T5">The type of item 5.</typeparam>
    /// <typeparam name="T6">The type of item 6.</typeparam>
    /// <typeparam name="T7">The type of item 7.</typeparam>
    public class TupleSerializer<T1, T2, T3, T4, T5, T6, T7> : SealedClassSerializerBase<Tuple<T1, T2, T3, T4, T5, T6, T7>>
    {
        // private fields
        private readonly Lazy<IBsonSerializer<T1>> _lazyItem1Serializer;
        private readonly Lazy<IBsonSerializer<T2>> _lazyItem2Serializer;
        private readonly Lazy<IBsonSerializer<T3>> _lazyItem3Serializer;
        private readonly Lazy<IBsonSerializer<T4>> _lazyItem4Serializer;
        private readonly Lazy<IBsonSerializer<T5>> _lazyItem5Serializer;
        private readonly Lazy<IBsonSerializer<T6>> _lazyItem6Serializer;
        private readonly Lazy<IBsonSerializer<T7>> _lazyItem7Serializer;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="TupleSerializer{T1, T2, T3, T4, T5, T6, T7}"/> class.
        /// </summary>
        public TupleSerializer()
            : this(BsonSerializer.SerializerRegistry)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TupleSerializer{T1, T2, T3, T4, T5, T6, T7}"/> class.
        /// </summary>
        /// <param name="item1Serializer">The Item1 serializer.</param>
        /// <param name="item2Serializer">The Item2 serializer.</param>
        /// <param name="item3Serializer">The Item3 serializer.</param>
        /// <param name="item4Serializer">The Item4 serializer.</param>
        /// <param name="item5Serializer">The Item5 serializer.</param>
        /// <param name="item6Serializer">The Item6 serializer.</param>
        /// <param name="item7Serializer">The Item7 serializer.</param>
        public TupleSerializer(
            IBsonSerializer<T1> item1Serializer,
            IBsonSerializer<T2> item2Serializer,
            IBsonSerializer<T3> item3Serializer,
            IBsonSerializer<T4> item4Serializer,
            IBsonSerializer<T5> item5Serializer,
            IBsonSerializer<T6> item6Serializer,
            IBsonSerializer<T7> item7Serializer)
        {
            if (item1Serializer == null) { throw new ArgumentNullException("item1Serializer"); }
            if (item2Serializer == null) { throw new ArgumentNullException("item2Serializer"); }
            if (item3Serializer == null) { throw new ArgumentNullException("item3Serializer"); }
            if (item4Serializer == null) { throw new ArgumentNullException("item4Serializer"); }
            if (item5Serializer == null) { throw new ArgumentNullException("item5Serializer"); }
            if (item6Serializer == null) { throw new ArgumentNullException("item6Serializer"); }
            if (item7Serializer == null) { throw new ArgumentNullException("item7Serializer"); }

            _lazyItem1Serializer = new Lazy<IBsonSerializer<T1>>(() => item1Serializer);
            _lazyItem2Serializer = new Lazy<IBsonSerializer<T2>>(() => item2Serializer);
            _lazyItem3Serializer = new Lazy<IBsonSerializer<T3>>(() => item3Serializer);
            _lazyItem4Serializer = new Lazy<IBsonSerializer<T4>>(() => item4Serializer);
            _lazyItem5Serializer = new Lazy<IBsonSerializer<T5>>(() => item5Serializer);
            _lazyItem6Serializer = new Lazy<IBsonSerializer<T6>>(() => item6Serializer);
            _lazyItem7Serializer = new Lazy<IBsonSerializer<T7>>(() => item7Serializer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TupleSerializer{T1, T2, T3, T4, T5, T6, T7}" /> class.
        /// </summary>
        /// <param name="serializerRegistry">The serializer registry.</param>
        public TupleSerializer(IBsonSerializerRegistry serializerRegistry)
        {
            if (serializerRegistry == null) { throw new ArgumentNullException("serializerRegistry"); }

            _lazyItem1Serializer = new Lazy<IBsonSerializer<T1>>(() => serializerRegistry.GetSerializer<T1>());
            _lazyItem2Serializer = new Lazy<IBsonSerializer<T2>>(() => serializerRegistry.GetSerializer<T2>());
            _lazyItem3Serializer = new Lazy<IBsonSerializer<T3>>(() => serializerRegistry.GetSerializer<T3>());
            _lazyItem4Serializer = new Lazy<IBsonSerializer<T4>>(() => serializerRegistry.GetSerializer<T4>());
            _lazyItem5Serializer = new Lazy<IBsonSerializer<T5>>(() => serializerRegistry.GetSerializer<T5>());
            _lazyItem6Serializer = new Lazy<IBsonSerializer<T6>>(() => serializerRegistry.GetSerializer<T6>());
            _lazyItem7Serializer = new Lazy<IBsonSerializer<T7>>(() => serializerRegistry.GetSerializer<T7>());
        }

        // public properties
        /// <summary>
        /// Gets the Item1 serializer.
        /// </summary>
        public IBsonSerializer<T1> Item1Serializer
        {
            get { return _lazyItem1Serializer.Value; }
        }

        /// <summary>
        /// Gets the Item2 serializer.
        /// </summary>
        public IBsonSerializer<T2> Item2Serializer
        {
            get { return _lazyItem2Serializer.Value; }
        }

        /// <summary>
        /// Gets the Item3 serializer.
        /// </summary>
        public IBsonSerializer<T3> Item3Serializer
        {
            get { return _lazyItem3Serializer.Value; }
        }

        /// <summary>
        /// Gets the Item4 serializer.
        /// </summary>
        public IBsonSerializer<T4> Item4Serializer
        {
            get { return _lazyItem4Serializer.Value; }
        }

        /// <summary>
        /// Gets the Item5 serializer.
        /// </summary>
        public IBsonSerializer<T5> Item5Serializer
        {
            get { return _lazyItem5Serializer.Value; }
        }

        /// <summary>
        /// Gets the Item6 serializer.
        /// </summary>
        public IBsonSerializer<T6> Item6Serializer
        {
            get { return _lazyItem6Serializer.Value; }
        }

        /// <summary>
        /// Gets the Item7 serializer.
        /// </summary>
        public IBsonSerializer<T7> Item7Serializer
        {
            get { return _lazyItem7Serializer.Value; }
        }

        // public methods
        /// <summary>
        /// Deserializes the value.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="args">The deserialization args.</param>
        /// <returns>A deserialized value.</returns>
        protected override Tuple<T1, T2, T3, T4, T5, T6, T7> DeserializeValue(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            context.Reader.ReadStartArray();
            var item1 = _lazyItem1Serializer.Value.Deserialize(context);
            var item2 = _lazyItem2Serializer.Value.Deserialize(context);
            var item3 = _lazyItem3Serializer.Value.Deserialize(context);
            var item4 = _lazyItem4Serializer.Value.Deserialize(context);
            var item5 = _lazyItem5Serializer.Value.Deserialize(context);
            var item6 = _lazyItem6Serializer.Value.Deserialize(context);
            var item7 = _lazyItem7Serializer.Value.Deserialize(context);
            context.Reader.ReadEndArray();

            return new Tuple<T1, T2, T3, T4, T5, T6, T7>(item1, item2, item3, item4, item5, item6, item7);
        }

        /// <summary>
        /// Serializes the value.
        /// </summary>
        /// <param name="context">The serialization context.</param>
        /// <param name="args">The serialization args.</param>
        /// <param name="value">The value.</param>
        protected override void SerializeValue(BsonSerializationContext context, BsonSerializationArgs args, Tuple<T1, T2, T3, T4, T5, T6, T7> value)
        {
            context.Writer.WriteStartArray();
            _lazyItem1Serializer.Value.Serialize(context, value.Item1);
            _lazyItem2Serializer.Value.Serialize(context, value.Item2);
            _lazyItem3Serializer.Value.Serialize(context, value.Item3);
            _lazyItem4Serializer.Value.Serialize(context, value.Item4);
            _lazyItem5Serializer.Value.Serialize(context, value.Item5);
            _lazyItem6Serializer.Value.Serialize(context, value.Item6);
            _lazyItem7Serializer.Value.Serialize(context, value.Item7);
            context.Writer.WriteEndArray();
        }
    }

    /// <summary>
    /// Represents a serializer for a <see cref="Tuple{T1, T2, T3, T4, T5, T6, T7, TRest}"/>.
    /// </summary>
    /// <typeparam name="T1">The type of item 1.</typeparam>
    /// <typeparam name="T2">The type of item 2.</typeparam>
    /// <typeparam name="T3">The type of item 3.</typeparam>
    /// <typeparam name="T4">The type of item 4.</typeparam>
    /// <typeparam name="T5">The type of item 5.</typeparam>
    /// <typeparam name="T6">The type of item 6.</typeparam>
    /// <typeparam name="T7">The type of item 7.</typeparam>
    /// <typeparam name="TRest">The type of the rest item.</typeparam>
    public class TupleSerializer<T1, T2, T3, T4, T5, T6, T7, TRest> : SealedClassSerializerBase<Tuple<T1, T2, T3, T4, T5, T6, T7, TRest>>
    {
        // private fields
        private readonly Lazy<IBsonSerializer<T1>> _lazyItem1Serializer;
        private readonly Lazy<IBsonSerializer<T2>> _lazyItem2Serializer;
        private readonly Lazy<IBsonSerializer<T3>> _lazyItem3Serializer;
        private readonly Lazy<IBsonSerializer<T4>> _lazyItem4Serializer;
        private readonly Lazy<IBsonSerializer<T5>> _lazyItem5Serializer;
        private readonly Lazy<IBsonSerializer<T6>> _lazyItem6Serializer;
        private readonly Lazy<IBsonSerializer<T7>> _lazyItem7Serializer;
        private readonly Lazy<IBsonSerializer<TRest>> _lazyRestSerializer;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="TupleSerializer{T1, T2, T3, T4, T5, T6, T7, TRest}"/> class.
        /// </summary>
        public TupleSerializer()
            : this(BsonSerializer.SerializerRegistry)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TupleSerializer{T1, T2, T3, T4, T5, T6, T7, TRest}"/> class.
        /// </summary>
        /// <param name="item1Serializer">The Item1 serializer.</param>
        /// <param name="item2Serializer">The Item2 serializer.</param>
        /// <param name="item3Serializer">The Item3 serializer.</param>
        /// <param name="item4Serializer">The Item4 serializer.</param>
        /// <param name="item5Serializer">The Item5 serializer.</param>
        /// <param name="item6Serializer">The Item6 serializer.</param>
        /// <param name="item7Serializer">The Item7 serializer.</param>
        /// <param name="restSerializer">The Rest serializer.</param>
        public TupleSerializer(
            IBsonSerializer<T1> item1Serializer,
            IBsonSerializer<T2> item2Serializer,
            IBsonSerializer<T3> item3Serializer,
            IBsonSerializer<T4> item4Serializer,
            IBsonSerializer<T5> item5Serializer,
            IBsonSerializer<T6> item6Serializer,
            IBsonSerializer<T7> item7Serializer,
            IBsonSerializer<TRest> restSerializer)
        {
            if (item1Serializer == null) { throw new ArgumentNullException("item1Serializer"); }
            if (item2Serializer == null) { throw new ArgumentNullException("item2Serializer"); }
            if (item3Serializer == null) { throw new ArgumentNullException("item3Serializer"); }
            if (item4Serializer == null) { throw new ArgumentNullException("item4Serializer"); }
            if (item5Serializer == null) { throw new ArgumentNullException("item5Serializer"); }
            if (item6Serializer == null) { throw new ArgumentNullException("item6Serializer"); }
            if (item7Serializer == null) { throw new ArgumentNullException("item7Serializer"); }
            if (restSerializer == null) { throw new ArgumentNullException("restSerializer"); }

            _lazyItem1Serializer = new Lazy<IBsonSerializer<T1>>(() => item1Serializer);
            _lazyItem2Serializer = new Lazy<IBsonSerializer<T2>>(() => item2Serializer);
            _lazyItem3Serializer = new Lazy<IBsonSerializer<T3>>(() => item3Serializer);
            _lazyItem4Serializer = new Lazy<IBsonSerializer<T4>>(() => item4Serializer);
            _lazyItem5Serializer = new Lazy<IBsonSerializer<T5>>(() => item5Serializer);
            _lazyItem6Serializer = new Lazy<IBsonSerializer<T6>>(() => item6Serializer);
            _lazyItem7Serializer = new Lazy<IBsonSerializer<T7>>(() => item7Serializer);
            _lazyRestSerializer = new Lazy<IBsonSerializer<TRest>>(() => restSerializer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TupleSerializer{T1, T2, T3, T4, T5, T6, T7, TRest}" /> class.
        /// </summary>
        /// <param name="serializerRegistry">The serializer registry.</param>
        public TupleSerializer(IBsonSerializerRegistry serializerRegistry)
        {
            if (serializerRegistry == null) { throw new ArgumentNullException("serializerRegistry"); }

            _lazyItem1Serializer = new Lazy<IBsonSerializer<T1>>(() => serializerRegistry.GetSerializer<T1>());
            _lazyItem2Serializer = new Lazy<IBsonSerializer<T2>>(() => serializerRegistry.GetSerializer<T2>());
            _lazyItem3Serializer = new Lazy<IBsonSerializer<T3>>(() => serializerRegistry.GetSerializer<T3>());
            _lazyItem4Serializer = new Lazy<IBsonSerializer<T4>>(() => serializerRegistry.GetSerializer<T4>());
            _lazyItem5Serializer = new Lazy<IBsonSerializer<T5>>(() => serializerRegistry.GetSerializer<T5>());
            _lazyItem6Serializer = new Lazy<IBsonSerializer<T6>>(() => serializerRegistry.GetSerializer<T6>());
            _lazyItem7Serializer = new Lazy<IBsonSerializer<T7>>(() => serializerRegistry.GetSerializer<T7>());
            _lazyRestSerializer = new Lazy<IBsonSerializer<TRest>>(() => serializerRegistry.GetSerializer<TRest>());
        }

        // public properties
        /// <summary>
        /// Gets the Item1 serializer.
        /// </summary>
        public IBsonSerializer<T1> Item1Serializer
        {
            get { return _lazyItem1Serializer.Value; }
        }

        /// <summary>
        /// Gets the Item2 serializer.
        /// </summary>
        public IBsonSerializer<T2> Item2Serializer
        {
            get { return _lazyItem2Serializer.Value; }
        }

        /// <summary>
        /// Gets the Item3 serializer.
        /// </summary>
        public IBsonSerializer<T3> Item3Serializer
        {
            get { return _lazyItem3Serializer.Value; }
        }

        /// <summary>
        /// Gets the Item4 serializer.
        /// </summary>
        public IBsonSerializer<T4> Item4Serializer
        {
            get { return _lazyItem4Serializer.Value; }
        }

        /// <summary>
        /// Gets the Item5 serializer.
        /// </summary>
        public IBsonSerializer<T5> Item5Serializer
        {
            get { return _lazyItem5Serializer.Value; }
        }

        /// <summary>
        /// Gets the Item6 serializer.
        /// </summary>
        public IBsonSerializer<T6> Item6Serializer
        {
            get { return _lazyItem6Serializer.Value; }
        }

        /// <summary>
        /// Gets the Item7 serializer.
        /// </summary>
        public IBsonSerializer<T7> Item7Serializer
        {
            get { return _lazyItem7Serializer.Value; }
        }

        /// <summary>
        /// Gets the Rest serializer.
        /// </summary>
        public IBsonSerializer<TRest> RestSerializer
        {
            get { return _lazyRestSerializer.Value; }
        }

        // public methods
        /// <summary>
        /// Deserializes the value.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="args">The deserialization args.</param>
        /// <returns>A deserialized value.</returns>
        protected override Tuple<T1, T2, T3, T4, T5, T6, T7, TRest> DeserializeValue(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            context.Reader.ReadStartArray();
            var item1 = _lazyItem1Serializer.Value.Deserialize(context);
            var item2 = _lazyItem2Serializer.Value.Deserialize(context);
            var item3 = _lazyItem3Serializer.Value.Deserialize(context);
            var item4 = _lazyItem4Serializer.Value.Deserialize(context);
            var item5 = _lazyItem5Serializer.Value.Deserialize(context);
            var item6 = _lazyItem6Serializer.Value.Deserialize(context);
            var item7 = _lazyItem7Serializer.Value.Deserialize(context);
            var rest = _lazyRestSerializer.Value.Deserialize(context);
            context.Reader.ReadEndArray();

            return new Tuple<T1, T2, T3, T4, T5, T6, T7, TRest>(item1, item2, item3, item4, item5, item6, item7, rest);
        }

        /// <summary>
        /// Serializes the value.
        /// </summary>
        /// <param name="context">The serialization context.</param>
        /// <param name="args">The serialization args.</param>
        /// <param name="value">The value.</param>
        protected override void SerializeValue(BsonSerializationContext context, BsonSerializationArgs args, Tuple<T1, T2, T3, T4, T5, T6, T7, TRest> value)
        {
            context.Writer.WriteStartArray();
            _lazyItem1Serializer.Value.Serialize(context, value.Item1);
            _lazyItem2Serializer.Value.Serialize(context, value.Item2);
            _lazyItem3Serializer.Value.Serialize(context, value.Item3);
            _lazyItem4Serializer.Value.Serialize(context, value.Item4);
            _lazyItem5Serializer.Value.Serialize(context, value.Item5);
            _lazyItem6Serializer.Value.Serialize(context, value.Item6);
            _lazyItem7Serializer.Value.Serialize(context, value.Item7);
            _lazyRestSerializer.Value.Serialize(context, value.Rest);
            context.Writer.WriteEndArray();
        }
    }
}

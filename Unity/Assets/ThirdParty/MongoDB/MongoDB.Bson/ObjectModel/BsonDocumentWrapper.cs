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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace MongoDB.Bson
{
    // this class is a wrapper for an object that we intend to serialize as a BsonDocument
    // it is a subclass of BsonDocument so that it may be used where a BsonDocument is expected
    // this class is mostly used by MongoCollection and MongoCursor when supporting generic query objects

    // if all that ever happens with this wrapped object is that it gets serialized then the BsonDocument is never materialized

    /// <summary>
    /// Represents a BsonDocument wrapper.
    /// </summary>
    public class BsonDocumentWrapper : MaterializedOnDemandBsonDocument
    {
        // private fields
        private readonly object _wrapped;
        private readonly IBsonSerializer _serializer;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="BsonDocumentWrapper"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public BsonDocumentWrapper(object value)
            : this(value, UndiscriminatedActualTypeSerializer<object>.Instance)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BsonDocumentWrapper"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The serializer.</param>
        public BsonDocumentWrapper(object value, IBsonSerializer serializer)
        {
            if (serializer == null)
            {
                throw new ArgumentNullException("serializer");
            }

            _wrapped = value;
            _serializer = serializer;
        }

        // public properties
        /// <summary>
        /// Gets the serializer.
        /// </summary>
        /// <value>
        /// The serializer.
        /// </value>
        public IBsonSerializer Serializer
        {
            get { return _serializer; }
        }

        /// <summary>
        /// Gets the wrapped value.
        /// </summary>
        public object Wrapped
        {
            get { return _wrapped; }
        }

        // public static methods
        /// <summary>
        /// Creates a new instance of the BsonDocumentWrapper class.
        /// </summary>
        /// <typeparam name="TNominalType">The nominal type of the wrapped object.</typeparam>
        /// <param name="value">The wrapped object.</param>
        /// <returns>A BsonDocumentWrapper.</returns>
        public static BsonDocumentWrapper Create<TNominalType>(TNominalType value)
        {
            return Create(typeof(TNominalType), value);
        }

        /// <summary>
        /// Creates a new instance of the BsonDocumentWrapper class.
        /// </summary>
        /// <param name="nominalType">The nominal type of the wrapped object.</param>
        /// <param name="value">The wrapped object.</param>
        /// <returns>A BsonDocumentWrapper.</returns>
        public static BsonDocumentWrapper Create(Type nominalType, object value)
        {
            var serializer = BsonSerializer.LookupSerializer(nominalType);
            return new BsonDocumentWrapper(value, serializer);
        }

        /// <summary>
        /// Creates a list of new instances of the BsonDocumentWrapper class.
        /// </summary>
        /// <typeparam name="TNominalType">The nominal type of the wrapped objects.</typeparam>
        /// <param name="values">A list of wrapped objects.</param>
        /// <returns>A list of BsonDocumentWrappers.</returns>
        public static IEnumerable<BsonDocumentWrapper> CreateMultiple<TNominalType>(IEnumerable<TNominalType> values)
        {
            if (values == null)
            {
                throw new ArgumentNullException("values");
            }

            var serializer = BsonSerializer.LookupSerializer(typeof(TNominalType));
            return values.Select(v => new BsonDocumentWrapper(v, serializer));
        }

        /// <summary>
        /// Creates a list of new instances of the BsonDocumentWrapper class.
        /// </summary>
        /// <param name="nominalType">The nominal type of the wrapped object.</param>
        /// <param name="values">A list of wrapped objects.</param>
        /// <returns>A list of BsonDocumentWrappers.</returns>
        public static IEnumerable<BsonDocumentWrapper> CreateMultiple(Type nominalType, IEnumerable values)
        {
            if (nominalType == null)
            {
                throw new ArgumentNullException("nominalType");
            }
            if (values == null)
            {
                throw new ArgumentNullException("values");
            }

            var serializer = BsonSerializer.LookupSerializer(nominalType);
            return values.Cast<object>().Select(v => new BsonDocumentWrapper(v, serializer));
        }

        // public methods
        /// <summary>
        /// Creates a shallow clone of the document (see also DeepClone).
        /// </summary>
        /// <returns>
        /// A shallow clone of the document.
        /// </returns>
        public override BsonValue Clone()
        {
            if (IsMaterialized)
            {
                return base.Clone();
            }
            else
            {
                return new BsonDocumentWrapper(
                    _wrapped,
                    _serializer);
            }
        }

        // protected methods
        /// <summary>
        /// Materializes the BsonDocument.
        /// </summary>
        /// <returns>The materialized elements.</returns>
        protected override IEnumerable<BsonElement> Materialize()
        {
            var bsonDocument = new BsonDocument();
            var writerSettings = BsonDocumentWriterSettings.Defaults;
            using (var bsonWriter = new BsonDocumentWriter(bsonDocument, writerSettings))
            {
                var context = BsonSerializationContext.CreateRoot(bsonWriter);
                _serializer.Serialize(context, _wrapped);
            }

            return bsonDocument.Elements;
        }

        /// <summary>
        /// Informs subclasses that the Materialize process completed so they can free any resources related to the unmaterialized state.
        /// </summary>
        protected override void MaterializeCompleted()
        {
        }
    }
}

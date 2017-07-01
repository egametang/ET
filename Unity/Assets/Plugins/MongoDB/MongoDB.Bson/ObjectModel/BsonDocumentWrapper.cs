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
        private Type _wrappedNominalType;
        private object _wrappedObject;
        private IBsonSerializer _serializer;
        private IBsonSerializationOptions _serializationOptions;
        private bool _isUpdateDocument;

        // constructors
        // needed for Deserialize
        // (even though we're going to end up throwing an InvalidOperationException)
        private BsonDocumentWrapper()
        {
        }

        /// <summary>
        /// Initializes a new instance of the BsonDocumentWrapper class.
        /// </summary>
        /// <param name="wrappedObject">The wrapped object.</param>
        public BsonDocumentWrapper(object wrappedObject)
            : this((wrappedObject == null) ? typeof(object) : wrappedObject.GetType(), wrappedObject)
        {
        }

        /// <summary>
        /// Initializes a new instance of the BsonDocumentWrapper class.
        /// </summary>
        /// <param name="wrappedNominalType">The nominal type of the wrapped object.</param>
        /// <param name="wrappedObject">The wrapped object.</param>
        public BsonDocumentWrapper(Type wrappedNominalType, object wrappedObject)
            : this(wrappedNominalType, wrappedObject, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the BsonDocumentWrapper class.
        /// </summary>
        /// <param name="wrappedNominalType">The nominal type of the wrapped object.</param>
        /// <param name="wrappedObject">The wrapped object.</param>
        /// <param name="isUpdateDocument">Whether the wrapped object is an update document that needs to be checked.</param>
        public BsonDocumentWrapper(Type wrappedNominalType, object wrappedObject, bool isUpdateDocument)
            : this(wrappedNominalType, wrappedObject, BsonSerializer.LookupSerializer(wrappedNominalType), null, isUpdateDocument)
        {
        }

        /// <summary>
        /// Initializes a new instance of the BsonDocumentWrapper class.
        /// </summary>
        /// <param name="wrappedNominalType">The nominal type of the wrapped object.</param>
        /// <param name="wrappedObject">The wrapped object.</param>
        /// <param name="serializer">The serializer.</param>
        /// <param name="serializationOptions">The serialization options.</param>
        /// <param name="isUpdateDocument">Whether the wrapped object is an update document that needs to be checked.</param>
        public BsonDocumentWrapper(Type wrappedNominalType, object wrappedObject, IBsonSerializer serializer, IBsonSerializationOptions serializationOptions, bool isUpdateDocument)
        {
            if (wrappedNominalType == null)
            {
                throw new ArgumentNullException("wrappedNominalType");
            }

            if (serializer == null)
            {
                throw new ArgumentNullException("serializer");
            }

            _wrappedNominalType = wrappedNominalType;
            _wrappedObject = wrappedObject;
            _serializer = serializer;
            _serializationOptions = serializationOptions;
            _isUpdateDocument = isUpdateDocument;
        }

        // public properties
        /// <summary>
        /// Gets whether the wrapped document is an update document.
        /// </summary>
        public bool IsUpdateDocument
        {
            get { return _isUpdateDocument; }
        }

        /// <summary>
        /// Gets the serialization options.
        /// </summary>
        /// <value>
        /// The serialization options.
        /// </value>
        public IBsonSerializationOptions SerializationOptions
        {
            get { return _serializationOptions; }
        }

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
        /// Gets the nominal type of the wrapped document.
        /// </summary>
        public Type WrappedNominalType
        {
            get { return _wrappedNominalType; }
        }

        /// <summary>
        /// Gets the wrapped object.
        /// </summary>
        public object WrappedObject
        {
            get { return _wrappedObject; }
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
        /// <typeparam name="TNominalType">The nominal type of the wrapped object.</typeparam>
        /// <param name="value">The wrapped object.</param>
        /// <param name="isUpdateDocument">Whether the wrapped object is an update document.</param>
        /// <returns>A BsonDocumentWrapper.</returns>
        public static BsonDocumentWrapper Create<TNominalType>(TNominalType value, bool isUpdateDocument)
        {
            return Create(typeof(TNominalType), value, isUpdateDocument);
        }

        /// <summary>
        /// Creates a new instance of the BsonDocumentWrapper class.
        /// </summary>
        /// <param name="nominalType">The nominal type of the wrapped object.</param>
        /// <param name="value">The wrapped object.</param>
        /// <returns>A BsonDocumentWrapper.</returns>
        public static BsonDocumentWrapper Create(Type nominalType, object value)
        {
            return Create(nominalType, value, false); // isUpdateDocument = false
        }

        /// <summary>
        /// Creates a new instance of the BsonDocumentWrapper class.
        /// </summary>
        /// <param name="nominalType">The nominal type of the wrapped object.</param>
        /// <param name="value">The wrapped object.</param>
        /// <param name="isUpdateDocument">Whether the wrapped object is an update document.</param>
        /// <returns>A BsonDocumentWrapper.</returns>
        public static BsonDocumentWrapper Create(Type nominalType, object value, bool isUpdateDocument)
        {
            return new BsonDocumentWrapper(nominalType, value, isUpdateDocument);
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

            return values.Select(v => new BsonDocumentWrapper(typeof(TNominalType), v));
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

            return values.Cast<object>().Select(v => new BsonDocumentWrapper(nominalType, v));
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
                    _wrappedNominalType,
                    _wrappedObject,
                    _serializer,
                    _serializationOptions,
                    _isUpdateDocument);
            }
        }

        /// <summary>
        /// Deserialize is an invalid operation for BsonDocumentWrapper.
        /// </summary>
        /// <param name="bsonReader">Not applicable.</param>
        /// <param name="nominalType">Not applicable.</param>
        /// <param name="options">Not applicable.</param>
        /// <returns>Not applicable.</returns>
        [Obsolete("Deserialize was intended to be private and will become private in a future release.")]
        public override object Deserialize(BsonReader bsonReader, Type nominalType, IBsonSerializationOptions options)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Serializes the wrapped object to a BsonWriter.
        /// </summary>
        /// <param name="bsonWriter">The writer.</param>
        /// <param name="nominalType">The nominal type (overridden by the wrapped nominal type).</param>
        /// <param name="options">The serialization options (can be null).</param>
        [Obsolete("Serialize was intended to be private and will become private in a future release.")]
        public override void Serialize(BsonWriter bsonWriter, Type nominalType, IBsonSerializationOptions options)
        {
            BsonDocumentWrapperSerializer.Instance.Serialize(bsonWriter, nominalType, this, options);
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
                BsonDocumentWrapperSerializer.Instance.Serialize(bsonWriter, typeof(BsonDocumentWrapper), this, null);
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

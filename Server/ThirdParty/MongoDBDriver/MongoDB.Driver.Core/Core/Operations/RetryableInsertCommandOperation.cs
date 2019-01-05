/* Copyright 2017-present MongoDB Inc.
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
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Core.Bindings;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.Operations.ElementNameValidators;
using MongoDB.Driver.Core.WireProtocol.Messages;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders;

namespace MongoDB.Driver.Core.Operations
{
    /// <summary>
    /// Represents an insert command operation.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document.</typeparam>
    public class RetryableInsertCommandOperation<TDocument> : RetryableWriteCommandOperationBase where TDocument : class
    {
        // private fields
        private bool? _bypassDocumentValidation;
        private readonly CollectionNamespace _collectionNamespace;
        private readonly BatchableSource<TDocument> _documents;
        private readonly IBsonSerializer<TDocument> _documentSerializer;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="RetryableInsertCommandOperation{TDocument}"/> class.
        /// </summary>
        /// <param name="collectionNamespace">The collection namespace.</param>
        /// <param name="documents">The documents.</param>
        /// <param name="documentSerializer">The document serializer.</param>
        /// <param name="messageEncoderSettings">The message encoder settings.</param>
        public RetryableInsertCommandOperation(
            CollectionNamespace collectionNamespace,
            BatchableSource<TDocument> documents,
            IBsonSerializer<TDocument> documentSerializer,
            MessageEncoderSettings messageEncoderSettings)
            : base(Ensure.IsNotNull(collectionNamespace, nameof(collectionNamespace)).DatabaseNamespace, messageEncoderSettings)
        {
            _collectionNamespace = Ensure.IsNotNull(collectionNamespace, nameof(collectionNamespace));
            _documents = Ensure.IsNotNull(documents, nameof(documents));
            _documentSerializer = Ensure.IsNotNull(documentSerializer, nameof(documentSerializer));
        }

        // public properties
        /// <summary>
        /// Gets or sets a value indicating whether to bypass document validation.
        /// </summary>
        /// <value>
        /// A value indicating whether to bypass document validation.
        /// </value>
        public bool? BypassDocumentValidation
        {
            get { return _bypassDocumentValidation; }
            set { _bypassDocumentValidation = value; }
        }

        /// <summary>
        /// Gets the collection namespace.
        /// </summary>
        /// <value>
        /// The collection namespace.
        /// </value>
        public CollectionNamespace CollectionNamespace
        {
            get { return _collectionNamespace; }
        }

        /// <summary>
        /// Gets the documents.
        /// </summary>
        /// <value>
        /// The documents.
        /// </value>
        public BatchableSource<TDocument> Documents
        {
            get { return _documents; }
        }

        /// <summary>
        /// Gets the document serializer.
        /// </summary>
        /// <value>
        /// The document serializer.
        /// </value>
        public IBsonSerializer<TDocument> DocumentSerializer
        {
            get { return _documentSerializer; }
        }

        // protected methods
        /// <inheritdoc />
        protected override BsonDocument CreateCommand(ICoreSessionHandle session, ConnectionDescription connectionDescription, int attempt, long? transactionNumber)
        {
            var writeConcern = WriteConcernHelper.GetWriteConcernForWriteCommand(session, WriteConcern);
            return new BsonDocument
            {
                { "insert", _collectionNamespace.CollectionName },
                { "ordered", IsOrdered },
                { "bypassDocumentValidation", () => _bypassDocumentValidation, _bypassDocumentValidation.HasValue },
                { "writeConcern", writeConcern, writeConcern != null },
                { "txnNumber", () => transactionNumber.Value, transactionNumber.HasValue }
            };
        }

        /// <inheritdoc />
        protected override IEnumerable<Type1CommandMessageSection> CreateCommandPayloads(IChannelHandle channel, int attempt)
        {
            BatchableSource<TDocument> documents;
            if (attempt == 1)
            {
                documents = _documents;
            }
            else
            {
                documents = new BatchableSource<TDocument>(_documents.Items, _documents.Offset, _documents.ProcessedCount, canBeSplit: false);
            }
            var isSystemIndexesCollection = _collectionNamespace.Equals(CollectionNamespace.DatabaseNamespace.SystemIndexesCollection);
            var elementNameValidator = isSystemIndexesCollection ? (IElementNameValidator)NoOpElementNameValidator.Instance : CollectionElementNameValidator.Instance;
            var maxBatchCount = Math.Min(MaxBatchCount ?? int.MaxValue, channel.ConnectionDescription.MaxBatchCount);
            var maxDocumentSize = channel.ConnectionDescription.MaxDocumentSize;
            var payload = new Type1CommandMessageSection<TDocument>("documents", documents, _documentSerializer, elementNameValidator, maxBatchCount, maxDocumentSize);
            return new Type1CommandMessageSection[] { payload };
        }

        // nested types
        private class InsertSerializer : SerializerBase<TDocument>
        {
            // private fields
            private IBsonSerializer _cachedSerializer;
            private readonly IBsonSerializer<TDocument> _documentSerializer;

            // constructors
            public InsertSerializer(IBsonSerializer<TDocument> documentSerializer)
            {
                _documentSerializer = documentSerializer;
                _cachedSerializer = documentSerializer;
            }

            // public methods
            public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, TDocument value)
            {
                IBsonSerializer serializer;

                var actualType = value.GetType();
                if (actualType == typeof(TDocument))
                {
                    serializer = _documentSerializer;
                }
                else
                {
                    if (_cachedSerializer.ValueType != actualType)
                    {
                        _cachedSerializer = BsonSerializer.LookupSerializer(actualType);
                    }
                    serializer = _cachedSerializer;
                }

                serializer.Serialize(context, value);
            }
        }
    }
}

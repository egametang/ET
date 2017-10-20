/* Copyright 2013-2015 MongoDB Inc.
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


using MongoDB.Bson.Serialization;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders;

namespace MongoDB.Driver.Core.WireProtocol.Messages
{
    /// <summary>
    /// Represents an Insert message.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document.</typeparam>
    public class InsertMessage<TDocument> : RequestMessage
    {
        // fields
        private readonly CollectionNamespace _collectionNamespace;
        private readonly bool _continueOnError;
        private readonly BatchableSource<TDocument> _documentSource;
        private readonly int _maxBatchCount;
        private readonly int _maxMessageSize;
        private readonly IBsonSerializer<TDocument> _serializer;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="InsertMessage{TDocument}"/> class.
        /// </summary>
        /// <param name="requestId">The request identifier.</param>
        /// <param name="collectionNamespace">The collection namespace.</param>
        /// <param name="serializer">The serializer.</param>
        /// <param name="documentSource">The document source.</param>
        /// <param name="maxBatchCount">The maximum batch count.</param>
        /// <param name="maxMessageSize">Maximum size of the message.</param>
        /// <param name="continueOnError">if set to <c>true</c> the server should continue on error.</param>
        public InsertMessage(
            int requestId,
            CollectionNamespace collectionNamespace,
            IBsonSerializer<TDocument> serializer,
            BatchableSource<TDocument> documentSource,
            int maxBatchCount,
            int maxMessageSize,
            bool continueOnError)
            : base(requestId)
        {
            _collectionNamespace = Ensure.IsNotNull(collectionNamespace, nameof(collectionNamespace));
            _serializer = Ensure.IsNotNull(serializer, nameof(serializer));
            _documentSource = Ensure.IsNotNull(documentSource, nameof(documentSource));
            _maxBatchCount = Ensure.IsGreaterThanZero(maxBatchCount, nameof(maxBatchCount));
            _maxMessageSize = Ensure.IsGreaterThanZero(maxMessageSize, nameof(maxMessageSize));
            _continueOnError = continueOnError;
        }

        // properties
        /// <summary>
        /// Gets the collection namespace.
        /// </summary>
        public CollectionNamespace CollectionNamespace
        {
            get { return _collectionNamespace; }
        }

        /// <summary>
        /// Gets a value indicating whether the server should continue on error.
        /// </summary>
        public bool ContinueOnError
        {
            get { return _continueOnError; }
        }

        /// <summary>
        /// Gets the document source.
        /// </summary>
        public BatchableSource<TDocument> DocumentSource
        {
            get { return _documentSource; }
        }

        /// <summary>
        /// Gets the maximum number of documents in a batch.
        /// </summary>
        public int MaxBatchCount
        {
            get { return _maxBatchCount; }
        }

        /// <summary>
        /// Gets the maximum size of a message.
        /// </summary>
        public int MaxMessageSize
        {
            get { return _maxMessageSize; }
        }

        /// <inheritdoc/>
        public override MongoDBMessageType MessageType
        {
            get { return MongoDBMessageType.Insert; }
        }

        /// <summary>
        /// Gets the serializer.
        /// </summary>
        public IBsonSerializer<TDocument> Serializer
        {
            get { return _serializer; }
        }

        // methods
        /// <inheritdoc/>
        public override IMessageEncoder GetEncoder(IMessageEncoderFactory encoderFactory)
        {
            return encoderFactory.GetInsertMessageEncoder<TDocument>(_serializer);
        }
    }
}

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


using System;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders;

namespace MongoDB.Driver.Core.WireProtocol.Messages
{
    /// <summary>
    /// Represents a Query message.
    /// </summary>
    public class QueryMessage : RequestMessage
    {
        // fields
        private readonly bool _awaitData;
        private readonly int _batchSize;
        private readonly CollectionNamespace _collectionNamespace;
        private readonly BsonDocument _fields;
        private readonly bool _noCursorTimeout;
        private readonly bool _oplogReplay;
        private readonly bool _partialOk;
        private readonly BsonDocument _query;
        private readonly IElementNameValidator _queryValidator;
        private readonly int _skip;
        private readonly bool _slaveOk;
        private readonly bool _tailableCursor;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryMessage"/> class.
        /// </summary>
        /// <param name="requestId">The request identifier.</param>
        /// <param name="collectionNamespace">The collection namespace.</param>
        /// <param name="query">The query.</param>
        /// <param name="fields">The fields.</param>
        /// <param name="queryValidator">The query validator.</param>
        /// <param name="skip">The number of documents to skip.</param>
        /// <param name="batchSize">The size of a batch.</param>
        /// <param name="slaveOk">if set to <c>true</c> it is OK if the server is not the primary.</param>
        /// <param name="partialOk">if set to <c>true</c> the server is allowed to return partial results if any shards are unavailable.</param>
        /// <param name="noCursorTimeout">if set to <c>true</c> the server should not timeout the cursor.</param>
        /// <param name="oplogReplay">if set to <c>true</c> the OplogReplay bit will be set.</param>
        /// <param name="tailableCursor">if set to <c>true</c> the query should return a tailable cursor.</param>
        /// <param name="awaitData">if set to <c>true</c> the server should await data (used with tailable cursors).</param>
        /// <param name="shouldBeSent">A delegate that determines whether this message should be sent.</param>
        public QueryMessage(
            int requestId,
            CollectionNamespace collectionNamespace,
            BsonDocument query,
            BsonDocument fields,
            IElementNameValidator queryValidator,
            int skip,
            int batchSize,
            bool slaveOk,
            bool partialOk,
            bool noCursorTimeout,
            bool oplogReplay,
            bool tailableCursor,
            bool awaitData,
            Func<bool> shouldBeSent = null)
            : base(requestId, shouldBeSent)
        {
            _collectionNamespace = Ensure.IsNotNull(collectionNamespace, nameof(collectionNamespace));
            _query = Ensure.IsNotNull(query, nameof(query));
            _fields = fields; // can be null
            _queryValidator = Ensure.IsNotNull(queryValidator, nameof(queryValidator));
            _skip = Ensure.IsGreaterThanOrEqualToZero(skip, nameof(skip));
            _batchSize = batchSize; // can be negative
            _slaveOk = slaveOk;
            _partialOk = partialOk;
            _noCursorTimeout = noCursorTimeout;
            _oplogReplay = oplogReplay;
            _tailableCursor = tailableCursor;
            _awaitData = awaitData;
        }

        // properties
        /// <summary>
        /// Gets a value indicating whether the server should await data (used with tailable cursors).
        /// </summary>
        public bool AwaitData
        {
            get { return _awaitData; }
        }

        /// <summary>
        /// Gets the size of a batch.
        /// </summary>
        public int BatchSize
        {
            get { return _batchSize; }
        }

        /// <summary>
        /// Gets the collection namespace.
        /// </summary>
        public CollectionNamespace CollectionNamespace
        {
            get { return _collectionNamespace; }
        }

        /// <summary>
        /// Gets the fields.
        /// </summary>
        public BsonDocument Fields
        {
            get { return _fields; }
        }

        /// <inheritdoc/>
        public override MongoDBMessageType MessageType
        {
            get { return MongoDBMessageType.Query; }
        }

        /// <summary>
        /// Gets a value indicating whether the server should not timeout the cursor.
        /// </summary>
        public bool NoCursorTimeout
        {
            get { return _noCursorTimeout; }
        }

        /// <summary>
        /// Gets a value indicating whether the OplogReplay bit will be set.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the OplogReplay bit will be set; otherwise, <c>false</c>.
        /// </value>
        public bool OplogReplay
        {
            get { return _oplogReplay; }
        }

        /// <summary>
        /// Gets a value indicating whether the server is allowed to return partial results if any shards are unavailable.
        /// </summary>
        public bool PartialOk
        {
            get { return _partialOk; }
        }

        /// <summary>
        /// Gets the query.
        /// </summary>
        public BsonDocument Query
        {
            get { return _query; }
        }

        /// <summary>
        /// Gets the query validator.
        /// </summary>
        public IElementNameValidator QueryValidator
        {
            get { return _queryValidator; }
        }

        /// <summary>
        /// Gets the number of documents to skip.
        /// </summary>
        public int Skip
        {
            get { return _skip; }
        }

        /// <summary>
        /// Gets a value indicating whether it is OK if the server is not the primary.
        /// </summary>
        public bool SlaveOk
        {
            get { return _slaveOk; }
        }

        /// <summary>
        /// Gets a value indicating whether the query should return a tailable cursor.
        /// </summary>
        public bool TailableCursor
        {
            get { return _tailableCursor; }
        }

        // methods
        /// <inheritdoc/>
        public override IMessageEncoder GetEncoder(IMessageEncoderFactory encoderFactory)
        {
            return encoderFactory.GetQueryMessageEncoder();
        }
    }
}

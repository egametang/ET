/* Copyright 2015 MongoDB Inc.
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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Core.Bindings;
using MongoDB.Driver.Core.Events;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.Servers;
using MongoDB.Driver.Core.WireProtocol;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders;

namespace MongoDB.Driver.Core.Operations
{
    /// <summary>
    /// Represents a Find opcode operation.
    /// </summary>
    /// <typeparam name="TDocument">The type of the returned documents.</typeparam>
    public class FindOpcodeOperation<TDocument> : IReadOperation<IAsyncCursor<TDocument>>
    {
        // fields
        private bool? _allowPartialResults;
        private int? _batchSize;
        private readonly CollectionNamespace _collectionNamespace;
        private string _comment;
        private CursorType _cursorType;
        private BsonDocument _filter;
        private int? _firstBatchSize;
        private BsonValue _hint;
        private int? _limit;
        private BsonDocument _max;
        private int? _maxScan;
        private TimeSpan? _maxTime;
        private readonly MessageEncoderSettings _messageEncoderSettings;
        private BsonDocument _min;
        private BsonDocument _modifiers;
        private bool? _noCursorTimeout;
        private bool? _oplogReplay;
        private BsonDocument _projection;
        private readonly IBsonSerializer<TDocument> _resultSerializer;
        private bool? _showRecordId;
        private int? _skip;
        private bool? _snapshot;
        private BsonDocument _sort;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="FindOpcodeOperation{TDocument}"/> class.
        /// </summary>
        /// <param name="collectionNamespace">The collection namespace.</param>
        /// <param name="resultSerializer">The result serializer.</param>
        /// <param name="messageEncoderSettings">The message encoder settings.</param>
        public FindOpcodeOperation(
            CollectionNamespace collectionNamespace,
            IBsonSerializer<TDocument> resultSerializer,
            MessageEncoderSettings messageEncoderSettings)
        {
            _collectionNamespace = Ensure.IsNotNull(collectionNamespace, nameof(collectionNamespace));
            _resultSerializer = Ensure.IsNotNull(resultSerializer, nameof(resultSerializer));
            _messageEncoderSettings = Ensure.IsNotNull(messageEncoderSettings, nameof(messageEncoderSettings));
            _cursorType = CursorType.NonTailable;
        }

        // properties
        /// <summary>
        /// Gets or sets a value indicating whether the server is allowed to return partial results if any shards are unavailable.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the server is allowed to return partial results if any shards are unavailable; otherwise, <c>false</c>.
        /// </value>
        public bool? AllowPartialResults
        {
            get { return _allowPartialResults; }
            set { _allowPartialResults = value; }
        }

        /// <summary>
        /// Gets or sets the size of a batch.
        /// </summary>
        /// <value>
        /// The size of a batch.
        /// </value>
        public int? BatchSize
        {
            get { return _batchSize; }
            set { _batchSize = Ensure.IsNullOrGreaterThanOrEqualToZero(value, nameof(value)); }
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
        /// Gets or sets the comment.
        /// </summary>
        /// <value>
        /// The comment.
        /// </value>
        public string Comment
        {
            get { return _comment; }
            set { _comment = value; }
        }

        /// <summary>
        /// Gets or sets the type of the cursor.
        /// </summary>
        /// <value>
        /// The type of the cursor.
        /// </value>
        public CursorType CursorType
        {
            get { return _cursorType; }
            set { _cursorType = value; }
        }

        /// <summary>
        /// Gets or sets the filter.
        /// </summary>
        /// <value>
        /// The filter.
        /// </value>
        public BsonDocument Filter
        {
            get { return _filter; }
            set { _filter = value; }
        }

        /// <summary>
        /// Gets or sets the size of the first batch.
        /// </summary>
        /// <value>
        /// The size of the first batch.
        /// </value>
        public int? FirstBatchSize
        {
            get { return _firstBatchSize; }
            set { _firstBatchSize = Ensure.IsNullOrGreaterThanOrEqualToZero(value, nameof(value)); }
        }

        /// <summary>
        /// Gets or sets the hint.
        /// </summary>
        /// <value>
        /// The hint.
        /// </value>
        public BsonValue Hint
        {
            get { return _hint; }
            set { _hint = value; }
        }

        /// <summary>
        /// Gets or sets the limit.
        /// </summary>
        /// <value>
        /// The limit.
        /// </value>
        public int? Limit
        {
            get { return _limit; }
            set { _limit = value; }
        }

        /// <summary>
        /// Gets or sets the max key value.
        /// </summary>
        /// <value>
        /// The max key value.
        /// </value>
        public BsonDocument Max
        {
            get { return _max; }
            set { _max = value; }
        }

        /// <summary>
        /// Gets or sets the max scan.
        /// </summary>
        /// <value>
        /// The max scan.
        /// </value>
        public int? MaxScan
        {
            get { return _maxScan; }
            set { _maxScan = Ensure.IsNullOrGreaterThanZero(value, nameof(value)); }
        }

        /// <summary>
        /// Gets or sets the maximum time the server should spend on this operation.
        /// </summary>
        /// <value>
        /// The maximum time the server should spend on this operation.
        /// </value>
        public TimeSpan? MaxTime
        {
            get { return _maxTime; }
            set { _maxTime = Ensure.IsNullOrGreaterThanZero(value, nameof(value)); }
        }

        /// <summary>
        /// Gets the message encoder settings.
        /// </summary>
        /// <value>
        /// The message encoder settings.
        /// </value>
        public MessageEncoderSettings MessageEncoderSettings
        {
            get { return _messageEncoderSettings; }
        }

        /// <summary>
        /// Gets or sets the min key value.
        /// </summary>
        /// <value>
        /// The max min value.
        /// </value>
        public BsonDocument Min
        {
            get { return _min; }
            set { _min = value; }
        }

        /// <summary>
        /// Gets or sets any additional query modifiers.
        /// </summary>
        /// <value>
        /// The additional query modifiers.
        /// </value>
        public BsonDocument Modifiers
        {
            get { return _modifiers; }
            set { _modifiers = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the server will not timeout the cursor.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the server will not timeout the cursor; otherwise, <c>false</c>.
        /// </value>
        public bool? NoCursorTimeout
        {
            get { return _noCursorTimeout; }
            set { _noCursorTimeout = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the OplogReplay bit will be set.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the OplogReplay bit will be set; otherwise, <c>false</c>.
        /// </value>
        public bool? OplogReplay
        {
            get { return _oplogReplay; }
            set { _oplogReplay = value; }
        }

        /// <summary>
        /// Gets or sets the projection.
        /// </summary>
        /// <value>
        /// The projection.
        /// </value>
        public BsonDocument Projection
        {
            get { return _projection; }
            set { _projection = value; }
        }

        /// <summary>
        /// Gets the result serializer.
        /// </summary>
        /// <value>
        /// The result serializer.
        /// </value>
        public IBsonSerializer<TDocument> ResultSerializer
        {
            get { return _resultSerializer; }
        }

        /// <summary>
        /// Gets or sets whether the record Id should be added to the result document.
        /// </summary>
        /// <value>
        /// Whether the record Id should be added to the result documentr.
        /// </value>
        public bool? ShowRecordId
        {
            get { return _showRecordId; }
            set { _showRecordId = value; }
        }

        /// <summary>
        /// Gets or sets the number of documents skip.
        /// </summary>
        /// <value>
        /// The number of documents skip.
        /// </value>
        public int? Skip
        {
            get { return _skip; }
            set { _skip = Ensure.IsNullOrGreaterThanOrEqualToZero(value, nameof(value)); }
        }

        /// <summary>
        /// Gets or sets whether to use snapshot behavior.
        /// </summary>
        /// <value>
        /// Whether to use snapshot behavior.
        /// </value>
        public bool? Snapshot
        {
            get { return _snapshot; }
            set { _snapshot = value; }
        }

        /// <summary>
        /// Gets or sets the sort specification.
        /// </summary>
        /// <value>
        /// The sort specification.
        /// </value>
        public BsonDocument Sort
        {
            get { return _sort; }
            set { _sort = value; }
        }

        // methods
        private CursorBatch<TDocument> ExecuteProtocol(IChannelHandle channel, BsonDocument wrappedQuery, bool slaveOk, CancellationToken cancellationToken)
        {
            var firstBatchSize = QueryHelper.CalculateFirstBatchSize(_limit, _firstBatchSize ?? _batchSize);

            return channel.Query<TDocument>(
                _collectionNamespace,
                wrappedQuery,
                _projection,
                NoOpElementNameValidator.Instance,
                _skip ?? 0,
                firstBatchSize,
                slaveOk,
                _allowPartialResults ?? false,
                _noCursorTimeout ?? false,
                _oplogReplay ?? false,
                _cursorType != CursorType.NonTailable, // tailable
                _cursorType == CursorType.TailableAwait, //await data
                _resultSerializer,
                _messageEncoderSettings,
                cancellationToken);
        }

        private Task<CursorBatch<TDocument>> ExecuteProtocolAsync(IChannelHandle channel, BsonDocument wrappedQuery, bool slaveOk, CancellationToken cancellationToken)
        {
            var firstBatchSize = QueryHelper.CalculateFirstBatchSize(_limit, _firstBatchSize ?? _batchSize);

            return channel.QueryAsync<TDocument>(
                _collectionNamespace,
                wrappedQuery,
                _projection,
                NoOpElementNameValidator.Instance,
                _skip ?? 0,
                firstBatchSize,
                slaveOk,
                _allowPartialResults ?? false,
                _noCursorTimeout ?? false,
                _oplogReplay ?? false,
                _cursorType != CursorType.NonTailable, // tailable
                _cursorType == CursorType.TailableAwait, //await data
                _resultSerializer,
                _messageEncoderSettings,
                cancellationToken);
        }

        internal BsonDocument CreateWrappedQuery(ServerType serverType, ReadPreference readPreference)
        {
            var readPreferenceDocument = QueryHelper.CreateReadPreferenceDocument(serverType, readPreference);

            var wrappedQuery = new BsonDocument
            {
                { "$query", _filter ?? new BsonDocument() },
                { "$readPreference", readPreferenceDocument, readPreferenceDocument != null },
                { "$orderby", _sort, _sort != null },
                { "$comment", _comment, _comment != null },
                { "$maxTimeMS", () => _maxTime.Value.TotalMilliseconds, _maxTime.HasValue },
                { "$hint", _hint, _hint != null },
                { "$max", _max, _max != null },
                { "$maxScan", () => _maxScan.Value, _maxScan.HasValue },
                { "$min", _min, _min != null },
                { "$showDiskLoc", () => _showRecordId.Value, _showRecordId.HasValue },
                { "$snapshot", () => _snapshot.Value, _snapshot.HasValue }
            };

            if (_modifiers != null)
            {
                wrappedQuery.Merge(_modifiers, overwriteExistingElements: false);
            }

            if (wrappedQuery.ElementCount == 1 && wrappedQuery.GetElement(0).Name == "$query")
            {
                var unwrappedQuery = wrappedQuery[0].AsBsonDocument;
                if (!unwrappedQuery.Contains("query"))
                {
                    return unwrappedQuery;
                }
            }

            return wrappedQuery;
        }

        /// <inheritdoc/>
        public IAsyncCursor<TDocument> Execute(IReadBinding binding, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(binding, nameof(binding));

            using (EventContext.BeginOperation())
            using (var channelSource = binding.GetReadChannelSource(cancellationToken))
            using (var channel = channelSource.GetChannel(cancellationToken))
            {
                var readPreference = binding.ReadPreference;
                var serverDescription = channelSource.ServerDescription;
                var wrappedQuery = CreateWrappedQuery(serverDescription.Type, readPreference);
                var slaveOk = readPreference != null && readPreference.ReadPreferenceMode != ReadPreferenceMode.Primary;

                using (EventContext.BeginFind(_batchSize, _limit))
                {
                    var batch = ExecuteProtocol(channel, wrappedQuery, slaveOk, cancellationToken);
                    return CreateCursor(channelSource, wrappedQuery, batch);
                }
            }
        }

        /// <inheritdoc/>
        public async Task<IAsyncCursor<TDocument>> ExecuteAsync(IReadBinding binding, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(binding, nameof(binding));

            using (EventContext.BeginOperation())
            using (var channelSource = await binding.GetReadChannelSourceAsync(cancellationToken).ConfigureAwait(false))
            using (var channel = await channelSource.GetChannelAsync(cancellationToken).ConfigureAwait(false))
            {
                var readPreference = binding.ReadPreference;
                var serverDescription = channelSource.ServerDescription;
                var wrappedQuery = CreateWrappedQuery(serverDescription.Type, readPreference);
                var slaveOk = readPreference != null && readPreference.ReadPreferenceMode != ReadPreferenceMode.Primary;

                using (EventContext.BeginFind(_batchSize, _limit))
                {
                    var batch = await ExecuteProtocolAsync(channel, wrappedQuery, slaveOk, cancellationToken).ConfigureAwait(false);
                    return CreateCursor(channelSource, wrappedQuery, batch);
                }
            }
        }

        /// <summary>
        /// Returns an explain operation for this find operation.
        /// </summary>
        /// <param name="verbosity">The verbosity.</param>
        /// <returns>An explain operation.</returns>
        public IReadOperation<BsonDocument> ToExplainOperation(ExplainVerbosity verbosity)
        {
            BsonDocument modifiers;
            if (_modifiers == null)
            {
                modifiers = new BsonDocument();
            }
            else
            {
                modifiers = (BsonDocument)_modifiers.DeepClone();
            }
            modifiers["$explain"] = true;
            var operation = new FindOpcodeOperation<BsonDocument>(_collectionNamespace, BsonDocumentSerializer.Instance, _messageEncoderSettings)
            {
                _allowPartialResults = _allowPartialResults,
                _batchSize = _batchSize,
                _comment = _comment,
                _cursorType = _cursorType,
                _filter = _filter,
                _firstBatchSize = _firstBatchSize,
                _limit = _limit,
                _maxTime = _maxTime,
                _modifiers = modifiers,
                _noCursorTimeout = _noCursorTimeout,
                _oplogReplay = _oplogReplay,
                _projection = _projection,
                _skip = _skip,
                _sort = _sort,
            };

            return new FindOpcodeExplainOperation(operation);
        }

        // private methods
        private IAsyncCursor<TDocument> CreateCursor(IChannelSourceHandle channelSource, BsonDocument query, CursorBatch<TDocument> batch)
        {
            var getMoreChannelSource = new ServerChannelSource(channelSource.Server);
            return new AsyncCursor<TDocument>(
                getMoreChannelSource,
                _collectionNamespace,
                query,
                batch.Documents,
                batch.CursorId,
                _batchSize,
                _limit < 0 ? Math.Abs(_limit.Value) : _limit,
                _resultSerializer,
                _messageEncoderSettings);
        }

        // nested types
        private class FindOpcodeExplainOperation : IReadOperation<BsonDocument>
        {
            private readonly FindOpcodeOperation<BsonDocument> _explainOperation;

            public FindOpcodeExplainOperation(FindOpcodeOperation<BsonDocument> explainOperation)
            {
                _explainOperation = explainOperation;
            }

            public BsonDocument Execute(IReadBinding binding, CancellationToken cancellationToken)
            {
                var cursor = _explainOperation.Execute(binding, cancellationToken);
                var documents = cursor.ToList(cancellationToken);
                return documents.Single();
            }

            public async Task<BsonDocument> ExecuteAsync(IReadBinding binding, CancellationToken cancellationToken)
            {
                var cursor = await _explainOperation.ExecuteAsync(binding, cancellationToken).ConfigureAwait(false);
                var documents = await cursor.ToListAsync(cancellationToken).ConfigureAwait(false);
                return documents.Single();
            }
        }
    }
}

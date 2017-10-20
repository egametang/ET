/* Copyright 2015-2016 MongoDB Inc.
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
    /// Represents a Find command operation.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document.</typeparam>
    public class FindCommandOperation<TDocument> : IReadOperation<IAsyncCursor<TDocument>>
    {
        #region static
        // private static fields
        private static IBsonSerializer<BsonDocument> __findCommandResultSerializer = new PartiallyRawBsonDocumentSerializer(
            "cursor", new PartiallyRawBsonDocumentSerializer(
                "firstBatch", new RawBsonArraySerializer()));
        #endregion

        // fields
        private bool? _allowPartialResults;
        private int? _batchSize;
        private Collation _collation;
        private readonly CollectionNamespace _collectionNamespace;
        private string _comment;
        private CursorType _cursorType;
        private BsonDocument _filter;
        private int? _firstBatchSize;
        private BsonValue _hint;
        private int? _limit;
        private BsonDocument _max;
        private TimeSpan? _maxAwaitTime;
        private int? _maxScan;
        private TimeSpan? _maxTime;
        private readonly MessageEncoderSettings _messageEncoderSettings;
        private BsonDocument _min;
        private bool? _noCursorTimeout;
        private bool? _oplogReplay;
        private BsonDocument _projection;
        private ReadConcern _readConcern = ReadConcern.Default;
        private readonly IBsonSerializer<TDocument> _resultSerializer;
        private bool? _returnKey;
        private bool? _showRecordId;
        private bool? _singleBatch;
        private int? _skip;
        private bool? _snapshot;
        private BsonDocument _sort;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="FindCommandOperation{TDocument}"/> class.
        /// </summary>
        /// <param name="collectionNamespace">The collection namespace.</param>
        /// <param name="resultSerializer">The result serializer.</param>
        /// <param name="messageEncoderSettings">The message encoder settings.</param>
        public FindCommandOperation(
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
        /// Gets or sets the collation.
        /// </summary>
        /// <value>
        /// The collation.
        /// </value>
        public Collation Collation
        {
            get { return _collation; }
            set { _collation = value; }
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
        /// Gets or sets the maximum await time for TailableAwait cursors.
        /// </summary>
        /// <value>
        /// The maximum await time for TailableAwait cursors.
        /// </value>
        public TimeSpan? MaxAwaitTime
        {
            get { return _maxAwaitTime; }
            set { _maxAwaitTime = value; }
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
        /// Gets or sets the read concern.
        /// </summary>
        /// <value>
        /// The read concern.
        /// </value>
        public ReadConcern ReadConcern
        {
            get { return _readConcern; }
            set { _readConcern = Ensure.IsNotNull(value, nameof(value)); }
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
        /// Gets or sets whether to only return the key values.
        /// </summary>
        /// <value>
        /// Whether to only return the key values.
        /// </value>
        public bool? ReturnKey
        {
            get { return _returnKey; }
            set { _returnKey = value; }
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
        /// Gets or sets whether to return only a single batch.
        /// </summary>
        /// <value>
        /// Whether to return only a single batchThe single batch.
        /// </value>
        public bool? SingleBatch
        {
            get { return _singleBatch; }
            set { _singleBatch = value; }
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
        internal BsonDocument CreateCommand(SemanticVersion serverVersion, ServerType serverType)
        {
            Feature.ReadConcern.ThrowIfNotSupported(serverVersion, _readConcern);
            Feature.Collation.ThrowIfNotSupported(serverVersion, _collation);

            var firstBatchSize = _firstBatchSize ?? (_batchSize > 0 ? _batchSize : null);
            var isShardRouter = serverType == ServerType.ShardRouter;

            var command = new BsonDocument
            {
                { "find", _collectionNamespace.CollectionName },
                { "filter", _filter, _filter != null },
                { "sort", _sort, _sort != null },
                { "projection", _projection, _projection != null },
                { "hint", _hint, _hint != null },
                { "skip", () => _skip.Value, _skip.HasValue },
                { "limit", () => Math.Abs(_limit.Value), _limit.HasValue && _limit != 0 },
                { "batchSize", () => firstBatchSize.Value, firstBatchSize.HasValue },
                { "singleBatch", () => _limit < 0 || _singleBatch.Value, _limit < 0 || _singleBatch.HasValue },
                { "comment", _comment, _comment != null },
                { "maxScan", () => _maxScan.Value, _maxScan.HasValue },
                { "maxTimeMS", () => _maxTime.Value.TotalMilliseconds, _maxTime.HasValue },
                { "max", _max, _max != null },
                { "min", _min, _min != null },
                { "returnKey", () => _returnKey.Value, _returnKey.HasValue },
                { "showRecordId", () => _showRecordId.Value, _showRecordId.HasValue },
                { "snapshot", () => _snapshot.Value, _snapshot.HasValue },
                { "tailable", true, _cursorType == CursorType.Tailable || _cursorType == CursorType.TailableAwait },
                { "oplogReplay", () => _oplogReplay.Value, _oplogReplay.HasValue },
                { "noCursorTimeout", () => _noCursorTimeout.Value, _noCursorTimeout.HasValue },
                { "awaitData", true, _cursorType == CursorType.TailableAwait },
                { "allowPartialResults", () => _allowPartialResults.Value, _allowPartialResults.HasValue && isShardRouter },
                { "readConcern", () => _readConcern.ToBsonDocument(), !_readConcern.IsServerDefault },
                { "collation", () => _collation.ToBsonDocument(), _collation != null }
            };

            return command;
        }

        private AsyncCursor<TDocument> CreateCursor(IChannelSourceHandle channelSource, BsonDocument commandResult, bool slaveOk)
        {
            var getMoreChannelSource = new ServerChannelSource(channelSource.Server);
            var firstBatch = CreateCursorBatch(commandResult);

            return new AsyncCursor<TDocument>(
                getMoreChannelSource,
                _collectionNamespace,
                _filter ?? new BsonDocument(),
                firstBatch.Documents,
                firstBatch.CursorId,
                _batchSize,
                _limit < 0 ? Math.Abs(_limit.Value) : _limit,
                _resultSerializer,
                _messageEncoderSettings,
                _cursorType == CursorType.TailableAwait ? _maxAwaitTime : null);
        }

        private CursorBatch<TDocument> CreateCursorBatch(BsonDocument commandResult)
        {
            var cursorDocument = commandResult["cursor"].AsBsonDocument;
            var cursorId = cursorDocument["id"].ToInt64();
            var batch = (RawBsonArray)cursorDocument["firstBatch"];

            using (batch)
            {
                var documents = CursorBatchDeserializationHelper.DeserializeBatch(batch, _resultSerializer, _messageEncoderSettings);
                return new CursorBatch<TDocument>(cursorId, documents);
            }
        }

        /// <inheritdoc/>
        public IAsyncCursor<TDocument> Execute(IReadBinding binding, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(binding, nameof(binding));

            using (EventContext.BeginOperation())
            using (var channelSource = binding.GetReadChannelSource(cancellationToken))
            using (var channel = channelSource.GetChannel(cancellationToken))
            using (var channelBinding = new ChannelReadBinding(channelSource.Server, channel, binding.ReadPreference))
            {
                var readPreference = binding.ReadPreference;
                var slaveOk = readPreference != null && readPreference.ReadPreferenceMode != ReadPreferenceMode.Primary;

                using (EventContext.BeginFind(_batchSize, _limit))
                {
                    var operation = CreateOperation(channel.ConnectionDescription.ServerVersion, channelSource.ServerDescription.Type);
                    var commandResult = operation.Execute(binding, cancellationToken);
                    return CreateCursor(channelSource, commandResult, slaveOk);
                }
            }
        }

        /// <inheritdoc/>
        public async Task<IAsyncCursor<TDocument>> ExecuteAsync(IReadBinding binding, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(binding, nameof(binding));

            using (EventContext.BeginOperation())
            using (var channelSource = await binding.GetReadChannelSourceAsync(cancellationToken).ConfigureAwait(false))
            using (var channel = await channelSource.GetChannelAsync(cancellationToken).ConfigureAwait(false))
            using (var channelBinding = new ChannelReadBinding(channelSource.Server, channel, binding.ReadPreference))
            {
                var readPreference = binding.ReadPreference;
                var slaveOk = readPreference != null && readPreference.ReadPreferenceMode != ReadPreferenceMode.Primary;

                using (EventContext.BeginFind(_batchSize, _limit))
                {
                    var operation = CreateOperation(channel.ConnectionDescription.ServerVersion, channelSource.ServerDescription.Type);
                    var commandResult = await operation.ExecuteAsync(binding, cancellationToken).ConfigureAwait(false);
                    return CreateCursor(channelSource, commandResult, slaveOk);
                }
            }
        }

        private ReadCommandOperation<BsonDocument> CreateOperation(SemanticVersion serverVersion, ServerType serverType)
        {
            var command = CreateCommand(serverVersion, serverType);
            var operation = new ReadCommandOperation<BsonDocument>(
                _collectionNamespace.DatabaseNamespace,
                command,
                __findCommandResultSerializer,
                _messageEncoderSettings);
            return operation;
        }
    }
}

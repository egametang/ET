/* Copyright 2013-2016 MongoDB Inc.
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
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Core.Bindings;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders;

namespace MongoDB.Driver.Core.Operations
{
    /// <summary>
    /// Represents a Find operation.
    /// </summary>
    /// <typeparam name="TDocument">The type of the returned documents.</typeparam>
    public class FindOperation<TDocument> : IReadOperation<IAsyncCursor<TDocument>>
    {
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
        private int? _maxScan;
        private TimeSpan? _maxAwaitTime;
        private TimeSpan? _maxTime;
        private readonly MessageEncoderSettings _messageEncoderSettings;
        private BsonDocument _min;
        private BsonDocument _modifiers;
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
        /// Initializes a new instance of the <see cref="FindOperation{TDocument}"/> class.
        /// </summary>
        /// <param name="collectionNamespace">The collection namespace.</param>
        /// <param name="resultSerializer">The result serializer.</param>
        /// <param name="messageEncoderSettings">The message encoder settings.</param>
        public FindOperation(
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
            set { _maxScan = value; }
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
            set { _maxTime = value; }
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
        /// Gets or sets the read concern.
        /// </summary>
        /// <value>
        /// The read concern.
        /// </value>
        public ReadConcern ReadConcern
        {
            get { return _readConcern; }
            set { _readConcern = Ensure.IsNotNull(value, "value"); }
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

        // public methods
        /// <inheritdoc/>
        public IAsyncCursor<TDocument> Execute(IReadBinding binding, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(binding, nameof(binding));

            using (var channelSource = binding.GetReadChannelSource(cancellationToken))
            using (var channel = channelSource.GetChannel(cancellationToken))
            using (var channelBinding = new ChannelReadBinding(channelSource.Server, channel, binding.ReadPreference))
            {
                var operation = CreateOperation(channel.ConnectionDescription.ServerVersion);
                return operation.Execute(channelBinding, cancellationToken);
            }
        }

        /// <inheritdoc/>
        public async Task<IAsyncCursor<TDocument>> ExecuteAsync(IReadBinding binding, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(binding, nameof(binding));

            using (var channelSource = await binding.GetReadChannelSourceAsync(cancellationToken).ConfigureAwait(false))
            using (var channel = await channelSource.GetChannelAsync(cancellationToken).ConfigureAwait(false))
            using (var channelBinding = new ChannelReadBinding(channelSource.Server, channel, binding.ReadPreference))
            {
                var operation = CreateOperation(channel.ConnectionDescription.ServerVersion);
                return await operation.ExecuteAsync(channelBinding, cancellationToken).ConfigureAwait(false);
            }
        }

        // private methods
        internal FindCommandOperation<TDocument> CreateFindCommandOperation()
        {
            var comment = _comment;
            var hint = _hint;
            var max = _max;
            var maxScan = _maxScan;
            var maxTime = _maxTime;
            var min = _min;
            var returnKey = _returnKey;
            var showRecordId = _showRecordId;
            var snapshot = _snapshot;
            var sort = _sort;

            if (_modifiers != null)
            {
                foreach (var element in _modifiers)
                {
                    var value = element.Value;
                    switch (element.Name)
                    {
                        case "$comment": comment = _comment ?? value.AsString; break;
                        case "$hint": hint = _hint ?? value; break;
                        case "$max": max = _max ?? value.AsBsonDocument; break;
                        case "$maxScan": maxScan = _maxScan ?? value.ToInt32(); break;
                        case "$maxTimeMS": maxTime = _maxTime ?? TimeSpan.FromMilliseconds(value.ToDouble()); break;
                        case "$min": min = _min ?? value.AsBsonDocument; break;
                        case "$orderby": sort = _sort ?? value.AsBsonDocument; break;
                        case "$returnKey": returnKey = _returnKey ?? value.ToBoolean(); break;
                        case "$showDiskLoc": showRecordId = _showRecordId ?? value.ToBoolean(); break;
                        case "$snapshot": snapshot = _snapshot ?? value.ToBoolean(); break;
                        default: throw new ArgumentException($"Modifier not supported by the Find command: '{element.Name}'.");
                    }
                }
            }

            var operation = new FindCommandOperation<TDocument>(
                _collectionNamespace,
                _resultSerializer,
                _messageEncoderSettings)
            {
                AllowPartialResults = _allowPartialResults,
                BatchSize = _batchSize,
                Collation = _collation,
                Comment = comment,
                CursorType = _cursorType,
                Filter = _filter,
                Hint = hint,
                FirstBatchSize = _firstBatchSize,
                Limit = _limit,
                Max = max,
                MaxAwaitTime = _maxAwaitTime,
                MaxScan = maxScan,
                MaxTime = maxTime,
                Min = min,
                NoCursorTimeout = _noCursorTimeout,
                OplogReplay = _oplogReplay,
                Projection = _projection,
                ReadConcern = _readConcern,
                ReturnKey = returnKey,
                ShowRecordId = showRecordId,
                SingleBatch = _singleBatch,
                Skip = _skip,
                Snapshot = snapshot,
                Sort = sort
            };

            return operation;
        }

        internal FindOpcodeOperation<TDocument> CreateFindOpcodeOperation()
        {
            if (!_readConcern.IsServerDefault)
            {
                throw new MongoClientException($"ReadConcern {_readConcern} is not supported by FindOpcodeOperation.");
            }
            if (_collation != null)
            {
                throw new NotSupportedException($"OP_QUERY does not support collations.");
            }

            var operation = new FindOpcodeOperation<TDocument>(
                _collectionNamespace,
                _resultSerializer,
                _messageEncoderSettings)
            {
                AllowPartialResults = _allowPartialResults,
                BatchSize = _batchSize,
                Comment = _comment,
                CursorType = _cursorType,
                Filter = _filter,
                FirstBatchSize = _firstBatchSize,
                Hint = _hint,
                Limit = (_singleBatch ?? false) ? -Math.Abs(_limit.Value) : _limit,
                Max = _max,
                MaxScan = _maxScan,
                MaxTime = _maxTime,
                Min = _min,
                Modifiers = _modifiers,
                NoCursorTimeout = _noCursorTimeout,
                OplogReplay = _oplogReplay,
                Projection = _projection,
                ShowRecordId = _showRecordId,
                Skip = _skip,
                Snapshot = _snapshot,
                Sort = _sort
            };

            return operation;
        }

        private IReadOperation<IAsyncCursor<TDocument>> CreateOperation(SemanticVersion serverVersion)
        {
            var hasExplainModifier = _modifiers != null && _modifiers.Contains("$explain");
            if (Feature.FindCommand.IsSupported(serverVersion) && !hasExplainModifier)
            {
                return CreateFindCommandOperation();
            }
            else
            {
                return CreateFindOpcodeOperation();
            }
        }
    }
}

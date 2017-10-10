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
using System.Collections.Generic;
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
using MongoDB.Driver.Core.WireProtocol;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders;

namespace MongoDB.Driver.Core.Operations
{
    /// <summary>
    /// Represents an async cursor.
    /// </summary>
    /// <typeparam name="TDocument">The type of the documents.</typeparam>
    public class AsyncCursor<TDocument> : IAsyncCursor<TDocument>
    {
        #region static
        // private static fields
        private static IBsonSerializer<BsonDocument> __getMoreCommandResultSerializer = new PartiallyRawBsonDocumentSerializer(
            "cursor", new PartiallyRawBsonDocumentSerializer(
                "nextBatch", new RawBsonArraySerializer()));
        #endregion

        // fields
        private readonly int? _batchSize;
        private readonly CollectionNamespace _collectionNamespace;
        private readonly IChannelSource _channelSource;
        private int _count;
        private IReadOnlyList<TDocument> _currentBatch;
        private long _cursorId;
        private bool _disposed;
        private IReadOnlyList<TDocument> _firstBatch;
        private readonly int? _limit;
        private readonly TimeSpan? _maxTime;
        private readonly MessageEncoderSettings _messageEncoderSettings;
        private readonly long? _operationId;
        private readonly BsonDocument _query;
        private readonly IBsonSerializer<TDocument> _serializer;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncCursor{TDocument}"/> class.
        /// </summary>
        /// <param name="channelSource">The channel source.</param>
        /// <param name="collectionNamespace">The collection namespace.</param>
        /// <param name="query">The query.</param>
        /// <param name="firstBatch">The first batch.</param>
        /// <param name="cursorId">The cursor identifier.</param>
        /// <param name="batchSize">The size of a batch.</param>
        /// <param name="limit">The limit.</param>
        /// <param name="serializer">The serializer.</param>
        /// <param name="messageEncoderSettings">The message encoder settings.</param>
        /// <param name="maxTime">The maxTime for each batch.</param>
        public AsyncCursor(
            IChannelSource channelSource,
            CollectionNamespace collectionNamespace,
            BsonDocument query,
            IReadOnlyList<TDocument> firstBatch,
            long cursorId,
            int? batchSize,
            int? limit,
            IBsonSerializer<TDocument> serializer,
            MessageEncoderSettings messageEncoderSettings,
            TimeSpan? maxTime = null)
        {
            _operationId = EventContext.OperationId;
            _channelSource = channelSource;
            _collectionNamespace = Ensure.IsNotNull(collectionNamespace, nameof(collectionNamespace));
            _query = Ensure.IsNotNull(query, nameof(query));
            _firstBatch = Ensure.IsNotNull(firstBatch, nameof(firstBatch));
            _cursorId = cursorId;
            _batchSize = Ensure.IsNullOrGreaterThanOrEqualToZero(batchSize, nameof(batchSize));
            _limit = Ensure.IsNullOrGreaterThanOrEqualToZero(limit, nameof(limit));
            _serializer = Ensure.IsNotNull(serializer, nameof(serializer));
            _messageEncoderSettings = messageEncoderSettings;
            _maxTime = maxTime;

            if (_limit > 0 && _firstBatch.Count > _limit)
            {
                _firstBatch = _firstBatch.Take(_limit.Value).ToList();
            }
            _count = _firstBatch.Count;

            // if we aren't going to need the channel source we can go ahead and Dispose it now
            if (_cursorId == 0 && _channelSource != null)
            {
                _channelSource.Dispose();
                _channelSource = null;
            }
        }

        // properties
        /// <inheritdoc/>
        public IEnumerable<TDocument> Current
        {
            get
            {
                ThrowIfDisposed();
                return _currentBatch;
            }
        }

        // methods
        private int CalculateGetMoreProtocolNumberToReturn()
        {
            var numberToReturn = _batchSize ?? 0;
            if (_limit > 0)
            {
                var remaining = _limit.Value - _count;
                if (numberToReturn == 0 || numberToReturn > remaining)
                {
                    numberToReturn = remaining;
                }
            }
            return numberToReturn;
        }

        private CursorBatch<TDocument> CreateCursorBatch(BsonDocument result)
        {
            var cursorDocument = result["cursor"].AsBsonDocument;
            var cursorId = cursorDocument["id"].ToInt64();
            var batch = (RawBsonArray)cursorDocument["nextBatch"];

            using (batch)
            {
                var documents = CursorBatchDeserializationHelper.DeserializeBatch(batch, _serializer, _messageEncoderSettings);
                return new CursorBatch<TDocument>(cursorId, documents);
            }
        }

        private BsonDocument CreateGetMoreCommand()
        {
            var command = new BsonDocument
            {
                { "getMore", _cursorId },
                { "collection", _collectionNamespace.CollectionName },
                { "batchSize", () => _batchSize.Value, _batchSize > 0 },
                { "maxTimeMS", () => _maxTime.Value.TotalMilliseconds, _maxTime.HasValue }
            };

            return command;
        }

        private CursorBatch<TDocument> ExecuteGetMoreCommand(IChannelHandle channel, CancellationToken cancellationToken)
        {
            var command = CreateGetMoreCommand();
            var result = channel.Command<BsonDocument>(
                _collectionNamespace.DatabaseNamespace,
                command,
                NoOpElementNameValidator.Instance,
                () => CommandResponseHandling.Return,
                false, // slaveOk
                __getMoreCommandResultSerializer,
                _messageEncoderSettings,
                cancellationToken);

            return CreateCursorBatch(result);
        }

        private async Task<CursorBatch<TDocument>> ExecuteGetMoreCommandAsync(IChannelHandle channel, CancellationToken cancellationToken)
        {
            var command = CreateGetMoreCommand();
            var result = await channel.CommandAsync<BsonDocument>(
                _collectionNamespace.DatabaseNamespace,
                command,
                NoOpElementNameValidator.Instance,
                () => CommandResponseHandling.Return,
                false, // slaveOk
                __getMoreCommandResultSerializer,
                _messageEncoderSettings,
                cancellationToken).ConfigureAwait(false);

            return CreateCursorBatch(result);
        }

        private CursorBatch<TDocument> ExecuteGetMoreProtocol(IChannelHandle channel, CancellationToken cancellationToken)
        {
            var numberToReturn = CalculateGetMoreProtocolNumberToReturn();

            return channel.GetMore<TDocument>(
                _collectionNamespace,
                _query,
                _cursorId,
                numberToReturn,
                _serializer,
                _messageEncoderSettings,
                cancellationToken);
        }

        private Task<CursorBatch<TDocument>> ExecuteGetMoreProtocolAsync(IChannelHandle channel, CancellationToken cancellationToken)
        {
            var numberToReturn = CalculateGetMoreProtocolNumberToReturn();

            return channel.GetMoreAsync<TDocument>(
                _collectionNamespace,
                _query,
                _cursorId,
                numberToReturn,
                _serializer,
                _messageEncoderSettings,
                cancellationToken);
        }

        private void ExecuteKillCursorsProtocol(IChannelHandle channel, CancellationToken cancellationToken)
        {
            channel.KillCursors(
                new[] { _cursorId },
                _messageEncoderSettings,
                cancellationToken);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!_disposed)
                {
                    try
                    {
                        if (_cursorId != 0)
                        {
                            using (var source = new CancellationTokenSource(TimeSpan.FromSeconds(10)))
                            {
                                KillCursor(_cursorId, source.Token);
                            }
                        }
                    }
                    catch
                    {
                        // ignore exceptions
                    }
                    if (_channelSource != null)
                    {
                        _channelSource.Dispose();
                    }
                }
            }
            _disposed = true;
        }

        private CursorBatch<TDocument> GetNextBatch(CancellationToken cancellationToken)
        {
            using (EventContext.BeginOperation(_operationId))
            using (var channel = _channelSource.GetChannel(cancellationToken))
            {
                if (Feature.FindCommand.IsSupported(channel.ConnectionDescription.ServerVersion))
                {
                    return ExecuteGetMoreCommand(channel, cancellationToken);
                }
                else
                {
                    return ExecuteGetMoreProtocol(channel, cancellationToken);
                }
            }
        }

        private async Task<CursorBatch<TDocument>> GetNextBatchAsync(CancellationToken cancellationToken)
        {
            using (EventContext.BeginOperation(_operationId))
            using (var channel = await _channelSource.GetChannelAsync(cancellationToken).ConfigureAwait(false))
            {
                if (Feature.FindCommand.IsSupported(channel.ConnectionDescription.ServerVersion))
                {
                    return await ExecuteGetMoreCommandAsync(channel, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    return await ExecuteGetMoreProtocolAsync(channel, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        private void KillCursor(long cursorId, CancellationToken cancellationToken)
        {
            try
            {
                using (EventContext.BeginOperation(_operationId))
                using (EventContext.BeginKillCursors(_collectionNamespace))
                using (var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10)))
                using (var channel = _channelSource.GetChannel(cancellationTokenSource.Token))
                {
                    ExecuteKillCursorsProtocol(channel, cancellationToken);
                }
            }
            catch
            {
                // ignore exceptions
            }
        }

        /// <inheritdoc/>
        public bool MoveNext(CancellationToken cancellationToken)
        {
            ThrowIfDisposed();

            bool hasMore;
            if (TryMoveNext(out hasMore))
            {
                return hasMore;
            }

            var batch = GetNextBatch(cancellationToken);
            SaveBatch(batch);
            return true;
        }

        /// <inheritdoc/>
        public async Task<bool> MoveNextAsync(CancellationToken cancellationToken)
        {
            ThrowIfDisposed();

            bool hasMore;
            if (TryMoveNext(out hasMore))
            {
                return hasMore;
            }

            var batch = await GetNextBatchAsync(cancellationToken).ConfigureAwait(false);
            SaveBatch(batch);
            return true;
        }

        private void SaveBatch(CursorBatch<TDocument> batch)
        {
            var documents = batch.Documents;

            _count += documents.Count;
            if (_limit > 0 && _count > _limit.Value)
            {
                var remove = _count - _limit.Value;
                var take = documents.Count - remove;
                documents = documents.Take(take).ToList();
                _count = _limit.Value;
            }

            _currentBatch = documents;
            _cursorId = batch.CursorId;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        private bool TryMoveNext(out bool hasMore)
        {
            hasMore = false;

            if (_firstBatch != null)
            {
                _currentBatch = _firstBatch;
                _firstBatch = null;
                hasMore = true;
                return true;
            }

            if (_currentBatch == null)
            {
                return true;
            }

            if (_cursorId == 0 || (_limit > 0 && _count == _limit.Value))
            {
                _currentBatch = null;
                return true;
            }

            return false;
        }
    }
}

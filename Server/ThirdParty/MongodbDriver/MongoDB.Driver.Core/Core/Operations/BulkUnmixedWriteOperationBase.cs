/* Copyright 2010-2016 MongoDB Inc.
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
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Events;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.WireProtocol;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders;

namespace MongoDB.Driver.Core.Operations
{
    internal abstract class BulkUnmixedWriteOperationBase : IWriteOperation<BulkWriteOperationResult>
    {
        // fields
        private bool? _bypassDocumentValidation;
        private CollectionNamespace _collectionNamespace;
        private bool _isOrdered = true;
        private int? _maxBatchCount;
        private int? _maxBatchLength;
        private MessageEncoderSettings _messageEncoderSettings;
        private IEnumerable<WriteRequest> _requests;
        private WriteConcern _writeConcern = WriteConcern.Acknowledged;

        // constructors
        protected BulkUnmixedWriteOperationBase(
            CollectionNamespace collectionNamespace,
            IEnumerable<WriteRequest> requests,
            MessageEncoderSettings messageEncoderSettings)
        {
            _collectionNamespace = Ensure.IsNotNull(collectionNamespace, nameof(collectionNamespace));
            _requests = Ensure.IsNotNull(requests, nameof(requests));
            _messageEncoderSettings = messageEncoderSettings;
        }

        // properties
        public bool? BypassDocumentValidation
        {
            get { return _bypassDocumentValidation; }
            set { _bypassDocumentValidation = value; }
        }

        public CollectionNamespace CollectionNamespace
        {
            get { return _collectionNamespace; }
        }

        protected abstract string CommandName { get; }

        public int? MaxBatchCount
        {
            get { return _maxBatchCount; }
            set { _maxBatchCount = Ensure.IsNullOrGreaterThanZero(value, nameof(value)); }
        }

        public int? MaxBatchLength
        {
            get { return _maxBatchLength; }
            set { _maxBatchLength = Ensure.IsNullOrGreaterThanZero(value, nameof(value)); }
        }

        public MessageEncoderSettings MessageEncoderSettings
        {
            get { return _messageEncoderSettings; }
            set { _messageEncoderSettings = value; }
        }

        public bool IsOrdered
        {
            get { return _isOrdered; }
            set { _isOrdered = value; }
        }

        public IEnumerable<WriteRequest> Requests
        {
            get { return _requests; }
        }

        protected abstract string RequestsElementName { get; }

        public WriteConcern WriteConcern
        {
            get { return _writeConcern; }
            set { _writeConcern = Ensure.IsNotNull(value, nameof(value)); }
        }

        // public methods
        public BulkWriteOperationResult Execute(IChannelHandle channel, CancellationToken cancellationToken)
        {
            using (EventContext.BeginOperation())
            {
                if (Feature.WriteCommands.IsSupported(channel.ConnectionDescription.ServerVersion))
                {
                    return ExecuteBatches(channel, cancellationToken);
                }
                else
                {
                    var emulator = CreateEmulator();
                    return emulator.Execute(channel, cancellationToken);
                }
            }
        }

        public BulkWriteOperationResult Execute(IWriteBinding binding, CancellationToken cancellationToken)
        {
            using (var channelSource = binding.GetWriteChannelSource(cancellationToken))
            using (var channel = channelSource.GetChannel(cancellationToken))
            {
                return Execute(channel, cancellationToken);
            }
        }

        public async Task<BulkWriteOperationResult> ExecuteAsync(IChannelHandle channel, CancellationToken cancellationToken)
        {
            using (EventContext.BeginOperation())
            {
                if (Feature.WriteCommands.IsSupported(channel.ConnectionDescription.ServerVersion))
                {
                    return await ExecuteBatchesAsync(channel, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    var emulator = CreateEmulator();
                    return await emulator.ExecuteAsync(channel, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        public async Task<BulkWriteOperationResult> ExecuteAsync(IWriteBinding binding, CancellationToken cancellationToken)
        {
            using (var channelSource = await binding.GetWriteChannelSourceAsync(cancellationToken).ConfigureAwait(false))
            using (var channel = await channelSource.GetChannelAsync(cancellationToken).ConfigureAwait(false))
            {
                return await ExecuteAsync(channel, cancellationToken).ConfigureAwait(false);
            }
        }

        // private methods
        private BsonDocument CreateBatchCommand(IChannelHandle channel, BatchableSource<WriteRequest> requestSource)
        {
            var maxBatchCount = Math.Min(_maxBatchCount ?? int.MaxValue, channel.ConnectionDescription.MaxBatchCount);
            var maxBatchLength = Math.Min(_maxBatchLength ?? int.MaxValue, channel.ConnectionDescription.MaxDocumentSize);
            var batchSerializer = CreateBatchSerializer(channel.ConnectionDescription, maxBatchCount, maxBatchLength);
            return CreateWriteCommand(batchSerializer, requestSource, channel.ConnectionDescription.ServerVersion);
        }

        private BulkWriteBatchResult CreateBatchResult(BatchableSource<WriteRequest> requestSource, int originalIndex, BsonDocument writeCommandResult)
        {
            var indexMap = new IndexMap.RangeBased(0, originalIndex, requestSource.Batch.Count);
            return BulkWriteBatchResult.Create(
                _isOrdered,
                requestSource.Batch,
                writeCommandResult,
                indexMap);
        }

        protected abstract BatchSerializer CreateBatchSerializer(ConnectionDescription connectionDescription, int maxBatchCount, int maxBatchLength);

        protected abstract BulkUnmixedWriteOperationEmulatorBase CreateEmulator();

        private BsonDocument CreateWriteCommand(BatchSerializer batchSerializer, BatchableSource<WriteRequest> requestSource, SemanticVersion serverVersion)
        {
            var batchWrapper = new BsonDocumentWrapper(requestSource, batchSerializer);

            WriteConcern effectiveWriteConcern = _writeConcern;
            if (!effectiveWriteConcern.IsAcknowledged && _isOrdered)
            {
                effectiveWriteConcern = WriteConcern.W1; // ignore the server's default, whatever it may be.
            }

            return new BsonDocument
            {
                { CommandName, _collectionNamespace.CollectionName },
                { "writeConcern", () => effectiveWriteConcern.ToBsonDocument(), !effectiveWriteConcern.IsServerDefault },
                { "ordered", _isOrdered },
                { "bypassDocumentValidation", () => _bypassDocumentValidation.Value, _bypassDocumentValidation.HasValue && Feature.BypassDocumentValidation.IsSupported(serverVersion) },
                { RequestsElementName, new BsonArray { batchWrapper } } // should be last
            };
        }

        protected virtual IEnumerable<WriteRequest> DecorateRequests(IEnumerable<WriteRequest> requests)
        {
            return requests;
        }

        private BulkWriteBatchResult ExecuteBatch(IChannelHandle channel, BatchableSource<WriteRequest> requestSource, int originalIndex, CancellationToken cancellationToken)
        {
            var writeCommand = CreateBatchCommand(channel, requestSource);
            var writeCommandResult = ExecuteProtocol(channel, writeCommand, () => GetResponseHandling(requestSource), cancellationToken);
            return CreateBatchResult(requestSource, originalIndex, writeCommandResult);
        }

        private async Task<BulkWriteBatchResult> ExecuteBatchAsync(IChannelHandle channel, BatchableSource<WriteRequest> requestSource, int originalIndex, CancellationToken cancellationToken)
        {
            var writeCommand = CreateBatchCommand(channel, requestSource);
            var writeCommandResult = await ExecuteProtocolAsync(channel, writeCommand, () => GetResponseHandling(requestSource), cancellationToken).ConfigureAwait(false);
            return CreateBatchResult(requestSource, originalIndex, writeCommandResult);
        }

        private BulkWriteOperationResult ExecuteBatches(IChannelHandle channel, CancellationToken cancellationToken)
        {
            var decoratedRequests = DecorateRequests(_requests);
            var helper = new BatchHelper(decoratedRequests, _writeConcern, _isOrdered);
            foreach (var batch in helper.Batches)
            {
                batch.Result = ExecuteBatch(channel, batch.RequestSource, batch.OriginalIndex, cancellationToken);
            }
            return helper.CreateFinalResultOrThrow(channel);
        }

        private async Task<BulkWriteOperationResult> ExecuteBatchesAsync(IChannelHandle channel, CancellationToken cancellationToken)
        {
            var decoratedRequests = DecorateRequests(_requests);
            var helper = new BatchHelper(decoratedRequests, _writeConcern, _isOrdered);
            foreach (var batch in helper.Batches)
            {
                batch.Result = await ExecuteBatchAsync(channel, batch.RequestSource, batch.OriginalIndex, cancellationToken).ConfigureAwait(false);
            }
            return helper.CreateFinalResultOrThrow(channel);
        }

        private BsonDocument ExecuteProtocol(IChannelHandle channel, BsonDocument command, Func<CommandResponseHandling> responseHandling, CancellationToken cancellationToken)
        {
            return channel.Command<BsonDocument>(
                _collectionNamespace.DatabaseNamespace,
                command,
                NoOpElementNameValidator.Instance,
                responseHandling,
                false, // slaveOk
                BsonDocumentSerializer.Instance,
                _messageEncoderSettings,
                cancellationToken) ?? new BsonDocument("ok", 1);
        }

        private async Task<BsonDocument> ExecuteProtocolAsync(IChannelHandle channel, BsonDocument command, Func<CommandResponseHandling> responseHandling, CancellationToken cancellationToken)
        {
            return (await channel.CommandAsync<BsonDocument>(
                _collectionNamespace.DatabaseNamespace,
                command,
                NoOpElementNameValidator.Instance,
                responseHandling,
                false, // slaveOk
                BsonDocumentSerializer.Instance,
                _messageEncoderSettings,
                cancellationToken).ConfigureAwait(false)) ?? new BsonDocument("ok", 1);
        }

        private CommandResponseHandling GetResponseHandling(BatchableSource<WriteRequest> source)
        {
            if (_writeConcern.IsAcknowledged || source.HasMore)
            {
                return CommandResponseHandling.Return;
            }

            return CommandResponseHandling.Ignore;
        }

        // nested types
        /// <summary>
        /// 
        /// </summary>
        private class BatchHelper
        {
            private readonly List<BulkWriteBatchResult> _batchResults = new List<BulkWriteBatchResult>();
            private bool _hasWriteErrors;
            private readonly bool _isOrdered;
            private IEnumerable<WriteRequest> _remainingRequests = Enumerable.Empty<WriteRequest>();
            private readonly IEnumerable<WriteRequest> _requests;
            private readonly WriteConcern _writeConcern;

            public BatchHelper(IEnumerable<WriteRequest> requests, WriteConcern writeConcern, bool isOrdered)
            {
                _requests = requests;
                _writeConcern = writeConcern;
                _isOrdered = isOrdered;
            }

            public IEnumerable<Batch> Batches
            {
                get
                {
                    using (var enumerator = _requests.GetEnumerator())
                    {
                        var originalIndex = 0;

                        var requestSource = new BatchableSource<WriteRequest>(enumerator);
                        while (requestSource.HasMore)
                        {
                            if (_hasWriteErrors && _isOrdered)
                            {
                                // note: we have to materialize the list of remaining items before the enumerator gets Disposed
                                _remainingRequests = _remainingRequests.Concat(requestSource.GetRemainingItems()).ToList();
                                break;
                            }

                            var batch = new Batch { RequestSource = requestSource, OriginalIndex = originalIndex };
                            yield return batch;
                            _batchResults.Add(batch.Result);
                            _hasWriteErrors |= batch.Result.HasWriteErrors;
                            originalIndex += batch.Result.BatchCount;

                            requestSource.ClearBatch();
                        }
                    }
                }
            }

            public BulkWriteOperationResult CreateFinalResultOrThrow(IChannelHandle channel)
            {
                var combiner = new BulkWriteBatchResultCombiner(_batchResults, _writeConcern.IsAcknowledged);
                return combiner.CreateResultOrThrowIfHasErrors(channel.ConnectionDescription.ConnectionId, _remainingRequests.ToList());
            }

            public class Batch
            {
                public int OriginalIndex;
                public BatchableSource<WriteRequest> RequestSource;
                public BulkWriteBatchResult Result;
            }
        }

        protected abstract class BatchSerializer : SerializerBase<BatchableSource<WriteRequest>>
        {
            // fields
            private int _batchCount;
            private int _batchLength;
            private int _batchStartPosition;
            private int _lastRequestPosition;
            private readonly ConnectionDescription _connectionDescription;
            private readonly int _maxBatchCount;
            private readonly int _maxBatchLength;

            // constructors
            public BatchSerializer(ConnectionDescription connectionDescription,  int maxBatchCount, int maxBatchLength)
            {
                _connectionDescription = connectionDescription;
                _maxBatchCount = maxBatchCount;
                _maxBatchLength = maxBatchLength;
            }

            // properties
            protected ConnectionDescription ConnectionDescription
            {
                get { return _connectionDescription; }
            }

            protected int MaxBatchCount
            {
                get { return _maxBatchCount; }
            }

            protected int MaxBatchLength
            {
                get { return _maxBatchLength; }
            }

            // methods
            private void AddRequest(BsonSerializationContext context, IByteBuffer overflow)
            {
                var bsonBinaryWriter = (BsonBinaryWriter)context.Writer;
                var stream = bsonBinaryWriter.BsonStream;
                _lastRequestPosition = (int)stream.Position;
                bsonBinaryWriter.WriteRawBsonDocument(overflow);
                _batchCount++;
                _batchLength = (int)stream.Position - _batchStartPosition;
            }

            private void AddRequest(BsonSerializationContext context, WriteRequest request)
            {
                var bsonBinaryWriter = (BsonBinaryWriter)context.Writer;
                var stream = bsonBinaryWriter.BsonStream;
                _lastRequestPosition = (int)stream.Position;
                SerializeRequest(context, request);
                _batchCount++;
                _batchLength = (int)stream.Position - _batchStartPosition;
            }

            private IByteBuffer RemoveLastRequest(BsonSerializationContext context)
            {
                var bsonBinaryWriter = (BsonBinaryWriter)context.Writer;
                var stream = bsonBinaryWriter.BsonStream;
                var lastRequestLength = (int)stream.Position - _lastRequestPosition;
                stream.Position = _lastRequestPosition;
                var lastRequest = new byte[lastRequestLength];
                stream.ReadBytes(lastRequest, 0, lastRequestLength);
                stream.Position = _lastRequestPosition;
                stream.SetLength(_lastRequestPosition);
                _batchCount--;
                _batchLength = (int)stream.Position - _batchStartPosition;

                if ((BsonType)lastRequest[0] != BsonType.Document)
                {
                    throw new MongoInternalException("Expected overflow item to be a BsonDocument.");
                }
                var sliceOffset = Array.IndexOf<byte>(lastRequest, 0) + 1; // skip over type and array index

                var buffer = new ByteArrayBuffer(lastRequest, isReadOnly: true);
                return new ByteBufferSlice(buffer, sliceOffset, lastRequest.Length - sliceOffset);
            }

            public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, BatchableSource<WriteRequest> requestSource)
            {
                if (requestSource.Batch == null)
                {
                    SerializeNextBatch(context, requestSource);
                }
                else
                {
                    SerializeSingleBatch(context, requestSource);
                }
            }

            private void SerializeNextBatch(BsonSerializationContext context, BatchableSource<WriteRequest> requestSource)
            {
                var batch = new List<WriteRequest>();

                var bsonBinaryWriter = (BsonBinaryWriter)context.Writer;
                _batchStartPosition = (int)bsonBinaryWriter.BsonStream.Position;

                var overflow = requestSource.StartBatch();
                if (overflow != null)
                {
                    AddRequest(context, (IByteBuffer)overflow.State);
                    batch.Add(overflow.Item);
                }

                // always go one document too far so that we can set IsDone as early as possible
                while (requestSource.MoveNext())
                {
                    var request = requestSource.Current;
                    AddRequest(context, request);

                    if ((_batchCount > _maxBatchCount || _batchLength > _maxBatchLength) && _batchCount > 1)
                    {
                        var serializedRequest = RemoveLastRequest(context);
                        overflow = new BatchableSource<WriteRequest>.Overflow { Item = request, State = serializedRequest };
                        requestSource.EndBatch(batch, overflow);
                        return;
                    }

                    batch.Add(request);
                }

                requestSource.EndBatch(batch);
            }

            private void SerializeSingleBatch(BsonSerializationContext context, BatchableSource<WriteRequest> requestSource)
            {
                var bsonBinaryWriter = (BsonBinaryWriter)context.Writer;
                _batchStartPosition = (int)bsonBinaryWriter.BsonStream.Position;

                // always go one document too far so that we can set IsDone as early as possible
                foreach (var request in requestSource.Batch)
                {
                    AddRequest(context, request);

                    if ((_batchCount > _maxBatchCount || _batchLength > _maxBatchLength) && _batchCount > 1)
                    {
                        throw new ArgumentException("The non-batchable requests do not fit in a single write command.");
                    }
                }
            }

            protected abstract void SerializeRequest(BsonSerializationContext context, WriteRequest request);
        }
    }
}

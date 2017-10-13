/* Copyright 2010-2015 MongoDB Inc.
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
using MongoDB.Driver.Core.Bindings;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Events;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders;

namespace MongoDB.Driver.Core.Operations
{
    /// <summary>
    /// Represents a mixed write bulk operation.
    /// </summary>
    public class BulkMixedWriteOperation : IWriteOperation<BulkWriteOperationResult>
    {
        // fields
        private bool? _bypassDocumentValidation;
        private readonly CollectionNamespace _collectionNamespace;
        private bool _isOrdered = true;
        private int? _maxBatchCount;
        private int? _maxBatchLength;
        private int? _maxDocumentSize;
        private int? _maxWireDocumentSize;
        private readonly MessageEncoderSettings _messageEncoderSettings;
        private readonly IEnumerable<WriteRequest> _requests;
        private WriteConcern _writeConcern;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="BulkMixedWriteOperation"/> class.
        /// </summary>
        /// <param name="collectionNamespace">The collection namespace.</param>
        /// <param name="requests">The requests.</param>
        /// <param name="messageEncoderSettings">The message encoder settings.</param>
        public BulkMixedWriteOperation(
            CollectionNamespace collectionNamespace,
            IEnumerable<WriteRequest> requests,
            MessageEncoderSettings messageEncoderSettings)
        {
            _collectionNamespace = Ensure.IsNotNull(collectionNamespace, nameof(collectionNamespace));
            _requests = Ensure.IsNotNull(requests, nameof(requests));
            _messageEncoderSettings = Ensure.IsNotNull(messageEncoderSettings, nameof(messageEncoderSettings));
            _writeConcern = WriteConcern.Acknowledged;
        }

        // properties
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
        /// Gets or sets a value indicating whether the writes must be performed in order.
        /// </summary>
        /// <value>
        /// <c>true</c> if the writes must be performed in order; otherwise, <c>false</c>.
        /// </value>
        public bool IsOrdered
        {
            get { return _isOrdered; }
            set { _isOrdered = value; }
        }

        /// <summary>
        /// Gets or sets the maximum number of documents in a batch.
        /// </summary>
        /// <value>
        /// The maximum number of documents in a batch.
        /// </value>
        public int? MaxBatchCount
        {
            get { return _maxBatchCount; }
            set { _maxBatchCount = Ensure.IsNullOrGreaterThanZero(value, nameof(value)); }
        }

        /// <summary>
        /// Gets or sets the maximum length of a batch.
        /// </summary>
        /// <value>
        /// The maximum length of a batch.
        /// </value>
        public int? MaxBatchLength
        {
            get { return _maxBatchLength; }
            set { _maxBatchLength = Ensure.IsNullOrGreaterThanZero(value, nameof(value)); }
        }

        /// <summary>
        /// Gets or sets the maximum size of a document.
        /// </summary>
        /// <value>
        /// The maximum size of a document.
        /// </value>
        public int? MaxDocumentSize
        {
            get { return _maxDocumentSize; }
            set { _maxDocumentSize = Ensure.IsNullOrGreaterThanZero(value, nameof(value)); }
        }

        /// <summary>
        /// Gets or sets the maximum size of a wire document.
        /// </summary>
        /// <value>
        /// The maximum size of a wire document.
        /// </value>
        public int? MaxWireDocumentSize
        {
            get { return _maxWireDocumentSize; }
            set { _maxWireDocumentSize = Ensure.IsNullOrGreaterThanZero(value, nameof(value)); }
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
        /// Gets the requests.
        /// </summary>
        /// <value>
        /// The requests.
        /// </value>
        public IEnumerable<WriteRequest> Requests
        {
            get { return _requests; }
        }

        /// <summary>
        /// Gets or sets the write concern.
        /// </summary>
        /// <value>
        /// The write concern.
        /// </value>
        public WriteConcern WriteConcern
        {
            get { return _writeConcern; }
            set { _writeConcern = Ensure.IsNotNull(value, nameof(value)); }
        }

        // public methods
        /// <inheritdoc/>
        public BulkWriteOperationResult Execute(IWriteBinding binding, CancellationToken cancellationToken)
        {
            using (EventContext.BeginOperation())
            using (var channelSource = binding.GetWriteChannelSource(cancellationToken))
            using (var channel = channelSource.GetChannel(cancellationToken))
            {
                var helper = new BatchHelper(this, channel);
                foreach (var batch in helper.GetBatches())
                {
                    batch.Result = ExecuteBatch(channel, batch.Run, batch.IsLast, cancellationToken);
                }
                return helper.GetFinalResultOrThrow();
            }
        }

        /// <inheritdoc/>
        public async Task<BulkWriteOperationResult> ExecuteAsync(IWriteBinding binding, CancellationToken cancellationToken)
        {
            using (EventContext.BeginOperation())
            using (var channelSource = await binding.GetWriteChannelSourceAsync(cancellationToken).ConfigureAwait(false))
            using (var channel = await channelSource.GetChannelAsync(cancellationToken).ConfigureAwait(false))
            {
                var helper = new BatchHelper(this, channel);
                foreach (var batch in helper.GetBatches())
                {
                    batch.Result = await ExecuteBatchAsync(channel, batch.Run, batch.IsLast, cancellationToken).ConfigureAwait(false);
                }
                return helper.GetFinalResultOrThrow();
            }
        }

        // private methods
        private BulkDeleteOperation CreateDeleteOperation(IEnumerable<DeleteRequest> requests, bool isLast)
        {
            return new BulkDeleteOperation(_collectionNamespace, requests, _messageEncoderSettings)
            {
                MaxBatchCount = _maxBatchCount,
                MaxBatchLength = _maxBatchLength,
                WriteConcern = GetEffectiveWriteConcern(isLast)
            };
        }

        private BulkInsertOperation CreateInsertOperation(IEnumerable<InsertRequest> requests, bool isLast)
        {
            return new BulkInsertOperation(_collectionNamespace, requests, _messageEncoderSettings)
            {
                BypassDocumentValidation = _bypassDocumentValidation,
                MaxBatchCount = _maxBatchCount,
                MaxBatchLength = _maxBatchLength,
                IsOrdered = _isOrdered,
                MessageEncoderSettings = _messageEncoderSettings,
                WriteConcern = GetEffectiveWriteConcern(isLast)
            };
        }

        private BulkUpdateOperation CreateUpdateOperation(IEnumerable<UpdateRequest> requests, bool isLast)
        {
            return new BulkUpdateOperation(_collectionNamespace, requests, _messageEncoderSettings)
            {
                BypassDocumentValidation = _bypassDocumentValidation,
                MaxBatchCount = _maxBatchCount,
                MaxBatchLength = _maxBatchLength,
                IsOrdered = _isOrdered,
                WriteConcern = GetEffectiveWriteConcern(isLast)
            };
        }

        private BulkWriteBatchResult ExecuteBatch(IChannelHandle channel, Run run, bool isLast, CancellationToken cancellationToken)
        {
            BulkWriteOperationResult result;
            MongoBulkWriteOperationException exception = null;
            try
            {
                switch (run.RequestType)
                {
                    case WriteRequestType.Delete:
                        result = ExecuteDeletes(channel, run.Requests.Cast<DeleteRequest>(), isLast, cancellationToken);
                        break;
                    case WriteRequestType.Insert:
                        result = ExecuteInserts(channel, run.Requests.Cast<InsertRequest>(), isLast, cancellationToken);
                        break;
                    case WriteRequestType.Update:
                        result = ExecuteUpdates(channel, run.Requests.Cast<UpdateRequest>(), isLast, cancellationToken);
                        break;
                    default:
                        throw new MongoInternalException("Unrecognized RequestType.");
                }
            }
            catch (MongoBulkWriteOperationException ex)
            {
                result = ex.Result;
                exception = ex;
            }

            return BulkWriteBatchResult.Create(result, exception, run.IndexMap);
        }

        private async Task<BulkWriteBatchResult> ExecuteBatchAsync(IChannelHandle channel, Run run, bool isLast, CancellationToken cancellationToken)
        {
            BulkWriteOperationResult result;
            MongoBulkWriteOperationException exception = null;
            try
            {
                switch (run.RequestType)
                {
                    case WriteRequestType.Delete:
                        result = await ExecuteDeletesAsync(channel, run.Requests.Cast<DeleteRequest>(), isLast, cancellationToken).ConfigureAwait(false);
                        break;
                    case WriteRequestType.Insert:
                        result = await ExecuteInsertsAsync(channel, run.Requests.Cast<InsertRequest>(), isLast, cancellationToken).ConfigureAwait(false);
                        break;
                    case WriteRequestType.Update:
                        result = await ExecuteUpdatesAsync(channel, run.Requests.Cast<UpdateRequest>(), isLast, cancellationToken).ConfigureAwait(false);
                        break;
                    default:
                        throw new MongoInternalException("Unrecognized RequestType.");
                }
            }
            catch (MongoBulkWriteOperationException ex)
            {
                result = ex.Result;
                exception = ex;
            }

            return BulkWriteBatchResult.Create(result, exception, run.IndexMap);
        }

        private BulkWriteOperationResult ExecuteDeletes(IChannelHandle channel, IEnumerable<DeleteRequest> requests, bool isLast, CancellationToken cancellationToken)
        {
            var operation = CreateDeleteOperation(requests, isLast);
            return operation.Execute(channel, cancellationToken);
        }

        private Task<BulkWriteOperationResult> ExecuteDeletesAsync(IChannelHandle channel, IEnumerable<DeleteRequest> requests, bool isLast, CancellationToken cancellationToken)
        {
            var operation = CreateDeleteOperation(requests, isLast);
            return operation.ExecuteAsync(channel, cancellationToken);
        }

        private BulkWriteOperationResult ExecuteInserts(IChannelHandle channel, IEnumerable<InsertRequest> requests, bool isLast, CancellationToken cancellationToken)
        {
            var operation = CreateInsertOperation(requests, isLast);
            return operation.Execute(channel, cancellationToken);
        }

        private Task<BulkWriteOperationResult> ExecuteInsertsAsync(IChannelHandle channel, IEnumerable<InsertRequest> requests, bool isLast, CancellationToken cancellationToken)
        {
            var operation = CreateInsertOperation(requests, isLast);
            return operation.ExecuteAsync(channel, cancellationToken);
        }

        private BulkWriteOperationResult ExecuteUpdates(IChannelHandle channel, IEnumerable<UpdateRequest> requests, bool isLast, CancellationToken cancellationToken)
        {
            var operation = CreateUpdateOperation(requests, isLast);
            return operation.Execute(channel, cancellationToken);
        }

        private Task<BulkWriteOperationResult> ExecuteUpdatesAsync(IChannelHandle channel, IEnumerable<UpdateRequest> requests, bool isLast, CancellationToken cancellationToken)
        {
            var operation = CreateUpdateOperation(requests, isLast);
            return operation.ExecuteAsync(channel, cancellationToken);
        }

        private WriteConcern GetEffectiveWriteConcern(bool isLast)
        {
            if (_isOrdered && !isLast && !_writeConcern.IsAcknowledged)
            {
                return WriteConcern.W1; // explicitly do not use the server's default.
            }

            return _writeConcern;
        }

        // nested types
        private class BatchHelper
        {
            private readonly List<BulkWriteBatchResult> _batchResults = new List<BulkWriteBatchResult>();
            private readonly IChannelHandle _channel;
            private bool _hasWriteErrors;
            private readonly BulkMixedWriteOperation _operation;
            private IEnumerable<WriteRequest> _remainingRequests = Enumerable.Empty<WriteRequest>();

            public BatchHelper(BulkMixedWriteOperation operation, IChannelHandle channel)
            {
                _operation = operation;
                _channel = channel;
            }

            public IEnumerable<Batch> GetBatches()
            {
                var runCount = 0;
                var maxRunLength = Math.Min(_operation._maxBatchCount ?? int.MaxValue, _channel.ConnectionDescription.MaxBatchCount);
                using (var runsEnumerator = (ReadAheadEnumerable<Run>.ReadAheadEnumerator)new ReadAheadEnumerable<Run>(FindRuns(maxRunLength)).GetEnumerator())
                {
                    while (runsEnumerator.MoveNext())
                    {
                        runCount++;

                        if (_hasWriteErrors && _operation._isOrdered)
                        {
                            _remainingRequests = _remainingRequests.Concat(runsEnumerator.Current.Requests);
                            continue;
                        }

                        var batch = new Batch
                        {
                            IsLast = !runsEnumerator.HasNext,
                            Run = runsEnumerator.Current
                        };
                        yield return batch;
                        _batchResults.Add(batch.Result);

                        _hasWriteErrors |= batch.Result.HasWriteErrors;
                    }

                    if (runCount == 0)
                    {
                        throw new InvalidOperationException("Bulk write operation is empty.");
                    }
                }
            }

            public BulkWriteOperationResult GetFinalResultOrThrow()
            {
                var combiner = new BulkWriteBatchResultCombiner(_batchResults, _operation._writeConcern.IsAcknowledged);
                return combiner.CreateResultOrThrowIfHasErrors(_channel.ConnectionDescription.ConnectionId, _remainingRequests.ToList());
            }

            private IEnumerable<Run> FindOrderedRuns(int maxRunLength)
            {
                Run run = null;

                var originalIndex = 0;
                foreach (var request in _operation._requests)
                {
                    if (run == null)
                    {
                        run = new Run();
                        run.Add(request, originalIndex);
                    }
                    else if (run.RequestType == request.RequestType)
                    {
                        if (run.Count == maxRunLength)
                        {
                            yield return run;
                            run = new Run();
                        }
                        run.Add(request, originalIndex);
                    }
                    else
                    {
                        yield return run;
                        run = new Run();
                        run.Add(request, originalIndex);
                    }

                    originalIndex++;
                }

                if (run != null)
                {
                    yield return run;
                }
            }

            private IEnumerable<Run> FindRuns(int maxRunLength)
            {
                if (_operation._isOrdered)
                {
                    return FindOrderedRuns(maxRunLength);
                }
                else
                {
                    return FindUnorderedRuns(maxRunLength);
                }
            }

            private IEnumerable<Run> FindUnorderedRuns(int maxRunLength)
            {
                var runs = new List<Run>();

                var originalIndex = 0;
                foreach (var request in _operation._requests)
                {
                    var run = runs.FirstOrDefault(r => r.RequestType == request.RequestType);

                    if (run == null)
                    {
                        run = new Run();
                        runs.Add(run);
                    }
                    else if (run.Count == maxRunLength)
                    {
                        yield return run;
                        runs.Remove(run);
                        run = new Run();
                        runs.Add(run);
                    }

                    run.Add(request, originalIndex);
                    originalIndex++;
                }

                foreach (var run in runs)
                {
                    yield return run;
                }
            }

            public class Batch
            {
                public bool IsLast;
                public BulkWriteBatchResult Result;
                public Run Run;
            }
        }

        private class Run
        {
            // fields
            private IndexMap _indexMap = new IndexMap.RangeBased();
            private readonly List<WriteRequest> _requests = new List<WriteRequest>();

            // properties
            public int Count
            {
                get { return _requests.Count; }
            }

            public IndexMap IndexMap
            {
                get { return _indexMap; }
            }

            public List<WriteRequest> Requests
            {
                get { return _requests; }
            }

            public WriteRequestType RequestType
            {
                get { return _requests[0].RequestType; }
            }

            // methods
            public void Add(WriteRequest request, int originalIndex)
            {
                var index = _requests.Count;
                _indexMap = _indexMap.Add(index, originalIndex);
                _requests.Add(request);
            }
        }
    }
}

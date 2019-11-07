/* Copyright 2010-present MongoDB Inc.
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
        private readonly List<WriteRequest> _requests;
        private bool _retryRequested;
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
            : this(collectionNamespace, Ensure.IsNotNull(requests, nameof(requests)).ToList(), messageEncoderSettings)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BulkMixedWriteOperation"/> class.
        /// </summary>
        /// <param name="collectionNamespace">The collection namespace.</param>
        /// <param name="requests">The requests.</param>
        /// <param name="messageEncoderSettings">The message encoder settings.</param>
        public BulkMixedWriteOperation(
            CollectionNamespace collectionNamespace,
            List<WriteRequest> requests,
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
        /// Gets or sets a value indicating whether [retry requested].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [retry requested]; otherwise, <c>false</c>.
        /// </value>
        public bool RetryRequested
        {
            get { return _retryRequested; }
            set { _retryRequested = value; }
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
            using (var context = RetryableWriteContext.Create(binding, _retryRequested, cancellationToken))
            {
                EnsureCollationIsSupportedIfAnyRequestHasCollation(context);
                context.DisableRetriesIfAnyWriteRequestIsNotRetryable(_requests);
                var helper = new BatchHelper(_requests, _isOrdered, _writeConcern);
                foreach (var batch in helper.GetBatches())
                {
                    batch.Result = ExecuteBatch(context, batch, cancellationToken);
                }
                return helper.GetFinalResultOrThrow(context.Channel.ConnectionDescription.ConnectionId);
            }
        }

        /// <inheritdoc/>
        public async Task<BulkWriteOperationResult> ExecuteAsync(IWriteBinding binding, CancellationToken cancellationToken)
        {
            using (EventContext.BeginOperation())
            using (var context = await RetryableWriteContext.CreateAsync(binding, _retryRequested, cancellationToken).ConfigureAwait(false))
            {
                EnsureCollationIsSupportedIfAnyRequestHasCollation(context);
                context.DisableRetriesIfAnyWriteRequestIsNotRetryable(_requests);
                var helper = new BatchHelper(_requests, _isOrdered, _writeConcern);
                foreach (var batch in helper.GetBatches())
                {
                    batch.Result = await ExecuteBatchAsync(context, batch, cancellationToken).ConfigureAwait(false);
                }
                return helper.GetFinalResultOrThrow(context.Channel.ConnectionDescription.ConnectionId);
            }
        }

        // private methods
        private IExecutableInRetryableWriteContext<BulkWriteOperationResult> CreateBulkDeleteOperation(Batch batch)
        {
            var requests = batch.Requests.Cast<DeleteRequest>();
            return new BulkDeleteOperation(_collectionNamespace, requests, _messageEncoderSettings)
            {
                MaxBatchCount = _maxBatchCount,
                MaxBatchLength = _maxBatchLength,
                WriteConcern = batch.WriteConcern,
                RetryRequested = _retryRequested
            };
        }

        private IExecutableInRetryableWriteContext<BulkWriteOperationResult> CreateBulkInsertOperation(Batch batch)
        {
            var requests = batch.Requests.Cast<InsertRequest>();
            return new BulkInsertOperation(_collectionNamespace, requests, _messageEncoderSettings)
            {
                BypassDocumentValidation = _bypassDocumentValidation,
                IsOrdered = _isOrdered,
                MaxBatchCount = _maxBatchCount,
                MaxBatchLength = _maxBatchLength,
                MessageEncoderSettings = _messageEncoderSettings,
                WriteConcern = batch.WriteConcern,
                RetryRequested = _retryRequested
            };
        }

        private IExecutableInRetryableWriteContext<BulkWriteOperationResult> CreateBulkUpdateOperation(Batch batch)
        {
            var requests = batch.Requests.Cast<UpdateRequest>();
            return new BulkUpdateOperation(_collectionNamespace, requests, _messageEncoderSettings)
            {
                BypassDocumentValidation = _bypassDocumentValidation,
                IsOrdered = _isOrdered,
                MaxBatchCount = _maxBatchCount,
                MaxBatchLength = _maxBatchLength,
                WriteConcern = batch.WriteConcern,
                RetryRequested = _retryRequested
            };
        }

        private IExecutableInRetryableWriteContext<BulkWriteOperationResult> CreateUnmixedBatchOperation(Batch batch)
        {
            switch (batch.BatchType)
            {
                case WriteRequestType.Delete: return CreateBulkDeleteOperation(batch);
                case WriteRequestType.Insert: return CreateBulkInsertOperation(batch);
                case WriteRequestType.Update: return CreateBulkUpdateOperation(batch);
                default: throw new ArgumentException("Invalid batch type.", nameof(batch));
            }
        }

        private void EnsureCollationIsSupportedIfAnyRequestHasCollation(RetryableWriteContext context)
        {
            var serverVersion = context.Channel.ConnectionDescription.ServerVersion;
            if (!Feature.Collation.IsSupported(serverVersion))
            {
                foreach (var request in _requests)
                {
                    if (RequestHasCollation(request))
                    {
                        throw new NotSupportedException($"Server version {serverVersion} does not support collations.");
                    }
                }
            }
        }

        private BulkWriteBatchResult ExecuteBatch(RetryableWriteContext context, Batch batch, CancellationToken cancellationToken)
        {
            BulkWriteOperationResult result;
            MongoBulkWriteOperationException exception = null;
            try
            {
                var operation = CreateUnmixedBatchOperation(batch);
                result = operation.Execute(context, cancellationToken);
            }
            catch (MongoBulkWriteOperationException ex)
            {
                result = ex.Result;
                exception = ex;
            }

            return BulkWriteBatchResult.Create(result, exception, batch.IndexMap);
        }

        private async Task<BulkWriteBatchResult> ExecuteBatchAsync(RetryableWriteContext context, Batch batch, CancellationToken cancellationToken)
        {
            BulkWriteOperationResult result;
            MongoBulkWriteOperationException exception = null;
            try
            {
                var operation = CreateUnmixedBatchOperation(batch);
                result = await operation.ExecuteAsync(context, cancellationToken).ConfigureAwait(false);
            }
            catch (MongoBulkWriteOperationException ex)
            {
                result = ex.Result;
                exception = ex;
            }

            return BulkWriteBatchResult.Create(result, exception, batch.IndexMap);
        }

        private bool RequestHasCollation(WriteRequest request)
        {
            DeleteRequest deleteRequest;
            if ((deleteRequest = request as DeleteRequest) != null)
            {
                return deleteRequest.Collation != null;
            }

            UpdateRequest updateRequest;
            if ((updateRequest = request as UpdateRequest) != null)
            {
                return updateRequest.Collation != null;
            }

            return false;
        }

        // nested types
        private class Batch
        {
            public WriteRequestType BatchType;
            public List<WriteRequest> Requests;
            public IndexMap IndexMap;
            public WriteConcern WriteConcern;
            public BulkWriteBatchResult Result;
        }

        private class BatchHelper
        {
            // private fields
            private readonly List<BulkWriteBatchResult> _batchResults = new List<BulkWriteBatchResult>();
            private bool _hasWriteErrors;
            private readonly bool _isOrdered;
            private List<WriteRequestWithIndex> _unprocessed;
            private WriteConcern _writeConcern;

            // constructors
            public BatchHelper(IEnumerable<WriteRequest> requests, bool ordered, WriteConcern writeConcern)
            {
                Ensure.IsNotNull(requests, nameof(requests));
                _isOrdered = ordered;
                _writeConcern = writeConcern;

                _unprocessed = new List<WriteRequestWithIndex>();
                var index = 0;
                foreach (var request in requests)
                {
                    _unprocessed.Add(new WriteRequestWithIndex { Index = index++, Request = request });
                }
            }

            // public methods
            public IEnumerable<Batch> GetBatches()
            {
                if (_unprocessed.Count == 0)
                {
                    throw new InvalidOperationException("Bulk write operation is empty.");
                }

                while (_unprocessed.Count > 0 && ShouldContinue())
                {
                    var batch = GetNextBatch();

                    yield return batch;

                    _batchResults.Add(batch.Result);
                    _hasWriteErrors |= batch.Result.HasWriteErrors;
                }
            }

            public BulkWriteOperationResult GetFinalResultOrThrow(ConnectionId connectionId)
            {
                var combiner = new BulkWriteBatchResultCombiner(_batchResults, _writeConcern.IsAcknowledged);
                return combiner.CreateResultOrThrowIfHasErrors(connectionId, _unprocessed.Select(r => r.Request).ToList());
            }

            // private methods
            private Batch GetNextBatch()
            {
                var batchType = _unprocessed[0].Request.RequestType;

                List<WriteRequest> requests;
                IndexMap indexMap;
                if (_isOrdered)
                {
                    var index = _unprocessed.FindIndex(r => r.Request.RequestType != batchType);
                    var count = index == -1 ? _unprocessed.Count : index;
                    requests = _unprocessed.Take(count).Select(r => r.Request).ToList();
                    indexMap = new IndexMap.RangeBased(0, _unprocessed[0].Index, count);
                    _unprocessed.RemoveRange(0, count);
                }
                else
                {
                    var matching = _unprocessed.Where(r => r.Request.RequestType == batchType).ToList();
                    requests = matching.Select(r => r.Request).ToList();
                    indexMap = new IndexMap.DictionaryBased();
                    for (var i = 0; i < matching.Count; i++)
                    {
                        indexMap.Add(i, matching[i].Index);
                    }
                    _unprocessed = _unprocessed.Where(r => r.Request.RequestType != batchType).ToList();                 
                }

                var writeConcern = _writeConcern;
                if (!writeConcern.IsAcknowledged && _isOrdered && _unprocessed.Count > 0)
                {
                    writeConcern = WriteConcern.W1; // explicitly do not use the server's default
                }

                return new Batch
                {
                    BatchType = batchType,
                    Requests = requests,
                    IndexMap = indexMap,
                    WriteConcern = writeConcern
                };
            }

            private bool ShouldContinue()
            {
                return !_hasWriteErrors || !_isOrdered;
            }

            private struct WriteRequestWithIndex
            {
                public WriteRequest Request;
                public int Index;
            }
        }
    }
}

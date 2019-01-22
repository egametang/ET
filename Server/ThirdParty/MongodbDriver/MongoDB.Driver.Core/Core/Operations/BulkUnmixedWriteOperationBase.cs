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
using MongoDB.Bson;
using MongoDB.Driver.Core.Bindings;
using MongoDB.Driver.Core.Events;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders;

namespace MongoDB.Driver.Core.Operations
{
    internal abstract class BulkUnmixedWriteOperationBase<TWriteRequest> : IWriteOperation<BulkWriteOperationResult>, IExecutableInRetryableWriteContext<BulkWriteOperationResult>
        where TWriteRequest : WriteRequest
    {
        // fields
        private bool? _bypassDocumentValidation;
        private CollectionNamespace _collectionNamespace;
        private bool _isOrdered = true;
        private int? _maxBatchCount;
        private int? _maxBatchLength;
        private MessageEncoderSettings _messageEncoderSettings;
        private List<TWriteRequest> _requests;
        private bool _retryRequested;
        private WriteConcern _writeConcern = WriteConcern.Acknowledged;

        // constructors
        protected BulkUnmixedWriteOperationBase(
            CollectionNamespace collectionNamespace,
            IEnumerable<TWriteRequest> requests,
            MessageEncoderSettings messageEncoderSettings)
            : this(collectionNamespace, Ensure.IsNotNull(requests, nameof(requests)).ToList(), messageEncoderSettings)
        {
        }

        protected BulkUnmixedWriteOperationBase(
            CollectionNamespace collectionNamespace,
            List<TWriteRequest> requests,
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

        public bool IsOrdered
        {
            get { return _isOrdered; }
            set { _isOrdered = value; }
        }

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

        public IEnumerable<TWriteRequest> Requests
        {
            get { return _requests; }
        }

        public bool RetryRequested
        {
            get { return _retryRequested; }
            set { _retryRequested = value; }
        }

        public WriteConcern WriteConcern
        {
            get { return _writeConcern; }
            set { _writeConcern = Ensure.IsNotNull(value, nameof(value)); }
        }

        // public methods
        public BulkWriteOperationResult Execute(RetryableWriteContext context, CancellationToken cancellationToken)
        {
            EnsureCollationIsSupportedIfAnyRequestHasCollation(context, _requests);
            if (Feature.WriteCommands.IsSupported(context.Channel.ConnectionDescription.ServerVersion))
            {
                return ExecuteBatches(context, cancellationToken);
            }
            else
            {
                var emulator = CreateEmulator();
                return emulator.Execute(context, cancellationToken);
            }
        }

        public BulkWriteOperationResult Execute(IWriteBinding binding, CancellationToken cancellationToken)
        {
            using (EventContext.BeginOperation())
            using (var context = RetryableWriteContext.Create(binding, _retryRequested, cancellationToken))
            {
                context.DisableRetriesIfAnyWriteRequestIsNotRetryable(_requests);
                return Execute(context, cancellationToken);
            }
        }

        public Task<BulkWriteOperationResult> ExecuteAsync(RetryableWriteContext context, CancellationToken cancellationToken)
        {
            EnsureCollationIsSupportedIfAnyRequestHasCollation(context, _requests);
            if (Feature.WriteCommands.IsSupported(context.Channel.ConnectionDescription.ServerVersion))
            {
                return ExecuteBatchesAsync(context, cancellationToken);
            }
            else
            {
                var emulator = CreateEmulator();
                return emulator.ExecuteAsync(context, cancellationToken);
            }
        }

        public async Task<BulkWriteOperationResult> ExecuteAsync(IWriteBinding binding, CancellationToken cancellationToken)
        {
            using (EventContext.BeginOperation())
            using (var context = await RetryableWriteContext.CreateAsync(binding, _retryRequested, cancellationToken).ConfigureAwait(false))
            {
                context.DisableRetriesIfAnyWriteRequestIsNotRetryable(_requests);
                return await ExecuteAsync(context, cancellationToken).ConfigureAwait(false);
            }
        }

        // protected methods
        protected abstract IRetryableWriteOperation<BsonDocument> CreateBatchOperation(Batch batch);

        protected abstract IExecutableInRetryableWriteContext<BulkWriteOperationResult> CreateEmulator();

        protected abstract bool RequestHasCollation(TWriteRequest request);

        // private methods
        private BulkWriteBatchResult CreateBatchResult(Batch batch, BsonDocument writeCommandResult)
        {
            var requests = batch.Requests;
            var requestsInBatch = requests.GetProcessedItems();
            var indexMap = new IndexMap.RangeBased(0, requests.Offset, requests.Count);
            return BulkWriteBatchResult.Create(
                _isOrdered,
                requestsInBatch,
                writeCommandResult,
                indexMap);
        }

        private void EnsureCollationIsSupportedIfAnyRequestHasCollation(RetryableWriteContext context, IEnumerable<TWriteRequest> requests)
        {
            var serverVersion = context.Channel.ConnectionDescription.ServerVersion;
            if (!Feature.Collation.IsSupported(serverVersion))
            {
                foreach (var request in requests)
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
            var operation = CreateBatchOperation(batch);
            BsonDocument operationResult;
            try
            {
                operationResult = RetryableWriteOperationExecutor.Execute(operation, context, cancellationToken);
            }
            catch (MongoWriteConcernException exception) when (exception.IsWriteConcernErrorOnly())
            {
                operationResult = exception.Result;
            }
            return CreateBatchResult(batch, operationResult);
        }

        private async Task<BulkWriteBatchResult> ExecuteBatchAsync(RetryableWriteContext context, Batch batch, CancellationToken cancellationToken)
        {
            var operation = CreateBatchOperation(batch);
            BsonDocument operationResult;
            try
            {
                operationResult = await RetryableWriteOperationExecutor.ExecuteAsync(operation, context, cancellationToken).ConfigureAwait(false);
            }
            catch (MongoWriteConcernException exception) when (exception.IsWriteConcernErrorOnly())
            {
                operationResult = exception.Result;
            }
            return CreateBatchResult(batch, operationResult);
        }

        private BulkWriteOperationResult ExecuteBatches(RetryableWriteContext context, CancellationToken cancellationToken)
        {
            var helper = new BatchHelper(_requests, _writeConcern, _isOrdered);
            foreach (var batch in helper.GetBatches())
            {
                batch.Result = ExecuteBatch(context, batch, cancellationToken);
            }
            return helper.CreateFinalResultOrThrow(context.Channel);
        }

        private async Task<BulkWriteOperationResult> ExecuteBatchesAsync(RetryableWriteContext context, CancellationToken cancellationToken)
        {
            var helper = new BatchHelper(_requests, _writeConcern, _isOrdered);
            foreach (var batch in helper.GetBatches())
            {
                batch.Result = await ExecuteBatchAsync(context, batch, cancellationToken).ConfigureAwait(false);
            }
            return helper.CreateFinalResultOrThrow(context.Channel);
        }

        // nested types
        private class BatchHelper
        {
            private readonly List<BulkWriteBatchResult> _batchResults = new List<BulkWriteBatchResult>();
            private bool _hasWriteErrors;
            private readonly bool _isOrdered;
            private readonly BatchableSource<TWriteRequest> _requests;
            private readonly WriteConcern _writeConcern;

            public BatchHelper(IReadOnlyList<TWriteRequest> requests, WriteConcern writeConcern, bool isOrdered)
            {
                _requests = new BatchableSource<TWriteRequest>(requests, 0, requests.Count, canBeSplit: true);
                _writeConcern = writeConcern;
                _isOrdered = isOrdered;
            }

            public IEnumerable<Batch> GetBatches()
            {
                while (_requests.Count > 0 && ShouldContinue())
                {
                    var batch = new Batch
                    {
                        Requests = _requests                      
                    };

                    yield return batch;

                    _batchResults.Add(batch.Result);
                    _hasWriteErrors |= batch.Result.HasWriteErrors;

                    _requests.AdvancePastProcessedItems();
                }
            }

            public BulkWriteOperationResult CreateFinalResultOrThrow(IChannelHandle channel)
            {
                var combiner = new BulkWriteBatchResultCombiner(_batchResults, _writeConcern.IsAcknowledged);
                var remainingRequests = _requests.GetUnprocessedItems();
                return combiner.CreateResultOrThrowIfHasErrors(channel.ConnectionDescription.ConnectionId, remainingRequests);
            }

            // private methods
            private bool ShouldContinue()
            {
                return !_hasWriteErrors || !_isOrdered;
            }
        }

        protected class Batch
        {
            public BatchableSource<TWriteRequest> Requests;
            public BulkWriteBatchResult Result;
        }
    }
}

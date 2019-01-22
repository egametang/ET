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

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver.Core.Bindings;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders;

namespace MongoDB.Driver.Core.Operations
{
    internal abstract class BulkUnmixedWriteOperationEmulatorBase<TWriteRequest> : IExecutableInRetryableWriteContext<BulkWriteOperationResult>
        where TWriteRequest : WriteRequest
    {
        // fields
        private readonly CollectionNamespace _collectionNamespace;
        private bool _isOrdered = true;
        private int? _maxBatchCount;
        private int? _maxBatchLength;
        private readonly MessageEncoderSettings _messageEncoderSettings;
        private readonly List<TWriteRequest> _requests;
        private WriteConcern _writeConcern = WriteConcern.Acknowledged;

        // constructors
        protected BulkUnmixedWriteOperationEmulatorBase(
            CollectionNamespace collectionNamespace,
            IEnumerable<TWriteRequest> requests,
            MessageEncoderSettings messageEncoderSettings)
            : this(collectionNamespace, Ensure.IsNotNull(requests, nameof(requests)).ToList(), messageEncoderSettings)
        {
        }

        protected BulkUnmixedWriteOperationEmulatorBase(
            CollectionNamespace collectionNamespace,
            List<TWriteRequest> requests,
            MessageEncoderSettings messageEncoderSettings)
        {
            _collectionNamespace = Ensure.IsNotNull(collectionNamespace, nameof(collectionNamespace));
            _requests = Ensure.IsNotNull(requests, nameof(requests));
            _messageEncoderSettings = messageEncoderSettings;
        }

        // properties
        public CollectionNamespace CollectionNamespace
        {
            get { return _collectionNamespace; }
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
        }

        public bool IsOrdered
        {
            get { return _isOrdered; }
            set { _isOrdered = value; }
        }

        public IEnumerable<TWriteRequest> Requests
        {
            get { return _requests; }
        }

        public WriteConcern WriteConcern
        {
            get { return _writeConcern; }
            set { _writeConcern = Ensure.IsNotNull(value, nameof(value)); }
        }

        // public methods
        public BulkWriteOperationResult Execute(RetryableWriteContext context, CancellationToken cancellationToken)
        {
            var helper = new BatchHelper(this, context.Channel);
            foreach (var batch in helper.GetBatches())
            {
                batch.Result = EmulateSingleRequest(context.Channel, batch.Request, batch.OriginalIndex, cancellationToken);
            }
            return helper.GetFinalResultOrThrow();
        }

        public async Task<BulkWriteOperationResult> ExecuteAsync(RetryableWriteContext context, CancellationToken cancellationToken)
        {
            var helper = new BatchHelper(this, context.Channel);
            foreach (var batch in helper.GetBatches())
            {
                batch.Result = await EmulateSingleRequestAsync(context.Channel, batch.Request, batch.OriginalIndex, cancellationToken).ConfigureAwait(false);
            }
            return helper.GetFinalResultOrThrow();
        }

        // protected methods
        protected abstract WriteConcernResult ExecuteProtocol(IChannelHandle channel, TWriteRequest request, CancellationToken cancellationToken);

        protected abstract Task<WriteConcernResult> ExecuteProtocolAsync(IChannelHandle channel, TWriteRequest request, CancellationToken cancellationToken);

        // private methods
        private BulkWriteBatchResult EmulateSingleRequest(IChannelHandle channel, TWriteRequest request, int originalIndex, CancellationToken cancellationToken)
        {
            WriteConcernResult writeConcernResult = null;
            MongoWriteConcernException writeConcernException = null;
            try
            {
                writeConcernResult = ExecuteProtocol(channel, request, cancellationToken);
            }
            catch (MongoWriteConcernException ex)
            {
                writeConcernResult = ex.WriteConcernResult;
                writeConcernException = ex;
            }

            return CreateSingleRequestResult(request, originalIndex, writeConcernResult, writeConcernException);
        }

        private async Task<BulkWriteBatchResult> EmulateSingleRequestAsync(IChannelHandle channel, TWriteRequest request, int originalIndex, CancellationToken cancellationToken)
        {
            WriteConcernResult writeConcernResult = null;
            MongoWriteConcernException writeConcernException = null;
            try
            {
                writeConcernResult = await ExecuteProtocolAsync(channel, request, cancellationToken).ConfigureAwait(false);
            }
            catch (MongoWriteConcernException ex)
            {
                writeConcernResult = ex.WriteConcernResult;
                writeConcernException = ex;
            }

            return CreateSingleRequestResult(request, originalIndex, writeConcernResult, writeConcernException);
        }

        private BulkWriteBatchResult CreateSingleRequestResult(TWriteRequest request, int originalIndex, WriteConcernResult writeConcernResult, MongoWriteConcernException writeConcernException)
        {
            var indexMap = new IndexMap.RangeBased(0, originalIndex, 1);
            return BulkWriteBatchResult.Create(
                request,
                writeConcernResult,
                writeConcernException,
                indexMap);
        }

        // nested types
        private class BatchHelper
        {
            private readonly List<BulkWriteBatchResult> _batchResults = new List<BulkWriteBatchResult>();
            private readonly IChannelHandle _channel;
            private bool _hasWriteErrors;
            private readonly BulkUnmixedWriteOperationEmulatorBase<TWriteRequest> _operation;
            private readonly List<TWriteRequest> _remainingRequests = new List<TWriteRequest>();

            public BatchHelper(BulkUnmixedWriteOperationEmulatorBase<TWriteRequest> operation, IChannelHandle channel)
            {
                _operation = operation;
                _channel = channel;
            }

            public IEnumerable<Batch> GetBatches()
            {
                var originalIndex = 0;
                foreach (var request in _operation._requests)
                {
                    if (_hasWriteErrors && _operation._isOrdered)
                    {
                        _remainingRequests.Add(request);
                        continue;
                    }

                    var batch = new Batch { Request = request, OriginalIndex = originalIndex };

                    yield return batch;

                    _batchResults.Add(batch.Result);
                    _hasWriteErrors |= batch.Result.HasWriteErrors;

                    originalIndex++;
                }
            }

            public BulkWriteOperationResult GetFinalResultOrThrow()
            {
                var combiner = new BulkWriteBatchResultCombiner(_batchResults, _operation._writeConcern.IsAcknowledged);
                return combiner.CreateResultOrThrowIfHasErrors(_channel.ConnectionDescription.ConnectionId, _remainingRequests);
            }

            public class Batch
            {
                public int OriginalIndex;
                public TWriteRequest Request;
                public BulkWriteBatchResult Result;
            }
        }
    }
}

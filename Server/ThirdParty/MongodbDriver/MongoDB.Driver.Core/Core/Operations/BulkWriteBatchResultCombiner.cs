/* Copyright 2010-2014 MongoDB Inc.
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
using System.Text;
using MongoDB.Driver.Core.Connections;

namespace MongoDB.Driver.Core.Operations
{
    internal class BulkWriteBatchResultCombiner
    {
        // fields
        private readonly IReadOnlyList<BulkWriteBatchResult> _batchResults;
        private readonly bool _isAcknowledged;

        // constructors
        public BulkWriteBatchResultCombiner(IReadOnlyList<BulkWriteBatchResult> batchResults, bool isAcknowledged)
        {
            _batchResults = batchResults;
            _isAcknowledged = isAcknowledged;
        }

        //  methods
        private int CombineBatchCount()
        {
            return _batchResults.Sum(r => r.BatchCount);
        }

        private long CombineDeletedCount()
        {
            return _batchResults.Sum(r => r.DeletedCount);
        }

        private long CombineInsertedCount()
        {
            return _batchResults.Sum(r => r.InsertedCount);
        }

        private long CombineMatchedCount()
        {
            return _batchResults.Sum(r => r.MatchedCount);
        }

        private long? CombineModifiedCount()
        {
            if (_batchResults.All(r => r.ModifiedCount.HasValue))
            {
                return _batchResults.Sum(r => r.ModifiedCount.Value);
            }
            else
            {
                return null;
            }
        }

        private IReadOnlyList<WriteRequest> CombineProcessedRequests()
        {
            if (_batchResults.Count == 1)
            {
                return _batchResults[0].ProcessedRequests;
            }
            else
            {
                return _batchResults.SelectMany(r => r.ProcessedRequests).ToList();
            }
        }

        private IReadOnlyList<WriteRequest> CombineUnprocessedRequests(IReadOnlyList<WriteRequest> remainingRequests)
        {
            if (_batchResults.Count == 1 && remainingRequests.Count == 0)
            {
                return _batchResults[0].UnprocessedRequests;
            }
            else
            {
                return _batchResults.SelectMany(r => r.UnprocessedRequests).Concat(remainingRequests).ToList();
            }
        }

        private IReadOnlyList<BulkWriteOperationUpsert> CombineUpserts()
        {
            if (_batchResults.Count == 1 && _batchResults[0].IndexMap.IsIdentityMap)
            {
                return _batchResults[0].Upserts;
            }
            else
            {
                return _batchResults.SelectMany(r => r.Upserts.Select(u => u.WithMappedIndex(r.IndexMap))).OrderBy(u => u.Index).ToList();
            }
        }

        private BulkWriteConcernError CombineWriteConcernErrors()
        {
            return _batchResults.Select(r => r.WriteConcernError).LastOrDefault(e => e != null);
        }

        private IReadOnlyList<BulkWriteOperationError> CombineWriteErrors()
        {
            if (_batchResults.Count == 1 && _batchResults[0].IndexMap.IsIdentityMap)
            {
                return _batchResults[0].WriteErrors;
            }
            else
            {
                return _batchResults.SelectMany(r => r.WriteErrors.Select(e => e.WithMappedIndex(r.IndexMap))).OrderBy(e => e.Index).ToList();
            }
        }

        private MongoBulkWriteOperationException CreateBulkWriteException(ConnectionId connectionId, IEnumerable<WriteRequest> remainingRequests)
        {
            var remainingRequestsList = remainingRequests.ToList();
            var result = CreateBulkWriteResult(remainingRequestsList.Count);
            var writeErrors = CombineWriteErrors();
            var writeConcernError = CombineWriteConcernErrors();
            var unprocessedRequests = CombineUnprocessedRequests(remainingRequestsList);

            return new MongoBulkWriteOperationException(connectionId, result, writeErrors, writeConcernError, unprocessedRequests);
        }

        private BulkWriteOperationResult CreateBulkWriteResult(int remainingRequestsCount)
        {
            var requestCount = CombineBatchCount() + remainingRequestsCount;
            var processedRequests = CombineProcessedRequests();

            if (!_isAcknowledged)
            {
                return new BulkWriteOperationResult.Unacknowledged(
                    requestCount,
                    processedRequests);
            }

            var matchedCount = CombineMatchedCount();
            var deletedCount = CombineDeletedCount();
            var insertedCount = CombineInsertedCount();
            var modifiedCount = CombineModifiedCount();
            var upserts = CombineUpserts();

            return new BulkWriteOperationResult.Acknowledged(
                requestCount,
                matchedCount,
                deletedCount,
                insertedCount,
                modifiedCount,
                processedRequests,
                upserts);
        }

        public BulkWriteOperationResult CreateResultOrThrowIfHasErrors(ConnectionId connectionId, IReadOnlyList<WriteRequest> remainingRequests)
        {
            if (_batchResults.Any(r => r.HasWriteErrors || r.HasWriteConcernError) && _isAcknowledged)
            {
                throw CreateBulkWriteException(connectionId, remainingRequests);
            }

            return CreateBulkWriteResult(remainingRequests.Count);
        }
    }
}

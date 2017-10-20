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
using System.Collections.ObjectModel;
using System.Linq;
using MongoDB.Bson;

namespace MongoDB.Driver.Core.Operations
{
    /// <summary>
    /// Represents the result of a bulk write operation.
    /// </summary>
#if NET45
    [Serializable]
#endif
    public abstract class BulkWriteOperationResult
    {
        // fields
        private readonly IReadOnlyList<WriteRequest> _processedRequests;
        private readonly int _requestCount;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="BulkWriteOperationResult" /> class.
        /// </summary>
        /// <param name="requestCount">The request count.</param>
        /// <param name="processedRequests">The processed requests.</param>
        protected BulkWriteOperationResult(
            int requestCount,
            IReadOnlyList<WriteRequest> processedRequests)
        {
            _requestCount = requestCount;
            _processedRequests = processedRequests;
        }

        // properties
        /// <summary>
        /// Gets the number of documents that were deleted.
        /// </summary>
        /// <value>
        /// The number of document that were deleted.
        /// </value>
        public abstract long DeletedCount { get; }

        /// <summary>
        /// Gets the number of documents that were inserted.
        /// </summary>
        /// <value>
        /// The number of document that were inserted.
        /// </value>
        public abstract long InsertedCount { get; }

        /// <summary>
        /// Gets a value indicating whether the bulk write operation was acknowledged.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the bulk write operation was acknowledged; otherwise, <c>false</c>.
        /// </value>
        public abstract bool IsAcknowledged { get; }

        /// <summary>
        /// Gets a value indicating whether the modified count is available.
        /// </summary>
        /// <remarks>
        /// The modified count is only available when all servers have been upgraded to 2.6 or above.
        /// </remarks>
        /// <value>
        /// <c>true</c> if the modified count is available; otherwise, <c>false</c>.
        /// </value>
        public abstract bool IsModifiedCountAvailable { get; }

        /// <summary>
        /// Gets the number of documents that were matched.
        /// </summary>
        /// <value>
        /// The number of document that were matched.
        /// </value>
        public abstract long MatchedCount { get; }

        /// <summary>
        /// Gets the number of documents that were actually modified during an update.
        /// </summary>
        /// <value>
        /// The number of document that were actually modified during an update.
        /// </value>
        public abstract long ModifiedCount { get; }

        /// <summary>
        /// Gets the processed requests.
        /// </summary>
        /// <value>
        /// The processed requests.
        /// </value>
        public IReadOnlyList<WriteRequest> ProcessedRequests
        {
            get { return _processedRequests; }
        }

        /// <summary>
        /// Gets the request count.
        /// </summary>
        /// <value>
        /// The request count.
        /// </value>
        public int RequestCount
        {
            get { return _requestCount; }
        }

        /// <summary>
        /// Gets a list with information about each request that resulted in an upsert.
        /// </summary>
        /// <value>
        /// The list with information about each request that resulted in an upsert.
        /// </value>
        public abstract IReadOnlyList<BulkWriteOperationUpsert> Upserts { get; }

        // nested classes
        /// <summary>
        /// Represents the result of an acknowledged bulk write operation.
        /// </summary>
#if NET45
    [Serializable]
#endif
        public class Acknowledged : BulkWriteOperationResult
        {
            // fields
            private readonly long _deletedCount;
            private readonly long _insertedCount;
            private readonly long _matchedCount;
            private readonly long? _modifiedCount;
            private readonly IReadOnlyList<BulkWriteOperationUpsert> _upserts;

            // constructors
            /// <summary>
            /// Initializes a new instance of the <see cref="Acknowledged" /> class.
            /// </summary>
            /// <param name="requestCount">The request count.</param>
            /// <param name="matchedCount">The matched count.</param>
            /// <param name="deletedCount">The deleted count.</param>
            /// <param name="insertedCount">The inserted count.</param>
            /// <param name="modifiedCount">The modified count.</param>
            /// <param name="processedRequests">The processed requests.</param>
            /// <param name="upserts">The upserts.</param>
            public Acknowledged(
                int requestCount,
                long matchedCount,
                long deletedCount,
                long insertedCount,
                long? modifiedCount,
                IReadOnlyList<WriteRequest> processedRequests,
                IReadOnlyList<BulkWriteOperationUpsert> upserts)
                : base(requestCount, processedRequests)
            {
                _matchedCount = matchedCount;
                _deletedCount = deletedCount;
                _insertedCount = insertedCount;
                _modifiedCount = modifiedCount;
                _upserts = upserts;
            }

            // properties
            /// <inheritdoc/>
            public override long DeletedCount
            {
                get { return _deletedCount; }
            }

            /// <inheritdoc/>
            public override long InsertedCount
            {
                get { return _insertedCount; }
            }

            /// <inheritdoc/>
            public override bool IsModifiedCountAvailable
            {
                get { return _modifiedCount.HasValue; }
            }

            /// <inheritdoc/>
            public override long MatchedCount
            {
                get { return _matchedCount; }
            }

            /// <inheritdoc/>
            public override long ModifiedCount
            {
                get
                {
                    if (!_modifiedCount.HasValue)
                    {
                        throw new NotSupportedException("ModifiedCount is not available.");
                    }
                    return _modifiedCount.Value;
                }
            }

            /// <inheritdoc/>
            public override bool IsAcknowledged
            {
                get { return true; }
            }

            /// <inheritdoc/>
            public override IReadOnlyList<BulkWriteOperationUpsert> Upserts
            {
                get { return _upserts; }
            }
        }

        /// <summary>
        /// Represents the result of an unacknowledged BulkWrite operation.
        /// </summary>
#if NET45
    [Serializable]
#endif
        public class Unacknowledged : BulkWriteOperationResult
        {
            // constructors
            /// <summary>
            /// Initializes a new instance of the <see cref="BulkWriteOperationResult.Unacknowledged" /> class.
            /// </summary>
            /// <param name="requestCount">The request count.</param>
            /// <param name="processedRequests">The processed requests.</param>
            public Unacknowledged(
                int requestCount,
                IReadOnlyList<WriteRequest> processedRequests)
                : base(requestCount, processedRequests)
            {
            }

            // properties
            /// <inheritdoc/>
            public override long DeletedCount
            {
                get { throw new NotSupportedException("Only acknowledged writes support the DeletedCount property."); }
            }

            /// <inheritdoc/>
            public override long InsertedCount
            {
                get { throw new NotSupportedException("Only acknowledged writes support the InsertedCount property."); }
            }

            /// <inheritdoc/>
            public override bool IsModifiedCountAvailable
            {
                get { throw new NotSupportedException("Only acknowledged writes support the IsModifiedCountAvailable property."); }
            }

            /// <inheritdoc/>
            public override long MatchedCount
            {
                get { throw new NotSupportedException("Only acknowledged writes support the MatchedCount property."); }
            }

            /// <inheritdoc/>
            public override long ModifiedCount
            {
                get { throw new NotSupportedException("Only acknowledged writes support the ModifiedCount property."); }
            }

            /// <inheritdoc/>
            public override bool IsAcknowledged
            {
                get { return false; }
            }

            /// <inheritdoc/>
            public override IReadOnlyList<BulkWriteOperationUpsert> Upserts
            {
                get { throw new NotSupportedException("Only acknowledged writes support the Upserts property."); }
            }
        }
    }
}

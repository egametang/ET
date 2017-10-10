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
using System.Collections.ObjectModel;
using System.Linq;
#if NET45
using System.Runtime.Serialization;
#endif
using MongoDB.Bson;

namespace MongoDB.Driver
{
    /// <summary>
    /// Represents the result of a bulk write operation.
    /// </summary>
#if NET45
    [Serializable]
#endif
    public abstract class BulkWriteResult
    {
        // fields
        private readonly int _requestCount;

        //constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="BulkWriteResult"/> class.
        /// </summary>
        /// <param name="requestCount">The request count.</param>
        protected BulkWriteResult(int requestCount)
        {
            _requestCount = requestCount;
        }

        // properties
        /// <summary>
        /// Gets the number of documents that were deleted.
        /// </summary>
        public abstract long DeletedCount { get; }

        /// <summary>
        /// Gets the number of documents that were inserted.
        /// </summary>
        public abstract long InsertedCount { get; }

        /// <summary>
        /// Gets a value indicating whether the bulk write operation was acknowledged.
        /// </summary>
        public abstract bool IsAcknowledged { get; }

        /// <summary>
        /// Gets a value indicating whether the modified count is available.
        /// </summary>
        /// <remarks>
        /// The modified count is only available when all servers have been upgraded to 2.6 or above.
        /// </remarks>
        public abstract bool IsModifiedCountAvailable { get; }

        /// <summary>
        /// Gets the number of documents that were matched.
        /// </summary>
        public abstract long MatchedCount { get; }

        /// <summary>
        /// Gets the number of documents that were actually modified during an update.
        /// </summary>
        public abstract long ModifiedCount { get; }

        /// <summary>
        /// Gets the request count.
        /// </summary>
        public int RequestCount
        {
            get { return _requestCount; }
        }

        /// <summary>
        /// Gets a list with information about each request that resulted in an upsert.
        /// </summary>
        public abstract IReadOnlyList<BulkWriteUpsert> Upserts { get; }
    }

    /// <summary>
    /// Represents the result of a bulk write operation.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document.</typeparam>
#if NET45
    [Serializable]
#endif
    public abstract class BulkWriteResult<TDocument> : BulkWriteResult
    {
        // private fields
        private readonly IReadOnlyList<WriteModel<TDocument>> _processedRequests;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="BulkWriteResult" /> class.
        /// </summary>
        /// <param name="requestCount">The request count.</param>
        /// <param name="processedRequests">The processed requests.</param>
        protected BulkWriteResult(
            int requestCount,
            IEnumerable<WriteModel<TDocument>> processedRequests)
            : base(requestCount)
        {
            _processedRequests = processedRequests.ToList();
        }

        // public properties
        /// <summary>
        /// Gets the processed requests.
        /// </summary>
        public IReadOnlyList<WriteModel<TDocument>> ProcessedRequests
        {
            get { return _processedRequests; }
        }

        // internal static methods
        internal static BulkWriteResult<TDocument> FromCore(Core.Operations.BulkWriteOperationResult result)
        {
            if (result.IsAcknowledged)
            {
                return new Acknowledged(
                    result.RequestCount,
                    result.MatchedCount,
                    result.DeletedCount,
                    result.InsertedCount,
                    result.IsModifiedCountAvailable ? (long?)result.ModifiedCount : null,
                    result.ProcessedRequests.Select(r => WriteModel<TDocument>.FromCore(r)),
                    result.Upserts.Select(u => BulkWriteUpsert.FromCore(u)));
            }

            return new Unacknowledged(
                result.RequestCount,
                result.ProcessedRequests.Select(r => WriteModel<TDocument>.FromCore(r)));
        }

        internal static BulkWriteResult<TDocument> FromCore(Core.Operations.BulkWriteOperationResult result, IEnumerable<WriteModel<TDocument>> requests)
        {
            if (result.IsAcknowledged)
            {
                return new Acknowledged(
                    result.RequestCount,
                    result.MatchedCount,
                    result.DeletedCount,
                    result.InsertedCount,
                    result.IsModifiedCountAvailable ? (long?)result.ModifiedCount : null,
                    requests,
                    result.Upserts.Select(u => BulkWriteUpsert.FromCore(u)));
            }

            return new Unacknowledged(
                result.RequestCount,
                requests);
        }

        // nested classes
        /// <summary>
        /// Result from an acknowledged write concern.
        /// </summary>
#if NET45
    [Serializable]
#endif
        public class Acknowledged : BulkWriteResult<TDocument>
        {
            // private fields
            private readonly long _deletedCount;
            private readonly long _insertedCount;
            private readonly long _matchedCount;
            private readonly long? _modifiedCount;
            private readonly IReadOnlyList<BulkWriteUpsert> _upserts;

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
                IEnumerable<WriteModel<TDocument>> processedRequests,
                IEnumerable<BulkWriteUpsert> upserts)
                : base(requestCount, processedRequests)
            {
                _matchedCount = matchedCount;
                _deletedCount = deletedCount;
                _insertedCount = insertedCount;
                _modifiedCount = modifiedCount;
                _upserts = upserts.ToList();
            }

            // public properties
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
            public override bool IsAcknowledged
            {
                get { return true; }
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
            public override IReadOnlyList<BulkWriteUpsert> Upserts
            {
                get { return _upserts; }
            }
        }

        /// <summary>
        /// Result from an unacknowledged write concern.
        /// </summary>
#if NET45
    [Serializable]
#endif
        public class Unacknowledged : BulkWriteResult<TDocument>
        {
            // constructors
            /// <summary>
            /// Initializes a new instance of the <see cref="Unacknowledged"/> class.
            /// </summary>
            /// <param name="requestCount">The request count.</param>
            /// <param name="processedRequests">The processed requests.</param>
            public Unacknowledged(
                int requestCount,
                IEnumerable<WriteModel<TDocument>> processedRequests)
                : base(requestCount, processedRequests)
            {
            }

            // public properties
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
            public override bool IsAcknowledged
            {
                get { return false; }
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
            public override IReadOnlyList<BulkWriteUpsert> Upserts
            {
                get { throw new NotSupportedException("Only acknowledged writes support the Upserts property."); }
            }
        }
    }
}

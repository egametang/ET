/* Copyright 2017-present MongoDB Inc.
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
using MongoDB.Driver.Core.Clusters;

namespace MongoDB.Driver.Core.Bindings
{
    /// <summary>
    /// The interface for a session in Core.
    /// </summary>
    public interface ICoreSession : IDisposable
    {
        // properties
        /// <summary>
        /// Gets the cluster time.
        /// </summary>
        /// <value>
        /// The cluster time.
        /// </value>
        BsonDocument ClusterTime { get; }

        /// <summary>
        /// Gets the current transaction.
        /// </summary>
        /// <value>
        /// The current transaction.
        /// </value>
        CoreTransaction CurrentTransaction { get; }

        /// <summary>
        /// Gets the session Id.
        /// </summary>
        /// <value>
        /// The session Id.
        /// </value>
        BsonDocument Id { get; }

        /// <summary>
        /// Gets a value indicate whether this instance is causally consistent.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the session is causally consistent.
        /// </value>
        bool IsCausallyConsistent { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is implicit session.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is implicit session; otherwise, <c>false</c>.
        /// </value>
        bool IsImplicit { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is in a transaction.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is in a transaction; otherwise, <c>false</c>.
        /// </value>
        bool IsInTransaction { get; }

        /// <summary>
        /// Gets the operation time.
        /// </summary>
        /// <value>
        /// The operation time.
        /// </value>
        BsonTimestamp OperationTime { get; }

        /// <summary>
        /// Gets the session options.
        /// </summary>
        /// <value>
        /// The session options.
        /// </value>
        CoreSessionOptions Options { get; }

        /// <summary>
        /// Gets the server session.
        /// </summary>
        /// <value>
        /// The server session.
        /// </value>
        ICoreServerSession ServerSession { get; }

        // methods
        /// <summary>
        /// Aborts the transaction.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        void AbortTransaction(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Aborts the transaction.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task.</returns>
        Task AbortTransactionAsync(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// The driver is about to send a command on this session. Called to track session state.
        /// </summary>
        void AboutToSendCommand();

        /// <summary>
        /// Advances the cluster time.
        /// </summary>
        /// <param name="newClusterTime">The new cluster time.</param>
        void AdvanceClusterTime(BsonDocument newClusterTime);

        /// <summary>
        /// Advances the operation time.
        /// </summary>
        /// <param name="newOperationTime">The new operation time.</param>
        void AdvanceOperationTime(BsonTimestamp newOperationTime);

        /// <summary>
        /// Advances the transaction id.
        /// </summary>
        /// <returns>The transaction id.</returns>
        long AdvanceTransactionNumber();

        /// <summary>
        /// Commits the transaction.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        void CommitTransaction(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Commits the transaction.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task.</returns>
        Task CommitTransactionAsync(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Starts a transaction.
        /// </summary>
        /// <param name="transactionOptions">The transaction options.</param>
        void StartTransaction(TransactionOptions transactionOptions = null);

        /// <summary>
        /// Called by the driver when the session is used (i.e. sent to the server).
        /// </summary>
        void WasUsed();
    }

    /// <summary>
    /// A handle to a reference counted core session.
    /// </summary>
    /// <seealso cref="MongoDB.Driver.Core.Bindings.ICoreSession" />
    public interface ICoreSessionHandle : ICoreSession
    {
        /// <summary>
        /// Increments the reference count of the underlying session and returns a new handle to it.
        /// </summary>
        /// <returns>A new handle.</returns>
        ICoreSessionHandle Fork();
    }
}

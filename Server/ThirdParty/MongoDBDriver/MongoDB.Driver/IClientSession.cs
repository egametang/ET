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
using MongoDB.Driver.Core.Bindings;

namespace MongoDB.Driver
{
    /// <summary>
    /// The interface for a client session.
    /// </summary>
    public interface IClientSession : IDisposable
    {
        // properties
        /// <summary>
        /// Gets the client.
        /// </summary>
        /// <value>
        /// The client.
        /// </value>
        IMongoClient Client { get; }

        /// <summary>
        /// Gets the cluster time.
        /// </summary>
        /// <value>
        /// The cluster time.
        /// </value>
        BsonDocument ClusterTime { get; }

        /// <summary>
        /// Gets a value indicating whether this session is an implicit session.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this session is an implicit session; otherwise, <c>false</c>.
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
        /// Gets the options.
        /// </summary>
        /// <value>
        /// The options.
        /// </value>
        ClientSessionOptions Options { get; }

        /// <summary>
        /// Gets the server session.
        /// </summary>
        /// <value>
        /// The server session.
        /// </value>
        IServerSession ServerSession { get; }

        /// <summary>
        /// Gets the wrapped core session (intended for internal use only).
        /// </summary>
        /// <value>
        /// The wrapped core session.
        /// </value>
        ICoreSessionHandle WrappedCoreSession { get; }

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
    }

    /// <summary>
    /// A handle to an underlying reference counted IClientSession.
    /// </summary>
    /// <seealso cref="MongoDB.Driver.IClientSession" />
    public interface IClientSessionHandle : IClientSession
    {
        /// <summary>
        /// Forks this instance.
        /// </summary>
        /// <returns>A session.</returns>
        IClientSessionHandle Fork();
    }
}

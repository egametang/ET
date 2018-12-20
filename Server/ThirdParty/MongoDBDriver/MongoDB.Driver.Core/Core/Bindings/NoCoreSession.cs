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
    /// An object that represents no core session.
    /// </summary>
    /// <seealso cref="MongoDB.Driver.Core.Bindings.ICoreSession" />
    public sealed class NoCoreSession : ICoreSession
    {
        #region static
        // private static fields
        private static readonly ICoreSession __instance = new NoCoreSession();

        // public static properties
        /// <summary>
        /// Gets the pre-created instance.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static ICoreSession Instance => __instance;

        // public static methods
        /// <summary>
        /// Returns a new handle to a NoCoreSession object.
        /// </summary>
        /// <returns>A new handle to the NoCoreSession object.</returns>
        public static ICoreSessionHandle NewHandle()
        {
            return new CoreSessionHandle(__instance);
        }
        #endregion

        // public properties
        /// <inheritdoc />
        public BsonDocument ClusterTime => null;

        /// <inheritdoc />
        public CoreTransaction CurrentTransaction => null;

        /// <inheritdoc />
        public BsonDocument Id => null;

        /// <inheritdoc />
        public bool IsCausallyConsistent => false;

        /// <inheritdoc />
        public bool IsImplicit => true;

        /// <inheritdoc />
        public bool IsInTransaction => false;

        /// <inheritdoc />
        public BsonTimestamp OperationTime => null;

        /// <inheritdoc />
        public CoreSessionOptions Options => null;

        /// <inheritdoc />
        public ICoreServerSession ServerSession => NoCoreServerSession.Instance;

        // public methods
        /// <inheritdoc />
        public void AbortTransaction(CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotSupportedException("NoCoreSession does not support AbortTransaction.");
        }

        /// <inheritdoc />
        public Task AbortTransactionAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotSupportedException("NoCoreSession does not support AbortTransactionAsync.");
        }

        /// <inheritdoc />
        public void AboutToSendCommand()
        {
        }

        /// <inheritdoc />
        public void AdvanceClusterTime(BsonDocument newClusterTime)
        {
        }

        /// <inheritdoc />
        public void AdvanceOperationTime(BsonTimestamp newOperationTime)
        {
        }

        /// <inheritdoc />
        public long AdvanceTransactionNumber()
        {
            return -1;
        }

        /// <inheritdoc />
        public void CommitTransaction(CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotSupportedException("NoCoreSession does not support CommitTransaction.");
        }

        /// <inheritdoc />
        public Task CommitTransactionAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotSupportedException("NoCoreSession does not support CommitTransactionAsync.");
        }

        /// <inheritdoc />
        public void Dispose()
        {
        }

        /// <inheritdoc />
        public void StartTransaction(TransactionOptions transactionOptions = null)
        {
            throw new NotSupportedException("NoCoreSession does not support StartTransaction.");
        }

        /// <inheritdoc />
        public void WasUsed()
        {
        }
    }
}

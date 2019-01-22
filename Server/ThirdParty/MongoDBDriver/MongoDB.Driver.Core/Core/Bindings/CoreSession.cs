/* Copyright 2018-present MongoDB Inc.
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
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.Operations;

namespace MongoDB.Driver.Core.Bindings
{
    /// <summary>
    /// Represents a session.
    /// </summary>
    /// <seealso cref="MongoDB.Driver.Core.Bindings.ICoreSession" />
    public sealed class CoreSession : ICoreSession
    {
        // private fields
        private readonly ICluster _cluster;
        private readonly IClusterClock _clusterClock = new ClusterClock();
        private CoreTransaction _currentTransaction;
        private bool _disposed;
        private bool _isCommitTransactionInProgress;
        private readonly IOperationClock _operationClock = new OperationClock();
        private readonly CoreSessionOptions _options;
        private readonly ICoreServerSession _serverSession;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreSession" /> class.
        /// </summary>
        /// <param name="cluster">The cluster.</param>
        /// <param name="serverSession">The server session.</param>
        /// <param name="options">The options.</param>
        public CoreSession(
            ICluster cluster,
            ICoreServerSession serverSession,
            CoreSessionOptions options)
        {
            _cluster = Ensure.IsNotNull(cluster, nameof(cluster));
            _serverSession = Ensure.IsNotNull(serverSession, nameof(serverSession));
            _options = Ensure.IsNotNull(options, nameof(options));
        }

        // public properties
        /// <summary>
        /// Gets the cluster.
        /// </summary>
        /// <value>
        /// The cluster.
        /// </value>
        public ICluster Cluster => _cluster;

        /// <inheritdoc />
        public BsonDocument ClusterTime => _clusterClock.ClusterTime;

        /// <inheritdoc />
        public CoreTransaction CurrentTransaction => _currentTransaction;

        /// <inheritdoc />
        public BsonDocument Id => _serverSession.Id;

        /// <inheritdoc />
        public bool IsCausallyConsistent => _options.IsCausallyConsistent;

        /// <inheritdoc />
        public bool IsImplicit => _options.IsImplicit;

        /// <inheritdoc />
        public bool IsInTransaction
        {
            get
            {
                if (_currentTransaction != null)
                {
                    switch (_currentTransaction.State)
                    {
                        case CoreTransactionState.Aborted:
                            return false;

                        case CoreTransactionState.Committed:
                            return _isCommitTransactionInProgress; // when retrying a commit we are temporarily "back in" the already committed transaction

                        default:
                            return true;
                    }
                }

                return false;
            }
        }

        /// <inheritdoc />
        public BsonTimestamp OperationTime => _operationClock.OperationTime;

        /// <inheritdoc />
        public CoreSessionOptions Options => _options;

        /// <inheritdoc />
        public ICoreServerSession ServerSession => _serverSession;

        // public methods
        /// <inheritdoc />
        public void AbortTransaction(CancellationToken cancellationToken = default(CancellationToken))
        {
            EnsureAbortTransactionCanBeCalled(nameof(AbortTransaction));

            try
            {
                if (_currentTransaction.IsEmpty)
                {
                    return;
                }

                try
                {
                    var firstAttempt = CreateAbortTransactionOperation();
                    ExecuteEndTransactionOnPrimary(firstAttempt, cancellationToken);
                    return;
                }
                catch (Exception exception) when (ShouldRetryEndTransactionException(exception))
                {
                    // ignore exception and retry
                }
                catch
                {
                    return; // ignore exception and return
                }

                try
                {
                    var secondAttempt = CreateAbortTransactionOperation();
                    ExecuteEndTransactionOnPrimary(secondAttempt, cancellationToken);
                }
                catch
                {
                    return; // ignore exception and return
                }
            }
            finally
            {
                _currentTransaction.SetState(CoreTransactionState.Aborted);
            }
        }

        /// <inheritdoc />
        public async Task AbortTransactionAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            EnsureAbortTransactionCanBeCalled(nameof(AbortTransaction));

            try
            {
                if (_currentTransaction.IsEmpty)
                {
                    return;
                }

                try
                {
                    var firstAttempt = CreateAbortTransactionOperation();
                    await ExecuteEndTransactionOnPrimaryAsync(firstAttempt, cancellationToken).ConfigureAwait(false);
                    return;
                }
                catch (Exception exception) when (ShouldRetryEndTransactionException(exception))
                {
                    // ignore exception and retry
                }
                catch
                {
                    return; // ignore exception and return
                }

                try
                {
                    var secondAttempt = CreateAbortTransactionOperation();
                    await ExecuteEndTransactionOnPrimaryAsync(secondAttempt, cancellationToken).ConfigureAwait(false);
                }
                catch
                {
                    return; // ignore exception and return
                }
            }
            finally
            {
                _currentTransaction.SetState(CoreTransactionState.Aborted);
            }
        }

        /// <inheritdoc />
        public void AboutToSendCommand()
        {
            if (_currentTransaction != null)
            {
                switch (_currentTransaction.State)
                {
                    case CoreTransactionState.Starting: // Starting changes to InProgress after the message is sent to the server
                    case CoreTransactionState.InProgress:
                        return;

                    case CoreTransactionState.Aborted:
                        _currentTransaction = null;
                        break;

                    case CoreTransactionState.Committed:
                        // don't set to null when retrying a commit
                        if (!_isCommitTransactionInProgress)
                        {
                            _currentTransaction = null;
                        }
                        return;

                    default:
                        throw new Exception($"Unexpected transaction state: {_currentTransaction.State}.");
                }
            }
        }

        /// <inheritdoc />
        public void AdvanceClusterTime(BsonDocument newClusterTime)
        {
            _clusterClock.AdvanceClusterTime(newClusterTime);
        }

        /// <inheritdoc />
        public void AdvanceOperationTime(BsonTimestamp newOperationTime)
        {
            _operationClock.AdvanceOperationTime(newOperationTime);
        }

        /// <inheritdoc />
        public long AdvanceTransactionNumber()
        {
            return _serverSession.AdvanceTransactionNumber();
        }

        /// <inheritdoc />
        public void CommitTransaction(CancellationToken cancellationToken = default(CancellationToken))
        {
            EnsureCommitTransactionCanBeCalled(nameof(CommitTransaction));

            try
            {
                _isCommitTransactionInProgress = true;
                if (_currentTransaction.IsEmpty)
                {
                    return;
                }

                try
                {
                    var firstAttempt = CreateCommitTransactionOperation();
                    ExecuteEndTransactionOnPrimary(firstAttempt, cancellationToken);
                    return;
                }
                catch (Exception exception) when (ShouldRetryEndTransactionException(exception))
                {
                    // ignore exception and retry
                }

                var secondAttempt = CreateCommitTransactionOperation();
                ExecuteEndTransactionOnPrimary(secondAttempt, cancellationToken);
            }
            finally
            {
                _isCommitTransactionInProgress = false;
                _currentTransaction.SetState(CoreTransactionState.Committed);
            }
        }

        /// <inheritdoc />
        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            EnsureCommitTransactionCanBeCalled(nameof(CommitTransaction));

            try
            {
                _isCommitTransactionInProgress = true;
                if (_currentTransaction.IsEmpty)
                {
                    return;
                }

                try
                {
                    var firstAttempt = CreateCommitTransactionOperation();
                    await ExecuteEndTransactionOnPrimaryAsync(firstAttempt, cancellationToken).ConfigureAwait(false);
                    return;
                }
                catch (Exception exception) when (ShouldRetryEndTransactionException(exception))
                {
                    // ignore exception and retry
                }

                var secondAttempt = CreateCommitTransactionOperation();
                await ExecuteEndTransactionOnPrimaryAsync(secondAttempt, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                _isCommitTransactionInProgress = false;
                _currentTransaction.SetState(CoreTransactionState.Committed);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (!_disposed)
            {
                if (_currentTransaction != null)
                {
                    switch (_currentTransaction.State)
                    {
                        case CoreTransactionState.Starting:
                        case CoreTransactionState.InProgress:
                            try
                            {
                                AbortTransaction(CancellationToken.None);
                            }
                            catch
                            {
                                // ignore exceptions
                            }
                            break;
                    }
                }

                _serverSession.Dispose();
                _disposed = true;
            }
        }

        /// <inheritdoc />
        public void StartTransaction(TransactionOptions transactionOptions = null)
        {
            EnsureStartTransactionCanBeCalled();

            var transactionNumber = AdvanceTransactionNumber();
            var effectiveTransactionOptions = GetEffectiveTransactionOptions(transactionOptions);
            if (!effectiveTransactionOptions.WriteConcern.IsAcknowledged)
            {
                throw new InvalidOperationException("Transactions do not support unacknowledged write concerns.");
            }

            _currentTransaction = new CoreTransaction(transactionNumber, effectiveTransactionOptions);
        }

        /// <inheritdoc />
        public void WasUsed()
        {
            _serverSession.WasUsed();
        }

        // private methods
        private IReadOperation<BsonDocument> CreateAbortTransactionOperation()
        {
            return new AbortTransactionOperation(GetTransactionWriteConcern());
        }

        private IReadOperation<BsonDocument> CreateCommitTransactionOperation()
        {
            return new CommitTransactionOperation(GetTransactionWriteConcern());
        }

        private void EnsureAbortTransactionCanBeCalled(string methodName)
        {
            if (_currentTransaction == null)
            {
                throw new InvalidOperationException($"{methodName} cannot be called when no transaction started.");
            }

            switch (_currentTransaction.State)
            {
                case CoreTransactionState.Starting:
                case CoreTransactionState.InProgress:
                    return;

                case CoreTransactionState.Aborted:
                    throw new InvalidOperationException($"Cannot call {methodName} twice.");

                case CoreTransactionState.Committed:
                    throw new InvalidOperationException($"Cannot call {methodName} after calling CommitTransaction.");

                default:
                    throw new Exception($"{methodName} called in unexpected transaction state: {_currentTransaction.State}.");
            }
        }

        private void EnsureCommitTransactionCanBeCalled(string methodName)
        {
            if (_currentTransaction == null)
            {
                throw new InvalidOperationException($"{methodName} cannot be called when no transaction started.");
            }

            switch (_currentTransaction.State)
            {
                case CoreTransactionState.Starting:
                case CoreTransactionState.InProgress:
                case CoreTransactionState.Committed:
                    return;

                case CoreTransactionState.Aborted:
                    throw new InvalidOperationException($"Cannot call {methodName} after calling AbortTransaction.");

                default:
                    throw new Exception($"{methodName} called in unexpected transaction state: {_currentTransaction.State}.");
            }
        }

        private void EnsureStartTransactionCanBeCalled()
        {
            if (_currentTransaction == null)
            {
                return;
            }

            switch (_currentTransaction.State)
            {
                case CoreTransactionState.Aborted:
                case CoreTransactionState.Committed:
                    return;

                default:
                    throw new InvalidOperationException("Transaction already in progress.");
            }
        }

        private TResult ExecuteEndTransactionOnPrimary<TResult>(IReadOperation<TResult> operation, CancellationToken cancellationToken)
        {
            using (var sessionHandle = new NonDisposingCoreSessionHandle(this))
            using (var binding = new WritableServerBinding(_cluster, sessionHandle))
            {
                return operation.Execute(binding, cancellationToken);
            }
        }

        private async Task<TResult> ExecuteEndTransactionOnPrimaryAsync<TResult>(IReadOperation<TResult> operation, CancellationToken cancellationToken)
        {
            using (var sessionHandle = new NonDisposingCoreSessionHandle(this))
            using (var binding = new WritableServerBinding(_cluster, sessionHandle))
            {
                return await operation.ExecuteAsync(binding, cancellationToken).ConfigureAwait(false);
            }
        }

        private TransactionOptions GetEffectiveTransactionOptions(TransactionOptions transactionOptions)
        {
            var readConcern = transactionOptions?.ReadConcern ?? _options.DefaultTransactionOptions?.ReadConcern ?? ReadConcern.Default;
            var readPreference = transactionOptions?.ReadPreference ?? _options.DefaultTransactionOptions?.ReadPreference ?? ReadPreference.Primary;
            var writeConcern = transactionOptions?.WriteConcern ?? _options.DefaultTransactionOptions?.WriteConcern ?? new WriteConcern();
            return new TransactionOptions(readConcern, readPreference, writeConcern);
        }

        private WriteConcern GetTransactionWriteConcern()
        {
            return
                _currentTransaction.TransactionOptions?.WriteConcern ??
                _options.DefaultTransactionOptions?.WriteConcern ??
                WriteConcern.WMajority;
        }

        private bool ShouldRetryEndTransactionException(Exception exception)
        {
            return RetryabilityHelper.IsRetryableWriteException(exception);
        }
    }
}

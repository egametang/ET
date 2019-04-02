/* Copyright 2013-present MongoDB Inc.
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
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Core.Bindings;
using MongoDB.Driver.Core.Clusters;
using MongoDB.Driver.Core.Configuration;
using MongoDB.Driver.Core.ConnectionPools;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Events;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.WireProtocol;
using MongoDB.Driver.Core.WireProtocol.Messages;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders;

namespace MongoDB.Driver.Core.Servers
{
    /// <summary>
    /// Represents a server in a MongoDB cluster.
    /// </summary>
    internal sealed class Server : IClusterableServer
    {
        #region static
        // static fields
        private static readonly List<Type> __invalidatingExceptions = new List<Type>
        {
            typeof(MongoNotPrimaryException),
            typeof(MongoNodeIsRecoveringException),
            typeof(MongoConnectionException),
            typeof(SocketException),
            typeof(EndOfStreamException),
            typeof(IOException),
        };
        #endregion

        // fields
        private readonly IClusterClock _clusterClock;
        private readonly ClusterConnectionMode _clusterConnectionMode;
        private IConnectionPool _connectionPool;
        private readonly EndPoint _endPoint;
        private readonly IServerMonitor _monitor;
        private readonly ServerId _serverId;
        private readonly ServerSettings _settings;
        private readonly InterlockedInt32 _state;

        private readonly Action<ServerOpeningEvent> _openingEventHandler;
        private readonly Action<ServerOpenedEvent> _openedEventHandler;
        private readonly Action<ServerClosingEvent> _closingEventHandler;
        private readonly Action<ServerClosedEvent> _closedEventHandler;
        private readonly Action<ServerDescriptionChangedEvent> _descriptionChangedEventHandler;

        // events
        public event EventHandler<ServerDescriptionChangedEventArgs> DescriptionChanged;

        // constructors
        public Server(ClusterId clusterId, IClusterClock clusterClock, ClusterConnectionMode clusterConnectionMode, ServerSettings settings, EndPoint endPoint, IConnectionPoolFactory connectionPoolFactory, IServerMonitorFactory serverMonitorFactory, IEventSubscriber eventSubscriber)
        {
            Ensure.IsNotNull(clusterId, nameof(clusterId));
            _clusterClock = Ensure.IsNotNull(clusterClock, nameof(clusterClock));
            _clusterConnectionMode = clusterConnectionMode;
            _settings = Ensure.IsNotNull(settings, nameof(settings));
            _endPoint = Ensure.IsNotNull(endPoint, nameof(endPoint));
            Ensure.IsNotNull(connectionPoolFactory, nameof(connectionPoolFactory));
            Ensure.IsNotNull(serverMonitorFactory, nameof(serverMonitorFactory));
            Ensure.IsNotNull(eventSubscriber, nameof(eventSubscriber));

            _serverId = new ServerId(clusterId, endPoint);
            _connectionPool = connectionPoolFactory.CreateConnectionPool(_serverId, endPoint);
            _state = new InterlockedInt32(State.Initial);
            _monitor = serverMonitorFactory.Create(_serverId, _endPoint);

            eventSubscriber.TryGetEventHandler(out _openingEventHandler);
            eventSubscriber.TryGetEventHandler(out _openedEventHandler);
            eventSubscriber.TryGetEventHandler(out _closingEventHandler);
            eventSubscriber.TryGetEventHandler(out _closedEventHandler);
            eventSubscriber.TryGetEventHandler(out _descriptionChangedEventHandler);
        }

        // properties
        public ServerDescription Description => _monitor.Description;

        public EndPoint EndPoint => _endPoint;

        public bool IsInitialized => _state.Value != State.Initial;

        public ServerId ServerId => _serverId;

        internal IClusterClock ClusterClock => _clusterClock;

        // methods
        public void Dispose()
        {
            if (_state.TryChange(State.Disposed))
            {
                if (_closingEventHandler != null)
                {
                    _closingEventHandler(new ServerClosingEvent(_serverId));
                }

                var stopwatch = Stopwatch.StartNew();
                _monitor.Dispose();
                _monitor.DescriptionChanged -= OnDescriptionChanged;
                _connectionPool.Dispose();
                stopwatch.Stop();

                if (_closedEventHandler != null)
                {
                    _closedEventHandler(new ServerClosedEvent(_serverId, stopwatch.Elapsed));
                }
            }
        }

        public IChannelHandle GetChannel(CancellationToken cancellationToken)
        {
            ThrowIfNotOpen();

            var connection = _connectionPool.AcquireConnection(cancellationToken);
            try
            {
                // ignoring the user's cancellation token here because we don't
                // want to throw this connection away simply because the user
                // wanted to cancel their operation. It will be better for the
                // collective to complete opening the connection than the throw
                // it away.
                connection.Open(CancellationToken.None);
                return new ServerChannel(this, connection);
            }
            catch (Exception ex)
            {
                if (ShouldClearConnectionPool(ex))
                {
                    try
                    {
                        _connectionPool.Clear();
                    }
                    catch
                    {                        
                        // ignore exceptions
                    }
                    
                }
                connection.Dispose();
                throw;
            }
        }

        public async Task<IChannelHandle> GetChannelAsync(CancellationToken cancellationToken)
        {
            ThrowIfNotOpen();

            var connection = await _connectionPool.AcquireConnectionAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                // ignoring the user's cancellation token here because we don't
                // want to throw this connection away simply because the user
                // wanted to cancel their operation. It will be better for the
                // collective to complete opening the connection than the throw
                // it away.
                await connection.OpenAsync(CancellationToken.None).ConfigureAwait(false);
                return new ServerChannel(this, connection);
            }
            catch (Exception ex)
            {
                if (ShouldClearConnectionPool(ex))
                {
                    try
                    {
                        _connectionPool.Clear();
                    }
                    catch
                    {                        
                        // ignore exceptions
                    }
                }
                connection.Dispose();
                throw;
            }
        }

        public void Initialize()
        {
            if (_state.TryChange(State.Initial, State.Open))
            {
                if (_openingEventHandler != null)
                {
                    _openingEventHandler(new ServerOpeningEvent(_serverId, _settings));
                }

                var stopwatch = Stopwatch.StartNew();
                _connectionPool.Initialize();
                _monitor.DescriptionChanged += OnDescriptionChanged;
                _monitor.Initialize();
                stopwatch.Stop();

                if (_openedEventHandler != null)
                {
                    _openedEventHandler(new ServerOpenedEvent(_serverId, _settings, stopwatch.Elapsed));
                }
            }
        }

        public void Invalidate()
        {
            ThrowIfNotOpen();
            _connectionPool.Clear();
            _monitor.Invalidate();
        }

        public void RequestHeartbeat()
        {
            ThrowIfNotOpen();
            _monitor.RequestHeartbeat();
        }

        private void OnDescriptionChanged(object sender, ServerDescriptionChangedEventArgs e)
        {
            if (e.NewServerDescription.HeartbeatException != null)
            {
                _connectionPool.Clear();
            }

            if (_descriptionChangedEventHandler != null)
            {
                _descriptionChangedEventHandler(new ServerDescriptionChangedEvent(e.OldServerDescription, e.NewServerDescription));
            }

            var handler = DescriptionChanged;
            if (handler != null)
            {
                try { handler(this, e); }
                catch { } // ignore exceptions
            }
        }

        private void HandleChannelException(IConnection connection, Exception ex)
        {
            if (_state.Value != State.Open)
            {
                return;
            }

            var aggregateException = ex as AggregateException;
            if (aggregateException != null && aggregateException.InnerExceptions.Count == 1)
            {
                ex = aggregateException.InnerException;
            }

            // For most connection exceptions, we are going to immediately
            // invalidate the server. However, we aren't going to invalidate
            // because of OperationCanceledExceptions. We trust that the
            // implementations of connection don't leave themselves in a state
            // where they can't be used based on user cancellation.
            if (ex.GetType() == typeof(OperationCanceledException))
            {
                return;
            }

            if (ShouldInvalidateServer(ex))
            {
                Invalidate();
            }
            else
            {
                RequestHeartbeat();
            }
        }

        private bool IsNotMaster(ServerErrorCode code, string message)
        {
            switch (code)
            {
                case ServerErrorCode.NotMaster: // 10107
                case ServerErrorCode.NotMasterNoSlaveOk: // 13435
                    return true;
            }

            if (message != null)
            {
                if (message.IndexOf("not master", StringComparison.OrdinalIgnoreCase) != -1 &&
                    message.IndexOf("not master or secondary", StringComparison.OrdinalIgnoreCase) == -1)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsNotMasterOrRecovering(ServerErrorCode code, string message)
        {
            return IsNotMaster(code, message) || IsRecovering(code, message);
        }

        private bool IsRecovering(ServerErrorCode code, string message)
        {
            switch (code)
            {
                case ServerErrorCode.InterruptedAtShutdown: // 11600
                case ServerErrorCode.InterruptedDueToReplStateChange: // 11602
                case ServerErrorCode.NotMasterOrSecondary: // 13436
                case ServerErrorCode.PrimarySteppedDown: // 189
                case ServerErrorCode.ShutdownInProgress: // 91
                    return true;
            }

            if (message != null)
            {
                if (message.IndexOf("not master or secondary", StringComparison.OrdinalIgnoreCase) != -1 ||
                    message.IndexOf("node is recovering", StringComparison.OrdinalIgnoreCase) != -1)
                {
                    return true;
                }
            }

            return false;
        }
        
        private bool ShouldClearConnectionPool(Exception ex)
        {
            return ex is MongoAuthenticationException;
        }

        private bool ShouldInvalidateServer(Exception exception)
        {
            if (__invalidatingExceptions.Contains(exception.GetType()))
            {
                return true;
            }

            var commandException = exception as MongoCommandException;
            if (commandException != null)
            {
                var code = (ServerErrorCode)commandException.Code;
                var message = commandException.ErrorMessage;

                if (IsNotMasterOrRecovering(code, message))
                {
                    return true;
                }

                if (commandException.GetType() == typeof(MongoWriteConcernException))
                {
                    var writeConcernException = (MongoWriteConcernException)commandException;
                    var writeConcernResult = writeConcernException.WriteConcernResult;
                    var response = writeConcernResult.Response;
                    var writeConcernError = response["writeConcernError"].AsBsonDocument;
                    if (writeConcernError != null)
                    {
                        code = (ServerErrorCode)writeConcernError.GetValue("code", -1).ToInt32();
                        message = writeConcernError.GetValue("errmsg", null)?.AsString;

                        if (IsNotMasterOrRecovering(code, message))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private void ThrowIfDisposed()
        {
            if (_state.Value == State.Disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        private void ThrowIfNotOpen()
        {
            if (_state.Value != State.Open)
            {
                ThrowIfDisposed();
                throw new InvalidOperationException("Server must be initialized.");
            }
        }

        // nested types
        private static class State
        {
            public const int Initial = 0;
            public const int Open = 1;
            public const int Disposed = 2;
        }

        private sealed class ServerChannel : IChannelHandle
        {
            // fields
            private readonly IConnectionHandle _connection;
            private bool _disposed;
            private readonly Server _server;

            // constructors
            public ServerChannel(Server server, IConnectionHandle connection)
            {
                _server = server;
                _connection = connection;
            }

            // properties
            public ConnectionDescription ConnectionDescription
            {
                get { return _connection.Description; }
            }

            // methods
            [Obsolete("Use the newest overload instead.")]
            public TResult Command<TResult>(
                DatabaseNamespace databaseNamespace,
                BsonDocument command,
                IElementNameValidator commandValidator,
                Func<CommandResponseHandling> responseHandling,
                bool slaveOk,
                IBsonSerializer<TResult> resultSerializer,
                MessageEncoderSettings messageEncoderSettings,
                CancellationToken cancellationToken)
            {
                var readPreference = GetEffectiveReadPreference(slaveOk, null);
                var result = Command(
                    NoCoreSession.Instance,
                    readPreference,
                    databaseNamespace,
                    command,
                    null, // commandPayloads
                    commandValidator,
                    null, // additionalOptions
                    null, // postWriteAction
                    CommandResponseHandling.Return,
                    resultSerializer,
                    messageEncoderSettings,
                    cancellationToken);

                if (responseHandling != null && responseHandling() != CommandResponseHandling.Return)
                {
                    throw new NotSupportedException("This overload requires responseHandling to be: Return.");
                }

                return result;
            }

            [Obsolete("Use the newest overload instead.")]
            public TResult Command<TResult>(
                ICoreSession session,
                ReadPreference readPreference,
                DatabaseNamespace databaseNamespace,
                BsonDocument command,
                IElementNameValidator commandValidator,
                BsonDocument additionalOptions,
                Func<CommandResponseHandling> responseHandling,
                bool slaveOk,
                IBsonSerializer<TResult> resultSerializer,
                MessageEncoderSettings messageEncoderSettings,
                CancellationToken cancellationToken)
            {
                readPreference = GetEffectiveReadPreference(slaveOk, readPreference);
                var result = Command(
                    session,
                    readPreference,
                    databaseNamespace,
                    command,
                    null, // commandPayloads
                    commandValidator,
                    additionalOptions,
                    null, // postWriteActions
                    CommandResponseHandling.Return,
                    resultSerializer,
                    messageEncoderSettings,
                    cancellationToken);

                if (responseHandling != null && responseHandling() != CommandResponseHandling.Return)
                {
                    throw new NotSupportedException("This overload requires responseHandling to be: Return.");
                }

                return result;
            }

            public TResult Command<TResult>(
                ICoreSession session,
                ReadPreference readPreference,
                DatabaseNamespace databaseNamespace,
                BsonDocument command,
                IEnumerable<Type1CommandMessageSection> commandPayloads,
                IElementNameValidator commandValidator,
                BsonDocument additionalOptions,
                Action<IMessageEncoderPostProcessor> postWriteAction,
                CommandResponseHandling responseHandling,
                IBsonSerializer<TResult> resultSerializer,
                MessageEncoderSettings messageEncoderSettings,
                CancellationToken cancellationToken)
            {
                var protocol = new CommandWireProtocol<TResult>(
                    CreateClusterClockAdvancingCoreSession(session),
                    readPreference,
                    databaseNamespace,
                    command,
                    commandPayloads,
                    commandValidator,
                    additionalOptions,
                    postWriteAction,
                    responseHandling,
                    resultSerializer,
                    messageEncoderSettings);

                return ExecuteProtocol(protocol, cancellationToken);
            }

            [Obsolete("Use the newest overload instead.")]
            public Task<TResult> CommandAsync<TResult>(
                DatabaseNamespace databaseNamespace,
                BsonDocument command,
                IElementNameValidator commandValidator,
                Func<CommandResponseHandling> responseHandling,
                bool slaveOk,
                IBsonSerializer<TResult> resultSerializer,
                MessageEncoderSettings messageEncoderSettings,
                CancellationToken cancellationToken)
            {
                var readPreference = GetEffectiveReadPreference(slaveOk, null);
                var result = CommandAsync(
                    NoCoreSession.Instance,
                    readPreference,
                    databaseNamespace,
                    command,
                    null, // commandPayloads
                    commandValidator,
                    null, // additionalOptions
                    null, // postWriteAction
                    CommandResponseHandling.Return,
                    resultSerializer,
                    messageEncoderSettings,
                    cancellationToken);

                if (responseHandling != null && responseHandling() != CommandResponseHandling.Return)
                {
                    throw new NotSupportedException("This overload requires responseHandling to be 'Return'.");
                }

                return result;
            }

            [Obsolete("Use the newest overload instead.")]
            public Task<TResult> CommandAsync<TResult>(
                ICoreSession session,
                ReadPreference readPreference,
                DatabaseNamespace databaseNamespace,
                BsonDocument command,
                IElementNameValidator commandValidator,
                BsonDocument additionalOptions,
                Func<CommandResponseHandling> responseHandling,
                bool slaveOk,
                IBsonSerializer<TResult> resultSerializer,
                MessageEncoderSettings messageEncoderSettings,
                CancellationToken cancellationToken)
            {
                readPreference = GetEffectiveReadPreference(slaveOk, readPreference);
                var result = CommandAsync(
                    session,
                    readPreference,
                    databaseNamespace,
                    command,
                    null, // commandPayloads
                    commandValidator,
                    additionalOptions,
                    null, // postWriteAction
                    CommandResponseHandling.Return,
                    resultSerializer,
                    messageEncoderSettings,
                    cancellationToken);

                if (responseHandling != null && responseHandling() != CommandResponseHandling.Return)
                {
                    throw new NotSupportedException("This overload requires responseHandling to be 'Return'.");
                }

                return result;
            }

            public Task<TResult> CommandAsync<TResult>(
                ICoreSession session,
                ReadPreference readPreference,
                DatabaseNamespace databaseNamespace,
                BsonDocument command,
                IEnumerable<Type1CommandMessageSection> commandPayloads,
                IElementNameValidator commandValidator,
                BsonDocument additionalOptions,
                Action<IMessageEncoderPostProcessor> postWriteAction,
                CommandResponseHandling responseHandling,
                IBsonSerializer<TResult> resultSerializer,
                MessageEncoderSettings messageEncoderSettings,
                CancellationToken cancellationToken)
            {
                var protocol = new CommandWireProtocol<TResult>(
                    CreateClusterClockAdvancingCoreSession(session),
                    readPreference,
                    databaseNamespace,
                    command,
                    commandPayloads,
                    commandValidator,
                    additionalOptions,
                    postWriteAction,
                    responseHandling,
                    resultSerializer,
                    messageEncoderSettings);

                return ExecuteProtocolAsync(protocol, cancellationToken);
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    _connection.Dispose();
                    _disposed = true;
                }
            }

            public WriteConcernResult Delete(
                CollectionNamespace collectionNamespace,
                BsonDocument query,
                bool isMulti,
                MessageEncoderSettings messageEncoderSettings,
                WriteConcern writeConcern,
                CancellationToken cancellationToken)
            {
                var protocol = new DeleteWireProtocol(
                    collectionNamespace,
                    query,
                    isMulti,
                    messageEncoderSettings,
                    writeConcern);

                return ExecuteProtocol(protocol, cancellationToken);
            }

            public Task<WriteConcernResult> DeleteAsync(
                CollectionNamespace collectionNamespace,
                BsonDocument query,
                bool isMulti,
                MessageEncoderSettings messageEncoderSettings,
                WriteConcern writeConcern,
                CancellationToken cancellationToken)
            {
                var protocol = new DeleteWireProtocol(
                    collectionNamespace,
                    query,
                    isMulti,
                    messageEncoderSettings,
                    writeConcern);

                return ExecuteProtocolAsync(protocol, cancellationToken);
            }

            public CursorBatch<TDocument> GetMore<TDocument>(
                CollectionNamespace collectionNamespace,
                BsonDocument query,
                long cursorId,
                int batchSize,
                IBsonSerializer<TDocument> serializer,
                MessageEncoderSettings messageEncoderSettings,
                CancellationToken cancellationToken)
            {
                var protocol = new GetMoreWireProtocol<TDocument>(
                    collectionNamespace,
                    query,
                    cursorId,
                    batchSize,
                    serializer,
                    messageEncoderSettings);

                return ExecuteProtocol(protocol, cancellationToken);
            }

            public Task<CursorBatch<TDocument>> GetMoreAsync<TDocument>(
              CollectionNamespace collectionNamespace,
              BsonDocument query,
              long cursorId,
              int batchSize,
              IBsonSerializer<TDocument> serializer,
              MessageEncoderSettings messageEncoderSettings,
              CancellationToken cancellationToken)
            {
                var protocol = new GetMoreWireProtocol<TDocument>(
                    collectionNamespace,
                    query,
                    cursorId,
                    batchSize,
                    serializer,
                    messageEncoderSettings);

                return ExecuteProtocolAsync(protocol, cancellationToken);
            }

            public WriteConcernResult Insert<TDocument>(
                CollectionNamespace collectionNamespace,
                WriteConcern writeConcern,
                IBsonSerializer<TDocument> serializer,
                MessageEncoderSettings messageEncoderSettings,
                BatchableSource<TDocument> documentSource,
                int? maxBatchCount,
                int? maxMessageSize,
                bool continueOnError,
                Func<bool> shouldSendGetLastError,
                CancellationToken cancellationToken)
            {
                var protocol = new InsertWireProtocol<TDocument>(
                    collectionNamespace,
                    writeConcern,
                    serializer,
                    messageEncoderSettings,
                    documentSource,
                    maxBatchCount,
                    maxMessageSize,
                    continueOnError,
                    shouldSendGetLastError);

                return ExecuteProtocol(protocol, cancellationToken);
            }

            public Task<WriteConcernResult> InsertAsync<TDocument>(
               CollectionNamespace collectionNamespace,
               WriteConcern writeConcern,
               IBsonSerializer<TDocument> serializer,
               MessageEncoderSettings messageEncoderSettings,
               BatchableSource<TDocument> documentSource,
               int? maxBatchCount,
               int? maxMessageSize,
               bool continueOnError,
               Func<bool> shouldSendGetLastError,
               CancellationToken cancellationToken)
            {
                var protocol = new InsertWireProtocol<TDocument>(
                    collectionNamespace,
                    writeConcern,
                    serializer,
                    messageEncoderSettings,
                    documentSource,
                    maxBatchCount,
                    maxMessageSize,
                    continueOnError,
                    shouldSendGetLastError);

                return ExecuteProtocolAsync(protocol, cancellationToken);
            }

            public void KillCursors(
                IEnumerable<long> cursorIds,
                MessageEncoderSettings messageEncoderSettings,
                CancellationToken cancellationToken)
            {
                var protocol = new KillCursorsWireProtocol(
                    cursorIds,
                    messageEncoderSettings);

                ExecuteProtocol(protocol, cancellationToken);
            }

            public Task KillCursorsAsync(
              IEnumerable<long> cursorIds,
              MessageEncoderSettings messageEncoderSettings,
              CancellationToken cancellationToken)
            {
                var protocol = new KillCursorsWireProtocol(
                    cursorIds,
                    messageEncoderSettings);

                return ExecuteProtocolAsync(protocol, cancellationToken);
            }

            public CursorBatch<TDocument> Query<TDocument>(
                CollectionNamespace collectionNamespace,
                BsonDocument query,
                BsonDocument fields,
                IElementNameValidator queryValidator,
                int skip,
                int batchSize,
                bool slaveOk,
                bool partialOk,
                bool noCursorTimeout,
                bool oplogReplay,
                bool tailableCursor,
                bool awaitData,
                IBsonSerializer<TDocument> serializer,
                MessageEncoderSettings messageEncoderSettings,
                CancellationToken cancellationToken)
            {
                slaveOk = GetEffectiveSlaveOk(slaveOk);
                var protocol = new QueryWireProtocol<TDocument>(
                    collectionNamespace,
                    query,
                    fields,
                    queryValidator,
                    skip,
                    batchSize,
                    slaveOk,
                    partialOk,
                    noCursorTimeout,
                    oplogReplay,
                    tailableCursor,
                    awaitData,
                    serializer,
                    messageEncoderSettings);

                return ExecuteProtocol(protocol, cancellationToken);
            }

            public Task<CursorBatch<TDocument>> QueryAsync<TDocument>(
             CollectionNamespace collectionNamespace,
             BsonDocument query,
             BsonDocument fields,
             IElementNameValidator queryValidator,
             int skip,
             int batchSize,
             bool slaveOk,
             bool partialOk,
             bool noCursorTimeout,
             bool oplogReplay,
             bool tailableCursor,
             bool awaitData,
             IBsonSerializer<TDocument> serializer,
             MessageEncoderSettings messageEncoderSettings,
             CancellationToken cancellationToken)
            {
                slaveOk = GetEffectiveSlaveOk(slaveOk);
                var protocol = new QueryWireProtocol<TDocument>(
                    collectionNamespace,
                    query,
                    fields,
                    queryValidator,
                    skip,
                    batchSize,
                    slaveOk,
                    partialOk,
                    noCursorTimeout,
                    oplogReplay,
                    tailableCursor,
                    awaitData,
                    serializer,
                    messageEncoderSettings);

                return ExecuteProtocolAsync(protocol, cancellationToken);
            }

            public WriteConcernResult Update(
                CollectionNamespace collectionNamespace,
                MessageEncoderSettings messageEncoderSettings,
                WriteConcern writeConcern,
                BsonDocument query,
                BsonDocument update,
                IElementNameValidator updateValidator,
                bool isMulti,
                bool isUpsert,
                CancellationToken cancellationToken)
            {
                var protocol = new UpdateWireProtocol(
                    collectionNamespace,
                    messageEncoderSettings,
                    writeConcern,
                    query,
                    update,
                    updateValidator,
                    isMulti,
                    isUpsert);

                return ExecuteProtocol(protocol, cancellationToken);
            }

            public Task<WriteConcernResult> UpdateAsync(
               CollectionNamespace collectionNamespace,
               MessageEncoderSettings messageEncoderSettings,
               WriteConcern writeConcern,
               BsonDocument query,
               BsonDocument update,
               IElementNameValidator updateValidator,
               bool isMulti,
               bool isUpsert,
               CancellationToken cancellationToken)
            {
                var protocol = new UpdateWireProtocol(
                    collectionNamespace,
                    messageEncoderSettings,
                    writeConcern,
                    query,
                    update,
                    updateValidator,
                    isMulti,
                    isUpsert);

                return ExecuteProtocolAsync(protocol, cancellationToken);
            }

            private ICoreSession CreateClusterClockAdvancingCoreSession(ICoreSession session)
            {
                return new ClusterClockAdvancingCoreSession(session, _server.ClusterClock);
            }

            private void ExecuteProtocol(IWireProtocol protocol, CancellationToken cancellationToken)
            {
                try
                {
                    protocol.Execute(_connection, cancellationToken);
                }
                catch (Exception ex)
                {
                    _server.HandleChannelException(_connection, ex);
                    throw;
                }
            }

            private TResult ExecuteProtocol<TResult>(IWireProtocol<TResult> protocol, CancellationToken cancellationToken)
            {
                try
                {
                    return protocol.Execute(_connection, cancellationToken);
                }
                catch (Exception ex)
                {
                    _server.HandleChannelException(_connection, ex);
                    throw;
                }
            }

            private async Task ExecuteProtocolAsync(IWireProtocol protocol, CancellationToken cancellationToken)
            {
                try
                {
                    await protocol.ExecuteAsync(_connection, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _server.HandleChannelException(_connection, ex);
                    throw;
                }
            }

            private async Task<TResult> ExecuteProtocolAsync<TResult>(IWireProtocol<TResult> protocol, CancellationToken cancellationToken)
            {
                try
                {
                    return await protocol.ExecuteAsync(_connection, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _server.HandleChannelException(_connection, ex);
                    throw;
                }
            }

            public IChannelHandle Fork()
            {
                ThrowIfDisposed();
                return new ServerChannel(_server, _connection.Fork());
            }

            private ReadPreference GetEffectiveReadPreference(bool slaveOk, ReadPreference readPreference)
            {
                if (_server._clusterConnectionMode == ClusterConnectionMode.Direct && _server.Description.Type != ServerType.ShardRouter)
                {
                    return ReadPreference.PrimaryPreferred;
                }

                if (readPreference == null)
                {
                    return slaveOk ? ReadPreference.SecondaryPreferred : ReadPreference.Primary;
                }

                var impliedSlaveOk = readPreference.ReadPreferenceMode != ReadPreferenceMode.Primary;
                if (slaveOk != impliedSlaveOk)
                {
                    throw new ArgumentException($"slaveOk {slaveOk} is inconsistent with read preference mode: {readPreference.ReadPreferenceMode}.");
                }

                return readPreference;
            }

            private bool GetEffectiveSlaveOk(bool slaveOk)
            {
                if (_server._clusterConnectionMode == ClusterConnectionMode.Direct && _server.Description.Type != ServerType.ShardRouter)
                {
                    return true;
                }

                return slaveOk;
            }

            private void ThrowIfDisposed()
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }
            }
        }
    }
}
/* Copyright 2016 MongoDB Inc.
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
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Events;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.WireProtocol;

namespace MongoDB.Driver.Core.Servers
{
    internal sealed class ServerMonitor : IServerMonitor
    {
        private static readonly TimeSpan __minHeartbeatInterval = TimeSpan.FromMilliseconds(500);

        private readonly ExponentiallyWeightedMovingAverage _averageRoundTripTimeCalculator = new ExponentiallyWeightedMovingAverage(0.2);
        private readonly ServerDescription _baseDescription;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private volatile IConnection _connection;
        private readonly IConnectionFactory _connectionFactory;
        private ServerDescription _currentDescription;
        private readonly EndPoint _endPoint;
        private HeartbeatDelay _heartbeatDelay;
        private readonly TimeSpan _heartbeatInterval;
        private readonly ServerId _serverId;
        private readonly InterlockedInt32 _state;
        private readonly TimeSpan _timeout;

        private readonly Action<ServerHeartbeatStartedEvent> _heartbeatStartedEventHandler;
        private readonly Action<ServerHeartbeatSucceededEvent> _heartbeatSucceededEventHandler;
        private readonly Action<ServerHeartbeatFailedEvent> _heartbeatFailedEventHandler;

        public event EventHandler<ServerDescriptionChangedEventArgs> DescriptionChanged;

        public ServerMonitor(ServerId serverId, EndPoint endPoint, IConnectionFactory connectionFactory, TimeSpan heartbeatInterval, TimeSpan timeout, IEventSubscriber eventSubscriber)
        {
            _serverId = Ensure.IsNotNull(serverId, nameof(serverId));
            _endPoint = Ensure.IsNotNull(endPoint, nameof(endPoint));
            _connectionFactory = Ensure.IsNotNull(connectionFactory, nameof(connectionFactory));
            Ensure.IsNotNull(eventSubscriber, nameof(eventSubscriber));

            _baseDescription = _currentDescription = new ServerDescription(_serverId, endPoint, heartbeatInterval: heartbeatInterval);
            _heartbeatInterval = heartbeatInterval;
            _timeout = timeout;
            _state = new InterlockedInt32(State.Initial);
            eventSubscriber.TryGetEventHandler(out _heartbeatStartedEventHandler);
            eventSubscriber.TryGetEventHandler(out _heartbeatSucceededEventHandler);
            eventSubscriber.TryGetEventHandler(out _heartbeatFailedEventHandler);
        }

        public ServerDescription Description => Interlocked.CompareExchange(ref _currentDescription, null, null);

        public void Dispose()
        {
            if (_state.TryChange(State.Disposed))
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
                if (_connection != null)
                {
                    _connection.Dispose();
                }
            }
        }

        public void Initialize()
        {
            if (_state.TryChange(State.Initial, State.Open))
            {
                MonitorServerAsync().ConfigureAwait(false);
            }
        }

        public void Invalidate()
        {
            OnDescriptionChanged(_baseDescription);
            RequestHeartbeat();
        }

        public void RequestHeartbeat()
        {
            ThrowIfNotOpen();
            var heartbeatDelay = Interlocked.CompareExchange(ref _heartbeatDelay, null, null);
            if (heartbeatDelay != null)
            {
                heartbeatDelay.RequestHeartbeat();
            }
        }

        private async Task MonitorServerAsync()
        {
            var metronome = new Metronome(_heartbeatInterval);
            var heartbeatCancellationToken = _cancellationTokenSource.Token;
            while (!heartbeatCancellationToken.IsCancellationRequested)
            {
                try
                {
                    await HeartbeatAsync(heartbeatCancellationToken).ConfigureAwait(false);
                    var newHeartbeatDelay = new HeartbeatDelay(metronome.GetNextTickDelay(), __minHeartbeatInterval);
                    var oldHeartbeatDelay = Interlocked.Exchange(ref _heartbeatDelay, newHeartbeatDelay);
                    if (oldHeartbeatDelay != null)
                    {
                        oldHeartbeatDelay.Dispose();
                    }
                    await newHeartbeatDelay.Task.ConfigureAwait(false);
                }
                catch
                {
                    // ignore these exceptions
                }
            }
        }

        private async Task<bool> HeartbeatAsync(CancellationToken cancellationToken)
        {
            const int maxRetryCount = 2;
            HeartbeatInfo heartbeatInfo = null;
            Exception heartbeatException = null;
            for (var attempt = 1; attempt <= maxRetryCount; attempt++)
            {
                var connection = _connection;
                try
                {
                    if (connection == null)
                    {
                        connection = _connectionFactory.CreateConnection(_serverId, _endPoint);
                        // if we are cancelling, it's because the server has
                        // been shut down and we really don't need to wait.
                        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                    }

                    heartbeatInfo = await GetHeartbeatInfoAsync(connection, cancellationToken).ConfigureAwait(false);
                    heartbeatException = null;

                    _connection = connection;
                    break;
                }
                catch (Exception ex)
                {
                    heartbeatException = ex;
                    _connection = null;
                    if (connection != null)
                    {
                        connection.Dispose();
                    }
                }
            }

            ServerDescription newDescription;
            if (heartbeatInfo != null)
            {
                var averageRoundTripTime = _averageRoundTripTimeCalculator.AddSample(heartbeatInfo.RoundTripTime);
                var averageRoundTripTimeRounded = TimeSpan.FromMilliseconds(Math.Round(averageRoundTripTime.TotalMilliseconds));
                var isMasterResult = heartbeatInfo.IsMasterResult;
                var buildInfoResult = heartbeatInfo.BuildInfoResult;

                newDescription = _baseDescription.With(
                    averageRoundTripTime: averageRoundTripTimeRounded,
                    canonicalEndPoint: isMasterResult.Me,
                    electionId: isMasterResult.ElectionId,
                    lastWriteTimestamp: isMasterResult.LastWriteTimestamp,
                    maxBatchCount: isMasterResult.MaxBatchCount,
                    maxDocumentSize: isMasterResult.MaxDocumentSize,
                    maxMessageSize: isMasterResult.MaxMessageSize,
                    replicaSetConfig: isMasterResult.GetReplicaSetConfig(),
                    state: ServerState.Connected,
                    tags: isMasterResult.Tags,
                    type: isMasterResult.ServerType,
                    version: buildInfoResult.ServerVersion,
                    wireVersionRange: new Range<int>(isMasterResult.MinWireVersion, isMasterResult.MaxWireVersion));
            }
            else
            {
                newDescription = _baseDescription;
            }

            if (heartbeatException != null)
            {
                newDescription = newDescription.With(heartbeatException: heartbeatException);
            }

            OnDescriptionChanged(newDescription);

            return true;
        }

        private async Task<HeartbeatInfo> GetHeartbeatInfoAsync(IConnection connection, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (_heartbeatStartedEventHandler != null)
            {
                _heartbeatStartedEventHandler(new ServerHeartbeatStartedEvent(connection.ConnectionId));
            }

            try
            {
                var isMasterCommand = new CommandWireProtocol<BsonDocument>(
                    DatabaseNamespace.Admin,
                    new BsonDocument("isMaster", 1),
                    true,
                    BsonDocumentSerializer.Instance,
                    null);

                var stopwatch = Stopwatch.StartNew();
                var isMasterResultDocument = await isMasterCommand.ExecuteAsync(connection, cancellationToken).ConfigureAwait(false);
                stopwatch.Stop();
                var isMasterResult = new IsMasterResult(isMasterResultDocument);

                var buildInfoCommand = new CommandWireProtocol<BsonDocument>(
                    DatabaseNamespace.Admin,
                    new BsonDocument("buildInfo", 1),
                    true,
                    BsonDocumentSerializer.Instance,
                    null);

                var buildInfoResultRocument = await buildInfoCommand.ExecuteAsync(connection, cancellationToken).ConfigureAwait(false);
                var buildInfoResult = new BuildInfoResult(buildInfoResultRocument);

                if (_heartbeatSucceededEventHandler != null)
                {
                    _heartbeatSucceededEventHandler(new ServerHeartbeatSucceededEvent(connection.ConnectionId, stopwatch.Elapsed));
                }

                return new HeartbeatInfo
                {
                    RoundTripTime = stopwatch.Elapsed,
                    IsMasterResult = isMasterResult,
                    BuildInfoResult = buildInfoResult
                };
            }
            catch (Exception ex)
            {
                if (_heartbeatFailedEventHandler != null)
                {
                    _heartbeatFailedEventHandler(new ServerHeartbeatFailedEvent(connection.ConnectionId, ex));
                }
                throw;
            }
        }

        private void OnDescriptionChanged(ServerDescription newDescription)
        {
            var oldDescription = Interlocked.CompareExchange(ref _currentDescription, null, null);
            if (oldDescription.Equals(newDescription))
            {
                return;
            }
            Interlocked.Exchange(ref _currentDescription, newDescription);

            var handler = DescriptionChanged;
            if (handler != null)
            {
                var args = new ServerDescriptionChangedEventArgs(oldDescription, newDescription);
                try { handler(this, args); }
                catch { } // ignore exceptions
            }
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
                throw new InvalidOperationException("Server monitor must be initialized.");
            }
        }

        // nested types
        private static class State
        {
            public const int Initial = 0;
            public const int Open = 1;
            public const int Disposed = 2;
        }

        private class HeartbeatInfo
        {
            public TimeSpan RoundTripTime;
            public IsMasterResult IsMasterResult;
            public BuildInfoResult BuildInfoResult;
        }
    }
}

/* Copyright 2013-2016 MongoDB Inc.
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
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver.Core.Async;
using MongoDB.Driver.Core.Configuration;
using MongoDB.Driver.Core.Events;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.Servers;

namespace MongoDB.Driver.Core.Clusters
{
    /// <summary>
    /// Represents a multi server cluster.
    /// </summary>
    internal sealed class MultiServerCluster : Cluster
    {
        // fields
        private readonly CancellationTokenSource _monitorServersCancellationTokenSource;
        private volatile ElectionInfo _maxElectionInfo;
        private volatile string _replicaSetName;
        private readonly AsyncQueue<ServerDescriptionChangedEventArgs> _serverDescriptionChangedQueue;
        private readonly List<IClusterableServer> _servers;
        private readonly object _serversLock = new object();
        private readonly InterlockedInt32 _state;

        private readonly Action<ClusterClosingEvent> _closingEventHandler;
        private readonly Action<ClusterClosedEvent> _closedEventHandler;
        private readonly Action<ClusterOpeningEvent> _openingEventHandler;
        private readonly Action<ClusterOpenedEvent> _openedEventHandler;
        private readonly Action<ClusterAddingServerEvent> _addingServerEventHandler;
        private readonly Action<ClusterAddedServerEvent> _addedServerEventHandler;
        private readonly Action<ClusterRemovingServerEvent> _removingServerEventHandler;
        private readonly Action<ClusterRemovedServerEvent> _removedServerEventHandler;

        // constructors
        public MultiServerCluster(ClusterSettings settings, IClusterableServerFactory serverFactory, IEventSubscriber eventSubscriber)
            : base(settings, serverFactory, eventSubscriber)
        {
            Ensure.IsGreaterThanZero(settings.EndPoints.Count, "settings.EndPoints.Count");
            if (settings.ConnectionMode == ClusterConnectionMode.Standalone)
            {
                throw new ArgumentException("ClusterConnectionMode.StandAlone is not supported for a MultiServerCluster.");
            }
            if (settings.ConnectionMode == ClusterConnectionMode.Direct)
            {
                throw new ArgumentException("ClusterConnectionMode.Direct is not supported for a MultiServerCluster.");
            }

            _monitorServersCancellationTokenSource = new CancellationTokenSource();
            _serverDescriptionChangedQueue = new AsyncQueue<ServerDescriptionChangedEventArgs>();
            _servers = new List<IClusterableServer>();
            _state = new InterlockedInt32(State.Initial);
            _replicaSetName = settings.ReplicaSetName;

            eventSubscriber.TryGetEventHandler(out _closingEventHandler);
            eventSubscriber.TryGetEventHandler(out _closedEventHandler);
            eventSubscriber.TryGetEventHandler(out _openingEventHandler);
            eventSubscriber.TryGetEventHandler(out _openedEventHandler);
            eventSubscriber.TryGetEventHandler(out _addingServerEventHandler);
            eventSubscriber.TryGetEventHandler(out _addedServerEventHandler);
            eventSubscriber.TryGetEventHandler(out _removingServerEventHandler);
            eventSubscriber.TryGetEventHandler(out _removedServerEventHandler);
        }

        // methods
        protected override void Dispose(bool disposing)
        {
            Stopwatch stopwatch = null;
            if (_state.TryChange(State.Disposed))
            {
                if (disposing)
                {
                    if (_closingEventHandler != null)
                    {
                        _closingEventHandler(new ClusterClosingEvent(ClusterId));
                    }

                    stopwatch = Stopwatch.StartNew();
                    _monitorServersCancellationTokenSource.Cancel();
                    _monitorServersCancellationTokenSource.Dispose();
                    var clusterDescription = Description;
                    lock (_serversLock)
                    {
                        foreach (var server in _servers.ToList())
                        {
                            RemoveServer(clusterDescription, server.EndPoint, "The cluster is closing.");
                        }
                    }
                    stopwatch.Stop();
                }
            }

            base.Dispose(disposing);

            if (stopwatch != null && _closedEventHandler != null)
            {
                _closedEventHandler(new ClusterClosedEvent(ClusterId, stopwatch.Elapsed));
            }
        }

        public override void Initialize()
        {
            base.Initialize();
            if (_state.TryChange(State.Initial, State.Open))
            {
                if (_openingEventHandler != null)
                {
                    _openingEventHandler(new ClusterOpeningEvent(ClusterId, Settings));
                }

                var stopwatch = Stopwatch.StartNew();
                MonitorServersAsync().ConfigureAwait(false);
                // We lock here even though AddServer locks. Monitors
                // are re-entrant such that this won't cause problems,
                // but could prevent issues of conflicting reports
                // from servers that are quick to respond.
                var clusterDescription = Description.WithType(Settings.ConnectionMode.ToClusterType());
                var newServers = new List<IClusterableServer>();
                lock (_serversLock)
                {
                    foreach (var endPoint in Settings.EndPoints)
                    {
                        clusterDescription = EnsureServer(clusterDescription, endPoint, newServers);
                    }
                }

                stopwatch.Stop();

                UpdateClusterDescription(clusterDescription);

                foreach (var server in newServers)
                {
                    server.Initialize();
                }

                if (_openedEventHandler != null)
                {
                    _openedEventHandler(new ClusterOpenedEvent(ClusterId, Settings, stopwatch.Elapsed));
                }
            }
        }

        private bool IsServerValidForCluster(ClusterType clusterType, ClusterConnectionMode connectionMode, ServerType serverType)
        {
            switch (clusterType)
            {
                case ClusterType.ReplicaSet:
                    return serverType.IsReplicaSetMember();

                case ClusterType.Sharded:
                    return serverType == ServerType.ShardRouter;

                case ClusterType.Unknown:
                    switch (connectionMode)
                    {
                        case ClusterConnectionMode.Automatic:
                            return serverType.IsReplicaSetMember() || serverType == ServerType.ShardRouter;

                        default:
                            throw new MongoInternalException("Unexpected connection mode.");
                    }

                default:
                    throw new MongoInternalException("Unexpected cluster type.");
            }
        }

        protected override void RequestHeartbeat()
        {
            List<IClusterableServer> servers;
            lock (_serversLock)
            {
                servers = _servers.ToList();
            }

            foreach (var server in servers)
            {
                if (server.IsInitialized)
                {
                    try
                    {
                        server.RequestHeartbeat();
                    }
                    catch (ObjectDisposedException)
                    {
                        // There is a possible race condition here
                        // due to the fact that we are working 
                        // with the server outside of the lock,
                        // meaning another thread could remove 
                        // the server and dispose of it before
                        // we invoke the method.
                    }
                }
            }
        }

        private async Task MonitorServersAsync()
        {
            var monitorServersCancellationToken = _monitorServersCancellationTokenSource.Token;
            while (!monitorServersCancellationToken.IsCancellationRequested)
            {
                try
                {
                    var eventArgs = await _serverDescriptionChangedQueue.DequeueAsync(monitorServersCancellationToken).ConfigureAwait(false); // TODO: add timeout and cancellationToken to DequeueAsync
                    ProcessServerDescriptionChanged(eventArgs);
                }
                catch
                {
                    // TODO: log this somewhere...
                }
            }
        }

        private void ServerDescriptionChangedHandler(object sender, ServerDescriptionChangedEventArgs args)
        {
            _serverDescriptionChangedQueue.Enqueue(args);
        }

        private void ProcessServerDescriptionChanged(ServerDescriptionChangedEventArgs args)
        {
            var newServerDescription = args.NewServerDescription;
            var newClusterDescription = Description;

            if (!_servers.Any(x => EndPointHelper.Equals(x.EndPoint, newServerDescription.EndPoint)))
            {
                return;
            }

            var newServers = new List<IClusterableServer>();
            if (newServerDescription.State == ServerState.Disconnected)
            {
                newClusterDescription = newClusterDescription.WithServerDescription(newServerDescription);
            }
            else
            {
                if (IsServerValidForCluster(newClusterDescription.Type, Settings.ConnectionMode, newServerDescription.Type))
                {
                    if (newClusterDescription.Type == ClusterType.Unknown)
                    {
                        newClusterDescription = newClusterDescription.WithType(newServerDescription.Type.ToClusterType());
                    }

                    switch (newClusterDescription.Type)
                    {
                        case ClusterType.ReplicaSet:
                            newClusterDescription = ProcessReplicaSetChange(newClusterDescription, args, newServers);
                            break;

                        case ClusterType.Sharded:
                            newClusterDescription = ProcessShardedChange(newClusterDescription, args);
                            break;

                        default:
                            throw new MongoInternalException("Unexpected cluster type.");
                    }
                }
                else
                {
                    newClusterDescription = newClusterDescription.WithoutServerDescription(newServerDescription.EndPoint);
                }
            }

            UpdateClusterDescription(newClusterDescription);

            foreach (var server in newServers)
            {
                server.Initialize();
            }
        }

        private ClusterDescription ProcessReplicaSetChange(ClusterDescription clusterDescription, ServerDescriptionChangedEventArgs args, List<IClusterableServer> newServers)
        {
            if (!args.NewServerDescription.Type.IsReplicaSetMember())
            {
                return RemoveServer(clusterDescription, args.NewServerDescription.EndPoint, string.Format("Server is a {0}, not a replica set member.", args.NewServerDescription.Type));
            }

            if (args.NewServerDescription.Type == ServerType.ReplicaSetGhost)
            {
                return clusterDescription.WithServerDescription(args.NewServerDescription);
            }

            if (_replicaSetName == null)
            {
                _replicaSetName = args.NewServerDescription.ReplicaSetConfig.Name;
            }

            if (_replicaSetName != args.NewServerDescription.ReplicaSetConfig.Name)
            {
                return RemoveServer(clusterDescription, args.NewServerDescription.EndPoint, string.Format("Server was a member of the '{0}' replica set, but should be '{1}'.", args.NewServerDescription.ReplicaSetConfig.Name, _replicaSetName));
            }

            clusterDescription = clusterDescription.WithServerDescription(args.NewServerDescription);
            clusterDescription = EnsureServers(clusterDescription, args.NewServerDescription, newServers);

            if (args.NewServerDescription.CanonicalEndPoint != null &&
                !EndPointHelper.Equals(args.NewServerDescription.CanonicalEndPoint, args.NewServerDescription.EndPoint))
            {
                return RemoveServer(clusterDescription, args.NewServerDescription.EndPoint, "CanonicalEndPoint is different than seed list EndPoint.");
            }

            if (args.NewServerDescription.Type == ServerType.ReplicaSetPrimary)
            {
                if (args.NewServerDescription.ReplicaSetConfig.Version != null)
                {
                    bool isCurrentPrimaryStale = true;
                    if (_maxElectionInfo != null)
                    {
                        isCurrentPrimaryStale = _maxElectionInfo.IsStale(args.NewServerDescription.ReplicaSetConfig.Version.Value, args.NewServerDescription.ElectionId);
                        var isReportedPrimaryStale = !isCurrentPrimaryStale;

                        if (isReportedPrimaryStale && args.NewServerDescription.ElectionId != null)
                        {
                            // we only invalidate the "newly" reported stale primary if electionId was used.
                            lock (_serversLock)
                            {
                                var server = _servers.SingleOrDefault(x => EndPointHelper.Equals(args.NewServerDescription.EndPoint, x.EndPoint));
                                server.Invalidate();
                                return clusterDescription.WithServerDescription(
                                    new ServerDescription(server.ServerId, server.EndPoint));
                            }
                        }
                    }

                    if (isCurrentPrimaryStale)
                    {
                        _maxElectionInfo = new ElectionInfo(
                            args.NewServerDescription.ReplicaSetConfig.Version.Value,
                            args.NewServerDescription.ElectionId);
                    }
                }

                var currentPrimaryEndPoints = clusterDescription.Servers
                    .Where(x => x.Type == ServerType.ReplicaSetPrimary)
                    .Where(x => !EndPointHelper.Equals(x.EndPoint, args.NewServerDescription.EndPoint))
                    .Select(x => x.EndPoint)
                    .ToList();

                if (currentPrimaryEndPoints.Count > 0)
                {
                    lock (_serversLock)
                    {
                        var currentPrimaries = _servers.Where(x => EndPointHelper.Contains(currentPrimaryEndPoints, x.EndPoint));
                        foreach (var currentPrimary in currentPrimaries)
                        {
                            // kick off the server to invalidate itself
                            currentPrimary.Invalidate();
                            // set it to disconnected in the cluster
                            clusterDescription = clusterDescription.WithServerDescription(
                                new ServerDescription(currentPrimary.ServerId, currentPrimary.EndPoint));
                        }
                    }
                }
            }

            return clusterDescription;
        }

        private ClusterDescription ProcessShardedChange(ClusterDescription clusterDescription, ServerDescriptionChangedEventArgs args)
        {
            if (args.NewServerDescription.Type != ServerType.ShardRouter)
            {
                return RemoveServer(clusterDescription, args.NewServerDescription.EndPoint, "Server is not a shard router.");
            }

            return clusterDescription.WithServerDescription(args.NewServerDescription);
        }

        private ClusterDescription EnsureServer(ClusterDescription clusterDescription, EndPoint endPoint, List<IClusterableServer> newServers)
        {
            if (_state.Value == State.Disposed)
            {
                return clusterDescription;
            }

            IClusterableServer server;
            Stopwatch stopwatch = new Stopwatch();
            lock (_serversLock)
            {
                if (_servers.Any(n => EndPointHelper.Equals(n.EndPoint, endPoint)))
                {
                    return clusterDescription;
                }

                if (_addingServerEventHandler != null)
                {
                    _addingServerEventHandler(new ClusterAddingServerEvent(ClusterId, endPoint));
                }

                stopwatch.Start();
                server = CreateServer(endPoint);
                server.DescriptionChanged += ServerDescriptionChangedHandler;
                _servers.Add(server);
                newServers.Add(server);
            }

            clusterDescription = clusterDescription.WithServerDescription(server.Description);
            stopwatch.Stop();

            if (_addedServerEventHandler != null)
            {
                _addedServerEventHandler(new ClusterAddedServerEvent(server.ServerId, stopwatch.Elapsed));
            }

            return clusterDescription;
        }

        private ClusterDescription EnsureServers(ClusterDescription clusterDescription, ServerDescription serverDescription, List<IClusterableServer> newServers)
        {
            if (serverDescription.Type == ServerType.ReplicaSetPrimary ||
                !clusterDescription.Servers.Any(x => x.Type == ServerType.ReplicaSetPrimary))
            {
                foreach (var endPoint in serverDescription.ReplicaSetConfig.Members)
                {
                    clusterDescription = EnsureServer(clusterDescription, endPoint, newServers);
                }
            }

            if (serverDescription.Type == ServerType.ReplicaSetPrimary)
            {
                var requiredEndPoints = serverDescription.ReplicaSetConfig.Members;
                var extraEndPoints = clusterDescription.Servers.Where(x => !EndPointHelper.Contains(requiredEndPoints, x.EndPoint)).Select(x => x.EndPoint);
                foreach (var endPoint in extraEndPoints)
                {
                    clusterDescription = RemoveServer(clusterDescription, endPoint, "Server is not in the host list of the primary.");
                }
            }

            return clusterDescription;
        }

        private ClusterDescription RemoveServer(ClusterDescription clusterDescription, EndPoint endPoint, string reason)
        {
            IClusterableServer server;
            var stopwatch = new Stopwatch();
            lock (_serversLock)
            {
                server = _servers.SingleOrDefault(x => EndPointHelper.Equals(x.EndPoint, endPoint));
                if (server == null)
                {
                    return clusterDescription;
                }

                if (_removingServerEventHandler != null)
                {
                    _removingServerEventHandler(new ClusterRemovingServerEvent(server.ServerId, reason));
                }

                stopwatch.Start();
                _servers.Remove(server);
            }

            server.DescriptionChanged -= ServerDescriptionChangedHandler;
            server.Dispose();
            stopwatch.Stop();

            if (_removedServerEventHandler != null)
            {
                _removedServerEventHandler(new ClusterRemovedServerEvent(server.ServerId, reason, stopwatch.Elapsed));
            }

            return clusterDescription.WithoutServerDescription(endPoint);
        }

        protected override bool TryGetServer(EndPoint endPoint, out IClusterableServer server)
        {
            lock (_serversLock)
            {
                server = _servers.FirstOrDefault(s => EndPointHelper.Equals(s.EndPoint, endPoint));
                return server != null;
            }
        }

        private void ThrowIfDisposed()
        {
            if (_state.Value == State.Disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        // nested classes
        private static class State
        {
            public const int Initial = 0;
            public const int Open = 1;
            public const int Disposed = 2;
        }

        private class ElectionInfo
        {
            private readonly int _setVersion;
            private readonly ElectionId _electionId;

            public ElectionInfo(int setVersion, ElectionId electionId)
            {
                _setVersion = setVersion;
                _electionId = electionId;
            }

            public int SetVersion => _setVersion;

            public ElectionId ElectionId => _electionId;

            public bool IsStale(int setVersion, ElectionId electionId)
            {
                if (_setVersion < setVersion)
                {
                    return true;
                }
                if (_setVersion > setVersion)
                {
                    return false;
                }

                if (_electionId == null)
                {
                    return true;
                }

                return _electionId.CompareTo(electionId) <= 0;
            }
        }
    }
}

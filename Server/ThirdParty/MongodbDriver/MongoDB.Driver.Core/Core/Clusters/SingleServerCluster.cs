/* Copyright 2013-2015 MongoDB Inc.
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
using MongoDB.Driver.Core.Configuration;
using MongoDB.Driver.Core.Events;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.Servers;

namespace MongoDB.Driver.Core.Clusters
{
    /// <summary>
    /// Represents a standalone cluster.
    /// </summary>
    internal sealed class SingleServerCluster : Cluster
    {
        // fields
        private IClusterableServer _server;
        private readonly InterlockedInt32 _state;

        private readonly Action<ClusterClosingEvent> _closingEventHandler;
        private readonly Action<ClusterClosedEvent> _closedEventHandler;
        private readonly Action<ClusterOpeningEvent> _openingEventHandler;
        private readonly Action<ClusterOpenedEvent> _openedEventHandler;
        private readonly Action<ClusterAddingServerEvent> _addingServerEventHandler;
        private readonly Action<ClusterAddedServerEvent> _addedServerEventHandler;
        private readonly Action<ClusterRemovingServerEvent> _removingServerEventHandler;
        private readonly Action<ClusterRemovedServerEvent> _removedServerEventHandler;

        // constructor
        internal SingleServerCluster(ClusterSettings settings, IClusterableServerFactory serverFactory, IEventSubscriber eventSubscriber)
            : base(settings, serverFactory, eventSubscriber)
        {
            Ensure.IsEqualTo(settings.EndPoints.Count, 1, "settings.EndPoints.Count");

            _state = new InterlockedInt32(State.Initial);

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

                    if (_server != null)
                    {
                        if (_removingServerEventHandler != null)
                        {
                            _removingServerEventHandler(new ClusterRemovingServerEvent(_server.ServerId, "Cluster is closing."));
                        }

                        _server.DescriptionChanged -= ServerDescriptionChanged;
                        _server.Dispose();

                        if (_removedServerEventHandler != null)
                        {
                            _removedServerEventHandler(new ClusterRemovedServerEvent(_server.ServerId, "Cluster is closing.", stopwatch.Elapsed));
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
                _server = CreateServer(Settings.EndPoints[0]);
                var newClusterDescription = Description
                    .WithType(Settings.ConnectionMode.ToClusterType())
                    .WithServerDescription(_server.Description);
                if (_addingServerEventHandler != null)
                {
                    _addingServerEventHandler(new ClusterAddingServerEvent(ClusterId, _server.EndPoint));
                }
                _server.DescriptionChanged += ServerDescriptionChanged;
                stopwatch.Stop();

                if (_addedServerEventHandler != null)
                {
                    _addedServerEventHandler(new ClusterAddedServerEvent(_server.ServerId, stopwatch.Elapsed));
                }

                UpdateClusterDescription(newClusterDescription);

                _server.Initialize();

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

                case ClusterType.Standalone:
                    return serverType == ServerType.Standalone;

                case ClusterType.Unknown:
                    switch (connectionMode)
                    {
                        case ClusterConnectionMode.Automatic:
                        case ClusterConnectionMode.Direct:
                            return true;

                        default:
                            throw new MongoInternalException("Unexpected connection mode.");
                    }

                default:
                    throw new MongoInternalException("Unexpected cluster type.");
            }
        }

        protected override void RequestHeartbeat()
        {
            _server.RequestHeartbeat();
        }

        private void ServerDescriptionChanged(object sender, ServerDescriptionChangedEventArgs args)
        {
            var newServerDescription = args.NewServerDescription;
            var newClusterDescription = Description;

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

                    newClusterDescription = newClusterDescription.WithServerDescription(newServerDescription);
                }
                else
                {
                    newClusterDescription = newClusterDescription.WithoutServerDescription(newServerDescription.EndPoint);
                }
            }

            UpdateClusterDescription(newClusterDescription);
        }

        protected override bool TryGetServer(EndPoint endPoint, out IClusterableServer server)
        {
            if (EndPointHelper.Equals(_server.EndPoint, endPoint))
            {
                server = _server;
                return true;
            }
            else
            {
                server = null;
                return false;
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
    }
}

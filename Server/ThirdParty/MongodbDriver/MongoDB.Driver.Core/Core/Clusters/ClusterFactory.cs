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

using MongoDB.Driver.Core.Configuration;
using MongoDB.Driver.Core.Events;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.Servers;

namespace MongoDB.Driver.Core.Clusters
{
    internal class ClusterFactory : IClusterFactory
    {
        // fields
        private readonly IEventSubscriber _eventSubscriber;
        private readonly IClusterableServerFactory _serverFactory;
        private readonly ClusterSettings _settings;

        // constructors
        public ClusterFactory(ClusterSettings settings, IClusterableServerFactory serverFactory, IEventSubscriber eventSubscriber)
        {
            _settings = Ensure.IsNotNull(settings, nameof(settings));
            _serverFactory = Ensure.IsNotNull(serverFactory, nameof(serverFactory));
            _eventSubscriber = Ensure.IsNotNull(eventSubscriber, nameof(eventSubscriber));
        }

        // methods
        public ICluster CreateCluster()
        {
            var connectionMode = _settings.ConnectionMode;

            if (connectionMode == ClusterConnectionMode.Automatic)
            {
                if (_settings.ReplicaSetName != null)
                {
                    connectionMode = ClusterConnectionMode.ReplicaSet;
                }
            }

            var settings = _settings.With(connectionMode: connectionMode);

            switch (connectionMode)
            {
                case ClusterConnectionMode.Automatic:
                    return settings.EndPoints.Count == 1 ? (ICluster)CreateSingleServerCluster(settings) : CreateMultiServerCluster(settings);
                case ClusterConnectionMode.Direct:
                case ClusterConnectionMode.Standalone:
                    return CreateSingleServerCluster(settings);
                case ClusterConnectionMode.ReplicaSet:
                case ClusterConnectionMode.Sharded:
                    return CreateMultiServerCluster(settings);
                default:
                    throw new MongoInternalException(string.Format("Invalid connection mode: {0}.", connectionMode));
            }
        }

        private MultiServerCluster CreateMultiServerCluster(ClusterSettings settings)
        {
            return new MultiServerCluster(settings, _serverFactory, _eventSubscriber);
        }

        private SingleServerCluster CreateSingleServerCluster(ClusterSettings settings)
        {
            return new SingleServerCluster(settings, _serverFactory, _eventSubscriber);
        }
    }
}

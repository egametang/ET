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

using System.Net;
using MongoDB.Driver.Core.Clusters;
using MongoDB.Driver.Core.Configuration;
using MongoDB.Driver.Core.ConnectionPools;
using MongoDB.Driver.Core.Events;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Core.Servers
{
    internal class ServerFactory : IClusterableServerFactory
    {
        // fields
        private readonly ClusterConnectionMode _clusterConnectionMode;
        private readonly IConnectionPoolFactory _connectionPoolFactory;
        private readonly IServerMonitorFactory _serverMonitorFactory;
        private readonly IEventSubscriber _eventSubscriber;
        private readonly ServerSettings _settings;

        // constructors
        public ServerFactory(ClusterConnectionMode clusterConnectionMode, ServerSettings settings, IConnectionPoolFactory connectionPoolFactory, IServerMonitorFactory serverMonitoryFactory, IEventSubscriber eventSubscriber)
        {
            _clusterConnectionMode = clusterConnectionMode;
            _settings = Ensure.IsNotNull(settings, nameof(settings));
            _connectionPoolFactory = Ensure.IsNotNull(connectionPoolFactory, nameof(connectionPoolFactory));
            _serverMonitorFactory = Ensure.IsNotNull(serverMonitoryFactory, nameof(serverMonitoryFactory));
            _eventSubscriber = Ensure.IsNotNull(eventSubscriber, nameof(eventSubscriber));
        }

        // methods
        /// <inheritdoc/>
        public IClusterableServer CreateServer(ClusterId clusterId, EndPoint endPoint)
        {
            return new Server(clusterId, _clusterConnectionMode, _settings, endPoint, _connectionPoolFactory, _serverMonitorFactory, _eventSubscriber);
        }
    }
}

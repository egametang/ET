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
using MongoDB.Driver.Core.Clusters;
using MongoDB.Driver.Core.Clusters.ServerSelectors;
using MongoDB.Driver.Core.Servers;

namespace MongoDB.Driver.Core.Events
{
    /// <preliminary/>
    /// <summary>
    /// Occurs after a server is selected.
    /// </summary>
    public struct ClusterSelectedServerEvent
    {
        private readonly ClusterDescription _clusterDescription;
        private readonly long? _operationId;
        private readonly IServerSelector _serverSelector;
        private readonly ServerDescription _selectedServer;
        private readonly TimeSpan _duration;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClusterSelectedServerEvent" /> struct.
        /// </summary>
        /// <param name="clusterDescription">The cluster description.</param>
        /// <param name="serverSelector">The server selector.</param>
        /// <param name="selectedServer">The selected server.</param>
        /// <param name="duration">The duration of time it took to select the server.</param>
        /// <param name="operationId">The operation identifier.</param>
        public ClusterSelectedServerEvent(ClusterDescription clusterDescription, IServerSelector serverSelector, ServerDescription selectedServer, TimeSpan duration, long? operationId)
        {
            _clusterDescription = clusterDescription;
            _serverSelector = serverSelector;
            _selectedServer = selectedServer;
            _duration = duration;
            _operationId = operationId;
        }

        /// <summary>
        /// Gets the cluster identifier.
        /// </summary>
        public ClusterId ClusterId
        {
            get { return _clusterDescription.ClusterId; }
        }

        /// <summary>
        /// Gets the cluster description.
        /// </summary>
        public ClusterDescription ClusterDescription
        {
            get { return _clusterDescription; }
        }

        /// <summary>
        /// Gets the duration of time it took to select the server.
        /// </summary>
        public TimeSpan Duration
        {
            get { return _duration; }
        }

        /// <summary>
        /// Gets the operation identifier.
        /// </summary>
        public long? OperationId
        {
            get { return _operationId; }
        }

        /// <summary>
        /// Gets the server selector.
        /// </summary>
        public IServerSelector ServerSelector
        {
            get { return _serverSelector; }
        }

        /// <summary>
        /// Gets the selected server.
        /// </summary>
        public ServerDescription SelectedServer
        {
            get { return _selectedServer; }
        }
    }
}

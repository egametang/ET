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

namespace MongoDB.Driver.Core.Events
{
    /// <preliminary/>
    /// <summary>
    /// Occurs when selecting a server fails.
    /// </summary>
    public struct ClusterSelectingServerFailedEvent
    {
        private readonly ClusterDescription _clusterDescription;
        private readonly long? _operationId;
        private readonly IServerSelector _serverSelector;
        private readonly Exception _exception;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClusterSelectingServerFailedEvent" /> struct.
        /// </summary>
        /// <param name="clusterDescription">The cluster description.</param>
        /// <param name="serverSelector">The server selector.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="operationId">The operation identifier.</param>
        public ClusterSelectingServerFailedEvent(ClusterDescription clusterDescription, IServerSelector serverSelector, Exception exception, long? operationId)
        {
            _clusterDescription = clusterDescription;
            _serverSelector = serverSelector;
            _exception = exception;
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
        /// Gets the exception.
        /// </summary>
        public Exception Exception
        {
            get { return _exception; }
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
    }
}

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

using MongoDB.Driver.Core.Clusters;
using MongoDB.Driver.Core.Servers;

namespace MongoDB.Driver.Core.Events
{
    /// <preliminary/>
    /// <summary>
    /// Occurs before a connection is checking out of the pool.
    /// </summary>
    public struct ConnectionPoolCheckingOutConnectionEvent
    {
        private readonly long? _operationId;
        private readonly ServerId _serverId;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionPoolCheckingOutConnectionEvent" /> struct.
        /// </summary>
        /// <param name="serverId">The server identifier.</param>
        /// <param name="operationId">The operation identifier.</param>
        public ConnectionPoolCheckingOutConnectionEvent(ServerId serverId, long? operationId)
        {
            _serverId = serverId;
            _operationId = operationId;
        }

        /// <summary>
        /// Gets the cluster identifier.
        /// </summary>
        public ClusterId ClusterId
        {
            get { return _serverId.ClusterId; }
        }

        /// <summary>
        /// Gets the operation identifier.
        /// </summary>
        public long? OperationId
        {
            get { return _operationId; }
        }

        /// <summary>
        /// Gets the server identifier.
        /// </summary>
        public ServerId ServerId
        {
            get { return _serverId; }
        }
    }
}

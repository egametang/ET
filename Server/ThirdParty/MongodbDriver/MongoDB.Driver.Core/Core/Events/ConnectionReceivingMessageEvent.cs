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
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Servers;

namespace MongoDB.Driver.Core.Events
{
    /// <preliminary/>
    /// <summary>
    /// Occurs before a message is received.
    /// </summary>
    public struct ConnectionReceivingMessageEvent
    {
        private readonly ConnectionId _connectionId;
        private readonly long? _operationId;
        private readonly int _responseTo;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionReceivingMessageEvent" /> struct.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="responseTo">The id of the message we are receiving a response to.</param>
        /// <param name="operationId">The operation identifier.</param>
        public ConnectionReceivingMessageEvent(ConnectionId connectionId, int responseTo, long? operationId)
        {
            _connectionId = connectionId;
            _responseTo = responseTo;
            _operationId = operationId;
        }

        /// <summary>
        /// Gets the cluster identifier.
        /// </summary>
        public ClusterId ClusterId
        {
            get { return _connectionId.ServerId.ClusterId; }
        }

        /// <summary>
        /// Gets the connection identifier.
        /// </summary>
        public ConnectionId ConnectionId
        {
            get { return _connectionId; }
        }

        /// <summary>
        /// Gets the operation identifier.
        /// </summary>
        public long? OperationId
        {
            get { return _operationId; }
        }

        /// <summary>
        /// Gets the id of the message we are receiving a response to.
        /// </summary>
        public int ResponseTo
        {
            get { return _responseTo; }
        }

        /// <summary>
        /// Gets the server identifier.
        /// </summary>
        public ServerId ServerId
        {
            get { return _connectionId.ServerId; }
        }
    }
}

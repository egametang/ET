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
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Servers;

namespace MongoDB.Driver.Core.Events
{
    /// <summary>
    /// Occurs after a message is received.
    /// </summary>
    /// <preliminary />
    public struct ConnectionReceivedMessageEvent
    {
        private readonly ConnectionId _connectionId;
        private readonly TimeSpan _deserializationDuration;
        private readonly TimeSpan _networkDuration;
        private readonly int _length;
        private readonly long? _operationId;
        private readonly int _responseTo;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionReceivedMessageEvent" /> struct.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="responseTo">The id of the message we received a response to.</param>
        /// <param name="length">The length of the received message.</param>
        /// <param name="networkDuration">The duration of network time it took to receive the message.</param>
        /// <param name="deserializationDuration">The duration of deserialization time it took to receive the message.</param>
        /// <param name="operationId">The operation identifier.</param>
        public ConnectionReceivedMessageEvent(ConnectionId connectionId, int responseTo, int length, TimeSpan networkDuration, TimeSpan deserializationDuration, long? operationId)
        {
            _connectionId = connectionId;
            _responseTo = responseTo;
            _length = length;
            _networkDuration = networkDuration;
            _deserializationDuration = deserializationDuration;
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
        /// Gets the duration of time it took to receive the message.
        /// </summary>
        public TimeSpan Duration
        {
            get { return _networkDuration + _deserializationDuration; }
        }

        /// <summary>
        /// Gets the duration of deserialization time it took to receive the message.
        /// </summary>
        public TimeSpan DeserializationDuration
        {
            get { return _deserializationDuration; }
        }

        /// <summary>
        /// Gets the duration of network time it took to receive the message.
        /// </summary>
        public TimeSpan NetworkDuration
        {
            get { return _networkDuration; }
        }

        /// <summary>
        /// Gets the length of the received message.
        /// </summary>
        public int Length
        {
            get { return _length; }
        }

        /// <summary>
        /// Gets the operation identifier.
        /// </summary>
        public long? OperationId
        {
            get { return _operationId; }
        }

        /// <summary>
        /// Gets the id of the message we received a response to.
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

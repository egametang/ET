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
    /// <preliminary/>
    /// <summary>
    /// Occurs when a message was unable to be received.
    /// </summary>
    public struct ConnectionReceivingMessageFailedEvent
    {
        private readonly ConnectionId _connectionId;
        private readonly Exception _exception;
        private readonly long? _operationId;
        private readonly int _responseTo;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionReceivingMessageFailedEvent" /> struct.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="responseTo">The id of the message we were receiving a response to.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="operationId">The operation identifier.</param>
        public ConnectionReceivingMessageFailedEvent(ConnectionId connectionId, int responseTo, Exception exception, long? operationId)
        {
            _connectionId = connectionId;
            _responseTo = responseTo;
            _exception = exception;
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
        /// Gets id of the message we were receiving a response to.
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

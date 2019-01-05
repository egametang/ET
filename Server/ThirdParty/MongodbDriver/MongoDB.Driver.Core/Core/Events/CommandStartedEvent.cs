﻿/* Copyright 2015-present MongoDB Inc.
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

using MongoDB.Bson;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Core.Events
{
    /// <summary>
    /// Occurs when a command has started.
    /// </summary>
    public struct CommandStartedEvent
    {
        private readonly BsonDocument _command;
        private readonly string _commandName;
        private readonly ConnectionId _connectionId;
        private readonly DatabaseNamespace _databaseNamespace;
        private readonly long? _operationId;
        private readonly int _requestId;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandStartedEvent" /> class.
        /// </summary>
        /// <param name="commandName">Name of the command.</param>
        /// <param name="command">The command.</param>
        /// <param name="databaseNamespace">The database namespace.</param>
        /// <param name="operationId">The operation identifier.</param>
        /// <param name="requestId">The request identifier.</param>
        /// <param name="connectionId">The connection identifier.</param>
        public CommandStartedEvent(string commandName, BsonDocument command, DatabaseNamespace databaseNamespace, long? operationId, int requestId, ConnectionId connectionId)
        {
            _commandName = Ensure.IsNotNullOrEmpty(commandName, "commandName");
            _command = Ensure.IsNotNull(command, "command");
            _databaseNamespace = Ensure.IsNotNull(databaseNamespace, "databaseNamespace");
            _connectionId = Ensure.IsNotNull(connectionId, "connectionId");
            _operationId = operationId;
            _requestId = requestId;
        }

        /// <summary>
        /// Gets the command.
        /// </summary>
        public BsonDocument Command
        {
            get { return _command; }
        }

        /// <summary>
        /// Gets the name of the command.
        /// </summary>
        public string CommandName
        {
            get { return _commandName; }
        }

        /// <summary>
        /// Gets the connection identifier.
        /// </summary>
        public ConnectionId ConnectionId
        {
            get { return _connectionId; }
        }

        /// <summary>
        /// Gets the database namespace.
        /// </summary>
        public DatabaseNamespace DatabaseNamespace
        {
            get { return _databaseNamespace; }
        }

        /// <summary>
        /// Gets the operation identifier.
        /// </summary>
        public long? OperationId
        {
            get { return _operationId; }
        }

        /// <summary>
        /// Gets the request identifier.
        /// </summary>
        public int RequestId
        {
            get { return _requestId; }
        }
    }
}
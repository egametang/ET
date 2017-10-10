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
using MongoDB.Driver.Core.Misc;
using MongoDB.Shared;

namespace MongoDB.Driver.Core.Connections
{
    /// <summary>
    /// Represents information describing a connection.
    /// </summary>
    public sealed class ConnectionDescription : IEquatable<ConnectionDescription>
    {
        // fields
        private readonly BuildInfoResult _buildInfoResult;
        private readonly ConnectionId _connectionId;
        private readonly IsMasterResult _isMasterResult;
        private readonly int _maxBatchCount;
        private readonly int _maxDocumentSize;
        private readonly int _maxMessageSize;
        private readonly SemanticVersion _serverVersion;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionDescription"/> class.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="isMasterResult">The issMaster result.</param>
        /// <param name="buildInfoResult">The buildInfo result.</param>
        public ConnectionDescription(ConnectionId connectionId, IsMasterResult isMasterResult, BuildInfoResult buildInfoResult)
        {
            _connectionId = Ensure.IsNotNull(connectionId, nameof(connectionId));
            _buildInfoResult = Ensure.IsNotNull(buildInfoResult, nameof(buildInfoResult));
            _isMasterResult = Ensure.IsNotNull(isMasterResult, nameof(isMasterResult));

            _maxBatchCount = isMasterResult.MaxBatchCount;
            _maxDocumentSize = isMasterResult.MaxDocumentSize;
            _maxMessageSize = isMasterResult.MaxMessageSize;
            _serverVersion = buildInfoResult.ServerVersion;
        }

        // properties
        /// <summary>
        /// Gets the buildInfo result.
        /// </summary>
        /// <value>
        /// The buildInfo result.
        /// </value>
        public BuildInfoResult BuildInfoResult
        {
            get { return _buildInfoResult; }
        }

        /// <summary>
        /// Gets the connection identifier.
        /// </summary>
        /// <value>
        /// The connection identifier.
        /// </value>
        public ConnectionId ConnectionId
        {
            get { return _connectionId; }
        }

        /// <summary>
        /// Gets the isMaster result.
        /// </summary>
        /// <value>
        /// The isMaster result.
        /// </value>
        public IsMasterResult IsMasterResult
        {
            get { return _isMasterResult; }
        }

        /// <summary>
        /// Gets the maximum number of documents in a batch.
        /// </summary>
        /// <value>
        /// The maximum number of documents in a batch.
        /// </value>
        public int MaxBatchCount
        {
            get { return _maxBatchCount; }
        }

        /// <summary>
        /// Gets the maximum size of a document.
        /// </summary>
        /// <value>
        /// The maximum size of a document.
        /// </value>
        public int MaxDocumentSize
        {
            get { return _maxDocumentSize; }
        }

        /// <summary>
        /// Gets the maximum size of a message.
        /// </summary>
        /// <value>
        /// The maximum size of a message.
        /// </value>
        public int MaxMessageSize
        {
            get { return _maxMessageSize; }
        }

        /// <summary>
        /// Gets the maximum size of a wire document.
        /// </summary>
        /// <value>
        /// The maximum size of a wire document.
        /// </value>
        public int MaxWireDocumentSize
        {
            get { return _maxDocumentSize + 16 * 1024; }
        }

        /// <summary>
        /// Gets the server version.
        /// </summary>
        /// <value>
        /// The server version.
        /// </value>
        public SemanticVersion ServerVersion
        {
            get { return _serverVersion; }
        }

        // methods
        /// <inheritdoc/>
        public bool Equals(ConnectionDescription other)
        {
            if (other == null)
            {
                return false;
            }

            return
                _buildInfoResult.Equals(other._buildInfoResult) &&
                _connectionId.StructurallyEquals(other._connectionId) &&
                _isMasterResult.Equals(other._isMasterResult);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return Equals(obj as ConnectionDescription);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return new Hasher()
                .Hash(_buildInfoResult)
                .Hash(_connectionId)
                .Hash(_isMasterResult)
                .GetHashCode();
        }

        /// <summary>
        /// Returns a new instance of ConnectionDescription with a different connection identifier.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>A connection description.</returns>
        public ConnectionDescription WithConnectionId(ConnectionId value)
        {
            return _connectionId.StructurallyEquals(value) ? this : new ConnectionDescription(value, _isMasterResult, _buildInfoResult);
        }
    }
}
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver.Core.Clusters;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Misc;
using MongoDB.Shared;

namespace MongoDB.Driver.Core.Servers
{
    /// <summary>
    /// Represents information about a server.
    /// </summary>
    public sealed class ServerDescription : IEquatable<ServerDescription>
    {
        // fields
        private readonly TimeSpan _averageRoundTripTime;
        private readonly EndPoint _canonicalEndPoint;
        private readonly ElectionId _electionId;
        private readonly EndPoint _endPoint;
        private readonly Exception _heartbeatException;
        private readonly TimeSpan _heartbeatInterval;
        private readonly DateTime _lastUpdateTimestamp;
        private readonly DateTime? _lastWriteTimestamp;
        private readonly int _maxBatchCount;
        private readonly int _maxDocumentSize;
        private readonly int _maxMessageSize;
        private readonly int _maxWireDocumentSize;
        private readonly ReplicaSetConfig _replicaSetConfig;
        private readonly ServerId _serverId;
        private readonly ServerState _state;
        private readonly TagSet _tags;
        private readonly ServerType _type;
        private readonly SemanticVersion _version;
        private readonly Range<int> _wireVersionRange;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ServerDescription" /> class.
        /// </summary>
        /// <param name="serverId">The server identifier.</param>
        /// <param name="endPoint">The end point.</param>
        /// <param name="averageRoundTripTime">The average round trip time.</param>
        /// <param name="canonicalEndPoint">The canonical end point.</param>
        /// <param name="electionId">The election identifier.</param>
        /// <param name="heartbeatException">The heartbeat exception.</param>
        /// <param name="heartbeatInterval">The heartbeat interval.</param>
        /// <param name="lastUpdateTimestamp">The last update timestamp.</param>
        /// <param name="lastWriteTimestamp">The last write timestamp.</param>
        /// <param name="maxBatchCount">The maximum batch count.</param>
        /// <param name="maxDocumentSize">The maximum size of a document.</param>
        /// <param name="maxMessageSize">The maximum size of a message.</param>
        /// <param name="maxWireDocumentSize">The maximum size of a wire document.</param>
        /// <param name="replicaSetConfig">The replica set configuration.</param>
        /// <param name="state">The server state.</param>
        /// <param name="tags">The replica set tags.</param>
        /// <param name="type">The server type.</param>
        /// <param name="version">The server version.</param>
        /// <param name="wireVersionRange">The wire version range.</param>
        public ServerDescription(
            ServerId serverId,
            EndPoint endPoint,
            Optional<TimeSpan> averageRoundTripTime = default(Optional<TimeSpan>),
            Optional<EndPoint> canonicalEndPoint = default(Optional<EndPoint>),
            Optional<ElectionId> electionId = default(Optional<ElectionId>),
            Optional<Exception> heartbeatException = default(Optional<Exception>),
            Optional<TimeSpan> heartbeatInterval = default(Optional<TimeSpan>),
            Optional<DateTime> lastUpdateTimestamp = default(Optional<DateTime>),
            Optional<DateTime?> lastWriteTimestamp = default(Optional<DateTime?>),
            Optional<int> maxBatchCount = default(Optional<int>),
            Optional<int> maxDocumentSize = default(Optional<int>),
            Optional<int> maxMessageSize = default(Optional<int>),
            Optional<int> maxWireDocumentSize = default(Optional<int>),
            Optional<ReplicaSetConfig> replicaSetConfig = default(Optional<ReplicaSetConfig>),
            Optional<ServerState> state = default(Optional<ServerState>),
            Optional<TagSet> tags = default(Optional<TagSet>),
            Optional<ServerType> type = default(Optional<ServerType>),
            Optional<SemanticVersion> version = default(Optional<SemanticVersion>),
            Optional<Range<int>> wireVersionRange = default(Optional<Range<int>>))
        {
            Ensure.IsNotNull(endPoint, nameof(endPoint));
            Ensure.IsNotNull(serverId, nameof(serverId));
            if (!EndPointHelper.Equals(endPoint, serverId.EndPoint))
            {
                throw new ArgumentException("EndPoint and ServerId.EndPoint must match.");
            }

            _averageRoundTripTime = averageRoundTripTime.WithDefault(TimeSpan.Zero);
            _canonicalEndPoint = canonicalEndPoint.WithDefault(null);
            _electionId = electionId.WithDefault(null);
            _endPoint = endPoint;
            _heartbeatException = heartbeatException.WithDefault(null);
            _heartbeatInterval = heartbeatInterval.WithDefault(TimeSpan.Zero);
            _lastUpdateTimestamp = lastUpdateTimestamp.WithDefault(DateTime.UtcNow);
            _lastWriteTimestamp = lastWriteTimestamp.WithDefault(null);
            _maxBatchCount = maxBatchCount.WithDefault(1000);
            _maxDocumentSize = maxDocumentSize.WithDefault(4 * 1024 * 1024);
            _maxMessageSize = maxMessageSize.WithDefault(Math.Max(_maxDocumentSize + 1024, 16000000));
            _maxWireDocumentSize = maxWireDocumentSize.WithDefault(_maxDocumentSize + 16 * 1024);
            _replicaSetConfig = replicaSetConfig.WithDefault(null);
            _serverId = serverId;
            _state = state.WithDefault(ServerState.Disconnected);
            _tags = tags.WithDefault(null);
            _type = type.WithDefault(ServerType.Unknown);
            _version = version.WithDefault(null);
            _wireVersionRange = wireVersionRange.WithDefault(null);
        }

        // properties
        /// <summary>
        /// Gets the average round trip time.
        /// </summary>
        /// <value>
        /// The average round trip time.
        /// </value>
        public TimeSpan AverageRoundTripTime
        {
            get { return _averageRoundTripTime; }
        }

        /// <summary>
        /// Gets the canonical end point. This is the endpoint that the cluster knows this 
        /// server by. Currently, it only applies to a replica set config and will match
        /// what is in the replica set configuration.
        /// </summary>
        public EndPoint CanonicalEndPoint
        {
            get { return _canonicalEndPoint; }
        }

        /// <summary>
        /// Gets the election identifier.
        /// </summary>
        public ElectionId ElectionId
        {
            get { return _electionId; }
        }

        /// <summary>
        /// Gets the end point.
        /// </summary>
        /// <value>
        /// The end point.
        /// </value>
        public EndPoint EndPoint
        {
            get { return _endPoint; }
        }

        /// <summary>
        /// Gets the most recent heartbeat exception.
        /// </summary>
        /// <value>
        /// The the most recent heartbeat exception (null if the most recent heartbeat succeeded).
        /// </value>
        public Exception HeartbeatException
        {
            get { return _heartbeatException; }
        }

        /// <summary>
        /// Gets the heartbeat interval.
        /// </summary>
        /// <value>
        /// The heartbeat interval.
        /// </value>
        public TimeSpan HeartbeatInterval
        {
            get { return _heartbeatInterval; }
        }

        /// <summary>
        /// Gets the last update timestamp (when the ServerDescription itself was last updated).
        /// </summary>
        /// <value>
        /// The last update timestamp.
        /// </value>
        public DateTime LastUpdateTimestamp
        {
            get { return _lastUpdateTimestamp; }
        }

        /// <summary>
        /// Gets the last write timestamp (from the lastWrite field of the isMaster result).
        /// </summary>
        /// <value>
        /// The last write timestamp.
        /// </value>
        public DateTime? LastWriteTimestamp
        {
            get { return _lastWriteTimestamp; }
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
            get { return _maxWireDocumentSize; }
        }

        /// <summary>
        /// Gets the replica set configuration.
        /// </summary>
        /// <value>
        /// The replica set configuration.
        /// </value>
        public ReplicaSetConfig ReplicaSetConfig
        {
            get { return _replicaSetConfig; }
        }

        /// <summary>
        /// Gets the server identifier.
        /// </summary>
        /// <value>
        /// The server identifier.
        /// </value>
        public ServerId ServerId
        {
            get { return _serverId; }
        }

        /// <summary>
        /// Gets the server state.
        /// </summary>
        /// <value>
        /// The server state.
        /// </value>
        public ServerState State
        {
            get { return _state; }
        }

        /// <summary>
        /// Gets the replica set tags.
        /// </summary>
        /// <value>
        /// The replica set tags (null if not a replica set or if the replica set has no tags).
        /// </value>
        public TagSet Tags
        {
            get { return _tags; }
        }

        /// <summary>
        /// Gets the server type.
        /// </summary>
        /// <value>
        /// The server type.
        /// </value>
        public ServerType Type
        {
            get { return _type; }
        }

        /// <summary>
        /// Gets the server version.
        /// </summary>
        /// <value>
        /// The server version.
        /// </value>
        public SemanticVersion Version
        {
            get { return _version; }
        }

        /// <summary>
        /// Gets the wire version range.
        /// </summary>
        /// <value>
        /// The wire version range.
        /// </value>
        public Range<int> WireVersionRange
        {
            get { return _wireVersionRange; }
        }

        // methods
        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return Equals(obj as ServerDescription);
        }

        /// <inheritdoc/>
        public bool Equals(ServerDescription other)
        {
            if (object.ReferenceEquals(other, null) || other.GetType() != typeof(ServerDescription))
            {
                return false;
            }

            return
                _averageRoundTripTime == other._averageRoundTripTime &&
                object.Equals(_canonicalEndPoint, other._canonicalEndPoint) &&
                object.Equals(_electionId, other._electionId) &&
                EndPointHelper.Equals(_endPoint, other._endPoint) &&
                object.Equals(_heartbeatException, other._heartbeatException) &&
                _heartbeatInterval == other._heartbeatInterval &&
                _lastUpdateTimestamp == other._lastUpdateTimestamp &&
                _lastWriteTimestamp == other._lastWriteTimestamp &&
                _maxBatchCount == other._maxBatchCount &&
                _maxDocumentSize == other._maxDocumentSize &&
                _maxMessageSize == other._maxMessageSize &&
                _maxWireDocumentSize == other._maxWireDocumentSize &&
                object.Equals(_replicaSetConfig, other._replicaSetConfig) &&
                _serverId.Equals(other._serverId) &&
                _state == other._state &&
                object.Equals(_tags, other._tags) &&
                _type == other._type &&
                object.Equals(_version, other._version) &&
                object.Equals(_wireVersionRange, other._wireVersionRange);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            // revision is ignored
            return new Hasher()
                .Hash(_averageRoundTripTime)
                .Hash(_canonicalEndPoint)
                .Hash(_electionId)
                .Hash(_endPoint)
                .Hash(_heartbeatException)
                .Hash(_heartbeatInterval)
                .Hash(_lastUpdateTimestamp)
                .Hash(_lastWriteTimestamp)
                .Hash(_maxBatchCount)
                .Hash(_maxDocumentSize)
                .Hash(_maxMessageSize)
                .Hash(_maxWireDocumentSize)
                .Hash(_replicaSetConfig)
                .Hash(_serverId)
                .Hash(_state)
                .Hash(_tags)
                .Hash(_type)
                .Hash(_version)
                .Hash(_wireVersionRange)
                .GetHashCode();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return new StringBuilder()
                .Append("{ ")
                .AppendFormat("ServerId: \"{0}\"", _serverId)
                .AppendFormat(", EndPoint: \"{0}\"", _endPoint)
                .AppendFormat(", State: \"{0}\"", _state)
                .AppendFormat(", Type: \"{0}\"", _type)
                .AppendFormatIf(_tags != null && !_tags.IsEmpty, ", Tags: \"{0}\"", _tags)
                .AppendFormatIf(_state == ServerState.Connected, ", WireVersionRange: \"{0}\"", _wireVersionRange)
                .AppendFormatIf(_electionId != null, ", ElectionId: \"{0}\"", _electionId)
                .AppendFormatIf(_heartbeatException != null, ", HeartbeatException: \"{0}\"", _heartbeatException)
                .Append(" }")
                .ToString();
        }

        /// <summary>
        /// Returns a new instance of ServerDescription with some values changed.
        /// </summary>
        /// <param name="averageRoundTripTime">The average round trip time.</param>
        /// <param name="canonicalEndPoint">The canonical end point.</param>
        /// <param name="electionId">The election identifier.</param>
        /// <param name="heartbeatException">The heartbeat exception.</param>
        /// <param name="heartbeatInterval">The heartbeat interval.</param>
        /// <param name="lastUpdateTimestamp">The last update timestamp.</param>
        /// <param name="lastWriteTimestamp">The last write timestamp.</param>
        /// <param name="maxBatchCount">The maximum batch count.</param>
        /// <param name="maxDocumentSize">The maximum size of a document.</param>
        /// <param name="maxMessageSize">The maximum size of a message.</param>
        /// <param name="maxWireDocumentSize">The maximum size of a wire document.</param>
        /// <param name="replicaSetConfig">The replica set configuration.</param>
        /// <param name="state">The server state.</param>
        /// <param name="tags">The replica set tags.</param>
        /// <param name="type">The server type.</param>
        /// <param name="version">The server version.</param>
        /// <param name="wireVersionRange">The wire version range.</param>
        /// <returns>
        /// A new instance of ServerDescription.
        /// </returns>
        public ServerDescription With(
            Optional<TimeSpan> averageRoundTripTime = default(Optional<TimeSpan>),
            Optional<EndPoint> canonicalEndPoint = default(Optional<EndPoint>),
            Optional<ElectionId> electionId = default(Optional<ElectionId>),
            Optional<Exception> heartbeatException = default(Optional<Exception>),
            Optional<TimeSpan> heartbeatInterval = default(Optional<TimeSpan>),
            Optional<DateTime> lastUpdateTimestamp = default(Optional<DateTime>),
            Optional<DateTime?> lastWriteTimestamp = default(Optional<DateTime?>),
            Optional<int> maxBatchCount = default(Optional<int>),
            Optional<int> maxDocumentSize = default(Optional<int>),
            Optional<int> maxMessageSize = default(Optional<int>),
            Optional<int> maxWireDocumentSize = default(Optional<int>),
            Optional<ReplicaSetConfig> replicaSetConfig = default(Optional<ReplicaSetConfig>),
            Optional<ServerState> state = default(Optional<ServerState>),
            Optional<TagSet> tags = default(Optional<TagSet>),
            Optional<ServerType> type = default(Optional<ServerType>),
            Optional<SemanticVersion> version = default(Optional<SemanticVersion>),
            Optional<Range<int>> wireVersionRange = default(Optional<Range<int>>))
        {
            if (!lastUpdateTimestamp.HasValue)
            {
                lastUpdateTimestamp = DateTime.UtcNow;
            }

            if (
                averageRoundTripTime.Replaces(_averageRoundTripTime) ||
                canonicalEndPoint.Replaces(_canonicalEndPoint) ||
                electionId.Replaces(_electionId) ||
                heartbeatException.Replaces(_heartbeatException) ||
                heartbeatInterval.Replaces(_heartbeatInterval) ||
                lastUpdateTimestamp.Replaces(_lastUpdateTimestamp) ||
                lastWriteTimestamp.Replaces(_lastWriteTimestamp) ||
                maxBatchCount.Replaces(_maxBatchCount) ||
                maxDocumentSize.Replaces(_maxDocumentSize) ||
                maxMessageSize.Replaces(_maxMessageSize) ||
                maxWireDocumentSize.Replaces(_maxWireDocumentSize) ||
                replicaSetConfig.Replaces(_replicaSetConfig) ||
                state.Replaces(_state) ||
                tags.Replaces(_tags) ||
                type.Replaces(_type) ||
                version.Replaces(_version) ||
                wireVersionRange.Replaces(_wireVersionRange))
            {
                return new ServerDescription(
                    _serverId,
                    _endPoint,
                    averageRoundTripTime: averageRoundTripTime.WithDefault(_averageRoundTripTime),
                    canonicalEndPoint: canonicalEndPoint.WithDefault(_canonicalEndPoint),
                    electionId: electionId.WithDefault(_electionId),
                    heartbeatException: heartbeatException.WithDefault(_heartbeatException),
                    heartbeatInterval: heartbeatInterval.WithDefault(_heartbeatInterval),
                    lastUpdateTimestamp: lastUpdateTimestamp.WithDefault(_lastUpdateTimestamp),
                    lastWriteTimestamp: lastWriteTimestamp.WithDefault(_lastWriteTimestamp),
                    maxBatchCount: maxBatchCount.WithDefault(_maxBatchCount),
                    maxDocumentSize: maxDocumentSize.WithDefault(_maxDocumentSize),
                    maxMessageSize: maxMessageSize.WithDefault(_maxMessageSize),
                    maxWireDocumentSize: maxWireDocumentSize.WithDefault(_maxWireDocumentSize),
                    replicaSetConfig: replicaSetConfig.WithDefault(_replicaSetConfig),
                    state: state.WithDefault(_state),
                    tags: tags.WithDefault(_tags),
                    type: type.WithDefault(_type),
                    version: version.WithDefault(_version),
                    wireVersionRange: wireVersionRange.WithDefault(_wireVersionRange));
            }
            else
            {
                return this;
            }
        }
    }
}

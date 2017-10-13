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
using System.Collections.Generic;
using System.Linq;
using System.Net;
using MongoDB.Driver.Core.Clusters;
using MongoDB.Driver.Core.Clusters.ServerSelectors;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Core.Configuration
{
    /// <summary>
    /// Represents settings for a cluster.
    /// </summary>
    public class ClusterSettings
    {
        #region static
        // static fields
        private static readonly IReadOnlyList<EndPoint> __defaultEndPoints = new EndPoint[] { new DnsEndPoint("localhost", 27017) };
        #endregion

        // fields
        private readonly ClusterConnectionMode _connectionMode;
        private readonly IReadOnlyList<EndPoint> _endPoints;
        private readonly int _maxServerSelectionWaitQueueSize;
        private readonly string _replicaSetName;
        private readonly TimeSpan _serverSelectionTimeout;
        private readonly IServerSelector _preServerSelector;
        private readonly IServerSelector _postServerSelector;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ClusterSettings"/> class.
        /// </summary>
        /// <param name="connectionMode">The connection mode.</param>
        /// <param name="endPoints">The end points.</param>
        /// <param name="maxServerSelectionWaitQueueSize">Maximum size of the server selection wait queue.</param>
        /// <param name="replicaSetName">Name of the replica set.</param>
        /// <param name="serverSelectionTimeout">The server selection timeout.</param>
        /// <param name="preServerSelector">The pre server selector.</param>
        /// <param name="postServerSelector">The post server selector.</param>
        public ClusterSettings(
            Optional<ClusterConnectionMode> connectionMode = default(Optional<ClusterConnectionMode>),
            Optional<IEnumerable<EndPoint>> endPoints = default(Optional<IEnumerable<EndPoint>>),
            Optional<int> maxServerSelectionWaitQueueSize = default(Optional<int>),
            Optional<string> replicaSetName = default(Optional<string>),
            Optional<TimeSpan> serverSelectionTimeout = default(Optional<TimeSpan>),
            Optional<IServerSelector> preServerSelector = default(Optional<IServerSelector>),
            Optional<IServerSelector> postServerSelector = default(Optional<IServerSelector>))
        {
            _connectionMode = connectionMode.WithDefault(ClusterConnectionMode.Automatic);
            _endPoints = Ensure.IsNotNull(endPoints.WithDefault(__defaultEndPoints), "endPoints").ToList();
            _maxServerSelectionWaitQueueSize = Ensure.IsGreaterThanOrEqualToZero(maxServerSelectionWaitQueueSize.WithDefault(500), "maxServerSelectionWaitQueueSize");
            _replicaSetName = replicaSetName.WithDefault(null);
            _serverSelectionTimeout = Ensure.IsGreaterThanOrEqualToZero(serverSelectionTimeout.WithDefault(TimeSpan.FromSeconds(30)), "serverSelectionTimeout");
            _preServerSelector = preServerSelector.WithDefault(null);
            _postServerSelector = postServerSelector.WithDefault(null);
        }

        // properties
        /// <summary>
        /// Gets the connection mode.
        /// </summary>
        /// <value>
        /// The connection mode.
        /// </value>
        public ClusterConnectionMode ConnectionMode
        {
            get { return _connectionMode; }
        }

        /// <summary>
        /// Gets the end points.
        /// </summary>
        /// <value>
        /// The end points.
        /// </value>
        public IReadOnlyList<EndPoint> EndPoints
        {
            get { return _endPoints; }
        }

        /// <summary>
        /// Gets the maximum size of the server selection wait queue.
        /// </summary>
        /// <value>
        /// The maximum size of the server selection wait queue.
        /// </value>
        public int MaxServerSelectionWaitQueueSize
        {
            get { return _maxServerSelectionWaitQueueSize; }
        }

        /// <summary>
        /// Gets the name of the replica set.
        /// </summary>
        /// <value>
        /// The name of the replica set.
        /// </value>
        public string ReplicaSetName
        {
            get { return _replicaSetName; }
        }

        /// <summary>
        /// Gets the server selection timeout.
        /// </summary>
        /// <value>
        /// The server selection timeout.
        /// </value>
        public TimeSpan ServerSelectionTimeout
        {
            get { return _serverSelectionTimeout; }
        }

        /// <summary>
        /// Gets the pre server selector.
        /// </summary>
        /// <value>
        /// The pre server selector.
        /// </value>
        public IServerSelector PreServerSelector
        {
            get { return _preServerSelector; }
        }

        /// <summary>
        /// Gets the post server selector.
        /// </summary>
        /// <value>
        /// The post server selector.
        /// </value>
        public IServerSelector PostServerSelector
        {
            get { return _postServerSelector; }
        }

        // methods
        /// <summary>
        /// Returns a new ClusterSettings instance with some settings changed.
        /// </summary>
        /// <param name="connectionMode">The connection mode.</param>
        /// <param name="endPoints">The end points.</param>
        /// <param name="maxServerSelectionWaitQueueSize">Maximum size of the server selection wait queue.</param>
        /// <param name="replicaSetName">Name of the replica set.</param>
        /// <param name="serverSelectionTimeout">The server selection timeout.</param>
        /// <param name="preServerSelector">The pre server selector.</param>
        /// <param name="postServerSelector">The post server selector.</param>
        /// <returns>A new ClusterSettings instance.</returns>
        public ClusterSettings With(
            Optional<ClusterConnectionMode> connectionMode = default(Optional<ClusterConnectionMode>),
            Optional<IEnumerable<EndPoint>> endPoints = default(Optional<IEnumerable<EndPoint>>),
            Optional<int> maxServerSelectionWaitQueueSize = default(Optional<int>),
            Optional<string> replicaSetName = default(Optional<string>),
            Optional<TimeSpan> serverSelectionTimeout = default(Optional<TimeSpan>),
            Optional<IServerSelector> preServerSelector = default(Optional<IServerSelector>),
            Optional<IServerSelector> postServerSelector = default(Optional<IServerSelector>))
        {
            return new ClusterSettings(
                connectionMode: connectionMode.WithDefault(_connectionMode),
                endPoints: Optional.Enumerable(endPoints.WithDefault(_endPoints)),
                maxServerSelectionWaitQueueSize: maxServerSelectionWaitQueueSize.WithDefault(_maxServerSelectionWaitQueueSize),
                replicaSetName: replicaSetName.WithDefault(_replicaSetName),
                serverSelectionTimeout: serverSelectionTimeout.WithDefault(_serverSelectionTimeout),
                preServerSelector: Optional.Create(preServerSelector.WithDefault(_preServerSelector)),
                postServerSelector: Optional.Create(postServerSelector.WithDefault(_postServerSelector)));
        }
    }
}

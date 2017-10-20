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
using System.Threading;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.Servers;

namespace MongoDB.Driver.Core.Clusters.ServerSelectors
{
    /// <summary>
    /// Represents a selector that selects servers based on a read preference.
    /// </summary>
    public class ReadPreferenceServerSelector : IServerSelector
    {
        #region static
        // static fields
        private static readonly ServerDescription[] __noServers = new ServerDescription[0];
        private static readonly ReadPreferenceServerSelector __primary = new ReadPreferenceServerSelector(ReadPreference.Primary);

        // static properties
        /// <summary>
        /// Gets a ReadPreferenceServerSelector that selects the Primary.
        /// </summary>
        /// <value>
        /// A server selector.
        /// </value>
        public static ReadPreferenceServerSelector Primary
        {
            get { return __primary; }
        }
        #endregion

        // fields
        private readonly TimeSpan? _maxStaleness; // with InfiniteTimespan converted to null
        private readonly ReadPreference _readPreference;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadPreferenceServerSelector"/> class.
        /// </summary>
        /// <param name="readPreference">The read preference.</param>
        public ReadPreferenceServerSelector(ReadPreference readPreference)
        {
            _readPreference = Ensure.IsNotNull(readPreference, nameof(readPreference));
            if (readPreference.MaxStaleness == Timeout.InfiniteTimeSpan)
            {
                _maxStaleness = null;
            }
            else
            {
                _maxStaleness = readPreference.MaxStaleness;
            }
        }

        // methods
        /// <inheritdoc/>
        public IEnumerable<ServerDescription> SelectServers(ClusterDescription cluster, IEnumerable<ServerDescription> servers)
        {
            if (_maxStaleness.HasValue)
            {
                if (cluster.Servers.Any(s => s.Type != ServerType.Unknown && !Feature.MaxStaleness.IsSupported(s.Version)))
                {
                    throw new NotSupportedException("All servers must be version 3.4 or newer to use max staleness.");
                }
            }

            if (cluster.ConnectionMode == ClusterConnectionMode.Direct)
            {
                return servers;
            }

            switch (cluster.Type)
            {
                case ClusterType.ReplicaSet: return SelectForReplicaSet(cluster, servers);
                case ClusterType.Sharded: return SelectForShardedCluster(servers);
                case ClusterType.Standalone: return SelectForStandaloneCluster(servers);
                case ClusterType.Unknown: return __noServers;
                default:
                    var message = string.Format("ReadPreferenceServerSelector is not implemented for cluster of type: {0}.", cluster.Type);
                    throw new NotImplementedException(message);
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Format("ReadPreferenceServerSelector{{ ReadPreference = {0} }}", _readPreference);
        }

        private IEnumerable<ServerDescription> SelectByTagSets(IEnumerable<ServerDescription> servers)
        {
            var tagSets = _readPreference.TagSets;
            if (tagSets == null || !tagSets.Any())
            {
                return servers;
            }

            foreach (var tagSet in _readPreference.TagSets)
            {
                var matchingServers = new List<ServerDescription>();
                foreach (var server in servers)
                {
                    if (server.Tags != null && server.Tags.ContainsAll(tagSet))
                    {
                        matchingServers.Add(server);
                    }
                }

                if (matchingServers.Count > 0)
                {
                    return matchingServers;
                }
            }

            return __noServers;
        }

        private IEnumerable<ServerDescription> SelectForReplicaSet(ClusterDescription cluster, IEnumerable<ServerDescription> servers)
        {
            EnsureMaxStalenessIsValid(cluster);

            servers = new CachedEnumerable<ServerDescription>(servers); // prevent multiple enumeration

            switch (_readPreference.ReadPreferenceMode)
            {
                case ReadPreferenceMode.Primary:
                    return SelectPrimary(servers);

                case ReadPreferenceMode.PrimaryPreferred:
                    var primary = SelectPrimary(servers);
                    if (primary.Count != 0)
                    {
                        return primary;
                    }
                    else
                    {
                        return SelectByTagSets(SelectFreshSecondaries(cluster, servers));
                    }

                case ReadPreferenceMode.Secondary:
                    return SelectByTagSets(SelectFreshSecondaries(cluster, servers));

                case ReadPreferenceMode.SecondaryPreferred:
                    var selectedSecondaries = SelectByTagSets(SelectFreshSecondaries(cluster, servers)).ToList();
                    if (selectedSecondaries.Count != 0)
                    {
                        return selectedSecondaries;
                    }
                    else
                    {
                        return SelectPrimary(servers);
                    }

                case ReadPreferenceMode.Nearest:
                    return SelectByTagSets(SelectPrimary(servers).Concat(SelectFreshSecondaries(cluster, servers)));

                default:
                    throw new ArgumentException("Invalid ReadPreferenceMode.");
            }
        }

        private IEnumerable<ServerDescription> SelectForShardedCluster(IEnumerable<ServerDescription> servers)
        {
            return servers.Where(n => n.Type == ServerType.ShardRouter); // ReadPreference will be sent to mongos
        }

        private IEnumerable<ServerDescription> SelectForStandaloneCluster(IEnumerable<ServerDescription> servers)
        {
            return servers.Where(n => n.Type == ServerType.Standalone); // standalone servers match any ReadPreference (to facilitate testing)
        }

        private List<ServerDescription> SelectPrimary(IEnumerable<ServerDescription> servers)
        {
            var primary = servers.Where(s => s.Type == ServerType.ReplicaSetPrimary).ToList();
            if (primary.Count > 1)
            {
                throw new MongoClientException($"More than one primary found: [{string.Join(", ", servers.Select(s => s.ToString()))}].");
            }
            return primary; // returned as a list because otherwise some callers would have to create a new list
        }

        private IEnumerable<ServerDescription> SelectSecondaries(IEnumerable<ServerDescription> servers)
        {
            return servers.Where(s => s.Type == ServerType.ReplicaSetSecondary);
        }

        private IEnumerable<ServerDescription> SelectFreshSecondaries(ClusterDescription cluster, IEnumerable<ServerDescription> servers)
        {
            var secondaries = SelectSecondaries(servers);

            if (_maxStaleness.HasValue)
            {
                var primary = SelectPrimary(cluster.Servers).SingleOrDefault();
                if (primary == null)
                {
                    return SelectFreshSecondariesWithNoPrimary(secondaries);
                }
                else
                {

                    return SelectFreshSecondariesWithPrimary(primary, secondaries);
                }
            }
            else
            {
                return secondaries;
            }
        }

        private IEnumerable<ServerDescription> SelectFreshSecondariesWithNoPrimary(IEnumerable<ServerDescription> secondaries)
        {
            var smax = secondaries
                .OrderByDescending(s => s.LastWriteTimestamp)
                .FirstOrDefault();
            if (smax == null)
            {
                return Enumerable.Empty<ServerDescription>();
            }

            return secondaries
                .Where(s =>
                {
                    var estimatedStaleness = smax.LastWriteTimestamp.Value - s.LastWriteTimestamp.Value + s.HeartbeatInterval;
                    return estimatedStaleness <= _maxStaleness;
                });
        }

        private IEnumerable<ServerDescription> SelectFreshSecondariesWithPrimary(ServerDescription primary, IEnumerable<ServerDescription> secondaries)
        {
            var p = primary;
            return secondaries
                .Where(s =>
                {
                    var estimatedStaleness = (s.LastUpdateTimestamp - s.LastWriteTimestamp.Value) - (p.LastUpdateTimestamp - p.LastWriteTimestamp.Value) + s.HeartbeatInterval;
                    return estimatedStaleness <= _maxStaleness;
                });
        }

        private void EnsureMaxStalenessIsValid(ClusterDescription cluster)
        {
            if (_maxStaleness.HasValue)
            {
                if (_maxStaleness.Value < TimeSpan.FromSeconds(90))
                {
                    var message = string.Format(
                        "Max staleness ({0} seconds) must greater than or equal to 90 seconds.",
                        (int)_maxStaleness.Value.TotalSeconds);
                    throw new Exception(message);
                }

                var anyServer = cluster.Servers.FirstOrDefault();
                if (anyServer == null)
                {
                    return;
                }

                var heartbeatInterval = anyServer.HeartbeatInterval; // all servers have the same HeartbeatInterval
                var idleWritePeriod = TimeSpan.FromSeconds(10);

                if (_maxStaleness.Value < heartbeatInterval + idleWritePeriod)
                {
                    var message = string.Format(
                        "Max staleness ({0} seconds) must greater than or equal to heartbeat interval ({1} seconds) plus idle write period ({2} seconds).",
                        (int)_maxStaleness.Value.TotalSeconds,
                        (int)heartbeatInterval.TotalSeconds,
                        (int)idleWritePeriod.TotalSeconds);
                    throw new Exception(message);
                }
            }
        }
    }
}

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
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver.Core.Clusters;

namespace MongoDB.Driver.Core.Servers
{
    /// <summary>
    /// Represents the server type.
    /// </summary>
    public enum ServerType
    {
        /// <summary>
        /// The server type is unknown.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// The server is a standalone server.
        /// </summary>
        Standalone,

        /// <summary>
        /// The server is a shard router.
        /// </summary>
        ShardRouter,

        /// <summary>
        /// The server is a replica set primary.
        /// </summary>
        ReplicaSetPrimary,

        /// <summary>
        /// The server is a replica set secondary.
        /// </summary>
        ReplicaSetSecondary,

        /// <summary>
        /// Use ReplicaSetSecondary instead.
        /// </summary>
        [Obsolete("Passives are treated the same as secondaries.")]
        ReplicaSetPassive,

        /// <summary>
        /// The server is a replica set arbiter.
        /// </summary>
        ReplicaSetArbiter,

        /// <summary>
        /// The server is a replica set member of some other type.
        /// </summary>
        ReplicaSetOther,

        /// <summary>
        /// The server is a replica set ghost member.
        /// </summary>
        ReplicaSetGhost
    }

    /// <summary>
    /// Represents extension methods on ServerType.
    /// </summary>
    public static class ServerTypeExtensions
    {
        /// <summary>
        /// Determines whether this server type is a replica set member.
        /// </summary>
        /// <param name="serverType">The type of the server.</param>
        /// <returns>Whether this server type is a replica set member.</returns>
        public static bool IsReplicaSetMember(this ServerType serverType)
        {
            return ToClusterType(serverType) == ClusterType.ReplicaSet;
        }

        /// <summary>
        /// Determines whether this server type is a writable server.
        /// </summary>
        /// <param name="serverType">The type of the server.</param>
        /// <returns>Whether this server type is a writable server.</returns>
        public static bool IsWritable(this ServerType serverType)
        {
            switch (serverType)
            {
                case ServerType.ReplicaSetPrimary:
                case ServerType.ShardRouter:
                case ServerType.Standalone:
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Infers the cluster type from the server type.
        /// </summary>
        /// <param name="serverType">The type of the server.</param>
        /// <returns>The cluster type.</returns>
        public static ClusterType ToClusterType(this ServerType serverType)
        {
            switch (serverType)
            {
                case ServerType.ReplicaSetPrimary:
                case ServerType.ReplicaSetSecondary:
                case ServerType.ReplicaSetArbiter:
                case ServerType.ReplicaSetOther:
                case ServerType.ReplicaSetGhost:
                    return ClusterType.ReplicaSet;
                case ServerType.ShardRouter:
                    return ClusterType.Sharded;
                case ServerType.Standalone:
                    return ClusterType.Standalone;
                case ServerType.Unknown:
                    return ClusterType.Unknown;
                default:
                    var message = string.Format("Invalid server type: {0}.", serverType);
                    throw new ArgumentException(message, "serverType");
            }
        }
    }
}

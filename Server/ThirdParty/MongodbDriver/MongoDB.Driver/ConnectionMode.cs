/* Copyright 2010-2016 MongoDB Inc.
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

namespace MongoDB.Driver
{
    /// <summary>
    /// Server connection mode.
    /// </summary>
#if NET45
    [Serializable]
#endif
    public enum ConnectionMode
    {
        /// <summary>
        /// Automatically determine how to connect.
        /// </summary>
        Automatic,
        /// <summary>
        /// Connect directly to a server.
        /// </summary>
        Direct,
        /// <summary>
        /// Connect to a replica set.
        /// </summary>
        ReplicaSet,
        /// <summary>
        /// Connect to one or more shard routers.
        /// </summary>
        ShardRouter,
        /// <summary>
        /// Connect to a standalone server.
        /// </summary>
        Standalone
    }

    internal static class ConnectionModeExtensionMethods
    {
        public static ClusterConnectionMode ToCore(this ConnectionMode value)
        {
            switch (value)
            {
                case ConnectionMode.Automatic: return ClusterConnectionMode.Automatic;
                case ConnectionMode.Direct: return ClusterConnectionMode.Direct;
                case ConnectionMode.ReplicaSet: return ClusterConnectionMode.ReplicaSet;
                case ConnectionMode.ShardRouter: return ClusterConnectionMode.Sharded;
                case ConnectionMode.Standalone: return ClusterConnectionMode.Standalone;
                default: throw new ArgumentException("Invalid ConnectionMode.", "value");
            }
        }
    }
}
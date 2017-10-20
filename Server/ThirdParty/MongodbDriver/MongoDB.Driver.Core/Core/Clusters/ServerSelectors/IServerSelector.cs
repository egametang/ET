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

using System.Collections.Generic;
using MongoDB.Driver.Core.Servers;

namespace MongoDB.Driver.Core.Clusters.ServerSelectors
{
    /// <summary>
    /// Represents a selector that selects servers.
    /// </summary>
    public interface IServerSelector
    {
        /// <summary>
        /// Selects the servers.
        /// </summary>
        /// <param name="cluster">The cluster.</param>
        /// <param name="servers">The servers.</param>
        /// <returns>The selected servers.</returns>
        IEnumerable<ServerDescription> SelectServers(ClusterDescription cluster, IEnumerable<ServerDescription> servers);
    }
}

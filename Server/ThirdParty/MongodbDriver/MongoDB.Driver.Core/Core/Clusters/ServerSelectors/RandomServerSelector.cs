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
using System.Linq;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.Servers;

namespace MongoDB.Driver.Core.Clusters.ServerSelectors
{
    /// <summary>
    /// Represents a selector that selects a random server.
    /// </summary>
    public class RandomServerSelector : IServerSelector
    {
        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="RandomServerSelector"/> class.
        /// </summary>
        public RandomServerSelector()
        {
        }

        // methods
        /// <inheritdoc/>
        public IEnumerable<ServerDescription> SelectServers(ClusterDescription cluster, IEnumerable<ServerDescription> servers)
        {
            var list = servers.ToList();
            switch (list.Count)
            {
                case 0:
                case 1:
                    return list;
                default:
                    var index = ThreadStaticRandom.Next(list.Count);
                    return new[] { list[index] };
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "RandomServerSelector";
        }
    }
}

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
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.Servers;

namespace MongoDB.Driver.Core.Clusters.ServerSelectors
{
    /// <summary>
    /// Represents a server selector that wraps a delegate.
    /// </summary>
    public class DelegateServerSelector : IServerSelector
    {
        // fields
        private readonly Func<ClusterDescription, IEnumerable<ServerDescription>, IEnumerable<ServerDescription>> _selector;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateServerSelector"/> class.
        /// </summary>
        /// <param name="selector">The selector.</param>
        public DelegateServerSelector(Func<ClusterDescription, IEnumerable<ServerDescription>, IEnumerable<ServerDescription>> selector)
        {
            _selector = Ensure.IsNotNull(selector, nameof(selector));
        }

        // methods
        /// <inheritdoc/>
        public IEnumerable<ServerDescription> SelectServers(ClusterDescription cluster, IEnumerable<ServerDescription> servers)
        {
            return _selector(cluster, servers);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "DelegateServerSelector";
        }
    }
}

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
using System.Net;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.Servers;

namespace MongoDB.Driver.Core.Clusters.ServerSelectors
{
    /// <summary>
    /// Represents a selector that selects servers based on an end point.
    /// </summary>
    public class EndPointServerSelector : IServerSelector
    {
        // fields
        private readonly EndPoint _endPoint;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="EndPointServerSelector"/> class.
        /// </summary>
        /// <param name="endPoint">The end point.</param>
        public EndPointServerSelector(EndPoint endPoint)
        {
            _endPoint = Ensure.IsNotNull(endPoint, nameof(endPoint));
        }

        // methods
        /// <inheritdoc/>
        public IEnumerable<ServerDescription> SelectServers(ClusterDescription cluster, IEnumerable<ServerDescription> servers)
        {
            return servers.Where(server => EndPointHelper.Equals(server.EndPoint, _endPoint));
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Format("EndPointServerSelector{{ EndPoint = {0} }}", EndPointHelper.ToString(_endPoint));
        }
    }
}

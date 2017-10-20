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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver.Core.Clusters;
using MongoDB.Driver.Core.Connections;

namespace MongoDB.Driver.Core.Servers
{
    /// <summary>
    /// Represents a server factory.
    /// </summary>
    public interface IClusterableServerFactory
    {
        // methods
        /// <summary>
        /// Creates the server.
        /// </summary>
        /// <param name="clusterId">The cluster identifier.</param>
        /// <param name="endPoint">The end point.</param>
        /// <returns>A server.</returns>
        IClusterableServer CreateServer(ClusterId clusterId, EndPoint endPoint);
    }
}

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

using MongoDB.Driver.Core.Clusters;

namespace MongoDB.Driver.Core.Events
{
    /// <preliminary/>
    /// <summary>
    /// Occurs before a cluster is closed.
    /// </summary>
    public struct ClusterClosingEvent
    {
        private readonly ClusterId _clusterId;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClusterClosingEvent"/> struct.
        /// </summary>
        /// <param name="clusterId">The cluster identifier.</param>
        public ClusterClosingEvent(ClusterId clusterId)
        {
            _clusterId = clusterId;
        }

        /// <summary>
        /// Gets the cluster identifier.
        /// </summary>
        public ClusterId ClusterId
        {
            get { return _clusterId; }
        }
    }
}

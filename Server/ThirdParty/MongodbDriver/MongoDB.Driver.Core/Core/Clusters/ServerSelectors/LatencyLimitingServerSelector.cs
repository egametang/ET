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
using System.Threading;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.Servers;

namespace MongoDB.Driver.Core.Clusters.ServerSelectors
{
    /// <summary>
    /// Represents a selector that selects servers within an acceptable latency range.
    /// </summary>
    public class LatencyLimitingServerSelector : IServerSelector
    {
        // fields
        private readonly TimeSpan _allowedLatencyRange;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="LatencyLimitingServerSelector"/> class.
        /// </summary>
        public LatencyLimitingServerSelector()
            : this(TimeSpan.FromMilliseconds(15))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LatencyLimitingServerSelector"/> class.
        /// </summary>
        /// <param name="allowedLatencyRange">The allowed latency range.</param>
        public LatencyLimitingServerSelector(TimeSpan allowedLatencyRange)
        {
            _allowedLatencyRange = Ensure.IsInfiniteOrGreaterThanOrEqualToZero(allowedLatencyRange, nameof(allowedLatencyRange));
        }

        // methods
        /// <inheritdoc/>
        public IEnumerable<ServerDescription> SelectServers(ClusterDescription cluster, IEnumerable<ServerDescription> servers)
        {
            if (_allowedLatencyRange == Timeout.InfiniteTimeSpan)
            {
                return servers;
            }

            var list = servers.ToList();
            switch (list.Count)
            {
                case 0:
                case 1:
                    return list;
                default:
                    var minAverageRoundTripTime = list.Min(s => s.AverageRoundTripTime);
                    var maxAverageRoundTripTime = minAverageRoundTripTime.Add(_allowedLatencyRange);
                    return list.Where(s => s.AverageRoundTripTime <= maxAverageRoundTripTime);
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Format("LatencyLimitingServerSelector{{ AllowedLatencyRange = {0} }}", _allowedLatencyRange);
        }
    }
}

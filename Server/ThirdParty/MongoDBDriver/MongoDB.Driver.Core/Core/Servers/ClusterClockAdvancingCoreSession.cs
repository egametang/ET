/* Copyright 2017-present MongoDB Inc.
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

using MongoDB.Bson;
using MongoDB.Driver.Core.Bindings;
using MongoDB.Driver.Core.Clusters;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Core.Servers
{
    internal  sealed class ClusterClockAdvancingCoreSession : WrappingCoreSession
    {
        private readonly IClusterClock _clusterClock;

        public ClusterClockAdvancingCoreSession(ICoreSession wrapped, IClusterClock clusterClock)
            : base(wrapped, ownsWrapped: false)
        {
            _clusterClock = Ensure.IsNotNull(clusterClock, nameof(clusterClock));
        }

        public override BsonDocument ClusterTime => ClusterClock.GreaterClusterTime(base.ClusterTime, _clusterClock.ClusterTime);

        public override void AdvanceClusterTime(BsonDocument newClusterTime)
        {
            base.AdvanceClusterTime(newClusterTime);
            _clusterClock.AdvanceClusterTime(newClusterTime);
        }
    }
}

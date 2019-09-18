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
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Core.Clusters
{
    /// <summary>
    /// A cluster clock.
    /// </summary>
    /// <seealso cref="MongoDB.Driver.Core.Clusters.IClusterClock" />
    internal class ClusterClock : IClusterClock
    {
        #region static
        // public static methods
        /// <summary>
        /// Returns the greater of two cluster times.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns>The greater cluster time.</returns>
        public static BsonDocument GreaterClusterTime(BsonDocument x, BsonDocument y)
        {
            if (x == null)
            {
                return y;
            }
            else if (y == null)
            {
                return x;
            }
            else
            {
                var xTimestamp = x["clusterTime"].AsBsonTimestamp;
                var yTimestamp = y["clusterTime"].AsBsonTimestamp;
                return xTimestamp > yTimestamp ? x : y;
            }
        }
        #endregion

        // private fields
        private BsonDocument _clusterTime;

        // public properties
        /// <inheritdoc />
        public BsonDocument ClusterTime => _clusterTime;

        // public methods
        /// <inheritdoc />
        public void AdvanceClusterTime(BsonDocument newClusterTime)
        {
            Ensure.IsNotNull(newClusterTime, nameof(newClusterTime));
            _clusterTime = GreaterClusterTime(_clusterTime, newClusterTime);
        }
    }

    /// <summary>
    /// An object that represents no cluster clock.
    /// </summary>
    /// <seealso cref="MongoDB.Driver.Core.Clusters.IClusterClock" />
    internal sealed class NoClusterClock : IClusterClock
    {
        /// <inheritdoc />
        public BsonDocument ClusterTime => null;

        /// <inheritdoc />
        public void AdvanceClusterTime(BsonDocument newClusterTime)
        {
        }
    }
}

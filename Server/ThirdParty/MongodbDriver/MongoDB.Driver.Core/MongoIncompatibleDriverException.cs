/* Copyright 2013-present MongoDB Inc.
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
using System.Linq;
#if NET452
using System.Runtime.Serialization;
#endif
using MongoDB.Driver.Core.Clusters;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver
{
    /// <summary>
    /// Represents a MongoDB incompatible driver exception.
    /// </summary>
#if NET452
    [Serializable]
#endif
    public class MongoIncompatibleDriverException : MongoClientException
    {
        #region static
        // static methods
        internal static void ThrowIfNotSupported(ClusterDescription description)
        {
            var isIncompatible = description.Servers
                .Any(sd => sd.WireVersionRange != null && !sd.WireVersionRange.Overlaps(Cluster.SupportedWireVersionRange));

            if (isIncompatible)
            {
                throw new MongoIncompatibleDriverException(description);
            }
        }

        private static string FormatMessage(ClusterDescription description)
        {
            var incompatibleServer = description.Servers
                .FirstOrDefault(sd => sd.WireVersionRange != null && !sd.WireVersionRange.Overlaps(Cluster.SupportedWireVersionRange));

            if (incompatibleServer == null)
            {
                return $"This version of the driver requires wire version {Cluster.SupportedWireVersionRange}";
            }

            if (incompatibleServer.WireVersionRange.Max < Cluster.SupportedWireVersionRange.Min)
            {
                return $"Server at {EndPointHelper.ToString(incompatibleServer.EndPoint)} reports wire version {incompatibleServer.WireVersionRange.Max},"
                    + $" but this version of the driver requires at least {Cluster.SupportedWireVersionRange.Min} (MongoDB {Cluster.MinSupportedServerVersion}).";
            }

            return $"Server at {EndPointHelper.ToString(incompatibleServer.EndPoint)} requires wire version {incompatibleServer.WireVersionRange.Min},"
                + $" but this version of the driver only supports up to {Cluster.SupportedWireVersionRange.Max}.";
        }
        #endregion

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="MongoIncompatibleDriverException"/> class.
        /// </summary>
        /// <param name="clusterDescription">The cluster description.</param>
        public MongoIncompatibleDriverException(ClusterDescription clusterDescription)
            : base(FormatMessage(clusterDescription), null)
        {
        }

#if NET452
        /// <summary>
        /// Initializes a new instance of the <see cref="MongoIncompatibleDriverException"/> class.
        /// </summary>
        /// <param name="info">The SerializationInfo.</param>
        /// <param name="context">The StreamingContext.</param>
        protected MongoIncompatibleDriverException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#endif
    }
}

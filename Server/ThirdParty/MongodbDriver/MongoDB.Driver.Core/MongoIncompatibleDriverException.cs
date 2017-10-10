/* Copyright 2013-2016 MongoDB Inc.
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
#if NET45
using System.Runtime.Serialization;
#endif
using MongoDB.Driver.Core.Clusters;

namespace MongoDB.Driver
{
    /// <summary>
    /// Represents a MongoDB incompatible driver exception.
    /// </summary>
#if NET45
    [Serializable]
#endif
    public class MongoIncompatibleDriverException : MongoClientException
    {
        #region static
        // static methods
        private static string FormatMessage(ClusterDescription clusterDescription)
        {
            return string.Format(
                "This version of the driver is not compatible with one or more of the servers to which it is connected: {0}.",
                clusterDescription);
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

#if NET45
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

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
using System.Net;
#if NET45
using System.Runtime.Serialization;
#endif
using MongoDB.Driver.Core.Clusters;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver
{
    /// <summary>
    /// Represents a MongoDB connection pool wait queue full exception.
    /// </summary>
#if NET45
    [Serializable]
#endif
    public class MongoWaitQueueFullException : MongoClientException
    {
        #region static
        // static methods
        internal static MongoWaitQueueFullException ForConnectionPool(EndPoint endPoint)
        {
            var message = string.Format(
                "The wait queue for acquiring a connection to server {0} is full.",
                EndPointHelper.ToString(endPoint));
            return new MongoWaitQueueFullException(message);
        }

        internal static MongoWaitQueueFullException ForServerSelection()
        {
            var message = "The wait queue for server selection is full.";
            return new MongoWaitQueueFullException(message);
        }
        #endregion

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="MongoWaitQueueFullException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        public MongoWaitQueueFullException(string message)
            : base(message, null)
        {
        }

#if NET45
        /// <summary>
        /// Initializes a new instance of the <see cref="MongoWaitQueueFullException"/> class.
        /// </summary>
        /// <param name="info">The SerializationInfo.</param>
        /// <param name="context">The StreamingContext.</param>
        protected MongoWaitQueueFullException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#endif
    }
}

/* Copyright 2010-2016 MongoDB Inc.
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
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver
{
    /// <summary>
    /// Represents a MongoDB connection exception.
    /// </summary>
#if NET45
    [Serializable]
#endif
    public class MongoConnectionException : MongoException
    {
        // fields
        private readonly ConnectionId _connectionId;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="MongoConnectionException"/> class.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="message">The error message.</param>
        public MongoConnectionException(ConnectionId connectionId, string message)
            : this(connectionId, message, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoConnectionException"/> class.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public MongoConnectionException(ConnectionId connectionId, string message, Exception innerException)
            : base(message, innerException)
        {
            _connectionId = Ensure.IsNotNull(connectionId, nameof(connectionId));
        }

#if NET45
        /// <summary>
        /// Initializes a new instance of the <see cref="MongoConnectionException"/> class.
        /// </summary>
        /// <param name="info">The SerializationInfo.</param>
        /// <param name="context">The StreamingContext.</param>
        public MongoConnectionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _connectionId = (ConnectionId)info.GetValue("_connectionId", typeof(ConnectionId));
        }
#endif

        // properties
        /// <summary>
        /// Gets the connection identifier.
        /// </summary>
        public ConnectionId ConnectionId
        {
            get { return _connectionId; }
        }

        // methods
#if NET45
        /// <inheritdoc/>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("_connectionId", _connectionId);
        }
#endif
    }
}

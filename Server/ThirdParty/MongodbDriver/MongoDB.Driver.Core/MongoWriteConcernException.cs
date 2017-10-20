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
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.Operations;

namespace MongoDB.Driver
{
    /// <summary>
    /// Represents a MongoDB write concern exception.
    /// </summary>
#if NET45
    [Serializable]
#endif
    public class MongoWriteConcernException : MongoCommandException
    {
        // fields
        private readonly WriteConcernResult _writeConcernResult;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="MongoWriteConcernException"/> class.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="message">The error message.</param>
        /// <param name="writeConcernResult">The command result.</param>
        public MongoWriteConcernException(ConnectionId connectionId, string message, WriteConcernResult writeConcernResult)
            : base(connectionId, message, null, writeConcernResult.Response)
        {
            _writeConcernResult = Ensure.IsNotNull(writeConcernResult, nameof(writeConcernResult));
        }

#if NET45
        /// <summary>
        /// Initializes a new instance of the <see cref="MongoWriteConcernException"/> class.
        /// </summary>
        /// <param name="info">The SerializationInfo.</param>
        /// <param name="context">The StreamingContext.</param>
        public MongoWriteConcernException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _writeConcernResult = (WriteConcernResult)info.GetValue("_writeConcernResult", typeof(WriteConcernResult));
        }
#endif

        // properties
        /// <summary>
        /// Gets the write concern result.
        /// </summary>
        /// <value>
        /// The write concern result.
        /// </value>
        public WriteConcernResult WriteConcernResult
        {
            get { return _writeConcernResult; }
        }

        // methods
#if NET45
        /// <inheritdoc/>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("_writeConcernResult", _writeConcernResult);
        }
#endif
    }
}

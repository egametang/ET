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
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Core.Connections;

namespace MongoDB.Driver
{
    /// <summary>
    /// Represents a MongoDB query exception.
    /// </summary>
#if NET45
    [Serializable]
#endif
    public class MongoQueryException : MongoServerException
    {
        // fields
        private readonly BsonDocument _query;
        private readonly BsonDocument _queryResult;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="MongoQueryException"/> class.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="message">The message.</param>
        /// <param name="query">The query.</param>
        /// <param name="queryResult">The query result.</param>
        public MongoQueryException(ConnectionId connectionId, string message, BsonDocument query, BsonDocument queryResult)
            : base(connectionId, message, null)
        {
            _query = query;
            _queryResult = queryResult;
        }

#if NET45
        /// <summary>
        /// Initializes a new instance of the <see cref="MongoQueryException"/> class.
        /// </summary>
        /// <param name="info">The SerializationInfo.</param>
        /// <param name="context">The StreamingContext.</param>
        protected MongoQueryException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _query = (BsonDocument)info.GetValue("_query", typeof(BsonDocument));
            _queryResult = (BsonDocument)info.GetValue("_queryResult", typeof(BsonDocument));
        }
#endif

        // properties
        /// <summary>
        /// Gets the query.
        /// </summary>
        /// <value>
        /// The query.
        /// </value>
        public BsonDocument Query
        {
            get { return _query; }
        }

        /// <summary>
        /// Gets the query result.
        /// </summary>
        /// <value>
        /// The query result.
        /// </value>
        public BsonDocument QueryResult
        {
            get { return _queryResult; }
        }

        // methods
#if NET45
        /// <inheritdoc/>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("_query", _query);
            info.AddValue("_queryResult", _queryResult);
        }
#endif
    }
}

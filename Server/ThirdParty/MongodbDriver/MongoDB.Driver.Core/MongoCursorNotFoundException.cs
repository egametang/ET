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
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver
{
    /// <summary>
    /// Represents a MongoDB cursor not found exception.
    /// </summary>
#if NET45
    [Serializable]
#endif
    public class MongoCursorNotFoundException : MongoQueryException
    {
        #region static
        // static methods
        private static string FormatMessage(ConnectionId connectionId, long cursorId)
        {
            return string.Format(
                "Cursor {0} not found on server {1} using connection {2}.",
                cursorId,
                EndPointHelper.ToString(connectionId.ServerId.EndPoint),
                connectionId.ServerValue);
        }
        #endregion

        // fields
        private readonly long _cursorId;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="MongoCursorNotFoundException"/> class.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="cursorId">The cursor identifier.</param>
        /// <param name="query">The query.</param>
        public MongoCursorNotFoundException(ConnectionId connectionId, long cursorId, BsonDocument query)
            : base(connectionId, FormatMessage(connectionId, cursorId), query, null)
        {
            _cursorId = cursorId;
        }

#if NET45
        /// <summary>
        /// Initializes a new instance of the <see cref="MongoCursorNotFoundException"/> class.
        /// </summary>
        /// <param name="info">The information.</param>
        /// <param name="context">The context.</param>
        protected MongoCursorNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _cursorId = info.GetInt64("_cursorId");
        }
#endif

        // properties
        /// <summary>
        /// Gets the cursor identifier.
        /// </summary>
        /// <value>
        /// The cursor identifier.
        /// </value>
        public long CursorId
        {
            get { return _cursorId; }
        }

        // methods
#if NET45
        /// <inheritdoc/>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("_cursorId", _cursorId);
        }
#endif
    }
}

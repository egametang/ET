/* Copyright 2015-present MongoDB Inc.
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
#if NET452
using System.Runtime.Serialization;
#endif
using MongoDB.Bson;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.GridFS
{
    /// <summary>
    /// Represents a GridFSChunk exception.
    /// </summary>
#if NET452
    [Serializable]
#endif
    public class GridFSChunkException : GridFSException
    {
#region static
        private static string FormatMessage(BsonValue id, long n, string reason)
        {
            Ensure.IsNotNull(id, nameof(id));
            Ensure.IsGreaterThanOrEqualToZero(n, nameof(n));
            Ensure.IsNotNull(reason, nameof(reason));
            return string.Format("GridFS chunk {0} of file id {1} is {2}.", n, id, reason);
        }
#endregion

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="GridFSChunkException" /> class.
        /// </summary>
        /// <param name="id">The file id.</param>
        /// <param name="n">The chunk number.</param>
        /// <param name="reason">The reason.</param>
        public GridFSChunkException(BsonValue id, long n, string reason)
            : base(FormatMessage(id, n, reason))
        {
        }

#if NET452
        /// <summary>
        /// Initializes a new instance of the <see cref="GridFSChunkException"/> class.
        /// </summary>
        /// <param name="info">The SerializationInfo.</param>
        /// <param name="context">The StreamingContext.</param>
        public GridFSChunkException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#endif
    }
}

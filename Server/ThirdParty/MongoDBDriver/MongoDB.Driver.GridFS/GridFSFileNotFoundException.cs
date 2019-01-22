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
    /// Represents a GridFSFileNotFound exception.
    /// </summary>
#if NET452
    [Serializable]
#endif
    public class GridFSFileNotFoundException : GridFSException
    {
#region static
        private static string FormatMessage(BsonValue id)
        {
            Ensure.IsNotNull(id, nameof(id));
            return string.Format("GridFS file not found: file id {0}.", id);
        }

        private static string FormatMessage(string filename, int revision)
        {
            Ensure.IsNotNull(filename, nameof(filename));
            return string.Format("GridFS file not found: revision {0} of filename \"{1}\".", revision, filename);
        }
#endregion

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="GridFSFileNotFoundException"/> class.
        /// </summary>
        /// <param name="id">The file id.</param>
        public GridFSFileNotFoundException(BsonValue id)
            : base(FormatMessage(id))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GridFSFileNotFoundException" /> class.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="revision">The revision.</param>
        public GridFSFileNotFoundException(string filename, int revision)
            : base(FormatMessage(filename, revision))
        {
        }

#if NET452
        /// <summary>
        /// Initializes a new instance of the <see cref="GridFSFileNotFoundException"/> class.
        /// </summary>
        /// <param name="info">The SerializationInfo.</param>
        /// <param name="context">The StreamingContext.</param>
        public GridFSFileNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#endif
    }
}

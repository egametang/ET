/* Copyright 2013-2015 MongoDB Inc.
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Core.WireProtocol
{
    /// <summary>
    /// Represents one result batch (returned from either a Query or a GetMore message)
    /// </summary>
    /// <typeparam name="TDocument">The type of the document.</typeparam>
    public struct CursorBatch<TDocument>
    {
        // fields
        private readonly long _cursorId;
        private readonly IReadOnlyList<TDocument> _documents;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="CursorBatch{TDocument}"/> struct.
        /// </summary>
        /// <param name="cursorId">The cursor identifier.</param>
        /// <param name="documents">The documents.</param>
        public CursorBatch(
            long cursorId,
            IReadOnlyList<TDocument> documents)
        {
            _cursorId = cursorId;
            _documents = Ensure.IsNotNull(documents, nameof(documents));
        }

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

        /// <summary>
        /// Gets the documents.
        /// </summary>
        /// <value>
        /// The documents.
        /// </value>
        public IReadOnlyList<TDocument> Documents
        {
            get { return _documents; }
        }
    }
}

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

using MongoDB.Bson;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Core.Operations
{
    /// <summary>
    /// Represents a request to delete one or more documents.
    /// </summary>
    public sealed class DeleteRequest : WriteRequest
    {
        // fields
        private Collation _collation;
        private readonly BsonDocument _filter;
        private int _limit;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteRequest" /> class.
        /// </summary>
        /// <param name="filter">The filter.</param>
        public DeleteRequest(BsonDocument filter)
            : base(WriteRequestType.Delete)
        {
            _filter = Ensure.IsNotNull(filter, nameof(filter));
            _limit = 1;
        }

        // properties
        /// <summary>
        /// Gets or sets the collation.
        /// </summary>
        public Collation Collation
        {
            get { return _collation; }
            set { _collation = value; }
        }

        /// <summary>
        /// Gets or sets the filter.
        /// </summary>
        public BsonDocument Filter
        {
            get { return _filter; }
        }

        /// <summary>
        /// Gets or sets a limit on the number of documents that should be deleted.
        /// </summary>
        /// <remarks>
        /// The server only supports 0 or 1, and 0 means that all matching documents should be deleted.
        /// </remarks>
        /// <value>
        /// A limit on the number of documents that should be deleted.
        /// </value>
        public int Limit
        {
            get { return _limit; }
            set { _limit = value; }
        }
    }
}

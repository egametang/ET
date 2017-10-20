/* Copyright 2010-2017 MongoDB Inc.
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
using MongoDB.Bson;
using MongoDB.Driver.Core.Misc;
using System.Collections.Generic;

namespace MongoDB.Driver.Core.Operations
{
    /// <summary>
    /// Represents a request to update one or more documents.
    /// </summary>
    public sealed class UpdateRequest : WriteRequest
    {
        // fields
        private IEnumerable<BsonDocument> _arrayFilters;
        private Collation _collation;
        private readonly BsonDocument _filter;
        private bool _isMulti;
        private bool _isUpsert;
        private readonly BsonDocument _update;
        private UpdateType _updateType;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateRequest" /> class.
        /// </summary>
        /// <param name="updateType">The update type.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="update">The update.</param>
        public UpdateRequest(UpdateType updateType, BsonDocument filter, BsonDocument update)
            : base(WriteRequestType.Update)
        {
            _updateType = updateType;
            _filter = Ensure.IsNotNull(filter, nameof(filter));
            _update = Ensure.IsNotNull(update, nameof(update));
            if (updateType == UpdateType.Update && _update.ElementCount == 0)
            {
                throw new ArgumentException("Updates must have at least 1 update operator.", nameof(update));
            }
        }

        // properties
        /// <summary>
        /// Gets or sets the array filters.
        /// </summary>
        /// <value>
        /// The array filters.
        /// </value>
        public IEnumerable<BsonDocument> ArrayFilters
        {
            get { return _arrayFilters; }
            set { _arrayFilters = value; }
        }

        /// <summary>
        /// Gets or sets the collation.
        /// </summary>
        public Collation Collation
        {
            get { return _collation; }
            set { _collation = value; }
        }

        /// <summary>
        /// Gets the filter.
        /// </summary>
        public BsonDocument Filter
        {
            get { return _filter; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this update should affect all matching documents.
        /// </summary>
        /// <value>
        /// <c>true</c> if this update should affect all matching documents; otherwise, <c>false</c>.
        /// </value>
        public bool IsMulti
        {
            get { return _isMulti; }
            set { _isMulti = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether a document should be inserted if no matching document is found.
        /// </summary>
        /// <value>
        ///   <c>true</c> if a document should be inserted if no matching document is found; otherwise, <c>false</c>.
        /// </value>
        public bool IsUpsert
        {
            get { return _isUpsert; }
            set { _isUpsert = value; }
        }

        /// <summary>
        /// Gets the update specification.
        /// </summary>
        public BsonDocument Update
        {
            get { return _update; }
        }

        /// <summary>
        /// Gets the update type.
        /// </summary>
        public UpdateType UpdateType
        {
            get { return _updateType; }
        }
    }
}

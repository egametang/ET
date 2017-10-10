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
using System.Collections.Generic;
using System.Linq;

namespace MongoDB.Driver
{
    /// <summary>
    /// Options for a findAndModify command to update an object.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document.</typeparam>
    /// <typeparam name="TProjection">The type of the projection (same as TDocument if there is no projection).</typeparam>
    public class FindOneAndUpdateOptions<TDocument, TProjection>
    {
        // fields
        private IEnumerable<ArrayFilterDefinition> _arrayFilters;
        private bool? _bypassDocumentValidation;
        private Collation _collation;
        private bool _isUpsert;
        private TimeSpan? _maxTime;
        private ProjectionDefinition<TDocument, TProjection> _projection;
        private ReturnDocument _returnDocument;
        private SortDefinition<TDocument> _sort;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="FindOneAndUpdateOptions{TDocument, TProjection}"/> class.
        /// </summary>
        public FindOneAndUpdateOptions()
        {
            _returnDocument = ReturnDocument.Before;
        }

        // properties
        /// <summary>
        /// Gets or sets the array filters.
        /// </summary>
        /// <value>
        /// The array filters.
        /// </value>
        public IEnumerable<ArrayFilterDefinition> ArrayFilters
        {
            get { return _arrayFilters; }
            set { _arrayFilters = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to bypass document validation.
        /// </summary>
        public bool? BypassDocumentValidation
        {
            get { return _bypassDocumentValidation; }
            set { _bypassDocumentValidation = value; }
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
        /// Gets or sets a value indicating whether to insert the document if it doesn't already exist.
        /// </summary>
        public bool IsUpsert
        {
            get { return _isUpsert; }
            set { _isUpsert = value; }
        }

        /// <summary>
        /// Gets or sets the maximum time.
        /// </summary>
        public TimeSpan? MaxTime
        {
            get { return _maxTime; }
            set { _maxTime = value; }
        }

        /// <summary>
        /// Gets or sets the projection.
        /// </summary>
        public ProjectionDefinition<TDocument, TProjection> Projection
        {
            get { return _projection; }
            set { _projection = value; }
        }

        /// <summary>
        /// Gets or sets which version of the document to return.
        /// </summary>
        public ReturnDocument ReturnDocument
        {
            get { return _returnDocument; }
            set { _returnDocument = value; }
        }

        /// <summary>
        /// Gets or sets the sort.
        /// </summary>
        public SortDefinition<TDocument> Sort
        {
            get { return _sort; }
            set { _sort = value; }
        }
    }

    /// <summary>
    /// Options for a findAndModify command to update an object.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document and the result.</typeparam>
    public class FindOneAndUpdateOptions<TDocument> : FindOneAndUpdateOptions<TDocument, TDocument>
    {
    }
}

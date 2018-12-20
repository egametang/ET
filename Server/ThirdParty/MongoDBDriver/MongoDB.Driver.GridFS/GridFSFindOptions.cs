/* Copyright 2016-present MongoDB Inc.
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
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.GridFS
{
    /// <summary>
    /// Represents options for a GridFS Find operation.
    /// </summary>
    /// <typeparam name="TFileId">The type of the file identifier.</typeparam>
    public class GridFSFindOptions<TFileId>
    {
        // fields
        private int? _batchSize;
        private int? _limit;
        private TimeSpan? _maxTime;
        private bool? _noCursorTimeout;
        private int? _skip;
        private SortDefinition<GridFSFileInfo<TFileId>> _sort;

        // properties
        /// <summary>
        /// Gets or sets the batch size.
        /// </summary>
        /// <value>
        /// The batch size.
        /// </value>
        public int? BatchSize
        {
            get { return _batchSize; }
            set { _batchSize = Ensure.IsNullOrGreaterThanZero(value, nameof(value)); }
        }

        /// <summary>
        /// Gets or sets the maximum number of documents to return.
        /// </summary>
        /// <value>
        /// The maximum number of documents to return.
        /// </value>
        public int? Limit
        {
            get { return _limit; }
            set { _limit = value; }
        }

        /// <summary>
        /// Gets or sets the maximum time the server should spend on the Find.
        /// </summary>
        /// <value>
        /// The maximum time the server should spend on the Find.
        /// </value>
        public TimeSpan? MaxTime
        {
            get { return _maxTime; }
            set { _maxTime = Ensure.IsNullOrInfiniteOrGreaterThanOrEqualToZero(value, nameof(value)); }
        }

        /// <summary>
        /// Gets or sets whether the cursor should not timeout.
        /// </summary>
        /// <value>
        /// Whether the cursor should not timeout.
        /// </value>
        public bool? NoCursorTimeout
        {
            get { return _noCursorTimeout; }
            set { _noCursorTimeout = value; }
        }

        /// <summary>
        /// Gets or sets the number of documents to skip.
        /// </summary>
        /// <value>
        /// The number of documents to skip.
        /// </value>
        public int? Skip
        {
            get { return _skip; }
            set { _skip = Ensure.IsNullOrGreaterThanOrEqualToZero(value, nameof(value)); }
        }

        /// <summary>
        /// Gets or sets the sort order.
        /// </summary>
        /// <value>
        /// The sort order.
        /// </value>
        public SortDefinition<GridFSFileInfo<TFileId>> Sort
        {
            get { return _sort; }
            set { _sort = value; }
        }
    }
}

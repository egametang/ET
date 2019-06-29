/* Copyright 2017-present MongoDB Inc.
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

namespace MongoDB.Driver
{
    /// <summary>
    /// Options for a list databases operation.
    /// </summary>
    public sealed class ListDatabasesOptions
    {
        // fields
        private FilterDefinition<BsonDocument> _filter;
        private bool? _nameOnly;

        // properties
        /// <summary>
        /// Gets or sets the filter.
        /// </summary>
        public FilterDefinition<BsonDocument> Filter
        {
            get { return _filter; }
            set { _filter = value; }
        }

        /// <summary>
        /// Gets or sets the NameOnly flag.
        /// </summary>
        public bool? NameOnly
        {
            get { return _nameOnly; }
            set { _nameOnly = value; }
        }
    }
}

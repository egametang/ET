/* Copyright 2010-2015 MongoDB Inc.
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
    /// Options for a list collections operation.
    /// </summary>
    public sealed class ListCollectionsOptions
    {
        // fields
        private FilterDefinition<BsonDocument> _filter;

        // properties
        /// <summary>
        /// Gets or sets the filter.
        /// </summary>
        public FilterDefinition<BsonDocument> Filter
        {
            get { return _filter; }
            set { _filter = value; }
        }
    }
}

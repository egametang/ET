/* Copyright 2016 MongoDB Inc.
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

using MongoDB.Bson.Serialization.Attributes;

namespace MongoDB.Driver
{
    /// <summary>
    /// Result type for the aggregate $sortByCount stage.
    /// </summary>
    /// <typeparam name="TId">The type of the identifier.</typeparam>
    public sealed class AggregateSortByCountResult<TId>
    {
        private long _count;
        private TId _id;

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateCountResult" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="count">The count.</param>
        public AggregateSortByCountResult(TId id, long count)
        {
            _id = id;
            _count = count;
        }

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        [BsonElement("count")]
        public long Count
        {
            get { return _count; }
        }

        /// <summary>
        /// Gets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        [BsonElement("_id")]
        public TId Id
        {
            get { return _id; }
        }
    }
}

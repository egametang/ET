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
    /// Represents the result of the $bucket stage.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    public class AggregateBucketResult<TValue>
    {
        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateBucketResult{TValue}"/> class.
        /// </summary>
        /// <param name="id">The inclusive lower boundary of the bucket.</param>
        /// <param name="count">The count.</param>
        public AggregateBucketResult(TValue id, long count)
        {
            Id = id;
            Count = count;
        }

        // public properties
        /// <summary>
        /// Gets the inclusive lower boundary of the bucket.
        /// </summary>
        /// <value>
        /// The inclusive lower boundary of the bucket.
        /// </value>
        [BsonId]
        public TValue Id { get; private set; }

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        [BsonElement("count")]
        public long Count { get; private set; }
    }
}

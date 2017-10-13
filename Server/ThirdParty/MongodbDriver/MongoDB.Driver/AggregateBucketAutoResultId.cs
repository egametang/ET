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
    /// Represents the _id value in the result of a $bucketAuto stage.
    /// </summary>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    public class AggregateBucketAutoResultId<TValue>
    {
        private readonly TValue _min;
        private readonly TValue _max;

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateBucketAutoResultId{TValue}"/> class.
        /// </summary>
        /// <param name="min">The minimum.</param>
        /// <param name="max">The maximum.</param>
        public AggregateBucketAutoResultId(TValue min, TValue max)
        {
            _min = min;
            _max = max;
        }

        /// <summary>
        /// Gets the max value.
        /// </summary>
        [BsonElement("max")]
        public TValue Max => _max;

        /// <summary>
        /// Gets the min value.
        /// </summary>
        [BsonElement("min")]
        public TValue Min => _min;
    }
}

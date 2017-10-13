/* Copyright 2015 MongoDB Inc.
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

using MongoDB.Bson.Serialization;

namespace MongoDB.Driver
{
    /// <summary>
    /// Options for the $unwind aggregation stage.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    public class AggregateUnwindOptions<TResult>
    {
        private FieldDefinition<TResult> _includeArrayIndex;
        private bool? _preserveNullAndEmptyArrays;
        private IBsonSerializer<TResult> _resultSerializer;

        /// <summary>
        /// Gets or sets the field with which to include the array index.
        /// </summary>
        public FieldDefinition<TResult> IncludeArrayIndex
        {
            get { return _includeArrayIndex; }
            set { _includeArrayIndex = value; }
        }

        /// <summary>
        /// Gets or sets whether to preserve null and empty arrays.
        /// </summary>
        public bool? PreserveNullAndEmptyArrays
        {
            get { return _preserveNullAndEmptyArrays; }
            set { _preserveNullAndEmptyArrays = value; }
        }

        /// <summary>
        /// Gets or sets the result serializer.
        /// </summary>
        public IBsonSerializer<TResult> ResultSerializer
        {
            get { return _resultSerializer; }
            set { _resultSerializer = value; }
        }
    }
}

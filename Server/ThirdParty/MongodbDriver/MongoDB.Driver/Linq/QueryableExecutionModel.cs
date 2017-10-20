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

using System;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization;

namespace MongoDB.Driver.Linq
{
    /// <summary>
    /// An execution model.
    /// </summary>
    public abstract class QueryableExecutionModel
    {
        /// <summary>
        /// Gets the type of the output.
        /// </summary>
        public abstract Type OutputType { get; }

        // prevent external inheritance
        internal QueryableExecutionModel()
        {
        }

        internal abstract Task ExecuteAsync<TInput>(IMongoCollection<TInput> collection, AggregateOptions options, CancellationToken cancellationToken);

        internal abstract object Execute<TInput>(IMongoCollection<TInput> collection, AggregateOptions options);
    }
}

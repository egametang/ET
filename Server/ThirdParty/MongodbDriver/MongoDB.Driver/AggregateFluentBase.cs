/* Copyright 2010-2016 MongoDB Inc.
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
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace MongoDB.Driver
{
    /// <summary>
    /// Base class for implementors of <see cref="IAggregateFluent{TResult}" />.
    /// </summary>
    /// <typeparam name="TResult">The type of the document.</typeparam>
    public abstract class AggregateFluentBase<TResult> : IOrderedAggregateFluent<TResult>
    {
        /// <inheritdoc />
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public virtual IMongoDatabase Database
        {
            get { throw new NotImplementedException(); }
        }

        /// <inheritdoc />
        public abstract AggregateOptions Options { get; }

        /// <inheritdoc />
        public abstract IList<IPipelineStageDefinition> Stages { get; }

        /// <inheritdoc />
        public abstract IAggregateFluent<TNewResult> AppendStage<TNewResult>(PipelineStageDefinition<TResult, TNewResult> stage);

        /// <inheritdoc />
        public abstract IAggregateFluent<TNewResult> As<TNewResult>(IBsonSerializer<TNewResult> newResultSerializer);

        /// <inheritdoc />
        public virtual IAggregateFluent<AggregateBucketResult<TValue>> Bucket<TValue>(
            AggregateExpressionDefinition<TResult, TValue> groupBy,
            IEnumerable<TValue> boundaries,
            AggregateBucketOptions<TValue> options = null)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public virtual IAggregateFluent<TNewResult> Bucket<TValue, TNewResult>(
            AggregateExpressionDefinition<TResult, TValue> groupBy,
            IEnumerable<TValue> boundaries,
            ProjectionDefinition<TResult, TNewResult> output,
            AggregateBucketOptions<TValue> options = null)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public virtual IAggregateFluent<AggregateBucketAutoResult<TValue>> BucketAuto<TValue>(
            AggregateExpressionDefinition<TResult, TValue> groupBy,
            int buckets,
            AggregateBucketAutoOptions options = null)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public virtual IAggregateFluent<TNewResult> BucketAuto<TValue, TNewResult>(
            AggregateExpressionDefinition<TResult, TValue> groupBy,
            int buckets,
            ProjectionDefinition<TResult, TNewResult> output,
            AggregateBucketAutoOptions options = null)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public virtual IAggregateFluent<AggregateCountResult> Count()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public virtual IAggregateFluent<TNewResult> Facet<TNewResult>(
            IEnumerable<AggregateFacet<TResult>> facets,
            AggregateFacetOptions<TNewResult> options = null)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public virtual IAggregateFluent<TNewResult> GraphLookup<TFrom, TConnectFrom, TConnectTo, TStartWith, TAsElement, TAs, TNewResult>(
            IMongoCollection<TFrom> from,
            FieldDefinition<TFrom, TConnectFrom> connectFromField,
            FieldDefinition<TFrom, TConnectTo> connectToField,
            AggregateExpressionDefinition<TResult, TStartWith> startWith,
            FieldDefinition<TNewResult, TAs> @as,
            FieldDefinition<TAsElement, int> depthField,
            AggregateGraphLookupOptions<TFrom, TAsElement, TNewResult> options = null)
                where TAs : IEnumerable<TAsElement>
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public abstract IAggregateFluent<TNewResult> Group<TNewResult>(ProjectionDefinition<TResult, TNewResult> group);

        /// <inheritdoc />
        public abstract IAggregateFluent<TResult> Limit(int limit);

        /// <inheritdoc />
        public virtual IAggregateFluent<TNewResult> Lookup<TForeignDocument, TNewResult>(string foreignCollectionName, FieldDefinition<TResult> localField, FieldDefinition<TForeignDocument> foreignField, FieldDefinition<TNewResult> @as, AggregateLookupOptions<TForeignDocument, TNewResult> options)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public abstract IAggregateFluent<TResult> Match(FilterDefinition<TResult> filter);

        /// <inheritdoc />
        public abstract IAggregateFluent<TNewResult> OfType<TNewResult>(IBsonSerializer<TNewResult> newResultSerializer) where TNewResult : TResult;

        /// <inheritdoc />
        public virtual IAsyncCursor<TResult> Out(string collectionName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public abstract Task<IAsyncCursor<TResult>> OutAsync(string collectionName, CancellationToken cancellationToken);

        /// <inheritdoc />
        public abstract IAggregateFluent<TNewResult> Project<TNewResult>(ProjectionDefinition<TResult, TNewResult> projection);

        /// <inheritdoc />
        public virtual IAggregateFluent<TNewResult> ReplaceRoot<TNewResult>(AggregateExpressionDefinition<TResult, TNewResult> newRoot)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public abstract IAggregateFluent<TResult> Skip(int skip);

        /// <inheritdoc />
        public abstract IAggregateFluent<TResult> Sort(SortDefinition<TResult> sort);

        /// <inheritdoc />
        public virtual IAggregateFluent<AggregateSortByCountResult<TId>> SortByCount<TId>(AggregateExpressionDefinition<TResult, TId> id)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public virtual IOrderedAggregateFluent<TResult> ThenBy(SortDefinition<TResult> newSort)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public abstract IAggregateFluent<TNewResult> Unwind<TNewResult>(FieldDefinition<TResult> field, IBsonSerializer<TNewResult> newResultSerializer);

        /// <inheritdoc />
        public virtual IAggregateFluent<TNewResult> Unwind<TNewResult>(FieldDefinition<TResult> field, AggregateUnwindOptions<TNewResult> options)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public virtual IAsyncCursor<TResult> ToCursor(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public abstract Task<IAsyncCursor<TResult>> ToCursorAsync(CancellationToken cancellationToken);
    }
}

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
    /// Fluent interface for aggregate.
    /// </summary>
    /// <remarks>
    /// This interface is not guaranteed to remain stable. Implementors should use
    /// <see cref="AggregateFluentBase{TResult}" />.
    /// </remarks>
    /// <typeparam name="TResult">The type of the result of the pipeline.</typeparam>
    public interface IAggregateFluent<TResult> : IAsyncCursorSource<TResult>
    {
        /// <summary>
        /// Gets the database.
        /// </summary>
        IMongoDatabase Database { get; }

        /// <summary>
        /// Gets the options.
        /// </summary>
        AggregateOptions Options { get; }

        /// <summary>
        /// Gets the stages.
        /// </summary>
        IList<IPipelineStageDefinition> Stages { get; }

        /// <summary>
        /// Appends the stage to the pipeline.
        /// </summary>
        /// <typeparam name="TNewResult">The type of the result of the stage.</typeparam>
        /// <param name="stage">The stage.</param>
        /// <returns>The fluent aggregate interface.</returns>
        IAggregateFluent<TNewResult> AppendStage<TNewResult>(PipelineStageDefinition<TResult, TNewResult> stage);

        /// <summary>
        /// Changes the result type of the pipeline.
        /// </summary>
        /// <typeparam name="TNewResult">The type of the new result.</typeparam>
        /// <param name="newResultSerializer">The new result serializer.</param>
        /// <returns>The fluent aggregate interface.</returns>
        IAggregateFluent<TNewResult> As<TNewResult>(IBsonSerializer<TNewResult> newResultSerializer = null);

        /// <summary>
        /// Appends a $bucket stage to the pipeline.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="groupBy">The expression providing the value to group by.</param>
        /// <param name="boundaries">The bucket boundaries.</param>
        /// <param name="options">The options.</param>
        /// <returns>The fluent aggregate interface.</returns>
        IAggregateFluent<AggregateBucketResult<TValue>> Bucket<TValue>(
            AggregateExpressionDefinition<TResult, TValue> groupBy,
            IEnumerable<TValue> boundaries,
            AggregateBucketOptions<TValue> options = null);

        /// <summary>
        /// Appends a $bucket stage to the pipeline with a custom projection.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <typeparam name="TNewResult">The type of the new result.</typeparam>
        /// <param name="groupBy">The expression providing the value to group by.</param>
        /// <param name="boundaries">The bucket boundaries.</param>
        /// <param name="output">The output projection.</param>
        /// <param name="options">The options.</param>
        /// <returns>The fluent aggregate interface.</returns>
        IAggregateFluent<TNewResult> Bucket<TValue, TNewResult>(
            AggregateExpressionDefinition<TResult, TValue> groupBy,
            IEnumerable<TValue> boundaries,
            ProjectionDefinition<TResult, TNewResult> output,
            AggregateBucketOptions<TValue> options = null);

        /// <summary>
        /// Appends a $bucketAuto stage to the pipeline.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="groupBy">The expression providing the value to group by.</param>
        /// <param name="buckets">The number of buckets.</param>
        /// <param name="options">The options (optional).</param>
        /// <returns>The fluent aggregate interface.</returns>
        IAggregateFluent<AggregateBucketAutoResult<TValue>> BucketAuto<TValue>(
            AggregateExpressionDefinition<TResult, TValue> groupBy,
            int buckets,
            AggregateBucketAutoOptions options = null);

        /// <summary>
        /// Appends a $bucketAuto stage to the pipeline with a custom projection.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <typeparam name="TNewResult">The type of the new result.</typeparam>
        /// <param name="groupBy">The expression providing the value to group by.</param>
        /// <param name="buckets">The number of buckets.</param>
        /// <param name="output">The output projection.</param>
        /// <param name="options">The options (optional).</param>
        /// <returns>The fluent aggregate interface.</returns>
        IAggregateFluent<TNewResult> BucketAuto<TValue, TNewResult>(
            AggregateExpressionDefinition<TResult, TValue> groupBy,
            int buckets,
            ProjectionDefinition<TResult, TNewResult> output,
            AggregateBucketAutoOptions options = null);

        /// <summary>
        /// Appends a count stage to the pipeline.
        /// </summary>
        /// <returns>The fluent aggregate interface.</returns>
        IAggregateFluent<AggregateCountResult> Count();

        /// <summary>
        /// Appends a $facet stage to the pipeline.
        /// </summary>
        /// <typeparam name="TNewResult">The type of the new result.</typeparam>
        /// <param name="facets">The facets.</param>
        /// <param name="options">The options.</param>
        /// <returns>
        /// The fluent aggregate interface.
        /// </returns>
        IAggregateFluent<TNewResult> Facet<TNewResult>(
            IEnumerable<AggregateFacet<TResult>> facets,
            AggregateFacetOptions<TNewResult> options = null);

        /// <summary>
        /// Appends a $graphLookup stage to the pipeline.
        /// </summary>
        /// <typeparam name="TFrom">The type of the from documents.</typeparam>
        /// <typeparam name="TConnectFrom">The type of the connect from field (must be either TConnectTo or a type that implements IEnumerable{TConnectTo}).</typeparam>
        /// <typeparam name="TConnectTo">The type of the connect to field.</typeparam>
        /// <typeparam name="TStartWith">The type of the start with expression (must be either TConnectTo or a type that implements IEnumerable{TConnectTo}).</typeparam>
        /// <typeparam name="TAsElement">The type of the as field elements.</typeparam>
        /// <typeparam name="TAs">The type of the as field.</typeparam>
        /// <typeparam name="TNewResult">The type of the new result (must be same as TResult with an additional as field).</typeparam>
        /// <param name="from">The from collection.</param>
        /// <param name="connectFromField">The connect from field.</param>
        /// <param name="connectToField">The connect to field.</param>
        /// <param name="startWith">The start with value.</param>
        /// <param name="as">The as field.</param>
        /// <param name="depthField">The depth field.</param>
        /// <param name="options">The options.</param>
        /// <returns>The fluent aggregate interface.</returns>
        IAggregateFluent<TNewResult> GraphLookup<TFrom, TConnectFrom, TConnectTo, TStartWith, TAsElement, TAs, TNewResult>(
            IMongoCollection<TFrom> from,
            FieldDefinition<TFrom, TConnectFrom> connectFromField,
            FieldDefinition<TFrom, TConnectTo> connectToField,
            AggregateExpressionDefinition<TResult, TStartWith> startWith,
            FieldDefinition<TNewResult, TAs> @as,
            FieldDefinition<TAsElement, int> depthField,
            AggregateGraphLookupOptions<TFrom, TAsElement, TNewResult> options = null)
                where TAs : IEnumerable<TAsElement>;

        /// <summary>
        /// Appends a group stage to the pipeline.
        /// </summary>
        /// <typeparam name="TNewResult">The type of the result of the stage.</typeparam>
        /// <param name="group">The group projection.</param>
        /// <returns>The fluent aggregate interface.</returns>
        IAggregateFluent<TNewResult> Group<TNewResult>(ProjectionDefinition<TResult, TNewResult> group);

        /// <summary>
        /// Appends a limit stage to the pipeline.
        /// </summary>
        /// <param name="limit">The limit.</param>
        /// <returns>The fluent aggregate interface.</returns>
        IAggregateFluent<TResult> Limit(int limit);

        /// <summary>
        /// Appends a lookup stage to the pipeline.
        /// </summary>
        /// <typeparam name="TForeignDocument">The type of the foreign document.</typeparam>
        /// <typeparam name="TNewResult">The type of the new result.</typeparam>
        /// <param name="foreignCollectionName">Name of the other collection.</param>
        /// <param name="localField">The local field.</param>
        /// <param name="foreignField">The foreign field.</param>
        /// <param name="as">The field in <typeparamref name="TNewResult" /> to place the foreign results.</param>
        /// <param name="options">The options.</param>
        /// <returns>The fluent aggregate interface.</returns>
        IAggregateFluent<TNewResult> Lookup<TForeignDocument, TNewResult>(string foreignCollectionName, FieldDefinition<TResult> localField, FieldDefinition<TForeignDocument> foreignField, FieldDefinition<TNewResult> @as, AggregateLookupOptions<TForeignDocument, TNewResult> options = null);

        /// <summary>
        /// Appends a match stage to the pipeline.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <returns>The fluent aggregate interface.</returns>
        IAggregateFluent<TResult> Match(FilterDefinition<TResult> filter);

        /// <summary>
        /// Appends a match stage to the pipeline that matches derived documents and changes the result type to the derived type.
        /// </summary>
        /// <typeparam name="TNewResult">The type of the derived documents.</typeparam>
        /// <param name="newResultSerializer">The new result serializer.</param>
        /// <returns>The fluent aggregate interface.</returns>
        IAggregateFluent<TNewResult> OfType<TNewResult>(IBsonSerializer<TNewResult> newResultSerializer = null) where TNewResult : TResult;

        /// <summary>
        /// Appends an out stage to the pipeline and executes it, and then returns a cursor to read the contents of the output collection.
        /// </summary>
        /// <param name="collectionName">Name of the collection.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A cursor.</returns>
        IAsyncCursor<TResult> Out(string collectionName, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Appends an out stage to the pipeline and executes it, and then returns a cursor to read the contents of the output collection.
        /// </summary>
        /// <param name="collectionName">Name of the collection.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task whose result is a cursor.</returns>
        Task<IAsyncCursor<TResult>> OutAsync(string collectionName, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Appends a project stage to the pipeline.
        /// </summary>
        /// <typeparam name="TNewResult">The type of the result of the stage.</typeparam>
        /// <param name="projection">The projection.</param>
        /// <returns>
        /// The fluent aggregate interface.
        /// </returns>
        IAggregateFluent<TNewResult> Project<TNewResult>(ProjectionDefinition<TResult, TNewResult> projection);

        /// <summary>
        /// Appends a $replaceRoot stage to the pipeline.
        /// </summary>
        /// <typeparam name="TNewResult">The type of the new result.</typeparam>
        /// <param name="newRoot">The new root.</param>
        /// <returns>The fluent aggregate interface.</returns>
        IAggregateFluent<TNewResult> ReplaceRoot<TNewResult>(AggregateExpressionDefinition<TResult, TNewResult> newRoot);

        /// <summary>
        /// Appends a skip stage to the pipeline.
        /// </summary>
        /// <param name="skip">The number of documents to skip.</param>
        /// <returns>The fluent aggregate interface.</returns>
        IAggregateFluent<TResult> Skip(int skip);

        /// <summary>
        /// Appends a sort stage to the pipeline.
        /// </summary>
        /// <param name="sort">The sort specification.</param>
        /// <returns>The fluent aggregate interface.</returns>
        IAggregateFluent<TResult> Sort(SortDefinition<TResult> sort);

        /// <summary>
        /// Appends a sortByCount stage to the pipeline.
        /// </summary>
        /// <typeparam name="TId">The type of the identifier.</typeparam>
        /// <param name="id">The identifier.</param>
        /// <returns>The fluent aggregate interface.</returns>
        IAggregateFluent<AggregateSortByCountResult<TId>> SortByCount<TId>(AggregateExpressionDefinition<TResult, TId> id);

        /// <summary>
        /// Appends an unwind stage to the pipeline.
        /// </summary>
        /// <typeparam name="TNewResult">The type of the result of the stage.</typeparam>
        /// <param name="field">The field.</param>
        /// <param name="newResultSerializer">The new result serializer.</param>
        /// <returns>
        /// The fluent aggregate interface.
        /// </returns>
        [Obsolete("Use the Unwind overload which takes an options parameter.")]
        IAggregateFluent<TNewResult> Unwind<TNewResult>(FieldDefinition<TResult> field, IBsonSerializer<TNewResult> newResultSerializer);

        /// <summary>
        /// Appends an unwind stage to the pipeline.
        /// </summary>
        /// <typeparam name="TNewResult">The type of the new result.</typeparam>
        /// <param name="field">The field.</param>
        /// <param name="options">The options.</param>
        /// <returns>The fluent aggregate interface.</returns>
        IAggregateFluent<TNewResult> Unwind<TNewResult>(FieldDefinition<TResult> field, AggregateUnwindOptions<TNewResult> options = null);
    }

    /// <summary>
    /// Fluent interface for aggregate.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    public interface IOrderedAggregateFluent<TResult> : IAggregateFluent<TResult>
    {
        /// <summary>
        /// Combines the current sort definition with an additional sort definition.
        /// </summary>
        /// <param name="newSort">The new sort.</param>
        /// <returns>The fluent aggregate interface.</returns>
        IOrderedAggregateFluent<TResult> ThenBy(SortDefinition<TResult> newSort);
    }
}

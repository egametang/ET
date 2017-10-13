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
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver
{
    /// <summary>
    /// Extension methods for <see cref="IAggregateFluent{TResult}"/>
    /// </summary>
    public static class IAggregateFluentExtensions
    {
        /// <summary>
        /// Appends a $bucket stage to the pipeline.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="aggregate">The aggregate.</param>
        /// <param name="groupBy">The expression providing the value to group by.</param>
        /// <param name="boundaries">The bucket boundaries.</param>
        /// <param name="options">The options.</param>
        /// <returns>The fluent aggregate interface.</returns>
        public static IAggregateFluent<AggregateBucketResult<TValue>> Bucket<TResult, TValue>(
            this IAggregateFluent<TResult> aggregate,
            Expression<Func<TResult, TValue>> groupBy,
            IEnumerable<TValue> boundaries,
            AggregateBucketOptions<TValue> options = null)
        {
            Ensure.IsNotNull(aggregate, nameof(aggregate));
            return aggregate.AppendStage(PipelineStageDefinitionBuilder.Bucket(groupBy, boundaries, options));
        }

        /// <summary>
        /// Appends a $bucket stage to the pipeline.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <typeparam name="TNewResult">The type of the new result.</typeparam>
        /// <param name="aggregate">The aggregate.</param>
        /// <param name="groupBy">The expression providing the value to group by.</param>
        /// <param name="boundaries">The bucket boundaries.</param>
        /// <param name="output">The output projection.</param>
        /// <param name="options">The options.</param>
        /// <returns>The fluent aggregate interface.</returns>
        public static IAggregateFluent<TNewResult> Bucket<TResult, TValue, TNewResult>(
            this IAggregateFluent<TResult> aggregate,
            Expression<Func<TResult, TValue>> groupBy,
            IEnumerable<TValue> boundaries,
            Expression<Func<IGrouping<TValue, TResult>, TNewResult>> output,
            AggregateBucketOptions<TValue> options = null)
        {
            Ensure.IsNotNull(aggregate, nameof(aggregate));
            return aggregate.AppendStage(PipelineStageDefinitionBuilder.Bucket(groupBy, boundaries, output, options));
        }

        /// <summary>
        /// Appends a $bucketAuto stage to the pipeline.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="aggregate">The aggregate.</param>
        /// <param name="groupBy">The expression providing the value to group by.</param>
        /// <param name="buckets">The number of buckets.</param>
        /// <param name="options">The options (optional).</param>
        /// <returns>The fluent aggregate interface.</returns>
        public static IAggregateFluent<AggregateBucketAutoResult<TValue>> BucketAuto<TResult, TValue>(
            this IAggregateFluent<TResult> aggregate,
            Expression<Func<TResult, TValue>> groupBy,
            int buckets,
            AggregateBucketAutoOptions options = null)
        {
            Ensure.IsNotNull(aggregate, nameof(aggregate));
            return aggregate.AppendStage(PipelineStageDefinitionBuilder.BucketAuto(groupBy, buckets, options));
        }

        /// <summary>
        /// Appends a $bucketAuto stage to the pipeline.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <typeparam name="TNewResult">The type of the new result.</typeparam>
        /// <param name="aggregate">The aggregate.</param>
        /// <param name="groupBy">The expression providing the value to group by.</param>
        /// <param name="buckets">The number of buckets.</param>
        /// <param name="output">The output projection.</param>
        /// <param name="options">The options (optional).</param>
        /// <returns>The fluent aggregate interface.</returns>
        public static IAggregateFluent<TNewResult> BucketAuto<TResult, TValue, TNewResult>(
            this IAggregateFluent<TResult> aggregate,
            Expression<Func<TResult, TValue>> groupBy,
            int buckets,
            Expression<Func<IGrouping<TValue, TResult>, TNewResult>> output,
            AggregateBucketAutoOptions options = null)
        {
            Ensure.IsNotNull(aggregate, nameof(aggregate));
            return aggregate.AppendStage(PipelineStageDefinitionBuilder.BucketAuto(groupBy, buckets, output, options));
        }

        /// <summary>
        /// Appends a $facet stage to the pipeline.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="aggregate">The aggregate.</param>
        /// <param name="facets">The facets.</param>
        /// <returns>The fluent aggregate interface.</returns>
        public static IAggregateFluent<AggregateFacetResults> Facet<TResult>(
            this IAggregateFluent<TResult> aggregate,
            IEnumerable<AggregateFacet<TResult>> facets)
        {
            Ensure.IsNotNull(aggregate, nameof(aggregate));
            return aggregate.AppendStage(PipelineStageDefinitionBuilder.Facet(facets));
        }

        /// <summary>
        /// Appends a $facet stage to the pipeline.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="aggregate">The aggregate.</param>
        /// <param name="facets">The facets.</param>
        /// <returns>The fluent aggregate interface.</returns>
        public static IAggregateFluent<AggregateFacetResults> Facet<TResult>(
            this IAggregateFluent<TResult> aggregate,
            params AggregateFacet<TResult>[] facets)
        {
            Ensure.IsNotNull(aggregate, nameof(aggregate));
            return aggregate.AppendStage(PipelineStageDefinitionBuilder.Facet(facets));
        }

        /// <summary>
        /// Appends a $facet stage to the pipeline.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <typeparam name="TNewResult">The type of the new result.</typeparam>
        /// <param name="aggregate">The aggregate.</param>
        /// <param name="facets">The facets.</param>
        /// <returns>
        /// The fluent aggregate interface.
        /// </returns>
        public static IAggregateFluent<TNewResult> Facet<TResult, TNewResult>(
            this IAggregateFluent<TResult> aggregate,
            params AggregateFacet<TResult>[] facets)
        {
            Ensure.IsNotNull(aggregate, nameof(aggregate));
            return aggregate.AppendStage(PipelineStageDefinitionBuilder.Facet<TResult, TNewResult>(facets));
        }

        /// <summary>
        /// Appends a $graphLookup stage to the pipeline.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <typeparam name="TFrom">The type of the from documents.</typeparam>
        /// <typeparam name="TConnectFrom">The type of the connect from field (must be either TConnectTo or a type that implements IEnumerable{TConnectTo}).</typeparam>
        /// <typeparam name="TConnectTo">The type of the connect to field.</typeparam>
        /// <typeparam name="TStartWith">The type of the start with expression (must be either TConnectTo or a type that implements IEnumerable{TConnectTo}).</typeparam>
        /// <typeparam name="TAs">The type of the as field.</typeparam>
        /// <typeparam name="TNewResult">The type of the new result (must be same as TResult with an additional as field).</typeparam>
        /// <param name="aggregate">The aggregate.</param>
        /// <param name="from">The from collection.</param>
        /// <param name="connectFromField">The connect from field.</param>
        /// <param name="connectToField">The connect to field.</param>
        /// <param name="startWith">The start with value.</param>
        /// <param name="as">The as field.</param>
        /// <param name="options">The options.</param>
        /// <returns>The fluent aggregate interface.</returns>
        public static IAggregateFluent<TNewResult> GraphLookup<TResult, TFrom, TConnectFrom, TConnectTo, TStartWith, TAs, TNewResult>(
            this IAggregateFluent<TResult> aggregate,
            IMongoCollection<TFrom> from,
            FieldDefinition<TFrom, TConnectFrom> connectFromField,
            FieldDefinition<TFrom, TConnectTo> connectToField,
            AggregateExpressionDefinition<TResult, TStartWith> startWith,
            FieldDefinition<TNewResult, TAs> @as,
            AggregateGraphLookupOptions<TFrom, TFrom, TNewResult> options = null)
                where TAs : IEnumerable<TFrom>
        {
            Ensure.IsNotNull(aggregate, nameof(aggregate));
            return aggregate.AppendStage(PipelineStageDefinitionBuilder.GraphLookup(from, connectFromField, connectToField, startWith, @as, options));
        }

        /// <summary>
        /// Appends a $graphLookup stage to the pipeline.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <typeparam name="TFrom">The type of the from documents.</typeparam>
        /// <param name="aggregate">The aggregate.</param>
        /// <param name="from">The from collection.</param>
        /// <param name="connectFromField">The connect from field.</param>
        /// <param name="connectToField">The connect to field.</param>
        /// <param name="startWith">The start with value.</param>
        /// <param name="as">The as field.</param>
        /// <param name="depthField">The depth field.</param>
        /// <returns>The fluent aggregate interface.</returns>
        public static IAggregateFluent<BsonDocument> GraphLookup<TResult, TFrom>(
            this IAggregateFluent<TResult> aggregate,
            IMongoCollection<TFrom> from,
            FieldDefinition<TFrom, BsonValue> connectFromField,
            FieldDefinition<TFrom, BsonValue> connectToField,
            AggregateExpressionDefinition<TResult, BsonValue> startWith,
            FieldDefinition<BsonDocument, IEnumerable<BsonDocument>> @as,
            FieldDefinition<BsonDocument, int> depthField = null)
        {
            Ensure.IsNotNull(aggregate, nameof(aggregate));
            return aggregate.AppendStage(PipelineStageDefinitionBuilder.GraphLookup(from, connectFromField, connectToField, startWith, @as, depthField));
        }

        /// <summary>
        /// Appends a $graphLookup stage to the pipeline.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <typeparam name="TNewResult">The type of the new result (must be same as TResult with an additional as field).</typeparam>
        /// <typeparam name="TFrom">The type of the from documents.</typeparam>
        /// <typeparam name="TConnectFrom">The type of the connect from field (must be either TConnectTo or a type that implements IEnumerable{TConnectTo}).</typeparam>
        /// <typeparam name="TConnectTo">The type of the connect to field.</typeparam>
        /// <typeparam name="TStartWith">The type of the start with expression (must be either TConnectTo or a type that implements IEnumerable{TConnectTo}).</typeparam>
        /// <typeparam name="TAs">The type of the as field.</typeparam>
        /// <param name="aggregate">The aggregate.</param>
        /// <param name="from">The from collection.</param>
        /// <param name="connectFromField">The connect from field.</param>
        /// <param name="connectToField">The connect to field.</param>
        /// <param name="startWith">The start with value.</param>
        /// <param name="as">The as field.</param>
        /// <param name="options">The options.</param>
        /// <returns>The fluent aggregate interface.</returns>
        public static IAggregateFluent<TNewResult> GraphLookup<TResult, TFrom, TConnectFrom, TConnectTo, TStartWith, TAs, TNewResult>(
            this IAggregateFluent<TResult> aggregate,
            IMongoCollection<TFrom> from,
            Expression<Func<TFrom, TConnectFrom>> connectFromField,
            Expression<Func<TFrom, TConnectTo>> connectToField,
            Expression<Func<TResult, TStartWith>> startWith,
            Expression<Func<TNewResult, TAs>> @as,
            AggregateGraphLookupOptions<TFrom, TFrom, TNewResult> options = null)
                where TAs : IEnumerable<TFrom>
        {
            Ensure.IsNotNull(aggregate, nameof(aggregate));
            return aggregate.AppendStage(PipelineStageDefinitionBuilder.GraphLookup(from, connectFromField, connectToField, startWith, @as, options, aggregate.Options?.TranslationOptions));
        }

        /// <summary>
        /// Appends a $graphLookup stage to the pipeline.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <typeparam name="TFrom">The type of the from documents.</typeparam>
        /// <typeparam name="TConnectFrom">The type of the connect from field (must be either TConnectTo or a type that implements IEnumerable{TConnectTo}).</typeparam>
        /// <typeparam name="TConnectTo">The type of the connect to field.</typeparam>
        /// <typeparam name="TStartWith">The type of the start with expression (must be either TConnectTo or a type that implements IEnumerable{TConnectTo}).</typeparam>
        /// <typeparam name="TAsElement">The type of the as field elements.</typeparam>
        /// <typeparam name="TAs">The type of the as field.</typeparam>
        /// <typeparam name="TNewResult">The type of the new result (must be same as TResult with an additional as field).</typeparam>
        /// <param name="aggregate">The aggregate.</param>
        /// <param name="from">The from collection.</param>
        /// <param name="connectFromField">The connect from field.</param>
        /// <param name="connectToField">The connect to field.</param>
        /// <param name="startWith">The start with value.</param>
        /// <param name="as">The as field.</param>
        /// <param name="depthField">The depth field.</param>
        /// <param name="options">The options.</param>
        /// <returns>The fluent aggregate interface.</returns>
        public static IAggregateFluent<TNewResult> GraphLookup<TResult, TFrom, TConnectFrom, TConnectTo, TStartWith, TAsElement, TAs, TNewResult>(
            this IAggregateFluent<TResult> aggregate,
            IMongoCollection<TFrom> from,
            Expression<Func<TFrom, TConnectFrom>> connectFromField,
            Expression<Func<TFrom, TConnectTo>> connectToField,
            Expression<Func<TResult, TStartWith>> startWith,
            Expression<Func<TNewResult, TAs>> @as,
            Expression<Func<TAsElement, int>> depthField,
            AggregateGraphLookupOptions<TFrom, TAsElement, TNewResult> options = null)
                where TAs : IEnumerable<TAsElement>
        {
            Ensure.IsNotNull(aggregate, nameof(aggregate));
            return aggregate.AppendStage(PipelineStageDefinitionBuilder.GraphLookup(from, connectFromField, connectToField, startWith, @as, depthField, options, aggregate.Options?.TranslationOptions));
        }

        /// <summary>
        /// Appends a group stage to the pipeline.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="aggregate">The aggregate.</param>
        /// <param name="group">The group projection.</param>
        /// <returns>
        /// The fluent aggregate interface.
        /// </returns>
        public static IAggregateFluent<BsonDocument> Group<TResult>(this IAggregateFluent<TResult> aggregate, ProjectionDefinition<TResult, BsonDocument> group)
        {
            Ensure.IsNotNull(aggregate, nameof(aggregate));
            return aggregate.AppendStage(PipelineStageDefinitionBuilder.Group(group));
        }

        /// <summary>
        /// Appends a group stage to the pipeline.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TNewResult">The type of the new result.</typeparam>
        /// <param name="aggregate">The aggregate.</param>
        /// <param name="id">The id.</param>
        /// <param name="group">The group projection.</param>
        /// <returns>
        /// The fluent aggregate interface.
        /// </returns>
        public static IAggregateFluent<TNewResult> Group<TResult, TKey, TNewResult>(this IAggregateFluent<TResult> aggregate, Expression<Func<TResult, TKey>> id, Expression<Func<IGrouping<TKey, TResult>, TNewResult>> group)
        {
            Ensure.IsNotNull(aggregate, nameof(aggregate));
            return aggregate.AppendStage(PipelineStageDefinitionBuilder.Group(id, group));
        }

        /// <summary>
        /// Appends a lookup stage to the pipeline.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="aggregate">The aggregate.</param>
        /// <param name="foreignCollectionName">Name of the foreign collection.</param>
        /// <param name="localField">The local field.</param>
        /// <param name="foreignField">The foreign field.</param>
        /// <param name="as">The field in the result to place the foreign matches.</param>
        /// <returns>The fluent aggregate interface.</returns>
        public static IAggregateFluent<BsonDocument> Lookup<TResult>(
            this IAggregateFluent<TResult> aggregate,
            string foreignCollectionName,
            FieldDefinition<TResult> localField,
            FieldDefinition<BsonDocument> foreignField,
            FieldDefinition<BsonDocument> @as)
        {
            Ensure.IsNotNull(aggregate, nameof(aggregate));
            Ensure.IsNotNull(foreignCollectionName, nameof(foreignCollectionName));
            var foreignCollection = aggregate.Database.GetCollection<BsonDocument>(foreignCollectionName);
            return aggregate.AppendStage(PipelineStageDefinitionBuilder.Lookup(foreignCollection, localField, foreignField, @as));
        }

        /// <summary>
        /// Appends a lookup stage to the pipeline.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <typeparam name="TForeignDocument">The type of the foreign collection.</typeparam>
        /// <typeparam name="TNewResult">The type of the new result.</typeparam>
        /// <param name="aggregate">The aggregate.</param>
        /// <param name="foreignCollection">The foreign collection.</param>
        /// <param name="localField">The local field.</param>
        /// <param name="foreignField">The foreign field.</param>
        /// <param name="as">The field in the result to place the foreign matches.</param>
        /// <param name="options">The options.</param>
        /// <returns>The fluent aggregate interface.</returns>
        public static IAggregateFluent<TNewResult> Lookup<TResult, TForeignDocument, TNewResult>(
            this IAggregateFluent<TResult> aggregate,
            IMongoCollection<TForeignDocument> foreignCollection,
            Expression<Func<TResult, object>> localField,
            Expression<Func<TForeignDocument, object>> foreignField,
            Expression<Func<TNewResult, object>> @as,
            AggregateLookupOptions<TForeignDocument, TNewResult> options = null)
        {
            Ensure.IsNotNull(aggregate, nameof(aggregate));
            return aggregate.AppendStage(PipelineStageDefinitionBuilder.Lookup(foreignCollection, localField, foreignField, @as, options));
        }

        /// <summary>
        /// Appends a match stage to the pipeline.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="aggregate">The aggregate.</param>
        /// <param name="filter">The filter.</param>
        /// <returns>
        /// The fluent aggregate interface.
        /// </returns>
        public static IAggregateFluent<TResult> Match<TResult>(this IAggregateFluent<TResult> aggregate, Expression<Func<TResult, bool>> filter)
        {
            Ensure.IsNotNull(aggregate, nameof(aggregate));
            return aggregate.AppendStage(PipelineStageDefinitionBuilder.Match(filter));
        }

        /// <summary>
        /// Appends a project stage to the pipeline.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="aggregate">The aggregate.</param>
        /// <param name="projection">The projection.</param>
        /// <returns>
        /// The fluent aggregate interface.
        /// </returns>
        public static IAggregateFluent<BsonDocument> Project<TResult>(this IAggregateFluent<TResult> aggregate, ProjectionDefinition<TResult, BsonDocument> projection)
        {
            Ensure.IsNotNull(aggregate, nameof(aggregate));
            return aggregate.AppendStage(PipelineStageDefinitionBuilder.Project(projection));
        }

        /// <summary>
        /// Appends a project stage to the pipeline.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <typeparam name="TNewResult">The type of the new result.</typeparam>
        /// <param name="aggregate">The aggregate.</param>
        /// <param name="projection">The projection.</param>
        /// <returns>
        /// The fluent aggregate interface.
        /// </returns>
        public static IAggregateFluent<TNewResult> Project<TResult, TNewResult>(this IAggregateFluent<TResult> aggregate, Expression<Func<TResult, TNewResult>> projection)
        {
            Ensure.IsNotNull(aggregate, nameof(aggregate));
            return aggregate.AppendStage(PipelineStageDefinitionBuilder.Project(projection));
        }

        /// <summary>
        /// Appends a $replaceRoot stage to the pipeline.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <typeparam name="TNewResult">The type of the new result.</typeparam>
        /// <param name="aggregate">The aggregate.</param>
        /// <param name="newRoot">The new root.</param>
        /// <returns>
        /// The fluent aggregate interface.
        /// </returns>
        public static IAggregateFluent<TNewResult> ReplaceRoot<TResult, TNewResult>(
            this IAggregateFluent<TResult> aggregate,
            Expression<Func<TResult, TNewResult>> newRoot)
        {
            Ensure.IsNotNull(aggregate, nameof(aggregate));
            return aggregate.AppendStage(PipelineStageDefinitionBuilder.ReplaceRoot(newRoot));
        }

        /// <summary>
        /// Appends an ascending sort stage to the pipeline.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="aggregate">The aggregate.</param>
        /// <param name="field">The field to sort by.</param>
        /// <returns>
        /// The fluent aggregate interface.
        /// </returns>
        public static IOrderedAggregateFluent<TResult> SortBy<TResult>(this IAggregateFluent<TResult> aggregate, Expression<Func<TResult, object>> field)
        {
            Ensure.IsNotNull(aggregate, nameof(aggregate));
            Ensure.IsNotNull(field, nameof(field));
            var sort = Builders<TResult>.Sort.Ascending(field);
            return (IOrderedAggregateFluent<TResult>)aggregate.AppendStage(PipelineStageDefinitionBuilder.Sort(sort));
        }

        /// <summary>
        /// Appends a sortByCount stage to the pipeline.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <param name="aggregate">The aggregate.</param>
        /// <param name="id">The id.</param>
        /// <returns>
        /// The fluent aggregate interface.
        /// </returns>
        public static IAggregateFluent<AggregateSortByCountResult<TKey>> SortByCount<TResult, TKey>(
            this IAggregateFluent<TResult> aggregate,
            Expression<Func<TResult, TKey>> id)
        {
            Ensure.IsNotNull(aggregate, nameof(aggregate));
            return aggregate.AppendStage(PipelineStageDefinitionBuilder.SortByCount(id));
        }

        /// <summary>
        /// Appends a descending sort stage to the pipeline.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="aggregate">The aggregate.</param>
        /// <param name="field">The field to sort by.</param>
        /// <returns>
        /// The fluent aggregate interface.
        /// </returns>
        public static IOrderedAggregateFluent<TResult> SortByDescending<TResult>(this IAggregateFluent<TResult> aggregate, Expression<Func<TResult, object>> field)
        {
            Ensure.IsNotNull(aggregate, nameof(aggregate));
            Ensure.IsNotNull(field, nameof(field));
            var sort = Builders<TResult>.Sort.Descending(field);
            return (IOrderedAggregateFluent<TResult>)aggregate.AppendStage(PipelineStageDefinitionBuilder.Sort(sort));
        }

        /// <summary>
        /// Modifies the current sort stage by appending an ascending field specification to it.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="aggregate">The aggregate.</param>
        /// <param name="field">The field to sort by.</param>
        /// <returns>
        /// The fluent aggregate interface.
        /// </returns>
        public static IOrderedAggregateFluent<TResult> ThenBy<TResult>(this IOrderedAggregateFluent<TResult> aggregate, Expression<Func<TResult, object>> field)
        {
            Ensure.IsNotNull(aggregate, nameof(aggregate));
            return aggregate.ThenBy(Builders<TResult>.Sort.Ascending(field));
        }

        /// <summary>
        /// Modifies the current sort stage by appending a descending field specification to it.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="aggregate">The aggregate.</param>
        /// <param name="field">The field to sort by.</param>
        /// <returns>
        /// The fluent aggregate interface.
        /// </returns>
        public static IOrderedAggregateFluent<TResult> ThenByDescending<TResult>(this IOrderedAggregateFluent<TResult> aggregate, Expression<Func<TResult, object>> field)
        {
            Ensure.IsNotNull(aggregate, nameof(aggregate));
            return aggregate.ThenBy(Builders<TResult>.Sort.Descending(field));
        }

        /// <summary>
        /// Appends an unwind stage to the pipeline.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="aggregate">The aggregate.</param>
        /// <param name="field">The field to unwind.</param>
        /// <returns>
        /// The fluent aggregate interface.
        /// </returns>
        public static IAggregateFluent<BsonDocument> Unwind<TResult>(this IAggregateFluent<TResult> aggregate, FieldDefinition<TResult> field)
        {
            Ensure.IsNotNull(aggregate, nameof(aggregate));
            return aggregate.AppendStage(PipelineStageDefinitionBuilder.Unwind(field));
        }

        /// <summary>
        /// Appends an unwind stage to the pipeline.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="aggregate">The aggregate.</param>
        /// <param name="field">The field to unwind.</param>
        /// <returns>
        /// The fluent aggregate interface.
        /// </returns>
        public static IAggregateFluent<BsonDocument> Unwind<TResult>(this IAggregateFluent<TResult> aggregate, Expression<Func<TResult, object>> field)
        {
            Ensure.IsNotNull(aggregate, nameof(aggregate));
            return aggregate.AppendStage(PipelineStageDefinitionBuilder.Unwind(field));
        }

        /// <summary>
        /// Appends an unwind stage to the pipeline.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <typeparam name="TNewResult">The type of the new result.</typeparam>
        /// <param name="aggregate">The aggregate.</param>
        /// <param name="field">The field to unwind.</param>
        /// <param name="newResultSerializer">The new result serializer.</param>
        /// <returns>
        /// The fluent aggregate interface.
        /// </returns>
        [Obsolete("Use the Unwind overload which takes an options parameter.")]
        public static IAggregateFluent<TNewResult> Unwind<TResult, TNewResult>(this IAggregateFluent<TResult> aggregate, Expression<Func<TResult, object>> field, IBsonSerializer<TNewResult> newResultSerializer)
        {
            Ensure.IsNotNull(aggregate, nameof(aggregate));
            return aggregate.AppendStage(PipelineStageDefinitionBuilder.Unwind(field, new AggregateUnwindOptions<TNewResult> { ResultSerializer = newResultSerializer }));
        }

        /// <summary>
        /// Appends an unwind stage to the pipeline.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <typeparam name="TNewResult">The type of the new result.</typeparam>
        /// <param name="aggregate">The aggregate.</param>
        /// <param name="field">The field to unwind.</param>
        /// <param name="options">The options.</param>
        /// <returns>
        /// The fluent aggregate interface.
        /// </returns>
        public static IAggregateFluent<TNewResult> Unwind<TResult, TNewResult>(this IAggregateFluent<TResult> aggregate, Expression<Func<TResult, object>> field, AggregateUnwindOptions<TNewResult> options = null)
        {
            Ensure.IsNotNull(aggregate, nameof(aggregate));
            return aggregate.AppendStage(PipelineStageDefinitionBuilder.Unwind(field, options));
        }

        /// <summary>
        /// Returns the first document of the aggregate result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="aggregate">The aggregate.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The fluent aggregate interface.
        /// </returns>
        public static TResult First<TResult>(this IAggregateFluent<TResult> aggregate, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(aggregate, nameof(aggregate));

            return IAsyncCursorSourceExtensions.First(aggregate.Limit(1), cancellationToken);
        }

        /// <summary>
        /// Returns the first document of the aggregate result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="aggregate">The aggregate.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The fluent aggregate interface.
        /// </returns>
        public static Task<TResult> FirstAsync<TResult>(this IAggregateFluent<TResult> aggregate, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(aggregate, nameof(aggregate));

            return IAsyncCursorSourceExtensions.FirstAsync(aggregate.Limit(1), cancellationToken);
        }

        /// <summary>
        /// Returns the first document of the aggregate result, or the default value if the result set is empty.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="aggregate">The aggregate.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The fluent aggregate interface.
        /// </returns>
        public static TResult FirstOrDefault<TResult>(this IAggregateFluent<TResult> aggregate, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(aggregate, nameof(aggregate));

            return IAsyncCursorSourceExtensions.FirstOrDefault(aggregate.Limit(1), cancellationToken);
        }

        /// <summary>
        /// Returns the first document of the aggregate result, or the default value if the result set is empty.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="aggregate">The aggregate.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The fluent aggregate interface.
        /// </returns>
        public static Task<TResult> FirstOrDefaultAsync<TResult>(this IAggregateFluent<TResult> aggregate, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(aggregate, nameof(aggregate));

            return IAsyncCursorSourceExtensions.FirstOrDefaultAsync(aggregate.Limit(1), cancellationToken);
        }

        /// <summary>
        /// Returns the only document of the aggregate result. Throws an exception if the result set does not contain exactly one document.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="aggregate">The aggregate.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The fluent aggregate interface.
        /// </returns>
        public static TResult Single<TResult>(this IAggregateFluent<TResult> aggregate, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(aggregate, nameof(aggregate));

            return IAsyncCursorSourceExtensions.Single(aggregate.Limit(2), cancellationToken);
        }

        /// <summary>
        /// Returns the only document of the aggregate result. Throws an exception if the result set does not contain exactly one document.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="aggregate">The aggregate.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The fluent aggregate interface.
        /// </returns>
        public static Task<TResult> SingleAsync<TResult>(this IAggregateFluent<TResult> aggregate, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(aggregate, nameof(aggregate));

            return IAsyncCursorSourceExtensions.SingleAsync(aggregate.Limit(2), cancellationToken);
        }

        /// <summary>
        /// Returns the only document of the aggregate result, or the default value if the result set is empty. Throws an exception if the result set contains more than one document.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="aggregate">The aggregate.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The fluent aggregate interface.
        /// </returns>
        public static TResult SingleOrDefault<TResult>(this IAggregateFluent<TResult> aggregate, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(aggregate, nameof(aggregate));

            return IAsyncCursorSourceExtensions.SingleOrDefault(aggregate.Limit(2), cancellationToken);
        }

        /// <summary>
        /// Returns the only document of the aggregate result, or the default value if the result set is empty. Throws an exception if the result set contains more than one document.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="aggregate">The aggregate.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The fluent aggregate interface.
        /// </returns>
        public static Task<TResult> SingleOrDefaultAsync<TResult>(this IAggregateFluent<TResult> aggregate, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(aggregate, nameof(aggregate));

            return IAsyncCursorSourceExtensions.SingleOrDefaultAsync(aggregate.Limit(2), cancellationToken);
        }
    }
}

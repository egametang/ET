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

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver
{
    internal class AggregateFluent<TDocument, TResult> : AggregateFluentBase<TResult>
    {
        // fields
        private readonly IMongoCollection<TDocument> _collection;
        private readonly AggregateOptions _options;
        private readonly PipelineDefinition<TDocument, TResult> _pipeline;

        // constructors
        public AggregateFluent(IMongoCollection<TDocument> collection, PipelineDefinition<TDocument, TResult> pipeline, AggregateOptions options)
        {
            _collection = Ensure.IsNotNull(collection, nameof(collection));
            _pipeline = Ensure.IsNotNull(pipeline, nameof(pipeline));
            _options = Ensure.IsNotNull(options, nameof(options));
        }

        // properties
        public override IMongoDatabase Database
        {
            get { return _collection.Database; }
        }

        public override AggregateOptions Options
        {
            get { return _options; }
        }

        public override IList<IPipelineStageDefinition> Stages
        {
            get { return _pipeline.Stages.ToList(); }
        }

        // methods
        public override IAggregateFluent<TNewResult> AppendStage<TNewResult>(PipelineStageDefinition<TResult, TNewResult> stage)
        {
            return WithPipeline(_pipeline.AppendStage(stage));
        }

        public override IAggregateFluent<TNewResult> As<TNewResult>(IBsonSerializer<TNewResult> newResultSerializer)
        {
            return WithPipeline(_pipeline.As(newResultSerializer));
        }

        public override IAggregateFluent<AggregateBucketResult<TValue>> Bucket<TValue>(
            AggregateExpressionDefinition<TResult, TValue> groupBy,
            IEnumerable<TValue> boundaries,
            AggregateBucketOptions<TValue> options = null)
        {
            return WithPipeline(_pipeline.Bucket(groupBy, boundaries, options));
        }

        public override IAggregateFluent<TNewResult> Bucket<TValue, TNewResult>(
            AggregateExpressionDefinition<TResult, TValue> groupBy,
            IEnumerable<TValue> boundaries,
            ProjectionDefinition<TResult, TNewResult> output,
            AggregateBucketOptions<TValue> options = null)
        {
            return WithPipeline(_pipeline.Bucket(groupBy, boundaries, output, options));
        }

        public override IAggregateFluent<AggregateBucketAutoResult<TValue>> BucketAuto<TValue>(
            AggregateExpressionDefinition<TResult, TValue> groupBy,
            int buckets,
            AggregateBucketAutoOptions options = null)
        {
            return WithPipeline(_pipeline.BucketAuto(groupBy, buckets, options));
        }

        public override IAggregateFluent<TNewResult> BucketAuto<TValue, TNewResult>(
            AggregateExpressionDefinition<TResult, TValue> groupBy,
            int buckets,
            ProjectionDefinition<TResult, TNewResult> output,
            AggregateBucketAutoOptions options = null)
        {
            return WithPipeline(_pipeline.BucketAuto(groupBy, buckets, output, options));
        }

        public override IAggregateFluent<AggregateCountResult> Count()
        {
            return WithPipeline(_pipeline.Count());
        }

        public override IAggregateFluent<TNewResult> Facet<TNewResult>(
            IEnumerable<AggregateFacet<TResult>> facets,
            AggregateFacetOptions<TNewResult> options = null)
        {
            return WithPipeline(_pipeline.Facet(facets, options));
        }

        public override IAggregateFluent<TNewResult> GraphLookup<TFrom, TConnectFrom, TConnectTo, TStartWith, TAsElement, TAs, TNewResult>(
            IMongoCollection<TFrom> from,
            FieldDefinition<TFrom, TConnectFrom> connectFromField,
            FieldDefinition<TFrom, TConnectTo> connectToField,
            AggregateExpressionDefinition<TResult, TStartWith> startWith,
            FieldDefinition<TNewResult, TAs> @as,
            FieldDefinition<TAsElement, int> depthField,
            AggregateGraphLookupOptions<TFrom, TAsElement, TNewResult> options = null)
        {
            return WithPipeline(_pipeline.GraphLookup(from, connectFromField, connectToField, startWith, @as, depthField, options));
        }

        public override IAggregateFluent<TNewResult> Group<TNewResult>(ProjectionDefinition<TResult, TNewResult> group)
        {
            return WithPipeline(_pipeline.Group(group));
        }

        public override IAggregateFluent<TResult> Limit(int limit)
        {
            return WithPipeline(_pipeline.Limit(limit));
        }

        public override IAggregateFluent<TNewResult> Lookup<TForeignDocument, TNewResult>(string foreignCollectionName, FieldDefinition<TResult> localField, FieldDefinition<TForeignDocument> foreignField, FieldDefinition<TNewResult> @as, AggregateLookupOptions<TForeignDocument, TNewResult> options)
        {
            Ensure.IsNotNull(foreignCollectionName, nameof(foreignCollectionName));
            var foreignCollection = _collection.Database.GetCollection<TForeignDocument>(foreignCollectionName);
            return WithPipeline(_pipeline.Lookup(foreignCollection, localField, foreignField, @as, options));
        }

        public override IAggregateFluent<TResult> Match(FilterDefinition<TResult> filter)
        {
            return WithPipeline(_pipeline.Match(filter));
        }

        public override IAggregateFluent<TNewResult> OfType<TNewResult>(IBsonSerializer<TNewResult> newResultSerializer)
        {
            return WithPipeline(_pipeline.OfType(newResultSerializer));
        }

        public override IAsyncCursor<TResult> Out(string collectionName, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(collectionName, nameof(collectionName));
            var outputCollection = Database.GetCollection<TResult>(collectionName);
            var aggregate = WithPipeline(_pipeline.Out(outputCollection));
            return aggregate.ToCursor(cancellationToken);
        }

        public override Task<IAsyncCursor<TResult>> OutAsync(string collectionName, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(collectionName, nameof(collectionName));
            var outputCollection = Database.GetCollection<TResult>(collectionName);
            var aggregate = WithPipeline(_pipeline.Out(outputCollection));
            return aggregate.ToCursorAsync(cancellationToken);
        }

        public override IAggregateFluent<TNewResult> Project<TNewResult>(ProjectionDefinition<TResult, TNewResult> projection)
        {
            return WithPipeline(_pipeline.Project(projection));
        }

        public override IAggregateFluent<TNewResult> ReplaceRoot<TNewResult>(AggregateExpressionDefinition<TResult, TNewResult> newRoot)
        {
            return WithPipeline(_pipeline.ReplaceRoot(newRoot));
        }

        public override IAggregateFluent<TResult> Skip(int skip)
        {
            return WithPipeline(_pipeline.Skip(skip));
        }

        public override IAggregateFluent<TResult> Sort(SortDefinition<TResult> sort)
        {
            return WithPipeline(_pipeline.Sort(sort));
        }

        public override IAggregateFluent<AggregateSortByCountResult<TId>> SortByCount<TId>(AggregateExpressionDefinition<TResult, TId> id)
        {
            return WithPipeline(_pipeline.SortByCount(id));
        }

        public override IOrderedAggregateFluent<TResult> ThenBy(SortDefinition<TResult> newSort)
        {
            Ensure.IsNotNull(newSort, nameof(newSort));
            var stages = _pipeline.Stages.ToList();
            var oldSortStage = (SortPipelineStageDefinition<TResult>)stages[stages.Count - 1];
            var oldSort = oldSortStage.Sort;
            var combinedSort = Builders<TResult>.Sort.Combine(oldSort, newSort);
            var combinedSortStage = PipelineStageDefinitionBuilder.Sort(combinedSort);
            stages[stages.Count - 1] = combinedSortStage;
            var newPipeline = new PipelineStagePipelineDefinition<TDocument, TResult>(stages);
            return (IOrderedAggregateFluent<TResult>)WithPipeline(newPipeline);
        }

        public override IAggregateFluent<TNewResult> Unwind<TNewResult>(FieldDefinition<TResult> field, IBsonSerializer<TNewResult> newResultSerializer)
        {
            return WithPipeline(_pipeline.Unwind(field, new AggregateUnwindOptions<TNewResult> { ResultSerializer = newResultSerializer }));
        }

        public override IAggregateFluent<TNewResult> Unwind<TNewResult>(FieldDefinition<TResult> field, AggregateUnwindOptions<TNewResult> options)
        {
            return WithPipeline(_pipeline.Unwind(field, options));
        }

        public override IAsyncCursor<TResult> ToCursor(CancellationToken cancellationToken)
        {
            return _collection.Aggregate(_pipeline, _options, cancellationToken);
        }

        public override Task<IAsyncCursor<TResult>> ToCursorAsync(CancellationToken cancellationToken)
        {
            return _collection.AggregateAsync(_pipeline, _options, cancellationToken);
        }

        public override string ToString()
        {
            return $"aggregate({_pipeline})";
        }

        public IAggregateFluent<TNewResult> WithPipeline<TNewResult>(PipelineDefinition<TDocument, TNewResult> pipeline)
        {
            return new AggregateFluent<TDocument, TNewResult>(_collection, pipeline, _options);
        }
    }
}

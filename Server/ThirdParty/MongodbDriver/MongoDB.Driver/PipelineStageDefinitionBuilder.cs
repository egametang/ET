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
using System.Reflection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Linq.Translators;

namespace MongoDB.Driver
{
    /// <summary>
    /// Methods for building pipeline stages.
    /// </summary>
    public static class PipelineStageDefinitionBuilder
    {
        /// <summary>
        /// Creates a $bucket stage.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TValue">The type of the values.</typeparam>
        /// <param name="groupBy">The group by expression.</param>
        /// <param name="boundaries">The boundaries.</param>
        /// <param name="options">The options.</param>
        /// <returns>The stage.</returns>
        public static PipelineStageDefinition<TInput, AggregateBucketResult<TValue>> Bucket<TInput, TValue>(
            AggregateExpressionDefinition<TInput, TValue> groupBy,
            IEnumerable<TValue> boundaries,
            AggregateBucketOptions<TValue> options = null)
        {
            Ensure.IsNotNull(groupBy, nameof(groupBy));
            Ensure.IsNotNull(boundaries, nameof(boundaries));

            const string operatorName = "$bucket";
            var stage = new DelegatedPipelineStageDefinition<TInput, AggregateBucketResult<TValue>>(
                operatorName,
                (s, sr) =>
                {
                    var valueSerializer = sr.GetSerializer<TValue>();
                    var renderedGroupBy = groupBy.Render(s, sr);
                    var serializedBoundaries = boundaries.Select(b => valueSerializer.ToBsonValue(b));
                    var serializedDefaultBucket = options != null && options.DefaultBucket.HasValue ? valueSerializer.ToBsonValue(options.DefaultBucket.Value) : null;
                    var document = new BsonDocument
                    {
                        { operatorName, new BsonDocument
                            {
                                { "groupBy", renderedGroupBy },
                                { "boundaries", new BsonArray(serializedBoundaries) },
                                { "default", serializedDefaultBucket, serializedDefaultBucket != null }
                            }
                        }
                    };
                    return new RenderedPipelineStageDefinition<AggregateBucketResult<TValue>>(
                        operatorName,
                        document,
                        sr.GetSerializer<AggregateBucketResult<TValue>>());
                });

            return stage;
        }

        /// <summary>
        /// Creates a $bucket stage.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TValue">The type of the values.</typeparam>
        /// <typeparam name="TOutput">The type of the output documents.</typeparam>
        /// <param name="groupBy">The group by expression.</param>
        /// <param name="boundaries">The boundaries.</param>
        /// <param name="output">The output projection.</param>
        /// <param name="options">The options.</param>
        /// <returns>The stage.</returns>
        public static PipelineStageDefinition<TInput, TOutput> Bucket<TInput, TValue, TOutput>(
            AggregateExpressionDefinition<TInput, TValue> groupBy,
            IEnumerable<TValue> boundaries,
            ProjectionDefinition<TInput, TOutput> output,
            AggregateBucketOptions<TValue> options = null)
        {
            Ensure.IsNotNull(groupBy, nameof(groupBy));
            Ensure.IsNotNull(boundaries, nameof(boundaries));
            Ensure.IsNotNull(output, nameof(output));

            const string operatorName = "$bucket";
            var stage = new DelegatedPipelineStageDefinition<TInput, TOutput>(
                operatorName,
                (s, sr) =>
                {
                    var valueSerializer = sr.GetSerializer<TValue>();
                    var outputSerializer = sr.GetSerializer<TOutput>();
                    var renderedGroupBy = groupBy.Render(s, sr);
                    var serializedBoundaries = boundaries.Select(b => valueSerializer.ToBsonValue(b));
                    var serializedDefaultBucket = options != null && options.DefaultBucket.HasValue ? valueSerializer.ToBsonValue(options.DefaultBucket.Value) : null;
                    var renderedOutput = output.Render(s, sr);
                    var document = new BsonDocument
                    {
                        { operatorName, new BsonDocument
                            {
                                { "groupBy", renderedGroupBy },
                                { "boundaries", new BsonArray(serializedBoundaries) },
                                { "default", serializedDefaultBucket, serializedDefaultBucket != null },
                                { "output", renderedOutput.Document }
                            }
                        }
                    };
                    return new RenderedPipelineStageDefinition<TOutput>(
                        operatorName,
                        document,
                        outputSerializer);
                });

            return stage;
        }

        /// <summary>
        /// Creates a $bucket stage.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TValue">The type of the values.</typeparam>
        /// <param name="groupBy">The group by expression.</param>
        /// <param name="boundaries">The boundaries.</param>
        /// <param name="options">The options.</param>
        /// <param name="translationOptions">The translation options.</param>
        /// <returns>The stage.</returns>
        public static PipelineStageDefinition<TInput, AggregateBucketResult<TValue>> Bucket<TInput, TValue>(
            Expression<Func<TInput, TValue>> groupBy,
            IEnumerable<TValue> boundaries,
            AggregateBucketOptions<TValue> options = null,
            ExpressionTranslationOptions translationOptions = null)
        {
            Ensure.IsNotNull(groupBy, nameof(groupBy));
            return Bucket(
                new ExpressionAggregateExpressionDefinition<TInput, TValue>(groupBy, translationOptions),
                boundaries,
                options);
        }

        /// <summary>
        /// Creates a $bucket stage.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TValue">The type of the values.</typeparam>
        /// <typeparam name="TOutput">The type of the output documents.</typeparam>
        /// <param name="groupBy">The group by expression.</param>
        /// <param name="boundaries">The boundaries.</param>
        /// <param name="output">The output projection.</param>
        /// <param name="options">The options.</param>
        /// <param name="translationOptions">The translation options.</param>
        /// <returns>The stage.</returns>
        public static PipelineStageDefinition<TInput, TOutput> Bucket<TInput, TValue, TOutput>(
            Expression<Func<TInput, TValue>> groupBy,
            IEnumerable<TValue> boundaries,
            Expression<Func<IGrouping<TValue, TInput>, TOutput>> output,
            AggregateBucketOptions<TValue> options = null,
            ExpressionTranslationOptions translationOptions = null)
        {
            Ensure.IsNotNull(groupBy, nameof(groupBy));
            Ensure.IsNotNull(output, nameof(output));
            return Bucket(
                new ExpressionAggregateExpressionDefinition<TInput, TValue>(groupBy, translationOptions),
                boundaries,
                new ExpressionBucketOutputProjection<TInput, TValue, TOutput>(x => default(TValue), output, translationOptions),
                options);
        }

        /// <summary>
        /// Creates a $bucketAuto stage.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TValue">The type of the values.</typeparam>
        /// <param name="groupBy">The group by expression.</param>
        /// <param name="buckets">The number of buckets.</param>
        /// <param name="options">The options.</param>
        /// <returns>The stage.</returns>
        public static PipelineStageDefinition<TInput, AggregateBucketAutoResult<TValue>> BucketAuto<TInput, TValue>(
            AggregateExpressionDefinition<TInput, TValue> groupBy,
            int buckets,
            AggregateBucketAutoOptions options = null)
        {
            Ensure.IsNotNull(groupBy, nameof(groupBy));
            Ensure.IsGreaterThanZero(buckets, nameof(buckets));

            const string operatorName = "$bucketAuto";
            var stage = new DelegatedPipelineStageDefinition<TInput, AggregateBucketAutoResult<TValue>>(
                operatorName,
                (s, sr) =>
                {
                    var renderedGroupBy = groupBy.Render(s, sr);
                    var document = new BsonDocument
                    {
                            { operatorName, new BsonDocument
                                {
                                    { "groupBy", renderedGroupBy },
                                    { "buckets", buckets },
                                    { "granularity", () => options.Granularity.Value.Value, options != null && options.Granularity.HasValue }
                                }
                            }
                    };
                    return new RenderedPipelineStageDefinition<AggregateBucketAutoResult<TValue>>(
                        operatorName,
                        document,
                        sr.GetSerializer<AggregateBucketAutoResult<TValue>>());
                });

            return stage;
        }

        /// <summary>
        /// Creates a $bucketAuto stage.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TValue">The type of the values.</typeparam>
        /// <typeparam name="TOutput">The type of the output documents.</typeparam>
        /// <param name="groupBy">The group by expression.</param>
        /// <param name="buckets">The number of buckets.</param>
        /// <param name="output">The output projection.</param>
        /// <param name="options">The options.</param>
        /// <returns>The stage.</returns>
        public static PipelineStageDefinition<TInput, TOutput> BucketAuto<TInput, TValue, TOutput>(
            AggregateExpressionDefinition<TInput, TValue> groupBy,
            int buckets,
            ProjectionDefinition<TInput, TOutput> output,
            AggregateBucketAutoOptions options = null)
        {
            Ensure.IsNotNull(groupBy, nameof(groupBy));
            Ensure.IsGreaterThanZero(buckets, nameof(buckets));
            Ensure.IsNotNull(output, nameof(output));

            const string operatorName = "$bucketAuto";
            var stage = new DelegatedPipelineStageDefinition<TInput, TOutput>(
                operatorName,
                (s, sr) =>
                {
                    var outputSerializer = sr.GetSerializer<TOutput>();
                    var renderedGroupBy = groupBy.Render(s, sr);
                    var renderedOutput = output.Render(s, sr);
                    var document = new BsonDocument
                    {
                        { operatorName, new BsonDocument
                            {
                                { "groupBy", renderedGroupBy },
                                { "buckets", buckets },
                                { "output", renderedOutput.Document },
                                { "granularity", () => options.Granularity.Value.Value, options != null && options.Granularity.HasValue }
                           }
                        }
                    };
                    return new RenderedPipelineStageDefinition<TOutput>(
                        operatorName,
                        document,
                        outputSerializer);
                });

            return stage;
        }

        /// <summary>
        /// Creates a $bucketAuto stage.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="groupBy">The group by expression.</param>
        /// <param name="buckets">The number of buckets.</param>
        /// <param name="options">The options (optional).</param>
        /// <param name="translationOptions">The translation options.</param>
        /// <returns>The stage.</returns>
        public static PipelineStageDefinition<TInput, AggregateBucketAutoResult<TValue>> BucketAuto<TInput, TValue>(
            Expression<Func<TInput, TValue>> groupBy,
            int buckets,
            AggregateBucketAutoOptions options = null,
            ExpressionTranslationOptions translationOptions = null)
        {
            Ensure.IsNotNull(groupBy, nameof(groupBy));
            return BucketAuto(
                new ExpressionAggregateExpressionDefinition<TInput, TValue>(groupBy, translationOptions),
                buckets,
                options);
        }

        /// <summary>
        /// Creates a $bucketAuto stage.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TValue">The type of the output documents.</typeparam>
        /// <typeparam name="TOutput">The type of the output documents.</typeparam>
        /// <param name="groupBy">The group by expression.</param>
        /// <param name="buckets">The number of buckets.</param>
        /// <param name="output">The output projection.</param>
        /// <param name="options">The options (optional).</param>
        /// <param name="translationOptions">The translation options.</param>
        /// <returns>The stage.</returns>
        public static PipelineStageDefinition<TInput, TOutput> BucketAuto<TInput, TValue, TOutput>(
            Expression<Func<TInput, TValue>> groupBy,
            int buckets,
            Expression<Func<IGrouping<TValue, TInput>, TOutput>> output,
            AggregateBucketAutoOptions options = null,
            ExpressionTranslationOptions translationOptions = null)
        {
            Ensure.IsNotNull(groupBy, nameof(groupBy));
            Ensure.IsNotNull(output, nameof(output));
            return BucketAuto(
                new ExpressionAggregateExpressionDefinition<TInput, TValue>(groupBy, translationOptions),
                buckets,
                new ExpressionBucketOutputProjection<TInput, TValue, TOutput>(x => default(TValue), output, translationOptions),
                options);
        }

        /// <summary>
        /// Creates a $count stage.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <returns>The stage.</returns>
        public static PipelineStageDefinition<TInput, AggregateCountResult> Count<TInput>()
        {
            const string operatorName = "$count";
            var stage = new DelegatedPipelineStageDefinition<TInput, AggregateCountResult>(
                operatorName,
                (s, sr) =>
                {
                    return new RenderedPipelineStageDefinition<AggregateCountResult>(
                        operatorName,
                        new BsonDocument(operatorName, "count"),
                        sr.GetSerializer<AggregateCountResult>());
                });

            return stage;
        }

        /// <summary>
        /// Creates a $facet stage.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TOutput">The type of the output documents.</typeparam>
        /// <param name="facets">The facets.</param>
        /// <param name="options">The options.</param>
        /// <returns>The stage.</returns>
        public static PipelineStageDefinition<TInput, TOutput> Facet<TInput, TOutput>(
            IEnumerable<AggregateFacet<TInput>> facets,
            AggregateFacetOptions<TOutput> options = null)
        {
            Ensure.IsNotNull(facets, nameof(facets));

            const string operatorName = "$facet";
            var materializedFacets = facets.ToArray();
            var stage = new DelegatedPipelineStageDefinition<TInput, TOutput>(
                operatorName,
                (s, sr) =>
                {
                    var facetsDocument = new BsonDocument();
                    foreach (var facet in materializedFacets)
                    {
                        var renderedPipeline = facet.RenderPipeline(s, sr);
                        facetsDocument.Add(facet.Name, renderedPipeline);
                    }
                    var document = new BsonDocument("$facet", facetsDocument);
                    var outputSerializer = options?.OutputSerializer ?? sr.GetSerializer<TOutput>();
                    return new RenderedPipelineStageDefinition<TOutput>(operatorName, document, outputSerializer);
                });

            return stage;
        }

        /// <summary>
        /// Creates a $facet stage.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <param name="facets">The facets.</param>
        /// <returns>The stage.</returns>
        public static PipelineStageDefinition<TInput, AggregateFacetResults> Facet<TInput>(
            IEnumerable<AggregateFacet<TInput>> facets)
        {
            Ensure.IsNotNull(facets, nameof(facets));
            var outputSerializer = new AggregateFacetResultsSerializer(
                facets.Select(f => f.Name),
                facets.Select(f => f.OutputSerializer ?? BsonSerializer.SerializerRegistry.GetSerializer(f.OutputType)));
            var options = new AggregateFacetOptions<AggregateFacetResults> { OutputSerializer = outputSerializer };
            return Facet(facets, options);
        }

        /// <summary>
        /// Creates a $facet stage.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <param name="facets">The facets.</param>
        /// <returns>The stage.</returns>
        public static PipelineStageDefinition<TInput, AggregateFacetResults> Facet<TInput>(
            params AggregateFacet<TInput>[] facets)
        {
            return Facet((IEnumerable<AggregateFacet<TInput>>)facets);
        }

        /// <summary>
        /// Creates a $facet stage.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TOutput">The type of the output documents.</typeparam>
        /// <param name="facets">The facets.</param>
        /// <returns>The stage.</returns>
        public static PipelineStageDefinition<TInput, TOutput> Facet<TInput, TOutput>(
            params AggregateFacet<TInput>[] facets)
        {
            return Facet<TInput, TOutput>((IEnumerable<AggregateFacet<TInput>>)facets);
        }

        /// <summary>
        /// Creates a $graphLookup stage.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TFrom">The type of the from documents.</typeparam>
        /// <typeparam name="TConnectFrom">The type of the connect from field (must be either TConnectTo or a type that implements IEnumerable{TConnectTo}).</typeparam>
        /// <typeparam name="TConnectTo">The type of the connect to field.</typeparam>
        /// <typeparam name="TStartWith">The type of the start with expression (must be either TConnectTo or a type that implements IEnumerable{TConnectTo}).</typeparam>
        /// <typeparam name="TAsElement">The type of the as field elements.</typeparam>
        /// <typeparam name="TAs">The type of the as field.</typeparam>
        /// <typeparam name="TOutput">The type of the output documents.</typeparam>
        /// <param name="from">The from collection.</param>
        /// <param name="connectFromField">The connect from field.</param>
        /// <param name="connectToField">The connect to field.</param>
        /// <param name="startWith">The start with value.</param>
        /// <param name="as">The as field.</param>
        /// <param name="depthField">The depth field.</param>
        /// <param name="options">The options.</param>
        /// <returns>The stage.</returns>
        public static PipelineStageDefinition<TInput, TOutput> GraphLookup<TInput, TFrom, TConnectFrom, TConnectTo, TStartWith, TAsElement, TAs, TOutput>(
            IMongoCollection<TFrom> from,
            FieldDefinition<TFrom, TConnectFrom> connectFromField,
            FieldDefinition<TFrom, TConnectTo> connectToField,
            AggregateExpressionDefinition<TInput, TStartWith> startWith,
            FieldDefinition<TOutput, TAs> @as,
            FieldDefinition<TAsElement, int> depthField,
            AggregateGraphLookupOptions<TFrom, TAsElement, TOutput> options = null)
                where TAs : IEnumerable<TAsElement>
        {
            Ensure.IsNotNull(from, nameof(from));
            Ensure.IsNotNull(connectFromField, nameof(connectFromField));
            Ensure.IsNotNull(connectToField, nameof(connectToField));
            Ensure.IsNotNull(startWith, nameof(startWith));
            Ensure.IsNotNull(@as, nameof(@as));
            Ensure.That(IsTConnectToOrEnumerableTConnectTo<TConnectFrom, TConnectTo>(), "TConnectFrom must be either TConnectTo or a type that implements IEnumerable<TConnectTo>.", nameof(TConnectFrom));
            Ensure.That(IsTConnectToOrEnumerableTConnectTo<TStartWith, TConnectTo>(), "TStartWith must be either TConnectTo or a type that implements IEnumerable<TConnectTo>.", nameof(TStartWith));

            const string operatorName = "$graphLookup";
            var stage = new DelegatedPipelineStageDefinition<TInput, TOutput>(
                operatorName,
                (s, sr) =>
                {
                    var inputSerializer = s;
                    var outputSerializer = options?.OutputSerializer ?? sr.GetSerializer<TOutput>();
                    var fromSerializer = options?.FromSerializer ?? sr.GetSerializer<TFrom>();
                    var asElementSerializer = options?.AsElementSerializer ?? sr.GetSerializer<TAsElement>();
                    var renderedConnectFromField = connectFromField.Render(fromSerializer, sr);
                    var renderedConnectToField = connectToField.Render(fromSerializer, sr);
                    var renderedStartWith = startWith.Render(inputSerializer, sr);
                    var renderedAs = @as.Render(outputSerializer, sr);
                    var renderedDepthField = depthField?.Render(asElementSerializer, sr);
                    var renderedRestrictSearchWithMatch = options?.RestrictSearchWithMatch?.Render(fromSerializer, sr);
                    var document = new BsonDocument
                    {
                        { operatorName, new BsonDocument
                            {
                                { "from", from.CollectionNamespace.CollectionName },
                                { "connectFromField", renderedConnectFromField.FieldName },
                                { "connectToField", renderedConnectToField.FieldName },
                                { "startWith", renderedStartWith },
                                { "as", renderedAs.FieldName },
                                { "depthField", () => renderedDepthField.FieldName, renderedDepthField != null },
                                { "maxDepth", () => options.MaxDepth.Value, options != null && options.MaxDepth.HasValue },
                                { "restrictSearchWithMatch", renderedRestrictSearchWithMatch, renderedRestrictSearchWithMatch != null }
                            }
                        }
                    };
                    return new RenderedPipelineStageDefinition<TOutput>(operatorName, document, outputSerializer);
                });

            return stage;
        }

        /// <summary>
        /// Creates a $graphLookup stage.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TFrom">The type of the from documents.</typeparam>
        /// <typeparam name="TConnectFrom">The type of the connect from field (must be either TConnectTo or a type that implements IEnumerable{TConnectTo}).</typeparam>
        /// <typeparam name="TConnectTo">The type of the connect to field.</typeparam>
        /// <typeparam name="TStartWith">The type of the start with expression (must be either TConnectTo or a type that implements IEnumerable{TConnectTo}).</typeparam>
        /// <typeparam name="TAs">The type of the as field.</typeparam>
        /// <typeparam name="TOutput">The type of the output documents.</typeparam>
        /// <param name="from">The from collection.</param>
        /// <param name="connectFromField">The connect from field.</param>
        /// <param name="connectToField">The connect to field.</param>
        /// <param name="startWith">The start with value.</param>
        /// <param name="as">The as field.</param>
        /// <param name="options">The options.</param>
        /// <returns>The stage.</returns>
        public static PipelineStageDefinition<TInput, TOutput> GraphLookup<TInput, TFrom, TConnectFrom, TConnectTo, TStartWith, TAs, TOutput>(
            IMongoCollection<TFrom> from,
            FieldDefinition<TFrom, TConnectFrom> connectFromField,
            FieldDefinition<TFrom, TConnectTo> connectToField,
            AggregateExpressionDefinition<TInput, TStartWith> startWith,
            FieldDefinition<TOutput, TAs> @as,
            AggregateGraphLookupOptions<TFrom, TFrom, TOutput> options = null)
                where TAs : IEnumerable<TFrom>
        {
            return GraphLookup(from, connectFromField, connectToField, startWith, @as, null, options);
        }

        /// <summary>
        /// Creates a $graphLookup stage.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TFrom">The type of the from documents.</typeparam>
        /// <param name="from">The from collection.</param>
        /// <param name="connectFromField">The connect from field.</param>
        /// <param name="connectToField">The connect to field.</param>
        /// <param name="startWith">The start with value.</param>
        /// <param name="as">The as field.</param>
        /// <param name="depthField">The depth field.</param>
        /// <returns>The fluent aggregate interface.</returns>
        public static PipelineStageDefinition<TInput, BsonDocument> GraphLookup<TInput, TFrom>(
            IMongoCollection<TFrom> from,
            FieldDefinition<TFrom, BsonValue> connectFromField,
            FieldDefinition<TFrom, BsonValue> connectToField,
            AggregateExpressionDefinition<TInput, BsonValue> startWith,
            FieldDefinition<BsonDocument, IEnumerable<BsonDocument>> @as,
            FieldDefinition<BsonDocument, int> depthField = null)
        {
            return GraphLookup(from, connectFromField, connectToField, startWith, @as, depthField, null);
        }

        /// <summary>
        /// Creates a $graphLookup stage.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TFrom">The type of the from documents.</typeparam>
        /// <typeparam name="TConnectFrom">The type of the connect from field (must be either TConnectTo or a type that implements IEnumerable{TConnectTo}).</typeparam>
        /// <typeparam name="TConnectTo">The type of the connect to field.</typeparam>
        /// <typeparam name="TStartWith">The type of the start with expression (must be either TConnectTo or a type that implements IEnumerable{TConnectTo}).</typeparam>
        /// <typeparam name="TAs">The type of the as field.</typeparam>
        /// <typeparam name="TOutput">The type of the output documents.</typeparam>
        /// <param name="from">The from collection.</param>
        /// <param name="connectFromField">The connect from field.</param>
        /// <param name="connectToField">The connect to field.</param>
        /// <param name="startWith">The start with value.</param>
        /// <param name="as">The as field.</param>
        /// <param name="options">The options.</param>
        /// <param name="translationOptions">The translation options.</param>
        /// <returns>The stage.</returns>
        public static PipelineStageDefinition<TInput, TOutput> GraphLookup<TInput, TFrom, TConnectFrom, TConnectTo, TStartWith, TAs, TOutput>(
            IMongoCollection<TFrom> from,
            Expression<Func<TFrom, TConnectFrom>> connectFromField,
            Expression<Func<TFrom, TConnectTo>> connectToField,
            Expression<Func<TInput, TStartWith>> startWith,
            Expression<Func<TOutput, TAs>> @as,
            AggregateGraphLookupOptions<TFrom, TFrom, TOutput> options = null,
            ExpressionTranslationOptions translationOptions = null)
                where TAs : IEnumerable<TFrom>
        {
            Ensure.IsNotNull(connectFromField, nameof(connectFromField));
            Ensure.IsNotNull(connectToField, nameof(connectToField));
            Ensure.IsNotNull(startWith, nameof(startWith));
            Ensure.IsNotNull(@as, nameof(@as));
            return GraphLookup(
                from,
                new ExpressionFieldDefinition<TFrom, TConnectFrom>(connectFromField),
                new ExpressionFieldDefinition<TFrom, TConnectTo>(connectToField),
                new ExpressionAggregateExpressionDefinition<TInput, TStartWith>(startWith, translationOptions),
                new ExpressionFieldDefinition<TOutput, TAs>(@as),
                options);
        }

        /// <summary>
        /// Creates a $graphLookup stage.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TFrom">The type of the from documents.</typeparam>
        /// <typeparam name="TConnectFrom">The type of the connect from field (must be either TConnectTo or a type that implements IEnumerable{TConnectTo}).</typeparam>
        /// <typeparam name="TConnectTo">The type of the connect to field.</typeparam>
        /// <typeparam name="TStartWith">The type of the start with expression (must be either TConnectTo or a type that implements IEnumerable{TConnectTo}).</typeparam>
        /// <typeparam name="TAsElement">The type of the as field elements.</typeparam>
        /// <typeparam name="TAs">The type of the as field.</typeparam>
        /// <typeparam name="TOutput">The type of the output documents.</typeparam>
        /// <param name="from">The from collection.</param>
        /// <param name="connectFromField">The connect from field.</param>
        /// <param name="connectToField">The connect to field.</param>
        /// <param name="startWith">The start with value.</param>
        /// <param name="as">The as field.</param>
        /// <param name="depthField">The depth field.</param>
        /// <param name="options">The options.</param>
        /// <param name="translationOptions">The translation options.</param>
        /// <returns>The stage.</returns>
        public static PipelineStageDefinition<TInput, TOutput> GraphLookup<TInput, TFrom, TConnectFrom, TConnectTo, TStartWith, TAsElement, TAs, TOutput>(
            IMongoCollection<TFrom> from,
            Expression<Func<TFrom, TConnectFrom>> connectFromField,
            Expression<Func<TFrom, TConnectTo>> connectToField,
            Expression<Func<TInput, TStartWith>> startWith,
            Expression<Func<TOutput, TAs>> @as,
            Expression<Func<TAsElement, int>> depthField,
            AggregateGraphLookupOptions<TFrom, TAsElement, TOutput> options = null,
            ExpressionTranslationOptions translationOptions = null)
                where TAs : IEnumerable<TAsElement>
        {
            Ensure.IsNotNull(connectFromField, nameof(connectFromField));
            Ensure.IsNotNull(connectToField, nameof(connectToField));
            Ensure.IsNotNull(startWith, nameof(startWith));
            Ensure.IsNotNull(@as, nameof(@as));
            Ensure.IsNotNull(depthField, nameof(depthField));
            return GraphLookup(
                from,
                new ExpressionFieldDefinition<TFrom, TConnectFrom>(connectFromField),
                new ExpressionFieldDefinition<TFrom, TConnectTo>(connectToField),
                new ExpressionAggregateExpressionDefinition<TInput, TStartWith>(startWith, translationOptions),
                new ExpressionFieldDefinition<TOutput, TAs>(@as),
                new ExpressionFieldDefinition<TAsElement, int>(depthField),
                options);
        }

        /// <summary>
        /// Creates a $group stage.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TOutput">The type of the output documents.</typeparam>
        /// <param name="group">The group projection.</param>
        /// <returns>The stage.</returns>
        public static PipelineStageDefinition<TInput, TOutput> Group<TInput, TOutput>(
            ProjectionDefinition<TInput, TOutput> group)
        {
            Ensure.IsNotNull(group, nameof(group));

            const string operatorName = "$group";
            var stage = new DelegatedPipelineStageDefinition<TInput, TOutput>(
                operatorName,
                (s, sr) =>
                {
                    var renderedProjection = group.Render(s, sr);
                    return new RenderedPipelineStageDefinition<TOutput>(operatorName, new BsonDocument(operatorName, renderedProjection.Document), renderedProjection.ProjectionSerializer);
                });

            return stage;
        }

        /// <summary>
        /// Creates a $group stage.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <param name="group">The group projection.</param>
        /// <returns>The stage.</returns>
        public static PipelineStageDefinition<TInput, BsonDocument> Group<TInput>(
            ProjectionDefinition<TInput, BsonDocument> group)
        {
            return Group<TInput, BsonDocument>(group);
        }

        /// <summary>
        /// Creates a $group stage.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TValue">The type of the values.</typeparam>
        /// <typeparam name="TOutput">The type of the output documents.</typeparam>
        /// <param name="value">The value field.</param>
        /// <param name="group">The group projection.</param>
        /// <param name="translationOptions">The translation options.</param>
        /// <returns>The stage.</returns>
        public static PipelineStageDefinition<TInput, TOutput> Group<TInput, TValue, TOutput>(
            Expression<Func<TInput, TValue>> value,
            Expression<Func<IGrouping<TValue, TInput>, TOutput>> group,
            ExpressionTranslationOptions translationOptions = null)
        {
            Ensure.IsNotNull(value, nameof(value));
            Ensure.IsNotNull(group, nameof(group));
            return Group(new GroupExpressionProjection<TInput, TValue, TOutput>(value, group, translationOptions));
        }

        /// <summary>
        /// Creates a $limit stage.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <param name="limit">The limit.</param>
        /// <returns>The stage.</returns>
        public static PipelineStageDefinition<TInput, TInput> Limit<TInput>(
            int limit)
        {
            Ensure.IsGreaterThanZero(limit, nameof(limit));
            return new BsonDocumentPipelineStageDefinition<TInput, TInput>(new BsonDocument("$limit", limit));
        }

        /// <summary>
        /// Creates a $lookup stage.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TForeignDocument">The type of the foreign collection documents.</typeparam>
        /// <typeparam name="TOutput">The type of the output documents.</typeparam>
        /// <param name="foreignCollection">The foreign collection.</param>
        /// <param name="localField">The local field.</param>
        /// <param name="foreignField">The foreign field.</param>
        /// <param name="as">The "as" field.</param>
        /// <param name="options">The options.</param>
        /// <returns>The stage.</returns>
        public static PipelineStageDefinition<TInput, TOutput> Lookup<TInput, TForeignDocument, TOutput>(
            IMongoCollection<TForeignDocument> foreignCollection,
            FieldDefinition<TInput> localField,
            FieldDefinition<TForeignDocument> foreignField,
            FieldDefinition<TOutput> @as,
            AggregateLookupOptions<TForeignDocument, TOutput> options = null)
        {
            Ensure.IsNotNull(foreignCollection, nameof(foreignCollection));
            Ensure.IsNotNull(localField, nameof(localField));
            Ensure.IsNotNull(foreignField, nameof(foreignField));
            Ensure.IsNotNull(@as, nameof(@as));

            options = options ?? new AggregateLookupOptions<TForeignDocument, TOutput>();
            const string operatorName = "$lookup";
            var stage = new DelegatedPipelineStageDefinition<TInput, TOutput>(
                operatorName,
                (inputSerializer, sr) =>
                {
                    var foreignSerializer = options.ForeignSerializer ?? (inputSerializer as IBsonSerializer<TForeignDocument>) ?? sr.GetSerializer<TForeignDocument>();
                    var outputSerializer = options.ResultSerializer ?? (inputSerializer as IBsonSerializer<TOutput>) ?? sr.GetSerializer<TOutput>();
                    return new RenderedPipelineStageDefinition<TOutput>(
                        operatorName, new BsonDocument(operatorName, new BsonDocument
                        {
                            { "from", foreignCollection.CollectionNamespace.CollectionName },
                            { "localField", localField.Render(inputSerializer, sr).FieldName },
                            { "foreignField", foreignField.Render(foreignSerializer, sr).FieldName },
                            { "as", @as.Render(outputSerializer, sr).FieldName }
                        }),
                        outputSerializer);
                });

            return stage;
        }

        /// <summary>
        /// Creates a $lookup stage.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TForeignDocument">The type of the foreign collection documents.</typeparam>
        /// <typeparam name="TOutput">The type of the output documents.</typeparam>
        /// <param name="foreignCollection">The foreign collection.</param>
        /// <param name="localField">The local field.</param>
        /// <param name="foreignField">The foreign field.</param>
        /// <param name="as">The "as" field.</param>
        /// <param name="options">The options.</param>
        /// <returns>The stage.</returns>
        public static PipelineStageDefinition<TInput, TOutput> Lookup<TInput, TForeignDocument, TOutput>(
            IMongoCollection<TForeignDocument> foreignCollection,
            Expression<Func<TInput, object>> localField,
            Expression<Func<TForeignDocument, object>> foreignField,
            Expression<Func<TOutput, object>> @as,
            AggregateLookupOptions<TForeignDocument, TOutput> options = null)
        {
            Ensure.IsNotNull(localField, nameof(localField));
            Ensure.IsNotNull(foreignField, nameof(foreignField));
            Ensure.IsNotNull(@as, nameof(@as));
            return Lookup(
                foreignCollection,
                new ExpressionFieldDefinition<TInput>(localField),
                new ExpressionFieldDefinition<TForeignDocument>(foreignField),
                new ExpressionFieldDefinition<TOutput>(@as),
                options);
        }

        /// <summary>
        /// Creates a $match stage.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <param name="filter">The filter.</param>
        /// <returns>The stage.</returns>
        public static PipelineStageDefinition<TInput, TInput> Match<TInput>(
            FilterDefinition<TInput> filter)
        {
            Ensure.IsNotNull(filter, nameof(filter));

            const string operatorName = "$match";
            var stage = new DelegatedPipelineStageDefinition<TInput, TInput>(
                operatorName,
                (s, sr) => new RenderedPipelineStageDefinition<TInput>(operatorName, new BsonDocument(operatorName, filter.Render(s, sr)), s));

            return stage;
        }

        /// <summary>
        /// Creates a $match stage.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <param name="filter">The filter.</param>
        /// <returns>The stage.</returns>
        public static PipelineStageDefinition<TInput, TInput> Match<TInput>(
            Expression<Func<TInput, bool>> filter)
        {
            Ensure.IsNotNull(filter, nameof(filter));
            return Match(new ExpressionFilterDefinition<TInput>(filter));
        }

        /// <summary>
        /// Create a $match stage that select documents of a sub type.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TOutput">The type of the output documents.</typeparam>
        /// <param name="outputSerializer">The output serializer.</param>
        /// <returns>The stage.</returns>
        public static PipelineStageDefinition<TInput, TOutput> OfType<TInput, TOutput>(
            IBsonSerializer<TOutput> outputSerializer = null)
                where TOutput : TInput
        {
            var discriminatorConvention = BsonSerializer.LookupDiscriminatorConvention(typeof(TOutput));
            if (discriminatorConvention == null)
            {
                var message = string.Format("OfType requires that a discriminator convention exist for type: {0}.", BsonUtils.GetFriendlyTypeName(typeof(TOutput)));
                throw new NotSupportedException(message);
            }

            var discriminatorValue = discriminatorConvention.GetDiscriminator(typeof(TInput), typeof(TOutput));
            var ofTypeFilter = new BsonDocument(discriminatorConvention.ElementName, discriminatorValue);

            const string operatorName = "$match";
            var stage = new DelegatedPipelineStageDefinition<TInput, TOutput>(
                operatorName,
                (s, sr) =>
                {
                    return new RenderedPipelineStageDefinition<TOutput>(
                        operatorName,
                        new BsonDocument(operatorName, ofTypeFilter),
                        outputSerializer ?? (s as IBsonSerializer<TOutput>) ?? sr.GetSerializer<TOutput>());
                });

            return stage;
        }

        /// <summary>
        /// Creates a $out stage.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <param name="outputCollection">The output collection.</param>
        /// <returns>The stage.</returns>
        public static PipelineStageDefinition<TInput, TInput> Out<TInput>(
            IMongoCollection<TInput> outputCollection)
        {
            Ensure.IsNotNull(outputCollection, nameof(outputCollection));
            return new BsonDocumentPipelineStageDefinition<TInput, TInput>(new BsonDocument("$out", outputCollection.CollectionNamespace.CollectionName));
        }

        /// <summary>
        /// Creates a $project stage.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TOutput">The type of the output documents.</typeparam>
        /// <param name="projection">The projection.</param>
        /// <returns>The stage.</returns>
        public static PipelineStageDefinition<TInput, TOutput> Project<TInput, TOutput>(
            ProjectionDefinition<TInput, TOutput> projection)
        {
            Ensure.IsNotNull(projection, nameof(projection));

            const string operatorName = "$project";
            var stage = new DelegatedPipelineStageDefinition<TInput, TOutput>(
                operatorName,
                (s, sr) =>
                {
                    var renderedProjection = projection.Render(s, sr);
                    BsonDocument document;
                    if (renderedProjection.Document == null)
                    {
                        document = new BsonDocument();
                    }
                    else
                    {
                        document = new BsonDocument(operatorName, renderedProjection.Document);
                    }
                    return new RenderedPipelineStageDefinition<TOutput>(operatorName, document, renderedProjection.ProjectionSerializer);
                });

            return stage;
        }

        /// <summary>
        /// Creates a $project stage.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <param name="projection">The projection.</param>
        /// <returns>The stage.</returns>
        public static PipelineStageDefinition<TInput, BsonDocument> Project<TInput>(
            ProjectionDefinition<TInput, BsonDocument> projection)
        {
            return Project<TInput, BsonDocument>(projection);
        }

        /// <summary>
        /// Creates a $project stage.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TOutput">The type of the output documents.</typeparam>
        /// <param name="projection">The projection.</param>
        /// <param name="translationOptions">The translation options.</param>
        /// <returns>The stage.</returns>
        public static PipelineStageDefinition<TInput, TOutput> Project<TInput, TOutput>(
            Expression<Func<TInput, TOutput>> projection,
            ExpressionTranslationOptions translationOptions = null)
        {
            Ensure.IsNotNull(projection, nameof(projection));
            return Project(new ProjectExpressionProjection<TInput, TOutput>(projection, translationOptions));
        }

        /// <summary>
        /// Creates a $replaceRoot stage.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TOutput">The type of the output documents.</typeparam>
        /// <param name="newRoot">The new root.</param>
        /// <returns>The stage.</returns>
        public static PipelineStageDefinition<TInput, TOutput> ReplaceRoot<TInput, TOutput>(
            AggregateExpressionDefinition<TInput, TOutput> newRoot)
        {
            Ensure.IsNotNull(newRoot, nameof(newRoot));

            const string operatorName = "$replaceRoot";
            var stage = new DelegatedPipelineStageDefinition<TInput, TOutput>(
                operatorName,
                (s, sr) =>
                {
                    var document = new BsonDocument(operatorName, new BsonDocument("newRoot", newRoot.Render(s, sr)));
                    var outputSerializer = sr.GetSerializer<TOutput>();
                    return new RenderedPipelineStageDefinition<TOutput>(operatorName, document, outputSerializer);
                });

            return stage;
        }

        /// <summary>
        /// Creates a $replaceRoot stage.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TOutput">The type of the output documents.</typeparam>
        /// <param name="newRoot">The new root.</param>
        /// <param name="translationOptions">The translation options.</param>
        /// <returns>The stage.</returns>
        public static PipelineStageDefinition<TInput, TOutput> ReplaceRoot<TInput, TOutput>(
            Expression<Func<TInput, TOutput>> newRoot,
            ExpressionTranslationOptions translationOptions = null)
        {
            Ensure.IsNotNull(newRoot, nameof(newRoot));
            return ReplaceRoot(new ExpressionAggregateExpressionDefinition<TInput, TOutput>(newRoot, translationOptions));
        }

        /// <summary>
        /// Creates a $skip stage.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <param name="skip">The skip.</param>
        /// <returns>The stage.</returns>
        public static PipelineStageDefinition<TInput, TInput> Skip<TInput>(
            int skip)
        {
            Ensure.IsGreaterThanOrEqualToZero(skip, nameof(skip));
            return new BsonDocumentPipelineStageDefinition<TInput, TInput>(new BsonDocument("$skip", skip));
        }

        /// <summary>
        /// Creates a $sort stage.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <param name="sort">The sort.</param>
        /// <returns>The stage.</returns>
        public static PipelineStageDefinition<TInput, TInput> Sort<TInput>(
            SortDefinition<TInput> sort)
        {
            Ensure.IsNotNull(sort, nameof(sort));
            return new SortPipelineStageDefinition<TInput>(sort);
        }

        /// <summary>
        /// Creates a $sortByCount stage.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TValue">The type of the values.</typeparam>
        /// <param name="value">The value expression.</param>
        /// <returns>The stage.</returns>
        public static PipelineStageDefinition<TInput, AggregateSortByCountResult<TValue>> SortByCount<TInput, TValue>(
            AggregateExpressionDefinition<TInput, TValue> value)
        {
            Ensure.IsNotNull(value, nameof(value));

            const string operatorName = "$sortByCount";
            var stage = new DelegatedPipelineStageDefinition<TInput, AggregateSortByCountResult<TValue>>(
                operatorName,
                (s, sr) =>
                {
                    var outputSerializer = sr.GetSerializer<AggregateSortByCountResult<TValue>>();
                    return new RenderedPipelineStageDefinition<AggregateSortByCountResult<TValue>>(operatorName, new BsonDocument(operatorName, value.Render(s, sr)), outputSerializer);
                });

            return stage;
        }

        /// <summary>
        /// Creates a $sortByCount stage.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TValue">The type of the values.</typeparam>
        /// <param name="value">The value.</param>
        /// <param name="translationOptions">The translation options.</param>
        /// <returns>The stage.</returns>
        public static PipelineStageDefinition<TInput, AggregateSortByCountResult<TValue>> SortByCount<TInput, TValue>(
            Expression<Func<TInput, TValue>> value,
            ExpressionTranslationOptions translationOptions = null)
        {
            Ensure.IsNotNull(value, nameof(value));
            return SortByCount(new ExpressionAggregateExpressionDefinition<TInput, TValue>(value, translationOptions));
        }

        /// <summary>
        /// Creates an $unwind stage.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TOutput">The type of the output documents.</typeparam>
        /// <param name="field">The field.</param>
        /// <param name="options">The options.</param>
        /// <returns>The stage.</returns>
        public static PipelineStageDefinition<TInput, TOutput> Unwind<TInput, TOutput>(
            FieldDefinition<TInput> field,
            AggregateUnwindOptions<TOutput> options = null)
        {
            Ensure.IsNotNull(field, nameof(field));
            options = options ?? new AggregateUnwindOptions<TOutput>();

            const string operatorName = "$unwind";
            var stage = new DelegatedPipelineStageDefinition<TInput, TOutput>(
                operatorName,
                (s, sr) =>
                {
                    var outputSerializer = options.ResultSerializer ?? (s as IBsonSerializer<TOutput>) ?? sr.GetSerializer<TOutput>();

                    var fieldName = "$" + field.Render(s, sr).FieldName;
                    string includeArrayIndexFieldName = null;
                    if (options.IncludeArrayIndex != null)
                    {
                        includeArrayIndexFieldName = options.IncludeArrayIndex.Render(outputSerializer, sr).FieldName;
                    }

                    BsonValue value = fieldName;
                    if (options.PreserveNullAndEmptyArrays.HasValue || includeArrayIndexFieldName != null)
                    {
                        value = new BsonDocument
                        {
                            { "path", fieldName },
                            { "preserveNullAndEmptyArrays", options.PreserveNullAndEmptyArrays, options.PreserveNullAndEmptyArrays.HasValue },
                            { "includeArrayIndex", includeArrayIndexFieldName, includeArrayIndexFieldName != null }
                        };
                    }
                    return new RenderedPipelineStageDefinition<TOutput>(
                        operatorName,
                        new BsonDocument(operatorName, value),
                        outputSerializer);
                });

            return stage;
        }

        /// <summary>
        /// Creates an $unwind stage.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <param name="field">The field to unwind.</param>
        /// <param name="options">The options.</param>
        /// <returns>The stage.</returns>
        public static PipelineStageDefinition<TInput, BsonDocument> Unwind<TInput>(
            FieldDefinition<TInput> field,
            AggregateUnwindOptions<BsonDocument> options = null)
        {
            return Unwind<TInput, BsonDocument>(field, options);
        }

        /// <summary>
        /// Creates an $unwind stage.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <param name="field">The field to unwind.</param>
        /// <param name="options">The options.</param>
        /// <returns>The stage.</returns>
        public static PipelineStageDefinition<TInput, BsonDocument> Unwind<TInput>(
            Expression<Func<TInput, object>> field,
            AggregateUnwindOptions<BsonDocument> options = null)
        {
            Ensure.IsNotNull(field, nameof(field));
            return Unwind(new ExpressionFieldDefinition<TInput>(field), options);
        }

        /// <summary>
        /// Creates an $unwind stage.
        /// </summary>
        /// <typeparam name="TInput">The type of the input documents.</typeparam>
        /// <typeparam name="TOutput">The type of the output documents.</typeparam>
        /// <param name="field">The field to unwind.</param>
        /// <param name="options">The options.</param>
        /// <returns>The stage.</returns>
        public static PipelineStageDefinition<TInput, TOutput> Unwind<TInput, TOutput>(
            Expression<Func<TInput, object>> field,
            AggregateUnwindOptions<TOutput> options = null)
        {
            Ensure.IsNotNull(field, nameof(field));
            return Unwind(new ExpressionFieldDefinition<TInput>(field), options);
        }

        // private methods
        private static bool IsTConnectToOrEnumerableTConnectTo<TConnectFrom, TConnectTo>()
        {
            if (typeof(TConnectFrom) == typeof(TConnectTo))
            {
                return true;
            }

            var ienumerableTConnectTo = typeof(IEnumerable<>).MakeGenericType(typeof(TConnectTo));
            if (typeof(TConnectFrom).GetTypeInfo().GetInterfaces().Contains(ienumerableTConnectTo))
            {
                return true;
            }

            return false;
        }
    }

    internal sealed class ExpressionBucketOutputProjection<TInput, TValue, TOutput> : ProjectionDefinition<TInput, TOutput>
    {
        private readonly Expression<Func<IGrouping<TValue, TInput>, TOutput>> _outputExpression;
        private readonly ExpressionTranslationOptions _translationOptions;
        private readonly Expression<Func<TInput, TValue>> _valueExpression;

        public ExpressionBucketOutputProjection(
            Expression<Func<TInput, TValue>> valueExpression,
            Expression<Func<IGrouping<TValue, TInput>, TOutput>> outputExpression,
            ExpressionTranslationOptions translationOptions)
        {
            _valueExpression = Ensure.IsNotNull(valueExpression, nameof(valueExpression));
            _outputExpression = Ensure.IsNotNull(outputExpression, nameof(outputExpression));
            _translationOptions = translationOptions; // can be null

        }

        public Expression<Func<IGrouping<TValue, TInput>, TOutput>> OutputExpression
        {
            get { return _outputExpression; }
        }

        public override RenderedProjectionDefinition<TOutput> Render(IBsonSerializer<TInput> documentSerializer, IBsonSerializerRegistry serializerRegistry)
        {
            var renderedOutput = AggregateGroupTranslator.Translate<TValue, TInput, TOutput>(_valueExpression, _outputExpression, documentSerializer, serializerRegistry, _translationOptions);
            var document = renderedOutput.Document;
            document.Remove("_id");
            return new RenderedProjectionDefinition<TOutput>(document, renderedOutput.ProjectionSerializer);
        }
    }

    internal sealed class GroupExpressionProjection<TInput, TKey, TOutput> : ProjectionDefinition<TInput, TOutput>
    {
        private readonly Expression<Func<TInput, TKey>> _idExpression;
        private readonly Expression<Func<IGrouping<TKey, TInput>, TOutput>> _groupExpression;
        private readonly ExpressionTranslationOptions _translationOptions;

        public GroupExpressionProjection(Expression<Func<TInput, TKey>> idExpression, Expression<Func<IGrouping<TKey, TInput>, TOutput>> groupExpression, ExpressionTranslationOptions translationOptions)
        {
            _idExpression = Ensure.IsNotNull(idExpression, nameof(idExpression));
            _groupExpression = Ensure.IsNotNull(groupExpression, nameof(groupExpression));
            _translationOptions = translationOptions; // can be null
        }

        public Expression<Func<TInput, TKey>> IdExpression
        {
            get { return _idExpression; }
        }

        public Expression<Func<IGrouping<TKey, TInput>, TOutput>> GroupExpression
        {
            get { return _groupExpression; }
        }

        public override RenderedProjectionDefinition<TOutput> Render(IBsonSerializer<TInput> documentSerializer, IBsonSerializerRegistry serializerRegistry)
        {
            return AggregateGroupTranslator.Translate<TKey, TInput, TOutput>(_idExpression, _groupExpression, documentSerializer, serializerRegistry, _translationOptions);
        }
    }

    internal sealed class ProjectExpressionProjection<TInput, TOutput> : ProjectionDefinition<TInput, TOutput>
    {
        private readonly Expression<Func<TInput, TOutput>> _expression;
        private readonly ExpressionTranslationOptions _translationOptions;

        public ProjectExpressionProjection(Expression<Func<TInput, TOutput>> expression, ExpressionTranslationOptions translationOptions)
        {
            _expression = Ensure.IsNotNull(expression, nameof(expression));
            _translationOptions = translationOptions; // can be null
        }

        public Expression<Func<TInput, TOutput>> Expression
        {
            get { return _expression; }
        }

        public override RenderedProjectionDefinition<TOutput> Render(IBsonSerializer<TInput> inputSerializer, IBsonSerializerRegistry serializerRegistry)
        {
            return AggregateProjectTranslator.Translate<TInput, TOutput>(_expression, inputSerializer, serializerRegistry, _translationOptions);
        }
    }

    internal class SortPipelineStageDefinition<TInput> : PipelineStageDefinition<TInput, TInput>
    {
        public SortPipelineStageDefinition(SortDefinition<TInput> sort)
        {
            Sort = sort;
        }

        public SortDefinition<TInput> Sort { get; private set; }

        public override string OperatorName => "$sort";

        public override RenderedPipelineStageDefinition<TInput> Render(IBsonSerializer<TInput> inputSerializer, IBsonSerializerRegistry serializerRegistry)
        {
            var renderedSort = Sort.Render(inputSerializer, serializerRegistry);
            var document = new BsonDocument(OperatorName, renderedSort);
            return new RenderedPipelineStageDefinition<TInput>(OperatorName, document, inputSerializer);
        }
    }
}

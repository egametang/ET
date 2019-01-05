/* Copyright 2010-present MongoDB Inc.
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
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Linq;

namespace MongoDB.Driver
{
    /// <summary>
    /// Extension methods for <see cref="IMongoCollection{T}"/>.
    /// </summary>
    public static class IMongoCollectionExtensions
    {
        /// <summary>
        /// Begins a fluent aggregation interface.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="options">The options.</param>
        /// <returns>
        /// A fluent aggregate interface.
        /// </returns>
        public static IAggregateFluent<TDocument> Aggregate<TDocument>(this IMongoCollection<TDocument> collection, AggregateOptions options = null)
        {
            var emptyPipeline = new EmptyPipelineDefinition<TDocument>(collection.DocumentSerializer);
            return new AggregateFluent<TDocument, TDocument>(null, collection, emptyPipeline, options ?? new AggregateOptions());
        }

        /// <summary>
        /// Begins a fluent aggregation interface.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="session">The session.</param>
        /// <param name="options">The options.</param>
        /// <returns>
        /// A fluent aggregate interface.
        /// </returns>
        public static IAggregateFluent<TDocument> Aggregate<TDocument>(this IMongoCollection<TDocument> collection, IClientSessionHandle session, AggregateOptions options = null)
        {
            Ensure.IsNotNull(session, nameof(session));
            var emptyPipeline = new EmptyPipelineDefinition<TDocument>(collection.DocumentSerializer);
            return new AggregateFluent<TDocument, TDocument>(session, collection, emptyPipeline, options ?? new AggregateOptions());
        }

        /// <summary>
        /// Creates a queryable source of documents.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="aggregateOptions">The aggregate options</param>
        /// <returns>A queryable source of documents.</returns>
        public static IMongoQueryable<TDocument> AsQueryable<TDocument>(this IMongoCollection<TDocument> collection, AggregateOptions aggregateOptions = null)
        {
            Ensure.IsNotNull(collection, nameof(collection));
            
            aggregateOptions = aggregateOptions ?? new AggregateOptions();
            var provider = new MongoQueryProviderImpl<TDocument>(collection, aggregateOptions);
            return new MongoQueryableImpl<TDocument, TDocument>(provider);
        }

        /// <summary>
        /// Counts the number of documents in the collection.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The number of documents in the collection.
        /// </returns>
        [Obsolete("Use CountDocuments or EstimatedDocumentCount instead.")]
        public static long Count<TDocument>(this IMongoCollection<TDocument> collection, Expression<Func<TDocument, bool>> filter, CountOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(filter, nameof(filter));

            return collection.Count(new ExpressionFilterDefinition<TDocument>(filter), options, cancellationToken);
        }

        /// <summary>
        /// Counts the number of documents in the collection.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="session">The session.</param>
        /// <param name="collection">The collection.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The number of documents in the collection.
        /// </returns>
        [Obsolete("Use CountDocuments or EstimatedDocumentCount instead.")]
        public static long Count<TDocument>(this IMongoCollection<TDocument> collection, IClientSessionHandle session, Expression<Func<TDocument, bool>> filter, CountOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(session, nameof(session));
            Ensure.IsNotNull(filter, nameof(filter));

            return collection.Count(session, new ExpressionFilterDefinition<TDocument>(filter), options, cancellationToken);
        }

        /// <summary>
        /// Counts the number of documents in the collection.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The number of documents in the collection.
        /// </returns>
        [Obsolete("Use CountDocumentsAsync or EstimatedDocumentCountAsync instead.")]
        public static Task<long> CountAsync<TDocument>(this IMongoCollection<TDocument> collection, Expression<Func<TDocument, bool>> filter, CountOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(filter, nameof(filter));

            return collection.CountAsync(new ExpressionFilterDefinition<TDocument>(filter), options, cancellationToken);
        }

        /// <summary>
        /// Counts the number of documents in the collection.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="session">The session.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The number of documents in the collection.
        /// </returns>
        [Obsolete("Use CountDocumentsAsync or EstimatedDocumentCountAsync instead.")]
        public static Task<long> CountAsync<TDocument>(this IMongoCollection<TDocument> collection, IClientSessionHandle session, Expression<Func<TDocument, bool>> filter, CountOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(session, nameof(session));
            Ensure.IsNotNull(filter, nameof(filter));

            return collection.CountAsync(session, new ExpressionFilterDefinition<TDocument>(filter), options, cancellationToken);
        }

        /// <summary>
        /// Counts the number of documents in the collection.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The number of documents in the collection.
        /// </returns>
        public static long CountDocuments<TDocument>(this IMongoCollection<TDocument> collection, Expression<Func<TDocument, bool>> filter, CountOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(filter, nameof(filter));

            return collection.CountDocuments(new ExpressionFilterDefinition<TDocument>(filter), options, cancellationToken);
        }

        /// <summary>
        /// Counts the number of documents in the collection.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="session">The session.</param>
        /// <param name="collection">The collection.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The number of documents in the collection.
        /// </returns>
        public static long CountDocuments<TDocument>(this IMongoCollection<TDocument> collection, IClientSessionHandle session, Expression<Func<TDocument, bool>> filter, CountOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(session, nameof(session));
            Ensure.IsNotNull(filter, nameof(filter));

            return collection.CountDocuments(session, new ExpressionFilterDefinition<TDocument>(filter), options, cancellationToken);
        }

        /// <summary>
        /// Counts the number of documents in the collection.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The number of documents in the collection.
        /// </returns>
        public static Task<long> CountDocumentsAsync<TDocument>(this IMongoCollection<TDocument> collection, Expression<Func<TDocument, bool>> filter, CountOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(filter, nameof(filter));

            return collection.CountDocumentsAsync(new ExpressionFilterDefinition<TDocument>(filter), options, cancellationToken);
        }

        /// <summary>
        /// Counts the number of documents in the collection.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="session">The session.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The number of documents in the collection.
        /// </returns>
        public static Task<long> CountDocumentsAsync<TDocument>(this IMongoCollection<TDocument> collection, IClientSessionHandle session, Expression<Func<TDocument, bool>> filter, CountOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(session, nameof(session));
            Ensure.IsNotNull(filter, nameof(filter));

            return collection.CountDocumentsAsync(session, new ExpressionFilterDefinition<TDocument>(filter), options, cancellationToken);
        }

        /// <summary>
        /// Deletes multiple documents.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The result of the delete operation.
        /// </returns>
        public static DeleteResult DeleteMany<TDocument>(this IMongoCollection<TDocument> collection, Expression<Func<TDocument, bool>> filter, CancellationToken cancellationToken = default(CancellationToken))
        {
            return collection.DeleteMany(new ExpressionFilterDefinition<TDocument>(filter), cancellationToken);
        }

        /// <summary>
        /// Deletes multiple documents.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The result of the delete operation.
        /// </returns>
        public static DeleteResult DeleteMany<TDocument>(this IMongoCollection<TDocument> collection, Expression<Func<TDocument, bool>> filter, DeleteOptions options, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(filter, nameof(filter));

            return collection.DeleteMany(new ExpressionFilterDefinition<TDocument>(filter), options, cancellationToken);
        }

        /// <summary>
        /// Deletes multiple documents.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="session">The session.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The result of the delete operation.
        /// </returns>
        public static DeleteResult DeleteMany<TDocument>(this IMongoCollection<TDocument> collection, IClientSessionHandle session, Expression<Func<TDocument, bool>> filter, DeleteOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(session, nameof(session));
            Ensure.IsNotNull(filter, nameof(filter));

            return collection.DeleteMany(session, new ExpressionFilterDefinition<TDocument>(filter), options, cancellationToken);
        }

        /// <summary>
        /// Deletes multiple documents.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The result of the delete operation.
        /// </returns>
        public static Task<DeleteResult> DeleteManyAsync<TDocument>(this IMongoCollection<TDocument> collection, Expression<Func<TDocument, bool>> filter, CancellationToken cancellationToken = default(CancellationToken))
        {
            return collection.DeleteManyAsync(new ExpressionFilterDefinition<TDocument>(filter), cancellationToken);
        }

        /// <summary>
        /// Deletes multiple documents.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The result of the delete operation.
        /// </returns>
        public static Task<DeleteResult> DeleteManyAsync<TDocument>(this IMongoCollection<TDocument> collection, Expression<Func<TDocument, bool>> filter, DeleteOptions options, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(filter, nameof(filter));

            return collection.DeleteManyAsync(new ExpressionFilterDefinition<TDocument>(filter), options, cancellationToken);
        }

        /// <summary>
        /// Deletes multiple documents.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="session">The session.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The result of the delete operation.
        /// </returns>
        public static Task<DeleteResult> DeleteManyAsync<TDocument>(this IMongoCollection<TDocument> collection, IClientSessionHandle session, Expression<Func<TDocument, bool>> filter, DeleteOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(session, nameof(session));
            Ensure.IsNotNull(filter, nameof(filter));

            return collection.DeleteManyAsync(session, new ExpressionFilterDefinition<TDocument>(filter), options, cancellationToken);
        }

        /// <summary>
        /// Deletes a single document.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The result of the delete operation.
        /// </returns>
        public static DeleteResult DeleteOne<TDocument>(this IMongoCollection<TDocument> collection, Expression<Func<TDocument, bool>> filter, CancellationToken cancellationToken = default(CancellationToken))
        {
            return collection.DeleteOne(new ExpressionFilterDefinition<TDocument>(filter), cancellationToken);
        }

        /// <summary>
        /// Deletes a single document.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The result of the delete operation.
        /// </returns>
        public static DeleteResult DeleteOne<TDocument>(this IMongoCollection<TDocument> collection, Expression<Func<TDocument, bool>> filter, DeleteOptions options, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(filter, nameof(filter));

            return collection.DeleteOne(new ExpressionFilterDefinition<TDocument>(filter), options, cancellationToken);
        }

        /// <summary>
        /// Deletes a single document.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="session">The session.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The result of the delete operation.
        /// </returns>
        public static DeleteResult DeleteOne<TDocument>(this IMongoCollection<TDocument> collection, IClientSessionHandle session, Expression<Func<TDocument, bool>> filter, DeleteOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(session, nameof(session));
            Ensure.IsNotNull(filter, nameof(filter));

            return collection.DeleteOne(session, new ExpressionFilterDefinition<TDocument>(filter), options, cancellationToken);
        }

        /// <summary>
        /// Deletes a single document.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The result of the delete operation.
        /// </returns>
        public static Task<DeleteResult> DeleteOneAsync<TDocument>(this IMongoCollection<TDocument> collection, Expression<Func<TDocument, bool>> filter, CancellationToken cancellationToken = default(CancellationToken))
        {
            return collection.DeleteOneAsync(new ExpressionFilterDefinition<TDocument>(filter), cancellationToken);
        }

        /// <summary>
        /// Deletes a single document.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The result of the delete operation.
        /// </returns>
        public static Task<DeleteResult> DeleteOneAsync<TDocument>(this IMongoCollection<TDocument> collection, Expression<Func<TDocument, bool>> filter, DeleteOptions options, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(filter, nameof(filter));

            return collection.DeleteOneAsync(new ExpressionFilterDefinition<TDocument>(filter), options, cancellationToken);
        }

        /// <summary>
        /// Deletes a single document.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="session">The session.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The result of the delete operation.
        /// </returns>
        public static Task<DeleteResult> DeleteOneAsync<TDocument>(this IMongoCollection<TDocument> collection, IClientSessionHandle session, Expression<Func<TDocument, bool>> filter, DeleteOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(session, nameof(session));
            Ensure.IsNotNull(filter, nameof(filter));

            return collection.DeleteOneAsync(session, new ExpressionFilterDefinition<TDocument>(filter), options, cancellationToken);
        }

        /// <summary>
        /// Gets the distinct values for a specified field.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <typeparam name="TField">The type of the result.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="field">The field.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The distinct values for the specified field.
        /// </returns>
        public static IAsyncCursor<TField> Distinct<TDocument, TField>(this IMongoCollection<TDocument> collection, Expression<Func<TDocument, TField>> field, FilterDefinition<TDocument> filter, DistinctOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(field, nameof(field));
            Ensure.IsNotNull(filter, nameof(filter));

            return collection.Distinct(
                new ExpressionFieldDefinition<TDocument, TField>(field),
                filter,
                options,
                cancellationToken);
        }

        /// <summary>
        /// Gets the distinct values for a specified field.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <typeparam name="TField">The type of the result.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="field">The field.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The distinct values for the specified field.
        /// </returns>
        public static IAsyncCursor<TField> Distinct<TDocument, TField>(this IMongoCollection<TDocument> collection, FieldDefinition<TDocument, TField> field, Expression<Func<TDocument, bool>> filter, DistinctOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(field, nameof(field));
            Ensure.IsNotNull(filter, nameof(filter));

            return collection.Distinct(
                field,
                new ExpressionFilterDefinition<TDocument>(filter),
                options,
                cancellationToken);
        }

        /// <summary>
        /// Gets the distinct values for a specified field.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <typeparam name="TField">The type of the result.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="field">The field.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The distinct values for the specified field.
        /// </returns>
        public static IAsyncCursor<TField> Distinct<TDocument, TField>(this IMongoCollection<TDocument> collection, Expression<Func<TDocument, TField>> field, Expression<Func<TDocument, bool>> filter, DistinctOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(field, nameof(field));
            Ensure.IsNotNull(filter, nameof(filter));

            return collection.Distinct(
                new ExpressionFieldDefinition<TDocument, TField>(field),
                new ExpressionFilterDefinition<TDocument>(filter),
                options,
                cancellationToken);
        }

        /// <summary>
        /// Gets the distinct values for a specified field.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <typeparam name="TField">The type of the result.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="session">The session.</param>
        /// <param name="field">The field.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The distinct values for the specified field.
        /// </returns>
        public static IAsyncCursor<TField> Distinct<TDocument, TField>(this IMongoCollection<TDocument> collection, IClientSessionHandle session, Expression<Func<TDocument, TField>> field, FilterDefinition<TDocument> filter, DistinctOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(session, nameof(session));
            Ensure.IsNotNull(field, nameof(field));
            Ensure.IsNotNull(filter, nameof(filter));

            return collection.Distinct(
                session,
                new ExpressionFieldDefinition<TDocument, TField>(field),
                filter,
                options,
                cancellationToken);
        }

        /// <summary>
        /// Gets the distinct values for a specified field.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <typeparam name="TField">The type of the result.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="session">The session.</param>
        /// <param name="field">The field.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The distinct values for the specified field.
        /// </returns>
        public static IAsyncCursor<TField> Distinct<TDocument, TField>(this IMongoCollection<TDocument> collection, IClientSessionHandle session, FieldDefinition<TDocument, TField> field, Expression<Func<TDocument, bool>> filter, DistinctOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(session, nameof(session));
            Ensure.IsNotNull(field, nameof(field));
            Ensure.IsNotNull(filter, nameof(filter));

            return collection.Distinct(
                session,
                field,
                new ExpressionFilterDefinition<TDocument>(filter),
                options,
                cancellationToken);
        }

        /// <summary>
        /// Gets the distinct values for a specified field.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <typeparam name="TField">The type of the result.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="session">The session.</param>
        /// <param name="field">The field.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The distinct values for the specified field.
        /// </returns>
        public static IAsyncCursor<TField> Distinct<TDocument, TField>(this IMongoCollection<TDocument> collection, IClientSessionHandle session, Expression<Func<TDocument, TField>> field, Expression<Func<TDocument, bool>> filter, DistinctOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(session, nameof(session));
            Ensure.IsNotNull(field, nameof(field));
            Ensure.IsNotNull(filter, nameof(filter));

            return collection.Distinct(
                session,
                new ExpressionFieldDefinition<TDocument, TField>(field),
                new ExpressionFilterDefinition<TDocument>(filter),
                options,
                cancellationToken);
        }

        /// <summary>
        /// Gets the distinct values for a specified field.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <typeparam name="TField">The type of the result.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="field">The field.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The distinct values for the specified field.
        /// </returns>
        public static Task<IAsyncCursor<TField>> DistinctAsync<TDocument, TField>(this IMongoCollection<TDocument> collection, Expression<Func<TDocument, TField>> field, FilterDefinition<TDocument> filter, DistinctOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(field, nameof(field));
            Ensure.IsNotNull(filter, nameof(filter));

            return collection.DistinctAsync(
                new ExpressionFieldDefinition<TDocument, TField>(field),
                filter,
                options,
                cancellationToken);
        }

        /// <summary>
        /// Gets the distinct values for a specified field.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <typeparam name="TField">The type of the result.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="field">The field.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The distinct values for the specified field.
        /// </returns>
        public static Task<IAsyncCursor<TField>> DistinctAsync<TDocument, TField>(this IMongoCollection<TDocument> collection, FieldDefinition<TDocument, TField> field, Expression<Func<TDocument, bool>> filter, DistinctOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(field, nameof(field));
            Ensure.IsNotNull(filter, nameof(filter));

            return collection.DistinctAsync(
                field,
                new ExpressionFilterDefinition<TDocument>(filter),
                options,
                cancellationToken);
        }

        /// <summary>
        /// Gets the distinct values for a specified field.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <typeparam name="TField">The type of the result.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="field">The field.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The distinct values for the specified field.
        /// </returns>
        public static Task<IAsyncCursor<TField>> DistinctAsync<TDocument, TField>(this IMongoCollection<TDocument> collection, Expression<Func<TDocument, TField>> field, Expression<Func<TDocument, bool>> filter, DistinctOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(field, nameof(field));
            Ensure.IsNotNull(filter, nameof(filter));

            return collection.DistinctAsync(
                new ExpressionFieldDefinition<TDocument, TField>(field),
                new ExpressionFilterDefinition<TDocument>(filter),
                options,
                cancellationToken);
        }

        /// <summary>
        /// Gets the distinct values for a specified field.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <typeparam name="TField">The type of the result.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="session">The session.</param>
        /// <param name="field">The field.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The distinct values for the specified field.
        /// </returns>
        public static Task<IAsyncCursor<TField>> DistinctAsync<TDocument, TField>(this IMongoCollection<TDocument> collection, IClientSessionHandle session, Expression<Func<TDocument, TField>> field, FilterDefinition<TDocument> filter, DistinctOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(session, nameof(session));
            Ensure.IsNotNull(field, nameof(field));
            Ensure.IsNotNull(filter, nameof(filter));

            return collection.DistinctAsync(
                session,
                new ExpressionFieldDefinition<TDocument, TField>(field),
                filter,
                options,
                cancellationToken);
        }

        /// <summary>
        /// Gets the distinct values for a specified field.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <typeparam name="TField">The type of the result.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="session">The session.</param>
        /// <param name="field">The field.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The distinct values for the specified field.
        /// </returns>
        public static Task<IAsyncCursor<TField>> DistinctAsync<TDocument, TField>(this IMongoCollection<TDocument> collection, IClientSessionHandle session, FieldDefinition<TDocument, TField> field, Expression<Func<TDocument, bool>> filter, DistinctOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(session, nameof(session));
            Ensure.IsNotNull(field, nameof(field));
            Ensure.IsNotNull(filter, nameof(filter));

            return collection.DistinctAsync(
                session,
                field,
                new ExpressionFilterDefinition<TDocument>(filter),
                options,
                cancellationToken);
        }

        /// <summary>
        /// Gets the distinct values for a specified field.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <typeparam name="TField">The type of the result.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="session">The session.</param>
        /// <param name="field">The field.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The distinct values for the specified field.
        /// </returns>
        public static Task<IAsyncCursor<TField>> DistinctAsync<TDocument, TField>(this IMongoCollection<TDocument> collection, IClientSessionHandle session, Expression<Func<TDocument, TField>> field, Expression<Func<TDocument, bool>> filter, DistinctOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(session, nameof(session));
            Ensure.IsNotNull(field, nameof(field));
            Ensure.IsNotNull(filter, nameof(filter));

            return collection.DistinctAsync(
                session,
                new ExpressionFieldDefinition<TDocument, TField>(field),
                new ExpressionFilterDefinition<TDocument>(filter),
                options,
                cancellationToken);
        }

        /// <summary>
        /// Begins a fluent find interface.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="options">The options.</param>
        /// <returns>
        /// A fluent find interface.
        /// </returns>
        public static IFindFluent<TDocument, TDocument> Find<TDocument>(this IMongoCollection<TDocument> collection, FilterDefinition<TDocument> filter, FindOptions options = null)
        {
            return FindHelper(null, collection, filter, options);
        }

        /// <summary>
        /// Begins a fluent find interface.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="session">The session.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="options">The options.</param>
        /// <returns>
        /// A fluent find interface.
        /// </returns>
        public static IFindFluent<TDocument, TDocument> Find<TDocument>(this IMongoCollection<TDocument> collection, IClientSessionHandle session, FilterDefinition<TDocument> filter, FindOptions options = null)
        {
            Ensure.IsNotNull(session, nameof(session));
            return FindHelper(session, collection, filter, options);
        }

        private static IFindFluent<TDocument, TDocument> FindHelper<TDocument>(IClientSessionHandle session, IMongoCollection<TDocument> collection, FilterDefinition<TDocument> filter, FindOptions options)
        {
            FindOptions<TDocument, TDocument> genericOptions;
            if (options == null)
            {
                genericOptions = new FindOptions<TDocument>();
            }
            else
            {
                genericOptions = new FindOptions<TDocument>
                {
                    AllowPartialResults = options.AllowPartialResults,
                    BatchSize = options.BatchSize,
                    Collation = options.Collation,
                    Comment = options.Comment,
                    CursorType = options.CursorType,
                    MaxAwaitTime = options.MaxAwaitTime,
                    MaxTime = options.MaxTime,
                    Modifiers = options.Modifiers,
                    NoCursorTimeout = options.NoCursorTimeout,
                    OplogReplay = options.OplogReplay
                };
            }

            return new FindFluent<TDocument, TDocument>(session, collection, filter, genericOptions);
        }

        /// <summary>
        /// Begins a fluent find interface.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="options">The options.</param>
        /// <returns>
        /// A fluent interface.
        /// </returns>
        public static IFindFluent<TDocument, TDocument> Find<TDocument>(this IMongoCollection<TDocument> collection, Expression<Func<TDocument, bool>> filter, FindOptions options = null)
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(filter, nameof(filter));

            return collection.Find(new ExpressionFilterDefinition<TDocument>(filter), options);
        }

        /// <summary>
        /// Begins a fluent find interface.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="session">The session.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="options">The options.</param>
        /// <returns>
        /// A fluent interface.
        /// </returns>
        public static IFindFluent<TDocument, TDocument> Find<TDocument>(this IMongoCollection<TDocument> collection, IClientSessionHandle session, Expression<Func<TDocument, bool>> filter, FindOptions options = null)
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(session, nameof(session));
            Ensure.IsNotNull(filter, nameof(filter));

            return collection.Find(session, new ExpressionFilterDefinition<TDocument>(filter), options);
        }

        /// <summary>
        /// Finds the documents matching the filter.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task whose result is a cursor.</returns>
        public static IAsyncCursor<TDocument> FindSync<TDocument>(this IMongoCollection<TDocument> collection, FilterDefinition<TDocument> filter, FindOptions<TDocument, TDocument> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(filter, nameof(filter));

            return collection.FindSync<TDocument>(filter, options, cancellationToken);
        }

        /// <summary>
        /// Finds the documents matching the filter.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task whose result is a cursor.</returns>
        public static IAsyncCursor<TDocument> FindSync<TDocument>(this IMongoCollection<TDocument> collection, Expression<Func<TDocument, bool>> filter, FindOptions<TDocument, TDocument> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(filter, nameof(filter));

            return collection.FindSync<TDocument>(new ExpressionFilterDefinition<TDocument>(filter), options, cancellationToken);
        }

        /// <summary>
        /// Finds the documents matching the filter.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="session">The session.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A Task whose result is a cursor.
        /// </returns>
        public static IAsyncCursor<TDocument> FindSync<TDocument>(this IMongoCollection<TDocument> collection, IClientSessionHandle session, FilterDefinition<TDocument> filter, FindOptions<TDocument, TDocument> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(session, nameof(session));
            Ensure.IsNotNull(filter, nameof(filter));

            return collection.FindSync<TDocument>(session, filter, options, cancellationToken);
        }

        /// <summary>
        /// Finds the documents matching the filter.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="session">The session.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A Task whose result is a cursor.
        /// </returns>
        public static IAsyncCursor<TDocument> FindSync<TDocument>(this IMongoCollection<TDocument> collection, IClientSessionHandle session, Expression<Func<TDocument, bool>> filter, FindOptions<TDocument, TDocument> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(session, nameof(session));
            Ensure.IsNotNull(filter, nameof(filter));

            return collection.FindSync<TDocument>(session, new ExpressionFilterDefinition<TDocument>(filter), options, cancellationToken);
        }

        /// <summary>
        /// Finds the documents matching the filter.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task whose result is a cursor.</returns>
        public static Task<IAsyncCursor<TDocument>> FindAsync<TDocument>(this IMongoCollection<TDocument> collection, FilterDefinition<TDocument> filter, FindOptions<TDocument, TDocument> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(filter, nameof(filter));

            return collection.FindAsync<TDocument>(filter, options, cancellationToken);
        }

        /// <summary>
        /// Finds the documents matching the filter.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task whose result is a cursor.</returns>
        public static Task<IAsyncCursor<TDocument>> FindAsync<TDocument>(this IMongoCollection<TDocument> collection, Expression<Func<TDocument, bool>> filter, FindOptions<TDocument, TDocument> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(filter, nameof(filter));

            return collection.FindAsync<TDocument>(new ExpressionFilterDefinition<TDocument>(filter), options, cancellationToken);
        }

        /// <summary>
        /// Finds the documents matching the filter.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="session">The session.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A Task whose result is a cursor.
        /// </returns>
        public static Task<IAsyncCursor<TDocument>> FindAsync<TDocument>(this IMongoCollection<TDocument> collection, IClientSessionHandle session, FilterDefinition<TDocument> filter, FindOptions<TDocument, TDocument> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(session, nameof(session));
            Ensure.IsNotNull(filter, nameof(filter));

            return collection.FindAsync<TDocument>(session, filter, options, cancellationToken);
        }

        /// <summary>
        /// Finds the documents matching the filter.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="session">The session.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A Task whose result is a cursor.
        /// </returns>
        public static Task<IAsyncCursor<TDocument>> FindAsync<TDocument>(this IMongoCollection<TDocument> collection, IClientSessionHandle session, Expression<Func<TDocument, bool>> filter, FindOptions<TDocument, TDocument> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(session, nameof(session));
            Ensure.IsNotNull(filter, nameof(filter));

            return collection.FindAsync<TDocument>(session, new ExpressionFilterDefinition<TDocument>(filter), options, cancellationToken);
        }

        /// <summary>
        /// Finds a single document and deletes it atomically.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The deleted document if one was deleted.
        /// </returns>
        public static TDocument FindOneAndDelete<TDocument>(this IMongoCollection<TDocument> collection, FilterDefinition<TDocument> filter, FindOneAndDeleteOptions<TDocument, TDocument> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(filter, nameof(filter));

            return collection.FindOneAndDelete<TDocument>(filter, options, cancellationToken);
        }

        /// <summary>
        /// Finds a single document and deletes it atomically.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The deleted document if one was deleted.
        /// </returns>
        public static TDocument FindOneAndDelete<TDocument>(this IMongoCollection<TDocument> collection, Expression<Func<TDocument, bool>> filter, FindOneAndDeleteOptions<TDocument, TDocument> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(filter, nameof(filter));

            return collection.FindOneAndDelete<TDocument>(new ExpressionFilterDefinition<TDocument>(filter), options, cancellationToken);
        }

        /// <summary>
        /// Finds a single document and deletes it atomically.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <typeparam name="TProjection">The type of the projection (same as TDocument if there is no projection).</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The returned document.
        /// </returns>
        public static TProjection FindOneAndDelete<TDocument, TProjection>(this IMongoCollection<TDocument> collection, Expression<Func<TDocument, bool>> filter, FindOneAndDeleteOptions<TDocument, TProjection> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(filter, nameof(filter));

            return collection.FindOneAndDelete<TProjection>(new ExpressionFilterDefinition<TDocument>(filter), options, cancellationToken);
        }

        /// <summary>
        /// Finds a single document and deletes it atomically.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="session">The session.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The deleted document if one was deleted.
        /// </returns>
        public static TDocument FindOneAndDelete<TDocument>(this IMongoCollection<TDocument> collection, IClientSessionHandle session, FilterDefinition<TDocument> filter, FindOneAndDeleteOptions<TDocument, TDocument> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(session, nameof(session));
            Ensure.IsNotNull(filter, nameof(filter));

            return collection.FindOneAndDelete<TDocument>(session, filter, options, cancellationToken);
        }

        /// <summary>
        /// Finds a single document and deletes it atomically.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="session">The session.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The deleted document if one was deleted.
        /// </returns>
        public static TDocument FindOneAndDelete<TDocument>(this IMongoCollection<TDocument> collection, IClientSessionHandle session, Expression<Func<TDocument, bool>> filter, FindOneAndDeleteOptions<TDocument, TDocument> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(session, nameof(session));
            Ensure.IsNotNull(filter, nameof(filter));

            return collection.FindOneAndDelete<TDocument>(session, new ExpressionFilterDefinition<TDocument>(filter), options, cancellationToken);
        }

        /// <summary>
        /// Finds a single document and deletes it atomically.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <typeparam name="TProjection">The type of the projection (same as TDocument if there is no projection).</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="session">The session.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The returned document.
        /// </returns>
        public static TProjection FindOneAndDelete<TDocument, TProjection>(this IMongoCollection<TDocument> collection, IClientSessionHandle session, Expression<Func<TDocument, bool>> filter, FindOneAndDeleteOptions<TDocument, TProjection> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(session, nameof(session));
            Ensure.IsNotNull(filter, nameof(filter));

            return collection.FindOneAndDelete<TProjection>(session, new ExpressionFilterDefinition<TDocument>(filter), options, cancellationToken);
        }

        /// <summary>
        /// Finds a single document and deletes it atomically.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The deleted document if one was deleted.
        /// </returns>
        public static Task<TDocument> FindOneAndDeleteAsync<TDocument>(this IMongoCollection<TDocument> collection, FilterDefinition<TDocument> filter, FindOneAndDeleteOptions<TDocument, TDocument> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(filter, nameof(filter));

            return collection.FindOneAndDeleteAsync<TDocument>(filter, options, cancellationToken);
        }

        /// <summary>
        /// Finds a single document and deletes it atomically.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The deleted document if one was deleted.
        /// </returns>
        public static Task<TDocument> FindOneAndDeleteAsync<TDocument>(this IMongoCollection<TDocument> collection, Expression<Func<TDocument, bool>> filter, FindOneAndDeleteOptions<TDocument, TDocument> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(filter, nameof(filter));

            return collection.FindOneAndDeleteAsync<TDocument>(new ExpressionFilterDefinition<TDocument>(filter), options, cancellationToken);
        }

        /// <summary>
        /// Finds a single document and deletes it atomically.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <typeparam name="TProjection">The type of the projection (same as TDocument if there is no projection).</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The returned document.
        /// </returns>
        public static Task<TProjection> FindOneAndDeleteAsync<TDocument, TProjection>(this IMongoCollection<TDocument> collection, Expression<Func<TDocument, bool>> filter, FindOneAndDeleteOptions<TDocument, TProjection> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(filter, nameof(filter));

            return collection.FindOneAndDeleteAsync<TProjection>(new ExpressionFilterDefinition<TDocument>(filter), options, cancellationToken);
        }

        /// <summary>
        /// Finds a single document and deletes it atomically.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="session">The session.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The deleted document if one was deleted.
        /// </returns>
        public static Task<TDocument> FindOneAndDeleteAsync<TDocument>(this IMongoCollection<TDocument> collection, IClientSessionHandle session, FilterDefinition<TDocument> filter, FindOneAndDeleteOptions<TDocument, TDocument> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(session, nameof(session));
            Ensure.IsNotNull(filter, nameof(filter));

            return collection.FindOneAndDeleteAsync<TDocument>(session, filter, options, cancellationToken);
        }

        /// <summary>
        /// Finds a single document and deletes it atomically.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="session">The session.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The deleted document if one was deleted.
        /// </returns>
        public static Task<TDocument> FindOneAndDeleteAsync<TDocument>(this IMongoCollection<TDocument> collection, IClientSessionHandle session, Expression<Func<TDocument, bool>> filter, FindOneAndDeleteOptions<TDocument, TDocument> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(session, nameof(session));
            Ensure.IsNotNull(filter, nameof(filter));

            return collection.FindOneAndDeleteAsync<TDocument>(session, new ExpressionFilterDefinition<TDocument>(filter), options, cancellationToken);
        }

        /// <summary>
        /// Finds a single document and deletes it atomically.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <typeparam name="TProjection">The type of the projection (same as TDocument if there is no projection).</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="session">The session.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The returned document.
        /// </returns>
        public static Task<TProjection> FindOneAndDeleteAsync<TDocument, TProjection>(this IMongoCollection<TDocument> collection, IClientSessionHandle session, Expression<Func<TDocument, bool>> filter, FindOneAndDeleteOptions<TDocument, TProjection> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(session, nameof(session));
            Ensure.IsNotNull(filter, nameof(filter));

            return collection.FindOneAndDeleteAsync<TProjection>(session, new ExpressionFilterDefinition<TDocument>(filter), options, cancellationToken);
        }

        /// <summary>
        /// Finds a single document and replaces it atomically.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="replacement">The replacement.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The returned document.
        /// </returns>
        public static TDocument FindOneAndReplace<TDocument>(this IMongoCollection<TDocument> collection, FilterDefinition<TDocument> filter, TDocument replacement, FindOneAndReplaceOptions<TDocument, TDocument> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(filter, nameof(filter));

            return collection.FindOneAndReplace<TDocument>(filter, replacement, options, cancellationToken);
        }

        /// <summary>
        /// Finds a single document and replaces it atomically.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="replacement">The replacement.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The returned document.
        /// </returns>
        public static TDocument FindOneAndReplace<TDocument>(this IMongoCollection<TDocument> collection, Expression<Func<TDocument, bool>> filter, TDocument replacement, FindOneAndReplaceOptions<TDocument, TDocument> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(filter, nameof(filter));

            return collection.FindOneAndReplace<TDocument>(new ExpressionFilterDefinition<TDocument>(filter), replacement, options, cancellationToken);
        }

        /// <summary>
        /// Finds a single document and replaces it atomically.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <typeparam name="TProjection">The type of the projection (same as TDocument if there is no projection).</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="replacement">The replacement.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The returned document.
        /// </returns>
        public static TProjection FindOneAndReplace<TDocument, TProjection>(this IMongoCollection<TDocument> collection, Expression<Func<TDocument, bool>> filter, TDocument replacement, FindOneAndReplaceOptions<TDocument, TProjection> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(filter, nameof(filter));

            return collection.FindOneAndReplace<TProjection>(filter, replacement, options, cancellationToken);
        }

        /// <summary>
        /// Finds a single document and replaces it atomically.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="session">The session.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="replacement">The replacement.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The returned document.
        /// </returns>
        public static TDocument FindOneAndReplace<TDocument>(this IMongoCollection<TDocument> collection, IClientSessionHandle session, FilterDefinition<TDocument> filter, TDocument replacement, FindOneAndReplaceOptions<TDocument, TDocument> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(session, nameof(session));
            Ensure.IsNotNull(filter, nameof(filter));

            return collection.FindOneAndReplace<TDocument>(session, filter, replacement, options, cancellationToken);
        }

        /// <summary>
        /// Finds a single document and replaces it atomically.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="session">The session.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="replacement">The replacement.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The returned document.
        /// </returns>
        public static TDocument FindOneAndReplace<TDocument>(this IMongoCollection<TDocument> collection, IClientSessionHandle session, Expression<Func<TDocument, bool>> filter, TDocument replacement, FindOneAndReplaceOptions<TDocument, TDocument> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(session, nameof(session));
            Ensure.IsNotNull(filter, nameof(filter));

            return collection.FindOneAndReplace<TDocument>(session, new ExpressionFilterDefinition<TDocument>(filter), replacement, options, cancellationToken);
        }

        /// <summary>
        /// Finds a single document and replaces it atomically.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <typeparam name="TProjection">The type of the projection (same as TDocument if there is no projection).</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="session">The session.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="replacement">The replacement.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The returned document.
        /// </returns>
        public static TProjection FindOneAndReplace<TDocument, TProjection>(this IMongoCollection<TDocument> collection, IClientSessionHandle session, Expression<Func<TDocument, bool>> filter, TDocument replacement, FindOneAndReplaceOptions<TDocument, TProjection> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(session, nameof(session));
            Ensure.IsNotNull(filter, nameof(filter));

            return collection.FindOneAndReplace<TProjection>(session, filter, replacement, options, cancellationToken);
        }

        /// <summary>
        /// Finds a single document and replaces it atomically.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="replacement">The replacement.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The returned document.
        /// </returns>
        public static Task<TDocument> FindOneAndReplaceAsync<TDocument>(this IMongoCollection<TDocument> collection, FilterDefinition<TDocument> filter, TDocument replacement, FindOneAndReplaceOptions<TDocument, TDocument> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(filter, nameof(filter));

            return collection.FindOneAndReplaceAsync<TDocument>(filter, replacement, options, cancellationToken);
        }

        /// <summary>
        /// Finds a single document and replaces it atomically.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="replacement">The replacement.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The returned document.
        /// </returns>
        public static Task<TDocument> FindOneAndReplaceAsync<TDocument>(this IMongoCollection<TDocument> collection, Expression<Func<TDocument, bool>> filter, TDocument replacement, FindOneAndReplaceOptions<TDocument, TDocument> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(filter, nameof(filter));

            return collection.FindOneAndReplaceAsync<TDocument>(new ExpressionFilterDefinition<TDocument>(filter), replacement, options, cancellationToken);
        }

        /// <summary>
        /// Finds a single document and replaces it atomically.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <typeparam name="TProjection">The type of the projection (same as TDocument if there is no projection).</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="replacement">The replacement.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The returned document.
        /// </returns>
        public static Task<TProjection> FindOneAndReplaceAsync<TDocument, TProjection>(this IMongoCollection<TDocument> collection, Expression<Func<TDocument, bool>> filter, TDocument replacement, FindOneAndReplaceOptions<TDocument, TProjection> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(filter, nameof(filter));

            return collection.FindOneAndReplaceAsync<TProjection>(filter, replacement, options, cancellationToken);
        }

        /// <summary>
        /// Finds a single document and replaces it atomically.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="session">The session.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="replacement">The replacement.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The returned document.
        /// </returns>
        public static Task<TDocument> FindOneAndReplaceAsync<TDocument>(this IMongoCollection<TDocument> collection, IClientSessionHandle session, FilterDefinition<TDocument> filter, TDocument replacement, FindOneAndReplaceOptions<TDocument, TDocument> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(session, nameof(session));
            Ensure.IsNotNull(filter, nameof(filter));

            return collection.FindOneAndReplaceAsync<TDocument>(session, filter, replacement, options, cancellationToken);
        }

        /// <summary>
        /// Finds a single document and replaces it atomically.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="session">The session.</param>
        /// <param name="replacement">The replacement.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The returned document.
        /// </returns>
        public static Task<TDocument> FindOneAndReplaceAsync<TDocument>(this IMongoCollection<TDocument> collection, Expression<Func<TDocument, bool>> filter, IClientSessionHandle session, TDocument replacement, FindOneAndReplaceOptions<TDocument, TDocument> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(session, nameof(session));
            Ensure.IsNotNull(filter, nameof(filter));

            return collection.FindOneAndReplaceAsync<TDocument>(session, new ExpressionFilterDefinition<TDocument>(filter), replacement, options, cancellationToken);
        }

        /// <summary>
        /// Finds a single document and replaces it atomically.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <typeparam name="TProjection">The type of the projection (same as TDocument if there is no projection).</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="session">The session.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="replacement">The replacement.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The returned document.
        /// </returns>
        public static Task<TProjection> FindOneAndReplaceAsync<TDocument, TProjection>(this IMongoCollection<TDocument> collection, IClientSessionHandle session, Expression<Func<TDocument, bool>> filter, TDocument replacement, FindOneAndReplaceOptions<TDocument, TProjection> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(session, nameof(session));
            Ensure.IsNotNull(filter, nameof(filter));

            return collection.FindOneAndReplaceAsync<TProjection>(session, filter, replacement, options, cancellationToken);
        }

        /// <summary>
        /// Finds a single document and updates it atomically.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="update">The update.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The returned document.
        /// </returns>
        public static TDocument FindOneAndUpdate<TDocument>(this IMongoCollection<TDocument> collection, FilterDefinition<TDocument> filter, UpdateDefinition<TDocument> update, FindOneAndUpdateOptions<TDocument, TDocument> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(filter, nameof(filter));
            Ensure.IsNotNull(update, nameof(update));

            return collection.FindOneAndUpdate<TDocument>(
                filter,
                update,
                options,
                cancellationToken);
        }

        /// <summary>
        /// Finds a single document and updates it atomically.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="update">The update.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The returned document.
        /// </returns>
        public static TDocument FindOneAndUpdate<TDocument>(this IMongoCollection<TDocument> collection, Expression<Func<TDocument, bool>> filter, UpdateDefinition<TDocument> update, FindOneAndUpdateOptions<TDocument, TDocument> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(filter, nameof(filter));
            Ensure.IsNotNull(update, nameof(update));

            return collection.FindOneAndUpdate<TDocument>(
                new ExpressionFilterDefinition<TDocument>(filter),
                update,
                options,
                cancellationToken);
        }

        /// <summary>
        /// Finds a single document and updates it atomically.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <typeparam name="TProjection">The type of the projection (same as TDocument if there is no projection).</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="update">The update.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The returned document.
        /// </returns>
        public static TProjection FindOneAndUpdate<TDocument, TProjection>(this IMongoCollection<TDocument> collection, Expression<Func<TDocument, bool>> filter, UpdateDefinition<TDocument> update, FindOneAndUpdateOptions<TDocument, TProjection> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(filter, nameof(filter));
            Ensure.IsNotNull(update, nameof(update));

            return collection.FindOneAndUpdate(new ExpressionFilterDefinition<TDocument>(filter), update, options, cancellationToken);
        }

        /// <summary>
        /// Finds a single document and updates it atomically.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="session">The session.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="update">The update.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The returned document.
        /// </returns>
        public static TDocument FindOneAndUpdate<TDocument>(this IMongoCollection<TDocument> collection, IClientSessionHandle session, FilterDefinition<TDocument> filter, UpdateDefinition<TDocument> update, FindOneAndUpdateOptions<TDocument, TDocument> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(session, nameof(session));
            Ensure.IsNotNull(filter, nameof(filter));
            Ensure.IsNotNull(update, nameof(update));

            return collection.FindOneAndUpdate<TDocument>(
                session,
                filter,
                update,
                options,
                cancellationToken);
        }

        /// <summary>
        /// Finds a single document and updates it atomically.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="session">The session.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="update">The update.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The returned document.
        /// </returns>
        public static TDocument FindOneAndUpdate<TDocument>(this IMongoCollection<TDocument> collection, IClientSessionHandle session, Expression<Func<TDocument, bool>> filter, UpdateDefinition<TDocument> update, FindOneAndUpdateOptions<TDocument, TDocument> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(session, nameof(session));
            Ensure.IsNotNull(filter, nameof(filter));
            Ensure.IsNotNull(update, nameof(update));

            return collection.FindOneAndUpdate<TDocument>(
                session,
                new ExpressionFilterDefinition<TDocument>(filter),
                update,
                options,
                cancellationToken);
        }

        /// <summary>
        /// Finds a single document and updates it atomically.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <typeparam name="TProjection">The type of the projection (same as TDocument if there is no projection).</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="session">The session.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="update">The update.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The returned document.
        /// </returns>
        public static TProjection FindOneAndUpdate<TDocument, TProjection>(this IMongoCollection<TDocument> collection, IClientSessionHandle session, Expression<Func<TDocument, bool>> filter, UpdateDefinition<TDocument> update, FindOneAndUpdateOptions<TDocument, TProjection> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(session, nameof(session));
            Ensure.IsNotNull(filter, nameof(filter));
            Ensure.IsNotNull(update, nameof(update));

            return collection.FindOneAndUpdate(session, new ExpressionFilterDefinition<TDocument>(filter), update, options, cancellationToken);
        }

        /// <summary>
        /// Finds a single document and updates it atomically.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="update">The update.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The returned document.
        /// </returns>
        public static Task<TDocument> FindOneAndUpdateAsync<TDocument>(this IMongoCollection<TDocument> collection, FilterDefinition<TDocument> filter, UpdateDefinition<TDocument> update, FindOneAndUpdateOptions<TDocument, TDocument> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(filter, nameof(filter));
            Ensure.IsNotNull(update, nameof(update));

            return collection.FindOneAndUpdateAsync<TDocument>(
                filter,
                update,
                options,
                cancellationToken);
        }

        /// <summary>
        /// Finds a single document and updates it atomically.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="update">The update.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The returned document.
        /// </returns>
        public static Task<TDocument> FindOneAndUpdateAsync<TDocument>(this IMongoCollection<TDocument> collection, Expression<Func<TDocument, bool>> filter, UpdateDefinition<TDocument> update, FindOneAndUpdateOptions<TDocument, TDocument> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(filter, nameof(filter));
            Ensure.IsNotNull(update, nameof(update));

            return collection.FindOneAndUpdateAsync<TDocument>(
                new ExpressionFilterDefinition<TDocument>(filter),
                update,
                options,
                cancellationToken);
        }

        /// <summary>
        /// Finds a single document and updates it atomically.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <typeparam name="TProjection">The type of the projection (same as TDocument if there is no projection).</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="update">The update.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The returned document.
        /// </returns>
        public static Task<TProjection> FindOneAndUpdateAsync<TDocument, TProjection>(this IMongoCollection<TDocument> collection, Expression<Func<TDocument, bool>> filter, UpdateDefinition<TDocument> update, FindOneAndUpdateOptions<TDocument, TProjection> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(filter, nameof(filter));
            Ensure.IsNotNull(update, nameof(update));

            return collection.FindOneAndUpdateAsync(new ExpressionFilterDefinition<TDocument>(filter), update, options, cancellationToken);
        }

        /// <summary>
        /// Finds a single document and updates it atomically.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="session">The session.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="update">The update.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The returned document.
        /// </returns>
        public static Task<TDocument> FindOneAndUpdateAsync<TDocument>(this IMongoCollection<TDocument> collection, IClientSessionHandle session, FilterDefinition<TDocument> filter, UpdateDefinition<TDocument> update, FindOneAndUpdateOptions<TDocument, TDocument> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(session, nameof(session));
            Ensure.IsNotNull(filter, nameof(filter));
            Ensure.IsNotNull(update, nameof(update));

            return collection.FindOneAndUpdateAsync<TDocument>(
                session,
                filter,
                update,
                options,
                cancellationToken);
        }

        /// <summary>
        /// Finds a single document and updates it atomically.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="session">The session.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="update">The update.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The returned document.
        /// </returns>
        public static Task<TDocument> FindOneAndUpdateAsync<TDocument>(this IMongoCollection<TDocument> collection, IClientSessionHandle session, Expression<Func<TDocument, bool>> filter, UpdateDefinition<TDocument> update, FindOneAndUpdateOptions<TDocument, TDocument> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(session, nameof(session));
            Ensure.IsNotNull(filter, nameof(filter));
            Ensure.IsNotNull(update, nameof(update));

            return collection.FindOneAndUpdateAsync<TDocument>(
                session,
                new ExpressionFilterDefinition<TDocument>(filter),
                update,
                options,
                cancellationToken);
        }

        /// <summary>
        /// Finds a single document and updates it atomically.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <typeparam name="TProjection">The type of the projection (same as TDocument if there is no projection).</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="session">The session.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="update">The update.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The returned document.
        /// </returns>
        public static Task<TProjection> FindOneAndUpdateAsync<TDocument, TProjection>(this IMongoCollection<TDocument> collection, IClientSessionHandle session, Expression<Func<TDocument, bool>> filter, UpdateDefinition<TDocument> update, FindOneAndUpdateOptions<TDocument, TProjection> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(session, nameof(session));
            Ensure.IsNotNull(filter, nameof(filter));
            Ensure.IsNotNull(update, nameof(update));

            return collection.FindOneAndUpdateAsync(session, new ExpressionFilterDefinition<TDocument>(filter), update, options, cancellationToken);
        }

        /// <summary>
        /// Replaces a single document.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="replacement">The replacement.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The result of the replacement.
        /// </returns>
        public static ReplaceOneResult ReplaceOne<TDocument>(this IMongoCollection<TDocument> collection, Expression<Func<TDocument, bool>> filter, TDocument replacement, UpdateOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(filter, nameof(filter));

            return collection.ReplaceOne(new ExpressionFilterDefinition<TDocument>(filter), replacement, options, cancellationToken);
        }

        /// <summary>
        /// Replaces a single document.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="session">The session.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="replacement">The replacement.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The result of the replacement.
        /// </returns>
        public static ReplaceOneResult ReplaceOne<TDocument>(this IMongoCollection<TDocument> collection, IClientSessionHandle session, Expression<Func<TDocument, bool>> filter, TDocument replacement, UpdateOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(session, nameof(session));
            Ensure.IsNotNull(filter, nameof(filter));

            return collection.ReplaceOne(session, new ExpressionFilterDefinition<TDocument>(filter), replacement, options, cancellationToken);
        }

        /// <summary>
        /// Replaces a single document.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="replacement">The replacement.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The result of the replacement.
        /// </returns>
        public static Task<ReplaceOneResult> ReplaceOneAsync<TDocument>(this IMongoCollection<TDocument> collection, Expression<Func<TDocument, bool>> filter, TDocument replacement, UpdateOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(filter, nameof(filter));

            return collection.ReplaceOneAsync(new ExpressionFilterDefinition<TDocument>(filter), replacement, options, cancellationToken);
        }

        /// <summary>
        /// Replaces a single document.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="session">The session.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="replacement">The replacement.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The result of the replacement.
        /// </returns>
        public static Task<ReplaceOneResult> ReplaceOneAsync<TDocument>(this IMongoCollection<TDocument> collection, IClientSessionHandle session, Expression<Func<TDocument, bool>> filter, TDocument replacement, UpdateOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(session, nameof(session));
            Ensure.IsNotNull(filter, nameof(filter));

            return collection.ReplaceOneAsync(session, new ExpressionFilterDefinition<TDocument>(filter), replacement, options, cancellationToken);
        }

        /// <summary>
        /// Updates many documents.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="update">The update.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The result of the update operation.
        /// </returns>
        public static UpdateResult UpdateMany<TDocument>(this IMongoCollection<TDocument> collection, Expression<Func<TDocument, bool>> filter, UpdateDefinition<TDocument> update, UpdateOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(filter, nameof(filter));

            return collection.UpdateMany(new ExpressionFilterDefinition<TDocument>(filter), update, options, cancellationToken);
        }

        /// <summary>
        /// Updates many documents.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="session">The session.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="update">The update.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The result of the update operation.
        /// </returns>
        public static UpdateResult UpdateMany<TDocument>(this IMongoCollection<TDocument> collection, IClientSessionHandle session, Expression<Func<TDocument, bool>> filter, UpdateDefinition<TDocument> update, UpdateOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(session, nameof(session));
            Ensure.IsNotNull(filter, nameof(filter));

            return collection.UpdateMany(session, new ExpressionFilterDefinition<TDocument>(filter), update, options, cancellationToken);
        }

        /// <summary>
        /// Updates many documents.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="update">The update.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The result of the update operation.
        /// </returns>
        public static Task<UpdateResult> UpdateManyAsync<TDocument>(this IMongoCollection<TDocument> collection, Expression<Func<TDocument, bool>> filter, UpdateDefinition<TDocument> update, UpdateOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(filter, nameof(filter));

            return collection.UpdateManyAsync(new ExpressionFilterDefinition<TDocument>(filter), update, options, cancellationToken);
        }

        /// <summary>
        /// Updates many documents.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="session">The session.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="update">The update.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The result of the update operation.
        /// </returns>
        public static Task<UpdateResult> UpdateManyAsync<TDocument>(this IMongoCollection<TDocument> collection, IClientSessionHandle session, Expression<Func<TDocument, bool>> filter, UpdateDefinition<TDocument> update, UpdateOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(session, nameof(session));
            Ensure.IsNotNull(filter, nameof(filter));

            return collection.UpdateManyAsync(session, new ExpressionFilterDefinition<TDocument>(filter), update, options, cancellationToken);
        }

        /// <summary>
        /// Updates a single document.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="update">The update.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The result of the update operation.
        /// </returns>
        public static UpdateResult UpdateOne<TDocument>(this IMongoCollection<TDocument> collection, Expression<Func<TDocument, bool>> filter, UpdateDefinition<TDocument> update, UpdateOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(filter, nameof(filter));

            return collection.UpdateOne(
                new ExpressionFilterDefinition<TDocument>(filter),
                update,
                options,
                cancellationToken);
        }

        /// <summary>
        /// Updates a single document.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="session">The session.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="update">The update.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The result of the update operation.
        /// </returns>
        public static UpdateResult UpdateOne<TDocument>(this IMongoCollection<TDocument> collection, IClientSessionHandle session, Expression<Func<TDocument, bool>> filter, UpdateDefinition<TDocument> update, UpdateOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(session, nameof(session));
            Ensure.IsNotNull(filter, nameof(filter));

            return collection.UpdateOne(
                session,
                new ExpressionFilterDefinition<TDocument>(filter),
                update,
                options,
                cancellationToken);
        }

        /// <summary>
        /// Updates a single document.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="update">The update.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The result of the update operation.
        /// </returns>
        public static Task<UpdateResult> UpdateOneAsync<TDocument>(this IMongoCollection<TDocument> collection, Expression<Func<TDocument, bool>> filter, UpdateDefinition<TDocument> update, UpdateOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(filter, nameof(filter));

            return collection.UpdateOneAsync(
                new ExpressionFilterDefinition<TDocument>(filter),
                update,
                options,
                cancellationToken);
        }

        /// <summary>
        /// Updates a single document.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="session">The session.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="update">The update.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The result of the update operation.
        /// </returns>
        public static Task<UpdateResult> UpdateOneAsync<TDocument>(this IMongoCollection<TDocument> collection, IClientSessionHandle session, Expression<Func<TDocument, bool>> filter, UpdateDefinition<TDocument> update, UpdateOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(session, nameof(session));
            Ensure.IsNotNull(filter, nameof(filter));

            return collection.UpdateOneAsync(
                session,
                new ExpressionFilterDefinition<TDocument>(filter),
                update,
                options,
                cancellationToken);
        }

        /// <summary>
        /// Watches changes on the collection.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A change stream.
        /// </returns>
        public static IAsyncCursor<ChangeStreamDocument<TDocument>> Watch<TDocument>(
            this IMongoCollection<TDocument> collection,
            ChangeStreamOptions options = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            var emptyPipeline = new EmptyPipelineDefinition<ChangeStreamDocument<TDocument>>();
            return collection.Watch(emptyPipeline, options, cancellationToken);
        }

        /// <summary>
        /// Watches changes on the collection.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="session">The session.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A change stream.
        /// </returns>
        public static IAsyncCursor<ChangeStreamDocument<TDocument>> Watch<TDocument>(
            this IMongoCollection<TDocument> collection,
            IClientSessionHandle session,
            ChangeStreamOptions options = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(session, nameof(session));
            var emptyPipeline = new EmptyPipelineDefinition<ChangeStreamDocument<TDocument>>();
            return collection.Watch(session, emptyPipeline, options, cancellationToken);
        }

        /// <summary>
        /// Watches changes on the collection.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A change stream.
        /// </returns>
        public static Task<IAsyncCursor<ChangeStreamDocument<TDocument>>> WatchAsync<TDocument>(
            this IMongoCollection<TDocument> collection,
            ChangeStreamOptions options = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            var emptyPipeline = new EmptyPipelineDefinition<ChangeStreamDocument<TDocument>>();
            return collection.WatchAsync(emptyPipeline, options, cancellationToken);
        }

        /// <summary>
        /// Watches changes on the collection.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="session">The session.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A change stream.
        /// </returns>
        public static Task<IAsyncCursor<ChangeStreamDocument<TDocument>>> WatchAsync<TDocument>(
            this IMongoCollection<TDocument> collection,
            IClientSessionHandle session,
            ChangeStreamOptions options = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(collection, nameof(collection));
            Ensure.IsNotNull(session, nameof(session));
            var emptyPipeline = new EmptyPipelineDefinition<ChangeStreamDocument<TDocument>>();
            return collection.WatchAsync(session, emptyPipeline, options, cancellationToken);
        }
    }
}

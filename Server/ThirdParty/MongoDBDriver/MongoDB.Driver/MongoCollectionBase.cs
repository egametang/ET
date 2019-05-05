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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Linq;

namespace MongoDB.Driver
{
    /// <summary>
    /// Base class for implementors of <see cref="IMongoCollection{TDocument}"/>.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document.</typeparam>
    public abstract class MongoCollectionBase<TDocument> : IMongoCollection<TDocument>
    {
        /// <inheritdoc />
        public abstract CollectionNamespace CollectionNamespace { get; }

        /// <inheritdoc />
        public abstract IMongoDatabase Database { get; }

        /// <inheritdoc />
        public abstract IBsonSerializer<TDocument> DocumentSerializer { get; }

        /// <inheritdoc />
        public abstract IMongoIndexManager<TDocument> Indexes { get; }

        /// <inheritdoc />
        public abstract MongoCollectionSettings Settings { get; }

        /// <inheritdoc />
        public virtual IAsyncCursor<TResult> Aggregate<TResult>(PipelineDefinition<TDocument, TResult> pipeline, AggregateOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public virtual IAsyncCursor<TResult> Aggregate<TResult>(IClientSessionHandle session, PipelineDefinition<TDocument, TResult> pipeline, AggregateOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public abstract Task<IAsyncCursor<TResult>> AggregateAsync<TResult>(PipelineDefinition<TDocument, TResult> pipeline, AggregateOptions options = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <inheritdoc />
        public virtual Task<IAsyncCursor<TResult>> AggregateAsync<TResult>(IClientSessionHandle session, PipelineDefinition<TDocument, TResult> pipeline, AggregateOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public virtual BulkWriteResult<TDocument> BulkWrite(IEnumerable<WriteModel<TDocument>> requests, BulkWriteOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public virtual BulkWriteResult<TDocument> BulkWrite(IClientSessionHandle session, IEnumerable<WriteModel<TDocument>> requests, BulkWriteOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public abstract Task<BulkWriteResult<TDocument>> BulkWriteAsync(IEnumerable<WriteModel<TDocument>> requests, BulkWriteOptions options = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <inheritdoc />
        public virtual Task<BulkWriteResult<TDocument>> BulkWriteAsync(IClientSessionHandle session, IEnumerable<WriteModel<TDocument>> requests, BulkWriteOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        [Obsolete("Use CountDocuments or EstimatedDocumentCount instead.")]
        public virtual long Count(FilterDefinition<TDocument> filter, CountOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        [Obsolete("Use CountDocuments or EstimatedDocumentCount instead.")]
        public virtual long Count(IClientSessionHandle session, FilterDefinition<TDocument> filter, CountOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        [Obsolete("Use CountDocumentsAsync or EstimatedDocumentCountAsync instead.")]
        public abstract Task<long> CountAsync(FilterDefinition<TDocument> filter, CountOptions options = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <inheritdoc />
        [Obsolete("Use CountDocumentsAsync or EstimatedDocumentCountAsync instead.")]
        public virtual Task<long> CountAsync(IClientSessionHandle session, FilterDefinition<TDocument> filter, CountOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public virtual long CountDocuments(FilterDefinition<TDocument> filter, CountOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public virtual long CountDocuments(IClientSessionHandle session, FilterDefinition<TDocument> filter, CountOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public virtual Task<long> CountDocumentsAsync(FilterDefinition<TDocument> filter, CountOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public virtual Task<long> CountDocumentsAsync(IClientSessionHandle session, FilterDefinition<TDocument> filter, CountOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public virtual DeleteResult DeleteMany(FilterDefinition<TDocument> filter, CancellationToken cancellationToken = default(CancellationToken))
        {
            return DeleteMany(filter, null, cancellationToken);
        }

        /// <inheritdoc />
        public virtual DeleteResult DeleteMany(FilterDefinition<TDocument> filter, DeleteOptions options, CancellationToken cancellationToken = default(CancellationToken))
        {
            return DeleteMany(filter, options, requests => BulkWrite(requests, null, cancellationToken));
        }

        /// <inheritdoc />
        public virtual DeleteResult DeleteMany(IClientSessionHandle session, FilterDefinition<TDocument> filter, DeleteOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return DeleteMany(filter, options, requests => BulkWrite(session, requests, null, cancellationToken));
        }

        private DeleteResult DeleteMany(FilterDefinition<TDocument> filter, DeleteOptions options, Func<IEnumerable<WriteModel<TDocument>>, BulkWriteResult> bulkWriteFunc)
        {
            Ensure.IsNotNull(filter, nameof(filter));
            options = options ?? new DeleteOptions();

            var model = new DeleteManyModel<TDocument>(filter)
            {
                Collation = options.Collation
            };
            try
            {
                var result = bulkWriteFunc(new[] { model });
                return DeleteResult.FromCore(result);
            }
            catch (MongoBulkWriteException<TDocument> ex)
            {
                throw MongoWriteException.FromBulkWriteException(ex);
            }
        }

        /// <inheritdoc />
        public virtual Task<DeleteResult> DeleteManyAsync(FilterDefinition<TDocument> filter, CancellationToken cancellationToken = default(CancellationToken))
        {
            return DeleteManyAsync(filter, null, cancellationToken);
        }

        /// <inheritdoc />
        public virtual Task<DeleteResult> DeleteManyAsync(FilterDefinition<TDocument> filter, DeleteOptions options, CancellationToken cancellationToken = default(CancellationToken))
        {
            return DeleteManyAsync(filter, options, requests => BulkWriteAsync(requests, null, cancellationToken));
        }

        /// <inheritdoc />
        public virtual Task<DeleteResult> DeleteManyAsync(IClientSessionHandle session, FilterDefinition<TDocument> filter, DeleteOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return DeleteManyAsync(filter, options, requests => BulkWriteAsync(session, requests, null, cancellationToken));
        }

        private async Task<DeleteResult> DeleteManyAsync(FilterDefinition<TDocument> filter, DeleteOptions options, Func<IEnumerable<WriteModel<TDocument>>, Task<BulkWriteResult<TDocument>>> bulkWriteFuncAsync)
        {
            Ensure.IsNotNull(filter, nameof(filter));
            options = options ?? new DeleteOptions();

            var model = new DeleteManyModel<TDocument>(filter)
            {
                Collation = options.Collation
            };
            try
            {
                var result = await bulkWriteFuncAsync(new[] { model }).ConfigureAwait(false);
                return DeleteResult.FromCore(result);
            }
            catch (MongoBulkWriteException<TDocument> ex)
            {
                throw MongoWriteException.FromBulkWriteException(ex);
            }
        }

        /// <inheritdoc />
        public virtual DeleteResult DeleteOne(FilterDefinition<TDocument> filter, CancellationToken cancellationToken = default(CancellationToken))
        {
            return DeleteOne(filter, null, cancellationToken);
        }

        /// <inheritdoc />
        public virtual DeleteResult DeleteOne(FilterDefinition<TDocument> filter, DeleteOptions options, CancellationToken cancellationToken = default(CancellationToken))
        {
            return DeleteOne(filter, options, requests => BulkWrite(requests, null, cancellationToken));
        }

        /// <inheritdoc />
        public virtual DeleteResult DeleteOne(IClientSessionHandle session, FilterDefinition<TDocument> filter, DeleteOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return DeleteOne(filter, options, requests => BulkWrite(session, requests, null, cancellationToken));
        }

        private DeleteResult DeleteOne(FilterDefinition<TDocument> filter, DeleteOptions options, Func<IEnumerable<WriteModel<TDocument>>, BulkWriteResult> bulkWrite)
        {
            Ensure.IsNotNull(filter, nameof(filter));
            options = options ?? new DeleteOptions();

            var model = new DeleteOneModel<TDocument>(filter)
            {
                Collation = options.Collation
            };
            try
            {
                var result = bulkWrite(new[] { model });
                return DeleteResult.FromCore(result);
            }
            catch (MongoBulkWriteException<TDocument> ex)
            {
                throw MongoWriteException.FromBulkWriteException(ex);
            }
        }

        /// <inheritdoc />
        public virtual Task<DeleteResult> DeleteOneAsync(FilterDefinition<TDocument> filter, CancellationToken cancellationToken = default(CancellationToken))
        {
            return DeleteOneAsync(filter, null, cancellationToken);
        }

        /// <inheritdoc />
        public virtual Task<DeleteResult> DeleteOneAsync(FilterDefinition<TDocument> filter, DeleteOptions options, CancellationToken cancellationToken = default(CancellationToken))
        {
            return DeleteOneAsync(filter, options, requests => BulkWriteAsync(requests, null, cancellationToken));
        }

        /// <inheritdoc />
        public virtual Task<DeleteResult> DeleteOneAsync(IClientSessionHandle session, FilterDefinition<TDocument> filter, DeleteOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return DeleteOneAsync(filter, options, requests => BulkWriteAsync(session, requests, null, cancellationToken));
        }

        private async Task<DeleteResult> DeleteOneAsync(FilterDefinition<TDocument> filter, DeleteOptions options, Func<IEnumerable<WriteModel<TDocument>>, Task<BulkWriteResult<TDocument>>> bulkWriteAsync)
        {
            Ensure.IsNotNull(filter, nameof(filter));
            options = options ?? new DeleteOptions();

            var model = new DeleteOneModel<TDocument>(filter)
            {
                Collation = options.Collation
            };
            try
            {
                var result = await bulkWriteAsync(new[] { model }).ConfigureAwait(false);
                return DeleteResult.FromCore(result);
            }
            catch (MongoBulkWriteException<TDocument> ex)
            {
                throw MongoWriteException.FromBulkWriteException(ex);
            }
        }

        /// <inheritdoc />
        public virtual IAsyncCursor<TField> Distinct<TField>(FieldDefinition<TDocument, TField> field, FilterDefinition<TDocument> filter, DistinctOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public virtual IAsyncCursor<TField> Distinct<TField>(IClientSessionHandle session, FieldDefinition<TDocument, TField> field, FilterDefinition<TDocument> filter, DistinctOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public abstract Task<IAsyncCursor<TField>> DistinctAsync<TField>(FieldDefinition<TDocument, TField> field, FilterDefinition<TDocument> filter, DistinctOptions options = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <inheritdoc />
        public virtual Task<IAsyncCursor<TField>> DistinctAsync<TField>(IClientSessionHandle session, FieldDefinition<TDocument, TField> field, FilterDefinition<TDocument> filter, DistinctOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public virtual long EstimatedDocumentCount(EstimatedDocumentCountOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public virtual Task<long> EstimatedDocumentCountAsync(EstimatedDocumentCountOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public virtual IAsyncCursor<TProjection> FindSync<TProjection>(FilterDefinition<TDocument> filter, FindOptions<TDocument, TProjection> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public virtual IAsyncCursor<TProjection> FindSync<TProjection>(IClientSessionHandle session, FilterDefinition<TDocument> filter, FindOptions<TDocument, TProjection> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public abstract Task<IAsyncCursor<TProjection>> FindAsync<TProjection>(FilterDefinition<TDocument> filter, FindOptions<TDocument, TProjection> options = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <inheritdoc />
        public virtual Task<IAsyncCursor<TProjection>> FindAsync<TProjection>(IClientSessionHandle session, FilterDefinition<TDocument> filter, FindOptions<TDocument, TProjection> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public virtual TProjection FindOneAndDelete<TProjection>(FilterDefinition<TDocument> filter, FindOneAndDeleteOptions<TDocument, TProjection> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public virtual TProjection FindOneAndDelete<TProjection>(IClientSessionHandle session, FilterDefinition<TDocument> filter, FindOneAndDeleteOptions<TDocument, TProjection> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public abstract Task<TProjection> FindOneAndDeleteAsync<TProjection>(FilterDefinition<TDocument> filter, FindOneAndDeleteOptions<TDocument, TProjection> options = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <inheritdoc />
        public virtual Task<TProjection> FindOneAndDeleteAsync<TProjection>(IClientSessionHandle session, FilterDefinition<TDocument> filter, FindOneAndDeleteOptions<TDocument, TProjection> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public virtual TProjection FindOneAndReplace<TProjection>(FilterDefinition<TDocument> filter, TDocument replacement, FindOneAndReplaceOptions<TDocument, TProjection> options = null, CancellationToken cancellationToken = default(CancellationToken))

        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public virtual TProjection FindOneAndReplace<TProjection>(IClientSessionHandle session, FilterDefinition<TDocument> filter, TDocument replacement, FindOneAndReplaceOptions<TDocument, TProjection> options = null, CancellationToken cancellationToken = default(CancellationToken))

        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public abstract Task<TProjection> FindOneAndReplaceAsync<TProjection>(FilterDefinition<TDocument> filter, TDocument replacement, FindOneAndReplaceOptions<TDocument, TProjection> options = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <inheritdoc />
        public virtual Task<TProjection> FindOneAndReplaceAsync<TProjection>(IClientSessionHandle session, FilterDefinition<TDocument> filter, TDocument replacement, FindOneAndReplaceOptions<TDocument, TProjection> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public virtual TProjection FindOneAndUpdate<TProjection>(FilterDefinition<TDocument> filter, UpdateDefinition<TDocument> update, FindOneAndUpdateOptions<TDocument, TProjection> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public virtual TProjection FindOneAndUpdate<TProjection>(IClientSessionHandle session, FilterDefinition<TDocument> filter, UpdateDefinition<TDocument> update, FindOneAndUpdateOptions<TDocument, TProjection> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public abstract Task<TProjection> FindOneAndUpdateAsync<TProjection>(FilterDefinition<TDocument> filter, UpdateDefinition<TDocument> update, FindOneAndUpdateOptions<TDocument, TProjection> options = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <inheritdoc />
        public virtual Task<TProjection> FindOneAndUpdateAsync<TProjection>(IClientSessionHandle session, FilterDefinition<TDocument> filter, UpdateDefinition<TDocument> update, FindOneAndUpdateOptions<TDocument, TProjection> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public virtual void InsertOne(TDocument document, InsertOneOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            InsertOne(document, options, (requests, bulkWriteOptions) => BulkWrite(requests, bulkWriteOptions, cancellationToken));
        }

        /// <inheritdoc />
        public virtual void InsertOne(IClientSessionHandle session, TDocument document, InsertOneOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            InsertOne(document, options, (requests, bulkWriteOptions) => BulkWrite(session, requests, bulkWriteOptions, cancellationToken));
        }

        private void InsertOne(TDocument document, InsertOneOptions options, Action<IEnumerable<WriteModel<TDocument>>, BulkWriteOptions> bulkWrite)
        {
            Ensure.IsNotNull((object)document, "document");

            var model = new InsertOneModel<TDocument>(document);
            try
            {
                var bulkWriteOptions = options == null ? null : new BulkWriteOptions
                {
                    BypassDocumentValidation = options.BypassDocumentValidation
                };
                bulkWrite(new[] { model }, bulkWriteOptions);
            }
            catch (MongoBulkWriteException<TDocument> ex)
            {
                throw MongoWriteException.FromBulkWriteException(ex);
            }
        }

        /// <inheritdoc />
        [Obsolete("Use the new overload of InsertOneAsync with an InsertOneOptions parameter instead.")]
        public virtual Task InsertOneAsync(TDocument document, CancellationToken _cancellationToken)
        {
            return InsertOneAsync(document, null, _cancellationToken);
        }

        /// <inheritdoc />
        public virtual Task InsertOneAsync(TDocument document, InsertOneOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return InsertOneAsync(document, options, (requests, bulkWriteOptions) => BulkWriteAsync(requests, bulkWriteOptions, cancellationToken));
        }

        /// <inheritdoc />
        public virtual Task InsertOneAsync(IClientSessionHandle session, TDocument document, InsertOneOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return InsertOneAsync(document, options, (requests, bulkWriteOptions) => BulkWriteAsync(session, requests, bulkWriteOptions, cancellationToken));
        }

        private async Task InsertOneAsync(TDocument document, InsertOneOptions options, Func<IEnumerable<WriteModel<TDocument>>, BulkWriteOptions, Task> bulkWriteAsync)
        {
            Ensure.IsNotNull((object)document, "document");

            var model = new InsertOneModel<TDocument>(document);
            try
            {
                var bulkWriteOptions = options == null ? null : new BulkWriteOptions
                {
                    BypassDocumentValidation = options.BypassDocumentValidation
                };
                await bulkWriteAsync(new[] { model }, bulkWriteOptions).ConfigureAwait(false);
            }
            catch (MongoBulkWriteException<TDocument> ex)
            {
                throw MongoWriteException.FromBulkWriteException(ex);
            }
        }

        /// <inheritdoc />
        public virtual void InsertMany(IEnumerable<TDocument> documents, InsertManyOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            InsertMany(documents, options, (requests, bulkWriteOptions) => BulkWrite(requests, bulkWriteOptions, cancellationToken));
        }

        /// <inheritdoc />
        public virtual void InsertMany(IClientSessionHandle session, IEnumerable<TDocument> documents, InsertManyOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            InsertMany(documents, options, (requests, bulkWriteOptions) => BulkWrite(session, requests, bulkWriteOptions, cancellationToken));
        }

        private void InsertMany(IEnumerable<TDocument> documents, InsertManyOptions options, Action<IEnumerable<WriteModel<TDocument>>, BulkWriteOptions> bulkWrite)
        {
            Ensure.IsNotNull(documents, nameof(documents));

            var models = documents.Select(x => new InsertOneModel<TDocument>(x));
            BulkWriteOptions bulkWriteOptions = options == null ? null : new BulkWriteOptions
            {
                BypassDocumentValidation = options.BypassDocumentValidation,
                IsOrdered = options.IsOrdered
            };
            bulkWrite(models, bulkWriteOptions);
        }

        /// <inheritdoc />
        public virtual Task InsertManyAsync(IEnumerable<TDocument> documents, InsertManyOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return InsertManyAsync(documents, options, (requests, bulkWriteOptions) => BulkWriteAsync(requests, bulkWriteOptions, cancellationToken));
        }

        /// <inheritdoc />
        public virtual Task InsertManyAsync(IClientSessionHandle session, IEnumerable<TDocument> documents, InsertManyOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return InsertManyAsync(documents, options, (requests, bulkWriteOptions) => BulkWriteAsync(session, requests, bulkWriteOptions, cancellationToken));
        }

        private Task InsertManyAsync(IEnumerable<TDocument> documents, InsertManyOptions options, Func<IEnumerable<WriteModel<TDocument>>, BulkWriteOptions, Task> bulkWriteAsync)
        {
            Ensure.IsNotNull(documents, nameof(documents));

            var models = documents.Select(x => new InsertOneModel<TDocument>(x));
            var bulkWriteOptions = options == null ? null : new BulkWriteOptions
            {
                BypassDocumentValidation = options.BypassDocumentValidation,
                IsOrdered = options.IsOrdered
            };
            return bulkWriteAsync(models, bulkWriteOptions);
        }

        /// <inheritdoc />
        public virtual IAsyncCursor<TResult> MapReduce<TResult>(BsonJavaScript map, BsonJavaScript reduce, MapReduceOptions<TDocument, TResult> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public virtual IAsyncCursor<TResult> MapReduce<TResult>(IClientSessionHandle session, BsonJavaScript map, BsonJavaScript reduce, MapReduceOptions<TDocument, TResult> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public abstract Task<IAsyncCursor<TResult>> MapReduceAsync<TResult>(BsonJavaScript map, BsonJavaScript reduce, MapReduceOptions<TDocument, TResult> options = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <inheritdoc />
        public virtual Task<IAsyncCursor<TResult>> MapReduceAsync<TResult>(IClientSessionHandle session, BsonJavaScript map, BsonJavaScript reduce, MapReduceOptions<TDocument, TResult> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public abstract IFilteredMongoCollection<TDerivedDocument> OfType<TDerivedDocument>() where TDerivedDocument : TDocument;

        /// <inheritdoc />
        public virtual ReplaceOneResult ReplaceOne(FilterDefinition<TDocument> filter, TDocument replacement, UpdateOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ReplaceOne(filter, replacement, options, (requests, bulkWriteOptions) => BulkWrite(requests, bulkWriteOptions, cancellationToken));
        }

        /// <inheritdoc />
        public virtual ReplaceOneResult ReplaceOne(IClientSessionHandle session, FilterDefinition<TDocument> filter, TDocument replacement, UpdateOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ReplaceOne(filter, replacement, options, (requests, bulkWriteOptions) => BulkWrite(session, requests, bulkWriteOptions, cancellationToken));
        }

        private ReplaceOneResult ReplaceOne(FilterDefinition<TDocument> filter, TDocument replacement, UpdateOptions options, Func<IEnumerable<WriteModel<TDocument>>, BulkWriteOptions, BulkWriteResult<TDocument>> bulkWrite)
        {
            Ensure.IsNotNull(filter, nameof(filter));
            Ensure.IsNotNull((object)replacement, "replacement");
            if (options?.ArrayFilters != null)
            {
                throw new ArgumentException("ArrayFilters cannot be used with ReplaceOne.", nameof(options));
            }

            options = options ?? new UpdateOptions();
            var model = new ReplaceOneModel<TDocument>(filter, replacement)
            {
                Collation = options.Collation,
                IsUpsert = options.IsUpsert
            };

            try
            {
                var bulkWriteOptions = new BulkWriteOptions
                {
                    BypassDocumentValidation = options.BypassDocumentValidation
                };
                var result = bulkWrite(new[] { model }, bulkWriteOptions);
                return ReplaceOneResult.FromCore(result);
            }
            catch (MongoBulkWriteException<TDocument> ex)
            {
                throw MongoWriteException.FromBulkWriteException(ex);
            }
        }

        /// <inheritdoc />
        public virtual Task<ReplaceOneResult> ReplaceOneAsync(FilterDefinition<TDocument> filter, TDocument replacement, UpdateOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ReplaceOneAsync(filter, replacement, options, (requests, bulkWriteOptions) => BulkWriteAsync(requests, bulkWriteOptions, cancellationToken));
        }

        /// <inheritdoc />
        public virtual Task<ReplaceOneResult> ReplaceOneAsync(IClientSessionHandle session, FilterDefinition<TDocument> filter, TDocument replacement, UpdateOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ReplaceOneAsync(filter, replacement, options, (requests, bulkWriteOptions) => BulkWriteAsync(session, requests, bulkWriteOptions, cancellationToken));
        }

        private async Task<ReplaceOneResult> ReplaceOneAsync(FilterDefinition<TDocument> filter, TDocument replacement, UpdateOptions options, Func<IEnumerable<WriteModel<TDocument>>, BulkWriteOptions, Task<BulkWriteResult<TDocument>>> bulkWriteAsync)
        {
            Ensure.IsNotNull(filter, nameof(filter));
            Ensure.IsNotNull((object)replacement, "replacement");
            if (options?.ArrayFilters != null)
            {
                throw new ArgumentException("ArrayFilters cannot be used with ReplaceOne.", nameof(options));
            }

            options = options ?? new UpdateOptions();
            var model = new ReplaceOneModel<TDocument>(filter, replacement)
            {
                Collation = options.Collation,
                IsUpsert = options.IsUpsert
            };

            try
            {
                var bulkWriteOptions = new BulkWriteOptions
                {
                    BypassDocumentValidation = options.BypassDocumentValidation
                };
                var result = await bulkWriteAsync(new[] { model }, bulkWriteOptions).ConfigureAwait(false);
                return ReplaceOneResult.FromCore(result);
            }
            catch (MongoBulkWriteException<TDocument> ex)
            {
                throw MongoWriteException.FromBulkWriteException(ex);
            }
        }

        /// <inheritdoc />
        public virtual UpdateResult UpdateMany(FilterDefinition<TDocument> filter, UpdateDefinition<TDocument> update, UpdateOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return UpdateMany(filter, update, options, (requests, bulkWriteOptions) => BulkWrite(requests, bulkWriteOptions, cancellationToken));
        }

        /// <inheritdoc />
        public virtual UpdateResult UpdateMany(IClientSessionHandle session, FilterDefinition<TDocument> filter, UpdateDefinition<TDocument> update, UpdateOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return UpdateMany(filter, update, options, (requests, bulkWriteOptions) => BulkWrite(session, requests, bulkWriteOptions, cancellationToken));
        }

        private UpdateResult UpdateMany(FilterDefinition<TDocument> filter, UpdateDefinition<TDocument> update, UpdateOptions options, Func<IEnumerable<WriteModel<TDocument>>, BulkWriteOptions, BulkWriteResult<TDocument>> bulkWrite)
        {
            Ensure.IsNotNull(filter, nameof(filter));
            Ensure.IsNotNull(update, nameof(update));

            options = options ?? new UpdateOptions();
            var model = new UpdateManyModel<TDocument>(filter, update)
            {
                ArrayFilters = options.ArrayFilters,
                Collation = options.Collation,
                IsUpsert = options.IsUpsert
            };

            try
            {
                var bulkWriteOptions = new BulkWriteOptions
                {
                    BypassDocumentValidation = options.BypassDocumentValidation
                };
                var result = bulkWrite(new[] { model }, bulkWriteOptions);
                return UpdateResult.FromCore(result);
            }
            catch (MongoBulkWriteException<TDocument> ex)
            {
                throw MongoWriteException.FromBulkWriteException(ex);
            }
        }

        /// <inheritdoc />
        public virtual Task<UpdateResult> UpdateManyAsync(FilterDefinition<TDocument> filter, UpdateDefinition<TDocument> update, UpdateOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return UpdateManyAsync(filter, update, options, (requests, bulkWriteOptions) => BulkWriteAsync(requests, bulkWriteOptions, cancellationToken));
        }

        /// <inheritdoc />
        public virtual Task<UpdateResult> UpdateManyAsync(IClientSessionHandle session, FilterDefinition<TDocument> filter, UpdateDefinition<TDocument> update, UpdateOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return UpdateManyAsync(filter, update, options, (requests, bulkWriteOptions) => BulkWriteAsync(session, requests, bulkWriteOptions, cancellationToken));
        }

        private async Task<UpdateResult> UpdateManyAsync(FilterDefinition<TDocument> filter, UpdateDefinition<TDocument> update, UpdateOptions options, Func<IEnumerable<WriteModel<TDocument>>, BulkWriteOptions, Task<BulkWriteResult<TDocument>>> bulkWriteAsync)
        {
            Ensure.IsNotNull(filter, nameof(filter));
            Ensure.IsNotNull(update, nameof(update));

            options = options ?? new UpdateOptions();
            var model = new UpdateManyModel<TDocument>(filter, update)
            {
                ArrayFilters = options.ArrayFilters,
                Collation = options.Collation,
                IsUpsert = options.IsUpsert
            };

            try
            {
                var bulkWriteOptions = new BulkWriteOptions
                {
                    BypassDocumentValidation = options.BypassDocumentValidation
                };
                var result = await bulkWriteAsync(new[] { model }, bulkWriteOptions).ConfigureAwait(false);
                return UpdateResult.FromCore(result);
            }
            catch (MongoBulkWriteException<TDocument> ex)
            {
                throw MongoWriteException.FromBulkWriteException(ex);
            }
        }

        /// <inheritdoc />
        public virtual UpdateResult UpdateOne(FilterDefinition<TDocument> filter, UpdateDefinition<TDocument> update, UpdateOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return UpdateOne(filter, update, options, (requests, bulkWriteOptions) => BulkWrite(requests, bulkWriteOptions, cancellationToken));
        }

        /// <inheritdoc />
        public virtual UpdateResult UpdateOne(IClientSessionHandle session, FilterDefinition<TDocument> filter, UpdateDefinition<TDocument> update, UpdateOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return UpdateOne(filter, update, options, (requests, bulkWriteOptions) => BulkWrite(session, requests, bulkWriteOptions, cancellationToken));
        }

        private UpdateResult UpdateOne(FilterDefinition<TDocument> filter, UpdateDefinition<TDocument> update, UpdateOptions options, Func<IEnumerable<WriteModel<TDocument>>, BulkWriteOptions, BulkWriteResult<TDocument>> bulkWrite)
        {
            Ensure.IsNotNull(filter, nameof(filter));
            Ensure.IsNotNull(update, nameof(update));

            options = options ?? new UpdateOptions();
            var model = new UpdateOneModel<TDocument>(filter, update)
            {
                ArrayFilters = options.ArrayFilters,
                Collation = options.Collation,
                IsUpsert = options.IsUpsert
            };

            try
            {
                var bulkWriteOptions = new BulkWriteOptions
                {
                    BypassDocumentValidation = options.BypassDocumentValidation
                };
                var result = bulkWrite(new[] { model }, bulkWriteOptions);
                return UpdateResult.FromCore(result);
            }
            catch (MongoBulkWriteException<TDocument> ex)
            {
                throw MongoWriteException.FromBulkWriteException(ex);
            }
        }

        /// <inheritdoc />
        public virtual Task<UpdateResult> UpdateOneAsync(FilterDefinition<TDocument> filter, UpdateDefinition<TDocument> update, UpdateOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return UpdateOneAsync(filter, update, options, (requests, bulkWriteOptions) => BulkWriteAsync(requests, bulkWriteOptions, cancellationToken));
        }

        /// <inheritdoc />
        public virtual Task<UpdateResult> UpdateOneAsync(IClientSessionHandle session, FilterDefinition<TDocument> filter, UpdateDefinition<TDocument> update, UpdateOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return UpdateOneAsync(filter, update, options, (requests, bulkWriteOptions) => BulkWriteAsync(session, requests, bulkWriteOptions, cancellationToken));
        }

        private async Task<UpdateResult> UpdateOneAsync(FilterDefinition<TDocument> filter, UpdateDefinition<TDocument> update, UpdateOptions options, Func<IEnumerable<WriteModel<TDocument>>, BulkWriteOptions, Task<BulkWriteResult<TDocument>>> bulkWriteAsync)
        {
            Ensure.IsNotNull(filter, nameof(filter));
            Ensure.IsNotNull(update, nameof(update));

            options = options ?? new UpdateOptions();
            var model = new UpdateOneModel<TDocument>(filter, update)
            {
                ArrayFilters = options.ArrayFilters,
                Collation = options.Collation,
                IsUpsert = options.IsUpsert
            };

            try
            {
                var bulkWriteOptions = new BulkWriteOptions
                {
                    BypassDocumentValidation = options.BypassDocumentValidation
                };
                var result = await bulkWriteAsync(new[] { model }, bulkWriteOptions).ConfigureAwait(false);
                return UpdateResult.FromCore(result);
            }
            catch (MongoBulkWriteException<TDocument> ex)
            {
                throw MongoWriteException.FromBulkWriteException(ex);
            }
        }

        /// <inheritdoc />
        public virtual IAsyncCursor<TResult> Watch<TResult>(
            PipelineDefinition<ChangeStreamDocument<TDocument>, TResult> pipeline,
            ChangeStreamOptions options = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException(); // implemented by subclasses
        }

        /// <inheritdoc />
        public virtual IAsyncCursor<TResult> Watch<TResult>(
            IClientSessionHandle session,
            PipelineDefinition<ChangeStreamDocument<TDocument>, TResult> pipeline,
            ChangeStreamOptions options = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException(); // implemented by subclasses
        }

        /// <inheritdoc />
        public virtual Task<IAsyncCursor<TResult>> WatchAsync<TResult>(
            PipelineDefinition<ChangeStreamDocument<TDocument>, TResult> pipeline,
            ChangeStreamOptions options = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException(); // implemented by subclasses
        }

        /// <inheritdoc />
        public virtual Task<IAsyncCursor<TResult>> WatchAsync<TResult>(
            IClientSessionHandle session,
            PipelineDefinition<ChangeStreamDocument<TDocument>, TResult> pipeline,
            ChangeStreamOptions options = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException(); // implemented by subclasses
        }

        /// <inheritdoc />
        public virtual IMongoCollection<TDocument> WithReadConcern(ReadConcern readConcern)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public abstract IMongoCollection<TDocument> WithReadPreference(ReadPreference readPreference);

        /// <inheritdoc />
        public abstract IMongoCollection<TDocument> WithWriteConcern(WriteConcern writeConcern);
    }
}

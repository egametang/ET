/* Copyright 2010-2017 MongoDB Inc.
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
        public abstract Task<IAsyncCursor<TResult>> AggregateAsync<TResult>(PipelineDefinition<TDocument, TResult> pipeline, AggregateOptions options = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <inheritdoc />
        public virtual BulkWriteResult<TDocument> BulkWrite(IEnumerable<WriteModel<TDocument>> requests, BulkWriteOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public abstract Task<BulkWriteResult<TDocument>> BulkWriteAsync(IEnumerable<WriteModel<TDocument>> requests, BulkWriteOptions options = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <inheritdoc />
        public virtual long Count(FilterDefinition<TDocument> filter, CountOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public abstract Task<long> CountAsync(FilterDefinition<TDocument> filter, CountOptions options = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <inheritdoc />
        public virtual DeleteResult DeleteMany(FilterDefinition<TDocument> filter, CancellationToken cancellationToken = default(CancellationToken))
        {
            return DeleteMany(filter, null, cancellationToken);
        }

        /// <inheritdoc />
        public virtual DeleteResult DeleteMany(FilterDefinition<TDocument> filter, DeleteOptions options, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(filter, nameof(filter));
            options = options ?? new DeleteOptions();

            var model = new DeleteManyModel<TDocument>(filter)
            {
                Collation = options.Collation
            };
            try
            {
                var result = BulkWrite(new[] { model }, null, cancellationToken);
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
        public virtual async Task<DeleteResult> DeleteManyAsync(FilterDefinition<TDocument> filter, DeleteOptions options, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(filter, nameof(filter));
            options = options ?? new DeleteOptions();

            var model = new DeleteManyModel<TDocument>(filter)
            {
                Collation = options.Collation
            };
            try
            {
                var result = await BulkWriteAsync(new[] { model }, null, cancellationToken).ConfigureAwait(false);
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
            Ensure.IsNotNull(filter, nameof(filter));
            options = options ?? new DeleteOptions();

            var model = new DeleteOneModel<TDocument>(filter)
            {
                Collation = options.Collation
            };
            try
            {
                var result = BulkWrite(new[] { model }, null, cancellationToken);
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
        public virtual async Task<DeleteResult> DeleteOneAsync(FilterDefinition<TDocument> filter, DeleteOptions options, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(filter, nameof(filter));
            options = options ?? new DeleteOptions();

            var model = new DeleteOneModel<TDocument>(filter)
            {
                Collation = options.Collation
            };
            try
            {
                var result = await BulkWriteAsync(new[] { model }, null, cancellationToken).ConfigureAwait(false);
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
        public abstract Task<IAsyncCursor<TField>> DistinctAsync<TField>(FieldDefinition<TDocument, TField> field, FilterDefinition<TDocument> filter, DistinctOptions options = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <inheritdoc />
        public virtual IAsyncCursor<TProjection> FindSync<TProjection>(FilterDefinition<TDocument> filter, FindOptions<TDocument, TProjection> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public abstract Task<IAsyncCursor<TProjection>> FindAsync<TProjection>(FilterDefinition<TDocument> filter, FindOptions<TDocument, TProjection> options = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <inheritdoc />
        public virtual TProjection FindOneAndDelete<TProjection>(FilterDefinition<TDocument> filter, FindOneAndDeleteOptions<TDocument, TProjection> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public abstract Task<TProjection> FindOneAndDeleteAsync<TProjection>(FilterDefinition<TDocument> filter, FindOneAndDeleteOptions<TDocument, TProjection> options = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <inheritdoc />
        public virtual TProjection FindOneAndReplace<TProjection>(FilterDefinition<TDocument> filter, TDocument replacement, FindOneAndReplaceOptions<TDocument, TProjection> options = null, CancellationToken cancellationToken = default(CancellationToken))

        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public abstract Task<TProjection> FindOneAndReplaceAsync<TProjection>(FilterDefinition<TDocument> filter, TDocument replacement, FindOneAndReplaceOptions<TDocument, TProjection> options = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <inheritdoc />
        public virtual TProjection FindOneAndUpdate<TProjection>(FilterDefinition<TDocument> filter, UpdateDefinition<TDocument> update, FindOneAndUpdateOptions<TDocument, TProjection> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public abstract Task<TProjection> FindOneAndUpdateAsync<TProjection>(FilterDefinition<TDocument> filter, UpdateDefinition<TDocument> update, FindOneAndUpdateOptions<TDocument, TProjection> options = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <inheritdoc />
        public virtual void InsertOne(TDocument document, InsertOneOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull((object)document, "document");

            var model = new InsertOneModel<TDocument>(document);
            try
            {
                var bulkWriteOptions = options == null ? null : new BulkWriteOptions
                {
                    BypassDocumentValidation = options.BypassDocumentValidation
                };
                BulkWrite(new[] { model }, bulkWriteOptions, cancellationToken);
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
        public virtual async Task InsertOneAsync(TDocument document, InsertOneOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull((object)document, "document");

            var model = new InsertOneModel<TDocument>(document);
            try
            {
                var bulkWriteOptions = options == null ? null : new BulkWriteOptions
                {
                    BypassDocumentValidation = options.BypassDocumentValidation
                };
                await BulkWriteAsync(new[] { model }, bulkWriteOptions, cancellationToken).ConfigureAwait(false);
            }
            catch (MongoBulkWriteException<TDocument> ex)
            {
                throw MongoWriteException.FromBulkWriteException(ex);
            }
        }

        /// <inheritdoc />
        public virtual void InsertMany(IEnumerable<TDocument> documents, InsertManyOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(documents, nameof(documents));

            var models = documents.Select(x => new InsertOneModel<TDocument>(x));
            BulkWriteOptions bulkWriteOptions = options == null ? null : new BulkWriteOptions
            {
                BypassDocumentValidation = options.BypassDocumentValidation,
                IsOrdered = options.IsOrdered
            };
            BulkWrite(models, bulkWriteOptions, cancellationToken);
        }

        /// <inheritdoc />
        public virtual Task InsertManyAsync(IEnumerable<TDocument> documents, InsertManyOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(documents, nameof(documents));

            var models = documents.Select(x => new InsertOneModel<TDocument>(x));
            var bulkWriteOptions = options == null ? null : new BulkWriteOptions
            {
                BypassDocumentValidation = options.BypassDocumentValidation,
                IsOrdered = options.IsOrdered
            };
            return BulkWriteAsync(models, bulkWriteOptions, cancellationToken);
        }

        /// <inheritdoc />
        public virtual IAsyncCursor<TResult> MapReduce<TResult>(BsonJavaScript map, BsonJavaScript reduce, MapReduceOptions<TDocument, TResult> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public abstract Task<IAsyncCursor<TResult>> MapReduceAsync<TResult>(BsonJavaScript map, BsonJavaScript reduce, MapReduceOptions<TDocument, TResult> options = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <inheritdoc />
        public abstract IFilteredMongoCollection<TDerivedDocument> OfType<TDerivedDocument>() where TDerivedDocument : TDocument;

        /// <inheritdoc />
        public virtual ReplaceOneResult ReplaceOne(FilterDefinition<TDocument> filter, TDocument replacement, UpdateOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
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
                var result = BulkWrite(new[] { model }, bulkWriteOptions, cancellationToken);
                return ReplaceOneResult.FromCore(result);
            }
            catch (MongoBulkWriteException<TDocument> ex)
            {
                throw MongoWriteException.FromBulkWriteException(ex);
            }
        }

        /// <inheritdoc />
        public virtual async Task<ReplaceOneResult> ReplaceOneAsync(FilterDefinition<TDocument> filter, TDocument replacement, UpdateOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
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
                var result = await BulkWriteAsync(new[] { model }, bulkWriteOptions, cancellationToken).ConfigureAwait(false);
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
                var result = BulkWrite(new[] { model }, bulkWriteOptions, cancellationToken);
                return UpdateResult.FromCore(result);
            }
            catch (MongoBulkWriteException<TDocument> ex)
            {
                throw MongoWriteException.FromBulkWriteException(ex);
            }
        }

        /// <inheritdoc />
        public virtual async Task<UpdateResult> UpdateManyAsync(FilterDefinition<TDocument> filter, UpdateDefinition<TDocument> update, UpdateOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
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
                var result = await BulkWriteAsync(new[] { model }, bulkWriteOptions, cancellationToken).ConfigureAwait(false);
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
                var result = BulkWrite(new[] { model }, bulkWriteOptions, cancellationToken);
                return UpdateResult.FromCore(result);
            }
            catch (MongoBulkWriteException<TDocument> ex)
            {
                throw MongoWriteException.FromBulkWriteException(ex);
            }
        }

        /// <inheritdoc />
        public virtual async Task<UpdateResult> UpdateOneAsync(FilterDefinition<TDocument> filter, UpdateDefinition<TDocument> update, UpdateOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
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
                var result = await BulkWriteAsync(new[] { model }, bulkWriteOptions, cancellationToken).ConfigureAwait(false);
                return UpdateResult.FromCore(result);
            }
            catch (MongoBulkWriteException<TDocument> ex)
            {
                throw MongoWriteException.FromBulkWriteException(ex);
            }
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

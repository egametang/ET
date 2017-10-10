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
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Core.Bindings;
using MongoDB.Driver.Core.Clusters;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.Operations;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders;
using MongoDB.Driver.Linq;

namespace MongoDB.Driver
{
    internal sealed class MongoCollectionImpl<TDocument> : MongoCollectionBase<TDocument>
    {
        // fields
        private readonly ICluster _cluster;
        private readonly CollectionNamespace _collectionNamespace;
        private readonly IMongoDatabase _database;
        private readonly MessageEncoderSettings _messageEncoderSettings;
        private readonly IOperationExecutor _operationExecutor;
        private readonly IBsonSerializer<TDocument> _documentSerializer;
        private readonly MongoCollectionSettings _settings;

        // constructors
        public MongoCollectionImpl(IMongoDatabase database, CollectionNamespace collectionNamespace, MongoCollectionSettings settings, ICluster cluster, IOperationExecutor operationExecutor)
            : this(database, collectionNamespace, settings, cluster, operationExecutor, Ensure.IsNotNull(settings, "settings").SerializerRegistry.GetSerializer<TDocument>())
        {
        }

        private MongoCollectionImpl(IMongoDatabase database, CollectionNamespace collectionNamespace, MongoCollectionSettings settings, ICluster cluster, IOperationExecutor operationExecutor, IBsonSerializer<TDocument> documentSerializer)
        {
            _database = Ensure.IsNotNull(database, nameof(database));
            _collectionNamespace = Ensure.IsNotNull(collectionNamespace, nameof(collectionNamespace));
            _settings = Ensure.IsNotNull(settings, nameof(settings)).Freeze();
            _cluster = Ensure.IsNotNull(cluster, nameof(cluster));
            _operationExecutor = Ensure.IsNotNull(operationExecutor, nameof(operationExecutor));
            _documentSerializer = Ensure.IsNotNull(documentSerializer, nameof(documentSerializer));

            _messageEncoderSettings = new MessageEncoderSettings
            {
                { MessageEncoderSettingsName.GuidRepresentation, _settings.GuidRepresentation },
                { MessageEncoderSettingsName.ReadEncoding, _settings.ReadEncoding ?? Utf8Encodings.Strict },
                { MessageEncoderSettingsName.WriteEncoding, _settings.WriteEncoding ?? Utf8Encodings.Strict }
            };
        }

        // properties
        public override CollectionNamespace CollectionNamespace
        {
            get { return _collectionNamespace; }
        }

        public override IMongoDatabase Database
        {
            get { return _database; }
        }

        public override IBsonSerializer<TDocument> DocumentSerializer
        {
            get { return _documentSerializer; }
        }

        public override IMongoIndexManager<TDocument> Indexes
        {
            get { return new MongoIndexManager(this); }
        }

        public override MongoCollectionSettings Settings
        {
            get { return _settings; }
        }

        // public methods
        public override IAsyncCursor<TResult> Aggregate<TResult>(PipelineDefinition<TDocument, TResult> pipeline, AggregateOptions options, CancellationToken cancellationToken)
        {
            var renderedPipeline = Ensure.IsNotNull(pipeline, nameof(pipeline)).Render(_documentSerializer, _settings.SerializerRegistry);
            options = options ?? new AggregateOptions();

            var last = renderedPipeline.Documents.LastOrDefault();
            if (last != null && last.GetElement(0).Name == "$out")
            {
                var aggregateOperation = CreateAggregateToCollectionOperation(renderedPipeline, options);
                ExecuteWriteOperation(aggregateOperation, cancellationToken);

                // we want to delay execution of the find because the user may
                // not want to iterate the results at all...
                var findOperation = CreateAggregateToCollectionFindOperation(last, renderedPipeline.OutputSerializer, options);
                var deferredCursor = new DeferredAsyncCursor<TResult>(
                    ct => ExecuteReadOperation(findOperation, ReadPreference.Primary, ct),
                    ct => ExecuteReadOperationAsync(findOperation, ReadPreference.Primary, ct));
                return deferredCursor;
            }
            else
            {
                var aggregateOperation = CreateAggregateOperation(renderedPipeline, options);
                return ExecuteReadOperation(aggregateOperation, cancellationToken);
            }
        }

        public override async Task<IAsyncCursor<TResult>> AggregateAsync<TResult>(PipelineDefinition<TDocument, TResult> pipeline, AggregateOptions options, CancellationToken cancellationToken)
        {
            var renderedPipeline = Ensure.IsNotNull(pipeline, nameof(pipeline)).Render(_documentSerializer, _settings.SerializerRegistry);
            options = options ?? new AggregateOptions();

            var last = renderedPipeline.Documents.LastOrDefault();
            if (last != null && last.GetElement(0).Name == "$out")
            {
                var aggregateOperation = CreateAggregateToCollectionOperation(renderedPipeline, options);
                await ExecuteWriteOperationAsync(aggregateOperation, cancellationToken).ConfigureAwait(false);

                // we want to delay execution of the find because the user may
                // not want to iterate the results at all...
                var findOperation = CreateAggregateToCollectionFindOperation(last, renderedPipeline.OutputSerializer, options);
                var deferredCursor = new DeferredAsyncCursor<TResult>(
                    ct => ExecuteReadOperation(findOperation, ReadPreference.Primary, ct),
                    ct => ExecuteReadOperationAsync(findOperation, ReadPreference.Primary, ct));
                return await Task.FromResult<IAsyncCursor<TResult>>(deferredCursor).ConfigureAwait(false);
            }
            else
            {
                var aggregateOperation = CreateAggregateOperation(renderedPipeline, options);
                return await ExecuteReadOperationAsync(aggregateOperation, cancellationToken).ConfigureAwait(false);
            }
        }

        public override BulkWriteResult<TDocument> BulkWrite(IEnumerable<WriteModel<TDocument>> requests, BulkWriteOptions options, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(requests, nameof(requests));
            if (!requests.Any())
            {
                throw new ArgumentException("Must contain at least 1 request.", "requests");
            }
            options = options ?? new BulkWriteOptions();

            var operation = CreateBulkWriteOperation(requests, options);
            try
            {
                var result = ExecuteWriteOperation(operation, cancellationToken);
                return BulkWriteResult<TDocument>.FromCore(result, requests);
            }
            catch (MongoBulkWriteOperationException ex)
            {
                throw MongoBulkWriteException<TDocument>.FromCore(ex, requests.ToList());
            }
        }

        public override async Task<BulkWriteResult<TDocument>> BulkWriteAsync(IEnumerable<WriteModel<TDocument>> requests, BulkWriteOptions options, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(requests, nameof(requests));
            if (!requests.Any())
            {
                throw new ArgumentException("Must contain at least 1 request.", "requests");
            }
            options = options ?? new BulkWriteOptions();

            var operation = CreateBulkWriteOperation(requests, options);
            try
            {
                var result = await ExecuteWriteOperationAsync(operation, cancellationToken).ConfigureAwait(false);
                return BulkWriteResult<TDocument>.FromCore(result, requests);
            }
            catch (MongoBulkWriteOperationException ex)
            {
                throw MongoBulkWriteException<TDocument>.FromCore(ex, requests.ToList());
            }
        }

        public override long Count(FilterDefinition<TDocument> filter, CountOptions options, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(filter, nameof(filter));
            options = options ?? new CountOptions();

            var operation = CreateCountOperation(filter, options);
            return ExecuteReadOperation(operation, cancellationToken);
        }

        public override Task<long> CountAsync(FilterDefinition<TDocument> filter, CountOptions options, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(filter, nameof(filter));
            options = options ?? new CountOptions();

            var operation = CreateCountOperation(filter, options);
            return ExecuteReadOperationAsync(operation, cancellationToken);
        }

        public override IAsyncCursor<TField> Distinct<TField>(FieldDefinition<TDocument, TField> field, FilterDefinition<TDocument> filter, DistinctOptions options, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(field, nameof(field));
            Ensure.IsNotNull(filter, nameof(filter));
            options = options ?? new DistinctOptions();

            var operation = CreateDistinctOperation(field, filter, options);
            return ExecuteReadOperation(operation, cancellationToken);
        }

        public override Task<IAsyncCursor<TField>> DistinctAsync<TField>(FieldDefinition<TDocument, TField> field, FilterDefinition<TDocument> filter, DistinctOptions options, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(field, nameof(field));
            Ensure.IsNotNull(filter, nameof(filter));
            options = options ?? new DistinctOptions();

            var operation = CreateDistinctOperation(field, filter, options);
            return ExecuteReadOperationAsync(operation, cancellationToken);
        }

        public override IAsyncCursor<TProjection> FindSync<TProjection>(FilterDefinition<TDocument> filter, FindOptions<TDocument, TProjection> options, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(filter, nameof(filter));
            options = options ?? new FindOptions<TDocument, TProjection>();

            var operation = CreateFindOperation<TProjection>(filter, options);
            return ExecuteReadOperation(operation, cancellationToken);
        }

        public override Task<IAsyncCursor<TProjection>> FindAsync<TProjection>(FilterDefinition<TDocument> filter, FindOptions<TDocument, TProjection> options, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(filter, nameof(filter));
            options = options ?? new FindOptions<TDocument, TProjection>();

            var operation = CreateFindOperation<TProjection>(filter, options);
            return ExecuteReadOperationAsync(operation, cancellationToken);
        }

        public override TProjection FindOneAndDelete<TProjection>(FilterDefinition<TDocument> filter, FindOneAndDeleteOptions<TDocument, TProjection> options, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(filter, nameof(filter));
            options = options ?? new FindOneAndDeleteOptions<TDocument, TProjection>();

            var operation = CreateFindOneAndDeleteOperation<TProjection>(filter, options);
            return ExecuteWriteOperation(operation, cancellationToken);
        }

        public override Task<TProjection> FindOneAndDeleteAsync<TProjection>(FilterDefinition<TDocument> filter, FindOneAndDeleteOptions<TDocument, TProjection> options, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(filter, nameof(filter));
            options = options ?? new FindOneAndDeleteOptions<TDocument, TProjection>();

            var operation = CreateFindOneAndDeleteOperation<TProjection>(filter, options);
            return ExecuteWriteOperationAsync(operation, cancellationToken);
        }

        public override TProjection FindOneAndReplace<TProjection>(FilterDefinition<TDocument> filter, TDocument replacement, FindOneAndReplaceOptions<TDocument, TProjection> options, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(filter, nameof(filter));
            var replacementObject = Ensure.IsNotNull((object)replacement, nameof(replacement)); // only box once if it's a struct
            options = options ?? new FindOneAndReplaceOptions<TDocument, TProjection>();

            var operation = CreateFindOneAndReplaceOperation(filter, replacementObject, options);
            return ExecuteWriteOperation(operation, cancellationToken);
        }

        public override Task<TProjection> FindOneAndReplaceAsync<TProjection>(FilterDefinition<TDocument> filter, TDocument replacement, FindOneAndReplaceOptions<TDocument, TProjection> options, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(filter, nameof(filter));
            var replacementObject = Ensure.IsNotNull((object)replacement, nameof(replacement)); // only box once if it's a struct
            options = options ?? new FindOneAndReplaceOptions<TDocument, TProjection>();

            var operation = CreateFindOneAndReplaceOperation(filter, replacementObject, options);
            return ExecuteWriteOperationAsync(operation, cancellationToken);
        }

        public override TProjection FindOneAndUpdate<TProjection>(FilterDefinition<TDocument> filter, UpdateDefinition<TDocument> update, FindOneAndUpdateOptions<TDocument, TProjection> options, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(filter, nameof(filter));
            Ensure.IsNotNull(update, nameof(update));
            options = options ?? new FindOneAndUpdateOptions<TDocument, TProjection>();

            var operation = CreateFindOneAndUpdateOperation(filter, update, options);
            return ExecuteWriteOperation(operation, cancellationToken);
        }

        public override Task<TProjection> FindOneAndUpdateAsync<TProjection>(FilterDefinition<TDocument> filter, UpdateDefinition<TDocument> update, FindOneAndUpdateOptions<TDocument, TProjection> options, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(filter, nameof(filter));
            Ensure.IsNotNull(update, nameof(update));
            options = options ?? new FindOneAndUpdateOptions<TDocument, TProjection>();

            var operation = CreateFindOneAndUpdateOperation(filter, update, options);
            return ExecuteWriteOperationAsync(operation, cancellationToken);
        }

        public override IAsyncCursor<TResult> MapReduce<TResult>(BsonJavaScript map, BsonJavaScript reduce, MapReduceOptions<TDocument, TResult> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(map, nameof(map));
            Ensure.IsNotNull(reduce, nameof(reduce));
            options = options ?? new MapReduceOptions<TDocument, TResult>();

            var outputOptions = options.OutputOptions ?? MapReduceOutputOptions.Inline;
            var resultSerializer = ResolveResultSerializer<TResult>(options.ResultSerializer);

            if (outputOptions == MapReduceOutputOptions.Inline)
            {
                var operation = CreateMapReduceOperation(map, reduce, options, resultSerializer);
                return ExecuteReadOperation(operation, cancellationToken);
            }
            else
            {
                var mapReduceOperation = CreateMapReduceOutputToCollectionOperation(map, reduce, options, outputOptions);
                ExecuteWriteOperation(mapReduceOperation, cancellationToken);

                var findOperation = CreateMapReduceOutputToCollectionFindOperation<TResult>(options, mapReduceOperation.OutputCollectionNamespace, resultSerializer);

                // we want to delay execution of the find because the user may
                // not want to iterate the results at all...
                var deferredCursor = new DeferredAsyncCursor<TResult>(
                    ct => ExecuteReadOperation(findOperation, ReadPreference.Primary, ct),
                    ct => ExecuteReadOperationAsync(findOperation, ReadPreference.Primary, ct));
                return deferredCursor;
            }
        }

        public override async Task<IAsyncCursor<TResult>> MapReduceAsync<TResult>(BsonJavaScript map, BsonJavaScript reduce, MapReduceOptions<TDocument, TResult> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(map, nameof(map));
            Ensure.IsNotNull(reduce, nameof(reduce));
            options = options ?? new MapReduceOptions<TDocument, TResult>();

            var outputOptions = options.OutputOptions ?? MapReduceOutputOptions.Inline;
            var resultSerializer = ResolveResultSerializer<TResult>(options.ResultSerializer);

            if (outputOptions == MapReduceOutputOptions.Inline)
            {
                var operation = CreateMapReduceOperation(map, reduce, options, resultSerializer);
                return await ExecuteReadOperationAsync(operation, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                var mapReduceOperation = CreateMapReduceOutputToCollectionOperation(map, reduce, options, outputOptions);
                await ExecuteWriteOperationAsync(mapReduceOperation, cancellationToken).ConfigureAwait(false);

                var findOperation = CreateMapReduceOutputToCollectionFindOperation<TResult>(options, mapReduceOperation.OutputCollectionNamespace, resultSerializer);

                // we want to delay execution of the find because the user may
                // not want to iterate the results at all...
                var deferredCursor = new DeferredAsyncCursor<TResult>(
                    ct => ExecuteReadOperation(findOperation, ReadPreference.Primary, ct),
                    ct => ExecuteReadOperationAsync(findOperation, ReadPreference.Primary, ct));
                return await Task.FromResult(deferredCursor).ConfigureAwait(false);
            }
        }

        public override IFilteredMongoCollection<TDerivedDocument> OfType<TDerivedDocument>()
        {
            var derivedDocumentSerializer = _settings.SerializerRegistry.GetSerializer<TDerivedDocument>();
            var ofTypeSerializer = new OfTypeSerializer<TDocument, TDerivedDocument>(derivedDocumentSerializer);
            var derivedDocumentCollection = new MongoCollectionImpl<TDerivedDocument>(_database, _collectionNamespace, _settings, _cluster, _operationExecutor, ofTypeSerializer);

            var rootOfTypeFilter = Builders<TDocument>.Filter.OfType<TDerivedDocument>();
            var renderedOfTypeFilter = rootOfTypeFilter.Render(_documentSerializer, _settings.SerializerRegistry);
            var ofTypeFilter = new BsonDocumentFilterDefinition<TDerivedDocument>(renderedOfTypeFilter);

            return new OfTypeMongoCollection<TDocument, TDerivedDocument>(this, derivedDocumentCollection, ofTypeFilter);
        }

        public override IMongoCollection<TDocument> WithReadConcern(ReadConcern readConcern)
        {
            var newSettings = _settings.Clone();
            newSettings.ReadConcern = readConcern;
            return new MongoCollectionImpl<TDocument>(_database, _collectionNamespace, newSettings, _cluster, _operationExecutor);
        }

        public override IMongoCollection<TDocument> WithReadPreference(ReadPreference readPreference)
        {
            var newSettings = _settings.Clone();
            newSettings.ReadPreference = readPreference;
            return new MongoCollectionImpl<TDocument>(_database, _collectionNamespace, newSettings, _cluster, _operationExecutor);
        }

        public override IMongoCollection<TDocument> WithWriteConcern(WriteConcern writeConcern)
        {
            var newSettings = _settings.Clone();
            newSettings.WriteConcern = writeConcern;
            return new MongoCollectionImpl<TDocument>(_database, _collectionNamespace, newSettings, _cluster, _operationExecutor);
        }

        // private methods
        private void AssignId(TDocument document)
        {
            var idProvider = _documentSerializer as IBsonIdProvider;
            if (idProvider != null)
            {
                object id;
                Type idNominalType;
                IIdGenerator idGenerator;
                if (idProvider.GetDocumentId(document, out id, out idNominalType, out idGenerator))
                {
                    if (idGenerator != null && idGenerator.IsEmpty(id))
                    {
                        id = idGenerator.GenerateId(this, document);
                        idProvider.SetDocumentId(document, id);
                    }
                }
            }
        }

        private WriteRequest ConvertWriteModelToWriteRequest(WriteModel<TDocument> model, int index)
        {
            switch (model.ModelType)
            {
                case WriteModelType.InsertOne:
                    var insertOneModel = (InsertOneModel<TDocument>)model;
                    if (_settings.AssignIdOnInsert)
                    {
                        AssignId(insertOneModel.Document);
                    }
                    return new InsertRequest(new BsonDocumentWrapper(insertOneModel.Document, _documentSerializer))
                    {
                        CorrelationId = index
                    };
                case WriteModelType.DeleteMany:
                    var deleteManyModel = (DeleteManyModel<TDocument>)model;
                    return new DeleteRequest(deleteManyModel.Filter.Render(_documentSerializer, _settings.SerializerRegistry))
                    {
                        CorrelationId = index,
                        Collation = deleteManyModel.Collation,
                        Limit = 0
                    };
                case WriteModelType.DeleteOne:
                    var deleteOneModel = (DeleteOneModel<TDocument>)model;
                    return new DeleteRequest(deleteOneModel.Filter.Render(_documentSerializer, _settings.SerializerRegistry))
                    {
                        CorrelationId = index,
                        Collation = deleteOneModel.Collation,
                        Limit = 1
                    };
                case WriteModelType.ReplaceOne:
                    var replaceOneModel = (ReplaceOneModel<TDocument>)model;
                    return new UpdateRequest(
                        UpdateType.Replacement,
                        replaceOneModel.Filter.Render(_documentSerializer, _settings.SerializerRegistry),
                        new BsonDocumentWrapper(replaceOneModel.Replacement, _documentSerializer))
                    {
                        Collation = replaceOneModel.Collation,
                        CorrelationId = index,
                        IsMulti = false,
                        IsUpsert = replaceOneModel.IsUpsert
                    };
                case WriteModelType.UpdateMany:
                    var updateManyModel = (UpdateManyModel<TDocument>)model;
                    return new UpdateRequest(
                        UpdateType.Update,
                        updateManyModel.Filter.Render(_documentSerializer, _settings.SerializerRegistry),
                        updateManyModel.Update.Render(_documentSerializer, _settings.SerializerRegistry))
                    {
                        ArrayFilters = RenderArrayFilters(updateManyModel.ArrayFilters),
                        Collation = updateManyModel.Collation,
                        CorrelationId = index,
                        IsMulti = true,
                        IsUpsert = updateManyModel.IsUpsert
                    };
                case WriteModelType.UpdateOne:
                    var updateOneModel = (UpdateOneModel<TDocument>)model;
                    return new UpdateRequest(
                        UpdateType.Update,
                        updateOneModel.Filter.Render(_documentSerializer, _settings.SerializerRegistry),
                        updateOneModel.Update.Render(_documentSerializer, _settings.SerializerRegistry))
                    {
                        ArrayFilters = RenderArrayFilters(updateOneModel.ArrayFilters),
                        Collation = updateOneModel.Collation,
                        CorrelationId = index,
                        IsMulti = false,
                        IsUpsert = updateOneModel.IsUpsert
                    };
                default:
                    throw new InvalidOperationException("Unknown type of WriteModel provided.");
            }
        }

        private AggregateOperation<TResult> CreateAggregateOperation<TResult>(RenderedPipelineDefinition<TResult> renderedPipeline, AggregateOptions options)
        {
            return new AggregateOperation<TResult>(
                _collectionNamespace,
                renderedPipeline.Documents,
                renderedPipeline.OutputSerializer,
                _messageEncoderSettings)
            {
                AllowDiskUse = options.AllowDiskUse,
                BatchSize = options.BatchSize,
                Collation = options.Collation,
                MaxTime = options.MaxTime,
                ReadConcern = _settings.ReadConcern,
                UseCursor = options.UseCursor
            };
        }

        private FindOperation<TResult> CreateAggregateToCollectionFindOperation<TResult>(BsonDocument outStage, IBsonSerializer<TResult> resultSerializer, AggregateOptions options)
        {
            var outputCollectionName = outStage.GetElement(0).Value.AsString;

            return new FindOperation<TResult>(
                new CollectionNamespace(_collectionNamespace.DatabaseNamespace, outputCollectionName),
                resultSerializer,
                _messageEncoderSettings)
            {
                BatchSize = options.BatchSize,
                Collation = options.Collation,
                MaxTime = options.MaxTime,
                ReadConcern = _settings.ReadConcern
            };
        }

        private AggregateToCollectionOperation CreateAggregateToCollectionOperation<TResult>(RenderedPipelineDefinition<TResult> renderedPipeline, AggregateOptions options)
        {
            return new AggregateToCollectionOperation(
                _collectionNamespace,
                renderedPipeline.Documents,
                _messageEncoderSettings)
            {
                AllowDiskUse = options.AllowDiskUse,
                BypassDocumentValidation = options.BypassDocumentValidation,
                Collation = options.Collation,
                MaxTime = options.MaxTime,
                WriteConcern = _settings.WriteConcern
            };
        }

        private BulkMixedWriteOperation CreateBulkWriteOperation(IEnumerable<WriteModel<TDocument>> requests, BulkWriteOptions options)
        {
            return new BulkMixedWriteOperation(
                _collectionNamespace,
                requests.Select(ConvertWriteModelToWriteRequest),
                _messageEncoderSettings)
            {
                BypassDocumentValidation = options.BypassDocumentValidation,
                IsOrdered = options.IsOrdered,
                WriteConcern = _settings.WriteConcern
            };
        }

        private CountOperation CreateCountOperation(FilterDefinition<TDocument> filter, CountOptions options)
        {
            return new CountOperation(_collectionNamespace, _messageEncoderSettings)
            {
                Collation = options.Collation,
                Filter = filter.Render(_documentSerializer, _settings.SerializerRegistry),
                Hint = options.Hint,
                Limit = options.Limit,
                MaxTime = options.MaxTime,
                ReadConcern = _settings.ReadConcern,
                Skip = options.Skip
            };
        }

        private DistinctOperation<TField> CreateDistinctOperation<TField>(FieldDefinition<TDocument, TField> field, FilterDefinition<TDocument> filter, DistinctOptions options)
        {
            var renderedField = field.Render(_documentSerializer, _settings.SerializerRegistry);
            var valueSerializer = GetValueSerializerForDistinct(renderedField, _settings.SerializerRegistry);

            return new DistinctOperation<TField>(
                _collectionNamespace,
                valueSerializer,
                renderedField.FieldName,
                _messageEncoderSettings)
            {
                Collation = options.Collation,
                Filter = filter.Render(_documentSerializer, _settings.SerializerRegistry),
                MaxTime = options.MaxTime,
                ReadConcern = _settings.ReadConcern
            };
        }

        private FindOneAndDeleteOperation<TProjection> CreateFindOneAndDeleteOperation<TProjection>(FilterDefinition<TDocument> filter, FindOneAndDeleteOptions<TDocument, TProjection> options)
        {
            var projection = options.Projection ?? new ClientSideDeserializationProjectionDefinition<TDocument, TProjection>();
            var renderedProjection = projection.Render(_documentSerializer, _settings.SerializerRegistry);

            return new FindOneAndDeleteOperation<TProjection>(
                _collectionNamespace,
                filter.Render(_documentSerializer, _settings.SerializerRegistry),
                new FindAndModifyValueDeserializer<TProjection>(renderedProjection.ProjectionSerializer),
                _messageEncoderSettings)
            {
                Collation = options.Collation,
                MaxTime = options.MaxTime,
                Projection = renderedProjection.Document,
                Sort = options.Sort == null ? null : options.Sort.Render(_documentSerializer, _settings.SerializerRegistry),
                WriteConcern = _settings.WriteConcern
            };
        }

        private FindOneAndReplaceOperation<TProjection> CreateFindOneAndReplaceOperation<TProjection>(FilterDefinition<TDocument> filter, object replacementObject, FindOneAndReplaceOptions<TDocument, TProjection> options)
        {
            var projection = options.Projection ?? new ClientSideDeserializationProjectionDefinition<TDocument, TProjection>();
            var renderedProjection = projection.Render(_documentSerializer, _settings.SerializerRegistry);

            return new FindOneAndReplaceOperation<TProjection>(
                _collectionNamespace,
                filter.Render(_documentSerializer, _settings.SerializerRegistry),
                new BsonDocumentWrapper(replacementObject, _documentSerializer),
                new FindAndModifyValueDeserializer<TProjection>(renderedProjection.ProjectionSerializer),
                _messageEncoderSettings)
            {
                BypassDocumentValidation = options.BypassDocumentValidation,
                Collation = options.Collation,
                IsUpsert = options.IsUpsert,
                MaxTime = options.MaxTime,
                Projection = renderedProjection.Document,
                ReturnDocument = options.ReturnDocument.ToCore(),
                Sort = options.Sort == null ? null : options.Sort.Render(_documentSerializer, _settings.SerializerRegistry),
                WriteConcern = _settings.WriteConcern
            };
        }

        private FindOneAndUpdateOperation<TProjection> CreateFindOneAndUpdateOperation<TProjection>(FilterDefinition<TDocument> filter, UpdateDefinition<TDocument> update, FindOneAndUpdateOptions<TDocument, TProjection> options)
        {
            var projection = options.Projection ?? new ClientSideDeserializationProjectionDefinition<TDocument, TProjection>();
            var renderedProjection = projection.Render(_documentSerializer, _settings.SerializerRegistry);

            return new FindOneAndUpdateOperation<TProjection>(
                _collectionNamespace,
                filter.Render(_documentSerializer, _settings.SerializerRegistry),
                update.Render(_documentSerializer, _settings.SerializerRegistry),
                new FindAndModifyValueDeserializer<TProjection>(renderedProjection.ProjectionSerializer),
                _messageEncoderSettings)
            {
                ArrayFilters = RenderArrayFilters(options.ArrayFilters),
                BypassDocumentValidation = options.BypassDocumentValidation,
                Collation = options.Collation,
                IsUpsert = options.IsUpsert,
                MaxTime = options.MaxTime,
                Projection = renderedProjection.Document,
                ReturnDocument = options.ReturnDocument.ToCore(),
                Sort = options.Sort == null ? null : options.Sort.Render(_documentSerializer, _settings.SerializerRegistry),
                WriteConcern = _settings.WriteConcern
            };
        }

        private FindOperation<TProjection> CreateFindOperation<TProjection>(FilterDefinition<TDocument> filter, FindOptions<TDocument, TProjection> options)
        {
            var projection = options.Projection ?? new ClientSideDeserializationProjectionDefinition<TDocument, TProjection>();
            var renderedProjection = projection.Render(_documentSerializer, _settings.SerializerRegistry);

            return new FindOperation<TProjection>(
                _collectionNamespace,
                renderedProjection.ProjectionSerializer,
                _messageEncoderSettings)
            {
                AllowPartialResults = options.AllowPartialResults,
                BatchSize = options.BatchSize,
                Collation = options.Collation,
                Comment = options.Comment,
                CursorType = options.CursorType.ToCore(),
                Filter = filter.Render(_documentSerializer, _settings.SerializerRegistry),
                Limit = options.Limit,
                MaxAwaitTime = options.MaxAwaitTime,
                MaxTime = options.MaxTime,
                Modifiers = options.Modifiers,
                NoCursorTimeout = options.NoCursorTimeout,
                OplogReplay = options.OplogReplay,
                Projection = renderedProjection.Document,
                ReadConcern = _settings.ReadConcern,
                Skip = options.Skip,
                Sort = options.Sort == null ? null : options.Sort.Render(_documentSerializer, _settings.SerializerRegistry)
            };
        }

        private MapReduceOperation<TResult> CreateMapReduceOperation<TResult>(BsonJavaScript map, BsonJavaScript reduce, MapReduceOptions<TDocument, TResult> options, IBsonSerializer<TResult> resultSerializer)
        {
            return new MapReduceOperation<TResult>(
                _collectionNamespace,
                map,
                reduce,
                resultSerializer,
                _messageEncoderSettings)
            {
                Collation = options.Collation,
                Filter = options.Filter == null ? null : options.Filter.Render(_documentSerializer, _settings.SerializerRegistry),
                FinalizeFunction = options.Finalize,
                JavaScriptMode = options.JavaScriptMode,
                Limit = options.Limit,
                MaxTime = options.MaxTime,
                ReadConcern = _settings.ReadConcern,
                Scope = options.Scope,
                Sort = options.Sort == null ? null : options.Sort.Render(_documentSerializer, _settings.SerializerRegistry),
                Verbose = options.Verbose
            };
        }

        private MapReduceOutputToCollectionOperation CreateMapReduceOutputToCollectionOperation<TResult>(BsonJavaScript map, BsonJavaScript reduce, MapReduceOptions<TDocument, TResult> options, MapReduceOutputOptions outputOptions)
        {
            var collectionOutputOptions = (MapReduceOutputOptions.CollectionOutput)outputOptions;
            var databaseNamespace = collectionOutputOptions.DatabaseName == null ?
                _collectionNamespace.DatabaseNamespace :
                new DatabaseNamespace(collectionOutputOptions.DatabaseName);
            var outputCollectionNamespace = new CollectionNamespace(databaseNamespace, collectionOutputOptions.CollectionName);

            return new MapReduceOutputToCollectionOperation(
                _collectionNamespace,
                outputCollectionNamespace,
                map,
                reduce,
                _messageEncoderSettings)
            {
                BypassDocumentValidation = options.BypassDocumentValidation,
                Collation = options.Collation,
                Filter = options.Filter == null ? null : options.Filter.Render(_documentSerializer, _settings.SerializerRegistry),
                FinalizeFunction = options.Finalize,
                JavaScriptMode = options.JavaScriptMode,
                Limit = options.Limit,
                MaxTime = options.MaxTime,
                NonAtomicOutput = collectionOutputOptions.NonAtomic,
                Scope = options.Scope,
                OutputMode = collectionOutputOptions.OutputMode,
                ShardedOutput = collectionOutputOptions.Sharded,
                Sort = options.Sort == null ? null : options.Sort.Render(_documentSerializer, _settings.SerializerRegistry),
                Verbose = options.Verbose,
                WriteConcern = _settings.WriteConcern
            };
        }

        private FindOperation<TResult> CreateMapReduceOutputToCollectionFindOperation<TResult>(MapReduceOptions<TDocument, TResult> options, CollectionNamespace outputCollectionNamespace, IBsonSerializer<TResult> resultSerializer)
        {
            return new FindOperation<TResult>(
                outputCollectionNamespace,
                resultSerializer,
                _messageEncoderSettings)
            {
                Collation = options.Collation,
                MaxTime = options.MaxTime,
                ReadConcern = _settings.ReadConcern
            };
        }

        private IBsonSerializer<TField> GetValueSerializerForDistinct<TField>(RenderedFieldDefinition<TField> renderedField, IBsonSerializerRegistry serializerRegistry)
        {
            if (renderedField.UnderlyingSerializer != null)
            {
                if (renderedField.UnderlyingSerializer.ValueType == typeof(TField))
                {
                    return (IBsonSerializer<TField>)renderedField.UnderlyingSerializer;
                }

                var arraySerializer = renderedField.UnderlyingSerializer as IBsonArraySerializer;
                if (arraySerializer != null)
                {
                    BsonSerializationInfo itemSerializationInfo;
                    if (arraySerializer.TryGetItemSerializationInfo(out itemSerializationInfo))
                    {
                        if (itemSerializationInfo.Serializer.ValueType == typeof(TField))
                        {
                            return (IBsonSerializer<TField>)itemSerializationInfo.Serializer;
                        }
                    }
                }
            }

            return serializerRegistry.GetSerializer<TField>();
        }

        private TResult ExecuteReadOperation<TResult>(IReadOperation<TResult> operation, CancellationToken cancellationToken)
        {
            return ExecuteReadOperation(operation, _settings.ReadPreference, cancellationToken);
        }

        private TResult ExecuteReadOperation<TResult>(IReadOperation<TResult> operation, ReadPreference readPreference, CancellationToken cancellationToken)
        {
            using (var binding = new ReadPreferenceBinding(_cluster, readPreference))
            {
                return _operationExecutor.ExecuteReadOperation(binding, operation, cancellationToken);
            }
        }

        private Task<TResult> ExecuteReadOperationAsync<TResult>(IReadOperation<TResult> operation, CancellationToken cancellationToken)
        {
            return ExecuteReadOperationAsync(operation, _settings.ReadPreference, cancellationToken);
        }

        private async Task<TResult> ExecuteReadOperationAsync<TResult>(IReadOperation<TResult> operation, ReadPreference readPreference, CancellationToken cancellationToken)
        {
            using (var binding = new ReadPreferenceBinding(_cluster, readPreference))
            {
                return await _operationExecutor.ExecuteReadOperationAsync(binding, operation, cancellationToken).ConfigureAwait(false);
            }
        }

        private TResult ExecuteWriteOperation<TResult>(IWriteOperation<TResult> operation, CancellationToken cancellationToken)
        {
            using (var binding = new WritableServerBinding(_cluster))
            {
                return _operationExecutor.ExecuteWriteOperation(binding, operation, cancellationToken);
            }
        }

        private async Task<TResult> ExecuteWriteOperationAsync<TResult>(IWriteOperation<TResult> operation, CancellationToken cancellationToken)
        {
            using (var binding = new WritableServerBinding(_cluster))
            {
                return await _operationExecutor.ExecuteWriteOperationAsync(binding, operation, cancellationToken).ConfigureAwait(false);
            }
        }

        private IEnumerable<BsonDocument> RenderArrayFilters(IEnumerable<ArrayFilterDefinition> arrayFilters)
        {
            if (arrayFilters == null)
            {
                return null;
            }

            var renderedArrayFilters = new List<BsonDocument>();
            foreach (var arrayFilter in arrayFilters)
            {
                var renderedArrayFilter = arrayFilter.Render(null, _settings.SerializerRegistry);
                renderedArrayFilters.Add(renderedArrayFilter);
            }

            return renderedArrayFilters;
        }

        private IBsonSerializer<TResult> ResolveResultSerializer<TResult>(IBsonSerializer<TResult> resultSerializer)
        {
            if (resultSerializer != null)
            {
                return resultSerializer;
            }

            if (typeof(TResult) == typeof(TDocument) && _documentSerializer != null)
            {
                return (IBsonSerializer<TResult>)_documentSerializer;
            }

            return _settings.SerializerRegistry.GetSerializer<TResult>();
        }

        private class MongoIndexManager : MongoIndexManagerBase<TDocument>
        {
            // private fields
            private readonly MongoCollectionImpl<TDocument> _collection;

            // constructors
            public MongoIndexManager(MongoCollectionImpl<TDocument> collection)
            {
                _collection = collection;
            }

            // public properties
            public override CollectionNamespace CollectionNamespace
            {
                get { return _collection.CollectionNamespace; }
            }

            public override IBsonSerializer<TDocument> DocumentSerializer
            {
                get { return _collection.DocumentSerializer; }
            }

            public override MongoCollectionSettings Settings
            {
                get { return _collection._settings; }
            }

            // public methods
            public override IEnumerable<string> CreateMany(IEnumerable<CreateIndexModel<TDocument>> models, CancellationToken cancellationToken = default(CancellationToken))
            {
                Ensure.IsNotNull(models, nameof(models));

                var requests = CreateCreateIndexRequests(models);
                var operation = CreateCreateIndexesOperation(requests);
                _collection.ExecuteWriteOperation(operation, cancellationToken);

                return requests.Select(x => x.GetIndexName());
            }

            public async override Task<IEnumerable<string>> CreateManyAsync(IEnumerable<CreateIndexModel<TDocument>> models, CancellationToken cancellationToken = default(CancellationToken))
            {
                Ensure.IsNotNull(models, nameof(models));

                var requests = CreateCreateIndexRequests(models);
                var operation = CreateCreateIndexesOperation(requests);
                await _collection.ExecuteWriteOperationAsync(operation, cancellationToken).ConfigureAwait(false);

                return requests.Select(x => x.GetIndexName());
            }

            public override void DropAll(CancellationToken cancellationToken)
            {
                var operation = CreateDropAllOperation();
                _collection.ExecuteWriteOperation(operation, cancellationToken);
            }

            public override Task DropAllAsync(CancellationToken cancellationToken)
            {
                var operation = CreateDropAllOperation();
                return _collection.ExecuteWriteOperationAsync(operation, cancellationToken);
            }

            public override void DropOne(string name, CancellationToken cancellationToken)
            {
                Ensure.IsNotNullOrEmpty(name, nameof(name));
                if (name == "*")
                {
                    throw new ArgumentException("Cannot specify '*' for the index name. Use DropAllAsync to drop all indexes.", "name");
                }

                var operation = CreateDropOneOperation(name);
                _collection.ExecuteWriteOperation(operation, cancellationToken);
            }

            public override Task DropOneAsync(string name, CancellationToken cancellationToken)
            {
                Ensure.IsNotNullOrEmpty(name, nameof(name));
                if (name == "*")
                {
                    throw new ArgumentException("Cannot specify '*' for the index name. Use DropAllAsync to drop all indexes.", "name");
                }

                var operation = CreateDropOneOperation(name);
                return _collection.ExecuteWriteOperationAsync(operation, cancellationToken);
            }

            public override IAsyncCursor<BsonDocument> List(CancellationToken cancellationToken = default(CancellationToken))
            {
                var operation = CreateListIndexesOperation();
                return _collection.ExecuteReadOperation(operation, ReadPreference.Primary, cancellationToken);
            }

            public override Task<IAsyncCursor<BsonDocument>> ListAsync(CancellationToken cancellationToken = default(CancellationToken))
            {
                var operation = CreateListIndexesOperation();
                return _collection.ExecuteReadOperationAsync(operation, ReadPreference.Primary, cancellationToken);
            }

            // private methods
            private CreateIndexesOperation CreateCreateIndexesOperation(IEnumerable<CreateIndexRequest> requests)
            {
                return new CreateIndexesOperation(_collection._collectionNamespace, requests, _collection._messageEncoderSettings)
                {
                    WriteConcern = _collection.Settings.WriteConcern
                };
            }

            private IEnumerable<CreateIndexRequest> CreateCreateIndexRequests(IEnumerable<CreateIndexModel<TDocument>> models)
            {
                return models.Select(m =>
                {
                    var options = m.Options ?? new CreateIndexOptions<TDocument>();
                    var keysDocument = m.Keys.Render(_collection._documentSerializer, _collection._settings.SerializerRegistry);
                    var renderedPartialFilterExpression = options.PartialFilterExpression == null ? null : options.PartialFilterExpression.Render(_collection._documentSerializer, _collection._settings.SerializerRegistry);

                    return new CreateIndexRequest(keysDocument)
                    {
                        Name = options.Name,
                        Background = options.Background,
                        Bits = options.Bits,
                        BucketSize = options.BucketSize,
                        Collation = options.Collation,
                        DefaultLanguage = options.DefaultLanguage,
                        ExpireAfter = options.ExpireAfter,
                        LanguageOverride = options.LanguageOverride,
                        Max = options.Max,
                        Min = options.Min,
                        PartialFilterExpression = renderedPartialFilterExpression,
                        Sparse = options.Sparse,
                        SphereIndexVersion = options.SphereIndexVersion,
                        StorageEngine = options.StorageEngine,
                        TextIndexVersion = options.TextIndexVersion,
                        Unique = options.Unique,
                        Version = options.Version,
                        Weights = options.Weights
                    };
                });
            }

            private DropIndexOperation CreateDropAllOperation()
            {
                return new DropIndexOperation(_collection._collectionNamespace, "*", _collection._messageEncoderSettings)
                {
                    WriteConcern = _collection.Settings.WriteConcern
                };
            }

            private DropIndexOperation CreateDropOneOperation(string name)
            {
                return new DropIndexOperation(_collection._collectionNamespace, name, _collection._messageEncoderSettings)
                {
                    WriteConcern = _collection.Settings.WriteConcern
                };
            }

            private ListIndexesOperation CreateListIndexesOperation()
            {
                return new ListIndexesOperation(_collection._collectionNamespace, _collection._messageEncoderSettings);
            }
        }
    }
}

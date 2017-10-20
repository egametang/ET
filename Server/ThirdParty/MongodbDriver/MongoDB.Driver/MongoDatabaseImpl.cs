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
using System.Reflection;
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

namespace MongoDB.Driver
{
    internal sealed class MongoDatabaseImpl : MongoDatabaseBase
    {
        // private fields
        private readonly IMongoClient _client;
        private readonly ICluster _cluster;
        private readonly DatabaseNamespace _databaseNamespace;
        private readonly IOperationExecutor _operationExecutor;
        private readonly MongoDatabaseSettings _settings;

        // constructors
        public MongoDatabaseImpl(IMongoClient client, DatabaseNamespace databaseNamespace, MongoDatabaseSettings settings, ICluster cluster, IOperationExecutor operationExecutor)
        {
            _client = Ensure.IsNotNull(client, nameof(client));
            _databaseNamespace = Ensure.IsNotNull(databaseNamespace, nameof(databaseNamespace));
            _settings = Ensure.IsNotNull(settings, nameof(settings)).Freeze();
            _cluster = Ensure.IsNotNull(cluster, nameof(cluster));
            _operationExecutor = Ensure.IsNotNull(operationExecutor, nameof(operationExecutor));
        }

        // public properties
        public override IMongoClient Client
        {
            get { return _client; }
        }

        public override DatabaseNamespace DatabaseNamespace
        {
            get { return _databaseNamespace; }
        }

        public override MongoDatabaseSettings Settings
        {
            get { return _settings; }
        }

        // public methods
        public override void CreateCollection(string name, CreateCollectionOptions options, CancellationToken cancellationToken)
        {
            Ensure.IsNotNullOrEmpty(name, nameof(name));

            if (options == null)
            {
                CreateCollectionHelper<BsonDocument>(name, null, cancellationToken);
                return;
            }

            if (options.GetType() == typeof(CreateCollectionOptions))
            {
                var genericOptions = CreateCollectionOptions<BsonDocument>.CoercedFrom(options);
                CreateCollectionHelper<BsonDocument>(name, genericOptions, cancellationToken);
                return;
            }

            var genericMethodDefinition = typeof(MongoDatabaseImpl).GetTypeInfo().GetMethod("CreateCollectionHelper", BindingFlags.NonPublic | BindingFlags.Instance);
            var documentType = options.GetType().GetTypeInfo().GetGenericArguments()[0];
            var methodInfo = genericMethodDefinition.MakeGenericMethod(documentType);
            methodInfo.Invoke(this, new object[] { name, options, cancellationToken });
        }

        public override Task CreateCollectionAsync(string name, CreateCollectionOptions options, CancellationToken cancellationToken)
        {
            Ensure.IsNotNullOrEmpty(name, nameof(name));

            if (options == null)
            {
                return CreateCollectionHelperAsync<BsonDocument>(name, null, cancellationToken);
            }

            if (options.GetType() == typeof(CreateCollectionOptions))
            {
                var genericOptions = CreateCollectionOptions<BsonDocument>.CoercedFrom(options);
                return CreateCollectionHelperAsync<BsonDocument>(name, genericOptions, cancellationToken);
            }

            var genericMethodDefinition = typeof(MongoDatabaseImpl).GetTypeInfo().GetMethod("CreateCollectionHelperAsync", BindingFlags.NonPublic | BindingFlags.Instance);
            var documentType = options.GetType().GetTypeInfo().GetGenericArguments()[0];
            var methodInfo = genericMethodDefinition.MakeGenericMethod(documentType);
            return (Task)methodInfo.Invoke(this, new object[] { name, options, cancellationToken });
        }

        public override void CreateView<TDocument, TResult>(string viewName, string viewOn, PipelineDefinition<TDocument, TResult> pipeline, CreateViewOptions<TDocument> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(viewName, nameof(viewName));
            Ensure.IsNotNull(viewOn, nameof(viewOn));
            Ensure.IsNotNull(pipeline, nameof(pipeline));
            options = options ?? new CreateViewOptions<TDocument>();
            var operation = CreateCreateViewOperation(viewName, viewOn, pipeline, options);
            ExecuteWriteOperation(operation, cancellationToken);
        }

        public override Task CreateViewAsync<TDocument, TResult>(string viewName, string viewOn, PipelineDefinition<TDocument, TResult> pipeline, CreateViewOptions<TDocument> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(viewName, nameof(viewName));
            Ensure.IsNotNull(viewOn, nameof(viewOn));
            Ensure.IsNotNull(pipeline, nameof(pipeline));
            options = options ?? new CreateViewOptions<TDocument>();
            var operation = CreateCreateViewOperation(viewName, viewOn, pipeline, options);
            return ExecuteWriteOperationAsync(operation, cancellationToken);
        }

        public override void DropCollection(string name, CancellationToken cancellationToken)
        {
            Ensure.IsNotNullOrEmpty(name, nameof(name));
            var operation = CreateDropCollectionOperation(name);
            ExecuteWriteOperation(operation, cancellationToken);
        }

        public override Task DropCollectionAsync(string name, CancellationToken cancellationToken)
        {
            Ensure.IsNotNullOrEmpty(name, nameof(name));
            var operation = CreateDropCollectionOperation(name);
            return ExecuteWriteOperationAsync(operation, cancellationToken);
        }

        public override IMongoCollection<TDocument> GetCollection<TDocument>(string name, MongoCollectionSettings settings)
        {
            Ensure.IsNotNullOrEmpty(name, nameof(name));

            settings = settings == null ?
                new MongoCollectionSettings() :
                settings.Clone();

            settings.ApplyDefaultValues(_settings);

            return new MongoCollectionImpl<TDocument>(this, new CollectionNamespace(_databaseNamespace, name), settings, _cluster, _operationExecutor);
        }

        public override IAsyncCursor<BsonDocument> ListCollections(ListCollectionsOptions options, CancellationToken cancellationToken)
        {
            options = options ?? new ListCollectionsOptions();
            var operation = CreateListCollectionsOperation(options);
            return ExecuteReadOperation(operation, ReadPreference.Primary, cancellationToken);
        }

        public override Task<IAsyncCursor<BsonDocument>> ListCollectionsAsync(ListCollectionsOptions options, CancellationToken cancellationToken)
        {
            options = options ?? new ListCollectionsOptions();
            var operation = CreateListCollectionsOperation(options);
            return ExecuteReadOperationAsync(operation, ReadPreference.Primary, cancellationToken);
        }

        public override void RenameCollection(string oldName, string newName, RenameCollectionOptions options, CancellationToken cancellationToken)
        {
            Ensure.IsNotNullOrEmpty(oldName, nameof(oldName));
            Ensure.IsNotNullOrEmpty(newName, nameof(newName));
            options = options ?? new RenameCollectionOptions();

            var operation = CreateRenameCollectionOperation(oldName, newName, options);
            ExecuteWriteOperation(operation, cancellationToken);
        }

        public override Task RenameCollectionAsync(string oldName, string newName, RenameCollectionOptions options, CancellationToken cancellationToken)
        {
            Ensure.IsNotNullOrEmpty(oldName, nameof(oldName));
            Ensure.IsNotNullOrEmpty(newName, nameof(newName));
            options = options ?? new RenameCollectionOptions();

            var operation = CreateRenameCollectionOperation(oldName, newName, options);
            return ExecuteWriteOperationAsync(operation, cancellationToken);
        }

        public override TResult RunCommand<TResult>(Command<TResult> command, ReadPreference readPreference = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(command, nameof(command));
            readPreference = readPreference ?? ReadPreference.Primary;

            var operation = CreateRunCommandOperation(command);
            return ExecuteReadOperation(operation, readPreference, cancellationToken);
        }

        public override Task<TResult> RunCommandAsync<TResult>(Command<TResult> command, ReadPreference readPreference = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(command, nameof(command));
            readPreference = readPreference ?? ReadPreference.Primary;

            var operation = CreateRunCommandOperation(command);
            return ExecuteReadOperationAsync(operation, readPreference, cancellationToken);
        }

        public override IMongoDatabase WithReadConcern(ReadConcern readConcern)
        {
            Ensure.IsNotNull(readConcern, nameof(readConcern));
            var newSettings = _settings.Clone();
            newSettings.ReadConcern = readConcern;
            return new MongoDatabaseImpl(_client, _databaseNamespace, newSettings, _cluster, _operationExecutor);
        }

        public override IMongoDatabase WithReadPreference(ReadPreference readPreference)
        {
            Ensure.IsNotNull(readPreference, nameof(readPreference));
            var newSettings = _settings.Clone();
            newSettings.ReadPreference = readPreference;
            return new MongoDatabaseImpl(_client, _databaseNamespace, newSettings, _cluster, _operationExecutor);
        }

        public override IMongoDatabase WithWriteConcern(WriteConcern writeConcern)
        {
            Ensure.IsNotNull(writeConcern, nameof(writeConcern));
            var newSettings = _settings.Clone();
            newSettings.WriteConcern = writeConcern;
            return new MongoDatabaseImpl(_client, _databaseNamespace, newSettings, _cluster, _operationExecutor);
        }

        // private methods
        private void CreateCollectionHelper<TDocument>(string name, CreateCollectionOptions<TDocument> options, CancellationToken cancellationToken)
        {
            options = options ?? new CreateCollectionOptions<TDocument>();

            var operation = CreateCreateCollectionOperation(name, options);
            ExecuteWriteOperation(operation, cancellationToken);
        }

        private Task CreateCollectionHelperAsync<TDocument>(string name, CreateCollectionOptions<TDocument> options, CancellationToken cancellationToken)
        {
            options = options ?? new CreateCollectionOptions<TDocument>();

            var operation = CreateCreateCollectionOperation(name, options);
            return ExecuteWriteOperationAsync(operation, cancellationToken);
        }

        private CreateCollectionOperation CreateCreateCollectionOperation(string name, CreateCollectionOptions options)
        {
            options = options ?? new CreateCollectionOptions();
            var messageEncoderSettings = GetMessageEncoderSettings();

            return new CreateCollectionOperation(new CollectionNamespace(_databaseNamespace, name), messageEncoderSettings)
            {
                AutoIndexId = options.AutoIndexId,
                Collation = options.Collation,
                Capped = options.Capped,
                MaxDocuments = options.MaxDocuments,
                MaxSize = options.MaxSize,
                NoPadding = options.NoPadding,
                StorageEngine = options.StorageEngine,
                UsePowerOf2Sizes = options.UsePowerOf2Sizes,
                WriteConcern = _settings.WriteConcern
            };
        }

        private CreateCollectionOperation CreateCreateCollectionOperation<TDocument>(string name, CreateCollectionOptions<TDocument> options)
        {
            var messageEncoderSettings = GetMessageEncoderSettings();
            BsonDocument validator = null;
            if (options.Validator != null)
            {
                var serializerRegistry = options.SerializerRegistry ?? BsonSerializer.SerializerRegistry;
                var documentSerializer = options.DocumentSerializer ?? serializerRegistry.GetSerializer<TDocument>();
                validator = options.Validator.Render(documentSerializer, serializerRegistry);
            }

            return new CreateCollectionOperation(new CollectionNamespace(_databaseNamespace, name), messageEncoderSettings)
            {
                AutoIndexId = options.AutoIndexId,
                Capped = options.Capped,
                Collation = options.Collation,
                IndexOptionDefaults = options.IndexOptionDefaults?.ToBsonDocument(),
                MaxDocuments = options.MaxDocuments,
                MaxSize = options.MaxSize,
                NoPadding = options.NoPadding,
                StorageEngine = options.StorageEngine,
                UsePowerOf2Sizes = options.UsePowerOf2Sizes,
                ValidationAction = options.ValidationAction,
                ValidationLevel = options.ValidationLevel,
                Validator = validator,
                WriteConcern = _settings.WriteConcern
            };
        }

        private CreateViewOperation CreateCreateViewOperation<TDocument, TResult>(string viewName, string viewOn, PipelineDefinition<TDocument, TResult> pipeline, CreateViewOptions<TDocument> options)
        {
            var serializerRegistry = options.SerializerRegistry ?? BsonSerializer.SerializerRegistry;
            var documentSerializer = options.DocumentSerializer ?? serializerRegistry.GetSerializer<TDocument>();
            var pipelineDocuments = pipeline.Render(documentSerializer, serializerRegistry).Documents;
            return new CreateViewOperation(_databaseNamespace, viewName, viewOn, pipelineDocuments, GetMessageEncoderSettings())
            {
                Collation = options.Collation,
                WriteConcern = _settings.WriteConcern
            };
        }

        private DropCollectionOperation CreateDropCollectionOperation(string name)
        {
            var collectionNamespace = new CollectionNamespace(_databaseNamespace, name);
            var messageEncoderSettings = GetMessageEncoderSettings();
            return new DropCollectionOperation(collectionNamespace, messageEncoderSettings)
            {
                WriteConcern = _settings.WriteConcern
            };
        }

        private ListCollectionsOperation CreateListCollectionsOperation(ListCollectionsOptions options)
        {
            var messageEncoderSettings = GetMessageEncoderSettings();
            return new ListCollectionsOperation(_databaseNamespace, messageEncoderSettings)
            {
                Filter = options.Filter?.Render(_settings.SerializerRegistry.GetSerializer<BsonDocument>(), _settings.SerializerRegistry)
            };
        }

        private RenameCollectionOperation CreateRenameCollectionOperation(string oldName, string newName, RenameCollectionOptions options)
        {
            var messageEncoderSettings = GetMessageEncoderSettings();
            return new RenameCollectionOperation(
                new CollectionNamespace(_databaseNamespace, oldName),
                new CollectionNamespace(_databaseNamespace, newName),
                messageEncoderSettings)
            {
                DropTarget = options.DropTarget,
                WriteConcern = _settings.WriteConcern
            };
        }

        private ReadCommandOperation<TResult> CreateRunCommandOperation<TResult>(Command<TResult> command)
        {
            var renderedCommand = command.Render(_settings.SerializerRegistry);
            var messageEncoderSettings = GetMessageEncoderSettings();
            return new ReadCommandOperation<TResult>(_databaseNamespace, renderedCommand.Document, renderedCommand.ResultSerializer, messageEncoderSettings);
        }

        private T ExecuteReadOperation<T>(IReadOperation<T> operation, ReadPreference readPreference, CancellationToken cancellationToken)
        {
            using (var binding = new ReadPreferenceBinding(_cluster, readPreference))
            {
                return _operationExecutor.ExecuteReadOperation(binding, operation, cancellationToken);
            }
        }

        private async Task<T> ExecuteReadOperationAsync<T>(IReadOperation<T> operation, ReadPreference readPreference, CancellationToken cancellationToken)
        {
            using (var binding = new ReadPreferenceBinding(_cluster, readPreference))
            {
                return await _operationExecutor.ExecuteReadOperationAsync(binding, operation, cancellationToken).ConfigureAwait(false);
            }
        }

        private T ExecuteWriteOperation<T>(IWriteOperation<T> operation, CancellationToken cancellationToken)
        {
            using (var binding = new WritableServerBinding(_cluster))
            {
                return _operationExecutor.ExecuteWriteOperation(binding, operation, cancellationToken);
            }
        }

        private async Task<T> ExecuteWriteOperationAsync<T>(IWriteOperation<T> operation, CancellationToken cancellationToken)
        {
            using (var binding = new WritableServerBinding(_cluster))
            {
                return await _operationExecutor.ExecuteWriteOperationAsync(binding, operation, cancellationToken).ConfigureAwait(false);
            }
        }

        private MessageEncoderSettings GetMessageEncoderSettings()
        {
            return new MessageEncoderSettings
            {
                { MessageEncoderSettingsName.GuidRepresentation, _settings.GuidRepresentation },
                { MessageEncoderSettingsName.ReadEncoding, _settings.ReadEncoding ?? Utf8Encodings.Strict },
                { MessageEncoderSettingsName.WriteEncoding, _settings.WriteEncoding ?? Utf8Encodings.Strict }
            };
        }
    }
}

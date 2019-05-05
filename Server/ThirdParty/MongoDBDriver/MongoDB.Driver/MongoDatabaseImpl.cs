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
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
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
            UsingImplicitSession(session => CreateCollection(session, name, options, cancellationToken), cancellationToken);
        }

        public override void CreateCollection(IClientSessionHandle session, string name, CreateCollectionOptions options, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(session, nameof(session));
            Ensure.IsNotNullOrEmpty(name, nameof(name));

            if (options == null)
            {
                CreateCollectionHelper<BsonDocument>(session, name, null, cancellationToken);
                return;
            }

            if (options.GetType() == typeof(CreateCollectionOptions))
            {
                var genericOptions = CreateCollectionOptions<BsonDocument>.CoercedFrom(options);
                CreateCollectionHelper<BsonDocument>(session, name, genericOptions, cancellationToken);
                return;
            }

            var genericMethodDefinition = typeof(MongoDatabaseImpl).GetTypeInfo().GetMethod("CreateCollectionHelper", BindingFlags.NonPublic | BindingFlags.Instance);
            var documentType = options.GetType().GetTypeInfo().GetGenericArguments()[0];
            var methodInfo = genericMethodDefinition.MakeGenericMethod(documentType);
            methodInfo.Invoke(this, new object[] { session, name, options, cancellationToken });
        }

        public override Task CreateCollectionAsync(string name, CreateCollectionOptions options, CancellationToken cancellationToken)
        {
            return UsingImplicitSessionAsync(session => CreateCollectionAsync(session, name, options, cancellationToken), cancellationToken);
        }

        public override Task CreateCollectionAsync(IClientSessionHandle session, string name, CreateCollectionOptions options, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(session, nameof(session));
            Ensure.IsNotNullOrEmpty(name, nameof(name));

            if (options == null)
            {
                return CreateCollectionHelperAsync<BsonDocument>(session, name, null, cancellationToken);
            }

            if (options.GetType() == typeof(CreateCollectionOptions))
            {
                var genericOptions = CreateCollectionOptions<BsonDocument>.CoercedFrom(options);
                return CreateCollectionHelperAsync<BsonDocument>(session, name, genericOptions, cancellationToken);
            }

            var genericMethodDefinition = typeof(MongoDatabaseImpl).GetTypeInfo().GetMethod("CreateCollectionHelperAsync", BindingFlags.NonPublic | BindingFlags.Instance);
            var documentType = options.GetType().GetTypeInfo().GetGenericArguments()[0];
            var methodInfo = genericMethodDefinition.MakeGenericMethod(documentType);
            return (Task)methodInfo.Invoke(this, new object[] { session, name, options, cancellationToken });
        }

        public override void CreateView<TDocument, TResult>(string viewName, string viewOn, PipelineDefinition<TDocument, TResult> pipeline, CreateViewOptions<TDocument> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            UsingImplicitSession(session => CreateView(session, viewName, viewOn, pipeline, options, cancellationToken), cancellationToken);
        }

        public override void CreateView<TDocument, TResult>(IClientSessionHandle session, string viewName, string viewOn, PipelineDefinition<TDocument, TResult> pipeline, CreateViewOptions<TDocument> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(session, nameof(session));
            Ensure.IsNotNull(viewName, nameof(viewName));
            Ensure.IsNotNull(viewOn, nameof(viewOn));
            Ensure.IsNotNull(pipeline, nameof(pipeline));
            options = options ?? new CreateViewOptions<TDocument>();
            var operation = CreateCreateViewOperation(viewName, viewOn, pipeline, options);
            ExecuteWriteOperation(session, operation, cancellationToken);
        }

        public override Task CreateViewAsync<TDocument, TResult>(string viewName, string viewOn, PipelineDefinition<TDocument, TResult> pipeline, CreateViewOptions<TDocument> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return UsingImplicitSessionAsync(session => CreateViewAsync(session, viewName, viewOn, pipeline, options, cancellationToken), cancellationToken);
        }

        public override Task CreateViewAsync<TDocument, TResult>(IClientSessionHandle session, string viewName, string viewOn, PipelineDefinition<TDocument, TResult> pipeline, CreateViewOptions<TDocument> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(session, nameof(session));
            Ensure.IsNotNull(viewName, nameof(viewName));
            Ensure.IsNotNull(viewOn, nameof(viewOn));
            Ensure.IsNotNull(pipeline, nameof(pipeline));
            options = options ?? new CreateViewOptions<TDocument>();
            var operation = CreateCreateViewOperation(viewName, viewOn, pipeline, options);
            return ExecuteWriteOperationAsync(session, operation, cancellationToken);
        }

        public override void DropCollection(string name, CancellationToken cancellationToken)
        {
            UsingImplicitSession(session => DropCollection(session, name, cancellationToken), cancellationToken);
        }

        public override void DropCollection(IClientSessionHandle session, string name, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(session, nameof(session));
            Ensure.IsNotNullOrEmpty(name, nameof(name));
            var operation = CreateDropCollectionOperation(name);
            ExecuteWriteOperation(session, operation, cancellationToken);
        }

        public override Task DropCollectionAsync(string name, CancellationToken cancellationToken)
        {
            return UsingImplicitSessionAsync(session => DropCollectionAsync(session, name, cancellationToken), cancellationToken);
        }

        public override Task DropCollectionAsync(IClientSessionHandle session, string name, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(session, nameof(session));
            Ensure.IsNotNullOrEmpty(name, nameof(name));
            var operation = CreateDropCollectionOperation(name);
            return ExecuteWriteOperationAsync(session, operation, cancellationToken);
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

        public override IAsyncCursor<string> ListCollectionNames(ListCollectionNamesOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return UsingImplicitSession(session => ListCollectionNames(session, options, cancellationToken), cancellationToken);
        }

        public override IAsyncCursor<string> ListCollectionNames(IClientSessionHandle session, ListCollectionNamesOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(session, nameof(session));
            var operation = CreateListCollectionNamesOperation(options);
            var effectiveReadPreference = ReadPreferenceResolver.GetEffectiveReadPreference(session, null, ReadPreference.Primary);
            var cursor = ExecuteReadOperation(session, operation, effectiveReadPreference, cancellationToken);
            return new BatchTransformingAsyncCursor<BsonDocument, string>(cursor, ExtractCollectionNames);
        }

        public override Task<IAsyncCursor<string>> ListCollectionNamesAsync(ListCollectionNamesOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return UsingImplicitSessionAsync(session => ListCollectionNamesAsync(session, options, cancellationToken), cancellationToken);
        }

        public override async Task<IAsyncCursor<string>> ListCollectionNamesAsync(IClientSessionHandle session, ListCollectionNamesOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(session, nameof(session));
            var operation = CreateListCollectionNamesOperation(options);
            var effectiveReadPreference = ReadPreferenceResolver.GetEffectiveReadPreference(session, null, ReadPreference.Primary);
            var cursor = await ExecuteReadOperationAsync(session, operation, effectiveReadPreference, cancellationToken).ConfigureAwait(false);
            return new BatchTransformingAsyncCursor<BsonDocument, string>(cursor, ExtractCollectionNames);
        }

        public override IAsyncCursor<BsonDocument> ListCollections(ListCollectionsOptions options, CancellationToken cancellationToken)
        {
            return UsingImplicitSession(session => ListCollections(session, options, cancellationToken), cancellationToken);
        }

        public override IAsyncCursor<BsonDocument> ListCollections(IClientSessionHandle session, ListCollectionsOptions options, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(session, nameof(session));
            var operation = CreateListCollectionsOperation(options);
            var effectiveReadPreference = ReadPreferenceResolver.GetEffectiveReadPreference(session, null, ReadPreference.Primary);
            return ExecuteReadOperation(session, operation, effectiveReadPreference, cancellationToken);
        }

        public override Task<IAsyncCursor<BsonDocument>> ListCollectionsAsync(ListCollectionsOptions options, CancellationToken cancellationToken)
        {
            return UsingImplicitSessionAsync(session => ListCollectionsAsync(session, options, cancellationToken), cancellationToken);
        }

        public override Task<IAsyncCursor<BsonDocument>> ListCollectionsAsync(IClientSessionHandle session, ListCollectionsOptions options, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(session, nameof(session));
            var operation = CreateListCollectionsOperation(options);
            var effectiveReadPreference = ReadPreferenceResolver.GetEffectiveReadPreference(session, null, ReadPreference.Primary);
            return ExecuteReadOperationAsync(session, operation, effectiveReadPreference, cancellationToken);
        }

        public override void RenameCollection(string oldName, string newName, RenameCollectionOptions options, CancellationToken cancellationToken)
        {
            UsingImplicitSession(session => RenameCollection(session, oldName, newName, options, cancellationToken), cancellationToken);
        }

        public override void RenameCollection(IClientSessionHandle session, string oldName, string newName, RenameCollectionOptions options, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(session, nameof(session));
            Ensure.IsNotNullOrEmpty(oldName, nameof(oldName));
            Ensure.IsNotNullOrEmpty(newName, nameof(newName));
            options = options ?? new RenameCollectionOptions();

            var operation = CreateRenameCollectionOperation(oldName, newName, options);
            ExecuteWriteOperation(session, operation, cancellationToken);
        }

        public override Task RenameCollectionAsync(string oldName, string newName, RenameCollectionOptions options, CancellationToken cancellationToken)
        {
            return UsingImplicitSessionAsync(session => RenameCollectionAsync(session, oldName, newName, options, cancellationToken), cancellationToken);
        }

        public override Task RenameCollectionAsync(IClientSessionHandle session, string oldName, string newName, RenameCollectionOptions options, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(session, nameof(session));
            Ensure.IsNotNullOrEmpty(oldName, nameof(oldName));
            Ensure.IsNotNullOrEmpty(newName, nameof(newName));
            options = options ?? new RenameCollectionOptions();

            var operation = CreateRenameCollectionOperation(oldName, newName, options);
            return ExecuteWriteOperationAsync(session, operation, cancellationToken);
        }

        public override TResult RunCommand<TResult>(Command<TResult> command, ReadPreference readPreference = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return UsingImplicitSession(session => RunCommand(session, command, readPreference, cancellationToken), cancellationToken);
        }

        public override TResult RunCommand<TResult>(IClientSessionHandle session, Command<TResult> command, ReadPreference readPreference = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(session, nameof(session));
            Ensure.IsNotNull(command, nameof(command));

            var operation = CreateRunCommandOperation(command);
            var effectiveReadPreference = ReadPreferenceResolver.GetEffectiveReadPreference(session, readPreference, ReadPreference.Primary);
            return ExecuteReadOperation(session, operation, effectiveReadPreference, cancellationToken);
        }

        public override Task<TResult> RunCommandAsync<TResult>(Command<TResult> command, ReadPreference readPreference = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return UsingImplicitSessionAsync(session => RunCommandAsync(session, command, readPreference, cancellationToken), cancellationToken);
        }

        public override Task<TResult> RunCommandAsync<TResult>(IClientSessionHandle session, Command<TResult> command, ReadPreference readPreference = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(session, nameof(session));
            Ensure.IsNotNull(command, nameof(command));

            var operation = CreateRunCommandOperation(command);
            var effectiveReadPreference = ReadPreferenceResolver.GetEffectiveReadPreference(session, readPreference, ReadPreference.Primary);
            return ExecuteReadOperationAsync(session, operation, effectiveReadPreference, cancellationToken);
        }

        public override IAsyncCursor<TResult> Watch<TResult>(
            PipelineDefinition<ChangeStreamDocument<BsonDocument>, TResult> pipeline,
            ChangeStreamOptions options = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return UsingImplicitSession(session => Watch(session, pipeline, options, cancellationToken), cancellationToken);
        }

        public override IAsyncCursor<TResult> Watch<TResult>(
            IClientSessionHandle session,
            PipelineDefinition<ChangeStreamDocument<BsonDocument>, TResult> pipeline,
            ChangeStreamOptions options = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(session, nameof(session));
            Ensure.IsNotNull(pipeline, nameof(pipeline));
            var operation = CreateChangeStreamOperation(pipeline, options);
            return ExecuteReadOperation(session, operation, cancellationToken);
        }

        public override Task<IAsyncCursor<TResult>> WatchAsync<TResult>(
            PipelineDefinition<ChangeStreamDocument<BsonDocument>, TResult> pipeline,
            ChangeStreamOptions options = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return UsingImplicitSessionAsync(session => WatchAsync(session, pipeline, options, cancellationToken), cancellationToken);
        }

        public override Task<IAsyncCursor<TResult>> WatchAsync<TResult>(
            IClientSessionHandle session,
            PipelineDefinition<ChangeStreamDocument<BsonDocument>, TResult> pipeline,
            ChangeStreamOptions options = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(session, nameof(session));
            Ensure.IsNotNull(pipeline, nameof(pipeline));
            var operation = CreateChangeStreamOperation(pipeline, options);
            return ExecuteReadOperationAsync(session, operation, cancellationToken);
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
        private void CreateCollectionHelper<TDocument>(IClientSessionHandle session, string name, CreateCollectionOptions<TDocument> options, CancellationToken cancellationToken)
        {
            options = options ?? new CreateCollectionOptions<TDocument>();

            var operation = CreateCreateCollectionOperation(name, options);
            ExecuteWriteOperation(session, operation, cancellationToken);
        }

        private Task CreateCollectionHelperAsync<TDocument>(IClientSessionHandle session, string name, CreateCollectionOptions<TDocument> options, CancellationToken cancellationToken)
        {
            options = options ?? new CreateCollectionOptions<TDocument>();

            var operation = CreateCreateCollectionOperation(name, options);
            return ExecuteWriteOperationAsync(session, operation, cancellationToken);
        }

        private CreateCollectionOperation CreateCreateCollectionOperation(string name, CreateCollectionOptions options)
        {
            options = options ?? new CreateCollectionOptions();
            var messageEncoderSettings = GetMessageEncoderSettings();

#pragma warning disable 618
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
#pragma warning restore
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

#pragma warning disable 618
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
#pragma warning restore
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

        private ListCollectionsOperation CreateListCollectionNamesOperation(ListCollectionNamesOptions options)
        {
            var messageEncoderSettings = GetMessageEncoderSettings();
            return new ListCollectionsOperation(_databaseNamespace, messageEncoderSettings)
            {
                Filter = options?.Filter?.Render(_settings.SerializerRegistry.GetSerializer<BsonDocument>(), _settings.SerializerRegistry),
                NameOnly = true
            };
        }

        private ListCollectionsOperation CreateListCollectionsOperation(ListCollectionsOptions options)
        {
            var messageEncoderSettings = GetMessageEncoderSettings();
            return new ListCollectionsOperation(_databaseNamespace, messageEncoderSettings)
            {
                Filter = options?.Filter?.Render(_settings.SerializerRegistry.GetSerializer<BsonDocument>(), _settings.SerializerRegistry)
            };
        }

        private IReadBinding CreateReadBinding(IClientSessionHandle session, ReadPreference readPreference)
        {
            if (session.IsInTransaction && readPreference.ReadPreferenceMode != ReadPreferenceMode.Primary)
            {
                throw new InvalidOperationException("Read preference in a transaction must be primary.");
            }

            var binding = new ReadPreferenceBinding(_cluster, readPreference, session.WrappedCoreSession.Fork());
            return new ReadBindingHandle(binding);
        }

        private IWriteBindingHandle CreateReadWriteBinding(IClientSessionHandle session)
        {
            var binding = new WritableServerBinding(_cluster, session.WrappedCoreSession.Fork());
            return new ReadWriteBindingHandle(binding);
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

        private ChangeStreamOperation<TResult> CreateChangeStreamOperation<TResult>(
            PipelineDefinition<ChangeStreamDocument<BsonDocument>, TResult> pipeline,
            ChangeStreamOptions options)
        {
            return ChangeStreamHelper.CreateChangeStreamOperation(this, pipeline, options, _settings.ReadConcern, GetMessageEncoderSettings());
        }

        private IEnumerable<string> ExtractCollectionNames(IEnumerable<BsonDocument> collections)
        {
            return collections.Select(collection => collection["name"].AsString);
        }

        private T ExecuteReadOperation<T>(IClientSessionHandle session, IReadOperation<T> operation, CancellationToken cancellationToken)
        {
            var readPreference = ReadPreferenceResolver.GetEffectiveReadPreference(session, null, _settings.ReadPreference);
            return ExecuteReadOperation(session, operation, readPreference, cancellationToken);
        }

        private T ExecuteReadOperation<T>(IClientSessionHandle session, IReadOperation<T> operation, ReadPreference readPreference, CancellationToken cancellationToken)
        {
            using (var binding = CreateReadBinding(session, readPreference))
            {
                return _operationExecutor.ExecuteReadOperation(binding, operation, cancellationToken);
            }
        }

        private Task<T> ExecuteReadOperationAsync<T>(IClientSessionHandle session, IReadOperation<T> operation, CancellationToken cancellationToken)
        {
            var readPreference = ReadPreferenceResolver.GetEffectiveReadPreference(session, null, _settings.ReadPreference);
            return ExecuteReadOperationAsync(session, operation, readPreference, cancellationToken);
        }

        private async Task<T> ExecuteReadOperationAsync<T>(IClientSessionHandle session, IReadOperation<T> operation, ReadPreference readPreference, CancellationToken cancellationToken)
        {
            using (var binding = CreateReadBinding(session, readPreference))
            {
                return await _operationExecutor.ExecuteReadOperationAsync(binding, operation, cancellationToken).ConfigureAwait(false);
            }
        }

        private T ExecuteWriteOperation<T>(IClientSessionHandle session, IWriteOperation<T> operation, CancellationToken cancellationToken)
        {
            using (var binding = CreateReadWriteBinding(session))
            {
                return _operationExecutor.ExecuteWriteOperation(binding, operation, cancellationToken);
            }
        }

        private async Task<T> ExecuteWriteOperationAsync<T>(IClientSessionHandle session, IWriteOperation<T> operation, CancellationToken cancellationToken)
        {
            using (var binding = CreateReadWriteBinding(session))
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

        private void UsingImplicitSession(Action<IClientSessionHandle> func, CancellationToken cancellationToken)
        {
            using (var session = _operationExecutor.StartImplicitSession(cancellationToken))
            {
                func(session);
            }
        }

        private TResult UsingImplicitSession<TResult>(Func<IClientSessionHandle, TResult> func, CancellationToken cancellationToken)
        {
            using (var session = _operationExecutor.StartImplicitSession(cancellationToken))
            {
                return func(session);
            }
        }

        private async Task UsingImplicitSessionAsync(Func<IClientSessionHandle, Task> funcAsync, CancellationToken cancellationToken)
        {
            using (var session = await _operationExecutor.StartImplicitSessionAsync(cancellationToken).ConfigureAwait(false))
            {
                await funcAsync(session).ConfigureAwait(false);
            }
        }

        private async Task<TResult> UsingImplicitSessionAsync<TResult>(Func<IClientSessionHandle, Task<TResult>> funcAsync, CancellationToken cancellationToken)
        {
            using (var session = await _operationExecutor.StartImplicitSessionAsync(cancellationToken).ConfigureAwait(false))
            {
                return await funcAsync(session).ConfigureAwait(false);
            }
        }
    }
}

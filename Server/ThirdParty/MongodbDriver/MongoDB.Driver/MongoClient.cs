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

using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Driver.Core.Bindings;
using MongoDB.Driver.Core.Clusters;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.Operations;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders;

namespace MongoDB.Driver
{
    /// <inheritdoc/>
    public class MongoClient : MongoClientBase
    {
        // private fields
        private readonly ICluster _cluster;
        private readonly IOperationExecutor _operationExecutor;
        private readonly MongoClientSettings _settings;

        // constructors
        /// <summary>
        /// Initializes a new instance of the MongoClient class.
        /// </summary>
        public MongoClient()
            : this(new MongoClientSettings())
        {
        }

        /// <summary>
        /// Initializes a new instance of the MongoClient class.
        /// </summary>
        /// <param name="settings">The settings.</param>
        public MongoClient(MongoClientSettings settings)
        {
            _settings = Ensure.IsNotNull(settings, nameof(settings)).FrozenCopy();
            _cluster = ClusterRegistry.Instance.GetOrCreateCluster(_settings.ToClusterKey());
            _operationExecutor = new OperationExecutor();
        }

        /// <summary>
        /// Initializes a new instance of the MongoClient class.
        /// </summary>
        /// <param name="url">The URL.</param>
        public MongoClient(MongoUrl url)
            : this(MongoClientSettings.FromUrl(url))
        {
        }

        /// <summary>
        /// Initializes a new instance of the MongoClient class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        public MongoClient(string connectionString)
            : this(ParseConnectionString(connectionString))
        {
        }

        internal MongoClient(IOperationExecutor operationExecutor, MongoClientSettings settings)
            : this(settings)
        {
            _operationExecutor = operationExecutor;
        }

        // public properties
        /// <summary>
        /// Gets the cluster.
        /// </summary>
        public override ICluster Cluster
        {
            get { return _cluster; }
        }

        /// <inheritdoc/>
        public sealed override MongoClientSettings Settings
        {
            get { return _settings; }
        }

        // private static methods
        private static MongoClientSettings ParseConnectionString(string connectionString)
        {
            var url = new MongoUrl(connectionString);
            return MongoClientSettings.FromUrl(url);
        }

        // public methods
        /// <inheritdoc/>
        public sealed override void DropDatabase(string name, CancellationToken cancellationToken = default(CancellationToken))
        {
            var messageEncoderSettings = GetMessageEncoderSettings();
            var operation = new DropDatabaseOperation(new DatabaseNamespace(name), messageEncoderSettings)
            {
                WriteConcern = _settings.WriteConcern
            };

            using (var binding = new WritableServerBinding(_cluster))
            {
                _operationExecutor.ExecuteWriteOperation(binding, operation, cancellationToken);
            }
        }

        /// <inheritdoc/>
        public sealed override async Task DropDatabaseAsync(string name, CancellationToken cancellationToken = default(CancellationToken))
        {
            var messageEncoderSettings = GetMessageEncoderSettings();
            var operation = new DropDatabaseOperation(new DatabaseNamespace(name), messageEncoderSettings)
            {
                WriteConcern = _settings.WriteConcern
            };

            using (var binding = new WritableServerBinding(_cluster))
            {
                await _operationExecutor.ExecuteWriteOperationAsync(binding, operation, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <inheritdoc/>
        public sealed override IMongoDatabase GetDatabase(string name, MongoDatabaseSettings settings = null)
        {
            settings = settings == null ?
                new MongoDatabaseSettings() :
                settings.Clone();

            settings.ApplyDefaultValues(_settings);

            return new MongoDatabaseImpl(this, new DatabaseNamespace(name), settings, _cluster, _operationExecutor);
        }

        /// <inheritdoc/>
        public sealed override IAsyncCursor<BsonDocument> ListDatabases(CancellationToken cancellationToken = default(CancellationToken))
        {
            var messageEncoderSettings = GetMessageEncoderSettings();
            var operation = new ListDatabasesOperation(messageEncoderSettings);

            using (var binding = new ReadPreferenceBinding(_cluster, _settings.ReadPreference))
            {
                return _operationExecutor.ExecuteReadOperation(binding, operation, cancellationToken);
            }
        }

        /// <inheritdoc/>
        public sealed override async Task<IAsyncCursor<BsonDocument>> ListDatabasesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var messageEncoderSettings = GetMessageEncoderSettings();
            var operation = new ListDatabasesOperation(messageEncoderSettings);

            using (var binding = new ReadPreferenceBinding(_cluster, _settings.ReadPreference))
            {
                return await _operationExecutor.ExecuteReadOperationAsync(binding, operation, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <inheritdoc/>
        public override IMongoClient WithReadConcern(ReadConcern readConcern)
        {
            Ensure.IsNotNull(readConcern, nameof(readConcern));
            var newSettings = Settings.Clone();
            newSettings.ReadConcern = readConcern;
            return new MongoClient(_operationExecutor, newSettings);
        }

        /// <inheritdoc/>
        public override IMongoClient WithReadPreference(ReadPreference readPreference)
        {
            Ensure.IsNotNull(readPreference, nameof(readPreference));
            var newSettings = Settings.Clone();
            newSettings.ReadPreference = readPreference;
            return new MongoClient(_operationExecutor, newSettings);
        }

        /// <inheritdoc/>
        public override IMongoClient WithWriteConcern(WriteConcern writeConcern)
        {
            Ensure.IsNotNull(writeConcern, nameof(writeConcern));
            var newSettings = Settings.Clone();
            newSettings.WriteConcern = writeConcern;
            return new MongoClient(_operationExecutor, newSettings);
        }

        // private methods
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

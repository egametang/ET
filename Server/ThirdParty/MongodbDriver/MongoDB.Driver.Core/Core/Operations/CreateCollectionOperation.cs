/* Copyright 2013-2016 MongoDB Inc.
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
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Core.Bindings;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.Servers;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders;

namespace MongoDB.Driver.Core.Operations
{
    /// <summary>
    /// Represents a create collection operation.
    /// </summary>
    public class CreateCollectionOperation : IWriteOperation<BsonDocument>
    {
        // fields
        private bool? _autoIndexId;
        private bool? _capped;
        private Collation _collation;
        private readonly CollectionNamespace _collectionNamespace;
        private BsonDocument _indexOptionDefaults;
        private long? _maxDocuments;
        private long? _maxSize;
        private readonly MessageEncoderSettings _messageEncoderSettings;
        private bool? _noPadding;
        private BsonDocument _storageEngine;
        private bool? _usePowerOf2Sizes;
        private DocumentValidationAction? _validationAction;
        private DocumentValidationLevel? _validationLevel;
        private BsonDocument _validator;
        private WriteConcern _writeConcern;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="CreateCollectionOperation"/> class.
        /// </summary>
        /// <param name="collectionNamespace">The collection namespace.</param>
        /// <param name="messageEncoderSettings">The message encoder settings.</param>
        public CreateCollectionOperation(
            CollectionNamespace collectionNamespace,
            MessageEncoderSettings messageEncoderSettings)
        {
            _collectionNamespace = Ensure.IsNotNull(collectionNamespace, nameof(collectionNamespace));
            _messageEncoderSettings = messageEncoderSettings;
        }

        // properties
        /// <summary>
        /// Gets or sets a value indicating whether an index on _id should be created automatically.
        /// </summary>
        /// <value>
        /// A value indicating whether an index on _id should be created automatically.
        /// </value>
        public bool? AutoIndexId
        {
            get { return _autoIndexId; }
            set { _autoIndexId = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the collection is a capped collection.
        /// </summary>
        /// <value>
        /// A value indicating whether the collection is a capped collection.
        /// </value>
        public bool? Capped
        {
            get { return _capped; }
            set { _capped = value; }
        }

        /// <summary>
        /// Gets or sets the collation.
        /// </summary>
        /// <value>
        /// The collation.
        /// </value>
        public Collation Collation
        {
            get { return _collation; }
            set { _collation = value; }
        }

        /// <summary>
        /// Gets the collection namespace.
        /// </summary>
        /// <value>
        /// The collection namespace.
        /// </value>
        public CollectionNamespace CollectionNamespace
        {
            get { return _collectionNamespace; }
        }

        /// <summary>
        /// Gets or sets the index option defaults.
        /// </summary>
        /// <value>
        /// The index option defaults.
        /// </value>
        public BsonDocument IndexOptionDefaults
        {
            get { return _indexOptionDefaults; }
            set { _indexOptionDefaults = value; }
        }

        /// <summary>
        /// Gets or sets the maximum number of documents in a capped collection.
        /// </summary>
        /// <value>
        /// The maximum number of documents in a capped collection.
        /// </value>
        public long? MaxDocuments
        {
            get { return _maxDocuments; }
            set { _maxDocuments = Ensure.IsNullOrGreaterThanZero(value, nameof(value)); }
        }

        /// <summary>
        /// Gets or sets the maximum size of a capped collection.
        /// </summary>
        /// <value>
        /// The maximum size of a capped collection.
        /// </value>
        public long? MaxSize
        {
            get { return _maxSize; }
            set { _maxSize = Ensure.IsNullOrGreaterThanZero(value, nameof(value)); }
        }

        /// <summary>
        /// Gets the message encoder settings.
        /// </summary>
        /// <value>
        /// The message encoder settings.
        /// </value>
        public MessageEncoderSettings MessageEncoderSettings
        {
            get { return _messageEncoderSettings; }
        }

        /// <summary>
        /// Gets or sets whether padding should not be used.
        /// </summary>
        public bool? NoPadding
        {
            get { return _noPadding; }
            set { _noPadding = value; }
        }

        /// <summary>
        /// Gets or sets the storage engine options.
        /// </summary>
        /// <value>
        /// The storage engine options.
        /// </value>
        public BsonDocument StorageEngine
        {
            get { return _storageEngine; }
            set { _storageEngine = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the collection should use power of 2 sizes.
        /// </summary>
        /// <value>
        /// A value indicating whether the collection should use power of 2 sizes..
        /// </value>
        public bool? UsePowerOf2Sizes
        {
            get { return _usePowerOf2Sizes; }
            set { _usePowerOf2Sizes = value; }
        }

        /// <summary>
        /// Gets or sets the validation action.
        /// </summary>
        /// <value>
        /// The validation action.
        /// </value>
        public DocumentValidationAction? ValidationAction
        {
            get { return _validationAction; }
            set { _validationAction = value; }
        }

        /// <summary>
        /// Gets or sets the validation level.
        /// </summary>
        /// <value>
        /// The validation level.
        /// </value>
        public DocumentValidationLevel? ValidationLevel
        {
            get { return _validationLevel; }
            set { _validationLevel = value; }
        }

        /// <summary>
        /// Gets or sets the validator.
        /// </summary>
        /// <value>
        /// The validator.
        /// </value>
        public BsonDocument Validator
        {
            get { return _validator; }
            set { _validator = value; }
        }

        /// <summary>
        /// Gets or sets the write concern.
        /// </summary>
        /// <value>
        /// The write concern.
        /// </value>
        public WriteConcern WriteConcern
        {
            get { return _writeConcern; }
            set { _writeConcern = value; }
        }

        // methods
        internal BsonDocument CreateCommand(SemanticVersion serverVersion)
        {
            Feature.Collation.ThrowIfNotSupported(serverVersion, _collation);

            var flags = GetFlags();
            return new BsonDocument
            {
                { "create", _collectionNamespace.CollectionName },
                { "capped", () => _capped.Value, _capped.HasValue },
                { "autoIndexId", () => _autoIndexId.Value, _autoIndexId.HasValue },
                { "size", () => _maxSize.Value, _maxSize.HasValue },
                { "max", () => _maxDocuments.Value, _maxDocuments.HasValue },
                { "flags", () => (int)flags.Value, flags.HasValue },
                { "storageEngine", () => _storageEngine, _storageEngine != null },
                { "indexOptionDefaults", _indexOptionDefaults, _indexOptionDefaults != null },
                { "validator", _validator, _validator != null },
                { "validationAction", () => _validationAction.Value.ToString().ToLowerInvariant(), _validationAction.HasValue },
                { "validationLevel", () => _validationLevel.Value.ToString().ToLowerInvariant(), _validationLevel.HasValue },
                { "collation", () => _collation.ToBsonDocument(), _collation != null },
                { "writeConcern", () => _writeConcern.ToBsonDocument(), Feature.CommandsThatWriteAcceptWriteConcern.ShouldSendWriteConcern(serverVersion, _writeConcern) }
            };
        }

        private CreateCollectionFlags? GetFlags()
        {
            if (_usePowerOf2Sizes.HasValue || _noPadding.HasValue)
            {
                var flags = CreateCollectionFlags.None;
                if (_usePowerOf2Sizes.HasValue && _usePowerOf2Sizes.Value)
                {
                    flags |= CreateCollectionFlags.UsePowerOf2Sizes;
                }
                if (_noPadding.HasValue && _noPadding.Value)
                {
                    flags |= CreateCollectionFlags.NoPadding;
                }
                return flags;
            }
            else
            {
                return null;
            }
        }

        /// <inheritdoc/>
        public BsonDocument Execute(IWriteBinding binding, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(binding, nameof(binding));

            using (var channelSource = binding.GetWriteChannelSource(cancellationToken))
            using (var channel = channelSource.GetChannel(cancellationToken))
            using (var channelBinding = new ChannelReadWriteBinding(channelSource.Server, channel))
            {
                var operation = CreateOperation(channel.ConnectionDescription.ServerVersion);
                var result = operation.Execute(channelBinding, cancellationToken);
                WriteConcernErrorHelper.ThrowIfHasWriteConcernError(channel.ConnectionDescription.ConnectionId, result);
                return result;
            }
        }

        /// <inheritdoc/>
        public async Task<BsonDocument> ExecuteAsync(IWriteBinding binding, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(binding, nameof(binding));

            using (var channelSource = await binding.GetWriteChannelSourceAsync(cancellationToken).ConfigureAwait(false))
            using (var channel = await channelSource.GetChannelAsync(cancellationToken).ConfigureAwait(false))
            using (var channelBinding = new ChannelReadWriteBinding(channelSource.Server, channel))
            {
                var operation = CreateOperation(channel.ConnectionDescription.ServerVersion);
                var result = await operation.ExecuteAsync(channelBinding, cancellationToken).ConfigureAwait(false);
                WriteConcernErrorHelper.ThrowIfHasWriteConcernError(channel.ConnectionDescription.ConnectionId, result);
                return result;
            }
        }

        private WriteCommandOperation<BsonDocument> CreateOperation(SemanticVersion serverVersion)
        {
            var command = CreateCommand(serverVersion);
            return new WriteCommandOperation<BsonDocument>(_collectionNamespace.DatabaseNamespace, command, BsonDocumentSerializer.Instance, _messageEncoderSettings);
        }

        [Flags]
        private enum CreateCollectionFlags
        {
            None = 0,
            UsePowerOf2Sizes = 1,
            NoPadding = 2
        }
    }
}

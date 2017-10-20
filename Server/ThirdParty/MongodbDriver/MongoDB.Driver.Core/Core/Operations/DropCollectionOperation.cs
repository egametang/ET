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
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders;

namespace MongoDB.Driver.Core.Operations
{
    /// <summary>
    /// Represents a drop collection operation.
    /// </summary>
    public class DropCollectionOperation : IWriteOperation<BsonDocument>
    {
        // fields
        private readonly CollectionNamespace _collectionNamespace;
        private readonly MessageEncoderSettings _messageEncoderSettings;
        private WriteConcern _writeConcern;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="DropCollectionOperation"/> class.
        /// </summary>
        /// <param name="collectionNamespace">The collection namespace.</param>
        /// <param name="messageEncoderSettings">The message encoder settings.</param>
        public DropCollectionOperation(
            CollectionNamespace collectionNamespace,
            MessageEncoderSettings messageEncoderSettings)
        {
            _collectionNamespace = Ensure.IsNotNull(collectionNamespace, nameof(collectionNamespace));
            _messageEncoderSettings = messageEncoderSettings;
        }

        // properties
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

        // public methods
        /// <inheritdoc/>
        public BsonDocument Execute(IWriteBinding binding, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(binding, nameof(binding));

            using (var channelSource = binding.GetWriteChannelSource(cancellationToken))
            using (var channel = channelSource.GetChannel(cancellationToken))
            using (var channelBinding = new ChannelReadWriteBinding(channelSource.Server, channel))
            {
                var operation = CreateOperation(channel.ConnectionDescription.ServerVersion);
                BsonDocument result;
                try
                {
                     result = operation.Execute(binding, cancellationToken);
                }
                catch (MongoCommandException ex)
                {
                    if (!ShouldIgnoreException(ex))
                    {
                        throw;
                    }
                    result = ex.Result;
                }
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
                BsonDocument result;
                try
                {
                    result = await operation.ExecuteAsync(binding, cancellationToken).ConfigureAwait(false);
                }
                catch (MongoCommandException ex)
                {
                    if (!ShouldIgnoreException(ex))
                    {
                        throw;
                    }
                    result = ex.Result;
                }
                WriteConcernErrorHelper.ThrowIfHasWriteConcernError(channel.ConnectionDescription.ConnectionId, result);
                return result;
            }
        }

        // private methods
        internal BsonDocument CreateCommand(SemanticVersion serverVersion)
        {
            return new BsonDocument
            {
                { "drop", _collectionNamespace.CollectionName },
                { "writeConcern", () => _writeConcern.ToBsonDocument(), Feature.CommandsThatWriteAcceptWriteConcern.ShouldSendWriteConcern(serverVersion, _writeConcern) }
            };
        }

        private WriteCommandOperation<BsonDocument> CreateOperation(SemanticVersion serverVersion)
        {
            var command = CreateCommand(serverVersion);
            return new WriteCommandOperation<BsonDocument>(_collectionNamespace.DatabaseNamespace, command, BsonDocumentSerializer.Instance, _messageEncoderSettings);
        }

        private bool ShouldIgnoreException(MongoCommandException ex)
        {
            return ex.ErrorMessage == "ns not found";
        }
    }
}

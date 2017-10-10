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
using System.Collections.Generic;
using System.Linq;
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
    /// Represents an aggregate explain operations.
    /// </summary>
    public class AggregateExplainOperation : IReadOperation<BsonDocument>
    {
        // fields
        private bool? _allowDiskUse;
        private Collation _collation;
        private CollectionNamespace _collectionNamespace;
        private TimeSpan? _maxTime;
        private MessageEncoderSettings _messageEncoderSettings;
        private IReadOnlyList<BsonDocument> _pipeline;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateExplainOperation"/> class.
        /// </summary>
        /// <param name="collectionNamespace">The collection namespace.</param>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="messageEncoderSettings">The message encoder settings.</param>
        public AggregateExplainOperation(CollectionNamespace collectionNamespace, IEnumerable<BsonDocument> pipeline, MessageEncoderSettings messageEncoderSettings)
        {
            _collectionNamespace = Ensure.IsNotNull(collectionNamespace, nameof(collectionNamespace));
            _pipeline = Ensure.IsNotNull(pipeline, nameof(pipeline)).ToList();
            _messageEncoderSettings = Ensure.IsNotNull(messageEncoderSettings, nameof(messageEncoderSettings));
        }

        // properties
        /// <summary>
        /// Gets or sets a value indicating whether the server is allowed to use the disk.
        /// </summary>
        /// <value>
        /// A value indicating whether the server is allowed to use the disk.
        /// </value>
        public bool? AllowDiskUse
        {
            get { return _allowDiskUse; }
            set { _allowDiskUse = value; }
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
        /// Gets or sets the maximum time the server should spend on this operation.
        /// </summary>
        /// <value>
        /// The maximum time the server should spend on this operation.
        /// </value>
        public TimeSpan? MaxTime
        {
            get { return _maxTime; }
            set { _maxTime = value; }
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
        /// Gets the pipeline.
        /// </summary>
        /// <value>
        /// The pipeline.
        /// </value>
        public IReadOnlyList<BsonDocument> Pipeline
        {
            get { return _pipeline; }
        }

        // methods
        internal BsonDocument CreateCommand(SemanticVersion serverVersion)
        {
            Feature.Collation.ThrowIfNotSupported(serverVersion, _collation);

            return new BsonDocument
            {
                { "aggregate", _collectionNamespace.CollectionName },
                { "explain", true },
                { "pipeline", new BsonArray(_pipeline) },
                { "allowDiskUse", () => _allowDiskUse.Value, _allowDiskUse.HasValue },
                { "maxTimeMS", () => _maxTime.Value.TotalMilliseconds, _maxTime.HasValue },
                { "collation", () => _collation.ToBsonDocument(), _collation != null }
            };
        }

        /// <inheritdoc/>
        public BsonDocument Execute(IReadBinding binding, CancellationToken cancellationToken)
        {
            using (var channelSource = binding.GetReadChannelSource(cancellationToken))
            using (var channel = channelSource.GetChannel(cancellationToken))
            using (var channelBinding = new ChannelReadBinding(channelSource.Server, channel, binding.ReadPreference))
            {
                var operation = CreateOperation(channel.ConnectionDescription.ServerVersion);
                return operation.Execute(channelBinding, cancellationToken);
            }
        }

        /// <inheritdoc/>
        public async Task<BsonDocument> ExecuteAsync(IReadBinding binding, CancellationToken cancellationToken)
        {
            using (var channelSource = await binding.GetReadChannelSourceAsync(cancellationToken).ConfigureAwait(false))
            using (var channel = await channelSource.GetChannelAsync(cancellationToken).ConfigureAwait(false))
            using (var channelBinding = new ChannelReadBinding(channelSource.Server, channel, binding.ReadPreference))
            {
                var operation = CreateOperation(channel.ConnectionDescription.ServerVersion);
                return await operation.ExecuteAsync(channelBinding, cancellationToken).ConfigureAwait(false);
            }
        }

        private ReadCommandOperation<BsonDocument> CreateOperation(SemanticVersion serverVersion)
        {
            var command = CreateCommand(serverVersion);
            return new ReadCommandOperation<BsonDocument>(
                _collectionNamespace.DatabaseNamespace,
                command,
                BsonDocumentSerializer.Instance,
                _messageEncoderSettings);
        }
    }
}

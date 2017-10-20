/* Copyright 2016 MongoDB Inc.
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
    /// Represents a create view operation.
    /// </summary>
    public class CreateViewOperation : IWriteOperation<BsonDocument>
    {
        // private fields
        private Collation _collation;
        private readonly DatabaseNamespace _databaseNamespace;
        private readonly MessageEncoderSettings _messageEncoderSettings;
        private readonly IReadOnlyList<BsonDocument> _pipeline;
        private readonly string _viewName;
        private readonly string _viewOn;
        private WriteConcern _writeConcern;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="CreateCollectionOperation" /> class.
        /// </summary>
        /// <param name="databaseNamespace">The name of the database.</param>
        /// <param name="viewName">The name of the view.</param>
        /// <param name="viewOn">The name of the collection that the view is on.</param>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="messageEncoderSettings">The message encoder settings.</param>
        public CreateViewOperation(
            DatabaseNamespace databaseNamespace,
            string viewName,
            string viewOn,
            IEnumerable<BsonDocument> pipeline,
            MessageEncoderSettings messageEncoderSettings)
        {
            _databaseNamespace = Ensure.IsNotNull(databaseNamespace, nameof(databaseNamespace));
            _viewName = Ensure.IsNotNull(viewName, nameof(viewName));
            _viewOn = Ensure.IsNotNull(viewOn, nameof(viewOn));
            _pipeline = Ensure.IsNotNull(pipeline, nameof(pipeline)).ToList();
            _messageEncoderSettings = Ensure.IsNotNull(messageEncoderSettings, nameof(messageEncoderSettings));
        }

        // public properties
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
        /// Gets the namespace of the database.
        /// </summary>
        /// <value>
        /// The namespace of the database.
        /// </value>
        public DatabaseNamespace DatabaseNamespace
        {
            get { return _databaseNamespace; }
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

        /// <summary>
        /// Gets the name of the view.
        /// </summary>
        /// <value>
        /// The name of the view.
        /// </value>
        public string ViewName
        {
            get { return _viewName; }
        }

        /// <summary>
        /// Gets the name of the collection that the view is on.
        /// </summary>
        /// <value>
        /// The name of the collection that the view is on.
        /// </value>
        public string ViewOn
        {
            get { return _viewOn; }
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

        // private methods
        internal BsonDocument CreateCommand(SemanticVersion serverVersion)
        {
            Feature.Views.ThrowIfNotSupported(serverVersion);
            Feature.Collation.ThrowIfNotSupported(serverVersion, _collation);

            return new BsonDocument
            {
                { "create", _viewName },
                { "viewOn", _viewOn },
                { "pipeline", new BsonArray(_pipeline) },
                { "collation", () => _collation.ToBsonDocument(), _collation != null },
                { "writeConcern", () => _writeConcern.ToBsonDocument(), Feature.CommandsThatWriteAcceptWriteConcern.ShouldSendWriteConcern(serverVersion, _writeConcern) }
            };
        }

        private WriteCommandOperation<BsonDocument> CreateOperation(SemanticVersion serverVersion)
        {
            var command = CreateCommand(serverVersion);
            return new WriteCommandOperation<BsonDocument>(_databaseNamespace, command, BsonDocumentSerializer.Instance, _messageEncoderSettings);
        }
    }
}

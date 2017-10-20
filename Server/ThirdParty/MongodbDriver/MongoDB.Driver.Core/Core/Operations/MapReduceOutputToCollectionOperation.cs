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
    /// Represents a map-reduce operation that outputs its results to a collection.
    /// </summary>
    public class MapReduceOutputToCollectionOperation : MapReduceOperationBase, IWriteOperation<BsonDocument>
    {
        // fields
        private bool? _bypassDocumentValidation;
        private bool? _nonAtomicOutput;
        private readonly CollectionNamespace _outputCollectionNamespace;
        private MapReduceOutputMode _outputMode;
        private bool? _shardedOutput;
        private WriteConcern _writeConcern;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="MapReduceOutputToCollectionOperation"/> class.
        /// </summary>
        /// <param name="collectionNamespace">The collection namespace.</param>
        /// <param name="outputCollectionNamespace">The output collection namespace.</param>
        /// <param name="mapFunction">The map function.</param>
        /// <param name="reduceFunction">The reduce function.</param>
        /// <param name="messageEncoderSettings">The message encoder settings.</param>
        public MapReduceOutputToCollectionOperation(
            CollectionNamespace collectionNamespace,
            CollectionNamespace outputCollectionNamespace,
            BsonJavaScript mapFunction,
            BsonJavaScript reduceFunction,
            MessageEncoderSettings messageEncoderSettings)
            : base(
                collectionNamespace,
                mapFunction,
                reduceFunction,
                messageEncoderSettings)
        {
            _outputCollectionNamespace = Ensure.IsNotNull(outputCollectionNamespace, nameof(outputCollectionNamespace));
            _outputMode = MapReduceOutputMode.Replace;
        }

        // properties
        /// <summary>
        /// Gets or sets a value indicating whether to bypass document validation.
        /// </summary>
        /// <value>
        /// A value indicating whether to bypass document validation.
        /// </value>
        public bool? BypassDocumentValidation
        {
            get { return _bypassDocumentValidation; }
            set { _bypassDocumentValidation = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the server should not lock the database for merge and reduce output modes.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the server should not lock the database for merge and reduce output modes; otherwise, <c>false</c>.
        /// </value>
        public bool? NonAtomicOutput
        {
            get { return _nonAtomicOutput; }
            set { _nonAtomicOutput = value; }
        }

        /// <summary>
        /// Gets the output collection namespace.
        /// </summary>
        /// <value>
        /// The output collection namespace.
        /// </value>
        public CollectionNamespace OutputCollectionNamespace
        {
            get { return _outputCollectionNamespace; }
        }

        /// <summary>
        /// Gets or sets the output mode.
        /// </summary>
        /// <value>
        /// The output mode.
        /// </value>
        public MapReduceOutputMode OutputMode
        {
            get { return _outputMode; }
            set { _outputMode = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the output collection should be sharded.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the output collection should be sharded; otherwise, <c>false</c>.
        /// </value>
        public bool? ShardedOutput
        {
            get { return _shardedOutput; }
            set { _shardedOutput = value; }
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
        /// <inheritdoc/>
        protected internal override BsonDocument CreateCommand(SemanticVersion serverVersion)
        {
            var command = base.CreateCommand(serverVersion);
            if (_bypassDocumentValidation.HasValue && Feature.BypassDocumentValidation.IsSupported(serverVersion))
            {
                command["bypassDocumentValidation"] = _bypassDocumentValidation.Value;
            }
            if (Feature.CommandsThatWriteAcceptWriteConcern.ShouldSendWriteConcern(serverVersion, _writeConcern))
            {
                command["writeConcern"] = _writeConcern.ToBsonDocument();
            }
            return command;
        }

        /// <inheritdoc/>
        protected override BsonDocument CreateOutputOptions()
        {
            var action = _outputMode.ToString().ToLowerInvariant();
            return new BsonDocument
            {
                { action, _outputCollectionNamespace.CollectionName },
                { "db", _outputCollectionNamespace.DatabaseNamespace.DatabaseName },
                { "sharded", () => _shardedOutput.Value, _shardedOutput.HasValue },
                { "nonAtomic", () => _nonAtomicOutput.Value, _nonAtomicOutput.HasValue }
            };
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
            return new WriteCommandOperation<BsonDocument>(CollectionNamespace.DatabaseNamespace, command, BsonDocumentSerializer.Instance, MessageEncoderSettings);
        }
    }
}

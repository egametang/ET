/* Copyright 2013-2015 MongoDB Inc.
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
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Core.Bindings;
using MongoDB.Driver.Core.Events;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders;

namespace MongoDB.Driver.Core.Operations
{
    /// <summary>
    /// Represents a parallel scan operation.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document.</typeparam>
    public class ParallelScanOperation<TDocument> : IReadOperation<IReadOnlyList<IAsyncCursor<TDocument>>>
    {
        // fields
        private int? _batchSize;
        private CollectionNamespace _collectionNamespace;
        private MessageEncoderSettings _messageEncoderSettings;
        private int _numberOfCursors = 4;
        private ReadConcern _readConcern = ReadConcern.Default;
        private IBsonSerializer<TDocument> _serializer;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ParallelScanOperation{TDocument}"/> class.
        /// </summary>
        /// <param name="collectionNamespace">The collection namespace.</param>
        /// <param name="numberOfCursors">The number of cursors.</param>
        /// <param name="serializer">The serializer.</param>
        /// <param name="messageEncoderSettings">The message encoder settings.</param>
        public ParallelScanOperation(
            CollectionNamespace collectionNamespace,
            int numberOfCursors,
            IBsonSerializer<TDocument> serializer,
            MessageEncoderSettings messageEncoderSettings)
        {
            _collectionNamespace = Ensure.IsNotNull(collectionNamespace, nameof(collectionNamespace));
            _numberOfCursors = Ensure.IsBetween(numberOfCursors, 0, 10000, nameof(numberOfCursors));
            _serializer = Ensure.IsNotNull(serializer, nameof(serializer));
            _messageEncoderSettings = messageEncoderSettings;
        }

        // properties
        /// <summary>
        /// Gets or sets the size of a batch.
        /// </summary>
        /// <value>
        /// The size of a batch.
        /// </value>
        public int? BatchSize
        {
            get { return _batchSize; }
            set { _batchSize = Ensure.IsNullOrGreaterThanZero(value, nameof(value)); }
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
        /// Gets the number of cursors.
        /// </summary>
        /// <value>
        /// The number of cursors.
        /// </value>
        public int NumberOfCursors
        {
            get { return _numberOfCursors; }
        }

        /// <summary>
        /// Gets or sets the read concern.
        /// </summary>
        /// <value>
        /// The read concern.
        /// </value>
        public ReadConcern ReadConcern
        {
            get { return _readConcern; }
            set { _readConcern = Ensure.IsNotNull(value, nameof(value)); }
        }

        /// <summary>
        /// Gets the serializer.
        /// </summary>
        /// <value>
        /// The serializer.
        /// </value>
        public IBsonSerializer<TDocument> Serializer
        {
            get { return _serializer; }
        }

        // methods
        internal BsonDocument CreateCommand(SemanticVersion serverVersion)
        {
            Feature.ReadConcern.ThrowIfNotSupported(serverVersion, _readConcern);

            return new BsonDocument
            {
                { "parallelCollectionScan", _collectionNamespace.CollectionName },
                { "numCursors", _numberOfCursors },
                { "readConcern", _readConcern.ToBsonDocument(), !_readConcern.IsServerDefault }
            };
        }

        private ReadCommandOperation<BsonDocument> CreateOperation(SemanticVersion serverVersion)
        {
            var command = CreateCommand(serverVersion);
            return new ReadCommandOperation<BsonDocument>(_collectionNamespace.DatabaseNamespace, command, BsonDocumentSerializer.Instance, _messageEncoderSettings);
        }

        private IReadOnlyList<IAsyncCursor<TDocument>> CreateCursors(IChannelSourceHandle channelSource, BsonDocument command, BsonDocument result)
        {
            var cursors = new List<AsyncCursor<TDocument>>();

            using (var getMoreChannelSource = new ChannelSourceHandle(new ServerChannelSource(channelSource.Server)))
            {
                foreach (var cursorDocument in result["cursors"].AsBsonArray.Select(v => v["cursor"].AsBsonDocument))
                {
                    var cursorId = cursorDocument["id"].ToInt64();
                    var firstBatch = cursorDocument["firstBatch"].AsBsonArray.Select(v =>
                    {
                        var bsonDocument = (BsonDocument)v;
                        using (var reader = new BsonDocumentReader(bsonDocument))
                        {
                            var context = BsonDeserializationContext.CreateRoot(reader);
                            var document = _serializer.Deserialize(context);
                            return document;
                        }
                    })
                        .ToList();

                    var cursor = new AsyncCursor<TDocument>(
                        getMoreChannelSource.Fork(),
                        _collectionNamespace,
                        command,
                        firstBatch,
                        cursorId,
                        _batchSize ?? 0,
                        0, // limit
                        _serializer,
                        _messageEncoderSettings);

                    cursors.Add(cursor);
                }
            }

            return cursors;
        }

        /// <inheritdoc/>
        public IReadOnlyList<IAsyncCursor<TDocument>> Execute(IReadBinding binding, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(binding, nameof(binding));

            using (EventContext.BeginOperation())
            using (var channelSource = binding.GetReadChannelSource(cancellationToken))
            using (var channel = channelSource.GetChannel(cancellationToken))
            using (var channelBinding = new ChannelReadBinding(channelSource.Server, channel, binding.ReadPreference))
            {
                var operation = CreateOperation(channel.ConnectionDescription.ServerVersion);
                var result = operation.Execute(channelBinding, cancellationToken);
                return CreateCursors(channelSource, operation.Command, result);
            }
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<IAsyncCursor<TDocument>>> ExecuteAsync(IReadBinding binding, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(binding, nameof(binding));

            using (EventContext.BeginOperation())
            using (var channelSource = await binding.GetReadChannelSourceAsync(cancellationToken).ConfigureAwait(false))
            using (var channel = await channelSource.GetChannelAsync(cancellationToken).ConfigureAwait(false))
            using (var channelBinding = new ChannelReadBinding(channelSource.Server, channel, binding.ReadPreference))
            {
                var operation = CreateOperation(channel.ConnectionDescription.ServerVersion);
                var result = await operation.ExecuteAsync(channelBinding, cancellationToken).ConfigureAwait(false);
                return CreateCursors(channelSource, operation.Command, result);
            }
        }
    }
}

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
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Core.Bindings;
using MongoDB.Driver.Core.Events;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders;

namespace MongoDB.Driver.Core.Operations
{
    /// <summary>
    /// Represents a list indexes operation.
    /// </summary>
    public class ListIndexesUsingCommandOperation : IReadOperation<IAsyncCursor<BsonDocument>>
    {
        // fields
        private readonly CollectionNamespace _collectionNamespace;
        private readonly MessageEncoderSettings _messageEncoderSettings;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ListIndexesUsingCommandOperation"/> class.
        /// </summary>
        /// <param name="collectionNamespace">The collection namespace.</param>
        /// <param name="messageEncoderSettings">The message encoder settings.</param>
        public ListIndexesUsingCommandOperation(
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

        // public methods
        /// <inheritdoc/>
        public IAsyncCursor<BsonDocument> Execute(IReadBinding binding, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(binding, nameof(binding));

            using (EventContext.BeginOperation())
            using (var channelSource = binding.GetReadChannelSource(cancellationToken))
            {
                var operation = CreateOperation();
                try
                {
                    var result = operation.Execute(channelSource, binding.ReadPreference, cancellationToken);
                    return CreateCursor(channelSource, result, operation.Command);
                }
                catch (MongoCommandException ex)
                {
                    if (IsCollectionNotFoundException(ex))
                    {
                        return new SingleBatchAsyncCursor<BsonDocument>(new List<BsonDocument>());
                    }
                    throw;
                }
            }
        }

        /// <inheritdoc/>
        public async Task<IAsyncCursor<BsonDocument>> ExecuteAsync(IReadBinding binding, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(binding, nameof(binding));

            using (EventContext.BeginOperation())
            using (var channelSource = await binding.GetReadChannelSourceAsync(cancellationToken).ConfigureAwait(false))
            {
                var operation = CreateOperation();
                try
                {
                    var result = await operation.ExecuteAsync(channelSource, binding.ReadPreference, cancellationToken).ConfigureAwait(false);
                    return CreateCursor(channelSource, result, operation.Command);
                }
                catch (MongoCommandException ex)
                {
                    if (IsCollectionNotFoundException(ex))
                    {
                        return new SingleBatchAsyncCursor<BsonDocument>(new List<BsonDocument>());
                    }
                    throw;
                }
            }
        }

        // private methods
        private ReadCommandOperation<BsonDocument> CreateOperation()
        {
            var databaseNamespace = _collectionNamespace.DatabaseNamespace;
            var command = new BsonDocument("listIndexes", _collectionNamespace.CollectionName);
            return new ReadCommandOperation<BsonDocument>(databaseNamespace, command, BsonDocumentSerializer.Instance, _messageEncoderSettings);
        }

        private IAsyncCursor<BsonDocument> CreateCursor(IChannelSourceHandle channelSource, BsonDocument result, BsonDocument command)
        {
            var getMoreChannelSource = new ServerChannelSource(channelSource.Server);
            var cursorDocument = result["cursor"].AsBsonDocument;
            var cursor = new AsyncCursor<BsonDocument>(
                getMoreChannelSource,
                CollectionNamespace.FromFullName(cursorDocument["ns"].AsString),
                command,
                cursorDocument["firstBatch"].AsBsonArray.OfType<BsonDocument>().ToList(),
                cursorDocument["id"].ToInt64(),
                0,
                0,
                BsonDocumentSerializer.Instance,
                _messageEncoderSettings);

            return cursor;
        }

        private bool IsCollectionNotFoundException(MongoCommandException ex)
        {
            return ex.Code == 26;
        }
    }
}

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
    /// Represents a list collections operation.
    /// </summary>
    public class ListCollectionsUsingCommandOperation : IReadOperation<IAsyncCursor<BsonDocument>>
    {
        // fields
        private BsonDocument _filter;
        private readonly DatabaseNamespace _databaseNamespace;
        private readonly MessageEncoderSettings _messageEncoderSettings;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ListCollectionsOperation"/> class.
        /// </summary>
        /// <param name="databaseNamespace">The database namespace.</param>
        /// <param name="messageEncoderSettings">The message encoder settings.</param>
        public ListCollectionsUsingCommandOperation(
            DatabaseNamespace databaseNamespace,
            MessageEncoderSettings messageEncoderSettings)
        {
            _databaseNamespace = Ensure.IsNotNull(databaseNamespace, nameof(databaseNamespace));
            _messageEncoderSettings = Ensure.IsNotNull(messageEncoderSettings, nameof(messageEncoderSettings));
        }

        // properties
        /// <summary>
        /// Gets or sets the filter.
        /// </summary>
        /// <value>
        /// The filter.
        /// </value>
        public BsonDocument Filter
        {
            get { return _filter; }
            set { _filter = value; }
        }

        /// <summary>
        /// Gets the database namespace.
        /// </summary>
        /// <value>
        /// The database namespace.
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

        // public methods
        /// <inheritdoc/>
        public IAsyncCursor<BsonDocument> Execute(IReadBinding binding, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(binding, nameof(binding));

            using (EventContext.BeginOperation())
            using (var channelSource = binding.GetReadChannelSource(cancellationToken))
            {
                var operation = CreateOperation();
                var result = operation.Execute(channelSource, binding.ReadPreference, cancellationToken);
                return CreateCursor(channelSource, operation.Command, result);
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
                var result = await operation.ExecuteAsync(channelSource, binding.ReadPreference, cancellationToken).ConfigureAwait(false);
                return CreateCursor(channelSource, operation.Command, result);
            }
        }

        // private methods
        private ReadCommandOperation<BsonDocument> CreateOperation()
        {
            var command = new BsonDocument
            {
                { "listCollections", 1 },
                { "filter", _filter, _filter != null }
            };
            return new ReadCommandOperation<BsonDocument>(_databaseNamespace, command, BsonDocumentSerializer.Instance, _messageEncoderSettings);
        }

        private IAsyncCursor<BsonDocument> CreateCursor(IChannelSourceHandle channelSource, BsonDocument command, BsonDocument result)
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
    }
}

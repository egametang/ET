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
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders;

namespace MongoDB.Driver.Core.Operations
{
    /// <summary>
    /// Represents the listDatabases command.
    /// </summary>
    public class ListDatabasesOperation : IReadOperation<IAsyncCursor<BsonDocument>>
    {
        // fields
        private MessageEncoderSettings _messageEncoderSettings;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ListDatabasesOperation"/> class.
        /// </summary>
        /// <param name="messageEncoderSettings">The message encoder settings.</param>
        public ListDatabasesOperation(MessageEncoderSettings messageEncoderSettings)
        {
            _messageEncoderSettings = messageEncoderSettings;
        }

        // properties
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
            var operation = CreateOperation();
            var reply = operation.Execute(binding, cancellationToken);
            return CreateCursor(reply);
        }

        /// <inheritdoc/>
        public async Task<IAsyncCursor<BsonDocument>> ExecuteAsync(IReadBinding binding, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(binding, nameof(binding));
            var operation = CreateOperation();
            var reply = await operation.ExecuteAsync(binding, cancellationToken).ConfigureAwait(false);
            return CreateCursor(reply);
        }

        // private methods
        internal BsonDocument CreateCommand()
        {
            return new BsonDocument { { "listDatabases", 1 } };
        }

        private IAsyncCursor<BsonDocument> CreateCursor(BsonDocument reply)
        {
            var databases = reply["databases"].AsBsonArray.OfType<BsonDocument>();
            return new SingleBatchAsyncCursor<BsonDocument>(databases.ToList());
        }

        private ReadCommandOperation<BsonDocument> CreateOperation()
        {
            var command = CreateCommand();
            return new ReadCommandOperation<BsonDocument>(DatabaseNamespace.Admin, command, BsonDocumentSerializer.Instance, _messageEncoderSettings);
        }
    }
}

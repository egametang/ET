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
    /// Represents a list indexes operation.
    /// </summary>
    public class ListIndexesUsingQueryOperation : IReadOperation<IAsyncCursor<BsonDocument>>
    {
        // fields
        private readonly CollectionNamespace _collectionNamespace;
        private readonly MessageEncoderSettings _messageEncoderSettings;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ListIndexesOperation"/> class.
        /// </summary>
        /// <param name="collectionNamespace">The collection namespace.</param>
        /// <param name="messageEncoderSettings">The message encoder settings.</param>
        public ListIndexesUsingQueryOperation(
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
            var operation = CreateOperation();
            return operation.Execute(binding, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<IAsyncCursor<BsonDocument>> ExecuteAsync(IReadBinding binding, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(binding, nameof(binding));
            var operation = CreateOperation();
            return await operation.ExecuteAsync(binding, cancellationToken).ConfigureAwait(false);
        }

        // private methods
        private FindOperation<BsonDocument> CreateOperation()
        {
            var systemIndexesCollection = _collectionNamespace.DatabaseNamespace.SystemIndexesCollection;
            var filter = new BsonDocument("ns", _collectionNamespace.FullName);
            return new FindOperation<BsonDocument>(systemIndexesCollection, BsonDocumentSerializer.Instance, _messageEncoderSettings)
            {
                Filter = filter
            };
        }
    }
}

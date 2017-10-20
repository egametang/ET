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

using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders;

namespace MongoDB.Driver.Core.WireProtocol.Messages
{
    /// <summary>
    /// Represents a GetMore message.
    /// </summary>
    public class GetMoreMessage : RequestMessage
    {
        // fields
        private readonly int _batchSize;
        private readonly CollectionNamespace _collectionNamespace;
        private readonly long _cursorId;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="GetMoreMessage"/> class.
        /// </summary>
        /// <param name="requestId">The request identifier.</param>
        /// <param name="collectionNamespace">The collection namespace.</param>
        /// <param name="cursorId">The cursor identifier.</param>
        /// <param name="batchSize">The size of a batch.</param>
        public GetMoreMessage(
            int requestId,
            CollectionNamespace collectionNamespace,
            long cursorId,
            int batchSize)
            : base(requestId)
        {
            _collectionNamespace = Ensure.IsNotNull(collectionNamespace, nameof(collectionNamespace));
            _cursorId = cursorId;
            _batchSize = Ensure.IsGreaterThanOrEqualToZero(batchSize, nameof(batchSize));
        }

        // properties
        /// <summary>
        /// Gets the size of a batch.
        /// </summary>
        public int BatchSize
        {
            get { return _batchSize; }
        }

        /// <summary>
        /// Gets the collection namespace.
        /// </summary>
        public CollectionNamespace CollectionNamespace
        {
            get { return _collectionNamespace; }
        }

        /// <summary>
        /// Gets the cursor identifier.
        /// </summary>
        public long CursorId
        {
            get { return _cursorId; }
        }

        /// <inheritdoc/>
        public override MongoDBMessageType MessageType
        {
            get { return MongoDBMessageType.GetMore; }
        }

        // methods
        /// <inheritdoc/>
        public override IMessageEncoder GetEncoder(IMessageEncoderFactory encoderFactory)
        {
            return encoderFactory.GetGetMoreMessageEncoder();
        }
    }
}

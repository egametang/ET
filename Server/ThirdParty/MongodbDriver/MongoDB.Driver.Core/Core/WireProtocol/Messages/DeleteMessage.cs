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


using MongoDB.Bson;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders;

namespace MongoDB.Driver.Core.WireProtocol.Messages
{
    /// <summary>
    /// Represents a Delete message.
    /// </summary>
    public class DeleteMessage : RequestMessage
    {
        // fields
        private readonly CollectionNamespace _collectionNamespace;
        private readonly bool _isMulti;
        private readonly BsonDocument _query;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteMessage"/> class.
        /// </summary>
        /// <param name="requestId">The request identifier.</param>
        /// <param name="collectionNamespace">The collection namespace.</param>
        /// <param name="query">The query.</param>
        /// <param name="isMulti">if set to <c>true</c> [is multi].</param>
        public DeleteMessage(
            int requestId,
            CollectionNamespace collectionNamespace,
            BsonDocument query,
            bool isMulti)
            : base(requestId)
        {
            _collectionNamespace = Ensure.IsNotNull(collectionNamespace, nameof(collectionNamespace));
            _query = Ensure.IsNotNull(query, nameof(query));
            _isMulti = isMulti;
        }

        // properties
        /// <summary>
        /// Gets the collection namespace.
        /// </summary>
        public CollectionNamespace CollectionNamespace
        {
            get { return _collectionNamespace; }
        }

        /// <summary>
        /// Gets a value indicating whether to delete all matching documents.
        /// </summary>
        public bool IsMulti
        {
            get { return _isMulti; }
        }

        /// <inheritdoc/>
        public override MongoDBMessageType MessageType
        {
            get { return MongoDBMessageType.Delete; }
        }

        /// <summary>
        /// Gets the query.
        /// </summary>
        public BsonDocument Query
        {
            get { return _query; }
        }

        // methods
        /// <inheritdoc/>
        public override IMessageEncoder GetEncoder(IMessageEncoderFactory encoderFactory)
        {
            return encoderFactory.GetDeleteMessageEncoder();
        }
    }
}

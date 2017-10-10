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
using MongoDB.Bson.IO;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders;

namespace MongoDB.Driver.Core.WireProtocol.Messages
{
    /// <summary>
    /// Represents an Update message.
    /// </summary>
    public class UpdateMessage : RequestMessage
    {
        // fields
        private readonly CollectionNamespace _collectionNamespace;
        private readonly bool _isMulti;
        private readonly bool _isUpsert;
        private readonly BsonDocument _query;
        private readonly BsonDocument _update;
        private readonly IElementNameValidator _updateValidator;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateMessage"/> class.
        /// </summary>
        /// <param name="requestId">The request identifier.</param>
        /// <param name="collectionNamespace">The collection namespace.</param>
        /// <param name="query">The query.</param>
        /// <param name="update">The update.</param>
        /// <param name="updateValidator">The update validator.</param>
        /// <param name="isMulti">if set to <c>true</c> all matching documents should be updated.</param>
        /// <param name="isUpsert">if set to <c>true</c> a document should be inserted if no matching document is found.</param>
        public UpdateMessage(
            int requestId,
            CollectionNamespace collectionNamespace,
            BsonDocument query,
            BsonDocument update,
            IElementNameValidator updateValidator,
            bool isMulti,
            bool isUpsert)
            : base(requestId)
        {
            _collectionNamespace = Ensure.IsNotNull(collectionNamespace, nameof(collectionNamespace));
            _query = Ensure.IsNotNull(query, nameof(query));
            _update = Ensure.IsNotNull(update, nameof(update));
            _updateValidator = Ensure.IsNotNull(updateValidator, nameof(updateValidator));
            _isMulti = isMulti;
            _isUpsert = isUpsert;
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
        /// Gets a value indicating whether all matching documents should be updated.
        /// </summary>
        public bool IsMulti
        {
            get { return _isMulti; }
        }

        /// <summary>
        /// Gets a value indicating whether a document should be inserted if no matching document is found.
        /// </summary>
        public bool IsUpsert
        {
            get { return _isUpsert; }
        }

        /// <inheritdoc/>
        public override MongoDBMessageType MessageType
        {
            get { return MongoDBMessageType.Update; }
        }

        /// <summary>
        /// Gets the query.
        /// </summary>
        public BsonDocument Query
        {
            get { return _query; }
        }

        /// <summary>
        /// Gets the update.
        /// </summary>
        public BsonDocument Update
        {
            get { return _update; }
        }

        /// <summary>
        /// Gets the update validator.
        /// </summary>
        public IElementNameValidator UpdateValidator
        {
            get { return _updateValidator; }
        }

        // methods
        /// <inheritdoc/>
        public override IMessageEncoder GetEncoder(IMessageEncoderFactory encoderFactory)
        {
            return encoderFactory.GetUpdateMessageEncoder();
        }
    }
}

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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders;

namespace MongoDB.Driver.Core.WireProtocol.Messages
{
    /// <summary>
    /// Represents a Reply message.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document.</typeparam>
    public class ReplyMessage<TDocument> : ResponseMessage
    {
        // fields
        private readonly bool _awaitCapable;
        private readonly long _cursorId;
        private readonly bool _cursorNotFound;
        private readonly List<TDocument> _documents;
        private readonly int _numberReturned;
        private readonly bool _queryFailure;
        private readonly BsonDocument _queryFailureDocument;
        private readonly IBsonSerializer<TDocument> _serializer;
        private readonly int _startingFrom;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ReplyMessage{TDocument}"/> class.
        /// </summary>
        /// <param name="awaitCapable">if set to <c>true</c> the server is await capable.</param>
        /// <param name="cursorId">The cursor identifier.</param>
        /// <param name="cursorNotFound">if set to <c>true</c> the cursor was not found.</param>
        /// <param name="documents">The documents.</param>
        /// <param name="numberReturned">The number of documents returned.</param>
        /// <param name="queryFailure">if set to <c>true</c> the query failed.</param>
        /// <param name="queryFailureDocument">The query failure document.</param>
        /// <param name="requestId">The request identifier.</param>
        /// <param name="responseTo">The identifier of the message this is a response to.</param>
        /// <param name="serializer">The serializer.</param>
        /// <param name="startingFrom">The position of the first document in this batch in the overall result.</param>
        public ReplyMessage(
            bool awaitCapable,
            long cursorId,
            bool cursorNotFound,
            List<TDocument> documents,
            int numberReturned,
            bool queryFailure,
            BsonDocument queryFailureDocument,
            int requestId,
            int responseTo,
            IBsonSerializer<TDocument> serializer,
            int startingFrom)
            : base(requestId, responseTo)
        {
            if (documents == null && queryFailureDocument == null && !cursorNotFound)
            {
                throw new ArgumentException("Either documents or queryFailureDocument must be provided.");
            }
            if (documents != null && queryFailureDocument != null)
            {
                throw new ArgumentException("Documents and queryFailureDocument cannot both be provided.");
            }
            _awaitCapable = awaitCapable;
            _cursorId = cursorId;
            _cursorNotFound = cursorNotFound;
            _documents = documents; // can be null
            _numberReturned = numberReturned;
            _queryFailure = queryFailure;
            _queryFailureDocument = queryFailureDocument; // can be null
            _serializer = Ensure.IsNotNull(serializer, nameof(serializer));
            _startingFrom = startingFrom;
        }

        // properties
        /// <summary>
        /// Gets a value indicating whether the server is await capable.
        /// </summary>
        public bool AwaitCapable
        {
            get { return _awaitCapable; }
        }

        /// <summary>
        /// Gets the cursor identifier.
        /// </summary>
        public long CursorId
        {
            get { return _cursorId; }
        }

        /// <summary>
        /// Gets a value indicating whether the cursor was not found.
        /// </summary>
        public bool CursorNotFound
        {
            get { return _cursorNotFound; }
        }

        /// <summary>
        /// Gets the documents.
        /// </summary>
        public List<TDocument> Documents
        {
            get { return _documents; }
        }

        /// <inheritdoc/>
        public override MongoDBMessageType MessageType
        {
            get { return MongoDBMessageType.Reply; }
        }

        /// <summary>
        /// Gets the number of documents returned.
        /// </summary>
        public int NumberReturned
        {
            get { return _numberReturned; }
        }

        /// <summary>
        /// Gets a value indicating whether the query failed.
        /// </summary>
        public bool QueryFailure
        {
            get { return _queryFailure; }
        }

        /// <summary>
        /// Gets the query failure document.
        /// </summary>
        public BsonDocument QueryFailureDocument
        {
            get { return _queryFailureDocument; }
        }

        /// <summary>
        /// Gets the serializer.
        /// </summary>
        public IBsonSerializer<TDocument> Serializer
        {
            get { return _serializer; }
        }

        /// <summary>
        /// Gets the position of the first document in this batch in the overall result.
        /// </summary>
        public int StartingFrom
        {
            get { return _startingFrom; }
        }

        // methods
        /// <inheritdoc/>
        public override IMessageEncoder GetEncoder(IMessageEncoderFactory encoderFactory)
        {
            return encoderFactory.GetReplyMessageEncoder<TDocument>(_serializer);
        }
    }
}

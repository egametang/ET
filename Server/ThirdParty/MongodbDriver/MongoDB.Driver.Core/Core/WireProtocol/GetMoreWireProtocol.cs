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
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.WireProtocol.Messages;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders;

namespace MongoDB.Driver.Core.WireProtocol
{
    internal class GetMoreWireProtocol<TDocument> : IWireProtocol<CursorBatch<TDocument>>
    {
        // fields
        private readonly int _batchSize;
        private readonly CollectionNamespace _collectionNamespace;
        private readonly long _cursorId;
        private readonly MessageEncoderSettings _messageEncoderSettings;
        private readonly BsonDocument _query;
        private readonly IBsonSerializer<TDocument> _serializer;

        // constructors
        public GetMoreWireProtocol(
            CollectionNamespace collectionNamespace,
            BsonDocument query,
            long cursorId,
            int batchSize,
            IBsonSerializer<TDocument> serializer,
            MessageEncoderSettings messageEncoderSettings)
        {
            _collectionNamespace = Ensure.IsNotNull(collectionNamespace, nameof(collectionNamespace));
            _query = Ensure.IsNotNull(query, nameof(query));
            _cursorId = cursorId;
            _batchSize = Ensure.IsGreaterThanOrEqualToZero(batchSize, nameof(batchSize));
            _serializer = Ensure.IsNotNull(serializer, nameof(serializer));
            _messageEncoderSettings = messageEncoderSettings;
        }

        // methods
        private GetMoreMessage CreateMessage()
        {
            return new GetMoreMessage(
                RequestMessage.GetNextRequestId(),
                _collectionNamespace,
                _cursorId,
                _batchSize);
        }

        public CursorBatch<TDocument> Execute(IConnection connection, CancellationToken cancellationToken)
        {
            var message = CreateMessage();
            connection.SendMessage(message, _messageEncoderSettings, cancellationToken);
            var encoderSelector = new ReplyMessageEncoderSelector<TDocument>(_serializer);
            var reply = connection.ReceiveMessage(message.RequestId, encoderSelector, _messageEncoderSettings, cancellationToken);
            return ProcessReply(connection.ConnectionId, (ReplyMessage<TDocument>)reply);
        }

        public async Task<CursorBatch<TDocument>> ExecuteAsync(IConnection connection, CancellationToken cancellationToken)
        {
            var message = CreateMessage();
            await connection.SendMessageAsync(message, _messageEncoderSettings, cancellationToken).ConfigureAwait(false);
            var encoderSelector = new ReplyMessageEncoderSelector<TDocument>(_serializer);
            var reply = await connection.ReceiveMessageAsync(message.RequestId, encoderSelector, _messageEncoderSettings, cancellationToken).ConfigureAwait(false);
            return ProcessReply(connection.ConnectionId, (ReplyMessage<TDocument>)reply);
        }

        private CursorBatch<TDocument> ProcessReply(ConnectionId connectionId, ReplyMessage<TDocument> reply)
        {
            if (reply.CursorNotFound)
            {
                throw new MongoCursorNotFoundException(connectionId, _cursorId, _query);
            }

            if (reply.QueryFailure)
            {
                var failureDocument = reply.QueryFailureDocument;
                var errorMessage = string.Format("GetMore QueryFailure: {0}.", failureDocument);
                throw ExceptionMapper.Map(connectionId, failureDocument) ?? new MongoQueryException(connectionId, errorMessage, _query, failureDocument);
            }

            return new CursorBatch<TDocument>(reply.CursorId, reply.Documents);
        }
    }
}

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
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.WireProtocol.Messages;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders;

namespace MongoDB.Driver.Core.WireProtocol
{
    internal class QueryWireProtocol<TDocument> : IWireProtocol<CursorBatch<TDocument>>
    {
        // fields
        private readonly bool _awaitData;
        private readonly int _batchSize;
        private readonly CollectionNamespace _collectionNamespace;
        private readonly MessageEncoderSettings _messageEncoderSettings;
        private readonly BsonDocument _fields;
        private readonly bool _noCursorTimeout;
        private readonly bool _oplogReplay;
        private readonly bool _partialOk;
        private readonly BsonDocument _query;
        private readonly IElementNameValidator _queryValidator;
        private readonly IBsonSerializer<TDocument> _serializer;
        private readonly int _skip;
        private readonly bool _slaveOk;
        private readonly bool _tailableCursor;

        // constructors
        public QueryWireProtocol(
            CollectionNamespace collectionNamespace,
            BsonDocument query,
            BsonDocument fields,
            IElementNameValidator queryValidator,
            int skip,
            int batchSize,
            bool slaveOk,
            bool partialOk,
            bool noCursorTimeout,
            bool oplogReplay,
            bool tailableCursor,
            bool awaitData,
            IBsonSerializer<TDocument> serializer,
            MessageEncoderSettings messageEncoderSettings)
        {
            _collectionNamespace = Ensure.IsNotNull(collectionNamespace, nameof(collectionNamespace));
            _query = Ensure.IsNotNull(query, nameof(query));
            _fields = fields; // can be null
            _queryValidator = Ensure.IsNotNull(queryValidator, nameof(queryValidator));
            _skip = Ensure.IsGreaterThanOrEqualToZero(skip, nameof(skip));
            _batchSize = batchSize; // can be negative
            _slaveOk = slaveOk;
            _partialOk = partialOk;
            _noCursorTimeout = noCursorTimeout;
            _oplogReplay = oplogReplay;
            _tailableCursor = tailableCursor;
            _awaitData = awaitData;
            _serializer = Ensure.IsNotNull(serializer, nameof(serializer));
            _messageEncoderSettings = messageEncoderSettings;
        }

        // methods
        private QueryMessage CreateMessage()
        {
            return new QueryMessage(
                RequestMessage.GetNextRequestId(),
                _collectionNamespace,
                _query,
                _fields,
                _queryValidator,
                _skip,
                _batchSize,
                _slaveOk,
                _partialOk,
                _noCursorTimeout,
                _oplogReplay,
                _tailableCursor,
                _awaitData);
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
            if (reply.QueryFailure)
            {
                var response = reply.QueryFailureDocument;

                var notPrimaryOrNodeIsRecoveringException = ExceptionMapper.MapNotPrimaryOrNodeIsRecovering(connectionId, response, "$err");
                if (notPrimaryOrNodeIsRecoveringException != null)
                {
                    throw notPrimaryOrNodeIsRecoveringException;
                }

                var mappedException = ExceptionMapper.Map(connectionId, response);
                if (mappedException != null)
                {
                    throw mappedException;
                }

                var message = string.Format("QueryFailure flag was true (response was {0}).", response.ToJson());
                throw new MongoQueryException(connectionId, message, _query, response);
            }

            return new CursorBatch<TDocument>(reply.CursorId, reply.Documents);
        }
    }
}

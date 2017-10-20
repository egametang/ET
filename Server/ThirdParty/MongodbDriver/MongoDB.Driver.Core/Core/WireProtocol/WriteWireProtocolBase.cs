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
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.WireProtocol.Messages;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders;

namespace MongoDB.Driver.Core.WireProtocol
{
    internal abstract class WriteWireProtocolBase : IWireProtocol<WriteConcernResult>
    {
        // fields
        private readonly CollectionNamespace _collectionNamespace;
        private readonly MessageEncoderSettings _messageEncoderSettings;
        private readonly Func<bool> _shouldSendGetLastError;
        private readonly WriteConcern _writeConcern;

        // constructors
        protected WriteWireProtocolBase(
            CollectionNamespace collectionNamespace,
            MessageEncoderSettings messageEncoderSettings,
            WriteConcern writeConcern,
            Func<bool> shouldSendGetLastError = null)
        {
            _collectionNamespace = Ensure.IsNotNull(collectionNamespace, nameof(collectionNamespace));
            _messageEncoderSettings = messageEncoderSettings;
            _writeConcern = Ensure.IsNotNull(writeConcern, nameof(writeConcern));
            _shouldSendGetLastError = shouldSendGetLastError;
        }

        // properties
        protected CollectionNamespace CollectionNamespace
        {
            get { return _collectionNamespace; }
        }

        protected WriteConcern WriteConcern
        {
            get { return _writeConcern; }
        }

        // methods
        private BsonDocument CreateGetLastErrorCommand()
        {
            var command = _writeConcern.ToBsonDocument();
            command.InsertAt(0, new BsonElement("getLastError", 1));
            return command;
        }

        private QueryMessage CreateGetLastErrorMessage(BsonDocument getLastErrorCommand)
        {
            return new QueryMessage(
               RequestMessage.GetNextRequestId(),
               _collectionNamespace.DatabaseNamespace.CommandCollection,
               getLastErrorCommand,
               null,
               NoOpElementNameValidator.Instance,
               0,
               -1,
               true,
               false,
               false,
               false,
               false,
               false,
               _shouldSendGetLastError);
        }

        private List<RequestMessage> CreateMessages(IConnection connection, out QueryMessage getLastErrorMessage)
        {
            var messages = new List<RequestMessage>();

            var writeMessage = CreateWriteMessage(connection);
            messages.Add(writeMessage);

            getLastErrorMessage = null;
            if (_writeConcern.IsAcknowledged)
            {
                var getLastErrorCommand = CreateGetLastErrorCommand();
                getLastErrorMessage = CreateGetLastErrorMessage(getLastErrorCommand);
                messages.Add(getLastErrorMessage);
            }

            return messages;
        }

        protected abstract RequestMessage CreateWriteMessage(IConnection connection);

        public WriteConcernResult Execute(IConnection connection, CancellationToken cancellationToken)
        {
            QueryMessage getLastErrorMessage;
            var messages = CreateMessages(connection, out getLastErrorMessage);

            connection.SendMessages(messages, _messageEncoderSettings, cancellationToken);
            if (getLastErrorMessage != null && getLastErrorMessage.WasSent)
            {
                var encoderSelector = new ReplyMessageEncoderSelector<BsonDocument>(BsonDocumentSerializer.Instance);
                var reply = connection.ReceiveMessage(getLastErrorMessage.RequestId, encoderSelector, _messageEncoderSettings, cancellationToken);
                return ProcessReply(connection.ConnectionId, getLastErrorMessage.Query, (ReplyMessage<BsonDocument>)reply);
            }
            else
            {
                return null;
            }
        }

        public async Task<WriteConcernResult> ExecuteAsync(IConnection connection, CancellationToken cancellationToken)
        {
            QueryMessage getLastErrorMessage;
            var messages = CreateMessages(connection, out getLastErrorMessage);

            await connection.SendMessagesAsync(messages, _messageEncoderSettings, cancellationToken).ConfigureAwait(false);
            if (getLastErrorMessage != null && getLastErrorMessage.WasSent)
            {
                var encoderSelector = new ReplyMessageEncoderSelector<BsonDocument>(BsonDocumentSerializer.Instance);
                var reply = await connection.ReceiveMessageAsync(getLastErrorMessage.RequestId, encoderSelector, _messageEncoderSettings, cancellationToken).ConfigureAwait(false);
                return ProcessReply(connection.ConnectionId, getLastErrorMessage.Query, (ReplyMessage<BsonDocument>)reply);
            }
            else
            {
                return null;
            }
        }

        private WriteConcernResult ProcessReply(ConnectionId connectionId, BsonDocument getLastErrorCommand, ReplyMessage<BsonDocument> reply)
        {
            if (reply.NumberReturned == 0)
            {
                throw new MongoCommandException(connectionId, "GetLastError reply had no documents.", getLastErrorCommand);
            }
            if (reply.NumberReturned > 1)
            {
                throw new MongoCommandException(connectionId, "GetLastError reply had more than one document.", getLastErrorCommand);
            }
            if (reply.QueryFailure)
            {
                throw new MongoCommandException(connectionId, "GetLastError reply had QueryFailure flag set.", getLastErrorCommand, reply.QueryFailureDocument);
            }

            var response = reply.Documents.Single();

            var notPrimaryOrNodeIsRecoveringException = ExceptionMapper.MapNotPrimaryOrNodeIsRecovering(connectionId, response, "err");
            if (notPrimaryOrNodeIsRecoveringException != null)
            {
                throw notPrimaryOrNodeIsRecoveringException;
            }

            var writeConcernResult = new WriteConcernResult(response);

            var mappedException = ExceptionMapper.Map(connectionId, writeConcernResult);
            if (mappedException != null)
            {
                throw mappedException;
            }

            return writeConcernResult;
        }
    }
}

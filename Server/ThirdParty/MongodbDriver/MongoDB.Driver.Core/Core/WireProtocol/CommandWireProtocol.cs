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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.WireProtocol.Messages;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders.BinaryEncoders;

namespace MongoDB.Driver.Core.WireProtocol
{
    internal class CommandWireProtocol<TCommandResult> : IWireProtocol<TCommandResult>
    {
        // fields
        private readonly BsonDocument _command;
        private readonly Func<CommandResponseHandling> _responseHandling;
        private readonly IElementNameValidator _commandValidator;
        private readonly DatabaseNamespace _databaseNamespace;
        private readonly MessageEncoderSettings _messageEncoderSettings;
        private readonly IBsonSerializer<TCommandResult> _resultSerializer;
        private readonly bool _slaveOk;

        // constructors
        public CommandWireProtocol(
            DatabaseNamespace databaseNamespace,
            BsonDocument command,
            bool slaveOk,
            IBsonSerializer<TCommandResult> resultSerializer,
            MessageEncoderSettings messageEncoderSettings)
            : this(
                databaseNamespace,
                command,
                NoOpElementNameValidator.Instance,
                () => CommandResponseHandling.Return,
                slaveOk,
                resultSerializer,
                messageEncoderSettings)
        {
        }

        public CommandWireProtocol(
            DatabaseNamespace databaseNamespace,
            BsonDocument command,
            IElementNameValidator commandValidator,
            Func<CommandResponseHandling> responseHandling,
            bool slaveOk,
            IBsonSerializer<TCommandResult> resultSerializer,
            MessageEncoderSettings messageEncoderSettings)
        {
            _databaseNamespace = Ensure.IsNotNull(databaseNamespace, nameof(databaseNamespace));
            _command = Ensure.IsNotNull(command, nameof(command));
            _commandValidator = Ensure.IsNotNull(commandValidator, nameof(commandValidator));
            _responseHandling = responseHandling;
            _slaveOk = slaveOk;
            _resultSerializer = Ensure.IsNotNull(resultSerializer, nameof(resultSerializer));
            _messageEncoderSettings = messageEncoderSettings;
        }

        // methods
        private QueryMessage CreateMessage()
        {
            return new QueryMessage(
                RequestMessage.GetNextRequestId(),
                _databaseNamespace.CommandCollection,
                _command,
                null,
                _commandValidator,
                0,
                -1,
                _slaveOk,
                false,
                false,
                false,
                false,
                false);
        }

        public TCommandResult Execute(IConnection connection, CancellationToken cancellationToken)
        {
            var message = CreateMessage();
            connection.SendMessage(message, _messageEncoderSettings, cancellationToken);

            switch (_responseHandling())
            {
                case CommandResponseHandling.Ignore:
                    IgnoreResponse(connection, message, cancellationToken);
                    return default(TCommandResult);
                default:
                    var encoderSelector = new ReplyMessageEncoderSelector<RawBsonDocument>(RawBsonDocumentSerializer.Instance);
                    var reply = connection.ReceiveMessage(message.RequestId, encoderSelector, _messageEncoderSettings, cancellationToken);
                    return ProcessReply(connection.ConnectionId, (ReplyMessage<RawBsonDocument>)reply);
            }
        }

        public async Task<TCommandResult> ExecuteAsync(IConnection connection, CancellationToken cancellationToken)
        {
            var message = CreateMessage();
            await connection.SendMessageAsync(message, _messageEncoderSettings, cancellationToken).ConfigureAwait(false);
            switch (_responseHandling())
            {
                case CommandResponseHandling.Ignore:
                    IgnoreResponse(connection, message, cancellationToken);
                    return default(TCommandResult);
                default:
                    var encoderSelector = new ReplyMessageEncoderSelector<RawBsonDocument>(RawBsonDocumentSerializer.Instance);
                    var reply = await connection.ReceiveMessageAsync(message.RequestId, encoderSelector, _messageEncoderSettings, cancellationToken).ConfigureAwait(false);
                    return ProcessReply(connection.ConnectionId, (ReplyMessage<RawBsonDocument>)reply);
            }
        }

        private void IgnoreResponse(IConnection connection, QueryMessage message, CancellationToken cancellationToken)
        {
            var encoderSelector = new ReplyMessageEncoderSelector<IgnoredReply>(IgnoredReplySerializer.Instance);
            connection.ReceiveMessageAsync(message.RequestId, encoderSelector, _messageEncoderSettings, cancellationToken).IgnoreExceptions();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        private TCommandResult ProcessReply(ConnectionId connectionId, ReplyMessage<RawBsonDocument> reply)
        {
            if (reply.NumberReturned == 0)
            {
                throw new MongoCommandException(connectionId, "Command returned no documents.", _command);
            }
            if (reply.NumberReturned > 1)
            {
                throw new MongoCommandException(connectionId, "Command returned multiple documents.", _command);
            }
            if (reply.QueryFailure)
            {
                var failureDocument = reply.QueryFailureDocument;
                throw ExceptionMapper.Map(connectionId, failureDocument) ?? new MongoCommandException(connectionId, "Command failed.", _command, failureDocument);
            }

            using (var rawDocument = reply.Documents[0])
            {
                if (!rawDocument.GetValue("ok", false).ToBoolean())
                {
                    var binaryReaderSettings = new BsonBinaryReaderSettings();
                    if (_messageEncoderSettings != null)
                    {
                        binaryReaderSettings.Encoding = _messageEncoderSettings.GetOrDefault<UTF8Encoding>(MessageEncoderSettingsName.ReadEncoding, Utf8Encodings.Strict);
                        binaryReaderSettings.GuidRepresentation = _messageEncoderSettings.GetOrDefault<GuidRepresentation>(MessageEncoderSettingsName.GuidRepresentation, GuidRepresentation.CSharpLegacy);
                    };
                    var materializedDocument = rawDocument.Materialize(binaryReaderSettings);

                    var commandName = _command.GetElement(0).Name;
                    if (commandName == "$query")
                    {
                        commandName = _command["$query"].AsBsonDocument.GetElement(0).Name;
                    }

                    var notPrimaryOrNodeIsRecoveringException = ExceptionMapper.MapNotPrimaryOrNodeIsRecovering(connectionId, materializedDocument, "errmsg");
                    if (notPrimaryOrNodeIsRecoveringException != null)
                    {
                        throw notPrimaryOrNodeIsRecoveringException;
                    }

                    string message;
                    BsonValue errmsgBsonValue;
                    if (materializedDocument.TryGetValue("errmsg", out errmsgBsonValue) && errmsgBsonValue.IsString)
                    {
                        var errmsg = errmsgBsonValue.ToString();
                        message = string.Format("Command {0} failed: {1}.", commandName, errmsg);
                    }
                    else
                    {
                        message = string.Format("Command {0} failed.", commandName);
                    }

                    var mappedException = ExceptionMapper.Map(connectionId, materializedDocument);
                    if (mappedException != null)
                    {
                        throw mappedException;
                    }

                    throw new MongoCommandException(connectionId, message, _command, materializedDocument);
                }

                using (var stream = new ByteBufferStream(rawDocument.Slice, ownsBuffer: false))
                {
                    var encoderFactory = new BinaryMessageEncoderFactory(stream, _messageEncoderSettings);
                    var encoder = (ReplyMessageBinaryEncoder<TCommandResult>)encoderFactory.GetReplyMessageEncoder<TCommandResult>(_resultSerializer);
                    using (var reader = encoder.CreateBinaryReader())
                    {
                        var context = BsonDeserializationContext.CreateRoot(reader);
                        return _resultSerializer.Deserialize(context);
                    }
                }
            }
        }

        private class IgnoredReply
        {
            public static IgnoredReply Instance = new IgnoredReply();
        }

        private class IgnoredReplySerializer : SerializerBase<IgnoredReply>
        {
            public static IgnoredReplySerializer Instance = new IgnoredReplySerializer();

            public override IgnoredReply Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
            {
                context.Reader.ReadStartDocument();
                while (context.Reader.ReadBsonType() != BsonType.EndOfDocument)
                {
                    context.Reader.SkipName();
                    context.Reader.SkipValue();
                }
                context.Reader.ReadEndDocument();
                return IgnoredReply.Instance;
            }
        }
    }
}

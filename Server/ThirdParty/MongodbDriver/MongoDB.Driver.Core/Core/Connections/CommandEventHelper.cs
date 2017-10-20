/* Copyright 2015-2016 MongoDB Inc.
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Core.Events;
using MongoDB.Driver.Core.WireProtocol.Messages;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders.BinaryEncoders;

namespace MongoDB.Driver.Core.Connections
{
    internal class CommandEventHelper
    {
        private static readonly string[] __writeConcernIndicators = new[] { "wtimeout", "jnote", "wnote" };
        private static readonly HashSet<string> __securitySensitiveCommands = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "authenticate",
            "saslStart",
            "saslContinue",
            "getnonce",
            "createUser",
            "updateUser",
            "copydbgetnonce",
            "copydbsaslstart",
            "copydb"
        };

        private readonly ConcurrentDictionary<int, CommandState> _state;
        private readonly Action<CommandStartedEvent> _startedEvent;
        private readonly Action<CommandSucceededEvent> _succeededEvent;
        private readonly Action<CommandFailedEvent> _failedEvent;

        private readonly bool _shouldProcessRequestMessages;
        private readonly bool _shouldTrackState;

        public CommandEventHelper(IEventSubscriber eventSubscriber)
        {
            eventSubscriber.TryGetEventHandler(out _startedEvent);
            eventSubscriber.TryGetEventHandler(out _succeededEvent);
            eventSubscriber.TryGetEventHandler(out _failedEvent);

            _shouldTrackState = _succeededEvent != null || _failedEvent != null;
            _shouldProcessRequestMessages = _startedEvent != null || _shouldTrackState;

            if (_shouldTrackState)
            {
                // we only need to track state if we have to raise
                // a succeeded or failed event
                _state = new ConcurrentDictionary<int, CommandState>();
            }
        }

        public bool ShouldCallBeforeSending
        {
            get { return _shouldProcessRequestMessages; }
        }

        public bool ShouldCallAfterSending
        {
            get { return _shouldTrackState; }
        }

        public bool ShouldCallErrorSending
        {
            get { return _shouldTrackState; }
        }

        public bool ShouldCallAfterReceiving
        {
            get { return _shouldTrackState; }
        }

        public bool ShouldCallErrorReceiving
        {
            get { return _shouldTrackState; }
        }

        public void BeforeSending(IEnumerable<RequestMessage> messages, ConnectionId connectionId, IByteBuffer buffer, MessageEncoderSettings encoderSettings, Stopwatch stopwatch)
        {
            using (var stream = new ByteBufferStream(buffer, ownsBuffer: false))
            {
                var messageQueue = new Queue<RequestMessage>(messages);

                while (messageQueue.Count > 0)
                {
                    ProcessRequestMessages(messageQueue, connectionId, stream, encoderSettings, stopwatch);
                }
            }
        }

        public void AfterSending(IEnumerable<RequestMessage> messages, ConnectionId connectionId)
        {
            foreach (var message in messages)
            {
                CommandState state;
                if (_state.TryGetValue(message.RequestId, out state) &&
                    state.ExpectedResponseType == ExpectedResponseType.None)
                {
                    state.Stopwatch.Stop();
                    if (_succeededEvent != null)
                    {
                        var @event = new CommandSucceededEvent(
                            state.CommandName,
                            state.NoResponseResponse ?? new BsonDocument("ok", 1),
                            state.OperationId,
                            message.RequestId,
                            connectionId,
                            state.Stopwatch.Elapsed);

                        _succeededEvent(@event);
                    }

                    _state.TryRemove(message.RequestId, out state);
                }
            }
        }

        public void ErrorSending(IEnumerable<RequestMessage> messages, ConnectionId connectionId, Exception exception)
        {
            foreach (var message in messages)
            {
                CommandState state;
                if (_state.TryRemove(message.RequestId, out state))
                {
                    state.Stopwatch.Stop();
                    if (_failedEvent != null)
                    {
                        var @event = new CommandFailedEvent(
                            state.CommandName,
                            exception,
                            state.OperationId,
                            message.RequestId,
                            connectionId,
                            state.Stopwatch.Elapsed);

                        _failedEvent(@event);
                    }
                }
            }
        }

        public void AfterReceiving(ResponseMessage message, IByteBuffer buffer, ConnectionId connectionId, MessageEncoderSettings encoderSettings)
        {
            CommandState state;
            if (!_state.TryRemove(message.ResponseTo, out state))
            {
                // this indicates a bug in the sending portion...
                return;
            }

            ProcessReplyMessage(state, message, buffer, connectionId, encoderSettings);
        }

        public void ErrorReceiving(int responseTo, ConnectionId connectionId, Exception exception)
        {
            CommandState state;
            if (!_state.TryRemove(responseTo, out state))
            {
                // this indicates a bug in the sending portion...
                return;
            }

            state.Stopwatch.Stop();
            if (_failedEvent != null)
            {
                _failedEvent(new CommandFailedEvent(
                    state.CommandName,
                    exception,
                    state.OperationId,
                    responseTo,
                    connectionId,
                    state.Stopwatch.Elapsed));
            }
        }

        public void ConnectionFailed(ConnectionId connectionId, Exception exception)
        {
            if (_failedEvent == null)
            {
                return;
            }

            var requestIds = _state.Keys;
            foreach (var requestId in requestIds)
            {
                CommandState state;
                if (_state.TryRemove(requestId, out state))
                {
                    state.Stopwatch.Stop();
                    var @event = new CommandFailedEvent(
                        state.CommandName,
                        exception,
                        state.OperationId,
                        requestId,
                        connectionId,
                        state.Stopwatch.Elapsed);

                    _failedEvent(@event);
                }
            }
        }

        private void ProcessRequestMessages(Queue<RequestMessage> messageQueue, ConnectionId connectionId, Stream stream, MessageEncoderSettings encoderSettings, Stopwatch stopwatch)
        {
            var message = messageQueue.Dequeue();
            switch (message.MessageType)
            {
                case MongoDBMessageType.Delete:
                    ProcessDeleteMessage((DeleteMessage)message, messageQueue, connectionId, new DeleteMessageBinaryEncoder(stream, encoderSettings), stopwatch);
                    break;
                case MongoDBMessageType.GetMore:
                    ProcessGetMoreMessage((GetMoreMessage)message, connectionId, stopwatch);
                    break;
                case MongoDBMessageType.Insert:
                    ProcessInsertMessage(message, messageQueue, connectionId, new InsertMessageBinaryEncoder<RawBsonDocument>(stream, encoderSettings, RawBsonDocumentSerializer.Instance), stopwatch);
                    break;
                case MongoDBMessageType.KillCursors:
                    ProcessKillCursorsMessages((KillCursorsMessage)message, connectionId, stopwatch);
                    break;
                case MongoDBMessageType.Query:
                    ProcessQueryMessage((QueryMessage)message, connectionId, new QueryMessageBinaryEncoder(stream, encoderSettings), stopwatch);
                    break;
                case MongoDBMessageType.Update:
                    ProcessUpdateMessage((UpdateMessage)message, messageQueue, connectionId, new UpdateMessageBinaryEncoder(stream, encoderSettings), stopwatch);
                    break;
                default:
                    throw new MongoInternalException("Invalid message type.");
            }
        }

        private void ProcessDeleteMessage(DeleteMessage originalMessage, Queue<RequestMessage> messageQueue, ConnectionId connectionId, DeleteMessageBinaryEncoder encoder, Stopwatch stopwatch)
        {
            var commandName = "delete";
            var operationId = EventContext.OperationId;
            int requestId = originalMessage.RequestId;
            var expectedResponseType = ExpectedResponseType.None;
            int gleRequestId;
            WriteConcern writeConcern;
            if (TryGetWriteConcernFromGLE(messageQueue, out gleRequestId, out writeConcern))
            {
                requestId = gleRequestId;
                expectedResponseType = ExpectedResponseType.GLE;
            }

            if (_startedEvent != null)
            {
                var decodedMessage = encoder.ReadMessage(RawBsonDocumentSerializer.Instance);
                try
                {
                    var entry = new BsonDocument
                    {
                        { "q", decodedMessage.Query },
                        { "limit", decodedMessage.IsMulti ? 0 : 1 }
                    };
                    var command = new BsonDocument
                    {
                        { commandName, decodedMessage.CollectionNamespace.CollectionName },
                        { "deletes", new BsonArray(new [] { entry }) }
                    };

                    if (writeConcern == null)
                    {
                        command["writeConcern"] = WriteConcern.Unacknowledged.ToBsonDocument();
                    }
                    else if (!writeConcern.IsServerDefault)
                    {
                        command["writeConcern"] = writeConcern.ToBsonDocument();
                    }

                    var @event = new CommandStartedEvent(
                        commandName,
                        command,
                        decodedMessage.CollectionNamespace.DatabaseNamespace,
                        operationId,
                        requestId,
                        connectionId);

                    _startedEvent(@event);
                }
                finally
                {
                    var disposable = decodedMessage.Query as IDisposable;
                    if (disposable != null)
                    {
                        disposable.Dispose();
                    }
                }
            }

            if (_shouldTrackState)
            {
                _state.TryAdd(requestId, new CommandState
                {
                    CommandName = commandName,
                    OperationId = operationId,
                    Stopwatch = stopwatch,
                    ExpectedResponseType = expectedResponseType
                });
            }
        }

        private void ProcessGetMoreMessage(GetMoreMessage originalMessage, ConnectionId connectionId, Stopwatch stopwatch)
        {
            var commandName = "getMore";
            var operationId = EventContext.OperationId;
            if (_startedEvent != null)
            {
                var command = new BsonDocument
                {
                    { commandName, originalMessage.CursorId },
                    { "collection",  originalMessage.CollectionNamespace.CollectionName },
                    { "batchSize", originalMessage.BatchSize, originalMessage.BatchSize > 0 }
                };

                var @event = new CommandStartedEvent(
                    "getMore",
                    command,
                    originalMessage.CollectionNamespace.DatabaseNamespace,
                    operationId,
                    originalMessage.RequestId,
                    connectionId);

                _startedEvent(@event);
            }

            if (_shouldTrackState)
            {
                _state.TryAdd(originalMessage.RequestId, new CommandState
                {
                    CommandName = commandName,
                    OperationId = operationId,
                    Stopwatch = stopwatch,
                    QueryNamespace = originalMessage.CollectionNamespace,
                    ExpectedResponseType = ExpectedResponseType.Query
                });
            }
        }

        private void ProcessInsertMessage(RequestMessage message, Queue<RequestMessage> messageQueue, ConnectionId connectionId, InsertMessageBinaryEncoder<RawBsonDocument> encoder, Stopwatch stopwatch)
        {
            var commandName = "insert";
            var operationId = EventContext.OperationId;
            var requestId = message.RequestId;
            var expectedResponseType = ExpectedResponseType.None;
            int numberOfDocuments = 0;
            int gleRequestId;
            WriteConcern writeConcern;
            if (TryGetWriteConcernFromGLE(messageQueue, out gleRequestId, out writeConcern))
            {
                requestId = gleRequestId;
                expectedResponseType = ExpectedResponseType.GLE;
            }

            if (_startedEvent != null)
            {
                // InsertMessage is generic, and we don't know the generic type... 
                // Plus, for this we really want BsonDocuments, not whatever the generic type is.
                var decodedMessage = encoder.ReadMessage();

                var documents = decodedMessage.DocumentSource.GetRemainingItems().ToList();
                numberOfDocuments = documents.Count;
                try
                {
                    var command = new BsonDocument
                    {
                        { commandName, decodedMessage.CollectionNamespace.CollectionName },
                        { "documents", new BsonArray(documents) },
                        { "ordered", !decodedMessage.ContinueOnError }
                    };

                    if (writeConcern == null)
                    {
                        command["writeConcern"] = WriteConcern.Unacknowledged.ToBsonDocument();
                    }
                    else if (!writeConcern.IsServerDefault)
                    {
                        command["writeConcern"] = writeConcern.ToBsonDocument();
                    }

                    var @event = new CommandStartedEvent(
                        commandName,
                        command,
                        decodedMessage.CollectionNamespace.DatabaseNamespace,
                        operationId,
                        requestId,
                        connectionId);

                    _startedEvent(@event);
                }
                finally
                {
                    documents.ForEach(d => d.Dispose());
                }
            }

            if (_shouldTrackState)
            {
                _state.TryAdd(requestId, new CommandState
                {
                    CommandName = commandName,
                    OperationId = operationId,
                    Stopwatch = stopwatch,
                    ExpectedResponseType = expectedResponseType,
                    NumberOfInsertedDocuments = numberOfDocuments
                });
            }
        }

        private void ProcessKillCursorsMessages(KillCursorsMessage originalMessage, ConnectionId connectionId, Stopwatch stopwatch)
        {
            const string commandName = "killCursors";
            var operationId = EventContext.OperationId;

            if (_startedEvent != null)
            {
                var collectionNamespace = EventContext.KillCursorsCollectionNamespace ?? DatabaseNamespace.Admin.CommandCollection;
                var command = new BsonDocument
                {
                    { commandName, collectionNamespace.CollectionName },
                    { "cursors", new BsonArray(originalMessage.CursorIds) }
                };

                var @event = new CommandStartedEvent(
                    commandName,
                    command,
                    collectionNamespace.DatabaseNamespace,
                    operationId,
                    originalMessage.RequestId,
                    connectionId);

                _startedEvent(@event);
            }

            if (_shouldTrackState)
            {
                _state.TryAdd(originalMessage.RequestId, new CommandState
                {
                    CommandName = commandName,
                    OperationId = operationId,
                    Stopwatch = stopwatch,
                    ExpectedResponseType = ExpectedResponseType.None,
                    NoResponseResponse = new BsonDocument
                    {
                        { "ok", 1 },
                        { "cursorsUnknown", new BsonArray(originalMessage.CursorIds) }
                    }
                });
            }
        }

        private void ProcessQueryMessage(QueryMessage originalMessage, ConnectionId connectionId, QueryMessageBinaryEncoder encoder, Stopwatch stopwatch)
        {
            var requestId = originalMessage.RequestId;
            var operationId = EventContext.OperationId;

            var decodedMessage = encoder.ReadMessage(RawBsonDocumentSerializer.Instance);
            try
            {
                var isCommand = IsCommand(decodedMessage.CollectionNamespace);
                string commandName;
                BsonDocument command;
                if (isCommand)
                {
                    command = decodedMessage.Query;
                    var firstElement = command.GetElement(0);
                    commandName = firstElement.Name;
                    if (__securitySensitiveCommands.Contains(commandName))
                    {
                        command = new BsonDocument();
                    }
                }
                else
                {
                    commandName = "find";
                    command = BuildFindCommandFromQuery(decodedMessage);
                    if (decodedMessage.Query.GetValue("$explain", false).ToBoolean())
                    {
                        commandName = "explain";
                        command = new BsonDocument("explain", command);
                    }
                }

                if (_startedEvent != null)
                {
                    var @event = new CommandStartedEvent(
                        commandName,
                        command,
                        decodedMessage.CollectionNamespace.DatabaseNamespace,
                        operationId,
                        requestId,
                        connectionId);

                    _startedEvent(@event);
                }

                if (_shouldTrackState)
                {
                    _state.TryAdd(requestId, new CommandState
                    {
                        CommandName = commandName,
                        OperationId = operationId,
                        Stopwatch = stopwatch,
                        QueryNamespace = decodedMessage.CollectionNamespace,
                        ExpectedResponseType = isCommand ? ExpectedResponseType.Command : ExpectedResponseType.Query
                    });
                }
            }
            finally
            {
                var disposable = decodedMessage.Query as IDisposable;
                if (disposable != null)
                {
                    disposable.Dispose();
                }
                disposable = decodedMessage.Fields as IDisposable;
                if (disposable != null)
                {
                    disposable.Dispose();
                }
            }
        }

        private void ProcessReplyMessage(CommandState state, ResponseMessage message, IByteBuffer buffer, ConnectionId connectionId, MessageEncoderSettings encoderSettings)
        {
            state.Stopwatch.Stop();
            bool disposeOfDocuments = false;
            var replyMessage = message as ReplyMessage<RawBsonDocument>;
            if (replyMessage == null)
            {
                // ReplyMessage is generic, which means that we can't use it here, so, we need to use a different one...
                using (var stream = new ByteBufferStream(buffer, ownsBuffer: false))
                {
                    var encoderFactory = new BinaryMessageEncoderFactory(stream, encoderSettings);
                    replyMessage = (ReplyMessage<RawBsonDocument>)encoderFactory
                        .GetReplyMessageEncoder(RawBsonDocumentSerializer.Instance)
                        .ReadMessage();
                    disposeOfDocuments = true;
                }
            }

            try
            {
                if (replyMessage.CursorNotFound ||
                    replyMessage.QueryFailure ||
                    (state.ExpectedResponseType != ExpectedResponseType.Query && replyMessage.Documents.Count == 0))
                {
                    var queryFailureDocument = replyMessage.QueryFailureDocument;
                    if (__securitySensitiveCommands.Contains(state.CommandName))
                    {
                        queryFailureDocument = new BsonDocument();
                    }
                    if (_failedEvent != null)
                    {
                        _failedEvent(new CommandFailedEvent(
                            state.CommandName,
                            new MongoCommandException(
                                connectionId,
                                string.Format("{0} command failed", state.CommandName),
                                null,
                                queryFailureDocument),
                            state.OperationId,
                            replyMessage.ResponseTo,
                            connectionId,
                            state.Stopwatch.Elapsed));
                    }
                }
                else
                {
                    switch (state.ExpectedResponseType)
                    {
                        case ExpectedResponseType.Command:
                            ProcessCommandReplyMessage(state, replyMessage, connectionId);
                            break;
                        case ExpectedResponseType.GLE:
                            ProcessGLEReplyMessage(state, replyMessage, connectionId);
                            break;
                        case ExpectedResponseType.Query:
                            ProcessQueryReplyMessage(state, replyMessage, connectionId);
                            break;
                    }
                }
            }
            finally
            {
                if (disposeOfDocuments && replyMessage.Documents != null)
                {
                    replyMessage.Documents.ForEach(d => d.Dispose());
                }
            }
        }

        private void ProcessCommandReplyMessage(CommandState state, ReplyMessage<RawBsonDocument> replyMessage, ConnectionId connectionId)
        {
            BsonDocument reply = replyMessage.Documents[0];
            BsonValue ok;
            if (!reply.TryGetValue("ok", out ok))
            {
                // this is a degenerate case with the server and 
                // we don't really know what to do here...
                return;
            }

            if (__securitySensitiveCommands.Contains(state.CommandName))
            {
                reply = new BsonDocument();
            }

            if (!ok.ToBoolean())
            {
                if (_failedEvent != null)
                {
                    _failedEvent(new CommandFailedEvent(
                        state.CommandName,
                        new MongoCommandException(
                            connectionId,
                            string.Format("{0} command failed", state.CommandName),
                            null,
                            reply),
                        state.OperationId,
                        replyMessage.ResponseTo,
                        connectionId,
                        state.Stopwatch.Elapsed));
                }
            }
            else if (_succeededEvent != null)
            {
                _succeededEvent(new CommandSucceededEvent(
                    state.CommandName,
                    reply,
                    state.OperationId,
                    replyMessage.ResponseTo,
                    connectionId,
                    state.Stopwatch.Elapsed));
            }
        }

        private void ProcessGLEReplyMessage(CommandState state, ReplyMessage<RawBsonDocument> replyMessage, ConnectionId connectionId)
        {
            var reply = replyMessage.Documents[0];
            BsonValue ok;
            if (!reply.TryGetValue("ok", out ok))
            {
                // this is a degenerate case with the server and 
                // we don't really know what to do here...
            }
            else if (!ok.ToBoolean())
            {
                if (_failedEvent != null)
                {
                    _failedEvent(new CommandFailedEvent(
                        state.CommandName,
                        new MongoCommandException(
                            connectionId,
                            string.Format("{0} command failed", state.CommandName),
                            null,
                            reply),
                        state.OperationId,
                        replyMessage.ResponseTo,
                        connectionId,
                        state.Stopwatch.Elapsed));
                }
            }
            else if (_succeededEvent != null)
            {
                var fakeReply = new BsonDocument("ok", 1);

                BsonValue n;
                if (reply.TryGetValue("n", out n))
                {
                    fakeReply["n"] = n;
                }

                BsonValue err;
                if (reply.TryGetValue("err", out err) && err != BsonNull.Value)
                {
                    var code = reply.GetValue("code", -1);
                    var errmsg = err.ToString();
                    var isWriteConcernError = __writeConcernIndicators.Any(x => errmsg.Contains(x));
                    if (isWriteConcernError)
                    {
                        fakeReply["writeConcernError"] = new BsonDocument
                        {
                            { "code", code },
                            { "errmsg", err }
                        };
                    }
                    else
                    {
                        fakeReply["writeErrors"] = new BsonArray(new[] { new BsonDocument
                        {
                            { "index", 0 },
                            { "code", code },
                            { "errmsg", err }
                        }});
                    }
                }
                else if (state.CommandName == "insert")
                {
                    fakeReply["n"] = state.NumberOfInsertedDocuments;
                }
                else if (state.CommandName == "update")
                {
                    // Unfortunately v2.4 GLE does not include the upserted field when
                    // the upserted _id is non-OID type.  We can detect this by the
                    // updatedExisting field + an n of 1
                    BsonValue upsertedValue;
                    var upserted = reply.TryGetValue("upserted", out upsertedValue) ||
                        (n == 1 && !reply.GetValue("updatedExisting", false).ToBoolean());

                    if (upserted)
                    {
                        fakeReply["upserted"] = new BsonArray(new[] { new BsonDocument
                        {
                            { "index", 0 },
                            { "_id", upsertedValue ?? state.UpsertedId ?? BsonUndefined.Value }
                        }});
                    }
                }

                _succeededEvent(new CommandSucceededEvent(
                    state.CommandName,
                    fakeReply,
                    state.OperationId,
                    replyMessage.ResponseTo,
                    connectionId,
                    state.Stopwatch.Elapsed));
            }
        }

        private void ProcessQueryReplyMessage(CommandState state, ReplyMessage<RawBsonDocument> replyMessage, ConnectionId connectionId)
        {
            if (_succeededEvent != null)
            {
                BsonDocument reply;
                if (state.CommandName == "explain")
                {
                    reply = new BsonDocument("ok", 1);
                    reply.Merge(replyMessage.Documents[0]);
                }
                else
                {
                    var batchName = state.CommandName == "find" ? "firstBatch" : "nextBatch";
                    reply = new BsonDocument
                    {
                        { "cursor", new BsonDocument
                                    {
                                        { "id", replyMessage.CursorId },
                                        { "ns", state.QueryNamespace.FullName },
                                        { batchName, new BsonArray(replyMessage.Documents) }
                                    }},
                        { "ok", 1 }
                    };
                }

                _succeededEvent(new CommandSucceededEvent(
                    state.CommandName,
                    reply,
                    state.OperationId,
                    replyMessage.ResponseTo,
                    connectionId,
                    state.Stopwatch.Elapsed));
            }
        }

        private void ProcessUpdateMessage(UpdateMessage originalMessage, Queue<RequestMessage> messageQueue, ConnectionId connectionId, UpdateMessageBinaryEncoder encoder, Stopwatch stopwatch)
        {
            var commandName = "update";
            int requestId = originalMessage.RequestId;
            var operationId = EventContext.OperationId;
            var expectedResponseType = ExpectedResponseType.None;
            BsonValue upsertedId = null;
            int gleRequestId;
            WriteConcern writeConcern;
            if (TryGetWriteConcernFromGLE(messageQueue, out gleRequestId, out writeConcern))
            {
                requestId = gleRequestId;
                expectedResponseType = ExpectedResponseType.GLE;
            }

            if (_startedEvent != null)
            {
                var decodedMessage = encoder.ReadMessage(RawBsonDocumentSerializer.Instance);
                try
                {
                    if (_shouldTrackState)
                    {
                        // GLE result on older versions of the server didn't return
                        // the upserted id when it wasn't an object id, so we'll
                        // attempt to get it from the messages.
                        if (!decodedMessage.Update.TryGetValue("_id", out upsertedId))
                        {
                            decodedMessage.Query.TryGetValue("_id", out upsertedId);
                        }
                    }

                    var entry = new BsonDocument
                    {
                        { "q", decodedMessage.Query },
                        { "u", decodedMessage.Update },
                        { "upsert", decodedMessage.IsUpsert },
                        { "multi", decodedMessage.IsMulti }
                    };
                    var command = new BsonDocument
                    {
                        { commandName, decodedMessage.CollectionNamespace.CollectionName },
                        { "updates", new BsonArray(new [] { entry }) }
                    };

                    if (writeConcern == null)
                    {
                        command["writeConcern"] = WriteConcern.Unacknowledged.ToBsonDocument();
                    }
                    else if (!writeConcern.IsServerDefault)
                    {
                        command["writeConcern"] = writeConcern.ToBsonDocument();
                    }

                    var @event = new CommandStartedEvent(
                        commandName,
                        command,
                        decodedMessage.CollectionNamespace.DatabaseNamespace,
                        operationId,
                        requestId,
                        connectionId);

                    _startedEvent(@event);
                }
                finally
                {
                    var disposable = decodedMessage.Query as IDisposable;
                    if (disposable != null)
                    {
                        disposable.Dispose();
                    }
                    disposable = decodedMessage.Update as IDisposable;
                    if (disposable != null)
                    {
                        disposable.Dispose();
                    }
                }
            }

            if (_shouldTrackState)
            {
                _state.TryAdd(requestId, new CommandState
                {
                    CommandName = commandName,
                    OperationId = operationId,
                    Stopwatch = Stopwatch.StartNew(),
                    ExpectedResponseType = expectedResponseType,
                    UpsertedId = upsertedId
                });
            }
        }

        private BsonDocument BuildFindCommandFromQuery(QueryMessage message)
        {
            var batchSize = EventContext.FindOperationBatchSize ?? 0;
            var limit = EventContext.FindOperationLimit ?? 0;
            var command = new BsonDocument
            {
                { "find", message.CollectionNamespace.CollectionName },
                { "projection", message.Fields, message.Fields != null },
                { "skip", message.Skip, message.Skip > 0 },
                { "batchSize", batchSize, batchSize > 0 },
                { "limit", limit, limit != 0 },
                { "awaitData", message.AwaitData, message.AwaitData },
                { "noCursorTimeout", message.NoCursorTimeout, message.NoCursorTimeout },
                { "allowPartialResults", message.PartialOk, message.PartialOk },
                { "tailable", message.TailableCursor, message.TailableCursor },
                { "oplogReplay", message.OplogReplay, message.OplogReplay }
            };

            var query = message.Query;
            if (query.ElementCount == 1 && !query.Contains("$query"))
            {
                command["filter"] = query;
            }
            else
            {
                foreach (var element in query)
                {
                    switch (element.Name)
                    {
                        case "$query":
                            command["filter"] = element.Value;
                            break;
                        case "$orderby":
                            command["sort"] = element.Value;
                            break;
                        case "$showDiskLoc":
                            command["showRecordId"] = element.Value;
                            break;
                        case "$explain":
                            // explain is special and gets handled elsewhere
                            break;
                        default:
                            if (element.Name.StartsWith("$"))
                            {
                                command[element.Name.Substring(1)] = element.Value;
                            }
                            else
                            {
                                // theoretically, this should never happen and is illegal.
                                // however, we'll push this up anyways and a command-failure
                                // event will likely get thrown.
                                command[element.Name] = element.Value;
                            }
                            break;
                    }
                }
            }

            return command;
        }

        private bool TryGetWriteConcernFromGLE(Queue<RequestMessage> messageQueue, out int requestId, out WriteConcern writeConcern)
        {
            requestId = -1;
            writeConcern = null;
            if (messageQueue.Count == 0)
            {
                return false;
            }

            var message = messageQueue.Peek();
            if (message.MessageType != MongoDBMessageType.Query)
            {
                return false;
            }

            var queryMessage = (QueryMessage)message;

            if (!IsCommand(queryMessage.CollectionNamespace))
            {
                return false;
            }

            var query = queryMessage.Query;
            var firstElement = query.GetElement(0);
            if (firstElement.Name != "getLastError")
            {
                return false;
            }

            messageQueue.Dequeue(); // consume it so that we don't process it later... 
            requestId = queryMessage.RequestId;

            writeConcern = WriteConcern.FromBsonDocument(query);
            return true;
        }

        private static bool IsCommand(CollectionNamespace collectionNamespace)
        {
            return collectionNamespace.Equals(collectionNamespace.DatabaseNamespace.CommandCollection);
        }

        private enum ExpectedResponseType
        {
            None,
            GLE,
            Query,
            Command
        }

        private class CommandState
        {
            public string CommandName;
            public long? OperationId;
            public Stopwatch Stopwatch;
            public CollectionNamespace QueryNamespace;
            public int NumberOfInsertedDocuments;
            public ExpectedResponseType ExpectedResponseType;
            public BsonDocument NoResponseResponse;
            public BsonValue UpsertedId;
        }
    }
}

/* Copyright 2013-present MongoDB Inc.
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
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson.IO;
using MongoDB.Driver.Core.Async;
using MongoDB.Driver.Core.Configuration;
using MongoDB.Driver.Core.Events;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.Servers;
using MongoDB.Driver.Core.WireProtocol.Messages;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders.BinaryEncoders;

namespace MongoDB.Driver.Core.Connections
{
    /// <summary>
    /// Represents a connection using the binary wire protocol over a binary stream.
    /// </summary>
    internal class BinaryConnection : IConnection
    {
        // fields
        private readonly CancellationToken _backgroundTaskCancellationToken;
        private readonly CancellationTokenSource _backgroundTaskCancellationTokenSource;
        private readonly CommandEventHelper _commandEventHelper;
        private ConnectionId _connectionId;
        private readonly IConnectionInitializer _connectionInitializer;
        private EndPoint _endPoint;
        private ConnectionDescription _description;
        private readonly Dropbox _dropbox = new Dropbox();
        private bool _failedEventHasBeenRaised;
        private DateTime _lastUsedAtUtc;
        private DateTime _openedAtUtc;
        private readonly object _openLock = new object();
        private Task _openTask;
        private readonly SemaphoreSlim _receiveLock;
        private readonly SemaphoreSlim _sendLock;
        private readonly ConnectionSettings _settings;
        private readonly InterlockedInt32 _state;
        private Stream _stream;
        private readonly IStreamFactory _streamFactory;

        private readonly Action<ConnectionFailedEvent> _failedEventHandler;
        private readonly Action<ConnectionClosingEvent> _closingEventHandler;
        private readonly Action<ConnectionClosedEvent> _closedEventHandler;
        private readonly Action<ConnectionOpeningEvent> _openingEventHandler;
        private readonly Action<ConnectionOpenedEvent> _openedEventHandler;
        private readonly Action<ConnectionOpeningFailedEvent> _failedOpeningEventHandler;
        private readonly Action<ConnectionReceivingMessageEvent> _receivingMessageEventHandler;
        private readonly Action<ConnectionReceivedMessageEvent> _receivedMessageEventHandler;
        private readonly Action<ConnectionReceivingMessageFailedEvent> _failedReceivingMessageEventHandler;
        private readonly Action<ConnectionSendingMessagesEvent> _sendingMessagesEventHandler;
        private readonly Action<ConnectionSentMessagesEvent> _sentMessagesEventHandler;
        private readonly Action<ConnectionSendingMessagesFailedEvent> _failedSendingMessagesEvent;

        // constructors
        public BinaryConnection(ServerId serverId, EndPoint endPoint, ConnectionSettings settings, IStreamFactory streamFactory, IConnectionInitializer connectionInitializer, IEventSubscriber eventSubscriber)
        {
            Ensure.IsNotNull(serverId, nameof(serverId));
            _endPoint = Ensure.IsNotNull(endPoint, nameof(endPoint));
            _settings = Ensure.IsNotNull(settings, nameof(settings));
            _streamFactory = Ensure.IsNotNull(streamFactory, nameof(streamFactory));
            _connectionInitializer = Ensure.IsNotNull(connectionInitializer, nameof(connectionInitializer));
            Ensure.IsNotNull(eventSubscriber, nameof(eventSubscriber));

            _backgroundTaskCancellationTokenSource = new CancellationTokenSource();
            _backgroundTaskCancellationToken = _backgroundTaskCancellationTokenSource.Token;

            _connectionId = new ConnectionId(serverId);
            _receiveLock = new SemaphoreSlim(1);
            _sendLock = new SemaphoreSlim(1);
            _state = new InterlockedInt32(State.Initial);

            _commandEventHelper = new CommandEventHelper(eventSubscriber);
            eventSubscriber.TryGetEventHandler(out _failedEventHandler);
            eventSubscriber.TryGetEventHandler(out _closingEventHandler);
            eventSubscriber.TryGetEventHandler(out _closedEventHandler);
            eventSubscriber.TryGetEventHandler(out _openingEventHandler);
            eventSubscriber.TryGetEventHandler(out _openedEventHandler);
            eventSubscriber.TryGetEventHandler(out _failedOpeningEventHandler);
            eventSubscriber.TryGetEventHandler(out _receivingMessageEventHandler);
            eventSubscriber.TryGetEventHandler(out _receivedMessageEventHandler);
            eventSubscriber.TryGetEventHandler(out _failedReceivingMessageEventHandler);
            eventSubscriber.TryGetEventHandler(out _sendingMessagesEventHandler);
            eventSubscriber.TryGetEventHandler(out _sentMessagesEventHandler);
            eventSubscriber.TryGetEventHandler(out _failedSendingMessagesEvent);
        }

        // properties
        public ConnectionId ConnectionId
        {
            get { return _connectionId; }
        }

        public ConnectionDescription Description
        {
            get { return _description; }
        }

        public EndPoint EndPoint
        {
            get { return _endPoint; }
        }

        public bool IsExpired
        {
            get
            {
                var now = DateTime.UtcNow;

                // connection has been alive for too long
                if (_settings.MaxLifeTime.TotalMilliseconds > -1 && now > _openedAtUtc.Add(_settings.MaxLifeTime))
                {
                    return true;
                }

                // connection has been idle for too long
                if (_settings.MaxIdleTime.TotalMilliseconds > -1 && now > _lastUsedAtUtc.Add(_settings.MaxIdleTime))
                {
                    return true;
                }

                return _state.Value > State.Open;
            }
        }

        public ConnectionSettings Settings
        {
            get { return _settings; }
        }

        // methods
        private void ConnectionFailed(Exception exception)
        {
            if (!_state.TryChange(State.Open, State.Failed) && !_state.TryChange(State.Initializing, State.Failed))
            {
                var currentState = _state.Value;
                if (currentState != State.Failed && currentState != State.Disposed)
                {
                    throw new InvalidOperationException($"Invalid BinaryConnection state transition from {currentState} to Failed.");
                }
            }

            if (!_failedEventHasBeenRaised)
            {
                _failedEventHasBeenRaised = true;
                if (_failedEventHandler != null)
                {
                    _failedEventHandler(new ConnectionFailedEvent(_connectionId, exception));
                }
                _commandEventHelper.ConnectionFailed(_connectionId, exception);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_state.TryChange(State.Disposed))
            {
                if (disposing)
                {
                    if (_closingEventHandler != null)
                    {
                        _closingEventHandler(new ConnectionClosingEvent(_connectionId, EventContext.OperationId));
                    }

                    var stopwatch = Stopwatch.StartNew();
                    _backgroundTaskCancellationTokenSource.Cancel();
                    _backgroundTaskCancellationTokenSource.Dispose();
                    _receiveLock.Dispose();
                    _sendLock.Dispose();

                    if (_stream != null)
                    {
                        try
                        {
                            _stream.Dispose();
                        }
                        catch
                        {
                            // eat this...
                        }
                    }

                    stopwatch.Stop();
                    if (_closedEventHandler != null)
                    {
                        _closedEventHandler(new ConnectionClosedEvent(_connectionId, stopwatch.Elapsed, EventContext.OperationId));
                    }
                }
            }
        }

        public void Open(CancellationToken cancellationToken)
        {
            ThrowIfDisposed();

            TaskCompletionSource<bool> taskCompletionSource = null;
            var connecting = false;
            lock (_openLock)
            {
                if (_state.TryChange(State.Initial, State.Connecting))
                {
                    _openedAtUtc = DateTime.UtcNow;
                    taskCompletionSource = new TaskCompletionSource<bool>();
                    _openTask = taskCompletionSource.Task;
                    _openTask.IgnoreExceptions();
                    connecting = true;
                }
            }

            if (connecting)
            {
                try
                {
                    OpenHelper(cancellationToken);
                    taskCompletionSource.TrySetResult(true);
                }
                catch (Exception ex)
                {
                    taskCompletionSource.TrySetException(ex);
                    throw;
                }
            }
            else
            {
                _openTask.GetAwaiter().GetResult();
            }
        }

        public Task OpenAsync(CancellationToken cancellationToken)
        {
            ThrowIfDisposed();

            lock (_openLock)
            {
                if (_state.TryChange(State.Initial, State.Connecting))
                {
                    _openedAtUtc = DateTime.UtcNow;
                    _openTask = OpenHelperAsync(cancellationToken);
                }
                return _openTask;
            }
        }

        private void OpenHelper(CancellationToken cancellationToken)
        {
            var helper = new OpenConnectionHelper(this);
            try
            {
                helper.OpeningConnection();
                _stream = _streamFactory.CreateStream(_endPoint, cancellationToken);
                helper.InitializingConnection();
                _description = _connectionInitializer.InitializeConnection(this, cancellationToken);
                helper.OpenedConnection();
            }
            catch (Exception ex)
            {
                var wrappedException = WrapException(ex, "opening a connection to the server");
                helper.FailedOpeningConnection(wrappedException);
                throw wrappedException;
            }
        }

        private async Task OpenHelperAsync(CancellationToken cancellationToken)
        {
            var helper = new OpenConnectionHelper(this);
            try
            {
                helper.OpeningConnection();
                _stream = await _streamFactory.CreateStreamAsync(_endPoint, cancellationToken).ConfigureAwait(false);
                helper.InitializingConnection();
                _description = await _connectionInitializer.InitializeConnectionAsync(this, cancellationToken).ConfigureAwait(false);
                helper.OpenedConnection();
            }
            catch (Exception ex)
            {
                var wrappedException = WrapException(ex, "opening a connection to the server");
                helper.FailedOpeningConnection(wrappedException);
                throw wrappedException;
            }
        }

        private IByteBuffer ReceiveBuffer()
        {
            try
            {
                var messageSizeBytes = new byte[4];
                _stream.ReadBytes(messageSizeBytes, 0, 4, _backgroundTaskCancellationToken);
                var messageSize = BitConverter.ToInt32(messageSizeBytes, 0);
                var inputBufferChunkSource = new InputBufferChunkSource(BsonChunkPool.Default);
                var buffer = ByteBufferFactory.Create(inputBufferChunkSource, messageSize);
                buffer.Length = messageSize;
                buffer.SetBytes(0, messageSizeBytes, 0, 4);
                _stream.ReadBytes(buffer, 4, messageSize - 4, _backgroundTaskCancellationToken);
                _lastUsedAtUtc = DateTime.UtcNow;
                buffer.MakeReadOnly();
                return buffer;
            }
            catch (Exception ex)
            {
                var wrappedException = WrapException(ex, "receiving a message from the server");
                ConnectionFailed(wrappedException);
                throw wrappedException;
            }
        }

        private IByteBuffer ReceiveBuffer(int responseTo, CancellationToken cancellationToken)
        {
            using (var receiveLockRequest = new SemaphoreSlimRequest(_receiveLock, cancellationToken))
            {
                var messageTask = _dropbox.GetMessageAsync(responseTo);
                try
                {
                    Task.WaitAny(messageTask, receiveLockRequest.Task);
                    if (messageTask.IsCompleted)
                    {
                        return _dropbox.RemoveMessage(responseTo);
                    }

                    receiveLockRequest.Task.GetAwaiter().GetResult(); // propagate exceptions
                    while (true)
                    {
                        var buffer = ReceiveBuffer();
                        _dropbox.AddMessage(buffer);

                        if (messageTask.IsCompleted)
                        {
                            return _dropbox.RemoveMessage(responseTo);
                        }

                        cancellationToken.ThrowIfCancellationRequested();
                    }
                }
                catch
                {
                    var ignored = messageTask.ContinueWith(
                        t => { _dropbox.RemoveMessage(responseTo).Dispose(); },
                        TaskContinuationOptions.OnlyOnRanToCompletion);
                    throw;
                }
            }
        }

        private async Task<IByteBuffer> ReceiveBufferAsync()
        {
            try
            {
                var messageSizeBytes = new byte[4];
                await _stream.ReadBytesAsync(messageSizeBytes, 0, 4, _backgroundTaskCancellationToken).ConfigureAwait(false);
                var messageSize = BitConverter.ToInt32(messageSizeBytes, 0);
                var inputBufferChunkSource = new InputBufferChunkSource(BsonChunkPool.Default);
                var buffer = ByteBufferFactory.Create(inputBufferChunkSource, messageSize);
                buffer.Length = messageSize;
                buffer.SetBytes(0, messageSizeBytes, 0, 4);
                await _stream.ReadBytesAsync(buffer, 4, messageSize - 4, _backgroundTaskCancellationToken).ConfigureAwait(false);
                _lastUsedAtUtc = DateTime.UtcNow;
                buffer.MakeReadOnly();
                return buffer;
            }
            catch (Exception ex)
            {
                var wrappedException = WrapException(ex, "receiving a message from the server");
                ConnectionFailed(wrappedException);
                throw wrappedException;
            }
        }

        private async Task<IByteBuffer> ReceiveBufferAsync(int responseTo, CancellationToken cancellationToken)
        {
            using (var receiveLockRequest = new SemaphoreSlimRequest(_receiveLock, cancellationToken))
            {
                var messageTask = _dropbox.GetMessageAsync(responseTo);
                try
                {
                    await Task.WhenAny(messageTask, receiveLockRequest.Task).ConfigureAwait(false);
                    if (messageTask.IsCompleted)
                    {
                        return _dropbox.RemoveMessage(responseTo);
                    }

                    receiveLockRequest.Task.GetAwaiter().GetResult(); // propagate exceptions
                    while (true)
                    {
                        var buffer = await ReceiveBufferAsync().ConfigureAwait(false);
                        _dropbox.AddMessage(buffer);

                        if (messageTask.IsCompleted)
                        {
                            return _dropbox.RemoveMessage(responseTo);
                        }

                        cancellationToken.ThrowIfCancellationRequested();
                    }
                }
                catch
                {
                    var ignored = messageTask.ContinueWith(
                        t => { _dropbox.RemoveMessage(responseTo).Dispose(); },
                        TaskContinuationOptions.OnlyOnRanToCompletion);
                    throw;
                }
            }
        }

        public ResponseMessage ReceiveMessage(
            int responseTo,
            IMessageEncoderSelector encoderSelector,
            MessageEncoderSettings messageEncoderSettings,
            CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(encoderSelector, nameof(encoderSelector));
            ThrowIfDisposedOrNotOpen();

            var helper = new ReceiveMessageHelper(this, responseTo, messageEncoderSettings);
            try
            {
                helper.ReceivingMessage();
                using (var buffer = ReceiveBuffer(responseTo, cancellationToken))
                {
                    var message = helper.DecodeMessage(buffer, encoderSelector, cancellationToken);
                    helper.ReceivedMessage(buffer, message);
                    return message;
                }
            }
            catch (Exception ex)
            {
                helper.FailedReceivingMessage(ex);
                throw;
            }
        }

        public async Task<ResponseMessage> ReceiveMessageAsync(
            int responseTo,
            IMessageEncoderSelector encoderSelector,
            MessageEncoderSettings messageEncoderSettings,
            CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(encoderSelector, nameof(encoderSelector));
            ThrowIfDisposedOrNotOpen();

            var helper = new ReceiveMessageHelper(this, responseTo, messageEncoderSettings);
            try
            {
                helper.ReceivingMessage();
                using (var buffer = await ReceiveBufferAsync(responseTo, cancellationToken).ConfigureAwait(false))
                {
                    var message = helper.DecodeMessage(buffer, encoderSelector, cancellationToken);
                    helper.ReceivedMessage(buffer, message);
                    return message;
                }
            }
            catch (Exception ex)
            {
                helper.FailedReceivingMessage(ex);
                throw;
            }
        }

        private void SendBuffer(IByteBuffer buffer, CancellationToken cancellationToken)
        {
            _sendLock.Wait(cancellationToken);
            try
            {
                if (_state.Value == State.Failed)
                {
                    throw new MongoConnectionClosedException(_connectionId);
                }

                try
                {
                    // don't use the caller's cancellationToken because once we start writing a message we have to write the whole thing
                    _stream.WriteBytes(buffer, 0, buffer.Length, _backgroundTaskCancellationToken);
                    _lastUsedAtUtc = DateTime.UtcNow;
                }
                catch (Exception ex)
                {
                    var wrappedException = WrapException(ex, "sending a message to the server");
                    ConnectionFailed(wrappedException);
                    throw wrappedException;
                }
            }
            finally
            {
                _sendLock.Release();
            }
        }

        private async Task SendBufferAsync(IByteBuffer buffer, CancellationToken cancellationToken)
        {
            await _sendLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (_state.Value == State.Failed)
                {
                    throw new MongoConnectionClosedException(_connectionId);
                }

                try
                {
                    // don't use the caller's cancellationToken because once we start writing a message we have to write the whole thing
                    await _stream.WriteBytesAsync(buffer, 0, buffer.Length, _backgroundTaskCancellationToken).ConfigureAwait(false);
                    _lastUsedAtUtc = DateTime.UtcNow;
                }
                catch (Exception ex)
                {
                    var wrappedException = WrapException(ex, "sending a message to the server");
                    ConnectionFailed(wrappedException);
                    throw wrappedException;
                }
            }
            finally
            {
                _sendLock.Release();
            }
        }

        public void SendMessages(IEnumerable<RequestMessage> messages, MessageEncoderSettings messageEncoderSettings, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(messages, nameof(messages));
            ThrowIfDisposedOrNotOpen();

            var helper = new SendMessagesHelper(this, messages, messageEncoderSettings);
            try
            {
                helper.EncodingMessages();
                using (var buffer = helper.EncodeMessages(cancellationToken))
                {
                    helper.SendingMessages(buffer);
                    SendBuffer(buffer, cancellationToken);
                    helper.SentMessages(buffer.Length);
                }
            }
            catch (Exception ex)
            {
                helper.FailedSendingMessages(ex);
                throw;
            }
        }

        public async Task SendMessagesAsync(IEnumerable<RequestMessage> messages, MessageEncoderSettings messageEncoderSettings, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(messages, nameof(messages));
            ThrowIfDisposedOrNotOpen();

            var helper = new SendMessagesHelper(this, messages, messageEncoderSettings);
            try
            {
                helper.EncodingMessages();
                using (var buffer = helper.EncodeMessages(cancellationToken))
                {
                    helper.SendingMessages(buffer);
                    await SendBufferAsync(buffer, cancellationToken).ConfigureAwait(false);
                    helper.SentMessages(buffer.Length);
                }
            }
            catch (Exception ex)
            {
                helper.FailedSendingMessages(ex);
                throw;
            }
        }

        // private methods
        private void ThrowIfDisposed()
        {
            if (_state.Value == State.Disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        private void ThrowIfDisposedOrNotOpen()
        {
            ThrowIfDisposed();
            if (_state.Value == State.Failed)
            {
                throw new MongoConnectionClosedException(_connectionId);
            }
            if (_state.Value != State.Open && _state.Value != State.Initializing)
            {
                throw new InvalidOperationException("The connection must be opened before it can be used.");
            }
        }

        private Exception WrapException(Exception ex, string action)
        {
            if (
#if NET452
                ex is ThreadAbortException ||
                ex is StackOverflowException ||
#endif
                ex is MongoAuthenticationException ||
                ex is OutOfMemoryException)
            {
                return ex;
            }
            else
            {
                var message = string.Format("An exception occurred while {0}.", action);
                return new MongoConnectionException(_connectionId, message, ex);
            }
        }

        // nested classes
        private class Dropbox
        {
            private readonly ConcurrentDictionary<int, TaskCompletionSource<IByteBuffer>> _messages = new ConcurrentDictionary<int, TaskCompletionSource<IByteBuffer>>();

            // public methods
            public void AddMessage(IByteBuffer message)
            {
                var responseTo = GetResponseTo(message);
                var tcs = _messages.GetOrAdd(responseTo, x => new TaskCompletionSource<IByteBuffer>());
                tcs.TrySetResult(message);
            }

            public Task<IByteBuffer> GetMessageAsync(int responseTo)
            {
                var tcs = _messages.GetOrAdd(responseTo, _ => new TaskCompletionSource<IByteBuffer>());
                return tcs.Task;
            }

            public IByteBuffer RemoveMessage(int responseTo)
            {
                TaskCompletionSource<IByteBuffer> tcs;
                _messages.TryRemove(responseTo, out tcs);
                return tcs.Task.GetAwaiter().GetResult(); // RemoveMessage is only called when Task is complete
            }

            // private methods
            private int GetResponseTo(IByteBuffer message)
            {
                var backingBytes = message.AccessBackingBytes(8);
                return BitConverter.ToInt32(backingBytes.Array, backingBytes.Offset);
            }
        }

        private class OpenConnectionHelper
        {
            private readonly BinaryConnection _connection;
            private Stopwatch _stopwatch;

            public OpenConnectionHelper(BinaryConnection connection)
            {
                _connection = connection;
            }

            public void FailedOpeningConnection(Exception wrappedException)
            {
                if (!_connection._state.TryChange(State.Connecting, State.Failed) && !_connection._state.TryChange(State.Initializing, State.Failed))
                {
                    var currentState = _connection._state.Value;
                    if (currentState != State.Failed && currentState != State.Disposed)
                    {
                        throw new InvalidOperationException($"Invalid BinaryConnection state transition from {currentState} to Failed.");
                    }
                }

                var handler = _connection._failedOpeningEventHandler;
                if (handler != null)
                {
                    handler(new ConnectionOpeningFailedEvent(_connection.ConnectionId, _connection._settings, wrappedException, EventContext.OperationId));
                }
            }

            public void InitializingConnection()
            {
                if (!_connection._state.TryChange(State.Connecting, State.Initializing))
                {
                    var currentState = _connection._state.Value;
                    if (currentState == State.Disposed)
                    {
                        throw new ObjectDisposedException(typeof(BinaryConnection).Name);
                    }
                    else
                    {
                        throw new InvalidOperationException($"Invalid BinaryConnection state transition from {currentState} to Initializing.");
                    }
                }
            }

            public void OpenedConnection()
            {
                _stopwatch.Stop();
                _connection._connectionId = _connection._description.ConnectionId;

                if (!_connection._state.TryChange(State.Initializing, State.Open))
                {
                    var currentState = _connection._state.Value;
                    if (currentState == State.Disposed)
                    {
                        throw new ObjectDisposedException(typeof(BinaryConnection).Name);
                    }
                    else
                    {
                        throw new InvalidOperationException($"Invalid BinaryConnection state transition from {currentState} to Open.");
                    }
                }

                var handler = _connection._openedEventHandler;
                if (handler != null)
                {
                    handler(new ConnectionOpenedEvent(_connection.ConnectionId, _connection._settings, _stopwatch.Elapsed, EventContext.OperationId));
                }
            }

            public void OpeningConnection()
            {
                var handler = _connection._openingEventHandler;
                if (handler != null)
                {
                    handler(new ConnectionOpeningEvent(_connection.ConnectionId, _connection._settings, EventContext.OperationId));
                }

                _stopwatch = Stopwatch.StartNew();
            }
        }

        private class ReceiveMessageHelper
        {
            private readonly BinaryConnection _connection;
            private TimeSpan _deserializationDuration;
            private readonly MessageEncoderSettings _messageEncoderSettings;
            private TimeSpan _networkDuration;
            private int _responseTo;
            private Stopwatch _stopwatch;

            public ReceiveMessageHelper(BinaryConnection connection, int responseTo, MessageEncoderSettings messageEncoderSettings)
            {
                _connection = connection;
                _responseTo = responseTo;
                _messageEncoderSettings = messageEncoderSettings;
            }

            public ResponseMessage DecodeMessage(IByteBuffer buffer, IMessageEncoderSelector encoderSelector, CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();

                _stopwatch.Stop();
                _networkDuration = _stopwatch.Elapsed;

                ResponseMessage message;
                _stopwatch.Restart();
                using (var stream = new ByteBufferStream(buffer, ownsBuffer: false))
                {
                    var encoderFactory = new BinaryMessageEncoderFactory(stream, _messageEncoderSettings);
                    var encoder = encoderSelector.GetEncoder(encoderFactory);
                    message = (ResponseMessage)encoder.ReadMessage();
                }
                _stopwatch.Stop();
                _deserializationDuration = _stopwatch.Elapsed;

                return message;
            }

            public void FailedReceivingMessage(Exception exception)
            {
                if (_connection._commandEventHelper.ShouldCallErrorReceiving)
                {
                    _connection._commandEventHelper.ErrorReceiving(_responseTo, _connection._connectionId, exception);
                }

                var handler = _connection._failedReceivingMessageEventHandler;
                if (handler != null)
                {
                    handler(new ConnectionReceivingMessageFailedEvent(_connection.ConnectionId, _responseTo, exception, EventContext.OperationId));
                }
            }

            public void ReceivedMessage(IByteBuffer buffer, ResponseMessage message)
            {
                if (_connection._commandEventHelper.ShouldCallAfterReceiving)
                {
                    _connection._commandEventHelper.AfterReceiving(message, buffer, _connection._connectionId, _messageEncoderSettings);
                }

                var handler = _connection._receivedMessageEventHandler;
                if (handler != null)
                {
                    handler(new ConnectionReceivedMessageEvent(_connection.ConnectionId, _responseTo, buffer.Length, _networkDuration, _deserializationDuration, EventContext.OperationId));
                }
            }

            public void ReceivingMessage()
            {
                var handler = _connection._receivingMessageEventHandler;
                if (handler != null)
                {
                    handler(new ConnectionReceivingMessageEvent(_connection.ConnectionId, _responseTo, EventContext.OperationId));
                }

                _stopwatch = Stopwatch.StartNew();
            }
        }

        private class SendMessagesHelper
        {
            private readonly Stopwatch _commandStopwatch;
            private readonly BinaryConnection _connection;
            private readonly MessageEncoderSettings _messageEncoderSettings;
            private readonly List<RequestMessage> _messages;
            private Lazy<List<int>> _requestIds;
            private TimeSpan _serializationDuration;
            private Stopwatch _networkStopwatch;

            public SendMessagesHelper(BinaryConnection connection, IEnumerable<RequestMessage> messages, MessageEncoderSettings messageEncoderSettings)
            {
                _connection = connection;
                _messages = messages.ToList();
                _messageEncoderSettings = messageEncoderSettings;

                _commandStopwatch = Stopwatch.StartNew();
                _requestIds = new Lazy<List<int>>(() => _messages.Select(m => m.RequestId).ToList());
            }

            public IByteBuffer EncodeMessages(CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var serializationStopwatch = Stopwatch.StartNew();
                var outputBufferChunkSource = new OutputBufferChunkSource(BsonChunkPool.Default);
                var buffer = new MultiChunkBuffer(outputBufferChunkSource);
                using (var stream = new ByteBufferStream(buffer, ownsBuffer: false))
                {
                    var encoderFactory = new BinaryMessageEncoderFactory(stream, _messageEncoderSettings);
                    foreach (var message in _messages)
                    {
                        if (message.ShouldBeSent == null || message.ShouldBeSent())
                        {
                            var encoder = message.GetEncoder(encoderFactory);
                            encoder.WriteMessage(message);
                            message.WasSent = true;
                        }

                        // Encoding messages includes serializing the
                        // documents, so encoding message could be expensive
                        // and worthy of us honoring cancellation here.
                        cancellationToken.ThrowIfCancellationRequested();
                    }
                    buffer.Length = (int)stream.Length;
                    buffer.MakeReadOnly();
                }
                serializationStopwatch.Stop();
                _serializationDuration = serializationStopwatch.Elapsed;

                return buffer;
            }

            public void EncodingMessages()
            {
                var handler = _connection._sendingMessagesEventHandler;
                if (handler != null)
                {
                    handler(new ConnectionSendingMessagesEvent(_connection.ConnectionId, _requestIds.Value, EventContext.OperationId));
                }
            }

            public void FailedSendingMessages(Exception ex)
            {
                if (_connection._commandEventHelper.ShouldCallErrorSending)
                {
                    _connection._commandEventHelper.ErrorSending(_messages, _connection._connectionId, ex);
                }

                var handler = _connection._failedSendingMessagesEvent;
                if (handler != null)
                {
                    handler(new ConnectionSendingMessagesFailedEvent(_connection.ConnectionId, _requestIds.Value, ex, EventContext.OperationId));
                }
            }

            public void SendingMessages(IByteBuffer buffer)
            {
                if (_connection._commandEventHelper.ShouldCallBeforeSending)
                {
                    _connection._commandEventHelper.BeforeSending(_messages, _connection.ConnectionId, buffer, _messageEncoderSettings, _commandStopwatch);
                }

                _networkStopwatch = Stopwatch.StartNew();
            }

            public void SentMessages(int bufferLength)
            {
                _networkStopwatch.Stop();
                var networkDuration = _networkStopwatch.Elapsed;

                if (_connection._commandEventHelper.ShouldCallAfterSending)
                {
                    _connection._commandEventHelper.AfterSending(_messages, _connection._connectionId);
                }

                var handler = _connection._sentMessagesEventHandler;
                if (handler != null)
                {
                    handler(new ConnectionSentMessagesEvent(_connection.ConnectionId, _requestIds.Value, bufferLength, networkDuration, _serializationDuration, EventContext.OperationId));
                }
            }
        }

        private static class State
        {
            // note: the numeric values matter because sometimes we compare their magnitudes
            public static int Initial = 0;
            public static int Connecting = 1;
            public static int Initializing = 2;
            public static int Open = 3;
            public static int Failed = 4;
            public static int Disposed = 5;
        }
    }
}

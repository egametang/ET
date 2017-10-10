/* Copyright 2015 MongoDB Inc.
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
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.WireProtocol;

namespace MongoDB.Driver.Core.Authentication
{
    /// <summary>
    /// Base class for a SASL authenticator.
    /// </summary>
    public abstract class SaslAuthenticator : IAuthenticator
    {
        // fields
        private readonly ISaslMechanism _mechanism;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="SaslAuthenticator"/> class.
        /// </summary>
        /// <param name="mechanism">The mechanism.</param>
        protected SaslAuthenticator(ISaslMechanism mechanism)
        {
            _mechanism = Ensure.IsNotNull(mechanism, nameof(mechanism));
        }

        // properties
        /// <inheritdoc/>
        public string Name
        {
            get { return _mechanism.Name; }
        }

        /// <summary>
        /// Gets the name of the database.
        /// </summary>
        /// <value>
        /// The name of the database.
        /// </value>
        public abstract string DatabaseName { get; }

        // methods
        /// <inheritdoc/>
        public void Authenticate(IConnection connection, ConnectionDescription description, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(connection, nameof(connection));
            Ensure.IsNotNull(description, nameof(description));

            using (var conversation = new SaslConversation(description.ConnectionId))
            {
                var currentStep = _mechanism.Initialize(connection, description);

                var command = CreateStartCommand(currentStep);
                while (true)
                {
                    BsonDocument result;
                    try
                    {
                        var protocol = CreateCommandProtocol(command);
                        result = protocol.Execute(connection, cancellationToken);
                    }
                    catch (MongoCommandException ex)
                    {
                        throw CreateException(connection, ex);
                    }

                    currentStep = Transition(conversation, currentStep, result);
                    if (currentStep == null)
                    {
                        return;
                    }

                    command = CreateContinueCommand(currentStep, result);
                }
            }
        }

        /// <inheritdoc/>
        public async Task AuthenticateAsync(IConnection connection, ConnectionDescription description, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(connection, nameof(connection));
            Ensure.IsNotNull(description, nameof(description));

            using (var conversation = new SaslConversation(description.ConnectionId))
            {
                var currentStep = _mechanism.Initialize(connection, description);

                var command = CreateStartCommand(currentStep);
                while (true)
                {
                    BsonDocument result;
                    try
                    {
                        var protocol = CreateCommandProtocol(command);
                        result = await protocol.ExecuteAsync(connection, cancellationToken).ConfigureAwait(false);
                    }
                    catch (MongoCommandException ex)
                    {
                        throw CreateException(connection, ex);
                    }

                    currentStep = Transition(conversation, currentStep, result);
                    if (currentStep == null)
                    {
                        return;
                    }

                    command = CreateContinueCommand(currentStep, result);
                }
            }
        }

        private CommandWireProtocol<BsonDocument> CreateCommandProtocol(BsonDocument command)
        {
            return new CommandWireProtocol<BsonDocument>(
                new DatabaseNamespace(DatabaseName),
                command,
                true,
                BsonDocumentSerializer.Instance,
                null);
        }

        private BsonDocument CreateContinueCommand(ISaslStep currentStep, BsonDocument result)
        {
            return new BsonDocument
            {
                { "saslContinue", 1 },
                { "conversationId", result["conversationId"].AsInt32 },
                { "payload", currentStep.BytesToSendToServer }
            };
        }

        private MongoAuthenticationException CreateException(IConnection connection, Exception ex)
        {
            var message = string.Format("Unable to authenticate using sasl protocol mechanism {0}.", Name);
            return new MongoAuthenticationException(connection.ConnectionId, message, ex);
        }

        private BsonDocument CreateStartCommand(ISaslStep currentStep)
        {
            return new BsonDocument
            {
                { "saslStart", 1 },
                { "mechanism", _mechanism.Name },
                { "payload", currentStep.BytesToSendToServer }
            };
        }

        private ISaslStep Transition(SaslConversation conversation, ISaslStep currentStep, BsonDocument result)
        {
            // we might be done here if the client is not expecting a reply from the server
            if (result.GetValue("done", false).ToBoolean() && currentStep.IsComplete)
            {
                return null;
            }

            currentStep = currentStep.Transition(conversation, result["payload"].AsByteArray);

            // we might be done here if the client had some final verification it needed to do
            if (result.GetValue("done", false).ToBoolean() && currentStep.IsComplete)
            {
                return null;
            }

            return currentStep;
        }

        // nested classes
        /// <summary>
        /// Represents a SASL conversation.
        /// </summary>
        protected sealed class SaslConversation : IDisposable
        {
            // fields
            private readonly ConnectionId _connectionId;
            private List<IDisposable> _itemsNeedingDisposal;
            private bool _isDisposed;

            // constructors
            /// <summary>
            /// Initializes a new instance of the <see cref="SaslConversation"/> class.
            /// </summary>
            /// <param name="connectionId">The connection identifier.</param>
            public SaslConversation(ConnectionId connectionId)
            {
                _connectionId = connectionId;
                _itemsNeedingDisposal = new List<IDisposable>();
            }

            // properties
            /// <summary>
            /// Gets the connection identifier.
            /// </summary>
            /// <value>
            /// The connection identifier.
            /// </value>
            public ConnectionId ConnectionId
            {
                get { return _connectionId; }
            }

            // methods
            /// <inheritdoc/>
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            /// <summary>
            /// Registers the item for disposal.
            /// </summary>
            /// <param name="item">The disposable item.</param>
            public void RegisterItemForDisposal(IDisposable item)
            {
                _itemsNeedingDisposal.Add(item);
            }

            private void Dispose(bool disposing)
            {
                if (!_isDisposed)
                {
                    // disposal should happen in reverse order of registration.
                    if (disposing && _itemsNeedingDisposal != null)
                    {
                        for (int i = _itemsNeedingDisposal.Count - 1; i >= 0; i--)
                        {
                            _itemsNeedingDisposal[i].Dispose();
                        }

                        _itemsNeedingDisposal.Clear();
                        _itemsNeedingDisposal = null;
                    }

                    _isDisposed = true;
                }
            }
        }

        /// <summary>
        /// Represents a SASL mechanism.
        /// </summary>
        protected interface ISaslMechanism
        {
            // properties
            /// <summary>
            /// Gets the name of the mechanism.
            /// </summary>
            /// <value>
            /// The name.
            /// </value>
            string Name { get; }

            // methods
            /// <summary>
            /// Initializes the mechanism.
            /// </summary>
            /// <param name="connection">The connection.</param>
            /// <param name="description">The connection description.</param>
            /// <returns>The initial SASL step.</returns>
            ISaslStep Initialize(IConnection connection, ConnectionDescription description);
        }

        /// <summary>
        /// Represents a SASL step.
        /// </summary>
        protected interface ISaslStep
        {
            // properties
            /// <summary>
            /// Gets the bytes to send to server.
            /// </summary>
            /// <value>
            /// The bytes to send to server.
            /// </value>
            byte[] BytesToSendToServer { get; }

            /// <summary>
            /// Gets a value indicating whether this instance is complete.
            /// </summary>
            /// <value>
            /// <c>true</c> if this instance is complete; otherwise, <c>false</c>.
            /// </value>
            bool IsComplete { get; }

            // methods
            /// <summary>
            /// Transitions the SASL conversation to the next step.
            /// </summary>
            /// <param name="conversation">The SASL conversation.</param>
            /// <param name="bytesReceivedFromServer">The bytes received from server.</param>
            /// <returns>The next SASL step.</returns>
            ISaslStep Transition(SaslConversation conversation, byte[] bytesReceivedFromServer);
        }

        /// <summary>
        /// Represents a completed SASL step.
        /// </summary>
        protected class CompletedStep : ISaslStep
        {
            // fields
            private readonly byte[] _bytesToSendToServer;

            // constructors
            /// <summary>
            /// Initializes a new instance of the <see cref="CompletedStep"/> class.
            /// </summary>
            public CompletedStep()
                : this(new byte[0])
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="CompletedStep"/> class.
            /// </summary>
            /// <param name="bytesToSendToServer">The bytes to send to server.</param>
            public CompletedStep(byte[] bytesToSendToServer)
            {
                _bytesToSendToServer = bytesToSendToServer;
            }

            // properties
            /// <inheritdoc/>
            public byte[] BytesToSendToServer
            {
                get { return _bytesToSendToServer; }
            }

            /// <inheritdoc/>
            public bool IsComplete
            {
                get { return true; }
            }

            /// <inheritdoc/>
            public ISaslStep Transition(SaslConversation conversation, byte[] bytesReceivedFromServer)
            {
                throw new InvalidOperationException("Sasl conversation has completed.");
            }
        }
    }
}
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
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.WireProtocol;

namespace MongoDB.Driver.Core.Authentication
{
    /// <summary>
    /// A MongoDB-X509 authenticator.
    /// </summary>
    public sealed class MongoDBX509Authenticator : IAuthenticator
    {
        // static properties
        /// <summary>
        /// Gets the name of the mechanism.
        /// </summary>
        /// <value>
        /// The name of the mechanism.
        /// </value>
        public static string MechanismName
        {
            get { return "MONGODB-X509"; }
        }

        // fields
        private readonly string _username;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="MongoDBX509Authenticator"/> class.
        /// </summary>
        /// <param name="username">The username.</param>
        public MongoDBX509Authenticator(string username)
        {
            _username = Ensure.IsNullOrNotEmpty(username, nameof(username));
        }

        // properties
        /// <inheritdoc/>
        public string Name
        {
            get { return MechanismName; }
        }

        // public methods
        /// <inheritdoc/>
        public void Authenticate(IConnection connection, ConnectionDescription description, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(connection, nameof(connection));
            Ensure.IsNotNull(description, nameof(description));
            EnsureUsernameIsNotNullOrNullIsSupported(connection, description);

            try
            {
                var protocol = CreateAuthenticateProtocol();
                protocol.Execute(connection, cancellationToken);
            }
            catch (MongoCommandException ex)
            {
                throw CreateException(connection, ex);
            }
        }

        /// <inheritdoc/>
        public async Task AuthenticateAsync(IConnection connection, ConnectionDescription description, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(connection, nameof(connection));
            Ensure.IsNotNull(description, nameof(description));
            EnsureUsernameIsNotNullOrNullIsSupported(connection, description);

            try
            {
                var protocol = CreateAuthenticateProtocol();
                await protocol.ExecuteAsync(connection, cancellationToken).ConfigureAwait(false);
            }
            catch (MongoCommandException ex)
            {
                throw CreateException(connection, ex);
            }
        }

        // private methods
        private CommandWireProtocol<BsonDocument> CreateAuthenticateProtocol()
        {
            var command = new BsonDocument
            {
                { "authenticate", 1 },
                { "mechanism", Name },
                { "user", _username, _username != null }
            };

            var protocol = new CommandWireProtocol<BsonDocument>(
                new DatabaseNamespace("$external"),
                command,
                true,
                BsonDocumentSerializer.Instance,
                null);

            return protocol;
        }

        private MongoAuthenticationException CreateException(IConnection connection, Exception ex)
        {
            var message = string.Format("Unable to authenticate username '{0}' using protocol '{1}'.", _username, Name);
            return new MongoAuthenticationException(connection.ConnectionId, message, ex);
        }

        private void EnsureUsernameIsNotNullOrNullIsSupported(IConnection connection, ConnectionDescription description)
        {
            var serverVersion = description.ServerVersion;
            if (_username == null && !Feature.ServerExtractsUsernameFromX509Certificate.IsSupported(serverVersion))
            {
                var message = $"Username cannot be null for server version {serverVersion}.";
                throw new MongoConnectionException(connection.ConnectionId, message);
            }
        }
    }
}

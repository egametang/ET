/* Copyright 2010-present MongoDB Inc.
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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Core.Authentication
{
    /// <summary>
    /// The default authenticator.
    /// If saslSupportedMechs is not present in the isMaster results for mechanism negotiation
    /// uses SCRAM-SHA-1 when talking to servers >= 3.0. Prior to server 3.0, uses MONGODB-CR.
    /// Else, uses SCRAM-SHA-256 if present in the list of mechanisms. Otherwise, uses 
    /// SCRAM-SHA-1 the default, regardless of whether SCRAM-SHA-1 is in the list.
    /// </summary>
    public class DefaultAuthenticator : IAuthenticator
    {
        // fields
        private readonly UsernamePasswordCredential _credential;
        private readonly IRandomStringGenerator _randomStringGenerator;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultAuthenticator"/> class.
        /// </summary>
        /// <param name="credential">The credential.</param>
        public DefaultAuthenticator(UsernamePasswordCredential credential)
            : this(credential, new DefaultRandomStringGenerator())
        {
        }

        internal DefaultAuthenticator(UsernamePasswordCredential credential, IRandomStringGenerator randomStringGenerator)
        {
            _credential = Ensure.IsNotNull(credential, nameof(credential));
            _randomStringGenerator = Ensure.IsNotNull(randomStringGenerator, nameof(randomStringGenerator));
        }

        // properties
        /// <inheritdoc/>
        public string Name => "DEFAULT";

        // methods
        /// <inheritdoc/>
        public void Authenticate(IConnection connection, ConnectionDescription description, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(connection, nameof(connection));
            Ensure.IsNotNull(description, nameof(description));

            // If we don't have SaslSupportedMechs as part of the response, that means we didn't piggyback the initial
            // isMaster request and should query the server (provided that the server >= 4.0), merging results into 
            // a new ConnectionDescription
            if (!description.IsMasterResult.HasSaslSupportedMechs 
                && Feature.ScramSha256Authentication.IsSupported(description.ServerVersion))
            {
                var command = CustomizeInitialIsMasterCommand(IsMasterHelper.CreateCommand());
                var isMasterProtocol = IsMasterHelper.CreateProtocol(command);
                var isMasterResult = IsMasterHelper.GetResult(connection, isMasterProtocol, cancellationToken);
                var mergedIsMasterResult = new IsMasterResult(description.IsMasterResult.Wrapped.Merge(isMasterResult.Wrapped));
                description = new ConnectionDescription(
                    description.ConnectionId, 
                    mergedIsMasterResult, 
                    description.BuildInfoResult);
            }

            var authenticator = CreateAuthenticator(connection, description);
            authenticator.Authenticate(connection, description, cancellationToken);
            
        }

        /// <inheritdoc/>
        public async Task AuthenticateAsync(IConnection connection, ConnectionDescription description, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(connection, nameof(connection));
            Ensure.IsNotNull(description, nameof(description));
            
            // If we don't have SaslSupportedMechs as part of the response, that means we didn't piggyback the initial
            // isMaster request and should query the server (provided that the server >= 4.0), merging results into 
            // a new ConnectionDescription
            if (!description.IsMasterResult.HasSaslSupportedMechs 
                && Feature.ScramSha256Authentication.IsSupported(description.ServerVersion))
            {
                var command = CustomizeInitialIsMasterCommand(IsMasterHelper.CreateCommand());
                var isMasterProtocol = IsMasterHelper.CreateProtocol(command);
                var isMasterResult = await IsMasterHelper.GetResultAsync(connection, isMasterProtocol, cancellationToken).ConfigureAwait(false);
                var mergedIsMasterResult = new IsMasterResult(description.IsMasterResult.Wrapped.Merge(isMasterResult.Wrapped));
                description = new ConnectionDescription(
                    description.ConnectionId, 
                    mergedIsMasterResult, 
                    description.BuildInfoResult);
            }  
            
            var authenticator = CreateAuthenticator(connection, description);
            await authenticator.AuthenticateAsync(connection, description, cancellationToken).ConfigureAwait(false);
        }


        /// <inheritdoc/>
        public BsonDocument CustomizeInitialIsMasterCommand(BsonDocument isMasterCommand)
        {
            return isMasterCommand.Merge(CreateSaslSupportedMechsRequest(_credential.Source, _credential.Username)); 
        }

        private static BsonDocument CreateSaslSupportedMechsRequest(string authenticationDatabaseName, string userName)
        {
            return new BsonDocument {{"saslSupportedMechs", $"{authenticationDatabaseName}.{userName}"}};
        }

        // see https://github.com/mongodb/specifications/blob/master/source/auth/auth.rst#defaults
        private IAuthenticator CreateAuthenticator(IConnection connection, ConnectionDescription description)
        {            
            // If a saslSupportedMechs field was present in the isMaster results for mechanism negotiation,
            // then it MUST be inspected to select a default mechanism.
            if (description.IsMasterResult.HasSaslSupportedMechs)
            {
                // If SCRAM-SHA-256 is present in the list of mechanisms, then it MUST be used as the default;
                // otherwise, SCRAM-SHA-1 MUST be used as the default, regardless of whether SCRAM-SHA-1 is in the list.
                return description.IsMasterResult.SaslSupportedMechs.Contains("SCRAM-SHA-256")
                    ? (IAuthenticator) new ScramSha256Authenticator(_credential, _randomStringGenerator)
                    :  new ScramSha1Authenticator(_credential, _randomStringGenerator);
            }
             // If saslSupportedMechs is not present in the isMaster results for mechanism negotiation, then SCRAM-SHA-1
             // MUST be used when talking to servers >= 3.0. Prior to server 3.0, MONGODB-CR MUST be used.
#pragma warning disable 618
            return Feature.ScramSha1Authentication.IsSupported(description.ServerVersion)
                    ? (IAuthenticator) new ScramSha1Authenticator(_credential, _randomStringGenerator)
                    : new MongoDBCRAuthenticator(_credential);
#pragma warning restore 618
        }
    }
}
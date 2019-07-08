/* Copyright 2018–present MongoDB Inc.
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


using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Core.Authentication;
using MongoDB.Driver.Core.WireProtocol;

namespace MongoDB.Driver.Core.Connections
{
    internal static class IsMasterHelper
    {   
        internal static BsonDocument AddClientDocumentToCommand(BsonDocument command, BsonDocument clientDocument)
        {
            return command.Add("client", clientDocument, clientDocument != null); 
        }
        
        internal static BsonDocument CreateCommand()
        {
            return new BsonDocument { { "isMaster", 1 } };
        }
        
        internal static BsonDocument CustomizeCommand(BsonDocument command, IReadOnlyList<IAuthenticator> authenticators)
        {
            return authenticators.Count == 1 ? authenticators[0].CustomizeInitialIsMasterCommand(command) : command;
        }

        internal static CommandWireProtocol<BsonDocument> CreateProtocol(BsonDocument isMasterCommand)
        {    
            return new CommandWireProtocol<BsonDocument>(
                databaseNamespace: DatabaseNamespace.Admin,
                command: isMasterCommand,
                slaveOk: true,
                resultSerializer: BsonDocumentSerializer.Instance,
                messageEncoderSettings: null);
        }
        
        internal static IsMasterResult GetResult(
            IConnection connection,
            CommandWireProtocol<BsonDocument> isMasterProtocol,
            CancellationToken cancellationToken)
        {
            try
            {
                return new IsMasterResult(isMasterProtocol.Execute(connection, cancellationToken));
            }
            catch (MongoCommandException ex) when (ex.Code == 11)
            {
                // If the isMaster command fails with error code 11 (UserNotFound), drivers must consider authentication
                // to have failed.In such a case, drivers MUST raise an error that is equivalent to what they would have
                // raised if the authentication mechanism were specified and the server responded the same way.
                throw new MongoAuthenticationException(connection.ConnectionId, "User not found.", ex);
            }
        }
        
        internal static async Task<IsMasterResult> GetResultAsync(
            IConnection connection,
            CommandWireProtocol<BsonDocument> isMasterProtocol,
            CancellationToken cancellationToken)
        {
            try
            {
                var isMasterResult = await isMasterProtocol.ExecuteAsync(connection, cancellationToken).ConfigureAwait(false);
                return new IsMasterResult(isMasterResult);
            }
            catch (MongoCommandException ex) when (ex.Code == 11)
            {
                // If the isMaster command fails with error code 11 (UserNotFound), drivers must consider authentication
                // to have failed.In such a case, drivers MUST raise an error that is equivalent to what they would have
                // raised if the authentication mechanism were specified and the server responded the same way.
                throw new MongoAuthenticationException(connection.ConnectionId, "User not found.", ex);
            }
        }
    }
}
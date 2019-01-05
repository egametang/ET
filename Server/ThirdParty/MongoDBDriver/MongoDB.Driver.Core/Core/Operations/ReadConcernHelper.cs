/* Copyright 2017-present MongoDB Inc.
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

using MongoDB.Bson;
using MongoDB.Driver.Core.Bindings;
using MongoDB.Driver.Core.Connections;

namespace MongoDB.Driver.Core.Operations
{
    internal static class ReadConcernHelper
    {
        public static BsonDocument GetReadConcernForCommand(ICoreSession session, ConnectionDescription connectionDescription, ReadConcern readConcern)
        {
            return session.IsInTransaction ? null : ToBsonDocument(session, connectionDescription, readConcern);
        }

        public static BsonDocument GetReadConcernForFirstCommandInTransaction(ICoreSession session, ConnectionDescription connectionDescription)
        {
            var readConcern = session.CurrentTransaction.TransactionOptions.ReadConcern;
            return ToBsonDocument(session, connectionDescription, readConcern);
        }

        // private static methods
        private static BsonDocument ToBsonDocument(ICoreSession session, ConnectionDescription connectionDescription, ReadConcern readConcern)
        {
            var sessionsAreSupported = connectionDescription.IsMasterResult.LogicalSessionTimeout != null;
            var shouldSendAfterClusterTime = sessionsAreSupported && session.IsCausallyConsistent && session.OperationTime != null;
            var shouldSendReadConcern = !readConcern.IsServerDefault || shouldSendAfterClusterTime;

            if (shouldSendReadConcern)
            {
                var readConcernDocument = readConcern.ToBsonDocument();
                if (shouldSendAfterClusterTime)
                {
                    readConcernDocument.Add("afterClusterTime", session.OperationTime);
                }
                return readConcernDocument;
            }

            return null;
        }
    }
}

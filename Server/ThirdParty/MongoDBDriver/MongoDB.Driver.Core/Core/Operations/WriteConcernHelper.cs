/* Copyright 2018-present MongoDB Inc.
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
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Core.Operations
{
    internal static class WriteConcernHelper
    {
        public static BsonDocument GetWriteConcernForCommand(ICoreSession session, WriteConcern writeConcern, SemanticVersion serverVersion, Feature requiredFeature)
        {
            if (!session.IsInTransaction && writeConcern != null && !writeConcern.IsServerDefault && requiredFeature.IsSupported(serverVersion))
            {
                return writeConcern.ToBsonDocument();
            }

            return null;
        }

        public static BsonDocument GetWriteConcernForCommandThatWrites(ICoreSession session, WriteConcern writeConcern, SemanticVersion serverVersion)
        {
            return GetWriteConcernForCommand(session, writeConcern, serverVersion, Feature.CommandsThatWriteAcceptWriteConcern);
        }

        public static BsonDocument GetWriteConcernForWriteCommand(ICoreSession session, WriteConcern writeConcern)
        {
            if (!session.IsInTransaction && writeConcern != null && !writeConcern.IsServerDefault)
            {
                return writeConcern.ToBsonDocument();
            }

            return null;
        }
    }
}

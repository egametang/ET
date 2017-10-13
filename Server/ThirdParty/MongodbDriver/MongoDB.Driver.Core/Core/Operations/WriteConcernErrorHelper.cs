/* Copyright 2016 MongoDB Inc.
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
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver.Core.Connections;

namespace MongoDB.Driver.Core.Operations
{
    internal static class WriteConcernErrorHelper
    {
        public static void ThrowIfHasWriteConcernError(ConnectionId connectionId, BsonDocument result)
        {
            BsonValue value;
            if (result.TryGetValue("writeConcernError", out value))
            {
                var message = (string)value.AsBsonDocument.GetValue("errmsg", null);
                var writeConcernResult = new WriteConcernResult(result);
                throw new MongoWriteConcernException(connectionId, message, writeConcernResult);
            }
        }
    }
}

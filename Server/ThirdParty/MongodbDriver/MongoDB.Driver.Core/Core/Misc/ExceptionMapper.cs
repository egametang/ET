/* Copyright 2010-2015 MongoDB Inc.
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
using MongoDB.Bson;
using MongoDB.Driver.Core.Connections;

namespace MongoDB.Driver.Core.Misc
{
    /// <summary>
    /// A mapper from error responses to custom exceptions.
    /// </summary>
    internal static class ExceptionMapper
    {
        /// <summary>
        /// Maps the specified response to a custom exception (if possible).
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="response">The response.</param>
        /// <returns>
        /// The custom exception (or null if the response could not be mapped to a custom exception).
        /// </returns>
        public static Exception Map(ConnectionId connectionId, BsonDocument response)
        {
            BsonValue code;
            if (response.TryGetValue("code", out code) && code.IsNumeric)
            {
                switch (code.ToInt32())
                {
                    case 50:
                    case 13475:
                    case 16986:
                    case 16712:
                        return new MongoExecutionTimeoutException(connectionId, message: "Operation exceeded time limit.");
                }
            }

            // the server sometimes sends a response that is missing the "code" field but does have an "errmsg" field
            BsonValue errmsg;
            if (response.TryGetValue("errmsg", out errmsg) && errmsg.IsString)
            {
                if (errmsg.AsString.Contains("exceeded time limit") ||
                    errmsg.AsString.Contains("execution terminated"))
                {
                    return new MongoExecutionTimeoutException(connectionId, message: "Operation exceeded time limit.");
                }
            }

            return null;
        }

        /// <summary>
        /// Maps the specified writeConcernResult to a custom exception (if necessary).
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="writeConcernResult">The write concern result.</param>
        /// <returns>
        /// The custom exception (or null if the writeConcernResult was not mapped to an exception).
        /// </returns>
        public static Exception Map(ConnectionId connectionId, WriteConcernResult writeConcernResult)
        {
            var code = GetCode(writeConcernResult.Response);
            if (code.HasValue)
            {
                switch (code.Value)
                {
                    case 11000:
                    case 11001:
                    case 12582:
                        var errorMessage = string.Format(
                            "WriteConcern detected an error '{0}'. (Response was {1}).",
                            writeConcernResult.LastErrorMessage, writeConcernResult.Response.ToJson());
                        return new MongoDuplicateKeyException(connectionId, errorMessage, writeConcernResult);
                }
            }

            bool ok = writeConcernResult.Response.GetValue("ok", false).ToBoolean();

            if (!ok)
            {
                var errorMessage = string.Format(
                    "WriteConcern detected an error '{0}'. (Response was {1}).",
                    writeConcernResult.LastErrorMessage, writeConcernResult.Response.ToJson());
                return new MongoWriteConcernException(connectionId, errorMessage, writeConcernResult);
            }

            if (writeConcernResult.HasLastErrorMessage)
            {
                var errorMessage = string.Format(
                    "WriteConcern detected an error '{0}'. (Response was {1}).",
                    writeConcernResult.LastErrorMessage,
                    writeConcernResult.Response.ToJson());
                return new MongoWriteConcernException(connectionId, errorMessage, writeConcernResult);
            }

            return null;
        }

        /// <summary>
        /// Maps the server response to a MongoNotPrimaryException or MongoNodeIsRecoveringException (if appropriate).
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="response">The server response.</param>
        /// <param name="errorMessageFieldName">Name of the error message field.</param>
        /// <returns>The exception, or null if no exception necessary.</returns>
        public static Exception MapNotPrimaryOrNodeIsRecovering(ConnectionId connectionId, BsonDocument response, string errorMessageFieldName)
        {
            BsonValue errorMessageBsonValue;
            if (response.TryGetValue(errorMessageFieldName, out errorMessageBsonValue) && errorMessageBsonValue.IsString)
            {
                var errorMessage = errorMessageBsonValue.ToString();
                if (errorMessage.StartsWith("not master", StringComparison.OrdinalIgnoreCase))
                {
                    return new MongoNotPrimaryException(connectionId, response);
                }
                if (errorMessage.StartsWith("node is recovering", StringComparison.OrdinalIgnoreCase))
                {
                    return new MongoNodeIsRecoveringException(connectionId, response);
                }
            }

            return null;
        }

        private static int? GetCode(BsonDocument response)
        {
            BsonValue code;
            if (!response.TryGetValue("code", out code))
            {
                BsonValue err;
                BsonValue errObjects;
                if (response.TryGetValue("err", out err) && response.TryGetValue("errObjects", out errObjects) && errObjects.IsBsonArray)
                {
                    foreach (var errObject in errObjects.AsBsonArray.OfType<BsonDocument>())
                    {
                        BsonValue currentErr = errObject.GetValue("err", null);
                        if (err.Equals(currentErr))
                        {
                            code = errObject.GetValue("code", null);
                            break;
                        }
                    }
                }
            }

            return (code != null) ? code.ToInt32() : (int?)null;
        }
    }
}
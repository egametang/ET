/* Copyright 2010-2014 MongoDB Inc.
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
using MongoDB.Bson;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Core.Operations
{
    internal class BulkWriteOperationResultConverter
    {
        // constructors
        public BulkWriteOperationResultConverter()
        {
        }

        // public methods
        public Exception ToWriteConcernException(ConnectionId connectionId, MongoBulkWriteOperationException bulkWriteException)
        {
            var writeConcernResult = ToWriteConcernResult(bulkWriteException.Result, bulkWriteException);

            var exception = ExceptionMapper.Map(connectionId, writeConcernResult.Response);
            if (exception == null)
            {
                exception = ExceptionMapper.Map(connectionId, writeConcernResult);
            }
            if (exception == null)
            {
                exception = new MongoWriteConcernException(connectionId, bulkWriteException.Message, writeConcernResult);
            }

            var writeConcernException = exception as MongoWriteConcernException;
            if (writeConcernException != null)
            { 
                writeConcernException.Data["results"] = new List<WriteConcernResult>(new[] { writeConcernResult });
            }

            return exception; // usually a WriteConcernException unless ExceptionMapper chose a different type
        }

        public WriteConcernResult ToWriteConcernResult(BulkWriteOperationResult bulkWriteResult)
        {
            return ToWriteConcernResult(bulkWriteResult, null);
        }

        // private methods
        private WriteConcernResult ToWriteConcernResult(BulkWriteOperationResult bulkWriteResult, MongoBulkWriteOperationException bulkWriteException)
        {
            if (!bulkWriteResult.IsAcknowledged)
            {
                return null;
            }

            // don't include InsertedCount in getLastErrorResponse
            var documentsAffectedCount =
                bulkWriteResult.DeletedCount +
                bulkWriteResult.MatchedCount +
                bulkWriteResult.Upserts.Count;

            var isUpdate = bulkWriteResult.ProcessedRequests.Any(r => r.RequestType == WriteRequestType.Update);

            var updatedExisting = false;
            BulkWriteOperationUpsert upsert = null;
            if (isUpdate)
            {
                upsert = bulkWriteResult.Upserts.LastOrDefault();
                updatedExisting = documentsAffectedCount > 0 && upsert == null;
            }

            var code = 0;
            string message = null;
            BsonDocument details = null;
            if (bulkWriteException != null)
            {
                var lastWriteError = bulkWriteException.WriteErrors.LastOrDefault();
                var writeConcernError = bulkWriteException.WriteConcernError;

                code = 8; // UnknownError
                if (lastWriteError != null)
                {
                    code = lastWriteError.Code;
                    message = lastWriteError.Message;
                    details = lastWriteError.Details;
                }
                else if (writeConcernError != null)
                {
                    code = writeConcernError.Code;
                    message = writeConcernError.Message;
                    details = writeConcernError.Details;
                }
            }

            var getLastErrorResponse = new BsonDocument
            {
                { "ok", 1 },
                { "code", code, code != 0 },
                { "err", message, message != null },
                { "n", documentsAffectedCount },
                { "updatedExisting", updatedExisting, isUpdate },
                { "upserted", () => upsert.Id, isUpdate && upsert != null },
            };
            if (details != null)
            {
                getLastErrorResponse.Merge(details, false); // don't overwrite existing elements
            }

            return new WriteConcernResult(getLastErrorResponse);
        }
    }
}

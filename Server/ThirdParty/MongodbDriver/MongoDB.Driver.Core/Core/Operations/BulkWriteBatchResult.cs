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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Core.Operations
{
    /// <summary>
    /// Represents the result of one batch executed using a write command.
    /// </summary>
    internal class BulkWriteBatchResult
    {
        #region static
        // static fields
        private static readonly IReadOnlyList<BulkWriteOperationUpsert> __noUpserts = new BulkWriteOperationUpsert[0];
        private static readonly IReadOnlyList<BulkWriteOperationError> __noWriteErrors = new BulkWriteOperationError[0];
        private static readonly IReadOnlyList<WriteRequest> __noWriteRequests = new WriteRequest[0];

        // static methods
        public static BulkWriteBatchResult Create(
            bool isOrdered,
            IReadOnlyList<WriteRequest> requests,
            BsonDocument writeCommandResponse,
            IndexMap indexMap)
        {
            var writeErrors = CreateWriteErrors(writeCommandResponse);
            var writeConcernError = CreateWriteConcernError(writeCommandResponse);
            var processedRequests = CreateProcessedRequests(requests, writeErrors, isOrdered);
            var unprocessedRequests = CreateUnprocessedRequests(requests, writeErrors, isOrdered);
            var upserts = CreateUpserts(writeCommandResponse);

            var n = writeCommandResponse.GetValue("n", 0).ToInt64();
            var matchedCount = 0L;
            var deletedCount = 0L;
            var insertedCount = 0L;
            long? modifiedCount = 0L;
            var firstRequest = requests.FirstOrDefault();
            if (firstRequest != null)
            {
                switch (firstRequest.RequestType)
                {
                    case WriteRequestType.Delete:
                        deletedCount = n;
                        break;
                    case WriteRequestType.Insert:
                        insertedCount = n;
                        break;
                    case WriteRequestType.Update:
                        matchedCount = n - upserts.Count();
                        BsonValue nModified;
                        if (writeCommandResponse.TryGetValue("nModified", out nModified))
                        {
                            modifiedCount = nModified.ToInt64();
                        }
                        else
                        {
                            modifiedCount = null;
                        }
                        break;
                }
            }

            return new BulkWriteBatchResult(
                requests.Count,
                processedRequests,
                unprocessedRequests,
                matchedCount,
                deletedCount,
                insertedCount,
                modifiedCount,
                upserts,
                writeErrors,
                writeConcernError,
                indexMap);
        }

        public static BulkWriteBatchResult Create(
           BulkWriteOperationResult result,
           MongoBulkWriteOperationException exception,
           IndexMap indexMap)
        {
            var matchedCount = 0L;
            var deletedCount = 0L;
            var insertedCount = 0L;
            long? modifiedCount = null;
            var upserts = __noUpserts;
            if (result.IsAcknowledged)
            {
                matchedCount = result.MatchedCount;
                deletedCount = result.DeletedCount;
                insertedCount = result.InsertedCount;
                modifiedCount = result.IsModifiedCountAvailable ? (long?)result.ModifiedCount : null;
                upserts = result.Upserts;
            }

            var unprocessedRequests = __noWriteRequests;
            var writeErrors = __noWriteErrors;
            BulkWriteConcernError writeConcernError = null;
            if (exception != null)
            {
                unprocessedRequests = exception.UnprocessedRequests;
                writeErrors = exception.WriteErrors;
                writeConcernError = exception.WriteConcernError;
            }

            return new BulkWriteBatchResult(
                result.RequestCount,
                result.ProcessedRequests,
                unprocessedRequests,
                matchedCount,
                deletedCount,
                insertedCount,
                modifiedCount,
                upserts,
                writeErrors,
                writeConcernError,
                indexMap);
        }

        public static BulkWriteBatchResult Create(
            WriteRequest request,
            WriteConcernResult writeConcernResult,
            MongoWriteConcernException writeConcernException,
            IndexMap indexMap)
        {
            var processedRequests = new[] { request };
            var unprocessedRequests = __noWriteRequests;
            BsonValue upsertId = null;
            var documentsAffected = 0L;

            if (writeConcernResult != null)
            {
                upsertId = writeConcernResult.Upserted;
                documentsAffected = writeConcernResult.DocumentsAffected;
                var updateRequest = request as UpdateRequest;

                if (upsertId == null &&
                    documentsAffected == 1 &&
                    updateRequest != null &&
                    updateRequest.IsUpsert &&
                    !writeConcernResult.UpdatedExisting)
                {
                    // Get the _id field first from the Update document
                    // and then from the Query document.
                    upsertId =
                        updateRequest.Update.ToBsonDocument().GetValue("_id", null) ??
                        updateRequest.Filter.ToBsonDocument().GetValue("_id", null);
                }
            }

            var upserts = (upsertId == null) ? __noUpserts : new[] { new BulkWriteOperationUpsert(0, upsertId) };
            var writeErrors = __noWriteErrors;
            BulkWriteConcernError writeConcernError = null;

            if (writeConcernException != null)
            {
                var getLastErrorResponse = writeConcernResult.Response;
                if (IsGetLasterrorResponseAWriteConcernError(getLastErrorResponse))
                {
                    writeConcernError = CreateWriteConcernErrorFromGetLastErrorResponse(getLastErrorResponse);
                }
                else
                {
                    writeErrors = new[] { CreateWriteErrorFromGetLastErrorResponse(getLastErrorResponse) };
                }
            }

            if (request.RequestType == WriteRequestType.Insert && writeErrors.Count == 0)
            {
                documentsAffected = 1; // note: DocumentsAffected is 0 for inserts
            }

            var matchedCount = 0L;
            var deletedCount = 0L;
            var insertedCount = 0L;
            long? modifiedCount = 0L;
            switch (request.RequestType)
            {
                case WriteRequestType.Delete:
                    deletedCount = documentsAffected;
                    break;
                case WriteRequestType.Insert:
                    insertedCount = documentsAffected;
                    break;
                case WriteRequestType.Update:
                    matchedCount = documentsAffected - upserts.Count();
                    modifiedCount = null; // getLasterror does not report this value
                    break;
            }

            return new BulkWriteBatchResult(
                1, // batchCount
                processedRequests,
                unprocessedRequests,
                matchedCount,
                deletedCount,
                insertedCount,
                modifiedCount,
                upserts,
                writeErrors,
                writeConcernError,
                indexMap);
        }

        private static IReadOnlyList<WriteRequest> CreateProcessedRequests(IReadOnlyList<WriteRequest> requests, IReadOnlyList<BulkWriteOperationError> writeErrors, bool isOrdered)
        {
            if (!isOrdered || writeErrors.Count == 0)
            {
                return requests;
            }
            else
            {
                var processedCount = writeErrors.Max(e => e.Index) + 1;
                return requests.Take(processedCount).ToList();
            }
        }

        private static IReadOnlyList<WriteRequest> CreateUnprocessedRequests(IReadOnlyList<WriteRequest> requests, IReadOnlyList<BulkWriteOperationError> writeErrors, bool isOrdered)
        {
            if (!isOrdered || writeErrors.Count == 0)
            {
                return __noWriteRequests;
            }
            else
            {
                var processedCount = writeErrors.Max(e => e.Index) + 1;
                return requests.Skip(processedCount).ToList();
            }
        }

        private static IReadOnlyList<BulkWriteOperationUpsert> CreateUpserts(BsonDocument writeCommandResponse)
        {
            var upserts = new List<BulkWriteOperationUpsert>();

            if (writeCommandResponse.Contains("upserted"))
            {
                foreach (BsonDocument value in writeCommandResponse["upserted"].AsBsonArray)
                {
                    var index = value["index"].ToInt32();
                    var id = value["_id"];
                    var upsert = new BulkWriteOperationUpsert(index, id);
                    upserts.Add(upsert);
                }
            }

            return upserts;
        }

        private static BulkWriteConcernError CreateWriteConcernError(BsonDocument writeCommandResponse)
        {
            if (writeCommandResponse.Contains("writeConcernError"))
            {
                var value = (BsonDocument)writeCommandResponse["writeConcernError"];
                var code = value["code"].ToInt32();
                var message = value["errmsg"].AsString;
                var details = (BsonDocument)value.GetValue("errInfo", null);
                return new BulkWriteConcernError(code, message, details);
            }

            return null;
        }

        private static BulkWriteConcernError CreateWriteConcernErrorFromGetLastErrorResponse(BsonDocument getLastErrorResponse)
        {
            var code = getLastErrorResponse.GetValue("code", 64).ToInt32(); // default = WriteConcernFailed

            string message = null;
            BsonValue value;
            if (getLastErrorResponse.TryGetValue("err", out value) && value.BsonType == BsonType.String)
            {
                message = value.AsString;
            }
            else if (getLastErrorResponse.TryGetValue("jnote", out value) && value.BsonType == BsonType.String)
            {
                message = value.AsString;
            }
            else if (getLastErrorResponse.TryGetValue("wnote", out value) && value.BsonType == BsonType.String)
            {
                message = value.AsString;
            }

            var details = new BsonDocument(getLastErrorResponse.Where(e => !new[] { "ok", "code", "err" }.Contains(e.Name)));

            return new BulkWriteConcernError(code, message, details);
        }

        private static BulkWriteOperationError CreateWriteErrorFromGetLastErrorResponse(BsonDocument getLastErrorResponse)
        {
            var code = getLastErrorResponse.GetValue("code", 8).ToInt32(); // default = UnknownError
            var message = (string)getLastErrorResponse.GetValue("err", null);
            return new BulkWriteOperationError(0, code, message, null);
        }

        private static IReadOnlyList<BulkWriteOperationError> CreateWriteErrors(BsonDocument writeCommandResponse)
        {
            var writeErrors = new List<BulkWriteOperationError>();

            if (writeCommandResponse.Contains("writeErrors"))
            {
                foreach (BsonDocument value in writeCommandResponse["writeErrors"].AsBsonArray)
                {
                    var index = value["index"].ToInt32();
                    var code = value["code"].ToInt32();
                    var message = value["errmsg"].AsString;
                    var details = (BsonDocument)value.GetValue("errInfo", null);
                    var writeError = new BulkWriteOperationError(index, code, message, details);
                    writeErrors.Add(writeError);
                }
            }

            return writeErrors;
        }

        private static bool IsGetLasterrorResponseAWriteConcernError(BsonDocument getLastErrorResponse)
        {
            return new[] { "wtimeout", "jnote", "wnote" }.Any(n => getLastErrorResponse.Contains(n));
        }
        #endregion

        // fields
        private readonly int _batchCount;
        private readonly long _deletedCount;
        private readonly IndexMap _indexMap;
        private readonly long _insertedCount;
        private readonly long _matchedCount;
        private readonly long? _modifiedCount;
        private readonly IReadOnlyList<WriteRequest> _processedRequests;
        private readonly IReadOnlyList<WriteRequest> _unprocessedRequests;
        private readonly IReadOnlyList<BulkWriteOperationUpsert> _upserts;
        private readonly BulkWriteConcernError _writeConcernError;
        private readonly IReadOnlyList<BulkWriteOperationError> _writeErrors;

        // constructors
        public BulkWriteBatchResult(
            int batchCount,
            IReadOnlyList<WriteRequest> processedRequests,
            IReadOnlyList<WriteRequest> unprocessedRequests,
            long matchedCount,
            long deletedCount,
            long insertedCount,
            long? modifiedCount,
            IReadOnlyList<BulkWriteOperationUpsert> upserts,
            IReadOnlyList<BulkWriteOperationError> writeErrors,
            BulkWriteConcernError writeConcernError,
            IndexMap indexMap)
        {
            _batchCount = batchCount;
            _matchedCount = matchedCount;
            _deletedCount = deletedCount;
            _insertedCount = insertedCount;
            _modifiedCount = modifiedCount;
            _indexMap = indexMap;
            _processedRequests = processedRequests;
            _writeErrors = writeErrors;
            _unprocessedRequests = unprocessedRequests;
            _upserts = upserts;
            _writeConcernError = writeConcernError;
        }

        // properties
        public int BatchCount
        {
            get { return _batchCount; }
        }

        public long DeletedCount
        {
            get { return _deletedCount; }
        }

        public bool HasWriteConcernError
        {
            get { return _writeConcernError != null; }
        }

        public bool HasWriteErrors
        {
            get { return _writeErrors.Count > 0; }
        }

        public IndexMap IndexMap
        {
            get { return _indexMap; }
        }

        public long InsertedCount
        {
            get { return _insertedCount; }
        }

        public long MatchedCount
        {
            get { return _matchedCount; }
        }

        public long? ModifiedCount
        {
            get { return _modifiedCount; }
        }

        public IReadOnlyList<WriteRequest> ProcessedRequests
        {
            get { return _processedRequests; }
        }

        public IReadOnlyList<WriteRequest> UnprocessedRequests
        {
            get { return _unprocessedRequests; }
        }

        public IReadOnlyList<BulkWriteOperationUpsert> Upserts
        {
            get { return _upserts; }
        }

        public BulkWriteConcernError WriteConcernError
        {
            get { return _writeConcernError; }
        }

        public IReadOnlyList<BulkWriteOperationError> WriteErrors
        {
            get { return _writeErrors; }
        }
    }
}

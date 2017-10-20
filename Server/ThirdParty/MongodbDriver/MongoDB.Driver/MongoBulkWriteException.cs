/* Copyright 2010-2016 MongoDB Inc.
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
#if NET45
using System.Runtime.Serialization;
#endif
using System.Text;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Operations;
using MongoDB.Driver.Support;

namespace MongoDB.Driver
{
    /// <summary>
    /// Represents a bulk write exception.
    /// </summary>
#if NET45
    [Serializable]
#endif
    public abstract class MongoBulkWriteException : MongoServerException
    {
        // private fields
        private readonly WriteConcernError _writeConcernError;
        private readonly IReadOnlyList<BulkWriteError> _writeErrors;

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoBulkWriteException" /> class.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="writeErrors">The write errors.</param>
        /// <param name="writeConcernError">The write concern error.</param>
        public MongoBulkWriteException(
            ConnectionId connectionId,
            IEnumerable<BulkWriteError> writeErrors,
            WriteConcernError writeConcernError)
            : base(connectionId, message: FormatMessage(writeErrors, writeConcernError))
        {
            _writeErrors = writeErrors.ToList();
            _writeConcernError = writeConcernError;
        }

#if NET45
        /// <summary>
        /// Initializes a new instance of the MongoQueryException class (this overload supports deserialization).
        /// </summary>
        /// <param name="info">The SerializationInfo.</param>
        /// <param name="context">The StreamingContext.</param>
        public MongoBulkWriteException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _writeConcernError = (WriteConcernError)info.GetValue("_writeConcernError", typeof(WriteConcernError));
            _writeErrors = (IReadOnlyList<BulkWriteError>)info.GetValue("_writeErrors", typeof(IReadOnlyList<BulkWriteError>));
        }
#endif

        // properties
        /// <summary>
        /// Gets the write concern error.
        /// </summary>
        public WriteConcernError WriteConcernError
        {
            get { return _writeConcernError; }
        }

        /// <summary>
        /// Gets the write errors.
        /// </summary>
        public IReadOnlyList<BulkWriteError> WriteErrors
        {
            get { return _writeErrors; }
        }

        // methods
#if NET45
        /// <summary>
        /// Gets the object data.
        /// </summary>
        /// <param name="info">The information.</param>
        /// <param name="context">The context.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("_writeConcernError", _writeConcernError);
            info.AddValue("_writeErrors", _writeErrors);
        }
#endif

        // private static methods
        private static string FormatMessage(IEnumerable<BulkWriteError> writeErrors, WriteConcernError writeConcernError)
        {
            var sb = new StringBuilder("A bulk write operation resulted in one or more errors.");
            if (writeErrors != null)
            {
                foreach (var writeError in writeErrors)
                {
                    sb.AppendLine().Append("  " + writeError.Message);
                }
            }
            if (writeConcernError != null)
            {
                sb.AppendLine().Append("  " + writeConcernError.Message);
            }

            return sb.ToString();
        }
    }

    /// <summary>
    /// Represents a bulk write exception.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document.</typeparam>
#if NET45
    [Serializable]
#endif
    public sealed class MongoBulkWriteException<TDocument> : MongoBulkWriteException
    {
        // private fields
        private readonly BulkWriteResult<TDocument> _result;
        private readonly IReadOnlyList<WriteModel<TDocument>> _unprocessedRequests;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="MongoBulkWriteException" /> class.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="result">The result.</param>
        /// <param name="writeErrors">The write errors.</param>
        /// <param name="writeConcernError">The write concern error.</param>
        /// <param name="unprocessedRequests">The unprocessed requests.</param>
        public MongoBulkWriteException(
            ConnectionId connectionId,
            BulkWriteResult<TDocument> result,
            IEnumerable<BulkWriteError> writeErrors,
            WriteConcernError writeConcernError,
            IEnumerable<WriteModel<TDocument>> unprocessedRequests)
            : base(connectionId, writeErrors, writeConcernError)
        {
            _result = result;

            _unprocessedRequests = unprocessedRequests.ToList();
        }

#if NET45
        /// <summary>
        /// Initializes a new instance of the MongoQueryException class (this overload supports deserialization).
        /// </summary>
        /// <param name="info">The SerializationInfo.</param>
        /// <param name="context">The StreamingContext.</param>
        public MongoBulkWriteException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            if (typeof(TDocument).IsSerializable)
            {
                _result = (BulkWriteResult<TDocument>)info.GetValue("_result", typeof(BulkWriteResult<TDocument>));
                _unprocessedRequests = (IReadOnlyList<WriteModel<TDocument>>)info.GetValue("_unprocessedRequests", typeof(IReadOnlyList<WriteModel<TDocument>>));
            }
        }
#endif

        // public properties
        /// <summary>
        /// Gets the result of the bulk write operation.
        /// </summary>
        public BulkWriteResult<TDocument> Result
        {
            get { return _result; }
        }

        /// <summary>
        /// Gets the unprocessed requests.
        /// </summary>
        public IReadOnlyList<WriteModel<TDocument>> UnprocessedRequests
        {
            get { return _unprocessedRequests; }
        }

        // methods
#if NET45
        /// <summary>
        /// Gets the object data.
        /// </summary>
        /// <param name="info">The information.</param>
        /// <param name="context">The context.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            if (typeof(TDocument).IsSerializable)
            {
                info.AddValue("_result", _result);
                info.AddValue("_unprocessedRequests", _unprocessedRequests);
            }
        }
#endif

        // internal static methods
        internal static MongoBulkWriteException<TDocument> FromCore(MongoBulkWriteOperationException ex)
        {
            return new MongoBulkWriteException<TDocument>(
                ex.ConnectionId,
                BulkWriteResult<TDocument>.FromCore(ex.Result),
                ex.WriteErrors.Select(e => BulkWriteError.FromCore(e)),
                WriteConcernError.FromCore(ex.WriteConcernError),
                ex.UnprocessedRequests.Select(r => WriteModel<TDocument>.FromCore(r)));
        }

        internal static MongoBulkWriteException<TDocument> FromCore(MongoBulkWriteOperationException ex, IReadOnlyList<WriteModel<TDocument>> requests)
        {
            var processedRequests = ex.Result.ProcessedRequests
                .Select(r => new { CorrelationId = r.CorrelationId.Value, Request = requests[r.CorrelationId.Value] })
                .OrderBy(x => x.CorrelationId)
                .Select(x => x.Request);

            var unprocessedRequests = ex.UnprocessedRequests
                .Select(r => new { CorrelationId = r.CorrelationId.Value, Request = requests[r.CorrelationId.Value] })
                .OrderBy(x => x.CorrelationId)
                .Select(x => x.Request);

            return new MongoBulkWriteException<TDocument>(
                ex.ConnectionId,
                BulkWriteResult<TDocument>.FromCore(ex.Result, processedRequests),
                ex.WriteErrors.Select(e => BulkWriteError.FromCore(e)),
                WriteConcernError.FromCore(ex.WriteConcernError),
                unprocessedRequests);
        }
    }
}

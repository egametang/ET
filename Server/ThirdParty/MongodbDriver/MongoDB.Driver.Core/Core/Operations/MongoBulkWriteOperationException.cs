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
using System.Collections.ObjectModel;
using System.Linq;
#if NET45
using System.Runtime.Serialization;
#endif
using System.Text;
using MongoDB.Driver.Core.Connections;

namespace MongoDB.Driver.Core.Operations
{
    /// <summary>
    /// Represents a bulk write operation exception.
    /// </summary>
#if NET45
    [Serializable]
#endif
    public class MongoBulkWriteOperationException : MongoServerException
    {
        // fields
        private BulkWriteOperationResult _result;
        private IReadOnlyList<WriteRequest> _unprocessedRequests;
        private BulkWriteConcernError _writeConcernError;
        private IReadOnlyList<BulkWriteOperationError> _writeErrors;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="MongoBulkWriteOperationException" /> class.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="result">The result.</param>
        /// <param name="writeErrors">The write errors.</param>
        /// <param name="writeConcernError">The write concern error.</param>
        /// <param name="unprocessedRequests">The unprocessed requests.</param>
        public MongoBulkWriteOperationException(
            ConnectionId connectionId,
            BulkWriteOperationResult result,
            IReadOnlyList<BulkWriteOperationError> writeErrors,
            BulkWriteConcernError writeConcernError,
            IReadOnlyList<WriteRequest> unprocessedRequests)
            : base(connectionId, FormatMessage(writeErrors, writeConcernError))
        {
            _result = result;
            _writeErrors = writeErrors;
            _writeConcernError = writeConcernError;
            _unprocessedRequests = unprocessedRequests;
        }

#if NET45
        /// <summary>
        /// Initializes a new instance of the <see cref="MongoBulkWriteOperationException" /> class.
        /// </summary>
        /// <param name="info">The SerializationInfo.</param>
        /// <param name="context">The StreamingContext.</param>
        public MongoBulkWriteOperationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _result = (BulkWriteOperationResult)info.GetValue("_result", typeof(BulkWriteOperationResult));
            _unprocessedRequests = (IReadOnlyList<WriteRequest>)info.GetValue("_unprocessedRequests", typeof(IReadOnlyList<WriteRequest>));
            _writeConcernError = (BulkWriteConcernError)info.GetValue("_writeConcernError", typeof(BulkWriteConcernError));
            _writeErrors = (IReadOnlyList<BulkWriteOperationError>)info.GetValue("_writeErrors", typeof(IReadOnlyList<BulkWriteOperationError>));
        }
#endif

        // properties
        /// <summary>
        /// Gets the result of the bulk write operation.
        /// </summary>
        public BulkWriteOperationResult Result
        {
            get { return _result; }
        }

        /// <summary>
        /// Gets the unprocessed requests.
        /// </summary>
        /// <value>
        /// The unprocessed requests.
        /// </value>
        /// <exception cref="System.NotImplementedException"></exception>
        public IReadOnlyList<WriteRequest> UnprocessedRequests
        {
            get { return _unprocessedRequests; }
        }

        /// <summary>
        /// Gets the write concern error.
        /// </summary>
        /// <value>
        /// The write concern error.
        /// </value>
        public BulkWriteConcernError WriteConcernError
        {
            get { return _writeConcernError; }
        }

        /// <summary>
        /// Gets the write errors.
        /// </summary>
        /// <value>
        /// The write errors.
        /// </value>
        public IReadOnlyList<BulkWriteOperationError> WriteErrors
        {
            get { return _writeErrors; }
        }

        // methods
#if NET45
        /// <inheritdoc/>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("_result", _result);
            info.AddValue("_unprocessedRequests", _unprocessedRequests);
            info.AddValue("_writeConcernError", _writeConcernError);
            info.AddValue("_writeErrors", _writeErrors);
        }
#endif

        private static string FormatMessage(IReadOnlyList<BulkWriteOperationError> writeErrors, BulkWriteConcernError writeConcernError)
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
}

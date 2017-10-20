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
#if NET45
using System.Runtime.Serialization;
#endif
using System.Text;
using MongoDB.Driver.Core.Connections;

namespace MongoDB.Driver
{
    /// <summary>
    /// Represents a write exception.
    /// </summary>
#if NET45
    [Serializable]
#endif
    public class MongoWriteException : MongoServerException
    {
        // static
        internal static MongoWriteException FromBulkWriteException(MongoBulkWriteException bulkException)
        {
            var writeConcernError = bulkException.WriteConcernError;
            var writeError = bulkException.WriteErrors.Count > 0
                ? bulkException.WriteErrors[0]
                : null;

            return new MongoWriteException(bulkException.ConnectionId, writeError, writeConcernError, bulkException);
        }

        // private fields
        private readonly WriteConcernError _writeConcernError;
        private readonly WriteError _writeError;

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoWriteException" /> class.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="writeError">The write error.</param>
        /// <param name="writeConcernError">The write concern error.</param>
        /// <param name="innerException">The inner exception.</param>
        public MongoWriteException(
            ConnectionId connectionId,
            WriteError writeError,
            WriteConcernError writeConcernError,
            Exception innerException)
            : base(connectionId, FormatMessage(writeError, writeConcernError), innerException)
        {
            _writeError = writeError;
            _writeConcernError = writeConcernError;
        }

#if NET45
        /// <summary>
        /// Initializes a new instance of the MongoQueryException class (this overload supports deserialization).
        /// </summary>
        /// <param name="info">The SerializationInfo.</param>
        /// <param name="context">The StreamingContext.</param>
        public MongoWriteException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _writeConcernError = (WriteConcernError)info.GetValue("_writeConcernError", typeof(WriteConcernError));
            _writeError = (WriteError)info.GetValue("_writeError", typeof(WriteError));
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
        /// Gets the write error.
        /// </summary>
        public WriteError WriteError
        {
            get { return _writeError; }
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
            info.AddValue("_writeError", _writeError);
        }
#endif

        // private static methods
        private static string FormatMessage(WriteError writeError, WriteConcernError writeConcernError)
        {
            var sb = new StringBuilder("A write operation resulted in an error.");
            if (writeError != null)
            {
                sb.AppendLine().Append("  " + writeError.Message);
            }
            if (writeConcernError != null)
            {
                sb.AppendLine().Append("  " + writeConcernError.Message);
            }

            return sb.ToString();
        }
    }
}

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

namespace MongoDB.Bson
{
    /// <summary>
    /// Represents a BSON exception.
    /// </summary>
#if NET45
    [Serializable]
#endif
    public class BsonException : Exception
    {
        // constructors
        /// <summary>
        /// Initializes a new instance of the BsonException class.
        /// </summary>
        public BsonException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the BsonException class.
        /// </summary>
        /// <param name="message">The error message.</param>
        public BsonException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the BsonException class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public BsonException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the BsonException class.
        /// </summary>
        /// <param name="format">The error message format string.</param>
        /// <param name="args">One or more args for the error message.</param>
        public BsonException(string format, params object[] args)
            : base(string.Format(format, args))
        {
        }

#if NET45
        /// <summary>
        /// Initializes a new instance of the BsonException class (this overload used by deserialization).
        /// </summary>
        /// <param name="info">The SerializationInfo.</param>
        /// <param name="context">The StreamingContext.</param>
        public BsonException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#endif
    }
}

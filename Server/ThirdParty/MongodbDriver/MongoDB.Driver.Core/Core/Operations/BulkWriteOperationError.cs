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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Core.Operations
{
    /// <summary>
    /// Represents the details of a write error for a particular request.
    /// </summary>
#if NET45
    [Serializable]
#endif
    public sealed class BulkWriteOperationError
    {
        // fields
        private readonly int _code;
        private readonly BsonDocument _details;
        private readonly int _index;
        private readonly string _message;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="BulkWriteOperationError"/> class.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="code">The code.</param>
        /// <param name="message">The message.</param>
        /// <param name="details">The details.</param>
        public BulkWriteOperationError(int index, int code, string message, BsonDocument details)
        {
            _code = code;
            _details = details;
            _index = index;
            _message = message;
        }

        // properties
        /// <summary>
        /// Gets the error category.
        /// </summary>
        /// <value>
        /// The error category.
        /// </value>
        public ServerErrorCategory Category
        {
            get
            {
                switch(_code)
                {
                    case 50:
                        return ServerErrorCategory.ExecutionTimeout;
                    case 11000:
                    case 11001:
                    case 12582:
                        return ServerErrorCategory.DuplicateKey;
                    default:
                        return ServerErrorCategory.Uncategorized;
                }
            }
        }

        /// <summary>
        /// Gets the error code.
        /// </summary>
        /// <value>
        /// The error code.
        /// </value>
        public int Code
        {
            get { return _code; }
        }

        /// <summary>
        /// Gets the error details.
        /// </summary>
        /// <value>
        /// The error details.
        /// </value>
        public BsonDocument Details
        {
            get { return _details; }
        }

        /// <summary>
        /// Gets the index of the request that had an error.
        /// </summary>
        /// <value>
        /// The index.
        /// </value>
        public int Index
        {
            get { return _index; }
        }

        /// <summary>
        /// Gets the error message.
        /// </summary>
        /// <value>
        /// The error message.
        /// </value>
        public string Message
        {
            get { return _message; }
        }

        // methods
        internal BulkWriteOperationError WithMappedIndex(IndexMap indexMap)
        {
            var mappedIndex = indexMap.Map(_index);
            return (_index == mappedIndex) ? this : new BulkWriteOperationError(mappedIndex, Code, Message, Details);
        }
    }
}


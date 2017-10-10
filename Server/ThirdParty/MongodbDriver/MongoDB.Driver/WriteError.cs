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
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.Support;

namespace MongoDB.Driver
{
    /// <summary>
    /// Represents the details of a write error.
    /// </summary>
#if NET45
    [Serializable]
#endif
    public class WriteError
    {
        // private fields
        private readonly ServerErrorCategory _category;
        private readonly int _code;
        private readonly BsonDocument _details;
        private readonly string _message;

        // constructors
        internal WriteError(ServerErrorCategory category, int code, string message, BsonDocument details)
        {
            _category = category;
            _code = code;
            _details = details;
            _message = message;
        }

        // public properties
        /// <summary>
        /// Gets the category.
        /// </summary>
        public ServerErrorCategory Category
        {
            get { return _category; }
        }

        /// <summary>
        /// Gets the error code.
        /// </summary>
        public int Code
        {
            get { return _code; }
        }

        /// <summary>
        /// Gets the error details.
        /// </summary>
        public BsonDocument Details
        {
            get { return _details; }
        }

        /// <summary>
        /// Gets the error message.
        /// </summary>
        public string Message
        {
            get { return _message; }
        }
    }
}

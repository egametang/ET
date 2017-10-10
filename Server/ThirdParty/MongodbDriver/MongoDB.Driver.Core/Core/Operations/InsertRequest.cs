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
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Core.Operations
{
    /// <summary>
    /// Represents a request to insert a document.
    /// </summary>
    public sealed class InsertRequest : WriteRequest
    {
        // fields
        private readonly BsonDocument _document;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="InsertRequest" /> class.
        /// </summary>
        /// <param name="document">The document.</param>
        public InsertRequest(BsonDocument document)
            : base(WriteRequestType.Insert)
        {
            _document = Ensure.IsNotNull(document, nameof(document));
        }

        // properties
        /// <summary>
        /// Gets or sets the document.
        /// </summary>
        /// <value>
        /// The document.
        /// </value>
        public BsonDocument Document
        {
            get { return _document; }
        }
    }
}

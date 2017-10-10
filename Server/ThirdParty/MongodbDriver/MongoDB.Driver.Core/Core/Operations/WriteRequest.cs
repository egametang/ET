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

namespace MongoDB.Driver.Core.Operations
{
    /// <summary>
    /// Represents a request to write something to the database.
    /// </summary>
#if NET45
    [Serializable]
#endif
    public abstract class WriteRequest
    {
        // fields
        private int? _correlationId;
        private readonly WriteRequestType _requestType;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="WriteRequest" /> class.
        /// </summary>
        /// <param name="requestType">The request type.</param>
        protected WriteRequest(WriteRequestType requestType)
        {
            _requestType = requestType;
        }

        // properties
        /// <summary>
        /// Gets or sets the correlation identifier.
        /// </summary>
        public int? CorrelationId
        {
            get { return _correlationId; }
            set { _correlationId = value; }
        }

        /// <summary>
        /// Gets the request type.
        /// </summary>
        /// <value>
        /// The request type.
        /// </value>
        public WriteRequestType RequestType
        {
            get { return _requestType; }
        }
    }
}

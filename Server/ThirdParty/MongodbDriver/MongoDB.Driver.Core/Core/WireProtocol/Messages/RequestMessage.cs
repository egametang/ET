/* Copyright 2013-2015 MongoDB Inc.
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
using System.Threading;

namespace MongoDB.Driver.Core.WireProtocol.Messages
{
    /// <summary>
    /// Represents a base class for request messages.
    /// </summary>
    public abstract class RequestMessage : MongoDBMessage
    {
        #region static
        // static fields
        private static int __requestId;

        // static properties
        /// <summary>
        /// Gets the current global request identifier.
        /// </summary>
        /// <value>
        /// The current global request identifier.
        /// </value>
        public static int CurrentGlobalRequestId
        {
            get { return __requestId; }
        }

        // static methods
        /// <summary>
        /// Gets the next request identifier.
        /// </summary>
        /// <returns>The next request identifier.</returns>
        public static int GetNextRequestId()
        {
            return Interlocked.Increment(ref __requestId);
        }
        #endregion

        // fields
        private readonly int _requestId;
        private readonly Func<bool> _shouldBeSent;
        private bool _wasSent;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="RequestMessage"/> class.
        /// </summary>
        /// <param name="requestId">The request identifier.</param>
        /// <param name="shouldBeSent">A delegate that determines whether this message should be sent.</param>
        protected RequestMessage(int requestId, Func<bool> shouldBeSent = null)
        {
            _requestId = requestId;
            _shouldBeSent = shouldBeSent;
        }

        // properties
        /// <summary>
        /// Gets the request identifier.
        /// </summary>
        /// <value>
        /// The request identifier.
        /// </value>
        public int RequestId
        {
            get { return _requestId; }
        }

        /// <summary>
        /// Gets a delegate that determines whether this message should be sent.
        /// </summary>
        /// <value>
        /// A delegate that determines whether this message be sent.
        /// </value>
        public Func<bool> ShouldBeSent
        {
            get { return _shouldBeSent; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this message was sent.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this message was sent; otherwise, <c>false</c>.
        /// </value>
        public bool WasSent
        {
            get { return _wasSent; }
            set { _wasSent = value; }
        }
    }
}

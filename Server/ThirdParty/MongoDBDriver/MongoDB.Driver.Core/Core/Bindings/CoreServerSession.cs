/* Copyright 2017-present MongoDB Inc.
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

namespace MongoDB.Driver
{
    /// <summary>
    /// A server session.
    /// </summary>
    /// <seealso cref="MongoDB.Driver.ICoreServerSession" />
    internal sealed class CoreServerSession : ICoreServerSession
    {
        #region static
        // private static methods
        private static BsonDocument GenerateSessionId()
        {
            var guid = Guid.NewGuid();
            var id = new BsonBinaryData(guid, GuidRepresentation.Standard);
            return new BsonDocument("id", id);
        }
        #endregion

        // private fields
        private readonly BsonDocument _id;
        private DateTime? _lastUsedAt;
        private long _transactionNumber;

        // constructors
        public CoreServerSession()
        {
            _id = GenerateSessionId();
            _transactionNumber = 0;
        }

        // public properties
        /// <inheritdoc />
        public BsonDocument Id => _id;

        /// <inheritdoc />
        public DateTime? LastUsedAt => _lastUsedAt;

        // public methods
        /// <inheritdoc />
        public long AdvanceTransactionNumber()
        {
            return ++_transactionNumber;
        }

        /// <inheritdoc />
        public void Dispose()
        {
        }

        /// <inheritdoc />
        public void WasUsed()
        {
            _lastUsedAt = DateTime.UtcNow;
        }
    }
}

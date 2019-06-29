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
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver
{
    /// <summary>
    /// A server session.
    /// </summary>
    /// <seealso cref="MongoDB.Driver.IServerSession" />
    internal sealed class ServerSession : IServerSession
    {
        // private fields
        private readonly ICoreServerSession _coreServerSession;

        // constructors
        public ServerSession(ICoreServerSession coreServerSession)
        {
            _coreServerSession = Ensure.IsNotNull(coreServerSession, nameof(coreServerSession));
        }

        // public properties
        /// <inheritdoc />
        public BsonDocument Id => _coreServerSession.Id;

        /// <inheritdoc />
        public DateTime? LastUsedAt => _coreServerSession.LastUsedAt;

        // public methods
        /// <inheritdoc />
        [Obsolete("Let the driver handle when to advance the transaction number.")]
        public long AdvanceTransactionNumber()
        {
            // do nothing
            return -1;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            // do nothing (the ServerSession does NOT own the wrapped core server session)
        }

        /// <inheritdoc />
        [Obsolete("Let the driver handle tracking when the session was last used.")]
        public void WasUsed()
        {
            // do nothing
        }
    }
}

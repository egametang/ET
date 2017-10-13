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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders;

namespace MongoDB.Driver.Core.WireProtocol.Messages
{
    /// <summary>
    /// Represents a KillCursors message.
    /// </summary>
    public class KillCursorsMessage : RequestMessage
    {
        // fields
        private readonly List<long> _cursorIds;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="KillCursorsMessage"/> class.
        /// </summary>
        /// <param name="requestId">The request identifier.</param>
        /// <param name="cursorIds">The cursor ids.</param>
        public KillCursorsMessage(
            int requestId,
            IEnumerable<long> cursorIds)
            : base(requestId)
        {
            _cursorIds = Ensure.IsNotNull(cursorIds, nameof(cursorIds)).ToList();
        }

        // properties
        /// <summary>
        /// Gets the cursor ids.
        /// </summary>
        public IReadOnlyList<long> CursorIds
        {
            get { return _cursorIds; }
        }

        /// <inheritdoc/>
        public override MongoDBMessageType MessageType
        {
            get { return MongoDBMessageType.KillCursors; }
        }

        // methods
        /// <inheritdoc/>
        public override IMessageEncoder GetEncoder(IMessageEncoderFactory encoderFactory)
        {
            return encoderFactory.GetKillCursorsMessageEncoder();
        }
    }
}

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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.WireProtocol.Messages;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders;

namespace MongoDB.Driver.Core.WireProtocol
{
    internal class KillCursorsWireProtocol : IWireProtocol
    {
        // fields
        private readonly IReadOnlyList<long> _cursorIds;
        private readonly MessageEncoderSettings _messageEncoderSettings;

        // constructors
        public KillCursorsWireProtocol(IEnumerable<long> cursorIds, MessageEncoderSettings messageEncoderSettings)
        {
            Ensure.IsNotNull(cursorIds, nameof(cursorIds));
            _cursorIds = (cursorIds as IReadOnlyList<long>) ?? cursorIds.ToList();
            _messageEncoderSettings = messageEncoderSettings;
        }

        // methods
        private KillCursorsMessage CreateMessage()
        {
            return new KillCursorsMessage(RequestMessage.GetNextRequestId(), _cursorIds);
        }

        public void Execute(IConnection connection, CancellationToken cancellationToken)
        {
            var message = CreateMessage();
            connection.SendMessage(message, _messageEncoderSettings, cancellationToken);
        }

        public async Task ExecuteAsync(IConnection connection, CancellationToken cancellationToken)
        {
            var message = CreateMessage();
            await connection.SendMessageAsync(message, _messageEncoderSettings, cancellationToken).ConfigureAwait(false);
        }
    }
}

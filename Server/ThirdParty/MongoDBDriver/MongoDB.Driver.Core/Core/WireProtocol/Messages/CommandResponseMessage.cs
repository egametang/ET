/* Copyright 2018-present MongoDB Inc.
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
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders;

namespace MongoDB.Driver.Core.WireProtocol.Messages
{
    /// <summary>
    /// Represents a command response message.
    /// </summary>
    /// <seealso cref="MongoDB.Driver.Core.WireProtocol.Messages.RequestMessage" />
    public class CommandResponseMessage : ResponseMessage
    {
        // private fields
        private readonly CommandMessage _wrappedMessage;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandRequestMessage" /> class.
        /// </summary>
        /// <param name="wrappedMessage">The wrapped message.</param>
        public CommandResponseMessage(CommandMessage wrappedMessage)
            : base(Ensure.IsNotNull(wrappedMessage, nameof(wrappedMessage)).RequestId, wrappedMessage.ResponseTo)
        {
            _wrappedMessage = Ensure.IsNotNull(wrappedMessage, nameof(wrappedMessage));
        }

        // public properties
        /// <inheritdoc />
        public override MongoDBMessageType MessageType => _wrappedMessage.MessageType;

        /// <summary>
        /// Gets the wrapped message.
        /// </summary>
        /// <value>
        /// The wrapped message.
        /// </value>
        public CommandMessage WrappedMessage => _wrappedMessage;

        // public methods
        /// <inheritdoc />
        public override IMessageEncoder GetEncoder(IMessageEncoderFactory encoderFactory)
        {
            return encoderFactory.GetCommandResponseMessageEncoder();
        }
    }
}

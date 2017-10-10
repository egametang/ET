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
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Core.Configuration;
using MongoDB.Driver.Core.WireProtocol.Messages;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders;

namespace MongoDB.Driver.Core.Connections
{
    /// <summary>
    /// Represents a connection.
    /// </summary>
    public interface IConnection : IDisposable
    {
        // properties
        /// <summary>
        /// Gets the connection identifier.
        /// </summary>
        /// <value>
        /// The connection identifier.
        /// </value>
        ConnectionId ConnectionId { get;}

        /// <summary>
        /// Gets the connection description.
        /// </summary>
        /// <value>
        /// The connection description.
        /// </value>
        ConnectionDescription Description { get; }

        /// <summary>
        /// Gets the end point.
        /// </summary>
        /// <value>
        /// The end point.
        /// </value>
        EndPoint EndPoint { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is expired.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is expired; otherwise, <c>false</c>.
        /// </value>
        bool IsExpired { get; }

        /// <summary>
        /// Gets the connection settings.
        /// </summary>
        /// <value>
        /// The connection settings.
        /// </value>
        ConnectionSettings Settings { get; }

        // methods
        /// <summary>
        /// Opens the connection.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        void Open(CancellationToken cancellationToken);

        /// <summary>
        /// Opens the connection.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task.</returns>
        Task OpenAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Receives a message.
        /// </summary>
        /// <param name="responseTo">The id of the sent message for which a response is to be received.</param>
        /// <param name="encoderSelector">The encoder selector.</param>
        /// <param name="messageEncoderSettings">The message encoder settings.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The response message.
        /// </returns>
        ResponseMessage ReceiveMessage(int responseTo, IMessageEncoderSelector encoderSelector, MessageEncoderSettings messageEncoderSettings, CancellationToken cancellationToken);

        /// <summary>
        /// Receives a message.
        /// </summary>
        /// <param name="responseTo">The id of the sent message for which a response is to be received.</param>
        /// <param name="encoderSelector">The encoder selector.</param>
        /// <param name="messageEncoderSettings">The message encoder settings.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A Task whose result is the response message.
        /// </returns>
        Task<ResponseMessage> ReceiveMessageAsync(int responseTo, IMessageEncoderSelector encoderSelector, MessageEncoderSettings messageEncoderSettings, CancellationToken cancellationToken);

        /// <summary>
        /// Sends the messages.
        /// </summary>
        /// <param name="messages">The messages.</param>
        /// <param name="messageEncoderSettings">The message encoder settings.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        void SendMessages(IEnumerable<RequestMessage> messages, MessageEncoderSettings messageEncoderSettings, CancellationToken cancellationToken);

        /// <summary>
        /// Sends the messages.
        /// </summary>
        /// <param name="messages">The messages.</param>
        /// <param name="messageEncoderSettings">The message encoder settings.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task.</returns>
        Task SendMessagesAsync(IEnumerable<RequestMessage> messages, MessageEncoderSettings messageEncoderSettings, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Represents a handle to a connection.
    /// </summary>
    public interface IConnectionHandle : IConnection
    {
        // methods
        /// <summary>
        /// A new handle to the underlying connection.
        /// </summary>
        /// <returns>A connection handle.</returns>
        IConnectionHandle Fork();
    }
}

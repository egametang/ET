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
using System.Net.Sockets;
using System.Threading;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Core.Configuration
{
    /// <summary>
    /// Represents settings for a TCP stream.
    /// </summary>
    public class TcpStreamSettings
    {
        // fields
        private readonly AddressFamily _addressFamily;
        private readonly TimeSpan _connectTimeout;
        private readonly TimeSpan? _readTimeout;
        private readonly int _receiveBufferSize;
        private readonly int _sendBufferSize;
        private readonly Action<Socket> _socketConfigurator;
        private readonly TimeSpan? _writeTimeout;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="TcpStreamSettings"/> class.
        /// </summary>
        /// <param name="addressFamily">The address family.</param>
        /// <param name="connectTimeout">The connect timeout.</param>
        /// <param name="readTimeout">The read timeout.</param>
        /// <param name="receiveBufferSize">Size of the receive buffer.</param>
        /// <param name="sendBufferSize">Size of the send buffer.</param>
        /// <param name="socketConfigurator">The socket configurator.</param>
        /// <param name="writeTimeout">The write timeout.</param>
        public TcpStreamSettings(
            Optional<AddressFamily> addressFamily = default(Optional<AddressFamily>),
            Optional<TimeSpan> connectTimeout = default(Optional<TimeSpan>),
            Optional<TimeSpan?> readTimeout = default(Optional<TimeSpan?>),
            Optional<int> receiveBufferSize = default(Optional<int>),
            Optional<int> sendBufferSize = default(Optional<int>),
            Optional<Action<Socket>> socketConfigurator = default(Optional<Action<Socket>>),
            Optional<TimeSpan?> writeTimeout = default(Optional<TimeSpan?>))
        {
            _addressFamily = addressFamily.WithDefault(AddressFamily.InterNetwork);
            _connectTimeout = Ensure.IsInfiniteOrGreaterThanOrEqualToZero(connectTimeout.WithDefault(Timeout.InfiniteTimeSpan), "connectTimeout");
            _readTimeout = Ensure.IsNullOrInfiniteOrGreaterThanOrEqualToZero(readTimeout.WithDefault(null), "readTimeout");
            _receiveBufferSize = Ensure.IsGreaterThanZero(receiveBufferSize.WithDefault(64 * 1024), "receiveBufferSize");
            _sendBufferSize = Ensure.IsGreaterThanZero(sendBufferSize.WithDefault(64 * 1024), "sendBufferSize");
            _socketConfigurator = socketConfigurator.WithDefault(null);
            _writeTimeout = Ensure.IsNullOrInfiniteOrGreaterThanOrEqualToZero(writeTimeout.WithDefault(null), "writeTimeout");
        }

        // properties
        /// <summary>
        /// Gets the address family.
        /// </summary>
        /// <value>
        /// The address family.
        /// </value>
        public AddressFamily AddressFamily
        {
            get { return _addressFamily; }
        }

        /// <summary>
        /// Gets the connect timeout.
        /// </summary>
        /// <value>
        /// The connect timeout.
        /// </value>
        public TimeSpan ConnectTimeout
        {
            get { return _connectTimeout; }
        }

        /// <summary>
        /// Gets the read timeout.
        /// </summary>
        /// <value>
        /// The read timeout.
        /// </value>
        public TimeSpan? ReadTimeout
        {
            get { return _readTimeout; }
        }

        /// <summary>
        /// Gets the size of the receive buffer.
        /// </summary>
        /// <value>
        /// The size of the receive buffer.
        /// </value>
        public int ReceiveBufferSize
        {
            get { return _receiveBufferSize; }
        }

        /// <summary>
        /// Gets the size of the send buffer.
        /// </summary>
        /// <value>
        /// The size of the send buffer.
        /// </value>
        public int SendBufferSize
        {
            get { return _sendBufferSize; }
        }

        /// <summary>
        /// Gets the socket configurator.
        /// </summary>
        /// <value>
        /// The socket configurator.
        /// </value>
        public Action<Socket> SocketConfigurator
        {
            get { return _socketConfigurator; }
        }

        /// <summary>
        /// Gets the write timeout.
        /// </summary>
        /// <value>
        /// The write timeout.
        /// </value>
        public TimeSpan? WriteTimeout
        {
            get { return _writeTimeout; }
        }

        // methods
        /// <summary>
        /// Returns a new TcpStreamSettings instance with some settings changed.
        /// </summary>
        /// <param name="addressFamily">The address family.</param>
        /// <param name="connectTimeout">The connect timeout.</param>
        /// <param name="readTimeout">The read timeout.</param>
        /// <param name="receiveBufferSize">Size of the receive buffer.</param>
        /// <param name="sendBufferSize">Size of the send buffer.</param>
        /// <param name="socketConfigurator">The socket configurator.</param>
        /// <param name="writeTimeout">The write timeout.</param>
        /// <returns>A new TcpStreamSettings instance.</returns>
        public TcpStreamSettings With(
            Optional<AddressFamily> addressFamily = default(Optional<AddressFamily>),
            Optional<TimeSpan> connectTimeout = default(Optional<TimeSpan>),
            Optional<TimeSpan?> readTimeout = default(Optional<TimeSpan?>),
            Optional<int> receiveBufferSize = default(Optional<int>),
            Optional<int> sendBufferSize = default(Optional<int>),
            Optional<Action<Socket>> socketConfigurator = default(Optional<Action<Socket>>),
            Optional<TimeSpan?> writeTimeout = default(Optional<TimeSpan?>))
        {
            return new TcpStreamSettings(
                addressFamily: addressFamily.WithDefault(_addressFamily),
                connectTimeout: connectTimeout.WithDefault(_connectTimeout),
                readTimeout: readTimeout.WithDefault(_readTimeout),
                receiveBufferSize: receiveBufferSize.WithDefault(_receiveBufferSize),
                sendBufferSize: sendBufferSize.WithDefault(_sendBufferSize),
                socketConfigurator: socketConfigurator.WithDefault(_socketConfigurator),
                writeTimeout: writeTimeout.WithDefault(_writeTimeout));
        }
    }
}

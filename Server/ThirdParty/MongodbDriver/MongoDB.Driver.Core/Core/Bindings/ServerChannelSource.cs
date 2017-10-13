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
using System.Threading.Tasks;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.Servers;

namespace MongoDB.Driver.Core.Bindings
{
    /// <summary>
    /// Represents a channel source that is bound to a server.
    /// </summary>
    public sealed class ServerChannelSource : IChannelSource
    {
        // fields
        private bool _disposed;
        private readonly IServer _server;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ServerChannelSource"/> class.
        /// </summary>
        /// <param name="server">The server.</param>
        public ServerChannelSource(IServer server)
        {
            _server = Ensure.IsNotNull(server, nameof(server));
        }

        // properties
        /// <inheritdoc/>
        public IServer Server
        {
            get { return _server; }
        }

        /// <inheritdoc/>
        public ServerDescription ServerDescription
        {
            get { return _server.Description; }
        }

        // methods
        /// <inheritdoc/>
        public IChannelHandle GetChannel(CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            return _server.GetChannel(cancellationToken);
        }

        /// <inheritdoc/>
        public Task<IChannelHandle> GetChannelAsync(CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            return _server.GetChannelAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                GC.SuppressFinalize(this);
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }
    }
}
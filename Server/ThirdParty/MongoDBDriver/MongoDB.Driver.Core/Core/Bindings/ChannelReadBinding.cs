/* Copyright 2013-present MongoDB Inc.
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
using MongoDB.Driver.Core.Clusters;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.Servers;

namespace MongoDB.Driver.Core.Bindings
{
    /// <summary>
    /// Represents a read binding that is bound to a channel.
    /// </summary>
    public sealed class ChannelReadBinding : IReadBinding
    {
        // fields
        private readonly IChannelHandle _channel;
        private bool _disposed;
        private readonly ReadPreference _readPreference;
        private readonly IServer _server;
        private readonly ICoreSessionHandle _session;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelReadBinding" /> class.
        /// </summary>
        /// <param name="server">The server.</param>
        /// <param name="channel">The channel.</param>
        /// <param name="readPreference">The read preference.</param>
        /// <param name="session">The session.</param>
        public ChannelReadBinding(IServer server, IChannelHandle channel, ReadPreference readPreference, ICoreSessionHandle session)
        {
            _server = Ensure.IsNotNull(server, nameof(server));
            _channel = Ensure.IsNotNull(channel, nameof(channel));
            _readPreference = Ensure.IsNotNull(readPreference, nameof(readPreference));
            _session = Ensure.IsNotNull(session, nameof(session));
        }

        // properties        
        /// <inheritdoc/>
        public ReadPreference ReadPreference
        {
            get { return _readPreference; }
        }

        /// <inheritdoc/>
        public ICoreSessionHandle Session
        {
            get { return _session; }
        }

        // methods
        /// <inheritdoc/>
        public void Dispose()
        {
            if (!_disposed)
            {
                _channel.Dispose();
                _session.Dispose();
                _disposed = true;
            }
        }

        /// <inheritdoc/>
        public IChannelSourceHandle GetReadChannelSource(CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            return GetReadChannelSourceHelper();
        }

        /// <inheritdoc/>
        public Task<IChannelSourceHandle> GetReadChannelSourceAsync(CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            return Task.FromResult<IChannelSourceHandle>(GetReadChannelSourceHelper());
        }

        private IChannelSourceHandle GetReadChannelSourceHelper()
        {
            return new ChannelSourceHandle(new ChannelChannelSource(_server, _channel.Fork(), _session.Fork()));
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }
    }
}

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
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.Servers;

namespace MongoDB.Driver.Core.Bindings
{
    /// <summary>
    /// Represents a read binding to a single server;
    /// </summary>
    public sealed class SingleServerReadBinding : IReadBinding
    {
        // fields
        private bool _disposed;
        private readonly ReadPreference _readPreference;
        private readonly IServer _server;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="SingleServerReadBinding" /> class.
        /// </summary>
        /// <param name="server">The server.</param>
        /// <param name="readPreference">The read preference.</param>
        public SingleServerReadBinding(IServer server, ReadPreference readPreference)
        {
            _server = Ensure.IsNotNull(server, nameof(server));
            _readPreference = Ensure.IsNotNull(readPreference, nameof(readPreference));
        }

        // properties
        /// <inheritdoc/>
        public ReadPreference ReadPreference
        {
            get { return _readPreference; }
        }

        // methods
        /// <inheritdoc/>
        public IChannelSourceHandle GetReadChannelSource(CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            return GetChannelSourceHelper();
        }

        /// <inheritdoc/>
        public Task<IChannelSourceHandle> GetReadChannelSourceAsync(CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            return Task.FromResult(GetChannelSourceHelper());
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

        private IChannelSourceHandle GetChannelSourceHelper()
        {
            return new ChannelSourceHandle(new ServerChannelSource(_server));
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

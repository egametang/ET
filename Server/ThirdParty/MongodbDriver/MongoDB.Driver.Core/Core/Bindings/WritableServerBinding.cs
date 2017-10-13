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
using MongoDB.Driver.Core.Clusters;
using MongoDB.Driver.Core.Clusters.ServerSelectors;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.Servers;

namespace MongoDB.Driver.Core.Bindings
{
    /// <summary>
    /// Represents a write binding to a writable server.
    /// </summary>
    public sealed class WritableServerBinding : IReadWriteBinding
    {
        // fields
        private readonly ICluster _cluster;
        private bool _disposed;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="WritableServerBinding"/> class.
        /// </summary>
        /// <param name="cluster">The cluster.</param>
        public WritableServerBinding(ICluster cluster)
        {
            _cluster = Ensure.IsNotNull(cluster, nameof(cluster));
        }

        // properties
        /// <inheritdoc/>
        public ReadPreference ReadPreference
        {
            get { return ReadPreference.Primary; }
        }

        // methods
        /// <inheritdoc/>
        public IChannelSourceHandle GetReadChannelSource(CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            var server = _cluster.SelectServer(WritableServerSelector.Instance, cancellationToken);
            return GetChannelSourceHelper(server);
        }

        /// <inheritdoc/>
        public async Task<IChannelSourceHandle> GetReadChannelSourceAsync(CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            var server = await _cluster.SelectServerAsync(WritableServerSelector.Instance, cancellationToken).ConfigureAwait(false);
            return GetChannelSourceHelper(server);
        }

        /// <inheritdoc/>
        public IChannelSourceHandle GetWriteChannelSource(CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            var server = _cluster.SelectServer(WritableServerSelector.Instance, cancellationToken);
            return GetChannelSourceHelper(server);
        }

        /// <inheritdoc/>
        public async Task<IChannelSourceHandle> GetWriteChannelSourceAsync(CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            var server = await _cluster.SelectServerAsync(WritableServerSelector.Instance, cancellationToken).ConfigureAwait(false);
            return GetChannelSourceHelper(server);
        }

        private IChannelSourceHandle GetChannelSourceHelper(IServer server)
        {
            return new ChannelSourceHandle(new ServerChannelSource(server));
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

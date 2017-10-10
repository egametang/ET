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
    /// Represents a read binding to a cluster using a ReadPreference to select the server.
    /// </summary>
    public sealed class ReadPreferenceBinding : IReadBinding
    {
        // fields
        private readonly ICluster _cluster;
        private bool _disposed;
        private readonly ReadPreference _readPreference;
        private readonly IServerSelector _serverSelector;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadPreferenceBinding"/> class.
        /// </summary>
        /// <param name="cluster">The cluster.</param>
        /// <param name="readPreference">The read preference.</param>
        public ReadPreferenceBinding(ICluster cluster, ReadPreference readPreference)
        {
            _cluster = Ensure.IsNotNull(cluster, nameof(cluster));
            _readPreference = Ensure.IsNotNull(readPreference, nameof(readPreference));
            _serverSelector = new ReadPreferenceServerSelector(readPreference);
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
            var server = _cluster.SelectServer(_serverSelector, cancellationToken);
            return GetChannelSourceHelper(server);
        }

        /// <inheritdoc/>
        public async Task<IChannelSourceHandle> GetReadChannelSourceAsync(CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            var server = await _cluster.SelectServerAsync(_serverSelector, cancellationToken).ConfigureAwait(false);
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

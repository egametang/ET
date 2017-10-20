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
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Core.Bindings
{
    /// <summary>
    /// Represents a read-write binding to a channel source.
    /// </summary>
    public sealed class ChannelSourceReadWriteBinding : IReadWriteBinding
    {
        // fields
        private readonly IChannelSourceHandle _channelSource;
        private bool _disposed;
        private readonly ReadPreference _readPreference;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelSourceReadWriteBinding"/> class.
        /// </summary>
        /// <param name="channelSource">The channel source.</param>
        /// <param name="readPreference">The read preference.</param>
        public ChannelSourceReadWriteBinding(IChannelSourceHandle channelSource, ReadPreference readPreference)
        {
            _channelSource = Ensure.IsNotNull(channelSource, nameof(channelSource));
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
        public IChannelSourceHandle GetWriteChannelSource(CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            return GetChannelSourceHelper();
        }

        /// <inheritdoc/>
        public Task<IChannelSourceHandle> GetWriteChannelSourceAsync(CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            return Task.FromResult(GetChannelSourceHelper());
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (!_disposed)
            {
                _channelSource.Dispose();
                _disposed = true;
                GC.SuppressFinalize(this);
            }
        }

        private IChannelSourceHandle GetChannelSourceHelper()
        {
            return _channelSource.Fork();
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

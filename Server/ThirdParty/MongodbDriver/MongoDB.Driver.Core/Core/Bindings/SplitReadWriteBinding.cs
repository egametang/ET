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
    /// Represents a split read-write binding, where the reads use one binding and the writes use another.
    /// </summary>
    public sealed class SplitReadWriteBinding : IReadWriteBinding
    {
        // fields
        private bool _disposed;
        private readonly IReadBinding _readBinding;
        private readonly IWriteBinding _writeBinding;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="SplitReadWriteBinding"/> class.
        /// </summary>
        /// <param name="readBinding">The read binding.</param>
        /// <param name="writeBinding">The write binding.</param>
        public SplitReadWriteBinding(IReadBinding readBinding, IWriteBinding writeBinding)
        {
            _readBinding = Ensure.IsNotNull(readBinding, nameof(readBinding));
            _writeBinding = Ensure.IsNotNull(writeBinding, nameof(writeBinding));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SplitReadWriteBinding"/> class.
        /// </summary>
        /// <param name="cluster">The cluster.</param>
        /// <param name="readPreference">The read preference.</param>
        public SplitReadWriteBinding(ICluster cluster, ReadPreference readPreference)
            : this(new ReadPreferenceBinding(cluster, readPreference), new WritableServerBinding(cluster))
        {
        }

        // properties
        /// <inheritdoc/>
        public ReadPreference ReadPreference
        {
            get { return _readBinding.ReadPreference; }
        }

        // methods
        /// <inheritdoc/>
        public IChannelSourceHandle GetReadChannelSource(CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            return _readBinding.GetReadChannelSource(cancellationToken);
        }

        /// <inheritdoc/>
        public Task<IChannelSourceHandle> GetReadChannelSourceAsync(CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            return _readBinding.GetReadChannelSourceAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public IChannelSourceHandle GetWriteChannelSource(CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            return _writeBinding.GetWriteChannelSource(cancellationToken);
        }

        /// <inheritdoc/>
        public Task<IChannelSourceHandle> GetWriteChannelSourceAsync(CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            return _writeBinding.GetWriteChannelSourceAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (!_disposed)
            {
                _readBinding.Dispose();
                _writeBinding.Dispose();
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

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
    /// Represents a handle to a channel source.
    /// </summary>
    public sealed class ChannelSourceHandle : IChannelSourceHandle
    {
        // fields
        private bool _disposed;
        private readonly ReferenceCounted<IChannelSource> _reference;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelSourceHandle"/> class.
        /// </summary>
        /// <param name="channelSource">The channel source.</param>
        public ChannelSourceHandle(IChannelSource channelSource)
            : this(new ReferenceCounted<IChannelSource>(channelSource))
        {
        }

        private ChannelSourceHandle(ReferenceCounted<IChannelSource> reference)
        {
            _reference = reference;
        }

        // properties
        /// <inheritdoc/>
        public IServer Server
        {
            get { return _reference.Instance.Server; }
        }

        /// <inheritdoc/>
        public ServerDescription ServerDescription
        {
            get { return _reference.Instance.ServerDescription; }
        }

        // methods
        /// <inheritdoc/>
        public IChannelHandle GetChannel(CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            return _reference.Instance.GetChannel(cancellationToken);
        }

        /// <inheritdoc/>
        public Task<IChannelHandle> GetChannelAsync(CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            return _reference.Instance.GetChannelAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (!_disposed)
            {
                _reference.DecrementReferenceCount();
                _disposed = true;
                GC.SuppressFinalize(this);
            }
        }

        /// <inheritdoc/>
        public IChannelSourceHandle Fork()
        {
            ThrowIfDisposed();
            _reference.IncrementReferenceCount();
            return new ChannelSourceHandle(_reference);
        }

        private void ThrowIfDisposed()
        {
            if(_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }
    }
}

/* Copyright 2010-2015 MongoDB Inc.
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
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver
{
    internal sealed class SingleBatchAsyncCursor<T> : IAsyncCursor<T>
    {
        private bool _disposed;
        private bool _moved;
        private readonly IReadOnlyList<T> _current;

        public SingleBatchAsyncCursor(IReadOnlyList<T> current)
        {
            _current = Ensure.IsNotNull(current, nameof(current));
        }

        public IEnumerable<T> Current
        {
            get
            {
                ThrowIfDisposed();
                if (!_moved)
                {
                    throw new InvalidOperationException("Must call MoveNextAsync first");
                }
                return _current;
            }
        }

        // public methods
        public bool MoveNext(CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            if (_moved)
            {
                return false;
            }
            _moved = true;
            return true;
        }

        public Task<bool> MoveNextAsync(CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            return Task.FromResult(MoveNext(cancellationToken));
        }

        public void Dispose()
        {
            _disposed = true;
        }

        // private methods
        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }
    }
}

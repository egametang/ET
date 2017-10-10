/* Copyright 2015 MongoDB Inc.
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
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Core.Operations
{
    internal class AsyncCursorEnumerator<TDocument> : IEnumerator<TDocument>
    {
        // private fields
        private IEnumerator<TDocument> _batchEnumerator;
        private readonly CancellationToken _cancellationToken;
        private readonly IAsyncCursor<TDocument> _cursor;
        private bool _disposed;
        private bool _finished;
        private bool _started;

        // constructors
        public AsyncCursorEnumerator(IAsyncCursor<TDocument> cursor, CancellationToken cancellationToken)
        {
            _cursor = Ensure.IsNotNull(cursor, nameof(cursor));
            _cancellationToken = cancellationToken;
        }

        // public properties
        public TDocument Current
        {
            get
            {
                ThrowIfDisposed();
                if (!_started)
                {
                    throw new InvalidOperationException("Enumeration has not started. Call MoveNext.");
                }
                if (_finished)
                {
                    throw new InvalidOperationException("Enumeration already finished.");
                }
                return _batchEnumerator.Current;
            }
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }

        // public properties
        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _batchEnumerator?.Dispose();
                _cursor.Dispose();
            }
        }

        public bool MoveNext()
        {
            ThrowIfDisposed();
            _started = true;

            if (_batchEnumerator != null && _batchEnumerator.MoveNext())
            {
                return true;
            }

            while (true)
            {
                if (_cursor.MoveNext(_cancellationToken))
                {
                    _batchEnumerator?.Dispose();
                    _batchEnumerator = _cursor.Current.GetEnumerator();
                    if (_batchEnumerator.MoveNext())
                    {
                        return true;
                    }
                }
                else
                {
                    _batchEnumerator = null;
                    _finished = true;
                    return false;
                }
            }
        }

        public void Reset()
        {
            ThrowIfDisposed();
            throw new NotSupportedException();
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

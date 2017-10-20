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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver
{
    /// <summary>
    /// Represents a cursor for an operation that is not actually executed until MoveNextAsync is called for the first time.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document.</typeparam>
    public sealed class DeferredAsyncCursor<TDocument> : IAsyncCursor<TDocument>
    {
        // fields
        private readonly Func<CancellationToken, IAsyncCursor<TDocument>> _execute;
        private readonly Func<CancellationToken, Task<IAsyncCursor<TDocument>>> _executeAsync;
        private IAsyncCursor<TDocument> _cursor;
        private bool _disposed;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="DeferredAsyncCursor{TDocument}"/> class.
        /// </summary>
        /// <param name="execute">The delegate to execute the first time MoveNext is called.</param>
        /// <param name="executeAsync">The delegate to execute the first time MoveNextAsync is called.</param>
        public DeferredAsyncCursor(
            Func<CancellationToken, IAsyncCursor<TDocument>> execute,
            Func<CancellationToken, Task<IAsyncCursor<TDocument>>> executeAsync)
        {
            _execute = Ensure.IsNotNull(execute, nameof(execute));
            _executeAsync = Ensure.IsNotNull(executeAsync, nameof(executeAsync));
        }

        // properties
        /// <inheritdoc/>
        public IEnumerable<TDocument> Current
        {
            get
            {
                ThrowIfDisposed();
                if (_cursor == null)
                {
                    throw new InvalidOperationException("Enumeration has not started. Call MoveNextAsync.");
                }

                return _cursor.Current;
            }
        }

        // methods
        /// <inheritdoc/>
        public bool MoveNext(CancellationToken cancellationToken)
        {
            ThrowIfDisposed();

            if (_cursor == null)
            {
                _cursor = _execute(cancellationToken);
            }

            return _cursor.MoveNext(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<bool> MoveNextAsync(CancellationToken cancellationToken)
        {
            ThrowIfDisposed();

            if (_cursor == null)
            {
                _cursor = await _executeAsync(cancellationToken).ConfigureAwait(false);
            }

            return await _cursor.MoveNextAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_cursor != null)
            {
                _cursor.Dispose();
                _cursor = null;
                _disposed = true;
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

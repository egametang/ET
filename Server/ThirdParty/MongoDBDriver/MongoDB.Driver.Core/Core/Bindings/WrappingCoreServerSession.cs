/* Copyright 2017-present MongoDB Inc.
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
using MongoDB.Bson;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver
{
    internal abstract class WrappingCoreServerSession : ICoreServerSession
    {
        // private fields
        protected bool _disposed;
        private readonly bool _ownsWrapped;
        private readonly ICoreServerSession _wrapped;

        // constructors
        public WrappingCoreServerSession(ICoreServerSession wrapped, bool ownsWrapped)
        {
            _wrapped = Ensure.IsNotNull(wrapped, nameof(wrapped));
            _ownsWrapped = ownsWrapped;
        }

        // public properties
        public BsonDocument Id
        {
            get
            {
                ThrowIfDisposed();
                return _wrapped.Id;
            }
        }

        public DateTime? LastUsedAt
        {
            get
            {
                ThrowIfDisposed();
                return _wrapped.LastUsedAt;
            }
        }

        public ICoreServerSession Wrapped
        {
            get
            {
                ThrowIfDisposed();
                return _wrapped;
            }
        }

        // public methods
        public long AdvanceTransactionNumber()
        {
            ThrowIfDisposed();
            return _wrapped.AdvanceTransactionNumber();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void WasUsed()
        {
            ThrowIfDisposed();
            _wrapped.WasUsed();
        }

        // protected methods
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_ownsWrapped)
                    {
                        _wrapped.Dispose();
                    }
                }
                _disposed = true;
            }
        }

        protected void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }
    }
}

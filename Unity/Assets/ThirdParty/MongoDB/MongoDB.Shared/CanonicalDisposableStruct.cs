/* Copyright 2010-2014 MongoDB Inc.
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

namespace MongoDB.Shared
{
    /// <summary>
    /// Represents a struct that owns one or more disposable resources.
    /// </summary>
    public struct CanonicalDisposableStruct : IDisposable
    {
        // private fields
        private bool _disposed;
        private IDisposable _disposableResource;

        /// <summary>
        /// Initializes a new instance of the <see cref="CanonicalDisposableStruct"/> struct.
        /// </summary>
        /// <param name="disposableResource">A disposable resource.</param>
        public CanonicalDisposableStruct(IDisposable disposableResource)
        {
            _disposed = false;
            _disposableResource = disposableResource;
        }

        // NOTE: structs can't have finalizers

        // public methods
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            // no need to call GC.SuppressFinalize for structs
        }

        /// <summary>
        /// Some method.
        /// </summary>
        public void SomeMethod()
        {
            ThrowIfDisposed();
            // ...
        }

        // private methods
        private void Dispose(bool disposing)
        {
            // this method can be called multiple times
            // make sure your implementation of this method does not throw any exceptions
            if (!_disposed)
            {
                if (disposing)
                {
                    // dispose of any managed disposable resources you own here
                    if (_disposableResource != null)
                    {
                        _disposableResource.Dispose();
                        _disposableResource = null; // not strictly necessary but a good idea
                    }
                }

                // dispose of any unmanaged resources here

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

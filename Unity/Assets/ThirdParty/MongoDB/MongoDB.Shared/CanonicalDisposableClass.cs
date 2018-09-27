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
    /// Represents a class that owns one or more disposable resources.
    /// </summary>
    public class CanonicalDisposableClass : IDisposable
    {
        // private fields
        private bool _disposed;
        private IDisposable _disposableResource;

        /// <summary>
        /// Initializes a new instance of the <see cref="CanonicalDisposableClass"/> class.
        /// </summary>
        /// <param name="disposableResource">A disposable resource.</param>
        public CanonicalDisposableClass(IDisposable disposableResource)
        {
            _disposableResource = disposableResource;
        }

        // NOTE: only implement a finalizer if you MUST
        //~CanonicalDisposableClass()
        //{
        //    Dispose(false);
        //}

        // protected properties
        /// <summary>
        /// Gets a value indicating whether this <see cref="CanonicalDisposableClass"/> is disposed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if disposed; otherwise, <c>false</c>.
        /// </value>
        protected bool Disposed
        {
            // only implement this property if you anticipate having derived classes
            // CLR compliance prevents us making the field itself protected because of the leading underscore
            get { return _disposed; }
        }

        // public methods
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this); // in case a derived class has a finalizer
        }

        /// <summary>
        /// Some method.
        /// </summary>
        public void SomeMethod()
        {
            ThrowIfDisposed();
            // ...
        }

        // protected methods
        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
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

        /// <summary>
        /// Throws if disposed.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException"></exception>
        protected void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }
    }
}

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
    /// Represents a class derived from an IDisposable class and that itself owns one ore more disposable resources.
    /// </summary>
    public class CanonicalDisposableDerivedClass : CanonicalDisposableClass
    {
        // private fields
        private IDisposable _anotherDisposableResource;

        /// <summary>
        /// Initializes a new instance of the <see cref="CanonicalDisposableDerivedClass"/> class.
        /// </summary>
        /// <param name="disposableResource">A disposable resource.</param>
        /// <param name="anotherDisposableResource">Another disposable resource.</param>
        public CanonicalDisposableDerivedClass(IDisposable disposableResource, IDisposable anotherDisposableResource)
            : base(disposableResource)
        {
            _anotherDisposableResource = anotherDisposableResource;
        }

        // NOTE: only implement a finalizer if you MUST (and only if the base class does not)
        //~CanonicalDisposableDerivedClass()
        //{
        //    Dispose(false);
        //}

        // public methods
        /// <summary>
        /// Another method.
        /// </summary>
        public void AnotherMethod()
        {
            ThrowIfDisposed();
            // ...
        }

        // protected methods
        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            // this method can be called multiple times
            // make sure your implementation of this method does not throw any exceptions
            if (!Disposed)
            {
                if (disposing)
                {
                    // dispose of any managed disposable resources you own here
                    if (_anotherDisposableResource != null)
                    {
                        _anotherDisposableResource.Dispose();
                        _anotherDisposableResource = null; // not strictly necessary but a good idea
                    }
                }

                // dispose of any unmanaged resources here
            }

            base.Dispose(disposing); // call base Dispose last
        }
    }
}

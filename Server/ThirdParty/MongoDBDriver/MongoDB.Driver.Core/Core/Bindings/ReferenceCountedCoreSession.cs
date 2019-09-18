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

namespace MongoDB.Driver.Core.Bindings
{
    /// <summary>
    /// A reference counted core session.
    /// </summary>
    /// <seealso cref="MongoDB.Driver.Core.Bindings.WrappingCoreSession" />
    internal sealed class ReferenceCountedCoreSession : WrappingCoreSession
    {
        // private fields
        private readonly object _lock = new object();
        private int _referenceCount;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceCountedCoreSession"/> class.
        /// </summary>
        /// <param name="wrapped">The wrapped.</param>
        public ReferenceCountedCoreSession(ICoreSession wrapped)
            : base(wrapped, ownsWrapped: true)
        {
            _referenceCount = 1;
        }

        // public methods
        /// <summary>
        /// Decrements the reference count.
        /// </summary>
        public void DecrementReferenceCount()
        {
            lock (_lock)
            {
                ThrowIfDisposed();
                if (--_referenceCount == 0)
                {
                    Dispose();
                }
            }
        }

        /// <summary>
        /// Increments the reference count.
        /// </summary>
        public void IncrementReferenceCount()
        {
            lock (_lock)
            {
                ThrowIfDisposed();
                _referenceCount++;
            }
        }
    }
}

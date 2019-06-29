/* Copyright 2018-present MongoDB Inc.
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
    /// A handle to a core session that should not be disposed when the handle is disposed.
    /// </summary>
    /// <seealso cref="MongoDB.Driver.Core.Bindings.ICoreSessionHandle" />
    internal sealed class NonDisposingCoreSessionHandle : WrappingCoreSession, ICoreSessionHandle
    {
        // private fields
        private readonly ICoreSession _wrapped;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="NonDisposingCoreSessionHandle" /> class.
        /// </summary>
        /// <param name="wrapped">The wrapped session.</param>
        public NonDisposingCoreSessionHandle(ICoreSession wrapped)
            : base(wrapped, ownsWrapped: false)
        {
            _wrapped = wrapped;
        }

        // public methods
        /// <inheritdoc />
        public ICoreSessionHandle Fork()
        {
            ThrowIfDisposed();
            return new NonDisposingCoreSessionHandle(_wrapped);
        }

        // protected methods
        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}

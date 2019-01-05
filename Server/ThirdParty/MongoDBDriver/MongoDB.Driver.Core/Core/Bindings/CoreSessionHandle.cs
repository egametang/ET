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

using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Core.Bindings
{
    /// <summary>
    /// A handle to a reference counted core session.
    /// </summary>
    /// <seealso cref="MongoDB.Driver.Core.Bindings.ICoreSessionHandle" />
    public sealed class CoreSessionHandle : WrappingCoreSession, ICoreSessionHandle
    {
        // private fields
        private readonly ReferenceCountedCoreSession _wrapped;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreSessionHandle"/> class.
        /// </summary>
        /// <param name="session">The session.</param>
        public CoreSessionHandle(ICoreSession session)
            : this(new ReferenceCountedCoreSession(Ensure.IsNotNull(session, nameof(session))))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreSessionHandle"/> class.
        /// </summary>
        /// <param name="wrapped">The wrapped.</param>
        internal CoreSessionHandle(ReferenceCountedCoreSession wrapped)
            : base(Ensure.IsNotNull(wrapped, nameof(wrapped)), ownsWrapped: false)
        {
            _wrapped = wrapped;
        }

        // public methods
        /// <inheritdoc />
        public ICoreSessionHandle Fork()
        {
            ThrowIfDisposed();
            _wrapped.IncrementReferenceCount();
            return new CoreSessionHandle(_wrapped);
        }

        // protected methods
        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!IsDisposed())
                {
                    _wrapped.DecrementReferenceCount();
                }
            }
            base.Dispose(disposing);
        }
    }
}

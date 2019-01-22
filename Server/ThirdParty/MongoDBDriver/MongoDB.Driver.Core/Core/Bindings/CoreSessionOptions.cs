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
    /// Core session options.
    /// </summary>
    public class CoreSessionOptions
    {
        // private fields
        private readonly TransactionOptions _defaultTransactionOptions;
        private readonly bool _isCausallyConsistent;
        private readonly bool _isImplicit;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreSessionOptions" /> class.
        /// </summary>
        /// <param name="isCausallyConsistent">if set to <c>true</c> this session is causally consistent]</param>
        /// <param name="isImplicit">if set to <c>true</c> this session is an implicit session.</param>
        /// <param name="defaultTransactionOptions">The default transaction options.</param>
        public CoreSessionOptions(
            bool isCausallyConsistent = false,
            bool isImplicit = false,
            TransactionOptions defaultTransactionOptions = null)
        {
            _isCausallyConsistent = isCausallyConsistent;
            _isImplicit = isImplicit;
            _defaultTransactionOptions = defaultTransactionOptions;
        }

        // public properties
        /// <summary>
        /// Gets the default transaction options.
        /// </summary>
        /// <value>
        /// The default transaction options.
        /// </value>
        public TransactionOptions DefaultTransactionOptions => _defaultTransactionOptions;

        /// <summary>
        /// Gets a value indicating whether this session is causally consistent.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this session is causally consistent; otherwise, <c>false</c>.
        /// </value>
        public bool IsCausallyConsistent => _isCausallyConsistent;

        /// <summary>
        /// Gets a value indicating whether this session is an implicit session.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this session is an implicit session; otherwise, <c>false</c>.
        /// </value>
        public bool IsImplicit => _isImplicit;
    }
}

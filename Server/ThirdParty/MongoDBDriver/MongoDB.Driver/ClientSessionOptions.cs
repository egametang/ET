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

using MongoDB.Driver.Core.Bindings;

namespace MongoDB.Driver
{
    /// <summary>
    /// Client session options.
    /// </summary>
    public class ClientSessionOptions
    {
        // public properties
        /// <summary>
        /// When true or unspecified, an application will read its own writes and subsequent
        /// reads will never observe an earlier version of the data.
        /// </summary>
        public bool? CausalConsistency { get; set; }

        /// <summary>
        /// Gets or sets the default transaction options.
        /// </summary>
        /// <value>
        /// The default transaction options.
        /// </value>
        public TransactionOptions DefaultTransactionOptions { get; set; }

        // internal methods
        internal CoreSessionOptions ToCore(bool isImplicit = false)
        {
            return new CoreSessionOptions(
                isCausallyConsistent: CausalConsistency ?? true,
                isImplicit: isImplicit,
                defaultTransactionOptions: DefaultTransactionOptions);
        }
    }
}

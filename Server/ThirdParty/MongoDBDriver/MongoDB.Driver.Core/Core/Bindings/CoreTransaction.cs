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
    /// The state of a transaction.
    /// </summary>
    public class CoreTransaction
    {
        // private fields
        private bool _isEmpty;
        private CoreTransactionState _state;
        private readonly long _transactionNumber;
        private readonly TransactionOptions _transactionOptions;

        // public constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreTransaction" /> class.
        /// </summary>
        /// <param name="transactionNumber">The transaction number.</param>
        /// <param name="transactionOptions">The transaction options.</param>
        public CoreTransaction(long transactionNumber, TransactionOptions transactionOptions)
        {
            _transactionNumber = transactionNumber;
            _transactionOptions = transactionOptions;
            _state = CoreTransactionState.Starting;
            _isEmpty = true;
        }

        // public properties
        /// <summary>
        /// Gets a value indicating whether the transaction is empty.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the transaction is empty; otherwise, <c>false</c>.
        /// </value>
        public bool IsEmpty => _isEmpty;

        /// <summary>
        /// Gets the transaction state.
        /// </summary>
        /// <value>
        /// The transaction state.
        /// </value>
        public CoreTransactionState State => _state;

        /// <summary>
        /// Gets the transaction number.
        /// </summary>
        /// <value>
        /// The transaction number.
        /// </value>
        public long TransactionNumber => _transactionNumber;

        /// <summary>
        /// Gets the transaction options.
        /// </summary>
        /// <value>
        /// The transaction options.
        /// </value>
        public TransactionOptions TransactionOptions => _transactionOptions;

        // internal methods
        internal void SetState(CoreTransactionState state)
        {
            _state = state;
            if (state == CoreTransactionState.InProgress)
            {
                _isEmpty = false;
            }
        }
    }
}

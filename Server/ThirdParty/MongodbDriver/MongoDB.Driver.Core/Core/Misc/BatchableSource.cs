/* Copyright 2013-2015 MongoDB Inc.
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
using System.Collections.Generic;
using System.Linq;

namespace MongoDB.Driver.Core.Misc
{
    /// <summary>
    /// Represents a source of items that can be broken into batches.
    /// </summary>
    /// <typeparam name="T">The type of the items.</typeparam>
    public sealed class BatchableSource<T>
    {
        #region static
        // static fields
        private static readonly T[] __emptyBatch = new T[0];
        #endregion

        // fields
        private IReadOnlyList<T> _batch;
        private IEnumerator<T> _enumerator;
        private bool _hasMore;
        private Overflow _overflow;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="BatchableSource{T}"/> class.
        /// </summary>
        /// <remarks>
        /// Use this overload when you know the batch is small and won't have to be broken up into sub-batches. 
        /// In that case using this overload is simpler than using an enumerator and using the other constructor.
        /// </remarks>
        /// <param name="batch">The single batch.</param>
        public BatchableSource(IEnumerable<T> batch)
        {
            _batch = Ensure.IsNotNull(batch, nameof(batch)).ToList();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BatchableSource{T}"/> class.
        /// </summary>
        /// <param name="enumerator">The enumerator that will provide the items for the batch.</param>
        public BatchableSource(IEnumerator<T> enumerator)
        {
            _enumerator = Ensure.IsNotNull(enumerator, nameof(enumerator));
            _hasMore = true;
        }

        // properties
        /// <summary>
        /// Gets the most recent batch.
        /// </summary>
        /// <value>
        /// The most recent batch.
        /// </value>
        public IReadOnlyList<T> Batch
        {
            get { return _batch; }
        }

        /// <summary>
        /// Gets the current item.
        /// </summary>
        /// <value>
        /// The current item.
        /// </value>
        public T Current
        {
            get
            {
                ThrowIfNotBatchable();
                ThrowIfHasBatch();
                return _enumerator.Current;
            }
        }

        /// <summary>
        /// Gets a value indicating whether there are more items.
        /// </summary>
        /// <value>
        ///   <c>true</c> if there are more items; otherwise, <c>false</c>.
        /// </value>
        public bool HasMore
        {
            get
            {
                return _hasMore;
            }
        }

        // methods
        /// <summary>
        /// Clears the most recent batch.
        /// </summary>
        public void ClearBatch()
        {
            ThrowIfNotBatchable();
            _batch = null;
        }

        /// <summary>
        /// Called when the last batch is complete.
        /// </summary>
        /// <param name="batch">The batch.</param>
        public void EndBatch(IReadOnlyList<T> batch)
        {
            ThrowIfNotBatchable();
            ThrowIfHasBatch();
            _batch = batch;
            _hasMore = false;
        }

        /// <summary>
        /// Called when an intermediate batch is complete.
        /// </summary>
        /// <param name="batch">The batch.</param>
        /// <param name="overflow">The overflow item.</param>
        public void EndBatch(IReadOnlyList<T> batch, Overflow overflow)
        {
            ThrowIfNotBatchable();
            ThrowIfHasBatch();
            _batch = batch;
            _overflow = overflow;
            _hasMore = true;
        }

        /// <summary>
        /// Gets all the remaining items that haven't been previously consumed.
        /// </summary>
        /// <returns>The remaining items.</returns>
        public IEnumerable<T> GetRemainingItems()
        {
            if (_overflow != null)
            {
                yield return _overflow.Item;
                _overflow = null;
            }

            if (_enumerator != null)
            {
                while (_enumerator.MoveNext())
                {
                    yield return _enumerator.Current;
                }
            }
            else
            {
                foreach (var item in _batch)
                {
                    yield return item;
                }
                _batch = __emptyBatch;
            }

            _hasMore = false;
        }

        /// <summary>
        /// Moves to the next item in the source.
        /// </summary>
        /// <returns>True if there are more items.</returns>
        public bool MoveNext()
        {
            ThrowIfNotBatchable();
            ThrowIfHasBatch();
            return _enumerator.MoveNext();
        }

        /// <summary>
        /// Starts a new batch.
        /// </summary>
        /// <returns>The overflow item of the previous batch if there is one; otherwise, null.</returns>
        public Overflow StartBatch()
        {
            ThrowIfNotBatchable();
            ThrowIfHasBatch();
            var overflow = _overflow;
            _overflow = null;
            return overflow;
        }

        private void ThrowIfHasBatch()
        {
            if (_batch != null)
            {
                throw new InvalidOperationException("This method can only be called when there is no current batch.");
            }
        }

        private void ThrowIfNotBatchable()
        {
            if (_enumerator == null)
            {
                throw new InvalidOperationException("This method can only be called when an enumerator was provided.");
            }
        }

        // nested types
        /// <summary>
        /// Represents an overflow item that did not fit in the most recent batch and will be become the first item in the next batch.
        /// </summary>
        public class Overflow
        {
            /// <summary>
            /// The item.
            /// </summary>
            public T Item;

            /// <summary>
            /// The state information, if any, that the consumer wishes to associate with the overflow item.
            /// </summary>
            public object State;
        }
    }
}

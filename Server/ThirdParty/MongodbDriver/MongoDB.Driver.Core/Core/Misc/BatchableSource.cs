/* Copyright 2013-present MongoDB Inc.
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
    /// Represents a batch of items that can be split if not all items can be processed at once.
    /// </summary>
    /// <typeparam name="T">The type of the items.</typeparam>
    public sealed class BatchableSource<T> : IBatchableSource<T>
    {
        #region static
        // private static methods
        private static IReadOnlyList<T> EnumeratorToList(IEnumerator<T> enumerator)
        {
            var list = new List<T>();
            while (enumerator.MoveNext())
            {
                list.Add(enumerator.Current);
            }
            return list;
        }
        #endregion

        // fields
        private readonly bool _canBeSplit;
        private int _count;
        private readonly IReadOnlyList<T> _items;
        private int _offset;
        private int _processedCount;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="BatchableSource{T}"/> class.
        /// </summary>
        /// <remarks>
        /// Use this overload when you know the batch is small and won't have to be broken up into sub-batches. 
        /// In that case using this overload is simpler than using an enumerator and using the other constructor.
        /// </remarks>
        /// <param name="batch">The single batch.</param>
        [Obsolete("Use one of the other constructors instead.")]
        public BatchableSource(IEnumerable<T> batch)
            : this(Ensure.IsNotNull(batch, nameof(batch)).ToList(), canBeSplit: true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BatchableSource{T}"/> class.
        /// </summary>
        /// <param name="enumerator">The enumerator that will provide the items for the batch.</param>
        [Obsolete("Use one of the other constructors instead.")]
        public BatchableSource(IEnumerator<T> enumerator)
            : this(EnumeratorToList(Ensure.IsNotNull(enumerator, nameof(enumerator))), canBeSplit: true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BatchableSource{T}" /> class.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <param name="canBeSplit">if set to <c>true</c> the batch can be split.</param>
        public BatchableSource(IReadOnlyList<T> items, bool canBeSplit = false)
            : this(Ensure.IsNotNull(items, nameof(items)), 0, items.Count, canBeSplit)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BatchableSource{T}"/> class.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="count">The count.</param>
        /// <param name="canBeSplit">if set to <c>true</c> the batch can be split.</param>
        public BatchableSource(IReadOnlyList<T> items, int offset, int count, bool canBeSplit)
        {
            _items = Ensure.IsNotNull(items, nameof(items));
            _offset = Ensure.IsBetween(offset, 0, items.Count, nameof(offset));
            _count = Ensure.IsBetween(count, 0, items.Count - offset, nameof(count));
            _canBeSplit = canBeSplit;
            _processedCount = 0;
        }

        // public properties
        /// <summary>
        /// Gets a value indicating whether all items were processed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if all items were processed; otherwise, <c>false</c>.
        /// </value>
        public bool AllItemsWereProcessed => _processedCount == _count;

        /// <summary>
        /// Gets a value indicating whether the batch can be split.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the batch can be split; otherwise, <c>false</c>.
        /// </value>
        public bool CanBeSplit => _canBeSplit;

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        public int Count
        {
            get { return _count; }
        }

        /// <summary>
        /// Gets the items.
        /// </summary>
        /// <value>
        /// The items.
        /// </value>
        public IReadOnlyList<T> Items => _items;

        /// <summary>
        /// Gets the offset.
        /// </summary>
        /// <value>
        /// The offset.
        /// </value>
        public int Offset => _offset;

        /// <summary>
        /// Gets the count of processed items. Equal to zero until SetProcessedCount has been called.
        /// </summary>
        /// <value>
        /// The count of processed items.
        /// </value>
        public int ProcessedCount => _processedCount;

        // public methods
        /// <summary>
        /// Advances past the processed items.
        /// </summary>
        public void AdvancePastProcessedItems()
        {
            _offset = _offset + _processedCount;
            _count = _count - _processedCount;
            _processedCount = 0;
        }

        /// <summary>
        /// Gets the items in the batch.
        /// </summary>
        /// <returns>
        /// The items in the batch.
        /// </returns>
        public IReadOnlyList<T> GetBatchItems()
        {
            return _items.Skip(_offset).Take(_count).ToList();
        }

        /// <summary>
        /// Gets the items that were processed.
        /// </summary>
        /// <returns>
        /// The items that were processed.
        /// </returns>
        public IReadOnlyList<T> GetProcessedItems()
        {
            return _items.Skip(_offset).Take(_processedCount).ToList();
        }

        /// <summary>
        /// Gets the items that were not processed.
        /// </summary>
        /// <returns>
        /// The items that were not processed.
        /// </returns>
        public IReadOnlyList<T> GetUnprocessedItems()
        {
            return _items.Skip(_offset + _processedCount).Take(_count - _processedCount).ToList();
        }

        /// <summary>
        /// Sets the processed count.
        /// </summary>
        /// <param name="value">The value.</param>
        public void SetProcessedCount(int value)
        {
            Ensure.IsBetween(value, 0, _count, nameof(value));
            if (value != _count && !_canBeSplit)
            {
                throw new InvalidOperationException("The batch cannot be split.");
            }

            _processedCount = value;
        }
    }
}

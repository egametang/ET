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

using System;
using System.Collections.Generic;
using System.Linq;

namespace MongoDB.Driver.Core.Misc
{
    /// <summary>
    /// Represents a batch of items that can be split if not all items can be processed at once.
    /// </summary>
    /// <typeparam name="T">The type of the items.</typeparam>
    public interface IBatchableSource<out T>
    {
        // properties
        /// <summary>
        /// Gets a value indicating whether all items were processed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if all items were processed; otherwise, <c>false</c>.
        /// </value>
        bool AllItemsWereProcessed { get; }

        /// <summary>
        /// Gets a value indicating whether the batch can be split.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the batch can be split; otherwise, <c>false</c>.
        /// </value>
        bool CanBeSplit { get; }

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        int Count { get; }

        /// <summary>
        /// Gets the items.
        /// </summary>
        /// <value>
        /// The items.
        /// </value>
        IReadOnlyList<T> Items { get; }

        /// <summary>
        /// Gets the offset.
        /// </summary>
        /// <value>
        /// The offset.
        /// </value>
        int Offset { get; }

        /// <summary>
        /// Gets the count of processed items. Equal to zero until SetProcessedCount has been called.
        /// </summary>
        /// <value>
        /// The count of processed items.
        /// </value>
        int ProcessedCount { get; }

        // methods
        /// <summary>
        /// Advances past the processed items.
        /// </summary>
        void AdvancePastProcessedItems();

        /// <summary>
        /// Gets the items in the batch.
        /// </summary>
        /// <returns>
        /// The items in the batch.
        /// </returns>
        IReadOnlyList<T> GetBatchItems();

        /// <summary>
        /// Gets the items that were processed.
        /// </summary>
        /// <returns>
        /// The items that were processed.
        /// </returns>
        IReadOnlyList<T> GetProcessedItems();

        /// <summary>
        /// Gets the items that were not processed.
        /// </summary>
        /// <returns>
        /// The items that were not processed.
        /// </returns>
        IReadOnlyList<T> GetUnprocessedItems();

        /// <summary>
        /// Sets the processed count.
        /// </summary>
        /// <param name="value">The value.</param>
        void SetProcessedCount(int value);
    }
}

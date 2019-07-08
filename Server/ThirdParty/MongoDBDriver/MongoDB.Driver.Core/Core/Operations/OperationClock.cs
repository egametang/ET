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

using MongoDB.Bson;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Core.Operations
{
    /// <summary>
    /// An operation clock.
    /// </summary>
    /// <seealso cref="MongoDB.Driver.Core.Operations.IOperationClock" />
    internal class OperationClock : IOperationClock
    {
        #region static
        // public static methods
        /// <summary>
        /// Returns the greater of two operation times.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns>The greater operation time.</returns>
        public static BsonTimestamp GreaterOperationTime(BsonTimestamp x, BsonTimestamp y)
        {
            if (x == null)
            {
                return y;
            }
            else if (y == null)
            {
                return x;
            }
            else
            {
                return x > y ? x : y;
            }
        }
        #endregion

        // private fields
        private BsonTimestamp _operationTime;

        // public properties
        /// <inheritdoc />
        public BsonTimestamp OperationTime => _operationTime;

        // public methods
        /// <inheritdoc />
        public void AdvanceOperationTime(BsonTimestamp newOperationTime)
        {
            Ensure.IsNotNull(newOperationTime, nameof(newOperationTime));
            _operationTime = GreaterOperationTime(_operationTime, newOperationTime);
        }
    }

    /// <summary>
    /// An object that represents no operation clock.
    /// </summary>
    /// <seealso cref="MongoDB.Driver.Core.Operations.IOperationClock" />
    public sealed class NoOperationClock : IOperationClock
    {
        /// <inheritdoc />
        public BsonTimestamp OperationTime => null;

        /// <inheritdoc />
        public void AdvanceOperationTime(BsonTimestamp newOperationTime)
        {
        }
    }
}

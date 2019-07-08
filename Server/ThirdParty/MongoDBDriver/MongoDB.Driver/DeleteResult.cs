/* Copyright 2010-present MongoDB Inc.
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
using System.Text;

namespace MongoDB.Driver
{
    /// <summary>
    /// The result of a delete operation.
    /// </summary>
    public abstract class DeleteResult
    {
        // static
        internal static DeleteResult FromCore(BulkWriteResult result)
        {
            if (result.IsAcknowledged)
            {
                return new Acknowledged(result.DeletedCount);
            }

            return Unacknowledged.Instance;
        }

        // properties
        /// <summary>
        /// Gets the deleted count. If IsAcknowledged is false, this will throw an exception.
        /// </summary>
        public abstract long DeletedCount { get; }

        /// <summary>
        /// Gets a value indicating whether the result is acknowledged.
        /// </summary>
        public abstract bool IsAcknowledged { get; }

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteResult"/> class.
        /// </summary>
        protected DeleteResult()
        {
        }

        // nested classes
        /// <summary>
        /// The result of an acknowledged delete operation.
        /// </summary>
        public class Acknowledged : DeleteResult
        {
            private readonly long _deletedCount;

            /// <summary>
            /// Initializes a new instance of the
            /// </summary>
            /// <param name="deletedCount">The deleted count.</param>
            public Acknowledged(long deletedCount)
            {
                _deletedCount = deletedCount;
            }

            /// <inheritdoc/>
            public override long DeletedCount
            {
                get { return _deletedCount; }
            }

            /// <inheritdoc/>
            public override bool IsAcknowledged
            {
                get { return true; }
            }
        }

        /// <summary>
        /// The result of an unacknowledged delete operation.
        /// </summary>
        public class Unacknowledged : DeleteResult
        {
            private static Unacknowledged __instance = new Unacknowledged();

            /// <summary>
            /// Gets the instance.
            /// </summary>
            public static Unacknowledged Instance
            {
                get { return __instance; }
            }

            private Unacknowledged()
            {
            }

            /// <inheritdoc/>
            public override long DeletedCount
            {
                get { throw new NotSupportedException("Only acknowledged writes support the DeletedCount property."); }
            }

            /// <inheritdoc/>
            public override bool IsAcknowledged
            {
                get { return false; }
            }
        }
    }
}
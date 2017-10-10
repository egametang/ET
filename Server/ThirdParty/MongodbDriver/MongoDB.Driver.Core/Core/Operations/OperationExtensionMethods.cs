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
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver.Core.Bindings;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Core.Operations
{
    /// <summary>
    /// Represents extension methods for operations.
    /// </summary>
    public static class OperationExtensionMethods
    {
        /// <summary>
        /// Executes a read operation using a channel source.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="operation">The read operation.</param>
        /// <param name="channelSource">The channel source.</param>
        /// <param name="readPreference">The read preference.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The result of the operation.</returns>
        public static TResult Execute<TResult>(
            this IReadOperation<TResult> operation,
            IChannelSourceHandle channelSource,
            ReadPreference readPreference,
            CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(operation, nameof(operation));
            using (var readBinding = new ChannelSourceReadWriteBinding(channelSource.Fork(), readPreference))
            {
                return operation.Execute(readBinding, cancellationToken);
            }
        }

        /// <summary>
        /// Executes a write operation using a channel source.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="operation">The write operation.</param>
        /// <param name="channelSource">The channel source.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The result of the operation.</returns>
        public static TResult Execute<TResult>(
            this IWriteOperation<TResult> operation,
            IChannelSourceHandle channelSource,
            CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(operation, nameof(operation));
            using (var writeBinding = new ChannelSourceReadWriteBinding(channelSource.Fork(), ReadPreference.Primary))
            {
                return operation.Execute(writeBinding, cancellationToken);
            }
        }

        /// <summary>
        /// Executes a read operation using a channel source.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="operation">The read operation.</param>
        /// <param name="channelSource">The channel source.</param>
        /// <param name="readPreference">The read preference.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task whose result is the result of the operation.</returns>
        public static async Task<TResult> ExecuteAsync<TResult>(
            this IReadOperation<TResult> operation,
            IChannelSourceHandle channelSource,
            ReadPreference readPreference,
            CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(operation, nameof(operation));
            using (var readBinding = new ChannelSourceReadWriteBinding(channelSource.Fork(), readPreference))
            {
                return await operation.ExecuteAsync(readBinding, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Executes a write operation using a channel source.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="operation">The write operation.</param>
        /// <param name="channelSource">The channel source.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task whose result is the result of the operation.</returns>
        public static async Task<TResult> ExecuteAsync<TResult>(
            this IWriteOperation<TResult> operation,
            IChannelSourceHandle channelSource,
            CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(operation, nameof(operation));
            using (var writeBinding = new ChannelSourceReadWriteBinding(channelSource.Fork(), ReadPreference.Primary))
            {
                return await operation.ExecuteAsync(writeBinding, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}

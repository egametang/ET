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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver.Core.Bindings;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Core.Operations
{
    /// <summary>
    /// Represents a context for retryable writes.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public sealed class RetryableWriteContext : IDisposable
    {
        #region static
        // public static methods
        /// <summary>
        /// Creates and initializes a retryable write operation context.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <param name="retryRequested">if set to <c>true</c> [retry requested].</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A retryable write context.</returns>
        public static RetryableWriteContext Create(IWriteBinding binding, bool retryRequested, CancellationToken cancellationToken)
        {
            var context = new RetryableWriteContext(binding, retryRequested);
            try
            {
                context.Initialize(cancellationToken);
                return context;
            }
            catch
            {
                context.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Creates and initializes a retryable write operation context.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <param name="retryRequested">if set to <c>true</c> [retry requested].</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A retryable write context.</returns>
        public static async Task<RetryableWriteContext> CreateAsync(IWriteBinding binding, bool retryRequested, CancellationToken cancellationToken)
        {
            var context = new RetryableWriteContext(binding, retryRequested);
            try
            {
                await context.InitializeAsync(cancellationToken).ConfigureAwait(false);
                return context;
            }
            catch
            {
                context.Dispose();
                throw;
            }
        }
        #endregion

        // private fields
        private readonly IWriteBinding _binding;
        private IChannelHandle _channel;
        private IChannelSourceHandle _channelSource;
        private bool _disposed;
        private bool _retryRequested;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="RetryableWriteContext"/> class.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <param name="retryRequested">if set to <c>true</c> the operation can be retried.</param>
        public RetryableWriteContext(IWriteBinding binding, bool retryRequested)
        {
            _binding = Ensure.IsNotNull(binding, nameof(binding));
            _retryRequested = retryRequested;
        }

        // public properties
        /// <summary>
        /// Gets the binding.
        /// </summary>
        /// <value>
        /// The binding.
        /// </value>
        public IWriteBinding Binding => _binding;

        /// <summary>
        /// Gets the channel.
        /// </summary>
        /// <value>
        /// The channel.
        /// </value>
        public IChannelHandle Channel => _channel;

        /// <summary>
        /// Gets the channel source.
        /// </summary>
        /// <value>
        /// The channel source.
        /// </value>
        public IChannelSourceHandle ChannelSource => _channelSource;

        /// <summary>
        /// Gets a value indicating whether the operation can be retried.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the operation can be retried; otherwise, <c>false</c>.
        /// </value>
        public bool RetryRequested => _retryRequested;

        // public methods
        /// <summary>
        /// Disables retries.
        /// </summary>
        /// <param name="requests">The requests.</param>
        public void DisableRetriesIfAnyWriteRequestIsNotRetryable(IEnumerable<WriteRequest> requests)
        {
            if (_retryRequested)
            {
                if (requests.Any(r => !r.IsRetryable(_channel.ConnectionDescription)))
                {
                    _retryRequested = false;
                }
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (!_disposed)
            {
                _channelSource?.Dispose();
                _channel?.Dispose();
                _disposed = true;
            }
        }

        /// <summary>
        /// Replaces the channel.
        /// </summary>
        /// <param name="channel">The channel.</param>
        public void ReplaceChannel(IChannelHandle channel)
        {
            Ensure.IsNotNull(channel, nameof(channel));
            _channel?.Dispose();
            _channel = channel;
        }

        /// <summary>
        /// Replaces the channel source.
        /// </summary>
        /// <param name="channelSource">The channel source.</param>
        public void ReplaceChannelSource(IChannelSourceHandle channelSource)
        {
            Ensure.IsNotNull(channelSource, nameof(channelSource));
            _channelSource?.Dispose();
            _channel?.Dispose();
            _channelSource = channelSource;
            _channel = null;
        }

        // private methods
        private void Initialize(CancellationToken cancellationToken)
        {
            _channelSource = _binding.GetWriteChannelSource(cancellationToken);
            _channel = _channelSource.GetChannel(cancellationToken);
        }

        private async Task InitializeAsync(CancellationToken cancellationToken)
        {
            _channelSource = await _binding.GetWriteChannelSourceAsync(cancellationToken).ConfigureAwait(false);
            _channel = await _channelSource.GetChannelAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}

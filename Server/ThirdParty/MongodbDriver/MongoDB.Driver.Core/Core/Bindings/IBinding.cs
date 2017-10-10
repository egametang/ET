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
using MongoDB.Driver.Core.Clusters;

namespace MongoDB.Driver.Core.Bindings
{
    /// <summary>
    /// Represents a binding that determines which channel source gets used for read operations.
    /// </summary>
    public interface IReadBinding : IDisposable
    {
        /// <summary>
        /// Gets the read preference.
        /// </summary>
        /// <value>
        /// The read preference.
        /// </value>
        ReadPreference ReadPreference { get; }

        /// <summary>
        /// Gets a channel source for read operations.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A channel source.</returns>
        IChannelSourceHandle GetReadChannelSource(CancellationToken cancellationToken);

        /// <summary>
        /// Gets a channel source for read operations.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A channel source.</returns>
        Task<IChannelSourceHandle> GetReadChannelSourceAsync(CancellationToken cancellationToken);
    }

    /// <summary>
    /// Represents a binding that determines which channel source gets used for write operations.
    /// </summary>
    public interface IWriteBinding : IDisposable
    {
        /// <summary>
        /// Gets a channel source for write operations.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A channel source.</returns>
        IChannelSourceHandle GetWriteChannelSource(CancellationToken cancellationToken);

        /// <summary>
        /// Gets a channel source for write operations.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A channel source.</returns>
        Task<IChannelSourceHandle> GetWriteChannelSourceAsync(CancellationToken cancellationToken);
    }

    /// <summary>
    /// Represents a binding that can be used for both read and write operations.
    /// </summary>
    public interface IReadWriteBinding : IReadBinding, IWriteBinding
    {
    }

    /// <summary>
    /// Represents a handle to a read binding.
    /// </summary>
    public interface IReadBindingHandle : IReadBinding
    {
        /// <summary>
        /// Returns a new handle to the underlying read binding.
        /// </summary>
        /// <returns>A read binding handle.</returns>
        IReadBindingHandle Fork();
    }

    /// <summary>
    /// Represents a handle to a write binding.
    /// </summary>
    public interface IWriteBindingHandle : IWriteBinding
    {
        /// <summary>
        /// Returns a new handle to the underlying write binding.
        /// </summary>
        /// <returns>A write binding handle.</returns>
        IWriteBindingHandle Fork();
    }

    /// <summary>
    /// Represents a handle to a read-write binding.
    /// </summary>
    public interface IReadWriteBindingHandle : IReadWriteBinding, IReadBindingHandle, IWriteBindingHandle
    {
        /// <summary>
        /// Returns a new handle to the underlying read-write binding.
        /// </summary>
        /// <returns>A read-write binding handle.</returns>
        new IReadWriteBindingHandle Fork();
    }
}

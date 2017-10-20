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
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Servers;

namespace MongoDB.Driver.Core.Bindings
{
    /// <summary>
    /// Represents a channel source.
    /// </summary>
    public interface IChannelSource : IDisposable
    {
        /// <summary>
        /// Gets the server.
        /// </summary>
        /// <value>
        /// The server.
        /// </value>
        IServer Server{ get; }

        /// <summary>
        /// Gets the server description.
        /// </summary>
        /// <value>
        /// The server description.
        /// </value>
        ServerDescription ServerDescription { get; }

        /// <summary>
        /// Gets a channel.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A channel.</returns>
        IChannelHandle GetChannel(CancellationToken cancellationToken);

        /// <summary>
        /// Gets a channel.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task whose result is a channel.</returns>
        Task<IChannelHandle> GetChannelAsync(CancellationToken cancellationToken);
    }

    /// <summary>
    /// Represents a handle to a channel source.
    /// </summary>
    public interface IChannelSourceHandle : IChannelSource
    {
        /// <summary>
        /// Returns a new handle to the underlying channel source.
        /// </summary>
        /// <returns>A handle to a channel source.</returns>
        IChannelSourceHandle Fork();
    }
}

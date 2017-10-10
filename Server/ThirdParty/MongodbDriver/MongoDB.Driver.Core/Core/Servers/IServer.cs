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
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver.Core.Bindings;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Events;

namespace MongoDB.Driver.Core.Servers
{
    /// <summary>
    /// Represents a MongoDB server.
    /// </summary>
    public interface IServer
    {
        // events
        /// <summary>
        /// Occurs when the server description changes.
        /// </summary>
        event EventHandler<ServerDescriptionChangedEventArgs> DescriptionChanged;

        // properties
        /// <summary>
        /// Gets the server description.
        /// </summary>
        /// <value>
        /// The server description.
        /// </value>
        ServerDescription Description { get; }

        /// <summary>
        /// Gets the end point.
        /// </summary>
        /// <value>
        /// The end point.
        /// </value>
        EndPoint EndPoint { get; }

        /// <summary>
        /// Gets the server identifier.
        /// </summary>
        /// <value>
        /// The server identifier.
        /// </value>
        ServerId ServerId { get; }

        // methods
        /// <summary>
        /// Gets a channel to the server.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A channel.</returns>
        IChannelHandle GetChannel(CancellationToken cancellationToken);

        // methods
        /// <summary>
        /// Gets a channel to the server.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task whose result is a channel.</returns>
        Task<IChannelHandle> GetChannelAsync(CancellationToken cancellationToken);
    }

    /// <summary>
    /// Represents a server that can be part of a cluster.
    /// </summary>
    public interface IClusterableServer : IServer, IDisposable
    {
        // properties
        /// <summary>
        /// Gets a value indicating whether this instance is initialized.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is initialized; otherwise, <c>false</c>.
        /// </value>
        bool IsInitialized { get; }

        // methods
        /// <summary>
        /// Initializes this instance.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Invalidates this instance (sets the server type to Unknown and clears the connection pool).
        /// </summary>
        void Invalidate();

        /// <summary>
        /// Requests a heartbeat as soon as possible.
        /// </summary>
        void RequestHeartbeat();
    }
}

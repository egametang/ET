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
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Servers;

namespace MongoDB.Driver.Core.ConnectionPools
{
    /// <summary>
    /// Represents a connection pool.
    /// </summary>
    public interface IConnectionPool : IDisposable
    {
        // properties
        /// <summary>
        /// Gets the server identifier.
        /// </summary>
        /// <value>
        /// The server identifier.
        /// </value>
        ServerId ServerId { get; }

        // methods
        /// <summary>
        /// Acquires a connection.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A connection.</returns>
        IConnectionHandle AcquireConnection(CancellationToken cancellationToken);

        /// <summary>
        /// Acquires a connection.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task whose result is a connection.</returns>
        Task<IConnectionHandle> AcquireConnectionAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Clears the connection pool.
        /// </summary>
        void Clear();

        /// <summary>
        /// Initializes the connection pool.
        /// </summary>
        void Initialize();
    }
}

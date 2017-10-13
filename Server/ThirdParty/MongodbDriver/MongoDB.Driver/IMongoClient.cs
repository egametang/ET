/* Copyright 2013-2016 MongoDB Inc.
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

using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver.Core.Clusters;

namespace MongoDB.Driver
{
    /// <summary>
    /// The client interface to MongoDB.
    /// </summary>
    /// <remarks>
    /// This interface is not guaranteed to remain stable. Implementors should use
    /// <see cref="MongoClientBase"/>.
    /// </remarks>
    public interface IMongoClient
    {
        /// <summary>
        /// Gets the cluster.
        /// </summary>
        /// <value>
        /// The cluster.
        /// </value>
        ICluster Cluster { get; }

        /// <summary>
        /// Gets the settings.
        /// </summary>
        MongoClientSettings Settings { get; }

        /// <summary>
        /// Drops the database with the specified name.
        /// </summary>
        /// <param name="name">The name of the database to drop.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        void DropDatabase(string name, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Drops the database with the specified name.
        /// </summary>
        /// <param name="name">The name of the database to drop.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task.</returns>
        Task DropDatabaseAsync(string name, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Gets a database.
        /// </summary>
        /// <param name="name">The name of the database.</param>
        /// <param name="settings">The database settings.</param>
        /// <returns>An implementation of a database.</returns>
        IMongoDatabase GetDatabase(string name, MongoDatabaseSettings settings = null);

        /// <summary>
        /// Lists the databases on the server.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A cursor.</returns>
        IAsyncCursor<BsonDocument> ListDatabases(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Lists the databases on the server.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task whose result is a cursor.</returns>
        Task<IAsyncCursor<BsonDocument>> ListDatabasesAsync(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Returns a new IMongoClient instance with a different read concern setting.
        /// </summary>
        /// <param name="readConcern">The read concern.</param>
        /// <returns>A new IMongoClient instance with a different read concern setting.</returns>
        IMongoClient WithReadConcern(ReadConcern readConcern);

        /// <summary>
        /// Returns a new IMongoClient instance with a different read preference setting.
        /// </summary>
        /// <param name="readPreference">The read preference.</param>
        /// <returns>A new IMongoClient instance with a different read preference setting.</returns>
        IMongoClient WithReadPreference(ReadPreference readPreference);

        /// <summary>
        /// Returns a new IMongoClient instance with a different write concern setting.
        /// </summary>
        /// <param name="writeConcern">The write concern.</param>
        /// <returns>A new IMongoClient instance with a different write concern setting.</returns>
        IMongoClient WithWriteConcern(WriteConcern writeConcern);
    }
}

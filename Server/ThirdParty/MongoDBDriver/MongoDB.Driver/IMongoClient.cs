/* Copyright 2013-present MongoDB Inc.
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

using System.Collections.Generic;
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
        /// <param name="session">The session.</param>
        /// <param name="name">The name of the database to drop.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        void DropDatabase(IClientSessionHandle session, string name, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Drops the database with the specified name.
        /// </summary>
        /// <param name="name">The name of the database to drop.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task.</returns>
        Task DropDatabaseAsync(string name, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Drops the database with the specified name.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="name">The name of the database to drop.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A task.
        /// </returns>
        Task DropDatabaseAsync(IClientSessionHandle session, string name, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Gets a database.
        /// </summary>
        /// <param name="name">The name of the database.</param>
        /// <param name="settings">The database settings.</param>
        /// <returns>An implementation of a database.</returns>
        IMongoDatabase GetDatabase(string name, MongoDatabaseSettings settings = null);

        /// <summary>
        /// Returns the names of the databases on the server.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The database names.</returns>
        IAsyncCursor<string> ListDatabaseNames(
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Returns the names of the databases on the server.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The database names.</returns>
        IAsyncCursor<string> ListDatabaseNames(
            IClientSessionHandle session,                                              
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Returns the names of the databases on the server.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The database names.</returns>
        Task<IAsyncCursor<string>> ListDatabaseNamesAsync(
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Returns the names of the databases on the server.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The database names.</returns>
        Task<IAsyncCursor<string>> ListDatabaseNamesAsync(
            IClientSessionHandle session,                                              
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Lists the databases on the server.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A cursor.</returns>
        IAsyncCursor<BsonDocument> ListDatabases(
            CancellationToken cancellationToken = default(CancellationToken));        
        
        /// <summary>
        /// Lists the databases on the server.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A cursor.</returns>
        IAsyncCursor<BsonDocument> ListDatabases(
            ListDatabasesOptions options,
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Lists the databases on the server.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A cursor.
        /// </returns>
        IAsyncCursor<BsonDocument> ListDatabases(
            IClientSessionHandle session,
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Lists the databases on the server.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A cursor.
        /// </returns>
        IAsyncCursor<BsonDocument> ListDatabases(
            IClientSessionHandle session,
            ListDatabasesOptions options,
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Lists the databases on the server.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task whose result is a cursor.</returns>
        Task<IAsyncCursor<BsonDocument>> ListDatabasesAsync(
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Lists the databases on the server.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="options">The options.</param>
        /// <returns>A Task whose result is a cursor.</returns>
        Task<IAsyncCursor<BsonDocument>> ListDatabasesAsync(
            ListDatabasesOptions options,
            CancellationToken cancellationToken = default(CancellationToken));


        /// <summary>
        /// Lists the databases on the server.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A Task whose result is a cursor.
        /// </returns>
        Task<IAsyncCursor<BsonDocument>> ListDatabasesAsync(
            IClientSessionHandle session,        
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Lists the databases on the server.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="options">The options.</param>        
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A Task whose result is a cursor.
        /// </returns>
        Task<IAsyncCursor<BsonDocument>> ListDatabasesAsync(
            IClientSessionHandle session,
            ListDatabasesOptions options,
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Starts a client session.
        /// </summary>
        /// <param name="options">The session options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A client session.
        /// </returns>
        IClientSessionHandle StartSession(ClientSessionOptions options = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Starts a client session.
        /// </summary>
        /// <param name="options">The session options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A Task whose result is a client session.
        /// </returns>
        Task<IClientSessionHandle> StartSessionAsync(ClientSessionOptions options = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Watches changes on all collections in all databases.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A change stream.
        /// </returns>
        IAsyncCursor<TResult> Watch<TResult>(
            PipelineDefinition<ChangeStreamDocument<BsonDocument>, TResult> pipeline,
            ChangeStreamOptions options = null,
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Watches changes on all collections in all databases.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="session">The session.</param>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A change stream.
        /// </returns>
        IAsyncCursor<TResult> Watch<TResult>(
            IClientSessionHandle session,
            PipelineDefinition<ChangeStreamDocument<BsonDocument>, TResult> pipeline,
            ChangeStreamOptions options = null,
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Watches changes on all collections in all databases.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A change stream.
        /// </returns>
        Task<IAsyncCursor<TResult>> WatchAsync<TResult>(
            PipelineDefinition<ChangeStreamDocument<BsonDocument>, TResult> pipeline,
            ChangeStreamOptions options = null,
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Watches changes on all collections in all databases.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="session">The session.</param>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A change stream.
        /// </returns>
        Task<IAsyncCursor<TResult>> WatchAsync<TResult>(
            IClientSessionHandle session,
            PipelineDefinition<ChangeStreamDocument<BsonDocument>, TResult> pipeline,
            ChangeStreamOptions options = null,
            CancellationToken cancellationToken = default(CancellationToken));

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

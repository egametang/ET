/* Copyright 2010-2017 MongoDB Inc.
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
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace MongoDB.Driver
{
    /// <summary>
    /// Represents a database in MongoDB.
    /// </summary>
    /// <remarks>
    /// This interface is not guaranteed to remain stable. Implementors should use
    /// <see cref="MongoDatabaseBase" />.
    /// </remarks>
    public interface IMongoDatabase
    {
        /// <summary>
        /// Gets the client.
        /// </summary>
        IMongoClient Client { get; }

        /// <summary>
        /// Gets the namespace of the database.
        /// </summary>
        DatabaseNamespace DatabaseNamespace { get; }

        /// <summary>
        /// Gets the settings.
        /// </summary>
        MongoDatabaseSettings Settings { get; }

        /// <summary>
        /// Creates the collection with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        void CreateCollection(string name, CreateCollectionOptions options = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Creates the collection with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task.</returns>
        Task CreateCollectionAsync(string name, CreateCollectionOptions options = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Creates a view.
        /// </summary>
        /// <typeparam name="TDocument">The type of the input documents.</typeparam>
        /// <typeparam name="TResult">The type of the pipeline result documents.</typeparam>
        /// <param name="viewName">The name of the view.</param>
        /// <param name="viewOn">The name of the collection that the view is on.</param>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        void CreateView<TDocument, TResult>(string viewName, string viewOn, PipelineDefinition<TDocument, TResult> pipeline, CreateViewOptions<TDocument> options = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Creates a view.
        /// </summary>
        /// <typeparam name="TDocument">The type of the input documents.</typeparam>
        /// <typeparam name="TResult">The type of the pipeline result documents.</typeparam>
        /// <param name="viewName">The name of the view.</param>
        /// <param name="viewOn">The name of the collection that the view is on.</param>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task.</returns>
        Task CreateViewAsync<TDocument, TResult>(string viewName, string viewOn, PipelineDefinition<TDocument, TResult> pipeline, CreateViewOptions<TDocument> options = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Drops the collection with the specified name.
        /// </summary>
        /// <param name="name">The name of the collection to drop.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        void DropCollection(string name, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Drops the collection with the specified name.
        /// </summary>
        /// <param name="name">The name of the collection to drop.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task.</returns>
        Task DropCollectionAsync(string name, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Gets a collection.
        /// </summary>
        /// <typeparam name="TDocument">The document type.</typeparam>
        /// <param name="name">The name of the collection.</param>
        /// <param name="settings">The settings.</param>
        /// <returns>An implementation of a collection.</returns>
        IMongoCollection<TDocument> GetCollection<TDocument>(string name, MongoCollectionSettings settings = null);

        /// <summary>
        /// Lists all the collections on the server.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A cursor.</returns>
        IAsyncCursor<BsonDocument> ListCollections(ListCollectionsOptions options = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Lists all the collections on the server.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task whose result is a cursor.</returns>
        Task<IAsyncCursor<BsonDocument>> ListCollectionsAsync(ListCollectionsOptions options = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Renames the collection.
        /// </summary>
        /// <param name="oldName">The old name.</param>
        /// <param name="newName">The new name.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        void RenameCollection(string oldName, string newName, RenameCollectionOptions options = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Renames the collection.
        /// </summary>
        /// <param name="oldName">The old name.</param>
        /// <param name="newName">The new name.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task.</returns>
        Task RenameCollectionAsync(string oldName, string newName, RenameCollectionOptions options = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Runs a command.
        /// </summary>
        /// <typeparam name="TResult">The result type of the command.</typeparam>
        /// <param name="command">The command.</param>
        /// <param name="readPreference">The read preference.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The result of the command.
        /// </returns>
        TResult RunCommand<TResult>(Command<TResult> command, ReadPreference readPreference = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Runs a command.
        /// </summary>
        /// <typeparam name="TResult">The result type of the command.</typeparam>
        /// <param name="command">The command.</param>
        /// <param name="readPreference">The read preference.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The result of the command.
        /// </returns>
        Task<TResult> RunCommandAsync<TResult>(Command<TResult> command, ReadPreference readPreference = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Returns a new IMongoDatabase instance with a different read concern setting.
        /// </summary>
        /// <param name="readConcern">The read concern.</param>
        /// <returns>A new IMongoDatabase instance with a different read concern setting.</returns>
        IMongoDatabase WithReadConcern(ReadConcern readConcern);

        /// <summary>
        /// Returns a new IMongoDatabase instance with a different read preference setting.
        /// </summary>
        /// <param name="readPreference">The read preference.</param>
        /// <returns>A new IMongoDatabase instance with a different read preference setting.</returns>
        IMongoDatabase WithReadPreference(ReadPreference readPreference);

        /// <summary>
        /// Returns a new IMongoDatabase instance with a different write concern setting.
        /// </summary>
        /// <param name="writeConcern">The write concern.</param>
        /// <returns>A new IMongoDatabase instance with a different write concern setting.</returns>
        IMongoDatabase WithWriteConcern(WriteConcern writeConcern);
    }
}
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
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace MongoDB.Driver
{
    /// <summary>
    /// An interface representing methods used to create, delete and modify indexes.
    /// </summary>
    /// <remarks>
    /// This interface is not guaranteed to remain stable. Implementors should use
    /// <see cref="MongoIndexManagerBase{TDocument}"/>.
    /// </remarks>
    /// <typeparam name="TDocument">The type of the document.</typeparam>
    public interface IMongoIndexManager<TDocument>
    {
        /// <summary>
        /// Gets the namespace of the collection.
        /// </summary>
        CollectionNamespace CollectionNamespace { get; }

        /// <summary>
        /// Gets the document serializer.
        /// </summary>
        IBsonSerializer<TDocument> DocumentSerializer { get; }

        /// <summary>
        /// Gets the collection settings.
        /// </summary>
        MongoCollectionSettings Settings { get; }

        /// <summary>
        /// Creates multiple indexes.
        /// </summary>
        /// <param name="models">The models defining each of the indexes.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// An <see cref="IEnumerable{String}" /> of the names of the indexes that were created.
        /// </returns>
        IEnumerable<string> CreateMany(IEnumerable<CreateIndexModel<TDocument>> models, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Creates multiple indexes.
        /// </summary>
        /// <param name="models">The models defining each of the indexes.</param>
        /// <param name="options">The options for create multiple indexes.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// An <see cref="IEnumerable{String}" /> of the names of the indexes that were created.
        /// </returns>
        IEnumerable<string> CreateMany(
            IEnumerable<CreateIndexModel<TDocument>> models,
            CreateManyIndexesOptions options,
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Creates multiple indexes.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="models">The models defining each of the indexes.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// An <see cref="IEnumerable{String}" /> of the names of the indexes that were created.
        /// </returns>
        IEnumerable<string> CreateMany(IClientSessionHandle session, IEnumerable<CreateIndexModel<TDocument>> models, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Creates multiple indexes.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="models">The models defining each of the indexes.</param>
        /// <param name="options">The options for create multiple indexes.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// An <see cref="IEnumerable{String}" /> of the names of the indexes that were created.
        /// </returns>
        IEnumerable<string> CreateMany(
            IClientSessionHandle session,
            IEnumerable<CreateIndexModel<TDocument>> models,
            CreateManyIndexesOptions options,
            CancellationToken cancellationToken = default(CancellationToken));
        
        /// <summary>
        /// Creates multiple indexes.
        /// </summary>
        /// <param name="models">The models defining each of the indexes.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A task whose result is an <see cref="IEnumerable{String}" /> of the names of the indexes that were created.
        /// </returns>
        Task<IEnumerable<string>> CreateManyAsync(IEnumerable<CreateIndexModel<TDocument>> models, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Creates multiple indexes.
        /// </summary>
        /// <param name="models">The models defining each of the indexes.</param>
        /// <param name="options">The options for create multiple indexes.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A task whose result is an <see cref="IEnumerable{String}" /> of the names of the indexes that were created.
        /// </returns>
        Task<IEnumerable<string>> CreateManyAsync(
            IEnumerable<CreateIndexModel<TDocument>> models,
            CreateManyIndexesOptions options,
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Creates multiple indexes.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="models">The models defining each of the indexes.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A task whose result is an <see cref="IEnumerable{String}" /> of the names of the indexes that were created.
        /// </returns>
        Task<IEnumerable<string>> CreateManyAsync(IClientSessionHandle session, IEnumerable<CreateIndexModel<TDocument>> models, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Creates multiple indexes.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="models">The models defining each of the indexes.</param>
        /// <param name="options">The options for create multiple indexes.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A task whose result is an <see cref="IEnumerable{String}" /> of the names of the indexes that were created.
        /// </returns>
        Task<IEnumerable<string>> CreateManyAsync(
            IClientSessionHandle session,
            IEnumerable<CreateIndexModel<TDocument>> models,
            CreateManyIndexesOptions options,
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Creates an index.
        /// </summary>
        /// <param name="model">The model defining the index.</param>
        /// <param name="options">The create index operation options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The name of the index that was created.
        /// </returns>
        string CreateOne(
            CreateIndexModel<TDocument> model,
            CreateOneIndexOptions options = null,
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Creates an index.
        /// </summary>
        /// <param name="keys">The keys.</param>       
        /// <param name="options">The create index request options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The name of the index that was created.
        /// </returns>
        [Obsolete("Use CreateOne with a CreateIndexModel instead.")]
        string CreateOne(IndexKeysDefinition<TDocument> keys, CreateIndexOptions options = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Creates an index.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="keys">The keys.</param>
        /// <param name="options">The create index request options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The name of the index that was created.
        /// </returns>
        [Obsolete("Use CreateOne with a CreateIndexModel instead.")]
        string CreateOne(IClientSessionHandle session, IndexKeysDefinition<TDocument> keys, CreateIndexOptions options = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Creates an index.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="model">The model defining the index.</param>
        /// <param name="options">The create index operation options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The name of the index that was created.
        /// </returns>
        string CreateOne(
            IClientSessionHandle session,
            CreateIndexModel<TDocument> model,
            CreateOneIndexOptions options = null,
            CancellationToken cancellationToken = default(CancellationToken));


        /// <summary>
        /// Creates an index.
        /// </summary>
        /// <param name="model">The model defining the index.</param>
        /// <param name="options">The create index operation options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A task whose result is the name of the index that was created.
        /// </returns>
        Task<string> CreateOneAsync(
            CreateIndexModel<TDocument> model,
            CreateOneIndexOptions options = null,
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Creates an index.
        /// </summary>
        /// <param name="keys">The keys.</param>
        /// <param name="options">The create index request options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A task whose result is the name of the index that was created.
        /// </returns>
        [Obsolete("Use CreateOneAsync with a CreateIndexModel instead.")]
        Task<string> CreateOneAsync(IndexKeysDefinition<TDocument> keys, CreateIndexOptions options = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Creates an index.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="keys">The keys.</param>
        /// <param name="options">The create index request options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A task whose result is the name of the index that was created.
        /// </returns>
        [Obsolete("Use CreateOneAsyc with a CreateIndexModel instead.")]
        Task<string> CreateOneAsync(IClientSessionHandle session, IndexKeysDefinition<TDocument> keys, CreateIndexOptions options = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Creates an index.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="model">The model defining the index.</param>
        /// <param name="options">The create index operation options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A task whose result is the name of the index that was created.
        /// </returns>
        Task<string> CreateOneAsync(
            IClientSessionHandle session,
            CreateIndexModel<TDocument> model,
            CreateOneIndexOptions options = null,
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Drops all the indexes.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        void DropAll(DropIndexOptions options, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Drops all the indexes.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        void DropAll(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Drops all the indexes.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        void DropAll(IClientSessionHandle session, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Drops all the indexes.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        void DropAll(IClientSessionHandle session, DropIndexOptions options, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Drops all the indexes.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A task.
        /// </returns>
        Task DropAllAsync(DropIndexOptions options, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Drops all the indexes.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task.</returns>
        Task DropAllAsync(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Drops all the indexes.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A task.
        /// </returns>
        Task DropAllAsync(IClientSessionHandle session, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Drops all the indexes.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A task.
        /// </returns>
        Task DropAllAsync(IClientSessionHandle session, DropIndexOptions options, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Drops an index by its name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        void DropOne(string name, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Drops an index by its name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="options">The options. </param>
        /// <param name="cancellationToken">The cancellation token.</param>
        void DropOne(string name, DropIndexOptions options, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Drops an index by its name.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="name">The name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        void DropOne(IClientSessionHandle session, string name, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Drops an index by its name.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="name">The name.</param>
        /// <param name="options">The options. </param>
        /// <param name="cancellationToken">The cancellation token.</param>
        void DropOne(IClientSessionHandle session, string name, DropIndexOptions options, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Drops an index by its name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task.</returns>
        Task DropOneAsync(string name, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Drops an index by its name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="options">The options. </param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task.</returns>
        Task DropOneAsync(string name, DropIndexOptions options, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Drops an index by its name.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="name">The name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A task.
        /// </returns>
        Task DropOneAsync(IClientSessionHandle session, string name, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Drops an index by its name.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="name">The name.</param>
        /// <param name= "options">The options. </param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A task.
        /// </returns>
        Task DropOneAsync(IClientSessionHandle session, string name, DropIndexOptions options, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Lists the indexes.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A cursor.</returns>
        IAsyncCursor<BsonDocument> List(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Lists the indexes.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A cursor.
        /// </returns>
        IAsyncCursor<BsonDocument> List(IClientSessionHandle session, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Lists the indexes.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task whose result is a cursor.</returns>
        Task<IAsyncCursor<BsonDocument>> ListAsync(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Lists the indexes.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A Task whose result is a cursor.
        /// </returns>
        Task<IAsyncCursor<BsonDocument>> ListAsync(IClientSessionHandle session, CancellationToken cancellationToken = default(CancellationToken));
    }
}

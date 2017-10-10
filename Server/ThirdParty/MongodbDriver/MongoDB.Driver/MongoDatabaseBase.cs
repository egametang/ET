/* Copyright 2010-2016 MongoDB Inc.
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
using MongoDB.Bson;

namespace MongoDB.Driver
{
    /// <summary>
    /// Base class for implementors of <see cref="IMongoDatabase" />.
    /// </summary>
    public abstract class MongoDatabaseBase : IMongoDatabase
    {
        // public properties
        /// <inheritdoc />
        public abstract IMongoClient Client { get; }

        /// <inheritdoc />
        public abstract DatabaseNamespace DatabaseNamespace { get; }

        /// <inheritdoc />
        public abstract MongoDatabaseSettings Settings { get; }

        // public methods
        /// <inheritdoc />
        public virtual void CreateCollection(string name, CreateCollectionOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public abstract Task CreateCollectionAsync(string name, CreateCollectionOptions options = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <inheritdoc />
        public virtual void CreateView<TDocument, TResult>(string viewName, string viewOn, PipelineDefinition<TDocument, TResult> pipeline, CreateViewOptions<TDocument> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public virtual Task CreateViewAsync<TDocument, TResult>(string viewName, string viewOn, PipelineDefinition<TDocument, TResult> pipeline, CreateViewOptions<TDocument> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public virtual void DropCollection(string name, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public abstract Task DropCollectionAsync(string name, CancellationToken cancellationToken = default(CancellationToken));

        /// <inheritdoc />
        public abstract IMongoCollection<TDocument> GetCollection<TDocument>(string name, MongoCollectionSettings settings = null);

        /// <inheritdoc />
        public virtual IAsyncCursor<BsonDocument> ListCollections(ListCollectionsOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public abstract Task<IAsyncCursor<BsonDocument>> ListCollectionsAsync(ListCollectionsOptions options = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <inheritdoc />
        public virtual void RenameCollection(string oldName, string newName, RenameCollectionOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public abstract Task RenameCollectionAsync(string oldName, string newName, RenameCollectionOptions options = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <inheritdoc />
        public virtual TResult RunCommand<TResult>(Command<TResult> command, ReadPreference readPreference = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public abstract Task<TResult> RunCommandAsync<TResult>(Command<TResult> command, ReadPreference readPreference = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <inheritdoc />
        public virtual IMongoDatabase WithReadConcern(ReadConcern readConcern)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public virtual IMongoDatabase WithReadPreference(ReadPreference readPreference)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public virtual IMongoDatabase WithWriteConcern(WriteConcern writeConcern)
        {
            throw new NotImplementedException();
        }
    }
}

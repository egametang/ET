/* Copyright 2010-2015 MongoDB Inc.
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
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace MongoDB.Driver
{
    /// <summary>
    /// Base class for implementors of <see cref="IMongoIndexManager{TDocument}"/>.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document.</typeparam>
    public abstract class MongoIndexManagerBase<TDocument> : IMongoIndexManager<TDocument>
    {
        // public properties
        /// <inheritdoc />
        public abstract CollectionNamespace CollectionNamespace { get; }

        /// <inheritdoc />
        public abstract IBsonSerializer<TDocument> DocumentSerializer { get; }

        /// <inheritdoc />
        public abstract MongoCollectionSettings Settings { get; }

        // public methods
        /// <inheritdoc />
        public virtual string CreateOne(IndexKeysDefinition<TDocument> keys, CreateIndexOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var model = new CreateIndexModel<TDocument>(keys, options);
            var result = CreateMany(new[] { model }, cancellationToken);
            return result.Single();
        }

        /// <inheritdoc />
        public virtual async Task<string> CreateOneAsync(IndexKeysDefinition<TDocument> keys, CreateIndexOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var model = new CreateIndexModel<TDocument>(keys, options);
            var result = await CreateManyAsync(new[] { model }, cancellationToken).ConfigureAwait(false);
            return result.Single();
        }

        /// <inheritdoc />
        public virtual IEnumerable<string> CreateMany(IEnumerable<CreateIndexModel<TDocument>> models, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public virtual Task<IEnumerable<string>> CreateManyAsync(IEnumerable<CreateIndexModel<TDocument>> models, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException("CreateManyAsync has not been implemented.");
        }

        /// <inheritdoc />
        public virtual void DropAll(CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public abstract Task DropAllAsync(CancellationToken cancellationToken = default(CancellationToken));

        /// <inheritdoc />
        public virtual void DropOne(string name, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public abstract Task DropOneAsync(string name, CancellationToken cancellationToken = default(CancellationToken));

        /// <inheritdoc />
        public virtual IAsyncCursor<BsonDocument> List(CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public abstract Task<IAsyncCursor<BsonDocument>> ListAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}

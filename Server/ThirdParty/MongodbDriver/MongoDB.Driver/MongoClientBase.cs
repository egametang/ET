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
using MongoDB.Driver.Core.Clusters;

namespace MongoDB.Driver
{
    /// <summary>
    /// Base class for implementors of <see cref="IMongoClient"/>.
    /// </summary>
    public abstract class MongoClientBase : IMongoClient
    {
        /// <inheritdoc />
        public abstract ICluster Cluster { get;  }

        /// <inheritdoc />
        public abstract MongoClientSettings Settings { get; }

        /// <inheritdoc />
        public virtual void DropDatabase(string name, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public abstract Task DropDatabaseAsync(string name, CancellationToken cancellationToken = default(CancellationToken));

        /// <inheritdoc />
        public abstract IMongoDatabase GetDatabase(string name, MongoDatabaseSettings settings = null);

        /// <inheritdoc />
        public virtual IAsyncCursor<BsonDocument> ListDatabases(CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public abstract Task<IAsyncCursor<BsonDocument>> ListDatabasesAsync(CancellationToken cancellationToken = default(CancellationToken));

        /// <inheritdoc />
        public virtual IMongoClient WithReadConcern(ReadConcern readConcern)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public virtual IMongoClient WithReadPreference(ReadPreference readPreference)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public virtual IMongoClient WithWriteConcern(WriteConcern writeConcern)
        {
            throw new NotImplementedException();
        }
    }
}

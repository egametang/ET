/* Copyright 2018-present MongoDB Inc.
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
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver
{
    /// <summary>
    /// Extension methods on IMongoDatabase.
    /// </summary>
    public static class IMongoDatabaseExtensions
    {
        /// <summary>
        /// Watches changes on all collection in a database.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A change stream.
        /// </returns>
        public static IAsyncCursor<ChangeStreamDocument<BsonDocument>> Watch(
            this IMongoDatabase database,
            ChangeStreamOptions options = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(database, nameof(database));
            var emptyPipeline = new EmptyPipelineDefinition<ChangeStreamDocument<BsonDocument>>();
            return database.Watch(emptyPipeline, options, cancellationToken);
        }

        /// <summary>
        /// Watches changes on all collection in a database.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="session">The session.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A change stream.
        /// </returns>
        public static IAsyncCursor<ChangeStreamDocument<BsonDocument>> Watch(
            this IMongoDatabase database,
            IClientSessionHandle session,
            ChangeStreamOptions options = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(database, nameof(database));
            Ensure.IsNotNull(session, nameof(session));
            var emptyPipeline = new EmptyPipelineDefinition<ChangeStreamDocument<BsonDocument>>();
            return database.Watch(session, emptyPipeline, options, cancellationToken);
        }

        /// <summary>
        /// Watches changes on all collection in a database.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A change stream.
        /// </returns>
        public static Task<IAsyncCursor<ChangeStreamDocument<BsonDocument>>> WatchAsync(
            this IMongoDatabase database,
            ChangeStreamOptions options = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(database, nameof(database));
            var emptyPipeline = new EmptyPipelineDefinition<ChangeStreamDocument<BsonDocument>>();
            return database.WatchAsync(emptyPipeline, options, cancellationToken);
        }

        /// <summary>
        /// Watches changes on all collection in a database.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="session">The session.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A change stream.
        /// </returns>
        public static Task<IAsyncCursor<ChangeStreamDocument<BsonDocument>>> WatchAsync(
            this IMongoDatabase database,
            IClientSessionHandle session,
            ChangeStreamOptions options = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(database, nameof(database));
            Ensure.IsNotNull(session, nameof(session));
            var emptyPipeline = new EmptyPipelineDefinition<ChangeStreamDocument<BsonDocument>>();
            return database.WatchAsync(session, emptyPipeline, options, cancellationToken);
        }
    }
}

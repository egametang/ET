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
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization;

namespace MongoDB.Driver
{
    /// <summary>
    /// Base class for implementors of <see cref="IFindFluent{TDocument, TProjection}" />.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document.</typeparam>
    /// <typeparam name="TProjection">The type of the projection (same as TDocument if there is no projection).</typeparam>
    public abstract class FindFluentBase<TDocument, TProjection> : IOrderedFindFluent<TDocument, TProjection>
    {
        /// <inheritdoc />
        public abstract FilterDefinition<TDocument> Filter { get; set; }

        /// <inheritdoc />
        public abstract FindOptions<TDocument, TProjection> Options { get; }

        /// <inheritdoc />
        public abstract IFindFluent<TDocument, TResult> As<TResult>(IBsonSerializer<TResult> resultSerializer);

        /// <inheritdoc />
        public virtual long Count(CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public abstract Task<long> CountAsync(CancellationToken cancellationToken = default(CancellationToken));

        /// <inheritdoc />
        public abstract IFindFluent<TDocument, TProjection> Limit(int? limit);

        /// <inheritdoc />
        public abstract IFindFluent<TDocument, TNewProjection> Project<TNewProjection>(ProjectionDefinition<TDocument, TNewProjection> projection);

        /// <inheritdoc />
        public abstract IFindFluent<TDocument, TProjection> Skip(int? skip);

        /// <inheritdoc />
        public abstract IFindFluent<TDocument, TProjection> Sort(SortDefinition<TDocument> sort);

        /// <inheritdoc />
        public virtual IAsyncCursor<TProjection> ToCursor(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public abstract Task<IAsyncCursor<TProjection>> ToCursorAsync(CancellationToken cancellationToken);
    }
}

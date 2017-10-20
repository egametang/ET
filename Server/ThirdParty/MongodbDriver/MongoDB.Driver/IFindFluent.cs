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

using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization;

namespace MongoDB.Driver
{
    /// <summary>
    /// Fluent interface for find.
    /// </summary>
    /// <remarks>
    /// This interface is not guaranteed to remain stable. Implementors should use
    /// <see cref="FindFluentBase{TDocument, TProjection}" />.
    /// </remarks>
    /// <typeparam name="TDocument">The type of the document.</typeparam>
    /// <typeparam name="TProjection">The type of the projection (same as TDocument if there is no projection).</typeparam>
    public interface IFindFluent<TDocument, TProjection> : IAsyncCursorSource<TProjection>
    {
        /// <summary>
        /// Gets or sets the filter.
        /// </summary>
        FilterDefinition<TDocument> Filter { get; set; }

        /// <summary>
        /// Gets the options.
        /// </summary>
        FindOptions<TDocument, TProjection> Options { get; }

        /// <summary>
        /// A simplified type of projection that changes the result type by using a different serializer.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="resultSerializer">The result serializer.</param>
        /// <returns>The fluent find interface.</returns>
        IFindFluent<TDocument, TResult> As<TResult>(IBsonSerializer<TResult> resultSerializer = null);

        /// <summary>
        /// Counts the number of documents.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The count.</returns>
        long Count(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Counts the number of documents.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task whose result is the count.</returns>
        Task<long> CountAsync(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Limits the number of documents.
        /// </summary>
        /// <param name="limit">The limit.</param>
        /// <returns>The fluent find interface.</returns>
        IFindFluent<TDocument, TProjection> Limit(int? limit);

        /// <summary>
        /// Projects the the result.
        /// </summary>
        /// <typeparam name="TNewProjection">The type of the projection.</typeparam>
        /// <param name="projection">The projection.</param>
        /// <returns>The fluent find interface.</returns>
        IFindFluent<TDocument, TNewProjection> Project<TNewProjection>(ProjectionDefinition<TDocument, TNewProjection> projection);

        /// <summary>
        /// Skips the the specified number of documents.
        /// </summary>
        /// <param name="skip">The skip.</param>
        /// <returns>The fluent find interface.</returns>
        IFindFluent<TDocument, TProjection> Skip(int? skip);

        /// <summary>
        /// Sorts the the documents.
        /// </summary>
        /// <param name="sort">The sort.</param>
        /// <returns>The fluent find interface.</returns>
        IFindFluent<TDocument, TProjection> Sort(SortDefinition<TDocument> sort);
    }

    /// <summary>
    /// Fluent interface for find.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document.</typeparam>
    /// <typeparam name="TProjection">The type of the projection (same as TDocument if there is no projection).</typeparam>
    public interface IOrderedFindFluent<TDocument, TProjection> : IFindFluent<TDocument, TProjection>
    {
    }
}

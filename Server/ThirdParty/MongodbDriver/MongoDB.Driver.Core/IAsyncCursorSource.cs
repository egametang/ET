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
using MongoDB.Driver.Core.Operations;

namespace MongoDB.Driver
{
    /// <summary>
    /// Represents an operation that will return a cursor when executed.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document.</typeparam>
    public interface IAsyncCursorSource<TDocument>
    {
        /// <summary>
        /// Executes the operation and returns a cursor to the results.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A cursor.</returns>
        IAsyncCursor<TDocument> ToCursor(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Executes the operation and returns a cursor to the results.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task whose result is a cursor.</returns>
        Task<IAsyncCursor<TDocument>> ToCursorAsync(CancellationToken cancellationToken = default(CancellationToken));
    }

    /// <summary>
    /// Represents extension methods for IAsyncCursorSource.
    /// </summary>
    public static class IAsyncCursorSourceExtensions
    {
        /// <summary>
        /// Determines whether the cursor returned by a cursor source contains any documents.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>True if the cursor contains any documents.</returns>
        public static bool Any<TDocument>(this IAsyncCursorSource<TDocument> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var cursor = source.ToCursor(cancellationToken))
            {
                return cursor.Any(cancellationToken);
            }
        }

        /// <summary>
        /// Determines whether the cursor returned by a cursor source contains any documents.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task whose result is true if the cursor contains any documents.</returns>
        public static async Task<bool> AnyAsync<TDocument>(this IAsyncCursorSource<TDocument> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var cursor = await source.ToCursorAsync(cancellationToken).ConfigureAwait(false))
            {
                return await cursor.AnyAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Returns the first document of a cursor returned by a cursor source.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The first document.</returns>
        public static TDocument First<TDocument>(this IAsyncCursorSource<TDocument> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var cursor = source.ToCursor(cancellationToken))
            {
                return cursor.First(cancellationToken);
            }
        }

        /// <summary>
        /// Returns the first document of a cursor returned by a cursor source.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task whose result is the first document.</returns>
        public static async Task<TDocument> FirstAsync<TDocument>(this IAsyncCursorSource<TDocument> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var cursor = await source.ToCursorAsync(cancellationToken).ConfigureAwait(false))
            {
                return await cursor.FirstAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Returns the first document of a cursor returned by a cursor source, or a default value if the cursor contains no documents.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The first document of the cursor, or a default value if the cursor contains no documents.</returns>
        public static TDocument FirstOrDefault<TDocument>(this IAsyncCursorSource<TDocument> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var cursor = source.ToCursor(cancellationToken))
            {
                return cursor.FirstOrDefault(cancellationToken);
            }
        }

        /// <summary>
        /// Returns the first document of a cursor returned by a cursor source, or a default value if the cursor contains no documents.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task whose result is the first document of the cursor, or a default value if the cursor contains no documents.</returns>
        public static async Task<TDocument> FirstOrDefaultAsync<TDocument>(this IAsyncCursorSource<TDocument> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var cursor = await source.ToCursorAsync(cancellationToken).ConfigureAwait(false))
            {
                return await cursor.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Calls a delegate for each document returned by the cursor.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="processor">The processor.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task that completes when all the documents have been processed.</returns>
        public static async Task ForEachAsync<TDocument>(this IAsyncCursorSource<TDocument> source, Func<TDocument, Task> processor, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var cursor = await source.ToCursorAsync(cancellationToken).ConfigureAwait(false))
            {
                await cursor.ForEachAsync(processor, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Calls a delegate for each document returned by the cursor.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="processor">The processor.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task that completes when all the documents have been processed.</returns>
        public static async Task ForEachAsync<TDocument>(this IAsyncCursorSource<TDocument> source, Func<TDocument, int, Task> processor, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var cursor = await source.ToCursorAsync(cancellationToken).ConfigureAwait(false))
            {
                await cursor.ForEachAsync(processor, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Calls a delegate for each document returned by the cursor.
        /// </summary>
        /// <remarks>
        /// If your delegate is going to take a long time to execute or is going to block
        /// consider using a different overload of ForEachAsync that uses a delegate that
        /// returns a Task instead.
        /// </remarks>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="processor">The processor.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task that completes when all the documents have been processed.</returns>
        public static async Task ForEachAsync<TDocument>(this IAsyncCursorSource<TDocument> source, Action<TDocument> processor, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var cursor = await source.ToCursorAsync(cancellationToken).ConfigureAwait(false))
            {
                await cursor.ForEachAsync(processor, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Calls a delegate for each document returned by the cursor.
        /// </summary>
        /// <remarks>
        /// If your delegate is going to take a long time to execute or is going to block
        /// consider using a different overload of ForEachAsync that uses a delegate that
        /// returns a Task instead.
        /// </remarks>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="processor">The processor.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task that completes when all the documents have been processed.</returns>
        public static async Task ForEachAsync<TDocument>(this IAsyncCursorSource<TDocument> source, Action<TDocument, int> processor, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var cursor = await source.ToCursorAsync(cancellationToken).ConfigureAwait(false))
            {
                await cursor.ForEachAsync(processor, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Returns the only document of a cursor returned by a cursor source. This method throws an exception if the cursor does not contain exactly one document.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The only document of a cursor.</returns>
        public static TDocument Single<TDocument>(this IAsyncCursorSource<TDocument> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var cursor = source.ToCursor(cancellationToken))
            {
                return cursor.Single(cancellationToken);
            }
        }

        /// <summary>
        /// Returns the only document of a cursor returned by a cursor source. This method throws an exception if the cursor does not contain exactly one document.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task whose result is the only document of a cursor.</returns>
        public static async Task<TDocument> SingleAsync<TDocument>(this IAsyncCursorSource<TDocument> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var cursor = await source.ToCursorAsync(cancellationToken).ConfigureAwait(false))
            {
                return await cursor.SingleAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Returns the only document of a cursor returned by a cursor source, or a default value if the cursor contains no documents.
        /// This method throws an exception if the cursor contains more than one document.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The only document of a cursor, or a default value if the cursor contains no documents.</returns>
        public static TDocument SingleOrDefault<TDocument>(this IAsyncCursorSource<TDocument> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var cursor = source.ToCursor(cancellationToken))
            {
                return cursor.SingleOrDefault(cancellationToken);
            }
        }

        /// <summary>
        /// Returns the only document of a cursor returned by a cursor source, or a default value if the cursor contains no documents.
        /// This method throws an exception if the cursor contains more than one document.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task whose result is the only document of a cursor, or a default value if the cursor contains no documents.</returns>
        public static async Task<TDocument> SingleOrDefaultAsync<TDocument>(this IAsyncCursorSource<TDocument> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var cursor = await source.ToCursorAsync(cancellationToken).ConfigureAwait(false))
            {
                return await cursor.SingleOrDefaultAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Wraps a cursor source in an IEnumerable. Each time GetEnumerator is called a new cursor is fetched from the cursor source.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An IEnumerable.</returns>
        public static IEnumerable<TDocument> ToEnumerable<TDocument>(this IAsyncCursorSource<TDocument> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            return new AsyncCursorSourceEnumerableAdapter<TDocument>(source, cancellationToken);
        }

        /// <summary>
        /// Returns a list containing all the documents returned by the cursor returned by a cursor source.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The list of documents.</returns>
        public static List<TDocument> ToList<TDocument>(this IAsyncCursorSource<TDocument> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var cursor = source.ToCursor(cancellationToken))
            {
                return cursor.ToList(cancellationToken);
            }
        }

        /// <summary>
        /// Returns a list containing all the documents returned by the cursor returned by a cursor source.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task whose value is the list of documents.</returns>
        public static async Task<List<TDocument>> ToListAsync<TDocument>(this IAsyncCursorSource<TDocument> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var cursor = await source.ToCursorAsync(cancellationToken).ConfigureAwait(false))
            {
                return await cursor.ToListAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}

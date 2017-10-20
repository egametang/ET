/* Copyright 2013-2015 MongoDB Inc.
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
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.Operations;

namespace MongoDB.Driver
{
    /// <summary>
    /// Represents an asynchronous cursor.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document.</typeparam>
    public interface IAsyncCursor<out TDocument> : IDisposable
    {
        /// <summary>
        /// Gets the current batch of documents.
        /// </summary>
        /// <value>
        /// The current batch of documents.
        /// </value>
        IEnumerable<TDocument> Current { get; }

        /// <summary>
        /// Moves to the next batch of documents.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Whether any more documents are available.</returns>
        bool MoveNext(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Moves to the next batch of documents.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task whose result indicates whether any more documents are available.</returns>
        Task<bool> MoveNextAsync(CancellationToken cancellationToken = default(CancellationToken));
    }

    /// <summary>
    /// Represents extension methods for IAsyncCursor.
    /// </summary>
    public static class IAsyncCursorExtensions
    {
        /// <summary>
        /// Determines whether the cursor contains any documents.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="cursor">The cursor.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>True if the cursor contains any documents.</returns>
        public static bool Any<TDocument>(this IAsyncCursor<TDocument> cursor, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (cursor)
            {
                var batch = GetFirstBatch(cursor, cancellationToken);
                return batch.Any();
            }
        }

        /// <summary>
        /// Determines whether the cursor contains any documents.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="cursor">The cursor.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task whose result is true if the cursor contains any documents.</returns>
        public static async Task<bool> AnyAsync<TDocument>(this IAsyncCursor<TDocument> cursor, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (cursor)
            {
                var batch = await GetFirstBatchAsync(cursor, cancellationToken).ConfigureAwait(false);
                return batch.Any();
            }
        }

        /// <summary>
        /// Returns the first document of a cursor.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="cursor">The cursor.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The first document.</returns>
        public static TDocument First<TDocument>(this IAsyncCursor<TDocument> cursor, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (cursor)
            {
                var batch = GetFirstBatch(cursor, cancellationToken);
                return batch.First();
            }
        }

        /// <summary>
        /// Returns the first document of a cursor.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="cursor">The cursor.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task whose result is the first document.</returns>
        public static async Task<TDocument> FirstAsync<TDocument>(this IAsyncCursor<TDocument> cursor, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (cursor)
            {
                var batch = await GetFirstBatchAsync(cursor, cancellationToken).ConfigureAwait(false);
                return batch.First();
            }
        }

        /// <summary>
        /// Returns the first document of a cursor, or a default value if the cursor contains no documents.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="cursor">The cursor.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The first document of the cursor, or a default value if the cursor contains no documents.</returns>
        public static TDocument FirstOrDefault<TDocument>(this IAsyncCursor<TDocument> cursor, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (cursor)
            {
                var batch = GetFirstBatch(cursor, cancellationToken);
                return batch.FirstOrDefault();
            }
        }

        /// <summary>
        /// Returns the first document of the cursor, or a default value if the cursor contains no documents.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="cursor">The cursor.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task whose result is the first document of the cursor, or a default value if the cursor contains no documents.</returns>
        public static async Task<TDocument> FirstOrDefaultAsync<TDocument>(this IAsyncCursor<TDocument> cursor, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (cursor)
            {
                var batch = await GetFirstBatchAsync(cursor, cancellationToken).ConfigureAwait(false);
                return batch.FirstOrDefault();
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
        public static Task ForEachAsync<TDocument>(this IAsyncCursor<TDocument> source, Func<TDocument, Task> processor, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ForEachAsync(source, (doc, _) => processor(doc), cancellationToken);
        }

        /// <summary>
        /// Calls a delegate for each document returned by the cursor.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="processor">The processor.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task that completes when all the documents have been processed.</returns>
        public static async Task ForEachAsync<TDocument>(this IAsyncCursor<TDocument> source, Func<TDocument, int, Task> processor, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(source, nameof(source));
            Ensure.IsNotNull(processor, nameof(processor));

            // yes, we are taking ownership... assumption being that they've
            // exhausted the thing and don't need it anymore.
            using (source)
            {
                var index = 0;
                while (await source.MoveNextAsync(cancellationToken).ConfigureAwait(false))
                {
                    foreach (var document in source.Current)
                    {
                        await processor(document, index++).ConfigureAwait(false);
                        cancellationToken.ThrowIfCancellationRequested();
                    }
                }
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
        public static Task ForEachAsync<TDocument>(this IAsyncCursor<TDocument> source, Action<TDocument> processor, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ForEachAsync(source, (doc, _) => processor(doc), cancellationToken);
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
        public static async Task ForEachAsync<TDocument>(this IAsyncCursor<TDocument> source, Action<TDocument, int> processor, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(source, nameof(source));
            Ensure.IsNotNull(processor, nameof(processor));

            // yes, we are taking ownership... assumption being that they've
            // exhausted the thing and don't need it anymore.
            using (source)
            {
                var index = 0;
                while (await source.MoveNextAsync(cancellationToken).ConfigureAwait(false))
                {
                    foreach (var document in source.Current)
                    {
                        processor(document, index++);
                        cancellationToken.ThrowIfCancellationRequested();
                    }
                }
            }
        }

        /// <summary>
        /// Returns the only document of a cursor. This method throws an exception if the cursor does not contain exactly one document.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="cursor">The cursor.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The only document of a cursor.</returns>
        public static TDocument Single<TDocument>(this IAsyncCursor<TDocument> cursor, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (cursor)
            {
                var batch = GetFirstBatch(cursor, cancellationToken);
                return batch.Single();
            }
        }

        /// <summary>
        /// Returns the only document of a cursor. This method throws an exception if the cursor does not contain exactly one document.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="cursor">The cursor.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task whose result is the only document of a cursor.</returns>
        public static async Task<TDocument> SingleAsync<TDocument>(this IAsyncCursor<TDocument> cursor, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (cursor)
            {
                var batch = await GetFirstBatchAsync(cursor, cancellationToken).ConfigureAwait(false);
                return batch.Single();
            }
        }

        /// <summary>
        /// Returns the only document of a cursor, or a default value if the cursor contains no documents.
        /// This method throws an exception if the cursor contains more than one document.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="cursor">The cursor.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The only document of a cursor, or a default value if the cursor contains no documents.</returns>
        public static TDocument SingleOrDefault<TDocument>(this IAsyncCursor<TDocument> cursor, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (cursor)
            {
                var batch = GetFirstBatch(cursor, cancellationToken);
                return batch.SingleOrDefault();
            }
        }

        /// <summary>
        /// Returns the only document of a cursor, or a default value if the cursor contains no documents.
        /// This method throws an exception if the cursor contains more than one document.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="cursor">The cursor.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task whose result is the only document of a cursor, or a default value if the cursor contains no documents.</returns>
        public static async Task<TDocument> SingleOrDefaultAsync<TDocument>(this IAsyncCursor<TDocument> cursor, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (cursor)
            {
                var batch = await GetFirstBatchAsync(cursor, cancellationToken).ConfigureAwait(false);
                return batch.SingleOrDefault();
            }
        }

        /// <summary>
        /// Wraps a cursor in an IEnumerable that can be enumerated one time.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="cursor">The cursor.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An IEnumerable</returns>
        public static IEnumerable<TDocument> ToEnumerable<TDocument>(this IAsyncCursor<TDocument> cursor, CancellationToken cancellationToken = default(CancellationToken))
        {
            return new AsyncCursorEnumerableOneTimeAdapter<TDocument>(cursor, cancellationToken);
        }

        /// <summary>
        /// Returns a list containing all the documents returned by a cursor.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The list of documents.</returns>
        public static List<TDocument> ToList<TDocument>(this IAsyncCursor<TDocument> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(source, nameof(source));

            var list = new List<TDocument>();

            // yes, we are taking ownership... assumption being that they've
            // exhausted the thing and don't need it anymore.
            using (source)
            {
                while (source.MoveNext(cancellationToken))
                {
                    list.AddRange(source.Current);
                    cancellationToken.ThrowIfCancellationRequested();
                }
            }
            return list;
        }

        /// <summary>
        /// Returns a list containing all the documents returned by a cursor.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task whose value is the list of documents.</returns>
        public static async Task<List<TDocument>> ToListAsync<TDocument>(this IAsyncCursor<TDocument> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            Ensure.IsNotNull(source, nameof(source));

            var list = new List<TDocument>();

            // yes, we are taking ownership... assumption being that they've
            // exhausted the thing and don't need it anymore.
            using (source)
            {
                while (await source.MoveNextAsync(cancellationToken).ConfigureAwait(false))
                {
                    list.AddRange(source.Current);
                    cancellationToken.ThrowIfCancellationRequested();
                }
            }
            return list;
        }

        // private methods
        private static IEnumerable<TDocument> GetFirstBatch<TDocument>(IAsyncCursor<TDocument> cursor, CancellationToken cancellationToken)
        {
            if (cursor.MoveNext(cancellationToken))
            {
                return cursor.Current;
            }
            else
            {
                return Enumerable.Empty<TDocument>();
            }
        }

        private static async Task<IEnumerable<TDocument>> GetFirstBatchAsync<TDocument>(IAsyncCursor<TDocument> cursor, CancellationToken cancellationToken)
        {
            if (await cursor.MoveNextAsync(cancellationToken).ConfigureAwait(false))
            {
                return cursor.Current;
            }
            else
            {
                return Enumerable.Empty<TDocument>();
            }
        }
    }
}

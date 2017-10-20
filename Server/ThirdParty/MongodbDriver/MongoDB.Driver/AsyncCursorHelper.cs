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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDB.Driver
{
    internal static class AsyncCursorHelper
    {
        public async static Task<bool> AnyAsync<T>(Task<IAsyncCursor<T>> cursorTask, CancellationToken cancellationToken)
        {
            using (var cursor = await cursorTask.ConfigureAwait(false))
            {
                while (await cursor.MoveNextAsync(cancellationToken).ConfigureAwait(false))
                {
                    var current = cursor.Current;
                    if (current.Any())
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public async static Task<T> FirstAsync<T>(Task<IAsyncCursor<T>> cursorTask, CancellationToken cancellationToken)
        {
            using (var cursor = await cursorTask.ConfigureAwait(false))
            {
                while (await cursor.MoveNextAsync(cancellationToken).ConfigureAwait(false))
                {
                    var current = cursor.Current;
                    if (current.Any())
                    {
                        return current.First();
                    }
                }

                throw new InvalidOperationException("The source sequence is empty.");
            }
        }

        public async static Task<T> FirstOrDefaultAsync<T>(Task<IAsyncCursor<T>> cursorTask, CancellationToken cancellationToken)
        {
            using (var cursor = await cursorTask.ConfigureAwait(false))
            {
                while (await cursor.MoveNextAsync(cancellationToken).ConfigureAwait(false))
                {
                    var current = cursor.Current;
                    if (current.Any())
                    {
                        return current.FirstOrDefault();
                    }
                }

                return default(T);
            }
        }

        public async static Task<T> SingleAsync<T>(Task<IAsyncCursor<T>> cursorTask, CancellationToken cancellationToken)
        {
            using (var cursor = await cursorTask.ConfigureAwait(false))
            {
                while (await cursor.MoveNextAsync(cancellationToken).ConfigureAwait(false))
                {
                    var current = cursor.Current;
                    if (current.Any())
                    {
                        return current.Single();
                    }
                }

                throw new InvalidOperationException("The source sequence is empty.");
            }
        }

        public async static Task<T> SingleOrDefaultAsync<T>(Task<IAsyncCursor<T>> cursorTask, CancellationToken cancellationToken)
        {
            using (var cursor = await cursorTask.ConfigureAwait(false))
            {
                while (await cursor.MoveNextAsync(cancellationToken).ConfigureAwait(false))
                {
                    var current = cursor.Current;
                    if (current.Any())
                    {
                        return current.SingleOrDefault();
                    }
                }

                return default(T);
            }
        }
    }
}

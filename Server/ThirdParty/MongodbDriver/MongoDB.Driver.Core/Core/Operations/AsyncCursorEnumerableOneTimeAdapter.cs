/* Copyright 2015 MongoDB Inc.
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
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Core.Operations
{
    internal class AsyncCursorEnumerableOneTimeAdapter<TDocument> : IEnumerable<TDocument>
    {
        // private fields
        private readonly CancellationToken _cancellationToken;
        private readonly IAsyncCursor<TDocument> _cursor;
        private bool _hasBeenEnumerated;

        // constructors
        public AsyncCursorEnumerableOneTimeAdapter(IAsyncCursor<TDocument> cursor, CancellationToken cancellationToken)
        {
            _cursor = Ensure.IsNotNull(cursor, nameof(cursor));
            _cancellationToken = cancellationToken;
        }

        // public methods
        public IEnumerator<TDocument> GetEnumerator()
        {
            if (_hasBeenEnumerated)
            {
                throw new InvalidOperationException("An IAsyncCursor can only be enumerated once.");
            }
            _hasBeenEnumerated = true;
            return new AsyncCursorEnumerator<TDocument>(_cursor, _cancellationToken);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}

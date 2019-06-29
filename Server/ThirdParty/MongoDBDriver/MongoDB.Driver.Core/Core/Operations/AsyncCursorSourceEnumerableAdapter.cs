﻿/* Copyright 2015-present MongoDB Inc.
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
    internal class AsyncCursorSourceEnumerableAdapter<TDocument> : IEnumerable<TDocument>
    {
        // private fields
        private readonly CancellationToken _cancellationToken;
        private readonly IAsyncCursorSource<TDocument> _source;

        // constructors
        public AsyncCursorSourceEnumerableAdapter(IAsyncCursorSource<TDocument> source, CancellationToken cancellationToken)
        {
            _source = Ensure.IsNotNull(source, nameof(source));
            _cancellationToken = cancellationToken;
        }

        // public methods
        public IEnumerator<TDocument> GetEnumerator()
        {
            var cursor = _source.ToCursor(_cancellationToken);
            return new AsyncCursorEnumerator<TDocument>(cursor, _cancellationToken);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}

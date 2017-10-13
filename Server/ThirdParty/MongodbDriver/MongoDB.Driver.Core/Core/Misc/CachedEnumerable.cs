﻿/* Copyright 2013-2015 MongoDB Inc.
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

using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MongoDB.Driver.Core.Misc
{
    internal class CachedEnumerable<T> : IEnumerable<T>
    {
        private readonly IEnumerable<T> _enumerable;
        private IReadOnlyList<T> _cached;

        public CachedEnumerable(IEnumerable<T> enumerable)
        {
            _enumerable = enumerable;
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (_cached == null)
            {
                _cached = (_enumerable as IReadOnlyList<T>) ?? _enumerable.ToList();
            }
            return _cached.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}

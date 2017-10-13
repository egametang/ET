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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Driver.Core.Misc
{
    internal class ReadAheadEnumerable<T> : IEnumerable<T>
    {
        private readonly IEnumerable<T> _wrapped;

        public ReadAheadEnumerable(IEnumerable<T> wrapped)
        {
            _wrapped = wrapped;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new ReadAheadEnumerator(_wrapped.GetEnumerator());
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        internal class ReadAheadEnumerator : IEnumerator<T>
        {
            private readonly IEnumerator<T> _wrapped;
            private bool _hasNext;
            private bool _hasStarted;
            private bool _hasCompleted;
            private T _current;
            private T _next;

            internal ReadAheadEnumerator(IEnumerator<T> wrapped)
            {
                _wrapped = wrapped;
            }

            public T Current
            {
                get
                {
                    if (!_hasStarted)
                    {
                        throw new InvalidOperationException("Enumeration has not started. Call MoveNext.");
                    }
                    if (_hasCompleted)
                    {
                        throw new InvalidOperationException("Enumeration already finished.");
                    }

                    return _current;
                }
            }

            object IEnumerator.Current => Current;

            public bool HasNext => _hasNext;

            public void Dispose()
            {
                _wrapped.Dispose();
            }

            public bool MoveNext()
            {
                if (!_hasStarted)
                {
                    _hasStarted = true;
                    if (_hasNext = _wrapped.MoveNext())
                    {
                        _next = _wrapped.Current;
                    }
                }

                if (_hasNext)
                {
                    _current = _next;
                    if (_hasNext = _wrapped.MoveNext())
                    {
                        _next = _wrapped.Current;
                    }

                    return true;
                }

                _hasCompleted = true;
                return false;
            }

            public void Reset()
            {
                _wrapped.Reset();
                _hasStarted = false;
                _hasNext = false;
                _current = default(T);
                _next = default(T);
            }
        }
    }
}

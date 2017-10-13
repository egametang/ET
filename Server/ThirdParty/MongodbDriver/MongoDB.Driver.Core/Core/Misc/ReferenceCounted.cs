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
using System.Threading;

namespace MongoDB.Driver.Core.Misc
{
    internal sealed class ReferenceCounted<T> where T : class, IDisposable
    {
        // fields
        private readonly T _instance;
        private readonly Action<T> _release;
        private int _referenceCount;

        // constructors
        public ReferenceCounted(T instance)
            : this(instance, x => x.Dispose())
        {
        }

        public ReferenceCounted(T instance, Action<T> release)
        {
            _instance = Ensure.IsNotNull(instance, nameof(instance));
            _release = Ensure.IsNotNull(release, nameof(release));
            _referenceCount = 1;
        }

        // properties
        public int ReferenceCount
        {
            get { return Interlocked.CompareExchange(ref _referenceCount, 0, 0); }
        }

        public T Instance
        {
            get { return _instance; }
        }

        // methods
        public void DecrementReferenceCount()
        {
            var value = Interlocked.Decrement(ref _referenceCount);
            if (value == 0)
            {
                _release(_instance);
            }
        }

        public void IncrementReferenceCount()
        {
            Interlocked.Increment(ref _referenceCount);
        }
    }
}
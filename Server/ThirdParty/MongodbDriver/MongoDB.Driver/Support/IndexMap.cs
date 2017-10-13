/* Copyright 2010-2014 MongoDB Inc.
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

using System.Collections.Generic;

namespace MongoDB.Driver.Support
{
    internal abstract class IndexMap
    {
        // private static fields
        private static readonly IndexMap __identityMap = new RangeBased(0, 0, int.MaxValue);

        // public static properties
        public static IndexMap IdentityMap
        {
            get { return __identityMap; }
        }

        // public properties
        public abstract bool IsIdentityMap { get;  }

        // public methods
        public abstract IndexMap Add(int index, int originalIndex);
        public abstract int Map(int index);

        // nested classes
        public class RangeBased : IndexMap
        {
            // private fields
            private int _index;
            private int _originalIndex;
            private int _count;

            // constructors
            public RangeBased()
            {
            }

            public RangeBased(int index, int originalIndex, int count)
            {
                _index = index;
                _originalIndex = originalIndex;
                _count = count;
            }

            // public properties
            public override bool IsIdentityMap
            {
                get { return _index == _originalIndex; }
            }

            // public methods
            public override IndexMap Add(int index, int originalIndex)
            {
                if (_count == 0)
                {
                    _index = index;
                    _originalIndex = originalIndex;
                    _count = 1;
                    return this;
                }
                else if (index == _index + _count && originalIndex == _originalIndex + _count)
                {
                    _count += 1;
                    return this;
                }
                else
                {
                    var dictionaryMap = new DictionaryBased(_index, _originalIndex, _count);
                    dictionaryMap.Add(index, originalIndex);
                    return dictionaryMap;
                }
            }

            public override int Map(int index)
            {
                var offset = index - _index;
                if (offset < 0 || offset >= _count)
                {
                    throw new KeyNotFoundException();
                }
                return _originalIndex + offset;
            }
        }

        public class DictionaryBased : IndexMap
        {
            // private fields
            private readonly Dictionary<int, int> _dictionary = new Dictionary<int, int>();

            // constructors
            public DictionaryBased()
            {
            }

            public DictionaryBased(int index, int originalIndex, int count)
            {
                var limit = index + count;
                for (int i = index, o = originalIndex; i < limit; i++, o++)
                {
                    _dictionary.Add(i, o);
                }
            }

            // public properties
            public override bool IsIdentityMap
            {
                get { return false; }
            }

            // public methods
            public override IndexMap Add(int index, int originalIndex)
            {
                _dictionary.Add(index, originalIndex);
                return this;
            }

            public override int Map(int index)
            {
                return _dictionary[index];
            }
        }
    }
}

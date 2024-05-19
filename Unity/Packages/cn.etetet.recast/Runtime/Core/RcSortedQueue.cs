/*
Copyright (c) 2009-2010 Mikko Mononen memon@inside.org
recast4j copyright (c) 2015-2019 Piotr Piastucki piotr@jtilia.org
DotRecast Copyright (c) 2023 Choi Ikpil ikpil@naver.com

This software is provided 'as-is', without any express or implied
warranty.  In no event will the authors be held liable for any damages
arising from the use of this software.
Permission is granted to anyone to use this software for any purpose,
including commercial applications, and to alter it and redistribute it
freely, subject to the following restrictions:
1. The origin of this software must not be misrepresented; you must not
 claim that you wrote the original software. If you use this software
 in a product, an acknowledgment in the product documentation would be
 appreciated but is not required.
2. Altered source versions must be plainly marked as such, and must not be
 misrepresented as being the original software.
3. This notice may not be removed or altered from any source distribution.
*/

using System;
using System.Collections.Generic;

namespace DotRecast.Core
{
    public class RcSortedQueue<T>
    {
        private bool _dirty;
        private readonly List<T> _items;
        private readonly Comparison<T> _comparison;

        public RcSortedQueue(Comparison<T> comparison)
        {
            _items = new List<T>();
            _comparison = (x, y) => comparison.Invoke(x, y) * -1; // reverse
        }

        public int Count()
        {
            return _items.Count;
        }

        public void Clear()
        {
            _items.Clear();
        }

        private void Balance()
        {
            if (_dirty)
            {
                _items.Sort(_comparison); // reverse
                _dirty = false;
            }
        }

        public T Peek()
        {
            Balance();
            return _items[_items.Count - 1];
        }

        public T Dequeue()
        {
            var node = Peek();
            _items.Remove(node);
            return node;
        }

        public void Enqueue(T item)
        {
            _items.Add(item);
            _dirty = true;
        }

        public void Remove(T item)
        {
            int idx = _items.FindLastIndex(x => item.Equals(x));
            if (0 > idx)
                return;

            _items.RemoveAt(idx);
        }

        public bool IsEmpty()
        {
            return 0 == _items.Count;
        }

        public List<T> ToList()
        {
            Balance();
            var temp = new List<T>(_items);
            temp.Reverse();
            return temp;
        }
    }
}
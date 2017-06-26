using System;
using System.Collections;
using System.Collections.Generic;

namespace UGUI.Collections
{
    internal class Pool<T> where T : new()
    {
        private readonly Stack<T> _stack = new Stack<T>();

        private readonly Action<T> _actionOnGet;

        private readonly Action<T> _actionOnRecycle;

        public int count { get; private set; }
        public int activeCount { get { return count - inactiveCount; } }
        public int inactiveCount { get { return _stack.Count; } }

        public Pool(Action<T> actionOnGet, Action<T> actionOnRecycle)
        {
            _actionOnGet = actionOnGet;
            _actionOnRecycle = actionOnRecycle;
        }

        public T Get()
        {
            T element;
            if (_stack.Count == 0)
            {
                element = new T();
                count++;
            }
            else
            {
                element = _stack.Pop();
            }
            if (_actionOnGet != null)
                _actionOnGet(element);
            return element;
        }

        public void Recycle(T element)
        {
            if (_stack.Count > 0 && ReferenceEquals(_stack.Peek(), element))
            {
                throw new Exception("Internal error. Trying to destroy object that is already released to pool.");
            }

            if (_actionOnRecycle != null)
            {
                _actionOnRecycle(element);
            }
            _stack.Push(element);
        }

        public void Clear()
        {
            _stack.Clear();
            count = 0;
        }
    }
}

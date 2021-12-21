// System.Collections.Generic.Stack<T>
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

[Serializable]
[DebuggerDisplay("Count = {Count}")]
[ComVisible(false)]
public class UncheckedStack<T> : IEnumerable<T>, IEnumerable, ICollection
#if NET_4_6 || NET_STANDARD_2_0
	, IReadOnlyCollection<T>
#endif
{
	[Serializable]
	public struct Enumerator : IEnumerator<T>, IDisposable, IEnumerator
	{
		private UncheckedStack<T> _stack;

		private int _index;

		private int _version;

		private T currentElement;

		public T Current
		{
			get
			{
				return currentElement;
			}
		}

		object IEnumerator.Current
		{
			get
			{
				return currentElement;
			}
		}

		internal Enumerator(UncheckedStack<T> stack)
		{
			_stack = stack;
			_version = _stack._version;
			_index = -2;
			currentElement = default(T);
		}

		public void Dispose()
		{
			_index = -1;
		}

		public bool MoveNext()
		{
			bool flag;
			if (_index == -2)
			{
				_index = _stack._size - 1;
				flag = (_index >= 0);
				if (flag)
				{
					currentElement = _stack._array[_index];
				}
				return flag;
			}
			if (_index == -1)
			{
				return false;
			}
			flag = (--_index >= 0);
			if (flag)
			{
				currentElement = _stack._array[_index];
			}
			else
			{
				currentElement = default(T);
			}
			return flag;
		}
		void IEnumerator.Reset()
		{
			_index = -2;
			currentElement = default(T);
		}
	}

	private T[] _array;

	private int _size;

	private int _version;

	[NonSerialized]
	private object _syncRoot;

	private const int _defaultCapacity = 4;

	private static T[] _emptyArray = new T[0];

	public int Count
	{
		get
		{
			return _size;
		}
	}

	bool ICollection.IsSynchronized
	{
		get
		{
			return false;
		}
	}

	object ICollection.SyncRoot
	{
		get
		{
			if (_syncRoot == null)
			{
				Interlocked.CompareExchange<object>(ref _syncRoot, new object(), (object)null);
			}
			return _syncRoot;
		}
	}

	public UncheckedStack()
	{
		_array = _emptyArray;
		_size = 0;
		_version = 0;
	}

	public UncheckedStack(int capacity)
	{
		_array = new T[capacity];
		_size = 0;
		_version = 0;
	}

	public UncheckedStack(IEnumerable<T> collection)
	{
		ICollection<T> collection2 = collection as ICollection<T>;
		if (collection2 != null)
		{
			int count = collection2.Count;
			_array = new T[count];
			collection2.CopyTo(_array, 0);
			_size = count;
			return;
		}
		_size = 0;
		_array = new T[4];
		foreach (T item in collection)
		{
			T local = item;
			Push(ref local);
		}
	}

	public void Clear()
	{
		Array.Clear(_array, 0, _size);
		_size = 0;
		_version++;
	}

	public bool Contains(T item)
	{
		int size = _size;
		EqualityComparer<T> @default = EqualityComparer<T>.Default;
		while (size-- > 0)
		{
			if (item == null)
			{
				if (_array[size] == null)
				{
					return true;
				}
			}
			else if (_array[size] != null && @default.Equals(_array[size], item))
			{
				return true;
			}
		}
		return false;
	}

	public void CopyTo(T[] array, int arrayIndex)
	{
		Array.Copy(_array, 0, array, arrayIndex, _size);
		Array.Reverse(array, arrayIndex, _size);
	}

	void ICollection.CopyTo(Array array, int arrayIndex)
	{
		Array.Copy(_array, 0, array, arrayIndex, _size);
		Array.Reverse(array, arrayIndex, _size);
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(this);
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return new Enumerator(this);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return new Enumerator(this);
	}
public void TrimExcess()
	{
		int num = (int)((double)_array.Length * 0.9);
		if (_size < num)
		{
			T[] array = new T[_size];
			Array.Copy(_array, 0, array, 0, _size);
			_array = array;
			_version++;
		}
	}

	public T Peek()
	{
		return _array[_size - 1];
	}

	public T Pop()
	{
		return _array[--_size];
	}

	public void Push(ref T item)
	{
		if (_size == _array.Length)
		{
			T[] array = new T[(_array.Length == 0) ? 4 : (2 * _array.Length)];
			Array.Copy(_array, 0, array, 0, _size);
			_array = array;
		}
		_array[_size++] = item;
	}

	public T[] ToArray()
	{
		T[] array = new T[_size];
		for (int i = 0; i < _size; i++)
		{
			array[i] = _array[_size - i - 1];
		}
		return array;
	}
}

// #define RESET_REMOVED_ELEMENTS

namespace IngameDebugConsole
{
	public class CircularBuffer<T>
	{
		private T[] arr;
		private int startIndex;

		public int Count { get; private set; }
		public T this[int index] { get { return arr[( startIndex + index ) % arr.Length]; } }

		public CircularBuffer( int capacity )
		{
			arr = new T[capacity];
		}

		// Old elements are overwritten when capacity is reached
		public void Add( T value )
		{
			if( Count < arr.Length )
				arr[Count++] = value;
			else
			{
				arr[startIndex] = value;
				if( ++startIndex >= arr.Length )
					startIndex = 0;
			}
		}
	}

	public class DynamicCircularBuffer<T>
	{
		private T[] arr;
		private int startIndex;

		public int Count { get; private set; }
		public T this[int index]
		{
			get { return arr[( startIndex + index ) % arr.Length]; }
			set { arr[( startIndex + index ) % arr.Length] = value; }
		}

		public DynamicCircularBuffer( int initialCapacity = 2 )
		{
			arr = new T[initialCapacity];
		}

		public void Add( T value )
		{
			if( Count >= arr.Length )
			{
				int prevSize = arr.Length;
				int newSize = prevSize > 0 ? prevSize * 2 : 2; // Size must be doubled (at least), or the shift operation below must consider IndexOutOfRange situations

				System.Array.Resize( ref arr, newSize );

				if( startIndex > 0 )
				{
					if( startIndex <= ( prevSize - 1 ) / 2 )
					{
						// Move elements [0,startIndex) to the end
						for( int i = 0; i < startIndex; i++ )
						{
							arr[i + prevSize] = arr[i];
#if RESET_REMOVED_ELEMENTS
							arr[i] = default( T );
#endif
						}
					}
					else
					{
						// Move elements [startIndex,prevSize) to the end
						int delta = newSize - prevSize;
						for( int i = prevSize - 1; i >= startIndex; i-- )
						{
							arr[i + delta] = arr[i];
#if RESET_REMOVED_ELEMENTS
							arr[i] = default( T );
#endif
						}

						startIndex += delta;
					}
				}
			}

			this[Count++] = value;
		}

		public T RemoveFirst()
		{
			T element = arr[startIndex];
#if RESET_REMOVED_ELEMENTS
			arr[startIndex] = default( T );
#endif

			if( ++startIndex >= arr.Length )
				startIndex = 0;

			Count--;
			return element;
		}

		public T RemoveLast()
		{
			T element = arr[Count - 1];
#if RESET_REMOVED_ELEMENTS
			arr[Count - 1] = default( T );
#endif

			Count--;
			return element;
		}
	}
}
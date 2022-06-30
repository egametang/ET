namespace IngameDebugConsole
{
	public class DebugLogIndexList
	{
		private int[] indices;
		private int size;

		public int Count { get { return size; } }
		public int this[int index] { get { return indices[index]; } }

		public DebugLogIndexList()
		{
			indices = new int[64];
			size = 0;
		}

		public void Add( int index )
		{
			if( size == indices.Length )
				System.Array.Resize( ref indices, size * 2 );

			indices[size++] = index;
		}

		public void Clear()
		{
			size = 0;
		}

		public int IndexOf( int index )
		{
			return System.Array.IndexOf( indices, index );
		}
	}
}
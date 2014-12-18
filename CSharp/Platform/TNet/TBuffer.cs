using System;
using System.Collections.Generic;

namespace TNet
{
	public class TBuffer
	{
		public const int ChunkSize = 8096;

		private readonly LinkedList<byte[]> bufferList = new LinkedList<byte[]>();

		public int LastIndex { get; set; }

		public int FirstIndex { get; set; }

		public int Count
		{
			get
			{
				if (this.bufferList.Count == 0)
				{
					return 0;
				}
				return (this.bufferList.Count - 1) * ChunkSize + this.LastIndex - this.FirstIndex;
			}
		}

		public void AddLast()
		{
			this.bufferList.AddLast(new byte[ChunkSize]);
		}

		public void RemoveFirst()
		{
			this.bufferList.RemoveFirst();
		}

		public byte[] First
		{
			get
			{
				return this.bufferList.First.Value;
			}
		}

		public byte[] Last
		{
			get
			{
				return this.bufferList.Last.Value;
			}
		}

		public void RecvFrom(byte[] buffer)
		{
			int n = buffer.Length;
			if (this.Count < n || n <= 0)
			{
				throw new Exception(string.Format("bufferList size < n, bufferList: {0} n: {1}", this.Count, n));
			}
			int alreadyCopyCount = 0;
			while (alreadyCopyCount < n)
			{
				if (ChunkSize - this.FirstIndex > n - alreadyCopyCount)
				{ 
					Array.Copy(this.bufferList.First.Value, this.FirstIndex, buffer, alreadyCopyCount,
							n - alreadyCopyCount);
					this.FirstIndex += n - alreadyCopyCount;
					alreadyCopyCount = n;
				}
				else
				{
					Array.Copy(this.bufferList.First.Value, this.FirstIndex, buffer, alreadyCopyCount,
							ChunkSize - this.FirstIndex);
					alreadyCopyCount += ChunkSize - this.FirstIndex;
					this.FirstIndex = 0;
					this.bufferList.RemoveFirst();
				}
			}
		}

		public void SendTo(byte[] buffer)
		{
			int alreadyCopyCount = 0;
			while (alreadyCopyCount < buffer.Length)
			{
				if (this.LastIndex == 0)
				{
					this.bufferList.AddLast(new byte[ChunkSize]);
				}
				if (ChunkSize - this.LastIndex > alreadyCopyCount)
				{
					Array.Copy(buffer, alreadyCopyCount, this.bufferList.Last.Value, this.LastIndex, alreadyCopyCount);
					this.LastIndex += alreadyCopyCount;
					alreadyCopyCount = 0;
				}
				else
				{
					Array.Copy(buffer, alreadyCopyCount, this.bufferList.Last.Value, this.LastIndex,
							ChunkSize - this.LastIndex);
					alreadyCopyCount -= ChunkSize - this.LastIndex;
					this.LastIndex = 0;
				}
			}
		}
	}
}
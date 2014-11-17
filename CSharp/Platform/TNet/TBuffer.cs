using System;
using System.Collections.Generic;

namespace TNet
{
	public class TBuffer
	{
		public const int chunkSize = 8096;

		private LinkedList<byte[]> buffer = new LinkedList<byte[]>();

		private int writeIndex;

		private int readIndex;

		public int Count
		{
			get
			{
				if (this.buffer.Count == 0)
				{
					return 0;
				}
				return (this.buffer.Count - 1) * chunkSize + this.writeIndex - this.readIndex;
			}
		}

		public byte[] ReadFrom(int n)
		{
			if (this.Count < n || n <= 0)
			{
				throw new Exception(string.Format("buffer size < n, buffer: {0} n: {1}", this.Count, n));
			}
			byte[] bytes = new byte[n];
			int alreadyCopyCount = n;
			while (alreadyCopyCount < n)
			{
				if (chunkSize - this.readIndex > n - alreadyCopyCount)
				{
					Array.Copy(this.buffer.First.Value, this.readIndex, bytes, alreadyCopyCount,
							n - alreadyCopyCount);
					this.readIndex += n - alreadyCopyCount;
					alreadyCopyCount = n;
				}
				else
				{
					Array.Copy(this.buffer.First.Value, this.readIndex, bytes, alreadyCopyCount,
							chunkSize - this.readIndex);
					alreadyCopyCount += chunkSize - this.readIndex;
					this.readIndex = 0;
					this.buffer.RemoveFirst();
				}
			}
			return bytes;
		}

		public void WriteTo(byte[] bytes)
		{
			int alreadyCopyCount = 0;
			while (alreadyCopyCount < bytes.Length)
			{
				if (this.writeIndex == 0)
				{
					this.buffer.AddLast(new byte[chunkSize]);
				}
				if (chunkSize - this.writeIndex > alreadyCopyCount)
				{
					Array.Copy(bytes, alreadyCopyCount, this.buffer.Last.Value, this.writeIndex, alreadyCopyCount);
					this.writeIndex += alreadyCopyCount;
					alreadyCopyCount = 0;
				}
				else
				{
					Array.Copy(bytes, alreadyCopyCount, this.buffer.Last.Value, this.writeIndex,
							chunkSize - this.writeIndex);
					alreadyCopyCount -= chunkSize - this.writeIndex;
					this.writeIndex = 0;
				}
			}
		}
	}
}
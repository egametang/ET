using System;
using System.Collections.Generic;
using System.Linq;

namespace Model
{
	public class TBuffer
	{
		public int ChunkSize = 8192;

		private readonly Queue<byte[]> bufferQueue = new Queue<byte[]>();

		private readonly Queue<byte[]> bufferCache = new Queue<byte[]>();

		public int LastIndex { get; set; }

		public int FirstIndex { get; set; }

		public TBuffer()
		{
			this.AddLast();
		}

		public TBuffer(int chunkSize)
		{
			this.ChunkSize = chunkSize;
			this.AddLast();
		}

		public int Count
		{
			get
			{
				int c = 0;
				if (this.bufferQueue.Count == 0)
				{
					c = 0;
				}
				else
				{
					c = (this.bufferQueue.Count - 1) * ChunkSize + this.LastIndex - this.FirstIndex;
				}
				if (c < 0)
				{
					Log.Error("TBuffer count < 0: {0}, {1}, {2}".Fmt(this.bufferQueue.Count, this.LastIndex, this.FirstIndex));
				}
				return c;
			}
		}

		public void AddLast()
		{
			byte[] buffer;
			if (this.bufferCache.Count > 0)
			{
				buffer = this.bufferCache.Dequeue();
			}
			else
			{
				buffer = new byte[ChunkSize];
			}
			this.bufferQueue.Enqueue(buffer);
		}

		public void RemoveFirst()
		{
			this.bufferCache.Enqueue(this.bufferQueue.First());
			this.bufferQueue.Dequeue();
		}

		public byte[] First
		{
			get
			{
				if (this.bufferQueue.Count == 0)
				{
					this.AddLast();
				}
				return this.bufferQueue.First();
			}
		}

		public byte[] Last
		{
			get
			{
				if (this.bufferQueue.Count == 0)
				{
					this.AddLast();
				}
				return this.bufferQueue.Last();
			}
		}

		public void RecvFrom(byte[] buffer)
		{
			if (this.Count < buffer.Length)
			{
				throw new Exception($"bufferList size < n, bufferList: {this.Count} buffer length: {buffer.Length}");
			}
			int alreadyCopyCount = 0;
			while (alreadyCopyCount < buffer.Length)
			{
				int n = buffer.Length - alreadyCopyCount;
				if (ChunkSize - this.FirstIndex > n)
				{
					Array.Copy(this.bufferQueue.First(), this.FirstIndex, buffer, alreadyCopyCount, n);
					this.FirstIndex += n;
					alreadyCopyCount += n;
				}
				else
				{
					Array.Copy(this.bufferQueue.First(), this.FirstIndex, buffer, alreadyCopyCount, ChunkSize - this.FirstIndex);
					alreadyCopyCount += ChunkSize - this.FirstIndex;
					this.FirstIndex = 0;
					this.bufferQueue.Dequeue();
				}
			}
		}

		public void SendTo(byte[] buffer)
		{
			int alreadyCopyCount = 0;
			while (alreadyCopyCount < buffer.Length)
			{
				if (this.LastIndex == ChunkSize)
				{
					this.AddLast();
					this.LastIndex = 0;
				}

				int n = buffer.Length - alreadyCopyCount;
				if (ChunkSize - this.LastIndex > n)
				{
					Array.Copy(buffer, alreadyCopyCount, this.bufferQueue.Last(), this.LastIndex, n);
					this.LastIndex += buffer.Length - alreadyCopyCount;
					alreadyCopyCount += n;
				}
				else
				{
					Array.Copy(buffer, alreadyCopyCount, this.bufferQueue.Last(), this.LastIndex, ChunkSize - this.LastIndex);
					alreadyCopyCount += ChunkSize - this.LastIndex;
					this.LastIndex = ChunkSize;
				}
			}
		}
		
		public void SendTo(byte[] buffer, int offset, int count)
		{
			int alreadyCopyCount = 0;
			while (alreadyCopyCount < count)
			{
				if (this.LastIndex == ChunkSize)
				{
					this.AddLast();
					this.LastIndex = 0;
				}

				int n = count - alreadyCopyCount;
				if (ChunkSize - this.LastIndex > n)
				{
					Array.Copy(buffer, alreadyCopyCount + offset, this.bufferQueue.Last(), this.LastIndex, n);
					this.LastIndex += count - alreadyCopyCount;
					alreadyCopyCount += n;
				}
				else
				{
					Array.Copy(buffer, alreadyCopyCount + offset, this.bufferQueue.Last(), this.LastIndex, ChunkSize - this.LastIndex);
					alreadyCopyCount += ChunkSize - this.LastIndex;
					this.LastIndex = ChunkSize;
				}
			}
		}
	}
}
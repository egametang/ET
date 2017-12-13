using System;
using System.Collections.Generic;
using System.Linq;

namespace Model
{
    public class Circularbuffer
    {
        public int ChunkSize = 8192;

        private readonly Queue<byte[]> bufferQueue = new Queue<byte[]>();

        private readonly Queue<byte[]> bufferCache = new Queue<byte[]>();

        public int LastIndex { get; set; }

        public int FirstIndex { get; set; }

        private byte[] firstBuffer;

        private byte[] lastBuffer;

        public Circularbuffer()
        {
            this.AddLast();
            this.firstBuffer = bufferQueue.Peek();
        }

        public Circularbuffer(int chunkSize)
        {
            this.ChunkSize = chunkSize;
            this.AddLast();
            this.firstBuffer = bufferQueue.Peek();
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
            this.lastBuffer = buffer;
        }

        public void RemoveFirst()
        {
            this.bufferCache.Enqueue(bufferQueue.Dequeue());
            if (this.bufferQueue.Count != 0)
                this.firstBuffer = bufferQueue.Peek();
        }

        public byte[] First
        {
            get
            {
                if (this.bufferQueue.Count == 0)
                {
                    this.AddLast();
                }
                this.firstBuffer = this.bufferQueue.Peek();
                return this.firstBuffer;
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
                return this.lastBuffer;
            }
        }

        public void RecvFrom(byte[] buffer, int count)
        {
            if (this.Count < count)
            {
                throw new Exception($"bufferList size < n, bufferList: {this.Count} buffer length: {buffer.Length} {count}");
            }
            int alreadyCopyCount = 0;
            while (alreadyCopyCount < count)
            {
                int n = count - alreadyCopyCount;
                if (ChunkSize - this.FirstIndex > n)
                {
                    Array.Copy(this.firstBuffer, this.FirstIndex, buffer, alreadyCopyCount, n);
                    this.FirstIndex += n;
                    alreadyCopyCount += n;
                }
                else
                {
                    Array.Copy(this.firstBuffer, this.FirstIndex, buffer, alreadyCopyCount, ChunkSize - this.FirstIndex);
                    alreadyCopyCount += ChunkSize - this.FirstIndex;
                    this.FirstIndex = 0;
                    this.RemoveFirst();
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
                    Array.Copy(buffer, alreadyCopyCount, this.lastBuffer, this.LastIndex, n);
                    this.LastIndex += buffer.Length - alreadyCopyCount;
                    alreadyCopyCount += n;
                }
                else
                {
                    Array.Copy(buffer, alreadyCopyCount, this.lastBuffer, this.LastIndex, ChunkSize - this.LastIndex);
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
                    Array.Copy(buffer, alreadyCopyCount + offset, this.lastBuffer, this.LastIndex, n);
                    this.LastIndex += count - alreadyCopyCount;
                    alreadyCopyCount += n;
                }
                else
                {
                    Array.Copy(buffer, alreadyCopyCount + offset, this.lastBuffer, this.LastIndex, ChunkSize - this.LastIndex);
                    alreadyCopyCount += ChunkSize - this.LastIndex;
                    this.LastIndex = ChunkSize;
                }
            }
        }
    }
}
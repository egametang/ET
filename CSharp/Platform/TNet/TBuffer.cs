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
                if (buffer.Count == 0)
                {
                    return 0;
                }
                return (buffer.Count - 1) * chunkSize + writeIndex - readIndex;
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
                if (chunkSize - readIndex > n - alreadyCopyCount)
                {
                    Array.Copy(buffer.First.Value, readIndex, bytes, alreadyCopyCount, n - alreadyCopyCount);
                    readIndex += n - alreadyCopyCount;
                    alreadyCopyCount = n;
                }
                else
                {

                    Array.Copy(buffer.First.Value, readIndex, bytes, alreadyCopyCount, chunkSize - readIndex);
                    alreadyCopyCount += chunkSize - readIndex;
                    readIndex = 0;
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
                if (writeIndex == 0)
                {
                    this.buffer.AddLast(new byte[chunkSize]);
                }
                if (chunkSize - writeIndex > alreadyCopyCount)
                {
                    Array.Copy(bytes, alreadyCopyCount, buffer.Last.Value, writeIndex, alreadyCopyCount);
                    writeIndex += alreadyCopyCount;
                    alreadyCopyCount = 0;
                }
                else
                {
                    Array.Copy(bytes, alreadyCopyCount, buffer.Last.Value, writeIndex, chunkSize - writeIndex);
                    alreadyCopyCount -= chunkSize - writeIndex;
                    writeIndex = 0;
                }
            }
        }
    }
}

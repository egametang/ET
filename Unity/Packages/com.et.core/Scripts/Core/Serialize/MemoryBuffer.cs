using System;
using System.Buffers;
using System.IO;

namespace ET
{
    public class MemoryBuffer: MemoryStream, IBufferWriter<byte>
    {
        private int origin;
        
        public MemoryBuffer()
        {
        }
        
        public MemoryBuffer(int capacity): base(capacity)
        {
        }
        
        public MemoryBuffer(byte[] buffer): base(buffer)
        {
        } 
        
        public MemoryBuffer(byte[] buffer, int index, int length): base(buffer, index, length)
        {
            this.origin = index;
        }
        
        public ReadOnlyMemory<byte> WrittenMemory => this.GetBuffer().AsMemory(this.origin, (int)this.Position);

        public ReadOnlySpan<byte> WrittenSpan => this.GetBuffer().AsSpan(this.origin, (int)this.Position);

        public void Advance(int count)
        {
            long newLength = this.Position + count;
            if (newLength > this.Length)
            {
                this.SetLength(newLength);
            }
            this.Position = newLength;
        }

        public Memory<byte> GetMemory(int sizeHint = 0)
        {
            if (this.Length - this.Position < sizeHint)
            {
                this.SetLength(this.Position + sizeHint);
            }
            var memory = this.GetBuffer().AsMemory((int)this.Position + this.origin, (int)(this.Length - this.Position));
            return memory;
        }

        public Span<byte> GetSpan(int sizeHint = 0)
        {
            if (this.Length - this.Position < sizeHint)
            {
                this.SetLength(this.Position + sizeHint);
            }
            var span = this.GetBuffer().AsSpan((int)this.Position + this.origin, (int)(this.Length - this.Position));
            return span;
        }
    }
}
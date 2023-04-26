using System;
using System.Buffers;
using System.IO;
using MemoryPack;

namespace ET
{
    public class MemoryBuffer: MemoryStream, IBufferWriter<byte>
    {
        private int _origin;
        
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
            _origin = index;
        }

        protected override void Dispose(bool disposing)
        {
            this.Seek(0, SeekOrigin.Begin);
        }
        
        public ReadOnlyMemory<byte> WrittenMemory => this.GetBuffer().AsMemory(_origin, (int)this.Position);

        public ReadOnlySpan<byte> WrittenSpan => this.GetBuffer().AsSpan(_origin, (int)this.Position);

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
            var memory = this.GetBuffer().AsMemory((int)this.Position + _origin, (int)(this.Length - this.Position));
            return memory;
        }

        public Span<byte> GetSpan(int sizeHint = 0)
        {
            if (this.Length - this.Position < sizeHint)
            {
                this.SetLength(this.Position + sizeHint);
            }
            var span = this.GetBuffer().AsSpan((int)this.Position + _origin, (int)(this.Length - this.Position));
            return span;
        }
    }
}
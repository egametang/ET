using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ET
{
    // KCP Segment Definition

    internal struct SegmentStruct:IDisposable
    {
        public Kcp.SegmentHead SegHead;
        public uint resendts; 
        public int  rto;
        public uint fastack;
        public uint xmit;
        
        private byte[] buffer;

        private ArrayPool<byte> arrayPool;

        public bool IsNull => this.buffer == null;

        public int WrittenCount
        {
            get => (int) this.SegHead.len;
            private set => this.SegHead.len = (uint) value;
        }

        public Span<byte> WrittenBuffer => this.buffer.AsSpan(0, (int) this.SegHead.len);

        public Span<byte> FreeBuffer => this.buffer.AsSpan(WrittenCount);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SegmentStruct(int size, ArrayPool<byte> arrayPool)
        {
            this.arrayPool = arrayPool;
            buffer = arrayPool.Rent(size);
            this.SegHead = default;
            this.resendts = default;
            this.rto = default;
            this.fastack = default;
            this.xmit = default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Encode(Span<byte> data, ref int size)
        {
            Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(data),this.SegHead);
            size += Unsafe.SizeOf<Kcp.SegmentHead>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Advance(int count)
        {
            this.WrittenCount += count;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            arrayPool.Return(this.buffer);
        }
    }
}

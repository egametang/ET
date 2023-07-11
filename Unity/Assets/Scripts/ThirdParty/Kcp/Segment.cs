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
            this.SegHead = new Kcp.SegmentHead() { len = 0 };
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
    
    // internal class Segment
    // {
    //     internal uint conv;     // conversation
    //     internal byte cmd;      // command, e.g. Kcp.CMD_ACK etc.
    //     // fragment (sent as 1 byte).
    //     // 0 if unfragmented, otherwise fragment numbers in reverse: N,..,32,1,0
    //     // this way the first received segment tells us how many fragments there are.
    //     internal byte frg;
    //     internal ushort wnd;      // window size that the receive can currently receive
    //     internal uint ts;       // timestamp
    //     internal uint sn;       // sequence number
    //     internal uint una;
    //     internal uint resendts; // resend timestamp
    //     internal int  rto;
    //     internal uint fastack;
    //     internal uint xmit;     // retransmit count
    //     
    //     internal MemoryStream data = new MemoryStream(Kcp.MTU_DEF);
    //
    //     [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //     internal int Encode(byte[] ptr, int offset)
    //     {
    //         int previousPosition = offset;
    //         
    //         var segHead = new Kcp.SegmentHead()
    //         {
    //             conv = this.conv,
    //             cmd = (byte) this.cmd,
    //             frg = (byte) frg,
    //             wnd = (ushort) this.wnd,
    //             ts = this.ts,
    //             sn = this.sn,
    //             una = this.una,
    //             len = (uint) this.data.Position,
    //         };
    //         
    //         Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(ptr.AsSpan(offset)),segHead);
    //         offset+=Unsafe.SizeOf<Kcp.SegmentHead>();
    //         
    //         int written = offset - previousPosition;
    //         return written;
    //     }
    //
    //     // reset to return a fresh segment to the pool
    //     [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //     internal void Reset()
    //     {
    //         conv = 0;
    //         cmd = 0;
    //         frg = 0;
    //         wnd = 0;
    //         ts  = 0;
    //         sn  = 0;
    //         una = 0;
    //         rto = 0;
    //         xmit = 0;
    //         resendts = 0;
    //         fastack  = 0;
    //
    //         // keep buffer for next pool usage, but reset length (= bytes written)
    //         data.SetLength(0);
    //     }
    // }
}

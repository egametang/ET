using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Bright.Serialization
{

    public enum EDeserializeError
    {
        OK,
        NOT_ENOUGH,
        EXCEED_SIZE,
        // UNMARSHAL_ERR,
    }

    public class SerializationException : Exception
    {
        public SerializationException() { }
        public SerializationException(string msg) : base(msg) { }

        public SerializationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    public readonly struct SegmentSaveState
    {
        public SegmentSaveState(int readerIndex, int writerIndex)
        {
            ReaderIndex = readerIndex;
            WriterIndex = writerIndex;
        }

        public int ReaderIndex { get; }

        public int WriterIndex { get; }
    }

    public sealed class ByteBuf : ICloneable, IEquatable<ByteBuf>
    {
        public ByteBuf()
        {
            Bytes = Array.Empty<byte>();
            ReaderIndex = WriterIndex = 0;
        }

        public ByteBuf(int capacity)
        {
            Bytes = capacity > 0 ? new byte[capacity] : Array.Empty<byte>();
            ReaderIndex = 0;
            WriterIndex = 0;
        }

        public ByteBuf(byte[] bytes)
        {
            Bytes = bytes;
            ReaderIndex = 0;
            WriterIndex = Capacity;
        }

        public ByteBuf(byte[] bytes, int readIndex, int writeIndex)
        {
            Bytes = bytes;
            ReaderIndex = readIndex;
            WriterIndex = writeIndex;
        }

        public ByteBuf(int capacity, Action<ByteBuf> releaser) : this(capacity)
        {
            _releaser = releaser;
        }

        public static ByteBuf Wrap(byte[] bytes)
        {
            return new ByteBuf(bytes, 0, bytes.Length);
        }

        public void Replace(byte[] bytes)
        {
            Bytes = bytes;
            ReaderIndex = 0;
            WriterIndex = Capacity;
        }

        public void Replace(byte[] bytes, int beginPos, int endPos)
        {
            Bytes = bytes;
            ReaderIndex = beginPos;
            WriterIndex = endPos;
        }

        public int ReaderIndex { get; set; }

        public int WriterIndex { get; set; }

        private readonly Action<ByteBuf> _releaser;

        public int Capacity => Bytes.Length;

        public int Size { get { return WriterIndex - ReaderIndex; } }

        public bool Empty => WriterIndex <= ReaderIndex;

        public bool NotEmpty => WriterIndex > ReaderIndex;


        public void AddWriteIndex(int add)
        {
            WriterIndex += add;
        }

        public void AddReadIndex(int add)
        {
            ReaderIndex += add;
        }

#pragma warning disable CA1819 // 属性不应返回数组
        public byte[] Bytes { get; private set; }
#pragma warning restore CA1819 // 属性不应返回数组

        public byte[] CopyData()
        {
            var n = Remaining;
            if (n > 0)
            {
                var arr = new byte[n];
                Buffer.BlockCopy(Bytes, ReaderIndex, arr, 0, n);
                return arr;
            }
            else
            {
                return Array.Empty<byte>();
            }
        }

        public int Remaining { get { return WriterIndex - ReaderIndex; } }

        public void DiscardReadBytes()
        {
            WriterIndex -= ReaderIndex;
            Array.Copy(Bytes, ReaderIndex, Bytes, 0, WriterIndex);
            ReaderIndex = 0;
        }

        public int NotCompactWritable { get { return Capacity - WriterIndex; } }

        public void WriteBytesWithoutSize(byte[] bs)
        {
            WriteBytesWithoutSize(bs, 0, bs.Length);
        }

        public void WriteBytesWithoutSize(byte[] bs, int offset, int len)
        {
            EnsureWrite(len);
            Buffer.BlockCopy(bs, offset, Bytes, WriterIndex, len);
            WriterIndex += len;
        }

        public void Clear()
        {
            ReaderIndex = WriterIndex = 0;
        }

        private const int MIN_CAPACITY = 16;

        private static int PropSize(int initSize, int needSize)
        {
            for (int i = Math.Max(initSize, MIN_CAPACITY); ; i <<= 1)
            {
                if (i >= needSize)
                {
                    return i;
                }
            }
        }

        private void EnsureWrite0(int size)
        {
            var needSize = WriterIndex + size - ReaderIndex;
            if (needSize < Capacity)
            {
                WriterIndex -= ReaderIndex;
                Array.Copy(Bytes, ReaderIndex, Bytes, 0, WriterIndex);
                ReaderIndex = 0;
            }
            else
            {
                int newCapacity = PropSize(Capacity, needSize);
                var newBytes = new byte[newCapacity];
                WriterIndex -= ReaderIndex;
                Buffer.BlockCopy(Bytes, ReaderIndex, newBytes, 0, WriterIndex);
                ReaderIndex = 0;
                Bytes = newBytes;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnsureWrite(int size)
        {
            if (WriterIndex + size > Capacity)
            {
                EnsureWrite0(size);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureRead(int size)
        {
            if (ReaderIndex + size > WriterIndex)
            {
                throw new SerializationException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool CanRead(int size)
        {
            return (ReaderIndex + size <= WriterIndex);
        }

        public void Append(byte x)
        {
            EnsureWrite(1);
            Bytes[WriterIndex++] = x;
        }

        public void WriteBool(bool b)
        {
            EnsureWrite(1);
            Bytes[WriterIndex++] = (byte)(b ? 1 : 0);
        }

        public bool ReadBool()
        {
            EnsureRead(1);
            return Bytes[ReaderIndex++] != 0;
        }

        public void WriteByte(byte x)
        {
            EnsureWrite(1);
            Bytes[WriterIndex++] = x;
        }

        public byte ReadByte()
        {
            EnsureRead(1);
            return Bytes[ReaderIndex++];
        }


        public void WriteShort(short x)
        {
            if (x >= 0)
            {
                if (x < 0x80)
                {
                    EnsureWrite(1);
                    Bytes[WriterIndex++] = (byte)x;
                    return;
                }
                else if (x < 0x4000)
                {
                    EnsureWrite(2);
                    Bytes[WriterIndex + 1] = (byte)x;
                    Bytes[WriterIndex] = (byte)((x >> 8) | 0x80);
                    WriterIndex += 2;
                    return;
                }
            }
            EnsureWrite(3);
            Bytes[WriterIndex] = 0xff;
            Bytes[WriterIndex + 2] = (byte)x;
            Bytes[WriterIndex + 1] = (byte)(x >> 8);
            WriterIndex += 3;
        }

        public short ReadShort()
        {
            EnsureRead(1);
            int h = Bytes[ReaderIndex];
            if (h < 0x80)
            {
                ReaderIndex++;
                return (short)h;
            }
            else if (h < 0xc0)
            {
                EnsureRead(2);
                int x = ((h & 0x3f) << 8) | Bytes[ReaderIndex + 1];
                ReaderIndex += 2;
                return (short)x;
            }
            else if ((h == 0xff))
            {
                EnsureRead(3);
                int x = (Bytes[ReaderIndex + 1] << 8) | Bytes[ReaderIndex + 2];
                ReaderIndex += 3;
                return (short)x;
            }
            else
            {
                throw new SerializationException();
            }
        }

        public short ReadFshort()
        {
            EnsureRead(2);
            short x;
#if CPU_SUPPORT_MEMORY_NOT_ALIGN
            unsafe
            {
                fixed (byte* b = &Bytes[ReaderIndex])
                {
                    x = *(short*)b;
                }
            }
#else
            x = (short)((Bytes[ReaderIndex + 1] << 8) | Bytes[ReaderIndex]);

#endif
            ReaderIndex += 2;
            return x;
        }

        public void WriteFshort(short x)
        {
            EnsureWrite(2);
#if CPU_SUPPORT_MEMORY_NOT_ALIGN
            unsafe
            {
                fixed (byte* b = &Bytes[WriterIndex])
                {
                    *(short*)b = x;
                }
            }
#else
            Bytes[WriterIndex] = (byte)x;
            Bytes[WriterIndex + 1] = (byte)(x >> 8);
#endif
            WriterIndex += 2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteInt(int x)
        {
            WriteUint((uint)x);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadInt()
        {
            return (int)ReadUint();
        }


        public void WriteUint(uint x)
        {
            // 如果有修改，记得也把 EndWriteSegment改了
            // 0 111 1111
            if (x < 0x80)
            {
                EnsureWrite(1);
                Bytes[WriterIndex++] = (byte)x;
            }
            else if (x < 0x4000) // 10 11 1111, -
            {
                EnsureWrite(2);
                Bytes[WriterIndex + 1] = (byte)x;
                Bytes[WriterIndex] = (byte)((x >> 8) | 0x80);
                WriterIndex += 2;
            }
            else if (x < 0x200000) // 110 1 1111, -,-
            {
                EnsureWrite(3);
                Bytes[WriterIndex + 2] = (byte)x;
                Bytes[WriterIndex + 1] = (byte)(x >> 8);
                Bytes[WriterIndex] = (byte)((x >> 16) | 0xc0);
                WriterIndex += 3;
            }
            else if (x < 0x10000000) // 1110 1111,-,-,-
            {
                EnsureWrite(4);
                Bytes[WriterIndex + 3] = (byte)x;
                Bytes[WriterIndex + 2] = (byte)(x >> 8);
                Bytes[WriterIndex + 1] = (byte)(x >> 16);
                Bytes[WriterIndex] = (byte)((x >> 24) | 0xe0);
                WriterIndex += 4;
            }
            else
            {
                EnsureWrite(5);
                Bytes[WriterIndex] = 0xf0;
                Bytes[WriterIndex + 4] = (byte)x;
                Bytes[WriterIndex + 3] = (byte)(x >> 8);
                Bytes[WriterIndex + 2] = (byte)(x >> 16);
                Bytes[WriterIndex + 1] = (byte)(x >> 24);
                WriterIndex += 5;
            }
        }

        public uint ReadUint()
        {
            ///
            /// 警告！ 如有修改，记得调整 TryDeserializeInplaceOctets
            EnsureRead(1);
            uint h = Bytes[ReaderIndex];
            if (h < 0x80)
            {
                ReaderIndex++;
                return h;
            }
            else if (h < 0xc0)
            {
                EnsureRead(2);
                uint x = ((h & 0x3f) << 8) | Bytes[ReaderIndex + 1];
                ReaderIndex += 2;
                return x;
            }
            else if (h < 0xe0)
            {
                EnsureRead(3);
                uint x = ((h & 0x1f) << 16) | ((uint)Bytes[ReaderIndex + 1] << 8) | Bytes[ReaderIndex + 2];
                ReaderIndex += 3;
                return x;
            }
            else if (h < 0xf0)
            {

                EnsureRead(4);
                uint x = ((h & 0x0f) << 24) | ((uint)Bytes[ReaderIndex + 1] << 16) | ((uint)Bytes[ReaderIndex + 2] << 8) | Bytes[ReaderIndex + 3];
                ReaderIndex += 4;
                return x;
            }
            else
            {
                EnsureRead(5);
                uint x = ((uint)Bytes[ReaderIndex + 1] << 24) | ((uint)(Bytes[ReaderIndex + 2] << 16)) | ((uint)Bytes[ReaderIndex + 3] << 8) | Bytes[ReaderIndex + 4];
                ReaderIndex += 5;
                return x;
            }
        }

        public unsafe void WriteUint_Unsafe(uint x)
        {
            // 0 111 1111
            if (x < 0x80)
            {
                EnsureWrite(1);
                Bytes[WriterIndex++] = (byte)(x << 1);
            }
            else if (x < 0x4000)// 10 11 1111, -
            {
                EnsureWrite(2);

                fixed (byte* wb = &Bytes[WriterIndex])
                {
                    *(uint*)(wb) = (x << 2 | 0b01);
                }

                WriterIndex += 2;
            }
            else if (x < 0x200000) // 110 1 1111, -,-
            {
                EnsureWrite(3);

                fixed (byte* wb = &Bytes[WriterIndex])
                {
                    *(uint*)(wb) = (x << 3 | 0b011);
                }
                WriterIndex += 3;
            }
            else if (x < 0x10000000) // 1110 1111,-,-,-
            {
                EnsureWrite(4);
                fixed (byte* wb = &Bytes[WriterIndex])
                {
                    *(uint*)(wb) = (x << 4 | 0b0111);
                }
                WriterIndex += 4;
            }
            else
            {
                EnsureWrite(5);
                fixed (byte* wb = &Bytes[WriterIndex])
                {
                    *(uint*)(wb) = (x << 5 | 0b01111);
                }
                WriterIndex += 5;
            }
        }

        public unsafe uint ReadUint_Unsafe()
        {
            ///
            /// 警告！ 如有修改，记得调整 TryDeserializeInplaceOctets
            EnsureRead(1);
            uint h = Bytes[ReaderIndex];
            if ((h & 0b1) == 0b0)
            {
                ReaderIndex++;
                return (h >> 1);
            }
            else if ((h & 0b11) == 0b01)
            {
                EnsureRead(2);
                fixed (byte* rb = &Bytes[ReaderIndex])
                {
                    ReaderIndex += 2;
                    return (*(uint*)rb) >> 2;
                }
            }
            else if ((h & 0b111) == 0b011)
            {
                EnsureRead(3);
                fixed (byte* rb = &Bytes[ReaderIndex])
                {
                    ReaderIndex += 3;
                    return (*(uint*)rb) >> 3;
                }
            }
            else if ((h & 0b1111) == 0b0111)
            {
                EnsureRead(4);
                fixed (byte* rb = &Bytes[ReaderIndex])
                {
                    ReaderIndex += 4;
                    return (*(uint*)rb) >> 4;
                }
            }
            else
            {
                EnsureRead(5);
                fixed (byte* rb = &Bytes[ReaderIndex])
                {
                    ReaderIndex += 5;
                    return (*(uint*)rb) >> 5;
                }
            }
        }

        public int ReadFint()
        {
            EnsureRead(4);
            int x;
#if CPU_SUPPORT_MEMORY_NOT_ALIGN
            unsafe
            {
                fixed (byte* b = &Bytes[ReaderIndex])
                {
                    x = *(int*)b;
                }
            }
#else
            x = (Bytes[ReaderIndex + 3] << 24) | (Bytes[ReaderIndex + 2] << 16) | (Bytes[ReaderIndex + 1] << 8) | (Bytes[ReaderIndex]);

#endif
            ReaderIndex += 4;
            return x;
        }


        public void WriteFint(int x)
        {
            EnsureWrite(4);
#if CPU_SUPPORT_MEMORY_NOT_ALIGN
            unsafe
            {
                fixed (byte* b = &Bytes[WriterIndex])
                {
                    *(int*)b = x;
                }
            }
#else
            Bytes[WriterIndex] = (byte)x;
            Bytes[WriterIndex + 1] = (byte)(x >> 8);
            Bytes[WriterIndex + 2] = (byte)(x >> 16);
            Bytes[WriterIndex + 3] = (byte)(x >> 24);
#endif
            WriterIndex += 4;
        }

        public int ReadFint_Safe()
        {
            EnsureRead(4);
            int x;

            x = (Bytes[ReaderIndex + 3] << 24) | (Bytes[ReaderIndex + 2] << 16) | (Bytes[ReaderIndex + 1] << 8) | (Bytes[ReaderIndex]);

            ReaderIndex += 4;
            return x;
        }


        public void WriteFint_Safe(int x)
        {
            EnsureWrite(4);
            Bytes[WriterIndex] = (byte)x;
            Bytes[WriterIndex + 1] = (byte)(x >> 8);
            Bytes[WriterIndex + 2] = (byte)(x >> 16);
            Bytes[WriterIndex + 3] = (byte)(x >> 24);
            WriterIndex += 4;
        }

        public void WriteLong(long x)
        {
            WriteUlong((ulong)x);
        }

        public long ReadLong()
        {
            return (long)ReadUlong();
        }

        public void WriteNumberAsLong(double x)
        {
            WriteLong((long)x);
        }

        public double ReadLongAsNumber()
        {
            return ReadLong();
        }

        private void WriteUlong(ulong x)
        {
            // 0 111 1111
            if (x < 0x80)
            {
                EnsureWrite(1);
                Bytes[WriterIndex++] = (byte)x;
            }
            else if (x < 0x4000) // 10 11 1111, -
            {
                EnsureWrite(2);
                Bytes[WriterIndex + 1] = (byte)x;
                Bytes[WriterIndex] = (byte)((x >> 8) | 0x80);
                WriterIndex += 2;
            }
            else if (x < 0x200000) // 110 1 1111, -,-
            {
                EnsureWrite(3);
                Bytes[WriterIndex + 2] = (byte)x;
                Bytes[WriterIndex + 1] = (byte)(x >> 8);
                Bytes[WriterIndex] = (byte)((x >> 16) | 0xc0);
                WriterIndex += 3;
            }
            else if (x < 0x10000000) // 1110 1111,-,-,-
            {
                EnsureWrite(4);
                Bytes[WriterIndex + 3] = (byte)x;
                Bytes[WriterIndex + 2] = (byte)(x >> 8);
                Bytes[WriterIndex + 1] = (byte)(x >> 16);
                Bytes[WriterIndex] = (byte)((x >> 24) | 0xe0);
                WriterIndex += 4;
            }
            else if (x < 0x800000000L) // 1111 0xxx,-,-,-,-
            {
                EnsureWrite(5);
                Bytes[WriterIndex + 4] = (byte)x;
                Bytes[WriterIndex + 3] = (byte)(x >> 8);
                Bytes[WriterIndex + 2] = (byte)(x >> 16);
                Bytes[WriterIndex + 1] = (byte)(x >> 24);
                Bytes[WriterIndex] = (byte)((x >> 32) | 0xf0);
                WriterIndex += 5;
            }
            else if (x < 0x40000000000L) // 1111 10xx, 
            {
                EnsureWrite(6);
                Bytes[WriterIndex + 5] = (byte)x;
                Bytes[WriterIndex + 4] = (byte)(x >> 8);
                Bytes[WriterIndex + 3] = (byte)(x >> 16);
                Bytes[WriterIndex + 2] = (byte)(x >> 24);
                Bytes[WriterIndex + 1] = (byte)(x >> 32);
                Bytes[WriterIndex] = (byte)((x >> 40) | 0xf8);
                WriterIndex += 6;
            }
            else if (x < 0x200000000000L) // 1111 110x,
            {
                EnsureWrite(7);
                Bytes[WriterIndex + 6] = (byte)x;
                Bytes[WriterIndex + 5] = (byte)(x >> 8);
                Bytes[WriterIndex + 4] = (byte)(x >> 16);
                Bytes[WriterIndex + 3] = (byte)(x >> 24);
                Bytes[WriterIndex + 2] = (byte)(x >> 32);
                Bytes[WriterIndex + 1] = (byte)(x >> 40);
                Bytes[WriterIndex] = (byte)((x >> 48) | 0xfc);
                WriterIndex += 7;
            }
            else if (x < 0x100000000000000L) // 1111 1110
            {
                EnsureWrite(8);
                Bytes[WriterIndex + 7] = (byte)x;
                Bytes[WriterIndex + 6] = (byte)(x >> 8);
                Bytes[WriterIndex + 5] = (byte)(x >> 16);
                Bytes[WriterIndex + 4] = (byte)(x >> 24);
                Bytes[WriterIndex + 3] = (byte)(x >> 32);
                Bytes[WriterIndex + 2] = (byte)(x >> 40);
                Bytes[WriterIndex + 1] = (byte)(x >> 48);
                Bytes[WriterIndex] = 0xfe;
                WriterIndex += 8;
            }
            else // 1111 1111
            {
                EnsureWrite(9);
                Bytes[WriterIndex] = 0xff;
                Bytes[WriterIndex + 8] = (byte)x;
                Bytes[WriterIndex + 7] = (byte)(x >> 8);
                Bytes[WriterIndex + 6] = (byte)(x >> 16);
                Bytes[WriterIndex + 5] = (byte)(x >> 24);
                Bytes[WriterIndex + 4] = (byte)(x >> 32);
                Bytes[WriterIndex + 3] = (byte)(x >> 40);
                Bytes[WriterIndex + 2] = (byte)(x >> 48);
                Bytes[WriterIndex + 1] = (byte)(x >> 56);
                WriterIndex += 9;
            }
        }

        public ulong ReadUlong()
        {
            EnsureRead(1);
            uint h = Bytes[ReaderIndex];
            if (h < 0x80)
            {
                ReaderIndex++;
                return h;
            }
            else if (h < 0xc0)
            {
                EnsureRead(2);
                uint x = ((h & 0x3f) << 8) | Bytes[ReaderIndex + 1];
                ReaderIndex += 2;
                return x;
            }
            else if (h < 0xe0)
            {
                EnsureRead(3);
                uint x = ((h & 0x1f) << 16) | ((uint)Bytes[ReaderIndex + 1] << 8) | Bytes[ReaderIndex + 2];
                ReaderIndex += 3;
                return x;
            }
            else if (h < 0xf0)
            {
                EnsureRead(4);
                uint x = ((h & 0x0f) << 24) | ((uint)Bytes[ReaderIndex + 1] << 16) | ((uint)Bytes[ReaderIndex + 2] << 8) | Bytes[ReaderIndex + 3];
                ReaderIndex += 4;
                return x;
            }
            else if (h < 0xf8)
            {
                EnsureRead(5);
                uint xl = ((uint)Bytes[ReaderIndex + 1] << 24) | ((uint)(Bytes[ReaderIndex + 2] << 16)) | ((uint)Bytes[ReaderIndex + 3] << 8) | (Bytes[ReaderIndex + 4]);
                uint xh = h & 0x07;
                ReaderIndex += 5;
                return ((ulong)xh << 32) | xl;
            }
            else if (h < 0xfc)
            {
                EnsureRead(6);
                uint xl = ((uint)Bytes[ReaderIndex + 2] << 24) | ((uint)(Bytes[ReaderIndex + 3] << 16)) | ((uint)Bytes[ReaderIndex + 4] << 8) | (Bytes[ReaderIndex + 5]);
                uint xh = ((h & 0x03) << 8) | Bytes[ReaderIndex + 1];
                ReaderIndex += 6;
                return ((ulong)xh << 32) | xl;
            }
            else if (h < 0xfe)
            {
                EnsureRead(7);
                uint xl = ((uint)Bytes[ReaderIndex + 3] << 24) | ((uint)(Bytes[ReaderIndex + 4] << 16)) | ((uint)Bytes[ReaderIndex + 5] << 8) | (Bytes[ReaderIndex + 6]);
                uint xh = ((h & 0x01) << 16) | ((uint)Bytes[ReaderIndex + 1] << 8) | Bytes[ReaderIndex + 2];
                ReaderIndex += 7;
                return ((ulong)xh << 32) | xl;
            }
            else if (h < 0xff)
            {
                EnsureRead(8);
                uint xl = ((uint)Bytes[ReaderIndex + 4] << 24) | ((uint)(Bytes[ReaderIndex + 5] << 16)) | ((uint)Bytes[ReaderIndex + 6] << 8) | (Bytes[ReaderIndex + 7]);
                uint xh = /*((h & 0x01) << 24) |*/ ((uint)Bytes[ReaderIndex + 1] << 16) | ((uint)Bytes[ReaderIndex + 2] << 8) | Bytes[ReaderIndex + 3];
                ReaderIndex += 8;
                return ((ulong)xh << 32) | xl;
            }
            else
            {
                EnsureRead(9);
                uint xl = ((uint)Bytes[ReaderIndex + 5] << 24) | ((uint)(Bytes[ReaderIndex + 6] << 16)) | ((uint)Bytes[ReaderIndex + 7] << 8) | (Bytes[ReaderIndex + 8]);
                uint xh = ((uint)Bytes[ReaderIndex + 1] << 24) | ((uint)Bytes[ReaderIndex + 2] << 16) | ((uint)Bytes[ReaderIndex + 3] << 8) | Bytes[ReaderIndex + 4];
                ReaderIndex += 9;
                return ((ulong)xh << 32) | xl;
            }
        }


        public void WriteFlong(long x)
        {
            EnsureWrite(8);
#if CPU_SUPPORT_MEMORY_NOT_ALIGN
            unsafe
            {
                fixed (byte* b = &Bytes[WriterIndex])
                {
                    *(long*)b = x;
                }
            }
#else

            Bytes[WriterIndex] = (byte)x;
            Bytes[WriterIndex + 1] = (byte)(x >> 8);
            Bytes[WriterIndex + 2] = (byte)(x >> 16);
            Bytes[WriterIndex + 3] = (byte)(x >> 24);
            Bytes[WriterIndex + 4] = (byte)(x >> 32);
            Bytes[WriterIndex + 5] = (byte)(x >> 40);
            Bytes[WriterIndex + 6] = (byte)(x >> 48);
            Bytes[WriterIndex + 7] = (byte)(x >> 56);
#endif
            WriterIndex += 8;
        }

        public long ReadFlong()
        {
            EnsureRead(8);
            long x;
#if CPU_SUPPORT_MEMORY_NOT_ALIGN
            unsafe
            {
                fixed (byte* b = &Bytes[ReaderIndex])
                {
                    x = *(long*)b;
                }
            }
#else
            int xl = (Bytes[ReaderIndex + 3] << 24) | ((Bytes[ReaderIndex + 2] << 16)) | (Bytes[ReaderIndex + 1] << 8) | (Bytes[ReaderIndex]);
            int xh = (Bytes[ReaderIndex + 7] << 24) | (Bytes[ReaderIndex + 6] << 16) | (Bytes[ReaderIndex + 5] << 8) | Bytes[ReaderIndex + 4];
            x = ((long)xh << 32) | (long)xl;
#endif
            ReaderIndex += 8;
            return x;
        }

        private static unsafe void Copy8(byte* dst, byte* src)
        {
            dst[0] = src[0];
            dst[1] = src[1];
            dst[2] = src[2];
            dst[3] = src[3];
            dst[4] = src[4];
            dst[5] = src[5];
            dst[6] = src[6];
            dst[7] = src[7];
        }

        private static unsafe void Copy4(byte* dst, byte* src)
        {
            dst[0] = src[0];
            dst[1] = src[1];
            dst[2] = src[2];
            dst[3] = src[3];
        }


        //const bool isLittleEndian = true;
        public void WriteFloat(float x)
        {
            EnsureWrite(4);
            unsafe
            {
                fixed (byte* b = &Bytes[WriterIndex])
                {
#if !CPU_SUPPORT_MEMORY_NOT_ALIGN
                    if ((long)b % 4 == 0)
                    {
                        *(float*)b = x;
                    }
                    else
                    {
                        Copy4(b, (byte*)&x);
                    }
#else
                    *(float*)b = x;
#endif
                }
            }

            //if (!BitConverter.IsLittleEndian)
            //{
            //    Array.Reverse(data, endPos, 4);
            //}
            WriterIndex += 4;
        }

        public float ReadFloat()
        {
            EnsureRead(4);
            //if (!BitConverter.IsLittleEndian)
            //{
            //    Array.Reverse(data, beginPos, 4);
            //}
            float x;
            unsafe
            {
                fixed (byte* b = &Bytes[ReaderIndex])
                {
#if !CPU_SUPPORT_MEMORY_NOT_ALIGN
                    if ((long)b % 4 == 0)
                    {
                        x = *(float*)b;
                    }
                    else
                    {
                        *((int*)&x) = (b[0]) | (b[1] << 8) | (b[2] << 16) | (b[3] << 24);
                    }
#else
                    x = *(float*)b;
#endif
                }
            }

            ReaderIndex += 4;
            return x;
        }

        public void WriteDouble(double x)
        {
            EnsureWrite(8);
            unsafe
            {
                fixed (byte* b = &Bytes[WriterIndex])
                {
#if !CPU_SUPPORT_MEMORY_NOT_ALIGN
                    if ((long)b % 8 == 0)
                    {
                        *(double*)b = x;
                    }
                    else
                    {
                        Copy8(b, (byte*)&x);
                    }
#else
                    *(double*)b = x;
#endif
                }
                //if (!BitConverter.IsLittleEndian)
                //{
                //    Array.Reverse(data, endPos, 8);
                //}
            }

            WriterIndex += 8;
        }

        public double ReadDouble()
        {
            EnsureRead(8);
            //if (!BitConverter.IsLittleEndian)
            //{
            //    Array.Reverse(data, beginPos, 8);
            //}
            double x;
            unsafe
            {
                fixed (byte* b = &Bytes[ReaderIndex])
                {
#if !CPU_SUPPORT_MEMORY_NOT_ALIGN
                    if ((long)b % 8 == 0)
                    {
                        x = *(double*)b;
                    }
                    else
                    {
                        int low = (b[0]) | (b[1] << 8) | (b[2] << 16) | (b[3] << 24);
                        int high = (b[4]) | (b[5] << 8) | (b[6] << 16) | (b[7] << 24);
                        *((long*)&x) = ((long)high << 32) | (uint)low;
                    }
#else
                    x = *(double*)b;
#endif
                }
            }

            ReaderIndex += 8;
            return x;
        }

        public void WriteSize(int n)
        {
            WriteUint((uint)n);
        }

        public int ReadSize()
        {
            return (int)ReadUint();
        }

        // marshal int 
        // n -> (n << 1) ^ (n >> 31)
        // Read
        // (x >>> 1) ^ ((x << 31) >> 31)
        // (x >>> 1) ^ -(n&1)
        public void WriteSint(int x)
        {
            WriteUint(((uint)x << 1) ^ ((uint)x >> 31));
        }

        public int ReadSint()
        {
            uint x = ReadUint();
            return (int)((x >> 1) ^ ((x & 1) << 31));
        }


        // marshal long
        // n -> (n << 1) ^ (n >> 63)
        // Read
        // (x >>> 1) ^((x << 63) >> 63)
        // (x >>> 1) ^ -(n&1L)
        public void WriteSlong(long x)
        {
            WriteUlong(((ulong)x << 1) ^ ((ulong)x >> 63));
        }

        public long ReadSlong()
        {
            long x = ReadLong();
            return ((long)((ulong)x >> 1) ^ ((x & 1) << 63));
        }

        public void WriteString(string x)
        {
            var n = x != null ? Encoding.UTF8.GetByteCount(x) : 0;
            WriteSize(n);
            if (n > 0)
            {
                EnsureWrite(n);
                Encoding.UTF8.GetBytes(x, 0, x.Length, Bytes, WriterIndex);
                WriterIndex += n;
            }
        }

        // byte[], [start, end)
        public static Func<byte[], int, int, string> StringCacheFinder { get; set; }

        public string ReadString()
        {
            var n = ReadSize();
            if (n > 0)
            {
                EnsureRead(n);
                string s;

                if (StringCacheFinder == null)
                {
                    s = Encoding.UTF8.GetString(Bytes, ReaderIndex, n);
                }
                else
                {
                    // 只缓存比较小的字符串
                    s = StringCacheFinder(Bytes, ReaderIndex, n);
                }
                ReaderIndex += n;
                return s;
            }
            else
            {
                return string.Empty;
            }
        }

        public void WriteBytes(byte[] x)
        {
            var n = x != null ? x.Length : 0;
            WriteSize(n);
            if (n > 0)
            {
                EnsureWrite(n);
                x.CopyTo(Bytes, WriterIndex);
                WriterIndex += n;
            }
        }

        public byte[] ReadBytes()
        {
            var n = ReadSize();
            if (n > 0)
            {
                EnsureRead(n);
                var x = new byte[n];
                Buffer.BlockCopy(Bytes, ReaderIndex, x, 0, n);
                ReaderIndex += n;
                return x;
            }
            else
            {
                return Array.Empty<byte>();
            }
        }

        // 以下是一些特殊类型

        public void WriteComplex(Complex x)
        {
            WriteDouble(x.Real);
            WriteDouble(x.Imaginary);
        }

        public Complex ReadComplex()
        {
            var x = ReadDouble();
            var y = ReadDouble();
            return new Complex(x, y);
        }

        public void WriteVector2(Vector2 x)
        {
            WriteFloat(x.X);
            WriteFloat(x.Y);
        }

        public Vector2 ReadVector2()
        {
            float x = ReadFloat();
            float y = ReadFloat();
            return new Vector2(x, y);
        }

        public void WriteVector3(Vector3 x)
        {
            WriteFloat(x.X);
            WriteFloat(x.Y);
            WriteFloat(x.Z);
        }

        public Vector3 ReadVector3()
        {
            float x = ReadFloat();
            float y = ReadFloat();
            float z = ReadFloat();
            return new Vector3(x, y, z);
        }

        public void WriteVector4(Vector4 x)
        {
            WriteFloat(x.X);
            WriteFloat(x.Y);
            WriteFloat(x.Z);
            WriteFloat(x.W);
        }

        public Vector4 ReadVector4()
        {
            float x = ReadFloat();
            float y = ReadFloat();
            float z = ReadFloat();
            float w = ReadFloat();
            return new Vector4(x, y, z, w);
        }


        public void WriteQuaternion(Quaternion x)
        {
            WriteFloat(x.X);
            WriteFloat(x.Y);
            WriteFloat(x.Z);
            WriteFloat(x.W);
        }

        public Quaternion ReadQuaternion()
        {
            float x = ReadFloat();
            float y = ReadFloat();
            float z = ReadFloat();
            float w = ReadFloat();
            return new Quaternion(x, y, z, w);
        }


        public void WriteMatrix4x4(Matrix4x4 x)
        {
            WriteFloat(x.M11);
            WriteFloat(x.M12);
            WriteFloat(x.M13);
            WriteFloat(x.M14);
            WriteFloat(x.M21);
            WriteFloat(x.M22);
            WriteFloat(x.M23);
            WriteFloat(x.M24);
            WriteFloat(x.M31);
            WriteFloat(x.M32);
            WriteFloat(x.M33);
            WriteFloat(x.M34);
            WriteFloat(x.M41);
            WriteFloat(x.M42);
            WriteFloat(x.M43);
            WriteFloat(x.M44);
        }

        public Matrix4x4 ReadMatrix4x4()
        {
            float m11 = ReadFloat();
            float m12 = ReadFloat();
            float m13 = ReadFloat();
            float m14 = ReadFloat();
            float m21 = ReadFloat();
            float m22 = ReadFloat();
            float m23 = ReadFloat();
            float m24 = ReadFloat();
            float m31 = ReadFloat();
            float m32 = ReadFloat();
            float m33 = ReadFloat();
            float m34 = ReadFloat();
            float m41 = ReadFloat();
            float m42 = ReadFloat();
            float m43 = ReadFloat();
            float m44 = ReadFloat();
            return new Matrix4x4(m11, m12, m13, m14,
                m21, m22, m23, m24,
                m31, m32, m33, m34,
                m41, m42, m43, m44);
        }

        internal void SkipBytes()
        {
            int n = ReadSize();
            EnsureRead(n);
            ReaderIndex += n;
        }


        public void WriteByteBufWithSize(ByteBuf o)
        {
            int n = o.Size;
            if (n > 0)
            {
                WriteSize(n);
                WriteBytesWithoutSize(o.Bytes, o.ReaderIndex, n);
            }
            else
            {
                WriteByte(0);
            }
        }

        public void WriteByteBufWithoutSize(ByteBuf o)
        {
            int n = o.Size;
            if (n > 0)
            {
                WriteBytesWithoutSize(o.Bytes, o.ReaderIndex, n);
            }
        }

        public bool TryReadByte(out byte x)
        {
            if (CanRead(1))
            {
                x = Bytes[ReaderIndex++];
                return true;
            }
            else
            {
                x = 0;
                return false;
            }
        }

        public EDeserializeError TryDeserializeInplaceByteBuf(int maxSize, ByteBuf inplaceTempBody)
        {
            //if (!CanRead(1)) { return EDeserializeError.NOT_ENOUGH; }
            int oldReadIndex = ReaderIndex;
            bool commit = false;
            try
            {
                int n;
                int h = Bytes[ReaderIndex];
                if (h < 0x80)
                {
                    ReaderIndex++;
                    n = h;
                }
                else if (h < 0xc0)
                {
                    if (!CanRead(2)) { return EDeserializeError.NOT_ENOUGH; }
                    n = ((h & 0x3f) << 8) | Bytes[ReaderIndex + 1];
                    ReaderIndex += 2;
                }
                else if (h < 0xe0)
                {
                    if (!CanRead(3)) { return EDeserializeError.NOT_ENOUGH; }
                    n = ((h & 0x1f) << 16) | (Bytes[ReaderIndex + 1] << 8) | Bytes[ReaderIndex + 2];
                    ReaderIndex += 3;
                }
                else if (h < 0xf0)
                {
                    if (!CanRead(4)) { return EDeserializeError.NOT_ENOUGH; }
                    n = ((h & 0x0f) << 24) | (Bytes[ReaderIndex + 1] << 16) | (Bytes[ReaderIndex + 2] << 8) | Bytes[ReaderIndex + 3];
                    ReaderIndex += 4;
                }
                else
                {
                    return EDeserializeError.EXCEED_SIZE;
                }

                if (n > maxSize)
                {
                    return EDeserializeError.EXCEED_SIZE;
                }
                if (Remaining < n)
                {
                    return EDeserializeError.NOT_ENOUGH;
                }

                int inplaceReadIndex = ReaderIndex;
                ReaderIndex += n;

                inplaceTempBody.Replace(Bytes, inplaceReadIndex, ReaderIndex);
                commit = true;
            }
            finally
            {
                if (!commit)
                {
                    ReaderIndex = oldReadIndex;
                }
            }

            return EDeserializeError.OK;
        }

        public void WriteRawTag(byte b1)
        {
            EnsureWrite(1);
            Bytes[WriterIndex++] = b1;
        }

        public void WriteRawTag(byte b1, byte b2)
        {
            EnsureWrite(2);
            Bytes[WriterIndex] = b1;
            Bytes[WriterIndex + 1] = b2;
            WriterIndex += 2;
        }

        public void WriteRawTag(byte b1, byte b2, byte b3)
        {
            EnsureWrite(3);
            Bytes[WriterIndex] = b1;
            Bytes[WriterIndex + 1] = b2;
            Bytes[WriterIndex + 2] = b3;
            WriterIndex += 3;
        }

        #region segment


        public void BeginWriteSegment(out int oldSize)
        {
            oldSize = Size;
            EnsureWrite(1);
            WriterIndex += 1;
        }

        public void EndWriteSegment(int oldSize)
        {
            int startPos = ReaderIndex + oldSize;
            int segmentSize = WriterIndex - startPos - 1;

            // 0 111 1111
            if (segmentSize < 0x80)
            {
                Bytes[startPos] = (byte)segmentSize;
            }
            else if (segmentSize < 0x4000) // 10 11 1111, -
            {
                EnsureWrite(1);
                Bytes[WriterIndex] = Bytes[startPos + 1];
                Bytes[startPos + 1] = (byte)segmentSize;

                Bytes[startPos] = (byte)((segmentSize >> 8) | 0x80);
                WriterIndex += 1;
            }
            else if (segmentSize < 0x200000) // 110 1 1111, -,-
            {
                EnsureWrite(2);
                Bytes[WriterIndex + 1] = Bytes[startPos + 2];
                Bytes[startPos + 2] = (byte)segmentSize;

                Bytes[WriterIndex] = Bytes[startPos + 1];
                Bytes[startPos + 1] = (byte)(segmentSize >> 8);

                Bytes[startPos] = (byte)((segmentSize >> 16) | 0xc0);
                WriterIndex += 2;
            }
            else if (segmentSize < 0x10000000) // 1110 1111,-,-,-
            {
                EnsureWrite(3);
                Bytes[WriterIndex + 2] = Bytes[startPos + 3];
                Bytes[startPos + 3] = (byte)segmentSize;

                Bytes[WriterIndex + 1] = Bytes[startPos + 2];
                Bytes[startPos + 2] = (byte)(segmentSize >> 8);

                Bytes[WriterIndex] = Bytes[startPos + 1];
                Bytes[startPos + 1] = (byte)(segmentSize >> 16);

                Bytes[startPos] = (byte)((segmentSize >> 24) | 0xe0);
                WriterIndex += 3;
            }
            else
            {
                throw new SerializationException("exceed max segment size");
            }
        }

        public void ReadSegment(out int startIndex, out int segmentSize)
        {
            EnsureRead(1);
            int h = Bytes[ReaderIndex++];

            startIndex = ReaderIndex;

            if (h < 0x80)
            {
                segmentSize = h;
                ReaderIndex += segmentSize;
            }
            else if (h < 0xc0)
            {
                EnsureRead(1);
                segmentSize = ((h & 0x3f) << 8) | Bytes[ReaderIndex];
                int endPos = ReaderIndex + segmentSize;
                Bytes[ReaderIndex] = Bytes[endPos];
                ReaderIndex += segmentSize + 1;
            }
            else if (h < 0xe0)
            {
                EnsureRead(2);
                segmentSize = ((h & 0x1f) << 16) | ((int)Bytes[ReaderIndex] << 8) | Bytes[ReaderIndex + 1];
                int endPos = ReaderIndex + segmentSize;
                Bytes[ReaderIndex] = Bytes[endPos];
                Bytes[ReaderIndex + 1] = Bytes[endPos + 1];
                ReaderIndex += segmentSize + 2;
            }
            else if (h < 0xf0)
            {
                EnsureRead(3);
                segmentSize = ((h & 0x0f) << 24) | ((int)Bytes[ReaderIndex] << 16) | ((int)Bytes[ReaderIndex + 1] << 8) | Bytes[ReaderIndex + 2];
                int endPos = ReaderIndex + segmentSize;
                Bytes[ReaderIndex] = Bytes[endPos];
                Bytes[ReaderIndex + 1] = Bytes[endPos + 1];
                Bytes[ReaderIndex + 2] = Bytes[endPos + 2];
                ReaderIndex += segmentSize + 3;
            }
            else
            {
                throw new SerializationException("exceed max size");
            }
            if (ReaderIndex > WriterIndex)
            {
                throw new SerializationException("segment data not enough");
            }
        }

        public void ReadSegment(ByteBuf buf)
        {
            ReadSegment(out int startPos, out var size);
            buf.Bytes = Bytes;
            buf.ReaderIndex = startPos;
            buf.WriterIndex = startPos + size;
        }

        public void EnterSegment(out SegmentSaveState saveState)
        {
            ReadSegment(out int startPos, out int size);

            saveState = new SegmentSaveState(ReaderIndex, WriterIndex);
            ReaderIndex = startPos;
            WriterIndex = startPos + size;
        }

        public void LeaveSegment(SegmentSaveState saveState)
        {
            ReaderIndex = saveState.ReaderIndex;
            WriterIndex = saveState.WriterIndex;
        }

        #endregion

        public override string ToString()
        {
            string[] datas = new string[WriterIndex - ReaderIndex];
            for (var i = ReaderIndex; i < WriterIndex; i++)
            {
                datas[i - ReaderIndex] = Bytes[i].ToString("X2");
            }
            return string.Join(".", datas);
        }

        public override bool Equals(object obj)
        {
            return (obj is ByteBuf other) && Equals(other);
        }

        public bool Equals(ByteBuf other)
        {
            if (other == null)
            {
                return false;
            }
            if (Size != other.Size)
            {
                return false;
            }
            for (int i = 0, n = Size; i < n; i++)
            {
                if (Bytes[ReaderIndex + i] != other.Bytes[other.ReaderIndex + i])
                {
                    return false;
                }
            }
            return true;
        }

        public object Clone()
        {
            return new ByteBuf(CopyData());
        }


        public static ByteBuf FromString(string value)
        {
            var ss = value.Split(',');
            byte[] data = new byte[ss.Length];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = byte.Parse(ss[i]);
            }
            return new ByteBuf(data);
        }

        public override int GetHashCode()
        {
            int hash = 17;
            for (int i = ReaderIndex; i < WriterIndex; i++)
            {
                hash = hash * 23 + Bytes[i];
            }
            return hash;
        }

        public void Release()
        {
            _releaser?.Invoke(this);
        }

#if SUPPORT_PUERTS_ARRAYBUF
        // -- add for puerts
        public Puerts.ArrayBuffer ReadArrayBuffer()
        {
            return new Puerts.ArrayBuffer(ReadBytes());
        }

        public void WriteArrayBuffer(Puerts.ArrayBuffer bytes)
        {
            WriteBytes(bytes.Bytes);
        }
#endif
    }
}

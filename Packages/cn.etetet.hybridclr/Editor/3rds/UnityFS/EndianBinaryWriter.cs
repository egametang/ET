using System;
using System.IO;

namespace UnityFS
{
    public class EndianBinaryWriter : BinaryWriter
    {
        private readonly byte[] buffer;

        public EndianType Endian;

        public EndianBinaryWriter(Stream stream, EndianType endian = EndianType.BigEndian) : base(stream)
        {
            Endian = endian;
            buffer = new byte[8];
        }

        public long Position
        {
            get => BaseStream.Position;
            set => BaseStream.Position = value;
        }

        public long Length => BaseStream.Length;

        public override void Write(short x)
        {
            Write((ushort)x);
        }

        private unsafe void WriteBufferBigEndian(byte[] dst, byte* src, int size)
        {
            System.Diagnostics.Debug.Assert(BitConverter.IsLittleEndian);
            for(int i = 0; i < size; i++)
            {
                dst[i] = src[size - i - 1];
            }
        }

        public unsafe override void Write(ushort x)
        {
            if (Endian == EndianType.BigEndian)
            {
                WriteBufferBigEndian(buffer, (byte*)&x, 2);
                Write(buffer, 0, 2);
                return;
            }
            base.Write(x);
        }

        public override void Write(int x)
        {
            Write((uint)x);
        }

        public unsafe override void Write(uint x)
        {
            if (Endian == EndianType.BigEndian)
            {
                WriteBufferBigEndian(buffer, (byte*)&x, 4);
                Write(buffer, 0, 4);
                return;
            }
            base.Write(x);
        }

        public override void Write(long x)
        {
            Write((ulong)x);
        }

        public unsafe override void Write(ulong x)
        {
            if (Endian == EndianType.BigEndian)
            {
                WriteBufferBigEndian(buffer, (byte*)&x, 8);
                Write(buffer, 0, 8);
                return;
            }
            base.Write(x);
        }

        public override void Write(float x)
        {
            if (Endian == EndianType.BigEndian)
            {
                var buf = BitConverter.GetBytes(x);
                Array.Reverse(buf, 0, 4);
                Write(buf, 0, 4);
                return;
            }
            base.Write(x);
        }

        public override void Write(double x)
        {
            if (Endian == EndianType.BigEndian)
            {
                var buf = BitConverter.GetBytes(x);
                Array.Reverse(buf, 0, 8);
                Write(buf, 0, 8);
                return;
            }
            base.Write(x);
        }
    }
}

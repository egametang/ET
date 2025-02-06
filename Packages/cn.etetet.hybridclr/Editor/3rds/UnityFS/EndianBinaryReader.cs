using System;
using System.IO;

namespace UnityFS
{
    public class EndianBinaryReader : BinaryReader
    {
        private readonly byte[] buffer;

        public EndianType Endian;

        public EndianBinaryReader(Stream stream, EndianType endian = EndianType.BigEndian) : base(stream)
        {
            Endian = endian;
            buffer = new byte[8];
        }

        public long Position
        {
            get => BaseStream.Position;
            set => BaseStream.Position = value;
        }

        private unsafe void ReadBufferBigEndian(byte* dst, byte[] src, int size)
        {
            System.Diagnostics.Debug.Assert(BitConverter.IsLittleEndian);
            for (int i = 0; i < size; i++)
            {
                dst[i] = src[size - i - 1];
            }
        }

        public override short ReadInt16()
        {
            return (short)ReadUInt16();
        }

        public unsafe override ushort ReadUInt16()
        {
            if (Endian == EndianType.BigEndian)
            {
                Read(buffer, 0, 2);
                ushort x = 0;
                ReadBufferBigEndian((byte*)&x, buffer, 2);
                return x;
            }
            return base.ReadUInt16();
        }

        public override int ReadInt32()
        {
            return (int)ReadUInt32();
        }

        public unsafe override uint ReadUInt32()
        {
            if (Endian == EndianType.BigEndian)
            {
                Read(buffer, 0, 4);
                uint x = 0;
                ReadBufferBigEndian((byte*)&x, buffer, 4);
                return x;
            }
            return base.ReadUInt32();
        }

        public override long ReadInt64()
        {
            return (long)ReadUInt64();
        }

        public unsafe override ulong ReadUInt64()
        {
            if (Endian == EndianType.BigEndian)
            {
                Read(buffer, 0, 8);

                ulong x = 0;
                ReadBufferBigEndian((byte*)&x, buffer, 8);
                return x;
            }
            return base.ReadUInt64();
        }

        public override float ReadSingle()
        {
            if (Endian == EndianType.BigEndian)
            {
                Read(buffer, 0, 4);
                Array.Reverse(buffer, 0, 4);
                return BitConverter.ToSingle(buffer, 0);
            }
            return base.ReadSingle();
        }

        public override double ReadDouble()
        {
            if (Endian == EndianType.BigEndian)
            {
                Read(buffer, 0, 8);
                Array.Reverse(buffer);
                return BitConverter.ToDouble(buffer, 0);
            }
            return base.ReadDouble();
        }
    }
}

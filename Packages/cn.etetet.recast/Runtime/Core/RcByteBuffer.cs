using System;
using System.Buffers.Binary;

namespace DotRecast.Core
{
    public class RcByteBuffer
    {
        private RcByteOrder _order;
        private byte[] _bytes;
        private int _position;

        public RcByteBuffer(byte[] bytes)
        {
            _order = BitConverter.IsLittleEndian
                ? RcByteOrder.LITTLE_ENDIAN
                : RcByteOrder.BIG_ENDIAN;

            _bytes = bytes;
            _position = 0;
        }

        public RcByteOrder Order()
        {
            return _order;
        }

        public void Order(RcByteOrder order)
        {
            _order = order;
        }

        public int Limit()
        {
            return _bytes.Length - _position;
        }

        public int Remaining()
        {
            int rem = Limit();
            return rem > 0 ? rem : 0;
        }


        public void Position(int pos)
        {
            _position = pos;
        }

        public int Position()
        {
            return _position;
        }

        public Span<byte> ReadBytes(int length)
        {
            var nextPos = _position + length;
            (nextPos, _position) = (_position, nextPos);

            return _bytes.AsSpan(nextPos, length);
        }

        public byte Get()
        {
            var span = ReadBytes(1);
            return span[0];
        }

        public short GetShort()
        {
            var span = ReadBytes(2);
            if (_order == RcByteOrder.BIG_ENDIAN)
            {
                return BinaryPrimitives.ReadInt16BigEndian(span);
            }
            else
            {
                return BinaryPrimitives.ReadInt16LittleEndian(span);
            }
        }


        public int GetInt()
        {
            var span = ReadBytes(4);
            if (_order == RcByteOrder.BIG_ENDIAN)
            {
                return BinaryPrimitives.ReadInt32BigEndian(span);
            }
            else
            {
                return BinaryPrimitives.ReadInt32LittleEndian(span);
            }
        }

        public float GetFloat()
        {
            var span = ReadBytes(4);
            if (_order == RcByteOrder.BIG_ENDIAN && BitConverter.IsLittleEndian)
            {
                span.Reverse();
            }
            else if (_order == RcByteOrder.LITTLE_ENDIAN && !BitConverter.IsLittleEndian)
            {
                span.Reverse();
            }

            return BitConverter.ToSingle(span);
        }

        public long GetLong()
        {
            var span = ReadBytes(8);
            if (_order == RcByteOrder.BIG_ENDIAN)
            {
                return BinaryPrimitives.ReadInt64BigEndian(span);
            }
            else
            {
                return BinaryPrimitives.ReadInt64LittleEndian(span);
            }
        }

        public void PutFloat(float v)
        {
            // if (_order == ByteOrder.BIG_ENDIAN)
            // {
            //     BinaryPrimitives.WriteInt32BigEndian(_bytes[_position]);
            // }
            // else
            // {
            //     BinaryPrimitives.ReadInt64LittleEndian(span);
            // }

            // ?
        }

        public void PutInt(int v)
        {
            // ?
        }
    }
}

using System;
using System.Net;
using System.Text;

namespace FlyingWormConsole3.LiteNetLib.Utils
{
    public class NetDataReader
    {
        protected byte[] _data;
        protected int _position;
        protected int _dataSize;
        private int _offset;

        public byte[] RawData
        {
            get { return _data; }
        }

        public int RawDataSize
        {
            get { return _dataSize; }
        }

        public int UserDataOffset
        {
            get { return _offset; }
        }

        public int UserDataSize
        {
            get { return _dataSize - _offset; }
        }

        public bool IsNull
        {
            get { return _data == null; }
        }

        public int Position
        {
            get { return _position; }
        }

        public bool EndOfData
        {
            get { return _position == _dataSize; }
        }

        public int AvailableBytes
        {
            get { return _dataSize - _position; }
        }
        
        public void SkipBytes(int count) 
        {
            _position += count;
        }

        public void SetSource(NetDataWriter dataWriter)
        {
            _data = dataWriter.Data;
            _position = 0;
            _offset = 0;
            _dataSize = dataWriter.Length;
        }

        public void SetSource(byte[] source)
        {
            _data = source;
            _position = 0;
            _offset = 0;
            _dataSize = source.Length;
        }

        public void SetSource(byte[] source, int offset)
        {
            _data = source;
            _position = offset;
            _offset = offset;
            _dataSize = source.Length;
        }

        public void SetSource(byte[] source, int offset, int maxSize)
        {
            _data = source;
            _position = offset;
            _offset = offset;
            _dataSize = maxSize;
        }

        public NetDataReader()
        {

        }

        public NetDataReader(NetDataWriter writer)
        {
            SetSource(writer);
        }

        public NetDataReader(byte[] source)
        {
            SetSource(source);
        }

        public NetDataReader(byte[] source, int offset)
        {
            SetSource(source, offset);
        }

        public NetDataReader(byte[] source, int offset, int maxSize)
        {
            SetSource(source, offset, maxSize);
        }

        #region GetMethods
        public IPEndPoint GetNetEndPoint()
        {
            string host = GetString(1000);
            int port = GetInt();
            return NetUtils.MakeEndPoint(host, port);
        }

        public byte GetByte()
        {
            byte res = _data[_position];
            _position += 1;
            return res;
        }

        public sbyte GetSByte()
        {
            var b = (sbyte)_data[_position];
            _position++;
            return b;
        }

        public bool[] GetBoolArray()
        {
            ushort size = BitConverter.ToUInt16(_data, _position);
            _position += 2;
            var arr = new bool[size];
            Buffer.BlockCopy(_data, _position, arr, 0, size);
            _position += size;
            return arr;
        }

        public ushort[] GetUShortArray()
        {
            ushort size = BitConverter.ToUInt16(_data, _position);
            _position += 2;
            var arr = new ushort[size];
            Buffer.BlockCopy(_data, _position, arr, 0, size * 2);
            _position += size * 2;
            return arr;
        }

        public short[] GetShortArray()
        {
            ushort size = BitConverter.ToUInt16(_data, _position);
            _position += 2;
            var arr = new short[size];
            Buffer.BlockCopy(_data, _position, arr, 0, size * 2);
            _position += size * 2;
            return arr;
        }

        public long[] GetLongArray()
        {
            ushort size = BitConverter.ToUInt16(_data, _position);
            _position += 2;
            var arr = new long[size];
            Buffer.BlockCopy(_data, _position, arr, 0, size * 8);
            _position += size * 8;
            return arr;
        }

        public ulong[] GetULongArray()
        {
            ushort size = BitConverter.ToUInt16(_data, _position);
            _position += 2;
            var arr = new ulong[size];
            Buffer.BlockCopy(_data, _position, arr, 0, size * 8);
            _position += size * 8;
            return arr;
        }

        public int[] GetIntArray()
        {
            ushort size = BitConverter.ToUInt16(_data, _position);
            _position += 2;
            var arr = new int[size];
            Buffer.BlockCopy(_data, _position, arr, 0, size * 4);
            _position += size * 4;
            return arr;
        }

        public uint[] GetUIntArray()
        {
            ushort size = BitConverter.ToUInt16(_data, _position);
            _position += 2;
            var arr = new uint[size];
            Buffer.BlockCopy(_data, _position, arr, 0, size * 4);
            _position += size * 4;
            return arr;
        }

        public float[] GetFloatArray()
        {
            ushort size = BitConverter.ToUInt16(_data, _position);
            _position += 2;
            var arr = new float[size];
            Buffer.BlockCopy(_data, _position, arr, 0, size * 4);
            _position += size * 4;
            return arr;
        }

        public double[] GetDoubleArray()
        {
            ushort size = BitConverter.ToUInt16(_data, _position);
            _position += 2;
            var arr = new double[size];
            Buffer.BlockCopy(_data, _position, arr, 0, size * 8);
            _position += size * 8;
            return arr;
        }

        public string[] GetStringArray()
        {
            ushort size = BitConverter.ToUInt16(_data, _position);
            _position += 2;
            var arr = new string[size];
            for (int i = 0; i < size; i++)
            {
                arr[i] = GetString();
            }
            return arr;
        }

        public string[] GetStringArray(int maxStringLength)
        {
            ushort size = BitConverter.ToUInt16(_data, _position);
            _position += 2;
            var arr = new string[size];
            for (int i = 0; i < size; i++)
            {
                arr[i] = GetString(maxStringLength);
            }
            return arr;
        }

        public bool GetBool()
        {
            bool res = _data[_position] > 0;
            _position += 1;
            return res;
        }

        public char GetChar()
        {
            char result = BitConverter.ToChar(_data, _position);
            _position += 2;
            return result;
        }

        public ushort GetUShort()
        {
            ushort result = BitConverter.ToUInt16(_data, _position);
            _position += 2;
            return result;
        }

        public short GetShort()
        {
            short result = BitConverter.ToInt16(_data, _position);
            _position += 2;
            return result;
        }

        public long GetLong()
        {
            long result = BitConverter.ToInt64(_data, _position);
            _position += 8;
            return result;
        }

        public ulong GetULong()
        {
            ulong result = BitConverter.ToUInt64(_data, _position);
            _position += 8;
            return result;
        }

        public int GetInt()
        {
            int result = BitConverter.ToInt32(_data, _position);
            _position += 4;
            return result;
        }

        public uint GetUInt()
        {
            uint result = BitConverter.ToUInt32(_data, _position);
            _position += 4;
            return result;
        }

        public float GetFloat()
        {
            float result = BitConverter.ToSingle(_data, _position);
            _position += 4;
            return result;
        }

        public double GetDouble()
        {
            double result = BitConverter.ToDouble(_data, _position);
            _position += 8;
            return result;
        }

        public string GetString(int maxLength)
        {
            int bytesCount = GetInt();
            if (bytesCount <= 0 || bytesCount > maxLength*2)
            {
                return string.Empty;
            }

            int charCount = Encoding.UTF8.GetCharCount(_data, _position, bytesCount);
            if (charCount > maxLength)
            {
                return string.Empty;
            }

            string result = Encoding.UTF8.GetString(_data, _position, bytesCount);
            _position += bytesCount;
            return result;
        }

        public string GetString()
        {
            int bytesCount = GetInt();
            if (bytesCount <= 0)
            {
                return string.Empty;
            }

            string result = Encoding.UTF8.GetString(_data, _position, bytesCount);
            _position += bytesCount;
            return result;
        }

        public ArraySegment<byte> GetRemainingBytesSegment()
        {
            ArraySegment<byte> segment = new ArraySegment<byte>(_data, _position, AvailableBytes);
            _position = _data.Length;
            return segment;
        }

        public T Get<T>() where T : INetSerializable, new()
        {
            var obj = new T();
            obj.Deserialize(this);
            return obj;
        }

        public byte[] GetRemainingBytes()
        {
            byte[] outgoingData = new byte[AvailableBytes];
            Buffer.BlockCopy(_data, _position, outgoingData, 0, AvailableBytes);
            _position = _data.Length;
            return outgoingData;
        }

        public void GetBytes(byte[] destination, int start, int count)
        {
            Buffer.BlockCopy(_data, _position, destination, start, count);
            _position += count;
        }

        public void GetBytes(byte[] destination, int count)
        {
            Buffer.BlockCopy(_data, _position, destination, 0, count);
            _position += count;
        }
        
        public sbyte[] GetSBytesWithLength()
        {
            int length = GetInt();
            sbyte[] outgoingData = new sbyte[length];
            Buffer.BlockCopy(_data, _position, outgoingData, 0, length);
            _position += length;
            return outgoingData;
        }
        
        public byte[] GetBytesWithLength()
        {
            int length = GetInt();
            byte[] outgoingData = new byte[length];
            Buffer.BlockCopy(_data, _position, outgoingData, 0, length);
            _position += length;
            return outgoingData;
        }
        #endregion

        #region PeekMethods

        public byte PeekByte()
        {
            return _data[_position];
        }

        public sbyte PeekSByte()
        {
            return (sbyte)_data[_position];
        }

        public bool PeekBool()
        {
            return _data[_position] > 0;
        }

        public char PeekChar()
        {
            return BitConverter.ToChar(_data, _position);
        }

        public ushort PeekUShort()
        {
            return BitConverter.ToUInt16(_data, _position);
        }

        public short PeekShort()
        {
            return BitConverter.ToInt16(_data, _position);
        }

        public long PeekLong()
        {
            return BitConverter.ToInt64(_data, _position);
        }

        public ulong PeekULong()
        {
            return BitConverter.ToUInt64(_data, _position);
        }

        public int PeekInt()
        {
            return BitConverter.ToInt32(_data, _position);
        }

        public uint PeekUInt()
        {
            return BitConverter.ToUInt32(_data, _position);
        }

        public float PeekFloat()
        {
            return BitConverter.ToSingle(_data, _position);
        }

        public double PeekDouble()
        {
            return BitConverter.ToDouble(_data, _position);
        }

        public string PeekString(int maxLength)
        {
            int bytesCount = BitConverter.ToInt32(_data, _position);
            if (bytesCount <= 0 || bytesCount > maxLength * 2)
            {
                return string.Empty;
            }

            int charCount = Encoding.UTF8.GetCharCount(_data, _position + 4, bytesCount);
            if (charCount > maxLength)
            {
                return string.Empty;
            }

            string result = Encoding.UTF8.GetString(_data, _position + 4, bytesCount);
            return result;
        }

        public string PeekString()
        {
            int bytesCount = BitConverter.ToInt32(_data, _position);
            if (bytesCount <= 0)
            {
                return string.Empty;
            }

            string result = Encoding.UTF8.GetString(_data, _position + 4, bytesCount);
            return result;
        }
        #endregion

        #region TryGetMethods
        public bool TryGetByte(out byte result)
        {
            if (AvailableBytes >= 1)
            {
                result = GetByte();
                return true;
            }
            result = 0;
            return false;
        }

        public bool TryGetSByte(out sbyte result)
        {
            if (AvailableBytes >= 1)
            {
                result = GetSByte();
                return true;
            }
            result = 0;
            return false;
        }

        public bool TryGetBool(out bool result)
        {
            if (AvailableBytes >= 1)
            {
                result = GetBool();
                return true;
            }
            result = false;
            return false;
        }

        public bool TryGetChar(out char result)
        {
            if (AvailableBytes >= 2)
            {
                result = GetChar();
                return true;
            }
            result = '\0';
            return false;
        }

        public bool TryGetShort(out short result)
        {
            if (AvailableBytes >= 2)
            {
                result = GetShort();
                return true;
            }
            result = 0;
            return false;
        }

        public bool TryGetUShort(out ushort result)
        {
            if (AvailableBytes >= 2)
            {
                result = GetUShort();
                return true;
            }
            result = 0;
            return false;
        }

        public bool TryGetInt(out int result)
        {
            if (AvailableBytes >= 4)
            {
                result = GetInt();
                return true;
            }
            result = 0;
            return false;
        }

        public bool TryGetUInt(out uint result)
        {
            if (AvailableBytes >= 4)
            {
                result = GetUInt();
                return true;
            }
            result = 0;
            return false;
        }

        public bool TryGetLong(out long result)
        {
            if (AvailableBytes >= 8)
            {
                result = GetLong();
                return true;
            }
            result = 0;
            return false;
        }

        public bool TryGetULong(out ulong result)
        {
            if (AvailableBytes >= 8)
            {
                result = GetULong();
                return true;
            }
            result = 0;
            return false;
        }

        public bool TryGetFloat(out float result)
        {
            if (AvailableBytes >= 4)
            {
                result = GetFloat();
                return true;
            }
            result = 0;
            return false;
        }

        public bool TryGetDouble(out double result)
        {
            if (AvailableBytes >= 8)
            {
                result = GetDouble();
                return true;
            }
            result = 0;
            return false;
        }

        public bool TryGetString(out string result)
        {
            if (AvailableBytes >= 4)
            {
                var bytesCount = PeekInt();
                if (AvailableBytes >= bytesCount + 4)
                {
                    result = GetString();
                    return true;
                }
            }
            result = null;
            return false;
        }

        public bool TryGetStringArray(out string[] result)
        {
            ushort size;
            if (!TryGetUShort(out size))
            {
                result = null;
                return false;
            }

            result = new string[size];
            for (int i = 0; i < size; i++)
            {
                if (!TryGetString(out result[i]))
                {
                    result = null;
                    return false;
                }
            }

            return true;
        }

        public bool TryGetBytesWithLength(out byte[] result)
        {
            if (AvailableBytes >= 4)
            {
                var length = PeekInt();
                if (length >= 0 && AvailableBytes >= length + 4)
                {
                    result = GetBytesWithLength();
                    return true;
                }
            }
            result = null;
            return false;
        }
        #endregion

        public void Clear()
        {
            _position = 0;
            _dataSize = 0;
            _data = null;
        }
    }
}


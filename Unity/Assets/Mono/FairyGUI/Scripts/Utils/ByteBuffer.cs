using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;

namespace FairyGUI.Utils
{
    /// <summary>
    /// 
    /// </summary>
    public class ByteBuffer
    {
        /// <summary>
        /// 
        /// </summary>
        public bool littleEndian;

        /// <summary>
        /// 
        /// </summary>
        public string[] stringTable;

        /// <summary>
        /// 
        /// </summary>
        public int version;

        int _pointer;
        int _offset;
        int _length;
        byte[] _data;

        static byte[] temp = new byte[8];

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public ByteBuffer(byte[] data, int offset = 0, int length = -1)
        {
            _data = data;
            _pointer = 0;
            _offset = offset;
            if (length < 0)
                _length = data.Length - offset;
            else
                _length = length;
            littleEndian = false;
        }

        /// <summary>
        /// 
        /// </summary>
        public int position
        {
            get { return _pointer; }
            set { _pointer = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public int length
        {
            get { return _length; }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool bytesAvailable
        {
            get { return _pointer < _length; }
        }

        /// <summary>
        /// 
        /// </summary>
        public byte[] buffer
        {
            get { return _data; }
            set
            {
                _data = value;
                _pointer = 0;
                _offset = 0;
                _length = _data.Length;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public int Skip(int count)
        {
            _pointer += count;
            return _pointer;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public byte ReadByte()
        {
            return _data[_offset + _pointer++];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="output"></param>
        /// <param name="destIndex"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public byte[] ReadBytes(byte[] output, int destIndex, int count)
        {
            if (count > _length - _pointer)
                throw new ArgumentOutOfRangeException();

            Array.Copy(_data, _offset + _pointer, output, destIndex, count);
            _pointer += count;
            return output;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public byte[] ReadBytes(int count)
        {
            if (count > _length - _pointer)
                throw new ArgumentOutOfRangeException();

            byte[] result = new byte[count];
            Array.Copy(_data, _offset + _pointer, result, 0, count);
            _pointer += count;
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ByteBuffer ReadBuffer()
        {
            int count = ReadInt();
            ByteBuffer ba = new ByteBuffer(_data, _pointer, count);
            ba.stringTable = stringTable;
            ba.version = version;
            _pointer += count;
            return ba;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public char ReadChar()
        {
            return (char)ReadShort();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool ReadBool()
        {
            bool result = _data[_offset + _pointer] == 1;
            _pointer++;
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public short ReadShort()
        {
            int startIndex = _offset + _pointer;
            _pointer += 2;
            if (littleEndian)
                return (short)(_data[startIndex] | (_data[startIndex + 1] << 8));
            else
                return (short)((_data[startIndex] << 8) | _data[startIndex + 1]);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ushort ReadUshort()
        {
            return (ushort)ReadShort();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int ReadInt()
        {
            int startIndex = _offset + _pointer;
            _pointer += 4;
            if (littleEndian)
                return (_data[startIndex]) | (_data[startIndex + 1] << 8) | (_data[startIndex + 2] << 16) | (_data[startIndex + 3] << 24);
            else
                return (_data[startIndex] << 24) | (_data[startIndex + 1] << 16) | (_data[startIndex + 2] << 8) | (_data[startIndex + 3]);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public uint ReadUint()
        {
            return (uint)ReadInt();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public float ReadFloat()
        {
            int startIndex = _offset + _pointer;
            _pointer += 4;
            if (littleEndian == BitConverter.IsLittleEndian)
                return BitConverter.ToSingle(_data, startIndex);
            else
            {
                temp[3] = _data[startIndex];
                temp[2] = _data[startIndex + 1];
                temp[1] = _data[startIndex + 2];
                temp[0] = _data[startIndex + 3];
                return BitConverter.ToSingle(temp, 0);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public long ReadLong()
        {
            int startIndex = _offset + _pointer;
            _pointer += 8;
            if (littleEndian)
            {
                int i1 = (_data[startIndex]) | (_data[startIndex + 1] << 8) | (_data[startIndex + 2] << 16) | (_data[startIndex + 3] << 24);
                int i2 = (_data[startIndex + 4]) | (_data[startIndex + 5] << 8) | (_data[startIndex + 6] << 16) | (_data[startIndex + 7] << 24);
                return (uint)i1 | ((long)i2 << 32);
            }
            else
            {
                int i1 = (_data[startIndex] << 24) | (_data[startIndex + 1] << 16) | (_data[startIndex + 2] << 8) | (_data[startIndex + 3]);
                int i2 = (_data[startIndex + 4] << 24) | (_data[startIndex + 5] << 16) | (_data[startIndex + 6] << 8) | (_data[startIndex + 7]);
                return (uint)i2 | ((long)i1 << 32);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public double ReadDouble()
        {
            int startIndex = _offset + _pointer;
            _pointer += 8;
            if (littleEndian == BitConverter.IsLittleEndian)
                return BitConverter.ToDouble(_data, startIndex);
            else
            {
                temp[7] = _data[startIndex];
                temp[6] = _data[startIndex + 1];
                temp[5] = _data[startIndex + 2];
                temp[4] = _data[startIndex + 3];
                temp[3] = _data[startIndex + 4];
                temp[2] = _data[startIndex + 5];
                temp[1] = _data[startIndex + 6];
                temp[0] = _data[startIndex + 7];
                return BitConverter.ToSingle(temp, 0);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string ReadString()
        {
            ushort len = ReadUshort();
            string result = Encoding.UTF8.GetString(_data, _offset + _pointer, len);
            _pointer += len;
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="len"></param>
        /// <returns></returns>
        public string ReadString(int len)
        {
            string result = Encoding.UTF8.GetString(_data, _offset + _pointer, len);
            _pointer += len;
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string ReadS()
        {
            int index = ReadUshort();
            if (index == 65534) //null
                return null;
            else if (index == 65533)
                return string.Empty;
            else
                return stringTable[index];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cnt"></param>
        /// <returns></returns>
        public string[] ReadSArray(int cnt)
        {
            string[] ret = new string[cnt];
            for (int i = 0; i < cnt; i++)
                ret[i] = ReadS();

            return ret;
        }

        private static List<GPathPoint> helperPoints = new List<GPathPoint>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="result"></param>
        public List<GPathPoint> ReadPath()
        {
            helperPoints.Clear();
            
            int len = ReadInt();
            if (len == 0)
                return helperPoints;

            for (int i = 0; i < len; i++)
            {
                GPathPoint.CurveType curveType = (GPathPoint.CurveType)ReadByte();
                switch (curveType)
                {
                    case GPathPoint.CurveType.Bezier:
                        helperPoints.Add(new GPathPoint(new Vector3(ReadFloat(), ReadFloat(), 0),
                            new Vector3(ReadFloat(), ReadFloat(), 0)));
                        break;

                    case GPathPoint.CurveType.CubicBezier:
                        helperPoints.Add(new GPathPoint(new Vector3(ReadFloat(), ReadFloat(), 0),
                            new Vector3(ReadFloat(), ReadFloat(), 0),
                            new Vector3(ReadFloat(), ReadFloat(), 0)));
                        break;

                    default:
                        helperPoints.Add(new GPathPoint(new Vector3(ReadFloat(), ReadFloat(), 0), curveType));
                        break;
                }
            }

            return helperPoints;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void WriteS(string value)
        {
            int index = ReadUshort();
            if (index != 65534 && index != 65533)
                stringTable[index] = value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Color ReadColor()
        {
            int startIndex = _offset + _pointer;
            byte r = _data[startIndex];
            byte g = _data[startIndex + 1];
            byte b = _data[startIndex + 2];
            byte a = _data[startIndex + 3];
            _pointer += 4;

            return new Color32(r, g, b, a);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="indexTablePos"></param>
        /// <param name="blockIndex"></param>
        /// <returns></returns>
        public bool Seek(int indexTablePos, int blockIndex)
        {
            int tmp = _pointer;
            _pointer = indexTablePos;
            int segCount = _data[_offset + _pointer++];
            if (blockIndex < segCount)
            {
                bool useShort = _data[_offset + _pointer++] == 1;
                int newPos;
                if (useShort)
                {
                    _pointer += 2 * blockIndex;
                    newPos = ReadShort();
                }
                else
                {
                    _pointer += 4 * blockIndex;
                    newPos = ReadInt();
                }

                if (newPos > 0)
                {
                    _pointer = indexTablePos + newPos;
                    return true;
                }
                else
                {
                    _pointer = tmp;
                    return false;
                }
            }
            else
            {
                _pointer = tmp;
                return false;
            }
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using UnityEngine;

namespace UniFramework.Network
{
	/// <summary>
	/// 环形缓冲区
	/// </summary>
	public class RingBuffer
	{
		private readonly byte[] _buffer;
		private int _readerIndex = 0;
		private int _writerIndex = 0;
		private int _markedReaderIndex = 0;
		private int _markedWriterIndex = 0;


		/// <summary>
		/// 字节缓冲区
		/// </summary>
		public RingBuffer(int capacity)
		{
			_buffer = new byte[capacity];
		}

		/// <summary>
		/// 字节缓冲区
		/// </summary>
		public RingBuffer(byte[] data)
		{
			_buffer = data;
			_writerIndex = data.Length;
		}

		/// <summary>
		/// 获取数据
		/// </summary>
		public byte[] GetBuffer()
		{
			return _buffer;
		}

		/// <summary>
		/// 缓冲区容量
		/// </summary>
		public int Capacity
		{
			get { return _buffer.Length; }
		}

		/// <summary>
		/// 清空缓冲区
		/// </summary>
		public void Clear()
		{
			_readerIndex = 0;
			_writerIndex = 0;
			_markedReaderIndex = 0;
			_markedWriterIndex = 0;
		}

		/// <summary>
		/// 删除已读部分，重新初始化数组
		/// </summary>
		public void DiscardReadBytes()
		{
			if (_readerIndex == 0)
				return;

			if (_readerIndex == _writerIndex)
			{
				_readerIndex = 0;
				_writerIndex = 0;
			}
			else
			{
				for (int i = 0, j = _readerIndex, length = _writerIndex - _readerIndex; i < length; i++, j++)
				{
					_buffer[i] = _buffer[j];
				}
				_writerIndex -= _readerIndex;
				_readerIndex = 0;
			}
		}

		#region 读取相关
		/// <summary>
		/// 读取的下标位置
		/// </summary>
		public int ReaderIndex
		{
			get { return _readerIndex; }
		}

		/// <summary>
		/// 当前可读数据量
		/// </summary>
		public int ReadableBytes
		{
			get { return _writerIndex - _readerIndex; }
		}

		/// <summary>
		/// 查询是否可以读取
		/// </summary>
		/// <param name="size">读取数据量</param>
		public bool IsReadable(int size = 1)
		{
			return _writerIndex - _readerIndex >= size;
		}

		/// <summary>
		/// 标记读取的下标位置，便于某些时候回退到该位置
		/// </summary>
		public void MarkReaderIndex()
		{
			_markedReaderIndex = _readerIndex;
		}

		/// <summary>
		/// 回退到标记的读取下标位置
		/// </summary>
		public void ResetReaderIndex()
		{
			_readerIndex = _markedReaderIndex;
		}
		#endregion

		#region 写入相关
		/// <summary>
		/// 写入的下标位置
		/// </summary>
		public int WriterIndex
		{
			get { return _writerIndex; }
		}

		/// <summary>
		/// 当前可写入数据量
		/// </summary>
		public int WriteableBytes
		{
			get { return Capacity - _writerIndex; }
		}

		/// <summary>
		/// 查询是否可以写入
		/// </summary>
		/// <param name="size">写入数据量</param>
		public bool IsWriteable(int size = 1)
		{
			return Capacity - _writerIndex >= size;
		}

		/// <summary>
		/// 标记写入的下标位置，便于某些时候回退到该位置。
		/// </summary>
		public void MarkWriterIndex()
		{
			_markedWriterIndex = _writerIndex;
		}

		/// <summary>
		/// 回退到标记的写入下标位置
		/// </summary>
		public void ResetWriterIndex()
		{
			_writerIndex = _markedWriterIndex;
		}
		#endregion

		#region 读取操作
		[Conditional("DEBUG")]
		private void CheckReaderIndex(int length)
		{
			if (_readerIndex + length > _writerIndex)
			{
				throw new IndexOutOfRangeException();
			}
		}

		public byte[] ReadBytes(int count)
		{
			CheckReaderIndex(count);
			var data = new byte[count];
			Buffer.BlockCopy(_buffer, _readerIndex, data, 0, count);
			_readerIndex += count;
			return data;
		}
		public bool ReadBool()
		{
			CheckReaderIndex(1);
			return _buffer[_readerIndex++] == 1;
		}
		public byte ReadByte()
		{
			CheckReaderIndex(1);
			return _buffer[_readerIndex++];
		}
		public sbyte ReadSbyte()
		{
			return (sbyte)ReadByte();
		}
		public short ReadShort()
		{
			CheckReaderIndex(2);
			short result = BitConverter.ToInt16(_buffer, _readerIndex);
			_readerIndex += 2;
			return result;
		}
		public ushort ReadUShort()
		{
			CheckReaderIndex(2);
			ushort result = BitConverter.ToUInt16(_buffer, _readerIndex);
			_readerIndex += 2;
			return result;
		}
		public int ReadInt()
		{
			CheckReaderIndex(4);
			int result = BitConverter.ToInt32(_buffer, _readerIndex);
			_readerIndex += 4;
			return result;
		}
		public uint ReadUInt()
		{
			CheckReaderIndex(4);
			uint result = BitConverter.ToUInt32(_buffer, _readerIndex);
			_readerIndex += 4;
			return result;
		}
		public long ReadLong()
		{
			CheckReaderIndex(8);
			long result = BitConverter.ToInt64(_buffer, _readerIndex);
			_readerIndex += 8;
			return result;
		}
		public ulong ReadULong()
		{
			CheckReaderIndex(8);
			ulong result = BitConverter.ToUInt64(_buffer, _readerIndex);
			_readerIndex += 8;
			return result;
		}
		public float ReadFloat()
		{
			CheckReaderIndex(4);
			float result = BitConverter.ToSingle(_buffer, _readerIndex);
			_readerIndex += 4;
			return result;
		}
		public double ReadDouble()
		{
			CheckReaderIndex(8);
			double result = BitConverter.ToDouble(_buffer, _readerIndex);
			_readerIndex += 8;
			return result;
		}
		public string ReadUTF()
		{
			ushort count = ReadUShort();
			CheckReaderIndex(count);
			string result = Encoding.UTF8.GetString(_buffer, _readerIndex, count - 1); // 注意：读取的时候忽略字符串末尾写入结束符
			_readerIndex += count;
			return result;
		}

		public List<int> ReadListInt()
		{
			List<int> result = new List<int>();
			int count = ReadInt();
			for (int i = 0; i < count; i++)
			{
				result.Add(ReadInt());
			}
			return result;
		}
		public List<long> ReadListLong()
		{
			List<long> result = new List<long>();
			int count = ReadInt();
			for (int i = 0; i < count; i++)
			{
				result.Add(ReadLong());
			}
			return result;
		}
		public List<float> ReadListFloat()
		{
			List<float> result = new List<float>();
			int count = ReadInt();
			for (int i = 0; i < count; i++)
			{
				result.Add(ReadFloat());
			}
			return result;
		}
		public List<double> ReadListDouble()
		{
			List<double> result = new List<double>();
			int count = ReadInt();
			for (int i = 0; i < count; i++)
			{
				result.Add(ReadDouble());
			}
			return result;
		}
		public List<string> ReadListUTF()
		{
			List<string> result = new List<string>();
			int count = ReadInt();
			for (int i = 0; i < count; i++)
			{
				result.Add(ReadUTF());
			}
			return result;
		}

		public Vector2 ReadVector2()
		{
			float x = ReadFloat();
			float y = ReadFloat();
			return new Vector2(x, y);
		}
		public Vector3 ReadVector3()
		{
			float x = ReadFloat();
			float y = ReadFloat();
			float z = ReadFloat();
			return new Vector3(x, y, z);
		}
		public Vector4 ReadVector4()
		{
			float x = ReadFloat();
			float y = ReadFloat();
			float z = ReadFloat();
			float w = ReadFloat();
			return new Vector4(x, y, z, w);
		}
		#endregion

		#region 写入操作
		[Conditional("DEBUG")]
		private void CheckWriterIndex(int length)
		{
			if (_writerIndex + length > Capacity)
			{
				throw new IndexOutOfRangeException();
			}
		}

		public void WriteBytes(byte[] data)
		{
			WriteBytes(data, 0, data.Length);
		}
		public void WriteBytes(byte[] data, int offset, int count)
		{
			CheckWriterIndex(count);
			Buffer.BlockCopy(data, offset, _buffer, _writerIndex, count);
			_writerIndex += count;
		}
		public void WriteBool(bool value)
		{
			WriteByte((byte)(value ? 1 : 0));
		}
		public void WriteByte(byte value)
		{
			CheckWriterIndex(1);
			_buffer[_writerIndex++] = value;
		}
		public void WriteSbyte(sbyte value)
		{
			// 注意：从sbyte强转到byte不会有数据变化或丢失
			WriteByte((byte)value);
		}
		public void WriteShort(short value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			WriteBytes(bytes);
		}
		public void WriteUShort(ushort value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			WriteBytes(bytes);
		}
		public void WriteInt(int value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			WriteBytes(bytes);
		}
		public void WriteUInt(uint value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			WriteBytes(bytes);
		}
		public void WriteLong(long value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			WriteBytes(bytes);
		}
		public void WriteULong(ulong value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			WriteBytes(bytes);
		}
		public void WriteFloat(float value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			WriteBytes(bytes);
		}
		public void WriteDouble(double value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			WriteBytes(bytes);
		}
		public void WriteUTF(string value)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(value);
			int num = bytes.Length + 1; // 注意：字符串末尾写入结束符
			if (num > ushort.MaxValue)
				throw new FormatException($"String length cannot be greater than {ushort.MaxValue} !");

			WriteUShort(Convert.ToUInt16(num));
			WriteBytes(bytes);
			WriteByte((byte)'\0');
		}

		public void WriteListInt(List<int> values)
		{
			int count = 0;
			if (values != null)
				count = values.Count;

			WriteInt(count);
			for (int i = 0; i < count; i++)
			{
				WriteInt(values[i]);
			}
		}
		public void WriteListLong(List<long> values)
		{
			int count = 0;
			if (values != null)
				count = values.Count;

			WriteInt(count);
			for (int i = 0; i < count; i++)
			{
				WriteLong(values[i]);
			}
		}
		public void WriteListFloat(List<float> values)
		{
			int count = 0;
			if (values != null)
				count = values.Count;

			WriteInt(count);
			for (int i = 0; i < count; i++)
			{
				WriteFloat(values[i]);
			}
		}
		public void WriteListDouble(List<double> values)
		{
			int count = 0;
			if (values != null)
				count = values.Count;

			WriteInt(count);
			for (int i = 0; i < count; i++)
			{
				WriteDouble(values[i]);
			}
		}
		public void WriteListUTF(List<string> values)
		{
			int count = 0;
			if (values != null)
				count = values.Count;

			WriteInt(count);
			for (int i = 0; i < count; i++)
			{
				WriteUTF(values[i]);
			}
		}

		public void WriteVector2(Vector2 value)
		{
			WriteFloat(value.x);
			WriteFloat(value.y);
		}
		public void WriteVector3(Vector3 value)
		{
			WriteFloat(value.x);
			WriteFloat(value.y);
			WriteFloat(value.z);
		}
		public void WriteVector4(Vector4 value)
		{
			WriteFloat(value.x);
			WriteFloat(value.y);
			WriteFloat(value.z);
			WriteFloat(value.w);
		}
		#endregion

		/// <summary>
		/// 大小端转换
		/// </summary>
		public static void ReverseOrder(byte[] data)
		{
			ReverseOrder(data, 0, data.Length);
		}
		public static void ReverseOrder(byte[] data, int offset, int length)
		{
			if (length <= 1)
				return;

			int end = offset + length - 1;
			int max = offset + length / 2;
			byte temp;
			for (int index = offset; index < max; index++, end--)
			{
				temp = data[end];
				data[end] = data[index];
				data[index] = temp;
			}
		}
	}
}
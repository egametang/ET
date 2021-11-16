using System;
using System.IO;

namespace ET
{
	public static class StreamHelper
	{
		public static void WriteToUint(this Stream stream, int offset, uint num)
		{
			if(stream == null)
			{
				return;
			}

			stream.Seek(offset, SeekOrigin.Begin);
			stream.WriteByte((byte)(num & 0xff));
			stream.WriteByte((byte)((num & 0xff00) >> 8));
			stream.WriteByte((byte)((num & 0xff0000) >> 16));
			stream.WriteByte((byte)((num & 0xff000000) >> 24));
		}
		
		public static void WriteToInt(this Stream stream, int offset, int num)
		{
			if (stream == null)
			{
				return;
			}

			stream.Seek(offset, SeekOrigin.Begin);
			stream.WriteByte((byte)(num & 0xff));
			stream.WriteByte((byte)((num & 0xff00) >> 8));
			stream.WriteByte((byte)((num & 0xff0000) >> 16));
			stream.WriteByte((byte)((num & 0xff000000) >> 24));
		}
		
		public static void WriteToByte(this Stream stream, int offset, byte num)
		{
			if (stream == null)
			{
				return;
			}

			stream.Seek(offset, SeekOrigin.Begin);
			stream.WriteByte(num);
		}
		
		public static void WriteToShort(this Stream stream, int offset, short num)
		{
			if (stream == null)
			{
				return;
			}

			stream.Seek(offset, SeekOrigin.Begin);
			stream.WriteByte((byte)(num & 0xff));
			stream.WriteByte((byte)((num & 0xff00) >> 8));
		}
		
		public static void WriteToUshort(this Stream stream, int offset, ushort num)
		{
			if (stream == null)
			{
				return;
			}

			stream.Seek(offset, SeekOrigin.Begin);
			stream.WriteByte((byte)(num & 0xff));
			stream.WriteByte((byte)((num & 0xff00) >> 8));
		}

		public static int ToInt32(this MemoryStream memoryStream, int offset)
		{
			if (memoryStream == null)
			{
				return 0;
			}

			return BitConverter.ToInt32(memoryStream.GetBuffer(), offset);
		}

		public static ushort ToUInt16(this MemoryStream memoryStream, int offset)
		{
			if (memoryStream == null)
			{
				return 0;
			}

			return BitConverter.ToUInt16(memoryStream.GetBuffer(), offset);
		}

		public static byte[] ReadBytes(MemoryStream memoryStream, int offset)
		{
			if (memoryStream != null && offset >= 0)
			{
				var count = memoryStream.Length - offset;

				if (count > 0)
				{
					var bytes = new byte[count];
					memoryStream.Read(bytes, 0, bytes.Length);

					return bytes;
				}
			}

			return null;
		}

		public static void WriteBytes(MemoryStream memoryStream, byte[] bytes)
		{
			if (memoryStream != null && bytes != null)
			{
				memoryStream.Write(bytes, 0, bytes.Length);
			}
		}
	}
}
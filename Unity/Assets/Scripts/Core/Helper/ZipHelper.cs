using System.IO;
using ICSharpCode.SharpZipLib.Zip.Compression;

namespace ET
{
	public static class ZipHelper
	{
		public static byte[] Compress(byte[] content)
		{
			//return content;
			Deflater compressor = new Deflater();
			compressor.SetLevel(Deflater.BEST_COMPRESSION);

			compressor.SetInput(content);
			compressor.Finish();

			using (MemoryStream bos = new MemoryStream(content.Length))
			{
				var buf = new byte[1024];
				while (!compressor.IsFinished)
				{
					int n = compressor.Deflate(buf);
					bos.Write(buf, 0, n);
				}
				return bos.ToArray();
			}
		}

		public static byte[] Decompress(byte[] content)
		{
			return Decompress(content, 0, content.Length);
		}

		public static byte[] Decompress(byte[] content, int offset, int count)
		{
			//return content;
			Inflater decompressor = new Inflater();
			decompressor.SetInput(content, offset, count);

			using (MemoryStream bos = new MemoryStream(content.Length))
			{
				var buf = new byte[1024];
				while (!decompressor.IsFinished)
				{
					int n = decompressor.Inflate(buf);
					bos.Write(buf, 0, n);
				}
				return bos.ToArray();
			}
		}
	}
}

/*
using System.IO;
using System.IO.Compression;

namespace ET
{
	public static class ZipHelper
	{
		public static byte[] Compress(byte[] content)
		{
			using (MemoryStream ms = new MemoryStream())
			using (DeflateStream stream = new DeflateStream(ms, CompressionMode.Compress, true))
			{
				stream.Write(content, 0, content.Length);
				return ms.ToArray();
			}
		}

		public static byte[] Decompress(byte[] content)
		{
			return Decompress(content, 0, content.Length);
		}

		public static byte[] Decompress(byte[] content, int offset, int count)
		{
			using (MemoryStream ms = new MemoryStream())
			using (DeflateStream stream = new DeflateStream(new MemoryStream(content, offset, count), CompressionMode.Decompress, true))
			{
				byte[] buffer = new byte[1024];
				while (true)
				{
					int bytesRead = stream.Read(buffer, 0, 1024);
					if (bytesRead == 0)
					{
						break;
					}
					ms.Write(buffer, 0, bytesRead);
				}
				return ms.ToArray();
			}
		}
	}
}
*/
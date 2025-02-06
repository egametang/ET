using System.IO;

namespace UnityFS
{
    public static class StreamExtensions
    {
        private const int BufferSize = 81920;

        public static void CopyTo(this Stream source, Stream destination, long size)
        {
            var buffer = new byte[BufferSize];
            for (var left = size; left > 0; left -= BufferSize)
            {
                int toRead = BufferSize < left ? BufferSize : (int)left;
                int read = source.Read(buffer, 0, toRead);
                destination.Write(buffer, 0, read);
                if (read != toRead)
                {
                    return;
                }
            }
        }

        public static byte[] ReadAllBytes(this Stream source)
        {
            source.Position = 0;
            var bytes = new byte[source.Length];
            source.Read(bytes, 0, bytes.Length);
            return bytes;
        }
    }
}

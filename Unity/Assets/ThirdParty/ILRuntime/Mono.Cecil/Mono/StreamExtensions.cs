using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ILRuntime.Mono
{
    static class StreamExtensions
    {
        public static void CopyTo(this Stream src, Stream destination)
        {
			if (destination == null)
			{
				throw new ArgumentNullException("destination");
			}
			if (!src.CanRead && !src.CanWrite)
			{
				throw new ObjectDisposedException(null, "ObjectDisposed_StreamClosed");
			}
			if (!destination.CanRead && !destination.CanWrite)
			{
				throw new ObjectDisposedException("destination", "ObjectDisposed_StreamClosed");
			}
			if (!src.CanRead)
			{
				throw new NotSupportedException("NotSupported_UnreadableStream");
			}
			if (!destination.CanWrite)
			{
				throw new NotSupportedException("NotSupported_UnwritableStream");
			}
			InternalCopyTo(src, destination, 81920);
		}
        static void InternalCopyTo(Stream src, Stream destination, int bufferSize)
        {
            byte[] array = new byte[bufferSize];
            int count;
            while ((count = src.Read(array, 0, array.Length)) != 0)
            {
                destination.Write(array, 0, count);
            }
        }
    }
}

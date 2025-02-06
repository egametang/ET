/*
Recast4J Copyright (c) 2015 Piotr Piastucki piotr@jtilia.org

This software is provided 'as-is', without any express or implied
warranty.  In no event will the authors be held liable for any damages
arising from the use of this software.
Permission is granted to anyone to use this software for any purpose,
including commercial applications, and to alter it and redistribute it
freely, subject to the following restrictions:
1. The origin of this software must not be misrepresented; you must not
 claim that you wrote the original software. If you use this software
 in a product, an acknowledgment in the product documentation would be
 appreciated but is not required.
2. Altered source versions must be plainly marked as such, and must not be
 misrepresented as being the original software.
3. This notice may not be removed or altered from any source distribution.
*/

using System;
using System.IO;
using DotRecast.Core;

namespace DotRecast.Detour.Io
{
    public static class IOUtils
    {
        public static RcByteBuffer ToByteBuffer(BinaryReader @is, bool direct)
        {
            byte[] data = ToByteArray(@is);
            if (direct)
            {
                Array.Reverse(data);
            }

            return new RcByteBuffer(data);
        }

        public static byte[] ToByteArray(BinaryReader inputStream)
        {
            using var msw = new MemoryStream();
            byte[] buffer = new byte[4096];
            int l;
            while ((l = inputStream.Read(buffer)) > 0)
            {
                msw.Write(buffer, 0, l);
            }

            return msw.ToArray();
        }


        public static RcByteBuffer ToByteBuffer(BinaryReader inputStream)
        {
            var bytes = ToByteArray(inputStream);
            return new RcByteBuffer(bytes);
        }

        public static int SwapEndianness(int i)
        {
            var s = (((uint)i >> 24) & 0xFF) | (((uint)i >> 8) & 0xFF00) | (((uint)i << 8) & 0xFF0000) | ((i << 24) & 0xFF000000);
            return (int)s;
        }
    }
}
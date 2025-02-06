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
    public abstract class DtWriter
    {
        protected void Write(BinaryWriter stream, float value, RcByteOrder order)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            int i = BitConverter.ToInt32(bytes, 0);
            Write(stream, i, order);
        }

        protected void Write(BinaryWriter stream, short value, RcByteOrder order)
        {
            if (order == RcByteOrder.BIG_ENDIAN)
            {
                stream.Write((byte)((value >> 8) & 0xFF));
                stream.Write((byte)(value & 0xFF));
            }
            else
            {
                stream.Write((byte)(value & 0xFF));
                stream.Write((byte)((value >> 8) & 0xFF));
            }
        }

        protected void Write(BinaryWriter stream, long value, RcByteOrder order)
        {
            if (order == RcByteOrder.BIG_ENDIAN)
            {
                Write(stream, (int)((ulong)value >> 32), order);
                Write(stream, (int)(value & 0xFFFFFFFF), order);
            }
            else
            {
                Write(stream, (int)(value & 0xFFFFFFFF), order);
                Write(stream, (int)((ulong)value >> 32), order);
            }
        }

        protected void Write(BinaryWriter stream, int value, RcByteOrder order)
        {
            if (order == RcByteOrder.BIG_ENDIAN)
            {
                stream.Write((byte)((value >> 24) & 0xFF));
                stream.Write((byte)((value >> 16) & 0xFF));
                stream.Write((byte)((value >> 8) & 0xFF));
                stream.Write((byte)(value & 0xFF));
            }
            else
            {
                stream.Write((byte)(value & 0xFF));
                stream.Write((byte)((value >> 8) & 0xFF));
                stream.Write((byte)((value >> 16) & 0xFF));
                stream.Write((byte)((value >> 24) & 0xFF));
            }
        }

        protected void Write(BinaryWriter stream, bool @bool)
        {
            Write(stream, (byte)(@bool ? 1 : 0));
        }

        protected void Write(BinaryWriter stream, byte value)
        {
            stream.Write(value);
        }

        protected void Write(BinaryWriter stream, MemoryStream data)
        {
            data.Position = 0;
            byte[] buffer = new byte[data.Length];
            data.Read(buffer, 0, buffer.Length);
            stream.Write(buffer);
        }
    }
}
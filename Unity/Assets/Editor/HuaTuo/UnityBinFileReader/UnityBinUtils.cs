using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Text;

namespace Huatuo.Editor.GlobalManagers
{
    public static class UnityBinUtils
    {
        public static void SwapUInt(ref uint val)
        {
            val = (val >> 24) | ((val >> 8) & 0x0000ff00) | ((val << 8) & 0x00ff0000) | (val << 24);
        }

        public static string ReadRawString(this BinaryReader br)
        {
            long startPos = br.BaseStream.Position;
            while (true)
            {
                byte val = br.ReadByte();
                if(val == 0)
                    break;
            }
            int size = (int)(br.BaseStream.Position - startPos);
            br.BaseStream.Position = startPos;

            byte[] buffer = br.ReadBytes(size);
            string ret = Encoding.UTF8.GetString(buffer, 0, size - 1);

            return ret;
        }

        public static void WriteRawString(this BinaryWriter bw, string str)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(str);
            bw.Write(buffer, 0, buffer.Length);
            bw.Write((byte)0);
        }

        public static string ReadSizeString(this BinaryReader br)
        {
            int size = br.ReadInt32();
            byte[] buff = br.ReadBytes(size);
            br.BaseStream.AlignOffset4();

            string ret = Encoding.UTF8.GetString(buff);
            return ret;
        }

        public static void WriteSizeString(this BinaryWriter bw, string str)
        {
            byte[] buff = Encoding.UTF8.GetBytes(str);
            bw.Write(buff.Length);
            bw.Write(buff, 0, buff.Length);
            bw.BaseStream.AlignOffset4();
        }

        public static void AlignOffset4(this Stream stream)
        {
            int offset = (((int)stream.Position + 3) >> 2) << 2;
            stream.Position = offset;
        }

        public static long AlignedReadInt64(this BinaryReader br)
        {
            br.BaseStream.AlignOffset4();
            return br.ReadInt64();
        }

        public static void AlignedWriteInt64(this BinaryWriter bw, long val)
        {
            bw.BaseStream.AlignOffset4();
            bw.Write(val);
        }
    }
}


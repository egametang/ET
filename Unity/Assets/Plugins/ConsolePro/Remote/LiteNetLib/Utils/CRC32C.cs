#if NETCOREAPP3_0_OR_GREATER || NETCOREAPP3_1
using System;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
#endif

namespace FlyingWormConsole3.LiteNetLib.Utils
{
    //Implementation from Crc32.NET
    public static class CRC32C
    {
        public const int ChecksumSize = 4;
        private const uint Poly = 0x82F63B78u;
        private static readonly uint[] Table;

        static CRC32C()
        {
#if NETCOREAPP3_0_OR_GREATER || NETCOREAPP3_1
            if(Sse42.IsSupported)
                return;
#endif
            Table = new uint[16 * 256];
            for (uint i = 0; i < 256; i++)
            {
                uint res = i;
                for (int t = 0; t < 16; t++)
                {
                    for (int k = 0; k < 8; k++) 
                        res = (res & 1) == 1 ? Poly ^ (res >> 1) : (res >> 1);
                    Table[t * 256 + i] = res;
                }
            }
        }

        /// <summary>
        /// Compute CRC32C for data
        /// </summary>
        /// <param name="input">input data</param>
        /// <param name="offset">offset</param>
        /// <param name="length">length</param>
        /// <returns>CRC32C checksum</returns>
        public static uint Compute(byte[] input, int offset, int length)
        {
            uint crcLocal = uint.MaxValue;
#if NETCOREAPP3_0_OR_GREATER || NETCOREAPP3_1
            if (Sse42.IsSupported)
            {
                var data = new ReadOnlySpan<byte>(input, offset, length);
                int processed = 0;
                if (Sse42.X64.IsSupported && data.Length > sizeof(ulong))
                {
                    processed = data.Length / sizeof(ulong) * sizeof(ulong);
                    var ulongs = MemoryMarshal.Cast<byte, ulong>(data.Slice(0, processed));
                    ulong crclong = crcLocal;
                    for (int i = 0; i < ulongs.Length; i++)
                    {
                        crclong = Sse42.X64.Crc32(crclong, ulongs[i]);
                    }

                    crcLocal = (uint)crclong;
                }
                else if (data.Length > sizeof(uint))
                {
                    processed = data.Length / sizeof(uint) * sizeof(uint);
                    var uints = MemoryMarshal.Cast<byte, uint>(data.Slice(0, processed));
                    for (int i = 0; i < uints.Length; i++)
                    {
                        crcLocal = Sse42.Crc32(crcLocal, uints[i]);
                    }
                }

                for (int i = processed; i < data.Length; i++)
                {
                    crcLocal = Sse42.Crc32(crcLocal, data[i]);
                }

                return crcLocal ^ uint.MaxValue;
            }
#endif
            while (length >= 16)
            {
                var a = Table[(3 * 256) + input[offset + 12]]
                        ^ Table[(2 * 256) + input[offset + 13]]
                        ^ Table[(1 * 256) + input[offset + 14]]
                        ^ Table[(0 * 256) + input[offset + 15]];

                var b = Table[(7 * 256) + input[offset + 8]]
                        ^ Table[(6 * 256) + input[offset + 9]]
                        ^ Table[(5 * 256) + input[offset + 10]]
                        ^ Table[(4 * 256) + input[offset + 11]];

                var c = Table[(11 * 256) + input[offset + 4]]
                        ^ Table[(10 * 256) + input[offset + 5]]
                        ^ Table[(9 * 256) + input[offset + 6]]
                        ^ Table[(8 * 256) + input[offset + 7]];

                var d = Table[(15 * 256) + ((byte)crcLocal ^ input[offset])]
                        ^ Table[(14 * 256) + ((byte)(crcLocal >> 8) ^ input[offset + 1])]
                        ^ Table[(13 * 256) + ((byte)(crcLocal >> 16) ^ input[offset + 2])]
                        ^ Table[(12 * 256) + ((crcLocal >> 24) ^ input[offset + 3])];

                crcLocal = d ^ c ^ b ^ a;
                offset += 16;
                length -= 16;
            }
            while (--length >= 0)
                crcLocal = Table[(byte)(crcLocal ^ input[offset++])] ^ crcLocal >> 8;
            return crcLocal ^ uint.MaxValue;
        }
    }
}

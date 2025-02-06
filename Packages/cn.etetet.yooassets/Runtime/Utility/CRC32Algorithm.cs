using System;
using System.Security.Cryptography;

namespace YooAsset
{
    internal class SafeProxy
    {
        private const uint Poly = 0xedb88320u;
        private readonly uint[] _table = new uint[16 * 256];

        internal SafeProxy()
        {
            Init(Poly);
        }
        public void Init(uint poly)
        {
            var table = _table;
            for (uint i = 0; i < 256; i++)
            {
                uint res = i;
                for (int t = 0; t < 16; t++)
                {
                    for (int k = 0; k < 8; k++) res = (res & 1) == 1 ? poly ^ (res >> 1) : (res >> 1);
                    table[(t * 256) + i] = res;
                }
            }
        }
        public uint Append(uint crc, byte[] input, int offset, int length)
        {
            uint crcLocal = uint.MaxValue ^ crc;

            uint[] table = _table;
            while (length >= 16)
            {
                var a = table[(3 * 256) + input[offset + 12]]
                    ^ table[(2 * 256) + input[offset + 13]]
                    ^ table[(1 * 256) + input[offset + 14]]
                    ^ table[(0 * 256) + input[offset + 15]];

                var b = table[(7 * 256) + input[offset + 8]]
                    ^ table[(6 * 256) + input[offset + 9]]
                    ^ table[(5 * 256) + input[offset + 10]]
                    ^ table[(4 * 256) + input[offset + 11]];

                var c = table[(11 * 256) + input[offset + 4]]
                    ^ table[(10 * 256) + input[offset + 5]]
                    ^ table[(9 * 256) + input[offset + 6]]
                    ^ table[(8 * 256) + input[offset + 7]];

                var d = table[(15 * 256) + ((byte)crcLocal ^ input[offset])]
                    ^ table[(14 * 256) + ((byte)(crcLocal >> 8) ^ input[offset + 1])]
                    ^ table[(13 * 256) + ((byte)(crcLocal >> 16) ^ input[offset + 2])]
                    ^ table[(12 * 256) + ((crcLocal >> 24) ^ input[offset + 3])];

                crcLocal = d ^ c ^ b ^ a;
                offset += 16;
                length -= 16;
            }

            while (--length >= 0)
                crcLocal = table[(byte)(crcLocal ^ input[offset++])] ^ crcLocal >> 8;

            return crcLocal ^ uint.MaxValue;
        }
    }

    /// <summary>
    /// This is .NET safe implementation of Crc32 algorithm.
    /// Implementation of CRC-32.
    /// This class supports several convenient static methods returning the CRC as UInt32.
    /// </summary>
    internal class CRC32Algorithm : HashAlgorithm
    {
        private uint _currentCrc;

        /// <summary>
        /// Initializes a new instance of the <see cref="CRC32Algorithm"/> class. 
        /// </summary>
        public CRC32Algorithm()
        {
#if !NETCORE13
            HashSizeValue = 32;
#endif
        }

        /// <summary>
        /// Resets internal state of the algorithm. Used internally.
        /// </summary>
        public override void Initialize()
        {
            _currentCrc = 0;
        }

        /// <summary>
        /// Appends CRC-32 from given buffer
        /// </summary>
        protected override void HashCore(byte[] input, int offset, int length)
        {
            _currentCrc = AppendInternal(_currentCrc, input, offset, length);
        }

        /// <summary>
        /// Computes CRC-32 from <see cref="HashCore"/>
        /// </summary>
        protected override byte[] HashFinal()
        {
            if (BitConverter.IsLittleEndian)
                return new[] { (byte)_currentCrc, (byte)(_currentCrc >> 8), (byte)(_currentCrc >> 16), (byte)(_currentCrc >> 24) };
            else
                return new[] { (byte)(_currentCrc >> 24), (byte)(_currentCrc >> 16), (byte)(_currentCrc >> 8), (byte)_currentCrc };
        }


        /// <summary>
        /// Computes CRC-32 from multiple buffers.
        /// Call this method multiple times to chain multiple buffers.
        /// </summary>
        /// <param name="initial">
        /// Initial CRC value for the algorithm. It is zero for the first buffer.
        /// Subsequent buffers should have their initial value set to CRC value returned by previous call to this method.
        /// </param>
        /// <param name="input">Input buffer with data to be checksummed.</param>
        /// <param name="offset">Offset of the input data within the buffer.</param>
        /// <param name="length">Length of the input data in the buffer.</param>
        /// <returns>Accumulated CRC-32 of all buffers processed so far.</returns>
        public static uint Append(uint initial, byte[] input, int offset, int length)
        {
            if (input == null)
                throw new ArgumentNullException("input");
            if (offset < 0 || length < 0 || offset + length > input.Length)
                throw new ArgumentOutOfRangeException("length");
            return AppendInternal(initial, input, offset, length);
        }

        /// <summary>
        /// Computes CRC-32 from multiple buffers.
        /// Call this method multiple times to chain multiple buffers.
        /// </summary>
        /// <param name="initial">
        /// Initial CRC value for the algorithm. It is zero for the first buffer.
        /// Subsequent buffers should have their initial value set to CRC value returned by previous call to this method.
        /// </param>
        /// <param name="input">Input buffer containing data to be checksummed.</param>
        /// <returns>Accumulated CRC-32 of all buffers processed so far.</returns>
        public static uint Append(uint initial, byte[] input)
        {
            if (input == null)
                throw new ArgumentNullException();
            return AppendInternal(initial, input, 0, input.Length);
        }

        /// <summary>
        /// Computes CRC-32 from input buffer.
        /// </summary>
        /// <param name="input">Input buffer with data to be checksummed.</param>
        /// <param name="offset">Offset of the input data within the buffer.</param>
        /// <param name="length">Length of the input data in the buffer.</param>
        /// <returns>CRC-32 of the data in the buffer.</returns>
        public static uint Compute(byte[] input, int offset, int length)
        {
            return Append(0, input, offset, length);
        }

        /// <summary>
        /// Computes CRC-32 from input buffer.
        /// </summary>
        /// <param name="input">Input buffer containing data to be checksummed.</param>
        /// <returns>CRC-32 of the buffer.</returns>
        public static uint Compute(byte[] input)
        {
            return Append(0, input);
        }

        /// <summary>
        /// Computes CRC-32 from input buffer and writes it after end of data (buffer should have 4 bytes reserved space for it). Can be used in conjunction with <see cref="IsValidWithCrcAtEnd(byte[],int,int)"/>
        /// </summary>
        /// <param name="input">Input buffer with data to be checksummed.</param>
        /// <param name="offset">Offset of the input data within the buffer.</param>
        /// <param name="length">Length of the input data in the buffer.</param>
        /// <returns>CRC-32 of the data in the buffer.</returns>
        public static uint ComputeAndWriteToEnd(byte[] input, int offset, int length)
        {
            if (length + 4 > input.Length)
                throw new ArgumentOutOfRangeException("length", "Length of data should be less than array length - 4 bytes of CRC data");
            var crc = Append(0, input, offset, length);
            var r = offset + length;
            input[r] = (byte)crc;
            input[r + 1] = (byte)(crc >> 8);
            input[r + 2] = (byte)(crc >> 16);
            input[r + 3] = (byte)(crc >> 24);
            return crc;
        }

        /// <summary>
        /// Computes CRC-32 from input buffer - 4 bytes and writes it as last 4 bytes of buffer. Can be used in conjunction with <see cref="IsValidWithCrcAtEnd(byte[])"/>
        /// </summary>
        /// <param name="input">Input buffer with data to be checksummed.</param>
        /// <returns>CRC-32 of the data in the buffer.</returns>
        public static uint ComputeAndWriteToEnd(byte[] input)
        {
            if (input.Length < 4)
                throw new ArgumentOutOfRangeException("input", "Input array should be 4 bytes at least");
            return ComputeAndWriteToEnd(input, 0, input.Length - 4);
        }

        /// <summary>
        /// Validates correctness of CRC-32 data in source buffer with assumption that CRC-32 data located at end of buffer in reverse bytes order. Can be used in conjunction with <see cref="ComputeAndWriteToEnd(byte[],int,int)"/>
        /// </summary>
        /// <param name="input">Input buffer with data to be checksummed.</param>
        /// <param name="offset">Offset of the input data within the buffer.</param>
        /// <param name="lengthWithCrc">Length of the input data in the buffer with CRC-32 bytes.</param>
        /// <returns>Is checksum valid.</returns>
        public static bool IsValidWithCrcAtEnd(byte[] input, int offset, int lengthWithCrc)
        {
            return Append(0, input, offset, lengthWithCrc) == 0x2144DF1C;
        }

        /// <summary>
        /// Validates correctness of CRC-32 data in source buffer with assumption that CRC-32 data located at end of buffer in reverse bytes order. Can be used in conjunction with <see cref="ComputeAndWriteToEnd(byte[],int,int)"/>
        /// </summary>
        /// <param name="input">Input buffer with data to be checksummed.</param>
        /// <returns>Is checksum valid.</returns>
        public static bool IsValidWithCrcAtEnd(byte[] input)
        {
            if (input.Length < 4)
                throw new ArgumentOutOfRangeException("input", "Input array should be 4 bytes at least");
            return Append(0, input, 0, input.Length) == 0x2144DF1C;
        }


        private static readonly SafeProxy _proxy = new SafeProxy();
        private static uint AppendInternal(uint initial, byte[] input, int offset, int length)
        {
            if (length > 0)
            {
                return _proxy.Append(initial, input, offset, length);
            }
            else
                return initial;
        }
    }
}
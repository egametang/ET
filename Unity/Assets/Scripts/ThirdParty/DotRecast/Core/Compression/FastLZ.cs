/*
 * Copyright 2014 The Netty Project
 *
 * The Netty Project licenses this file to you under the Apache License,
 * version 2.0 (the "License"); you may not use this file except in compliance
 * with the License. You may obtain a copy of the License at:
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations
 * under the License.
 */

using System;

namespace DotRecast.Core.Compression
{
    /**
     * Core of FastLZ compression algorithm.
     *
     * This class provides methods for compression and decompression of buffers and saves
     * constants which use by FastLzFrameEncoder and FastLzFrameDecoder.
     *
     * This is refactored code of <a href="https://code.google.com/p/jfastlz/">jfastlz</a>
     * library written by William Kinney.
     */
    public static class FastLZ
    {
        private const int MAX_DISTANCE = 8191;
        private const int MAX_FARDISTANCE = 65535 + MAX_DISTANCE - 1;

        private const int HASH_LOG = 13;
        private const int HASH_SIZE = 1 << HASH_LOG; // 8192
        private const int HASH_MASK = HASH_SIZE - 1;

        private const int MAX_COPY = 32;
        private const int MAX_LEN = 256 + 8;

        private const int MIN_RECOMENDED_LENGTH_FOR_LEVEL_2 = 1024 * 64;

        private const int MAGIC_NUMBER = 'F' << 16 | 'L' << 8 | 'Z';

        private const byte BLOCK_TYPE_NON_COMPRESSED = 0x00;
        private const byte BLOCK_TYPE_COMPRESSED = 0x01;
        private const byte BLOCK_WITHOUT_CHECKSUM = 0x00;
        private const byte BLOCK_WITH_CHECKSUM = 0x10;

        private const int OPTIONS_OFFSET = 3;
        private const int CHECKSUM_OFFSET = 4;

        private const int MAX_CHUNK_LENGTH = 0xFFFF;

        /**
     * Do not call {@link #Compress(byte[], int, int, byte[], int, int)} for input buffers
     * which length less than this value.
     */
        private const int MIN_LENGTH_TO_COMPRESSION = 32;

        /**
     * In this case {@link #Compress(byte[], int, int, byte[], int, int)} will choose level
     * automatically depending on the length of the input buffer. If length less than
     * {@link #MIN_RECOMENDED_LENGTH_FOR_LEVEL_2} {@link #LEVEL_1} will be choosen,
     * otherwise {@link #LEVEL_2}.
     */
        private const int LEVEL_AUTO = 0;

        /**
     * Level 1 is the fastest compression and generally useful for short data.
     */
        private const int LEVEL_1 = 1;

        /**
     * Level 2 is slightly slower but it gives better compression ratio.
     */
        private const int LEVEL_2 = 2;

        /**
     * The output buffer must be at least 6% larger than the input buffer and can not be smaller than 66 bytes.
     * @param inputLength length of input buffer
     * @return Maximum output buffer length
     */
        public static int CalculateOutputBufferLength(int inputLength)
        {
            int tempOutputLength = (int)(inputLength * 1.06);
            return Math.Max(tempOutputLength, 66);
        }

        /**
     * Compress a block of data in the input buffer and returns the size of compressed block.
     * The size of input buffer is specified by length. The minimum input buffer size is 32.
     *
     * If the input is not compressible, the return value might be larger than length (input buffer size).
     */
        public static int Compress(byte[] input, int inOffset, int inLength,
            byte[] output, int outOffset, int proposedLevel)
        {
            int level;
            if (proposedLevel == LEVEL_AUTO)
            {
                level = inLength < MIN_RECOMENDED_LENGTH_FOR_LEVEL_2 ? LEVEL_1 : LEVEL_2;
            }
            else
            {
                level = proposedLevel;
            }

            int ip = 0;
            int ipBound = ip + inLength - 2;
            int ipLimit = ip + inLength - 12;

            int op = 0;

            // const flzuint8* htab[HASH_SIZE];
            int[] htab = new int[HASH_SIZE];
            // const flzuint8** hslot;
            int hslot;
            // flzuint32 hval;
            // int OK b/c address starting from 0
            int hval;
            // flzuint32 copy;
            // int OK b/c address starting from 0
            int copy;

            /* sanity check */
            if (inLength < 4)
            {
                if (inLength != 0)
                {
                    // *op++ = length-1;
                    output[outOffset + op++] = (byte)(inLength - 1);
                    ipBound++;
                    while (ip <= ipBound)
                    {
                        output[outOffset + op++] = input[inOffset + ip++];
                    }

                    return inLength + 1;
                }

                // else
                return 0;
            }

            /* initializes hash table */
            //  for (hslot = htab; hslot < htab + HASH_SIZE; hslot++)
            for (hslot = 0; hslot < HASH_SIZE; hslot++)
            {
                //*hslot = ip;
                htab[hslot] = ip;
            }

            /* we start with literal copy */
            copy = 2;
            output[outOffset + op++] = (byte)(MAX_COPY - 1);
            output[outOffset + op++] = input[inOffset + ip++];
            output[outOffset + op++] = input[inOffset + ip++];

            /* main loop */
            while (ip < ipLimit)
            {
                int refs = 0;

                long distance = 0;

                /* minimum match length */
                // flzuint32 len = 3;
                // int OK b/c len is 0 and octal based
                int len = 3;

                /* comparison starting-point */
                int anchor = ip;

                bool matchLabel = false;

                /* check for a run */
                if (level == LEVEL_2)
                {
                    //If(ip[0] == ip[-1] && FASTLZ_READU16(ip-1)==FASTLZ_READU16(ip+1))
                    if (input[inOffset + ip] == input[inOffset + ip - 1] &&
                        ReadU16(input, inOffset + ip - 1) == ReadU16(input, inOffset + ip + 1))
                    {
                        distance = 1;
                        ip += 3;
                        refs = anchor - 1 + 3;

                        /*
                         * goto match;
                         */
                        matchLabel = true;
                    }
                }

                if (!matchLabel)
                {
                    /* find potential match */
                    // HASH_FUNCTION(hval,ip);
                    hval = HashFunction(input, inOffset + ip);
                    // hslot = htab + hval;
                    hslot = hval;
                    // refs = htab[hval];
                    refs = htab[hval];

                    /* calculate distance to the match */
                    distance = anchor - refs;

                    /* update hash table */
                    //*hslot = anchor;
                    htab[hslot] = anchor;

                    /* is this a match? check the first 3 bytes */
                    if (distance == 0
                        || (level == LEVEL_1 ? distance >= MAX_DISTANCE : distance >= MAX_FARDISTANCE)
                        || input[inOffset + refs++] != input[inOffset + ip++]
                        || input[inOffset + refs++] != input[inOffset + ip++]
                        || input[inOffset + refs++] != input[inOffset + ip++])
                    {
                        /*
                         * goto literal;
                         */
                        output[outOffset + op++] = input[inOffset + anchor++];
                        ip = anchor;
                        copy++;
                        if (copy == MAX_COPY)
                        {
                            copy = 0;
                            output[outOffset + op++] = (byte)(MAX_COPY - 1);
                        }

                        continue;
                    }

                    if (level == LEVEL_2)
                    {
                        /* far, needs at least 5-byte match */
                        if (distance >= MAX_DISTANCE)
                        {
                            if (input[inOffset + ip++] != input[inOffset + refs++]
                                || input[inOffset + ip++] != input[inOffset + refs++])
                            {
                                /*
                                 * goto literal;
                                 */
                                output[outOffset + op++] = input[inOffset + anchor++];
                                ip = anchor;
                                copy++;
                                if (copy == MAX_COPY)
                                {
                                    copy = 0;
                                    output[outOffset + op++] = (byte)(MAX_COPY - 1);
                                }

                                continue;
                            }

                            len += 2;
                        }
                    }
                } // end If(!matchLabel)

                /*
                 * match:
                 */
                /* last matched byte */
                ip = anchor + len;

                /* distance is biased */
                distance--;

                if (distance == 0)
                {
                    /* zero distance means a run */
                    //flzuint8 x = ip[-1];
                    byte x = input[inOffset + ip - 1];
                    while (ip < ipBound)
                    {
                        if (input[inOffset + refs++] != x)
                        {
                            break;
                        }
                        else
                        {
                            ip++;
                        }
                    }
                }
                else
                {
                    for (;;)
                    {
                        /* safe because the outer check against ip limit */
                        if (input[inOffset + refs++] != input[inOffset + ip++])
                        {
                            break;
                        }

                        if (input[inOffset + refs++] != input[inOffset + ip++])
                        {
                            break;
                        }

                        if (input[inOffset + refs++] != input[inOffset + ip++])
                        {
                            break;
                        }

                        if (input[inOffset + refs++] != input[inOffset + ip++])
                        {
                            break;
                        }

                        if (input[inOffset + refs++] != input[inOffset + ip++])
                        {
                            break;
                        }

                        if (input[inOffset + refs++] != input[inOffset + ip++])
                        {
                            break;
                        }

                        if (input[inOffset + refs++] != input[inOffset + ip++])
                        {
                            break;
                        }

                        if (input[inOffset + refs++] != input[inOffset + ip++])
                        {
                            break;
                        }

                        while (ip < ipBound)
                        {
                            if (input[inOffset + refs++] != input[inOffset + ip++])
                            {
                                break;
                            }
                        }

                        break;
                    }
                }

                /* if we have copied something, adjust the copy count */
                if (copy != 0)
                {
                    /* copy is biased, '0' means 1 byte copy */
                    // *(op-copy-1) = copy-1;
                    output[outOffset + op - copy - 1] = (byte)(copy - 1);
                }
                else
                {
                    /* back, to overwrite the copy count */
                    op--;
                }

                /* reset literal counter */
                copy = 0;

                /* length is biased, '1' means a match of 3 bytes */
                ip -= 3;
                len = ip - anchor;

                /* encode the match */
                if (level == LEVEL_2)
                {
                    if (distance < MAX_DISTANCE)
                    {
                        if (len < 7)
                        {
                            output[outOffset + op++] = (byte)((len << 5) + (int)((ulong)distance >> 8));
                            output[outOffset + op++] = (byte)(distance & 255);
                        }
                        else
                        {
                            output[outOffset + op++] = (byte)((7 << 5) + ((ulong)distance >> 8));
                            for (len -= 7; len >= 255; len -= 255)
                            {
                                output[outOffset + op++] = (byte)255;
                            }

                            output[outOffset + op++] = (byte)len;
                            output[outOffset + op++] = (byte)(distance & 255);
                        }
                    }
                    else
                    {
                        /* far away, but not yet in the another galaxy... */
                        if (len < 7)
                        {
                            distance -= MAX_DISTANCE;
                            output[outOffset + op++] = (byte)((len << 5) + 31);
                            output[outOffset + op++] = (byte)255;
                            output[outOffset + op++] = (byte)((ulong)distance >> 8);
                            output[outOffset + op++] = (byte)(distance & 255);
                        }
                        else
                        {
                            distance -= MAX_DISTANCE;
                            output[outOffset + op++] = (byte)((7 << 5) + 31);
                            for (len -= 7; len >= 255; len -= 255)
                            {
                                output[outOffset + op++] = (byte)255;
                            }

                            output[outOffset + op++] = (byte)len;
                            output[outOffset + op++] = (byte)255;
                            output[outOffset + op++] = (byte)((ulong)distance >> 8);
                            output[outOffset + op++] = (byte)(distance & 255);
                        }
                    }
                }
                else
                {
                    if (len > MAX_LEN - 2)
                    {
                        while (len > MAX_LEN - 2)
                        {
                            output[outOffset + op++] = (byte)((7 << 5) + ((ulong)distance >> 8));
                            output[outOffset + op++] = (byte)(MAX_LEN - 2 - 7 - 2);
                            output[outOffset + op++] = (byte)(distance & 255);
                            len -= MAX_LEN - 2;
                        }
                    }

                    if (len < 7)
                    {
                        output[outOffset + op++] = (byte)((len << 5) + (int)((ulong)distance >> 8));
                        output[outOffset + op++] = (byte)(distance & 255);
                    }
                    else
                    {
                        output[outOffset + op++] = (byte)((7 << 5) + (int)((ulong)distance >> 8));
                        output[outOffset + op++] = (byte)(len - 7);
                        output[outOffset + op++] = (byte)(distance & 255);
                    }
                }

                /* update the hash at match boundary */
                //HASH_FUNCTION(hval,ip);
                hval = HashFunction(input, inOffset + ip);
                htab[hval] = ip++;

                //HASH_FUNCTION(hval,ip);
                hval = HashFunction(input, inOffset + ip);
                htab[hval] = ip++;

                /* assuming literal copy */
                output[outOffset + op++] = (byte)(MAX_COPY - 1);

                continue;

                // Moved to be inline, with a 'continue'
                /*
                 * literal:
                 *
                  output[outOffset + op++] = input[inOffset + anchor++];
                  ip = anchor;
                  copy++;
                  If(copy == MAX_COPY){
                    copy = 0;
                    output[outOffset + op++] = MAX_COPY-1;
                  }
                */
            }

            /* left-over as literal copy */
            ipBound++;
            while (ip <= ipBound)
            {
                output[outOffset + op++] = input[inOffset + ip++];
                copy++;
                if (copy == MAX_COPY)
                {
                    copy = 0;
                    output[outOffset + op++] = (byte)(MAX_COPY - 1);
                }
            }

            /* if we have copied something, adjust the copy length */
            if (copy != 0)
            {
                //*(op-copy-1) = copy-1;
                output[outOffset + op - copy - 1] = (byte)(copy - 1);
            }
            else
            {
                op--;
            }

            if (level == LEVEL_2)
            {
                /* marker for fastlz2 */
                output[outOffset] |= 1 << 5;
            }

            return op;
        }

        /**
     * Decompress a block of compressed data and returns the size of the decompressed block.
     * If error occurs, e.g. the compressed data is corrupted or the output buffer is not large
     * enough, then 0 (zero) will be returned instead.
     *
     * Decompression is memory safe and guaranteed not to write the output buffer
     * more than what is specified in outLength.
     */
        public static int Decompress(byte[] input, int inOffset, int inLength,
            byte[] output, int outOffset, int outLength)
        {
            //int level = ((*(const flzuint8*)input) >> 5) + 1;
            int level = (input[inOffset] >> 5) + 1;
            if (level != LEVEL_1 && level != LEVEL_2)
            {
                throw new Exception($"invalid level: {level} (expected: {LEVEL_1} or {LEVEL_2})");
            }

            // const flzuint8* ip = (const flzuint8*) input;
            int ip = 0;
            // flzuint8* op = (flzuint8*) output;
            int op = 0;
            // flzuint32 ctrl = (*ip++) & 31;
            long ctrl = input[inOffset + ip++] & 31;

            int loop = 1;
            do
            {
                //  const flzuint8* refs = op;
                int refs = op;
                // flzuint32 len = ctrl >> 5;
                long len = ctrl >> 5;
                // flzuint32 ofs = (ctrl & 31) << 8;
                long ofs = (ctrl & 31) << 8;

                if (ctrl >= 32)
                {
                    len--;
                    // refs -= ofs;
                    refs -= (int)ofs;

                    int code;
                    if (len == 6)
                    {
                        if (level == LEVEL_1)
                        {
                            // len += *ip++;
                            len += input[inOffset + ip++] & 0xFF;
                        }
                        else
                        {
                            do
                            {
                                code = input[inOffset + ip++] & 0xFF;
                                len += code;
                            } while (code == 255);
                        }
                    }

                    if (level == LEVEL_1)
                    {
                        //  refs -= *ip++;
                        refs -= input[inOffset + ip++] & 0xFF;
                    }
                    else
                    {
                        code = input[inOffset + ip++] & 0xFF;
                        refs -= code;

                        /* match from 16-bit distance */
                        // If(FASTLZ_UNEXPECT_CONDITIONAL(code==255))
                        // If(FASTLZ_EXPECT_CONDITIONAL(ofs==(31 << 8)))
                        if (code == 255 && ofs == 31 << 8)
                        {
                            ofs = (input[inOffset + ip++] & 0xFF) << 8;
                            ofs += input[inOffset + ip++] & 0xFF;

                            refs = (int)(op - ofs - MAX_DISTANCE);
                        }
                    }

                    // if the output index + length of Block(?) + 3(?) is over the output limit?
                    if (op + len + 3 > outLength)
                    {
                        return 0;
                    }

                    // if (FASTLZ_UNEXPECT_CONDITIONAL(refs-1 < (flzuint8 *)output))
                    // if the address space of refs-1 is < the address of output?
                    // if we are still at the beginning of the output address?
                    if (refs - 1 < 0)
                    {
                        return 0;
                    }

                    if (ip < inLength)
                    {
                        ctrl = input[inOffset + ip++] & 0xFF;
                    }
                    else
                    {
                        loop = 0;
                    }

                    if (refs == op)
                    {
                        /* optimize copy for a run */
                        // flzuint8 b = refs[-1];
                        byte b = output[outOffset + refs - 1];
                        output[outOffset + op++] = b;
                        output[outOffset + op++] = b;
                        output[outOffset + op++] = b;
                        while (len != 0)
                        {
                            output[outOffset + op++] = b;
                            --len;
                        }
                    }
                    else
                    {
                        /* copy from reference */
                        refs--;

                        // *op++ = *refs++;
                        output[outOffset + op++] = output[outOffset + refs++];
                        output[outOffset + op++] = output[outOffset + refs++];
                        output[outOffset + op++] = output[outOffset + refs++];

                        while (len != 0)
                        {
                            output[outOffset + op++] = output[outOffset + refs++];
                            --len;
                        }
                    }
                }
                else
                {
                    ctrl++;

                    if (op + ctrl > outLength)
                    {
                        return 0;
                    }

                    if (ip + ctrl > inLength)
                    {
                        return 0;
                    }

                    //*op++ = *ip++;
                    output[outOffset + op++] = input[inOffset + ip++];

                    for (--ctrl; ctrl != 0; ctrl--)
                    {
                        // *op++ = *ip++;
                        output[outOffset + op++] = input[inOffset + ip++];
                    }

                    loop = ip < inLength ? 1 : 0;
                    if (loop != 0)
                    {
                        //  ctrl = *ip++;
                        ctrl = input[inOffset + ip++] & 0xFF;
                    }
                }

                // While(FASTLZ_EXPECT_CONDITIONAL(loop));
            } while (loop != 0);

            //  return op - (flzuint8*)output;
            return op;
        }

        private static int HashFunction(byte[] p, int offset)
        {
            int v = ReadU16(p, offset);
            v ^= ReadU16(p, offset + 1) ^ v >> 16 - HASH_LOG;
            v &= HASH_MASK;
            return v;
        }

        private static int ReadU16(byte[] data, int offset)
        {
            if (offset + 1 >= data.Length)
            {
                return data[offset] & 0xff;
            }

            return (data[offset + 1] & 0xff) << 8 | data[offset] & 0xff;
        }
    }
}
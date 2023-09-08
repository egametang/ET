/*
Copyright (c) 2009-2010 Mikko Mononen memon@inside.org
recast4j copyright (c) 2015-2019 Piotr Piastucki piotr@jtilia.org
DotRecast Copyright (c) 2023 Choi Ikpil ikpil@naver.com

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

using DotRecast.Core;
using DotRecast.Core.Compression;

namespace DotRecast.Detour.TileCache.Io.Compress
{
    public class DtTileCacheFastLzCompressor : IRcCompressor
    {
        public static readonly DtTileCacheFastLzCompressor Shared = new DtTileCacheFastLzCompressor();

        private DtTileCacheFastLzCompressor()
        {
        }

        public byte[] Decompress(byte[] buf)
        {
            return Decompress(buf, 0, buf.Length, buf.Length * 3);
        }

        public byte[] Decompress(byte[] buf, int offset, int len, int outputlen)
        {
            byte[] output = new byte[outputlen];
            FastLZ.Decompress(buf, offset, len, output, 0, outputlen);
            return output;
        }

        public byte[] Compress(byte[] buf)
        {
            byte[] output = new byte[FastLZ.CalculateOutputBufferLength(buf.Length)];
            int len = FastLZ.Compress(buf, 0, buf.Length, output, 0, output.Length);
            return RcArrayUtils.CopyOf(output, len);
        }
    }
}
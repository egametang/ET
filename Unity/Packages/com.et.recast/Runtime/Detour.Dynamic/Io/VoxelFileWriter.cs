/*
recast4j copyright (c) 2021 Piotr Piastucki piotr@jtilia.org
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

using System.IO;
using DotRecast.Core;
using DotRecast.Detour.Io;

namespace DotRecast.Detour.Dynamic.Io
{
    public class VoxelFileWriter : DtWriter
    {
        private readonly IRcCompressor _compressor;

        public VoxelFileWriter(IRcCompressor compressor)
        {
            _compressor = compressor;
        }

        public void Write(BinaryWriter stream, VoxelFile f, bool compression)
        {
            Write(stream, f, VoxelFile.PREFERRED_BYTE_ORDER, compression);
        }

        public void Write(BinaryWriter stream, VoxelFile f, RcByteOrder byteOrder, bool compression)
        {
            Write(stream, VoxelFile.MAGIC, byteOrder);
            Write(stream, VoxelFile.VERSION_EXPORTER_RECAST4J | (compression ? VoxelFile.VERSION_COMPRESSION_LZ4 : 0), byteOrder);
            Write(stream, f.walkableRadius, byteOrder);
            Write(stream, f.walkableHeight, byteOrder);
            Write(stream, f.walkableClimb, byteOrder);
            Write(stream, f.walkableSlopeAngle, byteOrder);
            Write(stream, f.cellSize, byteOrder);
            Write(stream, f.maxSimplificationError, byteOrder);
            Write(stream, f.maxEdgeLen, byteOrder);
            Write(stream, f.minRegionArea, byteOrder);
            Write(stream, f.regionMergeArea, byteOrder);
            Write(stream, f.vertsPerPoly, byteOrder);
            Write(stream, f.buildMeshDetail);
            Write(stream, f.detailSampleDistance, byteOrder);
            Write(stream, f.detailSampleMaxError, byteOrder);
            Write(stream, f.useTiles);
            Write(stream, f.tileSizeX, byteOrder);
            Write(stream, f.tileSizeZ, byteOrder);
            Write(stream, f.rotation.x, byteOrder);
            Write(stream, f.rotation.y, byteOrder);
            Write(stream, f.rotation.z, byteOrder);
            Write(stream, f.bounds[0], byteOrder);
            Write(stream, f.bounds[1], byteOrder);
            Write(stream, f.bounds[2], byteOrder);
            Write(stream, f.bounds[3], byteOrder);
            Write(stream, f.bounds[4], byteOrder);
            Write(stream, f.bounds[5], byteOrder);
            Write(stream, f.tiles.Count, byteOrder);
            foreach (VoxelTile t in f.tiles)
            {
                WriteTile(stream, t, byteOrder, compression);
            }
        }

        public void WriteTile(BinaryWriter stream, VoxelTile tile, RcByteOrder byteOrder, bool compression)
        {
            Write(stream, tile.tileX, byteOrder);
            Write(stream, tile.tileZ, byteOrder);
            Write(stream, tile.width, byteOrder);
            Write(stream, tile.depth, byteOrder);
            Write(stream, tile.borderSize, byteOrder);
            Write(stream, tile.boundsMin.x, byteOrder);
            Write(stream, tile.boundsMin.y, byteOrder);
            Write(stream, tile.boundsMin.z, byteOrder);
            Write(stream, tile.boundsMax.x, byteOrder);
            Write(stream, tile.boundsMax.y, byteOrder);
            Write(stream, tile.boundsMax.z, byteOrder);
            Write(stream, tile.cellSize, byteOrder);
            Write(stream, tile.cellHeight, byteOrder);
            byte[] bytes = tile.spanData;
            if (compression)
            {
                bytes = _compressor.Compress(bytes);
            }

            Write(stream, bytes.Length, byteOrder);
            stream.Write(bytes);
        }
    }
}
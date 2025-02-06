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

using System.IO;
using DotRecast.Core;
using DotRecast.Detour.Io;
using DotRecast.Detour.TileCache.Io.Compress;

namespace DotRecast.Detour.TileCache.Io
{
    public class DtTileCacheWriter : DtWriter
    {
        private readonly DtNavMeshParamWriter paramWriter = new DtNavMeshParamWriter();
        private readonly DtTileCacheBuilder builder = new DtTileCacheBuilder();
        private readonly IDtTileCacheCompressorFactory _compFactory;
        
        public DtTileCacheWriter(IDtTileCacheCompressorFactory compFactory)
        {
            _compFactory = compFactory;
        }


        public void Write(BinaryWriter stream, DtTileCache cache, RcByteOrder order, bool cCompatibility)
        {
            Write(stream, DtTileCacheSetHeader.TILECACHESET_MAGIC, order);
            Write(stream, cCompatibility
                ? DtTileCacheSetHeader.TILECACHESET_VERSION
                : DtTileCacheSetHeader.TILECACHESET_VERSION_RECAST4J, order);
            int numTiles = 0;
            for (int i = 0; i < cache.GetTileCount(); ++i)
            {
                DtCompressedTile tile = cache.GetTile(i);
                if (tile == null || tile.data == null)
                    continue;
                numTiles++;
            }

            Write(stream, numTiles, order);
            paramWriter.Write(stream, cache.GetNavMesh().GetParams(), order);
            WriteCacheParams(stream, cache.GetParams(), order);
            for (int i = 0; i < cache.GetTileCount(); i++)
            {
                DtCompressedTile tile = cache.GetTile(i);
                if (tile == null || tile.data == null)
                    continue;
                Write(stream, (int)cache.GetTileRef(tile), order);
                byte[] data = tile.data;
                DtTileCacheLayer layer = cache.DecompressTile(tile);
                var comp = _compFactory.Create(cCompatibility ? 0 : 1);
                data = builder.CompressTileCacheLayer(comp, layer, order, cCompatibility);
                Write(stream, data.Length, order);
                stream.Write(data);
            }
        }

        private void WriteCacheParams(BinaryWriter stream, DtTileCacheParams option, RcByteOrder order)
        {
            Write(stream, option.orig.x, order);
            Write(stream, option.orig.y, order);
            Write(stream, option.orig.z, order);

            Write(stream, option.cs, order);
            Write(stream, option.ch, order);
            Write(stream, option.width, order);
            Write(stream, option.height, order);
            Write(stream, option.walkableHeight, order);
            Write(stream, option.walkableRadius, order);
            Write(stream, option.walkableClimb, order);
            Write(stream, option.maxSimplificationError, order);
            Write(stream, option.maxTiles, order);
            Write(stream, option.maxObstacles, order);
        }
    }
}
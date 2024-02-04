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

namespace DotRecast.Detour.TileCache.Io
{
    public class DtTileCacheLayerHeaderReader
    {
        public DtTileCacheLayerHeader Read(RcByteBuffer data, bool cCompatibility)
        {
            DtTileCacheLayerHeader header = new DtTileCacheLayerHeader();
            header.magic = data.GetInt();
            header.version = data.GetInt();

            if (header.magic != DtTileCacheLayerHeader.DT_TILECACHE_MAGIC)
                throw new IOException("Invalid magic");
            if (header.version != DtTileCacheLayerHeader.DT_TILECACHE_VERSION)
                throw new IOException("Invalid version");

            header.tx = data.GetInt();
            header.ty = data.GetInt();
            header.tlayer = data.GetInt();
            
            header.bmin.x = data.GetFloat();
            header.bmin.y = data.GetFloat();
            header.bmin.z = data.GetFloat();
            header.bmax.x = data.GetFloat();
            header.bmax.y = data.GetFloat();
            header.bmax.z = data.GetFloat();

            header.hmin = data.GetShort() & 0xFFFF;
            header.hmax = data.GetShort() & 0xFFFF;
            header.width = data.Get() & 0xFF;
            header.height = data.Get() & 0xFF;
            header.minx = data.Get() & 0xFF;
            header.maxx = data.Get() & 0xFF;
            header.miny = data.Get() & 0xFF;
            header.maxy = data.Get() & 0xFF;
            if (cCompatibility)
            {
                data.GetShort(); // C struct padding
            }

            return header;
        }
    }
}
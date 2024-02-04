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
    public class DtMeshSetReader
    {
        private readonly DtMeshDataReader meshReader = new DtMeshDataReader();
        private readonly DtNavMeshParamsReader paramReader = new DtNavMeshParamsReader();

        public DtNavMesh Read(BinaryReader @is, int maxVertPerPoly)
        {
            return Read(IOUtils.ToByteBuffer(@is), maxVertPerPoly, false);
        }

        public DtNavMesh Read(RcByteBuffer bb, int maxVertPerPoly)
        {
            return Read(bb, maxVertPerPoly, false);
        }

        public DtNavMesh Read32Bit(BinaryReader @is, int maxVertPerPoly)
        {
            return Read(IOUtils.ToByteBuffer(@is), maxVertPerPoly, true);
        }

        public DtNavMesh Read32Bit(RcByteBuffer bb, int maxVertPerPoly)
        {
            return Read(bb, maxVertPerPoly, true);
        }

        public DtNavMesh Read(BinaryReader @is)
        {
            return Read(IOUtils.ToByteBuffer(@is));
        }

        public DtNavMesh Read(RcByteBuffer bb)
        {
            return Read(bb, -1, false);
        }

        DtNavMesh Read(RcByteBuffer bb, int maxVertPerPoly, bool is32Bit)
        {
            NavMeshSetHeader header = ReadHeader(bb, maxVertPerPoly);
            if (header.maxVertsPerPoly <= 0)
            {
                throw new IOException("Invalid number of verts per poly " + header.maxVertsPerPoly);
            }

            bool cCompatibility = header.version == NavMeshSetHeader.NAVMESHSET_VERSION;
            DtNavMesh mesh = new DtNavMesh(header.option, header.maxVertsPerPoly);
            ReadTiles(bb, is32Bit, header, cCompatibility, mesh);
            return mesh;
        }

        private NavMeshSetHeader ReadHeader(RcByteBuffer bb, int maxVertsPerPoly)
        {
            NavMeshSetHeader header = new NavMeshSetHeader();
            header.magic = bb.GetInt();
            if (header.magic != NavMeshSetHeader.NAVMESHSET_MAGIC)
            {
                header.magic = IOUtils.SwapEndianness(header.magic);
                if (header.magic != NavMeshSetHeader.NAVMESHSET_MAGIC)
                {
                    throw new IOException("Invalid magic " + header.magic);
                }

                bb.Order(bb.Order() == RcByteOrder.BIG_ENDIAN ? RcByteOrder.LITTLE_ENDIAN : RcByteOrder.BIG_ENDIAN);
            }

            header.version = bb.GetInt();
            if (header.version != NavMeshSetHeader.NAVMESHSET_VERSION && header.version != NavMeshSetHeader.NAVMESHSET_VERSION_RECAST4J_1
                                                                      && header.version != NavMeshSetHeader.NAVMESHSET_VERSION_RECAST4J)
            {
                throw new IOException("Invalid version " + header.version);
            }

            header.numTiles = bb.GetInt();
            header.option = paramReader.Read(bb);
            header.maxVertsPerPoly = maxVertsPerPoly;
            if (header.version == NavMeshSetHeader.NAVMESHSET_VERSION_RECAST4J)
            {
                header.maxVertsPerPoly = bb.GetInt();
            }

            return header;
        }

        private void ReadTiles(RcByteBuffer bb, bool is32Bit, NavMeshSetHeader header, bool cCompatibility, DtNavMesh mesh)
        {
            // Read tiles.
            for (int i = 0; i < header.numTiles; ++i)
            {
                NavMeshTileHeader tileHeader = new NavMeshTileHeader();
                if (is32Bit)
                {
                    tileHeader.tileRef = Convert32BitRef(bb.GetInt(), header.option);
                }
                else
                {
                    tileHeader.tileRef = bb.GetLong();
                }

                tileHeader.dataSize = bb.GetInt();
                if (tileHeader.tileRef == 0 || tileHeader.dataSize == 0)
                {
                    break;
                }

                if (cCompatibility && !is32Bit)
                {
                    bb.GetInt(); // C struct padding
                }

                DtMeshData data = meshReader.Read(bb, mesh.GetMaxVertsPerPoly(), is32Bit);
                mesh.AddTile(data, i, tileHeader.tileRef);
            }
        }

        private long Convert32BitRef(int refs, DtNavMeshParams option)
        {
            int m_tileBits = DtUtils.Ilog2(DtUtils.NextPow2(option.maxTiles));
            int m_polyBits = DtUtils.Ilog2(DtUtils.NextPow2(option.maxPolys));
            // Only allow 31 salt bits, since the salt mask is calculated using 32bit uint and it will overflow.
            int m_saltBits = Math.Min(31, 32 - m_tileBits - m_polyBits);
            int saltMask = (1 << m_saltBits) - 1;
            int tileMask = (1 << m_tileBits) - 1;
            int polyMask = (1 << m_polyBits) - 1;
            int salt = ((refs >> (m_polyBits + m_tileBits)) & saltMask);
            int it = ((refs >> m_polyBits) & tileMask);
            int ip = refs & polyMask;
            return DtNavMesh.EncodePolyId(salt, it, ip);
        }
    }
}
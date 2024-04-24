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

using System.IO;
using DotRecast.Core;

namespace DotRecast.Detour.Io
{
    public class DtMeshDataWriter : DtWriter
    {
        public void Write(BinaryWriter stream, DtMeshData data, RcByteOrder order, bool cCompatibility)
        {
            DtMeshHeader header = data.header;
            Write(stream, header.magic, order);
            Write(stream, cCompatibility ? DtMeshHeader.DT_NAVMESH_VERSION : DtMeshHeader.DT_NAVMESH_VERSION_RECAST4J_LAST, order);
            Write(stream, header.x, order);
            Write(stream, header.y, order);
            Write(stream, header.layer, order);
            Write(stream, header.userId, order);
            Write(stream, header.polyCount, order);
            Write(stream, header.vertCount, order);
            Write(stream, header.maxLinkCount, order);
            Write(stream, header.detailMeshCount, order);
            Write(stream, header.detailVertCount, order);
            Write(stream, header.detailTriCount, order);
            Write(stream, header.bvNodeCount, order);
            Write(stream, header.offMeshConCount, order);
            Write(stream, header.offMeshBase, order);
            Write(stream, header.walkableHeight, order);
            Write(stream, header.walkableRadius, order);
            Write(stream, header.walkableClimb, order);
            Write(stream, header.bmin.x, order);
            Write(stream, header.bmin.y, order);
            Write(stream, header.bmin.z, order);
            Write(stream, header.bmax.x, order);
            Write(stream, header.bmax.y, order);
            Write(stream, header.bmax.z, order);
            Write(stream, header.bvQuantFactor, order);
            WriteVerts(stream, data.verts, header.vertCount, order);
            WritePolys(stream, data, order, cCompatibility);
            if (cCompatibility)
            {
                byte[] linkPlaceholder = new byte[header.maxLinkCount * DtMeshDataReader.GetSizeofLink(false)];
                stream.Write(linkPlaceholder);
            }

            WritePolyDetails(stream, data, order, cCompatibility);
            WriteVerts(stream, data.detailVerts, header.detailVertCount, order);
            WriteDTris(stream, data);
            WriteBVTree(stream, data, order, cCompatibility);
            WriteOffMeshCons(stream, data, order);
        }

        private void WriteVerts(BinaryWriter stream, float[] verts, int count, RcByteOrder order)
        {
            for (int i = 0; i < count * 3; i++)
            {
                Write(stream, verts[i], order);
            }
        }

        private void WritePolys(BinaryWriter stream, DtMeshData data, RcByteOrder order, bool cCompatibility)
        {
            for (int i = 0; i < data.header.polyCount; i++)
            {
                if (cCompatibility)
                {
                    Write(stream, 0xFFFF, order);
                }

                for (int j = 0; j < data.polys[i].verts.Length; j++)
                {
                    Write(stream, (short)data.polys[i].verts[j], order);
                }

                for (int j = 0; j < data.polys[i].neis.Length; j++)
                {
                    Write(stream, (short)data.polys[i].neis[j], order);
                }

                Write(stream, (short)data.polys[i].flags, order);
                Write(stream, (byte)data.polys[i].vertCount);
                Write(stream, (byte)data.polys[i].areaAndtype);
            }
        }

        private void WritePolyDetails(BinaryWriter stream, DtMeshData data, RcByteOrder order, bool cCompatibility)
        {
            for (int i = 0; i < data.header.detailMeshCount; i++)
            {
                Write(stream, data.detailMeshes[i].vertBase, order);
                Write(stream, data.detailMeshes[i].triBase, order);
                Write(stream, (byte)data.detailMeshes[i].vertCount);
                Write(stream, (byte)data.detailMeshes[i].triCount);
                if (cCompatibility)
                {
                    Write(stream, (short)0, order);
                }
            }
        }

        private void WriteDTris(BinaryWriter stream, DtMeshData data)
        {
            for (int i = 0; i < data.header.detailTriCount * 4; i++)
            {
                Write(stream, (byte)data.detailTris[i]);
            }
        }

        private void WriteBVTree(BinaryWriter stream, DtMeshData data, RcByteOrder order, bool cCompatibility)
        {
            for (int i = 0; i < data.header.bvNodeCount; i++)
            {
                if (cCompatibility)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        Write(stream, (short)data.bvTree[i].bmin[j], order);
                    }

                    for (int j = 0; j < 3; j++)
                    {
                        Write(stream, (short)data.bvTree[i].bmax[j], order);
                    }
                }
                else
                {
                    for (int j = 0; j < 3; j++)
                    {
                        Write(stream, data.bvTree[i].bmin[j], order);
                    }

                    for (int j = 0; j < 3; j++)
                    {
                        Write(stream, data.bvTree[i].bmax[j], order);
                    }
                }

                Write(stream, data.bvTree[i].i, order);
            }
        }

        private void WriteOffMeshCons(BinaryWriter stream, DtMeshData data, RcByteOrder order)
        {
            for (int i = 0; i < data.header.offMeshConCount; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    Write(stream, data.offMeshCons[i].pos[j], order);
                }

                Write(stream, data.offMeshCons[i].rad, order);
                Write(stream, (short)data.offMeshCons[i].poly, order);
                Write(stream, (byte)data.offMeshCons[i].flags);
                Write(stream, (byte)data.offMeshCons[i].side);
                Write(stream, data.offMeshCons[i].userId, order);
            }
        }
    }
}
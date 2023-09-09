/*
recast4j copyright (c) 2015-2019 Piotr Piastucki piotr@jtilia.org

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

namespace DotRecast.Detour.Extras
{
    public class ObjExporter
    {
        public void Export(DtNavMesh mesh)
        {
            string filename = Path.Combine(Directory.GetCurrentDirectory(), "Demo", "astar.obj");
            using var fs = new FileStream(filename, FileMode.CreateNew);
            using var fw = new StreamWriter(fs);
            for (int i = 0; i < mesh.GetTileCount(); i++)
            {
                DtMeshTile tile = mesh.GetTile(i);
                if (tile != null)
                {
                    for (int v = 0; v < tile.data.header.vertCount; v++)
                    {
                        fw.Write("v " + tile.data.verts[v * 3] + " " + tile.data.verts[v * 3 + 1] + " "
                                 + tile.data.verts[v * 3 + 2] + "\n");
                    }
                }
            }

            int vertexOffset = 1;
            for (int i = 0; i < mesh.GetTileCount(); i++)
            {
                DtMeshTile tile = mesh.GetTile(i);
                if (tile != null)
                {
                    for (int p = 0; p < tile.data.header.polyCount; p++)
                    {
                        fw.Write("f ");
                        DtPoly poly = tile.data.polys[p];
                        for (int v = 0; v < poly.vertCount; v++)
                        {
                            fw.Write(poly.verts[v] + vertexOffset + " ");
                        }

                        fw.Write("\n");
                    }

                    vertexOffset += tile.data.header.vertCount;
                }
            }
        }

        /*
         *
            MeshSetReader reader = new MeshSetReader();
            ObjExporter exporter = new ObjExporter();
            exporter.Export(mesh);
            reader.Read(new FileInputStream("/home/piotr/Downloads/graph/all_tiles_navmesh.bin"), 3);
    
    
         */
    }
}
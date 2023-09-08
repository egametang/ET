using System.Collections.Generic;
using DotRecast.Core;
using DotRecast.Recast;
using static DotRecast.Recast.RcConstants;


namespace DotRecast.Detour.Extras.Jumplink
{
    public class EdgeExtractor
    {
        public JumpEdge[] ExtractEdges(RcPolyMesh mesh)
        {
            List<JumpEdge> edges = new List<JumpEdge>();
            if (mesh != null)
            {
                RcVec3f orig = mesh.bmin;
                float cs = mesh.cs;
                float ch = mesh.ch;
                for (int i = 0; i < mesh.npolys; i++)
                {
                    if (i > 41 || i < 41)
                    {
                        //           continue;
                    }

                    int nvp = mesh.nvp;
                    int p = i * 2 * nvp;
                    for (int j = 0; j < nvp; ++j)
                    {
                        if (j != 1)
                        {
                            //                    continue;
                        }

                        if (mesh.polys[p + j] == RC_MESH_NULL_IDX)
                        {
                            break;
                        }

                        // Skip connected edges.
                        if ((mesh.polys[p + nvp + j] & 0x8000) != 0)
                        {
                            int dir = mesh.polys[p + nvp + j] & 0xf;
                            if (dir == 0xf)
                            {
                                // Border
                                if (mesh.polys[p + nvp + j] != RC_MESH_NULL_IDX)
                                {
                                    continue;
                                }

                                int nj = j + 1;
                                if (nj >= nvp || mesh.polys[p + nj] == RC_MESH_NULL_IDX)
                                {
                                    nj = 0;
                                }

                                int va = mesh.polys[p + j] * 3;
                                int vb = mesh.polys[p + nj] * 3;
                                JumpEdge e = new JumpEdge();
                                e.sp.x = orig.x + mesh.verts[vb] * cs;
                                e.sp.y = orig.y + mesh.verts[vb + 1] * ch;
                                e.sp.z = orig.z + mesh.verts[vb + 2] * cs;
                                e.sq.x = orig.x + mesh.verts[va] * cs;
                                e.sq.y = orig.y + mesh.verts[va + 1] * ch;
                                e.sq.z = orig.z + mesh.verts[va + 2] * cs;
                                edges.Add(e);
                            }
                        }
                    }
                }
            }

            return edges.ToArray();
        }
    }
}
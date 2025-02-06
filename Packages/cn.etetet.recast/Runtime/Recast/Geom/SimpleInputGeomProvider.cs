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

using System;
using System.Collections.Generic;
using DotRecast.Core;

namespace DotRecast.Recast.Geom
{
    public class SimpleInputGeomProvider : IInputGeomProvider
    {
        public readonly float[] vertices;
        public readonly int[] faces;
        public readonly float[] normals;
        private RcVec3f bmin;
        private RcVec3f bmax;

        private readonly List<RcConvexVolume> volumes = new List<RcConvexVolume>();
        private readonly RcTriMesh _mesh;

        public SimpleInputGeomProvider(List<float> vertexPositions, List<int> meshFaces)
            : this(MapVertices(vertexPositions), MapFaces(meshFaces))
        {
        }

        private static int[] MapFaces(List<int> meshFaces)
        {
            int[] faces = new int[meshFaces.Count];
            for (int i = 0; i < faces.Length; i++)
            {
                faces[i] = meshFaces[i];
            }

            return faces;
        }

        private static float[] MapVertices(List<float> vertexPositions)
        {
            float[] vertices = new float[vertexPositions.Count];
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = vertexPositions[i];
            }

            return vertices;
        }

        public SimpleInputGeomProvider(float[] vertices, int[] faces)
        {
            this.vertices = vertices;
            this.faces = faces;
            normals = new float[faces.Length];
            CalculateNormals();
            bmin = RcVec3f.Zero;
            bmax = RcVec3f.Zero;
            RcVec3f.Copy(ref bmin, vertices, 0);
            RcVec3f.Copy(ref bmax, vertices, 0);
            for (int i = 1; i < vertices.Length / 3; i++)
            {
                bmin.Min(vertices, i * 3);
                bmax.Max(vertices, i * 3);
            }

            _mesh = new RcTriMesh(vertices, faces);
        }

        public RcTriMesh GetMesh()
        {
            return _mesh;
        }

        public RcVec3f GetMeshBoundsMin()
        {
            return bmin;
        }

        public RcVec3f GetMeshBoundsMax()
        {
            return bmax;
        }

        public IList<RcConvexVolume> ConvexVolumes()
        {
            return volumes;
        }

        public void AddConvexVolume(float[] verts, float minh, float maxh, RcAreaModification areaMod)
        {
            RcConvexVolume vol = new RcConvexVolume();
            vol.hmin = minh;
            vol.hmax = maxh;
            vol.verts = verts;
            vol.areaMod = areaMod;
        }

        public void AddConvexVolume(RcConvexVolume convexVolume)
        {
            volumes.Add(convexVolume);
        }

        public IEnumerable<RcTriMesh> Meshes()
        {
            return RcImmutableArray.Create(_mesh);
        }

        public List<RcOffMeshConnection> GetOffMeshConnections()
        {
            throw new NotImplementedException();
        }

        public void AddOffMeshConnection(RcVec3f start, RcVec3f end, float radius, bool bidir, int area, int flags)
        {
            throw new NotImplementedException();
        }

        public void RemoveOffMeshConnections(Predicate<RcOffMeshConnection> filter)
        {
            throw new NotImplementedException();
        }

        public void CalculateNormals()
        {
            for (int i = 0; i < faces.Length; i += 3)
            {
                int v0 = faces[i] * 3;
                int v1 = faces[i + 1] * 3;
                int v2 = faces[i + 2] * 3;

                var e0 = new RcVec3f();
                var e1 = new RcVec3f();
                e0.x = vertices[v1 + 0] - vertices[v0 + 0];
                e0.y = vertices[v1 + 1] - vertices[v0 + 1];
                e0.z = vertices[v1 + 2] - vertices[v0 + 2];

                e1.x = vertices[v2 + 0] - vertices[v0 + 0];
                e1.y = vertices[v2 + 1] - vertices[v0 + 1];
                e1.z = vertices[v2 + 2] - vertices[v0 + 2];

                normals[i] = e0.y * e1.z - e0.z * e1.y;
                normals[i + 1] = e0.z * e1.x - e0.x * e1.z;
                normals[i + 2] = e0.x * e1.y - e0.y * e1.x;
                float d = (float)Math.Sqrt(normals[i] * normals[i] + normals[i + 1] * normals[i + 1] + normals[i + 2] * normals[i + 2]);
                if (d > 0)
                {
                    d = 1.0f / d;
                    normals[i] *= d;
                    normals[i + 1] *= d;
                    normals[i + 2] *= d;
                }
            }
        }
    }
}
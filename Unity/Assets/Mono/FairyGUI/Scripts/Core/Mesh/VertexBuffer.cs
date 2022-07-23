using System.Collections.Generic;
using UnityEngine;

namespace FairyGUI
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class VertexBuffer
    {
        /// <summary>
        /// 
        /// </summary>
        public Rect contentRect;

        /// <summary>
        /// 
        /// </summary>
        public Rect uvRect;

        /// <summary>
        /// 
        /// </summary>
        public Color32 vertexColor;

        /// <summary>
        /// 
        /// </summary>
        public Vector2 textureSize;

        /// <summary>
        /// 
        /// </summary>
        public readonly List<Vector3> vertices;

        /// <summary>
        /// 
        /// </summary>
        public readonly List<Color32> colors;

        /// <summary>
        /// 
        /// </summary>
        public readonly List<Vector2> uvs;

        /// <summary>
        /// 
        /// </summary>
        public readonly List<Vector2> uvs2;

        /// <summary>
        /// 
        /// </summary>
        public readonly List<int> triangles;

        static public Vector2[] NormalizedUV = new Vector2[] {
            new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0) };

        static public Vector2[] NormalizedPosition = new Vector2[] {
            new Vector2(0, 1), new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1) };

        internal bool _alphaInVertexColor;
        internal bool _isArbitraryQuad;

        static Stack<VertexBuffer> _pool = new Stack<VertexBuffer>();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static VertexBuffer Begin()
        {
            if (_pool.Count > 0)
            {
                VertexBuffer inst = _pool.Pop();
                inst.Clear();
                return inst;
            }
            else
                return new VertexBuffer();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        public static VertexBuffer Begin(VertexBuffer source)
        {
            VertexBuffer vb = Begin();
            vb.contentRect = source.contentRect;
            vb.uvRect = source.uvRect;
            vb.vertexColor = source.vertexColor;
            vb.textureSize = source.textureSize;

            return vb;
        }

        private VertexBuffer()
        {
            vertices = new List<Vector3>();
            colors = new List<Color32>();
            uvs = new List<Vector2>();
            uvs2 = new List<Vector2>();
            triangles = new List<int>();
        }

        /// <summary>
        /// 
        /// </summary>
        public void End()
        {
            _pool.Push(this);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Clear()
        {
            vertices.Clear();
            colors.Clear();
            uvs.Clear();
            uvs2.Clear();
            triangles.Clear();

            _isArbitraryQuad = false;
            _alphaInVertexColor = false;
        }

        /// <summary>
        /// 
        /// </summary>
        public int currentVertCount
        {
            get
            {
                return vertices.Count;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        public void AddVert(Vector3 position)
        {
            position.y = -position.y;
            vertices.Add(position);
            colors.Add(vertexColor);
            if (vertexColor.a != 255)
                _alphaInVertexColor = true;
            uvs.Add(new Vector2(
                    Mathf.Lerp(uvRect.xMin, uvRect.xMax, (position.x - contentRect.xMin) / contentRect.width),
                    Mathf.Lerp(uvRect.yMax, uvRect.yMin, (-position.y - contentRect.yMin) / contentRect.height)));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="color"></param>
        public void AddVert(Vector3 position, Color32 color)
        {
            position.y = -position.y;
            vertices.Add(position);
            colors.Add(color);
            if (color.a != 255)
                _alphaInVertexColor = true;
            uvs.Add(new Vector2(
                    Mathf.Lerp(uvRect.xMin, uvRect.xMax, (position.x - contentRect.xMin) / contentRect.width),
                    Mathf.Lerp(uvRect.yMax, uvRect.yMin, (-position.y - contentRect.yMin) / contentRect.height)));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="color"></param>
        /// <param name="uv"></param>
        public void AddVert(Vector3 position, Color32 color, Vector2 uv)
        {
            position.y = -position.y;
            vertices.Add(position);
            uvs.Add(new Vector2(uv.x, uv.y));
            colors.Add(color);
            if (color.a != 255)
                _alphaInVertexColor = true;
        }

        /// <summary>
        /// 
        /// 1---2
        /// | / |
        /// 0---3
        /// </summary>
        /// <param name="vertRect"></param>
        public void AddQuad(Rect vertRect)
        {
            AddVert(new Vector3(vertRect.xMin, vertRect.yMax, 0f));
            AddVert(new Vector3(vertRect.xMin, vertRect.yMin, 0f));
            AddVert(new Vector3(vertRect.xMax, vertRect.yMin, 0f));
            AddVert(new Vector3(vertRect.xMax, vertRect.yMax, 0f));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vertRect"></param>
        /// <param name="color"></param>
        public void AddQuad(Rect vertRect, Color32 color)
        {
            AddVert(new Vector3(vertRect.xMin, vertRect.yMax, 0f), color);
            AddVert(new Vector3(vertRect.xMin, vertRect.yMin, 0f), color);
            AddVert(new Vector3(vertRect.xMax, vertRect.yMin, 0f), color);
            AddVert(new Vector3(vertRect.xMax, vertRect.yMax, 0f), color);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vertRect"></param>
        /// <param name="color"></param>
        /// <param name="uvRect"></param>
        public void AddQuad(Rect vertRect, Color32 color, Rect uvRect)
        {
            vertices.Add(new Vector3(vertRect.xMin, -vertRect.yMax, 0f));
            vertices.Add(new Vector3(vertRect.xMin, -vertRect.yMin, 0f));
            vertices.Add(new Vector3(vertRect.xMax, -vertRect.yMin, 0f));
            vertices.Add(new Vector3(vertRect.xMax, -vertRect.yMax, 0f));

            uvs.Add(new Vector2(uvRect.xMin, uvRect.yMin));
            uvs.Add(new Vector2(uvRect.xMin, uvRect.yMax));
            uvs.Add(new Vector2(uvRect.xMax, uvRect.yMax));
            uvs.Add(new Vector2(uvRect.xMax, uvRect.yMin));

            colors.Add(color);
            colors.Add(color);
            colors.Add(color);
            colors.Add(color);

            if (color.a != 255)
                _alphaInVertexColor = true;
        }

        static List<Vector4> helperV4List = new List<Vector4>(4) { Vector4.zero, Vector4.zero, Vector4.zero, Vector4.zero };
        internal List<Vector4> FixUVForArbitraryQuad()
        {
            //ref1 http://www.reedbeta.com/blog/quadrilateral-interpolation-part-1/
            //ref2 https://bitlush.com/blog/arbitrary-quadrilaterals-in-opengl-es-2-0

            Vector4 qq = Vector4.one;
            Vector2 a = vertices[2] - vertices[0];
            Vector2 b = vertices[1] - vertices[3];
            Vector2 c = vertices[0] - vertices[3];

            float cross = a.x * b.y - a.y * b.x;
            if (cross != 0)
            {
                float s = (a.x * c.y - a.y * c.x) / cross;
                if (s > 0 && s < 1)
                {
                    float t = (b.x * c.y - b.y * c.x) / cross;
                    if (t > 0 && t < 1)
                    {
                        qq.x = 1 / (1 - t);
                        qq.y = 1 / s;
                        qq.z = 1 / t;
                        qq.w = 1 / (1 - s);
                    }
                }
            }

            for (int i = 0; i < 4; i++)
            {
                Vector4 v = uvs[i];
                float q = qq[i];
                v.x *= q;
                v.y *= q;
                v.w = q;
                helperV4List[i] = v;
            }

            return helperV4List;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="startIndex"></param>
        /// <param name="count"></param>
        public void RepeatColors(Color32[] value, int startIndex, int count)
        {
            int len = Mathf.Min(startIndex + count, vertices.Count);
            int colorCount = value.Length;
            int k = 0;
            for (int i = startIndex; i < len; i++)
            {
                Color32 c = value[(k++) % colorCount];
                colors[i] = c;
                if (c.a != 255)
                    _alphaInVertexColor = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="idx0"></param>
        /// <param name="idx1"></param>
        /// <param name="idx2"></param>
        public void AddTriangle(int idx0, int idx1, int idx2)
        {
            triangles.Add(idx0);
            triangles.Add(idx1);
            triangles.Add(idx2);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="idxList"></param>
        /// <param name="startVertexIndex"></param>
        public void AddTriangles(int[] idxList, int startVertexIndex = 0)
        {
            if (startVertexIndex != 0)
            {
                if (startVertexIndex < 0)
                    startVertexIndex = vertices.Count + startVertexIndex;

                int cnt = idxList.Length;
                for (int i = 0; i < cnt; i++)
                    triangles.Add(idxList[i] + startVertexIndex);
            }
            else
                triangles.AddRange(idxList);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startVertexIndex"></param>
        public void AddTriangles(int startVertexIndex = 0)
        {
            int cnt = vertices.Count;
            if (startVertexIndex < 0)
                startVertexIndex = cnt + startVertexIndex;

            for (int i = startVertexIndex; i < cnt; i += 4)
            {
                triangles.Add(i);
                triangles.Add(i + 1);
                triangles.Add(i + 2);

                triangles.Add(i + 2);
                triangles.Add(i + 3);
                triangles.Add(i);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Vector3 GetPosition(int index)
        {
            if (index < 0)
                index = vertices.Count + index;

            Vector3 vec = vertices[index];
            vec.y = -vec.y;
            return vec;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="usePercent"></param>
        /// <returns></returns>
        public Vector2 GetUVAtPosition(Vector2 position, bool usePercent)
        {
            if (usePercent)
            {
                return new Vector2(Mathf.Lerp(uvRect.xMin, uvRect.xMax, position.x),
                    Mathf.Lerp(uvRect.yMax, uvRect.yMin, position.y));
            }
            else
                return new Vector2(Mathf.Lerp(uvRect.xMin, uvRect.xMax, (position.x - contentRect.xMin) / contentRect.width),
                    Mathf.Lerp(uvRect.yMax, uvRect.yMin, (position.y - contentRect.yMin) / contentRect.height));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vb"></param>
        public void Append(VertexBuffer vb)
        {
            int len = vertices.Count;
            vertices.AddRange(vb.vertices);
            uvs.AddRange(vb.uvs);
            uvs2.AddRange(vb.uvs2);
            colors.AddRange(vb.colors);
            if (len != 0)
            {
                int len1 = vb.triangles.Count;
                for (int i = 0; i < len1; i++)
                    triangles.Add(vb.triangles[i] + len);
            }
            else
                triangles.AddRange(vb.triangles);

            if (vb._alphaInVertexColor)
                _alphaInVertexColor = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vb"></param>
        public void Insert(VertexBuffer vb)
        {
            vertices.InsertRange(0, vb.vertices);
            uvs.InsertRange(0, vb.uvs);
            uvs2.InsertRange(0, vb.uvs2);
            colors.InsertRange(0, vb.colors);
            int len = triangles.Count;
            if (len != 0)
            {
                int len1 = vb.vertices.Count;
                for (int i = 0; i < len; i++)
                    triangles[i] += len1;
            }
            triangles.InsertRange(0, vb.triangles);

            if (vb._alphaInVertexColor)
                _alphaInVertexColor = true;
        }
    }
}

using UnityEngine;

namespace FairyGUI
{
    /// <summary>
    /// 
    /// </summary>
    public class RegularPolygonMesh : IMeshFactory, IHitTest
    {
        /// <summary>
        /// 
        /// </summary>
        public Rect? drawRect;

        /// <summary>
        /// 
        /// </summary>
        public int sides;

        /// <summary>
        /// 
        /// </summary>
        public float lineWidth;

        /// <summary>
        /// 
        /// </summary>
        public Color32 lineColor;

        /// <summary>
        /// 
        /// </summary>
        public Color32? centerColor;

        /// <summary>
        /// 
        /// </summary>
        public Color32? fillColor;

        /// <summary>
        /// 
        /// </summary>
        public float[] distances;

        /// <summary>
        /// 
        /// </summary>
        public float rotation;

        public RegularPolygonMesh()
        {
            sides = 3;
            lineColor = Color.black;
        }

        public void OnPopulateMesh(VertexBuffer vb)
        {
            if (distances != null && distances.Length < sides)
            {
                Debug.LogError("distances.Length<sides");
                return;
            }

            Rect rect = drawRect != null ? (Rect)drawRect : vb.contentRect;
            Color32 color = fillColor != null ? (Color32)fillColor : vb.vertexColor;

            float angleDelta = 2 * Mathf.PI / sides;
            float angle = rotation * Mathf.Deg2Rad;
            float radius = Mathf.Min(rect.width / 2, rect.height / 2);

            float centerX = radius + rect.x;
            float centerY = radius + rect.y;
            vb.AddVert(new Vector3(centerX, centerY, 0), centerColor == null ? color : (Color32)centerColor);
            for (int i = 0; i < sides; i++)
            {
                float r = radius;
                if (distances != null)
                    r *= distances[i];
                float xv = Mathf.Cos(angle) * (r - lineWidth);
                float yv = Mathf.Sin(angle) * (r - lineWidth);
                Vector3 vec = new Vector3(xv + centerX, yv + centerY, 0);
                vb.AddVert(vec, color);
                if (lineWidth > 0)
                {
                    vb.AddVert(vec, lineColor);

                    xv = Mathf.Cos(angle) * r + centerX;
                    yv = Mathf.Sin(angle) * r + centerY;
                    vb.AddVert(new Vector3(xv, yv, 0), lineColor);
                }
                angle += angleDelta;
            }

            if (lineWidth > 0)
            {
                int tmp = sides * 3;
                for (int i = 0; i < tmp; i += 3)
                {
                    if (i != tmp - 3)
                    {
                        vb.AddTriangle(0, i + 1, i + 4);
                        vb.AddTriangle(i + 5, i + 2, i + 3);
                        vb.AddTriangle(i + 3, i + 6, i + 5);
                    }
                    else
                    {
                        vb.AddTriangle(0, i + 1, 1);
                        vb.AddTriangle(2, i + 2, i + 3);
                        vb.AddTriangle(i + 3, 3, 2);
                    }
                }
            }
            else
            {
                for (int i = 0; i < sides; i++)
                    vb.AddTriangle(0, i + 1, (i == sides - 1) ? 1 : i + 2);
            }
        }

        public bool HitTest(Rect contentRect, Vector2 point)
        {
            if (drawRect != null)
                return ((Rect)drawRect).Contains(point);
            else
                return contentRect.Contains(point);
        }
    }
}

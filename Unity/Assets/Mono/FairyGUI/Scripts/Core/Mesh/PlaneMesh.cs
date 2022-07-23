using UnityEngine;

namespace FairyGUI
{
    /// <summary>
    /// 
    /// </summary>
    public class PlaneMesh : IMeshFactory
    {
        public int gridSize = 30;

        public void OnPopulateMesh(VertexBuffer vb)
        {
            float w = vb.contentRect.width;
            float h = vb.contentRect.height;
            float xMax = vb.contentRect.xMax;
            float yMax = vb.contentRect.yMax;
            int hc = (int)Mathf.Min(Mathf.CeilToInt(w / gridSize), 9);
            int vc = (int)Mathf.Min(Mathf.CeilToInt(h / gridSize), 9);
            int eachPartX = Mathf.FloorToInt(w / hc);
            int eachPartY = Mathf.FloorToInt(h / vc);
            float x, y;
            for (int i = 0; i <= vc; i++)
            {
                if (i == vc)
                    y = yMax;
                else
                    y = vb.contentRect.y + i * eachPartY;
                for (int j = 0; j <= hc; j++)
                {
                    if (j == hc)
                        x = xMax;
                    else
                        x = vb.contentRect.x + j * eachPartX;
                    vb.AddVert(new Vector3(x, y, 0));
                }
            }

            for (int i = 0; i < vc; i++)
            {
                int k = i * (hc + 1);
                for (int j = 1; j <= hc; j++)
                {
                    int m = k + j;
                    vb.AddTriangle(m - 1, m, m + hc);
                    vb.AddTriangle(m, m + hc + 1, m + hc);
                }
            }
        }
    }
}
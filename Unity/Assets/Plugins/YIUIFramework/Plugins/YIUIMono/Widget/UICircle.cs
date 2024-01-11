using UnityEngine;
using UnityEngine.Sprites;
using UnityEngine.UI;

namespace YIUIFramework
{
    /// <summary>
    /// 圆形遮罩
    /// </summary>
    public class UICircle : Graphic
    {
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            Vector2 size = rectTransform.rect.size;
            Vector2 pivot = rectTransform.pivot;

            Debug.Log("XX" + pivot.x);
            Debug.Log("YY" + pivot.y);

            UIVertex vertex = UIVertex.simpleVert;
            vertex.color = Color.clear;

            // 顶点1
            vertex.position = new Vector2(pivot.x * size.x, pivot.y * size.y);
            vh.AddVert(vertex);

            // 顶点2
            vertex.position = new Vector2((pivot.x - 0.5f) * size.x, (pivot.y - 0.5f) * size.y);
            vh.AddVert(vertex);

            // 顶点3
            vertex.position = new Vector2((pivot.x + 0.5f) * size.x, (pivot.y - 0.5f) * size.y);
            vh.AddVert(vertex);

            // 添加三角形
            vh.AddTriangle(0, 1, 2);
        }
    }
}
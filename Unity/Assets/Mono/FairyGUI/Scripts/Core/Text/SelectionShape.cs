using System.Collections.Generic;
using UnityEngine;
using FairyGUI.Utils;

namespace FairyGUI
{
    /// <summary>
    /// 
    /// </summary>
    public class SelectionShape : DisplayObject, IMeshFactory
    {
        public readonly List<Rect> rects;

        public SelectionShape()
        {
            CreateGameObject("SelectionShape");
            graphics = new NGraphics(gameObject);
            graphics.texture = NTexture.Empty;
            graphics.meshFactory = this;

            rects = new List<Rect>();
        }

        /// <summary>
        /// 
        /// </summary>
        public Color color
        {
            get
            {
                return graphics.color;
            }
            set
            {
                graphics.color = value;
                graphics.Tint();
            }
        }

        public void Refresh()
        {
            int count = rects.Count;
            if (count > 0)
            {
                Rect rect = new Rect();
                rect = rects[0];
                Rect tmp;
                for (int i = 1; i < count; i++)
                {
                    tmp = rects[i];
                    rect = ToolSet.Union(ref rect, ref tmp);
                }
                SetSize(rect.xMax, rect.yMax);
            }
            else
                SetSize(0, 0);
            graphics.SetMeshDirty();
        }

        public void Clear()
        {
            rects.Clear();
            graphics.SetMeshDirty();
        }

        public void OnPopulateMesh(VertexBuffer vb)
        {
            int count = rects.Count;
            if (count == 0 || this.color == Color.clear)
                return;

            for (int i = 0; i < count; i++)
                vb.AddQuad(rects[i]);
            vb.AddTriangles();
        }

        protected override DisplayObject HitTest()
        {
            Vector2 localPoint = WorldToLocal(HitTestContext.worldPoint, HitTestContext.direction);

            if (_contentRect.Contains(localPoint))
            {
                int count = rects.Count;
                for (int i = 0; i < count; i++)
                {
                    if (rects[i].Contains(localPoint))
                        return this;
                }
            }

            return null;
        }
    }
}

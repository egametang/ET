using System.Collections.Generic;
using UnityEngine;

namespace FairyGUI
{
    /// <summary>
    /// 
    /// </summary>
    public class CompositeMesh : IMeshFactory, IHitTest
    {
        /// <summary>
        /// 
        /// </summary>
        public readonly List<IMeshFactory> elements;

        /// <summary>
        /// If it is -1, means all elements are active, otherwise, only the specific element is active
        /// </summary>
        public int activeIndex;

        public CompositeMesh()
        {
            elements = new List<IMeshFactory>();
            activeIndex = -1;
        }

        public void OnPopulateMesh(VertexBuffer vb)
        {
            int cnt = elements.Count;
            if (cnt == 1)
                elements[0].OnPopulateMesh(vb);
            else
            {
                VertexBuffer vb2 = VertexBuffer.Begin(vb);

                for (int i = 0; i < cnt; i++)
                {
                    if (activeIndex == -1 || i == activeIndex)
                    {
                        vb2.Clear();
                        elements[i].OnPopulateMesh(vb2);
                        vb.Append(vb2);
                    }
                }

                vb2.End();
            }
        }

        public bool HitTest(Rect contentRect, Vector2 point)
        {
            if (!contentRect.Contains(point))
                return false;

            bool flag = false;
            int cnt = elements.Count;
            for (int i = 0; i < cnt; i++)
            {
                if (activeIndex == -1 || i == activeIndex)
                {
                    IHitTest ht = elements[i] as IHitTest;
                    if (ht != null)
                    {
                        if (ht.HitTest(contentRect, point))
                            return true;
                    }
                    else
                        flag = true;
                }
            }

            return flag;
        }
    }
}
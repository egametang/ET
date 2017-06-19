using UnityEngine;
using System.Collections;
using UnityEditor;

public class UESScrollView : UESDisplayObject
{
    protected Vector2 m_ScrollPosition;

    protected override void Awake()
    {
        base.Awake();
        Rect = new Rect(Vector2.zero, new Vector2(INIT_WIDTH, 100));
    }

    internal override void OnChildrenGUI()
    {
        if (Enable)
        {
            //渲染自身
            if (m_Mask != null) GUI.BeginClip(m_DrawingMask, Vector2.zero, Vector2.zero, false);
            GUI.SetNextControlName(m_Guid);
            m_Stage.PushToRenderList(this);
            OnGUI();

            var viewRect = new Rect();
            if (m_Children.Count>0)
            {
                viewRect = m_Children[0].m_Rect;
                foreach (var child in m_Children)
                {
                    viewRect = Rect.MinMaxRect
                        (
                        Mathf.Min(viewRect.xMin,child.m_Rect.xMin),
                        Mathf.Min(viewRect.yMin, child.m_Rect.yMin),
                        Mathf.Max(viewRect.xMax, child.m_Rect.xMax),
                        Mathf.Max(viewRect.yMax, child.m_Rect.yMax)
                        );
                }
            }
            var cacheScrollPosition = m_ScrollPosition;
            m_ScrollPosition =GUI.BeginScrollView(m_DrawingRect, m_ScrollPosition, viewRect);
            var offset = cacheScrollPosition - m_ScrollPosition;
            if (offset != Vector2.zero)
            {
                foreach (var child in m_Children)
                {
                    var position = new Rect(child.m_Rect.position + offset, child.m_Rect.size);
                    child.m_Rect = position;
                }
                UpdateValues(this);
            }

            //渲染子物体
            foreach (var child in m_Children)
            {
                if (child.Enable) child.OnChildrenGUI();
            }

            GUI.EndScrollView();
            if (m_Mask != null) GUI.EndClip();
        }
    }

    public override Rect Rect
    {
        get
        {
            return base.Rect;
        }

        set
        {
            m_Mask = new Rect(Vector2.zero, value.size);
            base.Rect = value;
        }
    }

    public override Rect? Mask
    {
        get
        {
            return base.Mask;
        }

        set
        {
            Debug.LogError("此组件暂不支持遮罩！");
        }
    }

    public Vector2 ScrollPosition
    {
        get
        {
            return m_ScrollPosition;
        }
        set
        {
            m_ScrollPosition = value;
        }
    }
}

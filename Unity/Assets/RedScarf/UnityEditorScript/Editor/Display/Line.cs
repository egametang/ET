using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>
/// 绘制直线
/// </summary>
public class UESLine : UESDisplayObject
{
    const float HIT_WIDTH_MIN = 10f;                        //线的最小碰撞宽度

    protected Vector2[] m_Points;
    protected Vector2[] m_DrawingPoints;
    protected Vector2[] m_GlobalPoints;
    protected Color m_Color;

    protected override void Awake()
    {
        base.Awake();
        SetDrawingStyle(Color.white);
        Points = new Vector2[]
        {
            new Vector2(0,0),
            new Vector2(150,30),
            new Vector2(300,0)
        };
    }

    public override void OnGUI()
    {
        base.OnGUI();

        if (m_DrawingPoints == null|| m_DrawingPoints.Length<2) return;

        Handles.BeginGUI();
        Handles.color = m_Color;
        for (var i=0;i< m_DrawingPoints.Length - 1;i++)
        {
            Handles.DrawLine(m_DrawingPoints[i], m_DrawingPoints[i+1]);
        }
        Handles.EndGUI();
    }

    protected override void UpdateSelf()
    {
        m_DrawingPoints = new Vector2[m_Points.Length];
        var containsPointRect = UESTool.GetContainsPointsRect(m_Points);
        for (var i = 0; i < m_DrawingPoints.Length; i++)
        {
            m_DrawingPoints[i] = m_Points[i] + m_DrawingRect.position - containsPointRect.position;
        }
        m_GlobalPoints = new Vector2[m_Points.Length];
        for (var i = 0; i < m_GlobalPoints.Length; i++)
        {
            m_GlobalPoints[i] = Local2Global(m_Parent, m_Points[i]);
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
            if (m_Points == null)
            {
                //Debug.Log("直接设置Points");
                return;
            }

            var containsPointRect = UESTool.GetContainsPointsRect(m_Points);
            var offset = value.position - containsPointRect.position;
            for(var i=0;i<m_Points.Length;i++)
            {
                m_Points[i] = new Vector2(m_Points[i].x + offset.x, m_Points[i].y + offset.y);
            }
            Points = m_Points;
        }
    }

    public void SetDrawingStyle(Color color)
    {
        m_Color = color;
    }

    public Vector2[] Points
    {
        get
        {
            return m_Points;
        }
        set
        {
            if (value.Length < 2)
            {
                Debug.Log("最少2个点.");
                return;
            }

            m_Points = value;
            var containsPointRect = UESTool.GetContainsPointsRect(value);
            m_Rect = containsPointRect;
            UpdateValues(m_Parent);
        }
    }

    public override bool IsHit(Vector2 globalPoint)
    {
        if (m_GlobalPoints == null) return false;
        if (!base.IsHit(globalPoint)) return false;

        for (var i = 0; i < m_GlobalPoints.Length - 1; i++)
        {
            var dist = HandleUtility.DistancePointLine(globalPoint, m_GlobalPoints[i], m_GlobalPoints[i + 1]);
            if (dist < HIT_WIDTH_MIN)
            {
                return true;
            }
        }

        return false;
    }
}
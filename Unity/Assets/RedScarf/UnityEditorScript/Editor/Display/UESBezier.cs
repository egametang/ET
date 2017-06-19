using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>
/// 绘制贝塞尔曲线
/// </summary>
public class UESBezier : UESDisplayObject
{
    const float TANGENT_LENGTH = 0.3f;                                          //操控轴占百分比
    const float HIT_WIDTH_MIN = 10f;                                            //线的最小碰撞宽度

    protected Vector2[] m_Points;
    protected UESBezierPointData[] bezierPoints;
    protected UESBezierPointData[] globalBezierPoints;
    protected UESBezierPointData[] drawingBezierPoints;
    protected Color m_Color;
    protected float m_Width = 1;                                                //宽
    protected Texture2D m_Texture;                                              //位图纹理

    protected override void Awake()
    {
        base.Awake();
        SetDrawingStyle(Color.white, null, 1);
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

        Handles.BeginGUI();
        Handles.color = m_Color;
        if (bezierPoints!=null)
        {
            foreach (var point in drawingBezierPoints)
            {
                Handles.DrawBezier(point.startPosition, point.endPosition, point.startTangent, point.endTangent, m_Color, m_Texture,m_Width);
            }
        }
        Handles.EndGUI();
    }

    public void SetDrawingStyle(Color color,Texture2D texture,float width)
    {
        m_Color = color;
        m_Texture = texture;
        m_Width = width;
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
            for (var i = 0; i < m_Points.Length; i++)
            {
                m_Points[i] = new Vector2(m_Points[i].x + offset.x, m_Points[i].y + offset.y);
            }
            Points = m_Points;
        }
    }

    /// <summary>
    /// 关键点
    /// 使用父容器坐标
    /// </summary>
    public Vector2[] Points
    {
        get
        {
            return m_Points;
        }
        set
        {
            if (value == null||value.Length<2)
            {
                Debug.LogError("m_Points不能为空，且点数量>=2");
                return;
            }

            m_Points = value;
            m_Rect = UESTool.GetContainsPointsRect(m_Points);
            UpdateValues(m_Parent);
        }
    }

    protected override void UpdateSelf()
    {
        if (m_Points == null || m_Points.Length < 3) return;

        var last = m_Points.Length - 1;
        Queue<Vector2> tangentQueue = new Queue<Vector2>();
        bezierPoints = new UESBezierPointData[last];

        for (var i = 0; i < m_Points.Length; i++)
        {
            if (i == 0) { tangentQueue.Enqueue(m_Points[0]); continue; }
            if (i == last) { tangentQueue.Enqueue(m_Points[last]); continue; }

            Vector3 currentPoint = m_Points[i];
            Vector3 pevPoint = m_Points[i - 1];
            Vector3 nextPoint = m_Points[i + 1];

            var sidePev = pevPoint - currentPoint;
            var sideNext = nextPoint - currentPoint;
            var median = Vector3.Lerp(sidePev, sideNext, 0.5f);     //中线

            var sidePevCross = Vector3.Cross(median, sidePev);
            var sidePevTangent = Vector3.Cross(sidePevCross, median).normalized * Vector3.Distance(pevPoint, currentPoint) * TANGENT_LENGTH + currentPoint;
            var sideNextCross = Vector3.Cross(median, sideNext);
            var sideNextTangent = Vector3.Cross(sideNextCross, median).normalized * Vector3.Distance(nextPoint, currentPoint) * TANGENT_LENGTH + currentPoint;

            tangentQueue.Enqueue(sidePevTangent);
            tangentQueue.Enqueue(sideNextTangent);
        }

        for (var i = 0; i < last; i++)
        {
            bezierPoints[i] = new UESBezierPointData(m_Points[i], m_Points[i + 1], tangentQueue.Dequeue(), tangentQueue.Dequeue());
        }

        globalBezierPoints = new UESBezierPointData[bezierPoints.Length];
        for (var i = 0; i < globalBezierPoints.Length; i++)
        {
            var globalOffset = Local2Global(m_Parent, bezierPoints[i].startPosition) - bezierPoints[i].startPosition;
            var globalBezierPoint = bezierPoints[i];
            globalBezierPoint.startPosition += globalOffset;
            globalBezierPoint.endPosition += globalOffset;
            globalBezierPoint.startTangent += globalOffset;
            globalBezierPoint.endTangent += globalOffset;
            globalBezierPoints[i] = globalBezierPoint;
        }

        drawingBezierPoints = new UESBezierPointData[bezierPoints.Length];
        var offset = m_DrawingRect.position - UESTool.GetContainsPointsRect(m_Points).position;
        for (var i = 0; i < drawingBezierPoints.Length; i++)
        {
            var point = bezierPoints[i];
            point.startPosition += offset;
            point.endPosition += offset;
            point.startTangent += offset;
            point.endTangent += offset;
            drawingBezierPoints[i] = point;
        }
    }

    public override bool IsHit(Vector2 globalPoint)
    {
        foreach (var point in globalBezierPoints)
        {
            var dist = HandleUtility.DistancePointBezier(globalPoint, point.startPosition, point.endPosition, point.startTangent, point.endTangent);
            if (dist < HIT_WIDTH_MIN)
            {
                return true;
            }
        }

        return false;
    }
}

[System.Serializable]
public struct UESBezierPointData
{
    public Vector2 startPosition;
    public Vector2 endPosition;
    public Vector2 startTangent;
    public Vector2 endTangent;

    public UESBezierPointData(Vector2 startPosition, Vector2 endPosition, Vector2 startTangent, Vector2 endTangent)
    {
        this.startPosition = startPosition;
        this.endPosition = endPosition;
        this.startTangent = startTangent;
        this.endTangent = endTangent;
    }
}
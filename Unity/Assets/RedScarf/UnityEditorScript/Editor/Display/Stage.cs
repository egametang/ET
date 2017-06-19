using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 舞台
/// </summary>
public sealed class UESStage : UESDisplayObject
{
    private UESDisplayObject m_DragObject;
    private Vector2 m_DragOffset;
    private bool m_Repaint;
    List<UESDisplayObject> m_RenderList;

    protected override void Awake()
    {
        base.Awake();

        m_RenderList = new List<UESDisplayObject>();
        m_Stage = this;
        m_Depth = -1;
        m_Rect = new Rect();
        m_GlobalRect = m_Rect;
        m_Mask = m_Rect;
        m_GlobalHitArea = m_Rect;
    }

    public override void OnGUI()
    {
        m_Rect =new Rect(Vector2.zero, m_Window.position.size);
        m_GlobalRect = m_Rect;
        m_Mask = m_Rect;
        m_GlobalHitArea = m_Rect;

        //渲染元素,更新渲染列表
        m_RenderList.Clear();
        foreach (var child in m_Children)
        {
            child.OnChildrenGUI();
        }

        if (m_DragObject != null)
        {
            var widget = new Rect(Event.current.mousePosition + m_DragOffset, m_DragObject.GlobalRect.size);
            m_DragObject.GlobalRect = widget;
        }

        m_Window.Repaint();
    }

    public override void Update()
    {
        //递归调用子物体OnChildrenUpdate方法
        foreach (var child in m_Children)
        {
            child.OnChildrenUpdate();
        }
    }

    /// <summary>
    /// 渲染列表，按照渲染先后顺序排列
    /// </summary>
    public List<UESDisplayObject> RenderList { get { return m_RenderList; } }
    public override int Depth { get { return m_Depth; } set { } }
    public override UESDisplayObject Parent { get { return null; } }
    public override Rect? Mask { get { return m_Mask; } set { } }
    public override Rect Rect { get { return m_Rect; } set { } }


    /// <summary>
    /// 当前拖拽的元素
    /// </summary>
    public UESDisplayObject DragObject { get { return m_DragObject; } set { m_DragObject = value; } }

    internal void StartDrag(UESDisplayObject display)
    {
        if (display.m_Stage != this || display is UESStage||display==this) return;

        m_DragObject = display;
        m_DragOffset = display.GlobalRect.position - Event.current.mousePosition;
    }

    internal void StopDrag(UESDisplayObject display)
    {
        if(display==m_DragObject)m_DragObject = null;
    }

    internal void PushToRenderList(UESDisplayObject display)
    {
        m_RenderList.Add(display);
    }
}
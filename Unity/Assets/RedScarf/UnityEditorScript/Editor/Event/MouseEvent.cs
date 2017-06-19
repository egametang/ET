using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class UESMouseEvent : UESEvent
{
    public const string MOUSE_DOWN         = "MouseDown";
    public const string MOUSE_UP           = "MouseUp";
    public const string MOUSE_DRAG         = "MouseDrag";
    public const string MOUSE_DRAG_START   = "MouseDragStart";
    public const string MOUSE_DRAG_END     = "MouseDragEnd";
    public const string MOUSE_MOVE         = "MouseMove";
    public const string MOUSE_CLICK        = "MouseClick";
    public const string MOUSE_MIDDLE_CLICK = "MouseMiddleClick";
    public const string MOUSE_RIGHT_CLICK  = "MouseRightClick";
    public const string MOUSE_DOUBLE_CLICK = "MouseDoubleClick";
    public const string MOUSE_SCROLL_WHUESL = "MouseScrollWheel";

    const uint CLICK_MOVE_THRESHOLD = 2;
    const float CLICK_TIME_THRESHOLD = 0.3f;

    static List<UESMouseEvent> mouseEventList=new List<UESMouseEvent>();
    static Vector2 startPos;
    static ClickTypes clickType = ClickTypes.None;
    static double checkClickTime;
    static bool isMouseDown;
    static bool isInvokeClick;
    static int clickCount;
    static Vector2 mousePosition;
    static int button;

    protected enum ClickTypes
    {
        None,
        Click,
        DoubleClick
    }

    protected enum DragSteps
    {
        Unknown,
        Start,
        Draging,
        End
    }

    protected DragSteps dragStep;

    public UESMouseEvent(UESObject target, string type, Action<UESEvent> callback)
        : base(target, type, callback)
    {
        checkClickTime = EditorApplication.timeSinceStartup;
    }

    public override void OnGUI()
    {
        m_CurrentEvent = Event.current;
        var display = m_CurrentTarget as UESDisplayObject;
        if (display == null || display.m_Stage == null || display.m_Stage.m_Window != UESWindow.focusedWindow) return;

        if (Event.current.isMouse)
        {
            if (mouseEventList.IndexOf(this) == 0)
            {
                if (!isMouseDown&&Event.current.type==EventType.MouseDown)
                {
                    isMouseDown = true;
                    checkClickTime = EditorApplication.timeSinceStartup;
                    clickType = ClickTypes.None;
                    isInvokeClick = false;
                }
                clickCount = Mathf.Max(clickCount, Event.current.clickCount);
            }
            button=Event.current.button;

            switch (Event.current.type)
            {
                case EventType.MouseDown:
                    startPos = Event.current.mousePosition;
                    dragStep = DragSteps.Unknown;
                    if (m_Type == MOUSE_DOWN && Event.current.button == 0) Invoke();
                    break;

                case EventType.MouseDrag:
                    if (m_Type == MOUSE_DRAG_START && Event.current.button == 0 && dragStep == DragSteps.Unknown)
                    {
                        dragStep=DragSteps.Start;
                        Invoke();
                    }
                    else if (m_Type == MOUSE_DRAG && Event.current.button == 0 && dragStep == DragSteps.Draging&&Event.current.delta!=Vector2.zero) Invoke();

                    dragStep = DragSteps.Draging;
                    break;

                case EventType.MouseUp:
                    if (m_Type == MOUSE_UP && Event.current.button == 0) Invoke();

                    if (m_Type == MOUSE_DRAG_END && Event.current.button == 0&&dragStep!=DragSteps.Unknown)
                    {
                        dragStep = DragSteps.End;
                        Invoke();
                    }
                    break;
            }
        }

        //判断MouseMove
        //此处判断不会受EditorWindow.wantsMouseMove==true的影响
        if (m_Type == MOUSE_MOVE && mousePosition != Event.current.mousePosition) Invoke();

        //判断MouseScrollWheel
        if (Event.current.type == EventType.ScrollWheel && m_Type == MOUSE_SCROLL_WHUESL && Event.current.delta != Vector2.zero) Invoke();

        mousePosition = Event.current.mousePosition;
    }

    public override void Update()
    {
        if (EditorApplication.timeSinceStartup - checkClickTime >= CLICK_TIME_THRESHOLD)
        {
            if (clickCount == 0) clickType = ClickTypes.None;
            else if (clickCount >= 2) clickType = ClickTypes.DoubleClick;
            else if (clickCount == 1) clickType = ClickTypes.Click;
            clickCount = 0;
            isMouseDown = false;
            checkClickTime = EditorApplication.timeSinceStartup;
            isInvokeClick = true;
        }
        if (isInvokeClick)
        {
            bool isMove = Vector2.Distance(startPos, mousePosition) > CLICK_MOVE_THRESHOLD ? true : false;
            if (!isMove)
            {
                if (clickType == ClickTypes.Click)
                {
                    if (button == 0 && m_Type == MOUSE_CLICK) Invoke();
                    else if (button == 1 && m_Type == MOUSE_RIGHT_CLICK) Invoke();
                    else if (button == 2 && m_Type == MOUSE_MIDDLE_CLICK) Invoke();
                }
                else if (clickType == ClickTypes.DoubleClick)
                {
                    if (button == 0 && m_Type == MOUSE_DOUBLE_CLICK) Invoke();
                }
            }
        }
        if (mouseEventList.IndexOf(this) == mouseEventList.Count - 1) isInvokeClick = false;
    }

    public override void Invoke()
    {
        //检测最顶部的显示对象触发事件
        var display = (UESDisplayObject)m_CurrentTarget;
        var hitList = UESTool.GetHitDisplayList(display.Stage, mousePosition);
        hitList.Sort(
            (a,b) =>
            {
                if (a.GlobalDepth == b.GlobalDepth) return 0;
                return (a.GlobalDepth > b.GlobalDepth) ? -1 : 1;
            }
            );
        hitList.RemoveAll(
            (x) =>
            {
                return (x.MouseEnabled) ? false : true;
            }
            );
        if (hitList.Count>0)
        {
            //最顶部的显示对象才会触发
            var target = hitList[0];
            bool isContainChild = false;
            FindContainChild(display, target.Parent, target, ref isContainChild);
            if (isContainChild)
            {
                m_Target = target;
                base.Invoke();
            }
        }
    }

    internal override void OnAddToManager()
    {
        mouseEventList.Add(this);
    }

    internal override void OnRemoveFromManager()
    {
        mouseEventList.Remove(this);
    }

    /// <summary>
    /// 递归查找元素是否包含目标
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="child"></param>
    /// <returns></returns>
    void FindContainChild(UESDisplayObject display,UESDisplayObject parent,UESDisplayObject target,ref bool isContain)
    {
        if (display == target || parent == display)
        {
            isContain = true;
            return;
        }

        if (parent != null && !(parent is UESStage)) FindContainChild(display, parent.Parent, target, ref isContain);
    }
}

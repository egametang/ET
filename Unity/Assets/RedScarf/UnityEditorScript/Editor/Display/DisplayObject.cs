using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class UESDisplayObject: UESObject
{
    public const int INIT_WIDTH = 300;
    public const int INIT_HEIGHT = 16; 

    [SerializeField]protected bool m_MouseEnabled;
    [SerializeField]protected bool m_Enable;
    [SerializeField]protected int m_Depth;
    [SerializeField]protected internal Rect m_Rect;                     //局部尺寸
    [SerializeField]protected Rect m_GlobalRect;                        //全局尺寸
    [SerializeField]protected Rect m_GlobalHitArea;                     //全局热点击区域
    [SerializeField]protected Rect? m_Mask;                             //遮罩
    [SerializeField]protected Rect? m_GlobalAccumulationIntersectMask;  //从Stage开始累积的遮罩重叠部分
    [SerializeField]protected Vector2 m_RectOffset;                     //子物体偏移，一般不需要设置
    [SerializeField]protected Vector2 m_GlobalClipOffset;               //全局坐标系画布偏移值
    [SerializeField]protected Rect m_DrawingRect;                       //当前物体渲染区域,前缀有Drawing为绘制时属性
    [SerializeField]protected Rect m_DrawingMask;                       //渲染遮罩区域

    protected UESDisplayObject m_Parent;
    protected List<UESDisplayObject> m_Children;
    protected internal UESStage m_Stage;

    public UESDisplayObject()
        :base()
    {
        m_MouseEnabled = true;
        m_Enable = true;
        m_Rect = new Rect();
        m_RectOffset = Vector2.zero;
        m_GlobalRect = new Rect();
        m_GlobalHitArea = new Rect();
        m_Children = new List<UESDisplayObject>();
        m_DrawingRect = new Rect();
        m_GlobalClipOffset = Vector2.zero;
        Rect = new Rect(Vector2.zero, new Vector2(INIT_WIDTH, INIT_HEIGHT));
    }

    /// <summary>
    /// 渲染子元素
    /// </summary>
    internal virtual void OnChildrenGUI()
    {
        if (Enable)
        {
            if (m_Mask != null) GUI.BeginClip(m_DrawingMask, Vector2.zero, Vector2.zero, false);

            //渲染自身
            GUI.SetNextControlName(m_Guid);
            m_Stage.PushToRenderList(this);
            OnGUI();

            //渲染子物体
            foreach (var child in m_Children)
            {
                if (child.Enable) child.OnChildrenGUI();
            }

            if (m_Mask != null) GUI.EndClip();
        }
    }

    /// <summary>
    /// 更新子元素
    /// </summary>
    internal virtual void OnChildrenUpdate()
    {
        if (Enable)
        {
            Update();
            foreach (var child in m_Children)
            {
                if (child.Enable)
                {
                    child.OnChildrenUpdate();
                }
            }
        }
    }

    /// <summary>
    /// 渲染自身
    /// </summary>
    public virtual void OnGUI() { }

    /// <summary>
    /// 更新自身，有需要重写此方法
    /// </summary>
    public virtual new void Update() { }

    /// <summary>
    /// 舞台
    /// </summary>
    public UESStage Stage { get { return m_Stage; } }

    /// <summary>
    /// 是否接受鼠标事件
    /// </summary>
    public bool MouseEnabled { get { return m_MouseEnabled; } set { m_MouseEnabled = value; } }

    /// <summary>
    /// 舞台坐标系热交互区域
    /// </summary>
    public Rect GlobalHitArea { get { return m_GlobalHitArea; } }

    /// <summary>
    /// 父级容器
    /// </summary>
    public virtual UESDisplayObject Parent { get { return m_Parent; } }

    /// <summary>
    /// 子元素列表
    /// </summary>
    public List<UESDisplayObject> Children { get { return m_Children; } }

    /// <summary>
    /// 相对于父级的渲染深度
    /// </summary>
    public virtual int Depth
    {
        get
        {
            return m_Depth;
        }
        set
        {
            m_Depth = value;
            if(m_Parent!=null)m_Parent.SortByDepth();
        }
    }

    /// <summary>
    /// 遮罩
    /// 使用自身坐标系，不使用遮罩设置值为null
    /// </summary>
    public virtual Rect? Mask
    {
        get
        {
            return m_Mask;
        }
        set
        {
            m_Mask = value;
            UpdateValues(m_Parent);
        }
    }

    public int GlobalDepth
    {
        get
        {
            if (m_Stage == null) return -1;
            return m_Stage.RenderList.IndexOf(this);
        }
    }

    /// <summary>
    /// 局部坐标
    /// </summary>
    public Vector2 Position
    {
        get
        {
            return m_Rect.position;
        }
        set
        {
            Rect = new Rect(value, m_Rect.size);
        }
    }

    /// <summary>
    /// 局部尺寸
    /// </summary>
    public Vector2 Size
    {
        get
        {
            return m_Rect.size;
        }
        set
        {
            Rect = new Rect(m_Rect.position, value);
        }
    }

    /// <summary>
    /// 局部显示区域
    /// </summary>
    public virtual Rect Rect
    {
        get
        {
            return m_Rect;
        }
        set
        {
            m_Rect = value;
            UpdateValues(m_Parent);
        }
    }

    /// <summary>
    /// 全局显示区域
    /// </summary>
    public virtual Rect GlobalRect {
        get
        {
            return m_GlobalRect;
        }
        set
        {
            if (m_Stage == null)
            {
                Debug.LogError("元素尚未添加到舞台，设置此值无效！");
                return;
            }

            Rect = new Rect(Global2Local(m_Parent, value.position), value.size);
        }
    }

    /// <summary>
    /// 激活
    /// </summary>
    public virtual bool Enable { get { return m_Enable; } set { m_Enable = value; } }

    /// <summary>
    /// 添加子元素
    /// </summary>
    /// <param name="child"></param>
    public virtual void AddChild(UESDisplayObject child)
    {
        if (child is UESStage) { Debug.LogError(child + "舞台不能作为子元素"); return; }
        if (child == null) { Debug.Log(child + "为null"); return; }
        if (child == this) { Debug.LogError("不能添加自己"); return; }
        if (m_Children.Contains(child)) { Debug.Log(child + "重复添加了"); return; }

        if (child.m_Parent != null) child.m_Parent.RemoveChild(child);
        child.m_Parent = this;
        m_Children.Add(child);
        int depth = 0;
        foreach (var item in m_Children)
        {
            depth = Mathf.Max(item.m_Depth,depth);
        }
        child.Depth = depth + 1;
        UpdateValues(this);
    }

    /// <summary>
    /// 移除子元素
    /// </summary>
    /// <param name="index">子元素索引</param>
    public virtual void RemoveAt(int index)
    {
        if (index >= 0 && index < m_Children.Count)
        {
            RemoveChild(m_Children[index]);
        }
        else Debug.LogError("索引越界");
    }

    /// <summary>
    /// 移除子元素
    /// </summary>
    /// <param name="child"></param>
    public virtual void RemoveChild(UESDisplayObject child)
    {
        if (child == null)
        {
            Debug.Log("child不能为null");
            return;
        }
        if (!m_Children.Contains(child))
        {
            Debug.Log("不包含"+child);
            return;
        }

        child.m_Parent = null;
        child.m_Stage = null;
        m_Children.Remove(child);
        UpdateValues(this);
    }

    /// <summary>
    /// 更新数据
    /// 从Stage根结点递归子物体由上至下更新，所以默认为父级数据已更新完为最新数据，子物体数据依赖于父级
    /// </summary>
    /// <param name="display"></param>
    protected virtual void UpdateValues(UESDisplayObject display)
    {
        if (display == null) return;

        if (display.m_Parent != null)
            display.m_Stage = display.m_Parent.m_Stage;
        else if(!(display is UESStage)) display.m_Stage = null;

        //更新自身数值
        if (display.m_Stage != null && display.m_Parent != null&&!(display is UESStage))
        {
            //舞台坐标系，其他属性依赖于此值更新
            display.m_GlobalRect = new Rect(display.m_Parent.m_GlobalRect.position + display.m_Parent.m_RectOffset + display.m_Rect.position, display.m_Rect.size);

            //渲染相关
            display.m_GlobalClipOffset = display.m_Parent.m_GlobalClipOffset;
            display.m_GlobalAccumulationIntersectMask = display.m_Parent.m_GlobalAccumulationIntersectMask;
            var offset = display.m_Parent.m_GlobalRect.position - display.m_Parent.m_GlobalClipOffset + display.m_Rect.position;
            if (display.m_Mask != null)
            {
                var maskRect = (Rect)display.m_Mask;
                display.m_DrawingMask = new Rect(offset + maskRect.position, maskRect.size);
                display.m_GlobalClipOffset += display.m_DrawingMask.position;
                display.m_DrawingRect = new Rect(-maskRect.position, display.m_Rect.size);

                var globalMask = new Rect(display.m_GlobalRect.position + maskRect.position, maskRect.size);
                if (display.m_GlobalAccumulationIntersectMask != null) display.m_GlobalAccumulationIntersectMask = UESTool.GetRectIntersect((Rect)display.m_GlobalAccumulationIntersectMask, globalMask);
                else display.m_GlobalAccumulationIntersectMask = globalMask;
            }
            else
            {
                display.m_DrawingRect = new Rect(offset, display.m_Rect.size);
            }

            //全局热点击区域
            if (display.m_GlobalAccumulationIntersectMask != null) display.m_GlobalHitArea = UESTool.GetRectIntersect((Rect)display.m_GlobalAccumulationIntersectMask, display.m_GlobalRect);
            else display.m_GlobalHitArea = display.m_GlobalRect;

            //子类重写此方法更新其他值
            if (display == this) UpdateSelf();
        }

        //更新子物体
        foreach (var child in display.m_Children)
        {
            child.UpdateValues(child);
        }
    }

    /// <summary>
    /// 重写此方法更新自身其他值
    /// </summary>
    protected virtual void UpdateSelf()
    {

    }

    /// <summary>
    /// 按照深度排序
    /// </summary>
    protected void SortByDepth()
    {
        m_Children.Sort(
            (a, b) =>
            {
                if (a.Depth == b.Depth) return 0;
                else return a.Depth > b.Depth ? 1 : -1;
            }
            );
    }

    /// <summary>
    /// 判断点是否与此元素碰撞
    /// 一般情况下hitArea包含globalPoint即可以碰撞，特殊情况根据情况重写,如内部显示为矢量贝塞尔曲线
    /// </summary>
    /// <param name="globalPoint"></param>
    /// <returns></returns>
    public virtual bool IsHit(Vector2 globalPoint)
    {
        return m_GlobalHitArea.Contains(globalPoint);
    }

    /// <summary>
    /// 开始拖拽
    /// </summary>
    public void StartDrag()
    {
        if (m_Stage == null) return;

        m_Stage.StartDrag(this);
    }

    /// <summary>
    /// 停止拖拽
    /// </summary>
    public void StopDrag()
    {
        if (m_Stage == null) return;

        m_Stage.StopDrag(this);
    }

    /// <summary>
    /// 局部坐标-->全局坐标
    /// </summary>
    /// <param name="container"></param>
    /// <param name="point"></param>
    /// <returns></returns>
    public static Vector2 Local2Global(UESDisplayObject container, Vector2 point)
    {
        if (container.Stage == null)
        {
            Debug.LogWarning("对象尚未添加到舞台，不能计算坐标！");
            return Vector3.zero;
        }

        return container.GlobalRect.position + point;
    }

    /// <summary>
    /// 全局坐标-->局部坐标
    /// </summary>
    /// <param name="container"></param>
    /// <param name="point"></param>
    /// <returns></returns>
    public static Vector2 Global2Local(UESDisplayObject container, Vector2 point)
    {
        return point - Local2Global(container, Vector2.zero);
    }
}
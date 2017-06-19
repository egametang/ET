using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Events;

/// <summary>
/// 事件基类
/// </summary>
public class UESEvent
{
    public const string UPDATE = "Update";                              //window Update时调用
    public const string ON_GUI = "OnGUI";                               //window OnGUI时调用
    public const string RESIZE = "Resize";                              //stage大小变化时调用

    protected internal UESObject m_Target;
    protected internal UESObject m_CurrentTarget;
    protected internal Action<UESEvent> callback;
    protected internal string m_Type;
    protected internal Event m_CurrentEvent;
    protected internal string m_Key;

    public UESEvent(UESObject target, string type, Action<UESEvent> callback)
    {
        this.m_Target = target;
        this.m_CurrentTarget = target;
        this.m_Type = type;
        this.callback = callback;

        var t = this.GetType();
        m_Key = target.GUID + "/" +
                t.FullName + "/" +
                type;
    }

    public string Key { get { return m_Key; } }
    public UESObject Target { get { return m_Target; } }
    public UESObject CurrentTarget { get { return m_CurrentTarget; } }
    public string Type { get { return m_Type; } }

    /// <summary>
    /// Unity默认事件
    /// </summary>
    public Event CurrentEvent { get { return m_CurrentEvent; } }

    Vector2 cacheStageSize;

    public virtual void Invoke()
    {
        if (callback != null) callback.Invoke(this);
    }

    internal virtual void OnAddToManager()
    {
        
    }

    internal virtual void OnRemoveFromManager()
    {
        
    }

    public virtual void OnGUI() {
        m_CurrentEvent = Event.current;
        switch (m_Type)
        {
            case ON_GUI:
                Invoke();
                break;

            case RESIZE:
                if(m_CurrentTarget is UESStage)
                {
                    var stage= (UESStage)m_CurrentTarget;
                    if (cacheStageSize != stage.Rect.size)
                    {
                        cacheStageSize = stage.Rect.size;
                        Invoke();
                    } 
                }
                break;
        }
    }

    public virtual void Update()
    {
        switch (m_Type)
        {
            case UPDATE:
                Invoke();
                break;
        }
    }
}

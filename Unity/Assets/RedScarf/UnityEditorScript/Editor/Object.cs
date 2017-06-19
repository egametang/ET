using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

/// <summary>
/// 基类
/// </summary>
public class UESObject : ScriptableObject, IUESEventDispatcher
{
    [SerializeField]protected string m_ObjName;
    [SerializeField]protected string m_Guid;
    [SerializeField]protected bool m_Init;

    protected internal UESWindow m_Window;

    public UESObject()
    {
        m_Guid = Guid.NewGuid().ToString();
    }

    public static new T CreateInstance<T>() where T : ScriptableObject
    {
        CreateInstance("");
        return null;
    }

    public static new ScriptableObject CreateInstance(Type type)
    {
        return CreateInstance("");
    }

    public static new ScriptableObject CreateInstance(string className)
    {
        Debug.LogError("使用UESTool.Creat方法创建对象");
        return null;
    }

    public static new void Destroy(UnityEngine.Object obj)
    {
        Destroy(obj, 0);
    }

    public static new void Destroy(UnityEngine.Object obj,float delay)
    {
        Debug.LogError("使用UESTool.Destroy方法销毁对象");
    }

    public static new void DestroyImmediate(UnityEngine.Object obj)
    {
        Destroy(obj, 0);
    }

    /// <summary>
    /// 注册下
    /// </summary>
    /// <param name="window"></param>
    internal virtual void Register(UESWindow window)
    {
        m_Window = window;
        m_Window.Register(this);
        m_Init = true;
        if (string.IsNullOrEmpty(m_ObjName))
        {
            var name = GetType().Name.Substring(3);
            m_ObjName = name.ToLower()[0] + name.Substring(1) + "_" + m_Window.GetChildInstanceID(this);
        }
    }

    public virtual void Update() { }


    /// <summary>
    /// 唯一标识符
    /// </summary>
    public string GUID { get { return m_Guid; } }

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get { return m_ObjName; } set { m_ObjName = value; } }

    /// <summary>
    /// 名称
    /// </summary>
    public new string name { get { return m_ObjName; } set { m_ObjName = value; } }

    /// <summary>
    /// 窗口
    /// </summary>
    public UESWindow Window { get { return m_Window; } }

    protected virtual void Awake()
    {
        
    }

    protected virtual void OnEnable()
    {

    }

    protected virtual void OnDisable()
    {

    }

    /// <summary>
    /// 添加事件监听
    /// </summary>
    /// <param name="type"></param>
    /// <param name="callback"></param>
    public void AddEventListener(string type, Action<UESEvent> callback)
    {
        UESEventManager.AddEventListener(this, type, callback);
    }

    /// <summary>
    /// 移除事件监听
    /// </summary>
    /// <param name="type"></param>
    /// <param name="callback"></param>
    public void RemoveEventListener(string type, Action<UESEvent> callback)
    {
        UESEventManager.RemoveEventListener(this, type, callback);
    }

    public override string ToString()
    {
        return " 【"+base.ToString()+":"+m_ObjName+"】  ";
    }
}

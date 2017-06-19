using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

/// <summary>
/// 窗口基类
/// </summary>
public abstract class UESWindow : EditorWindow
{
    [SerializeField]
	protected internal UESStage stage;
    protected bool isFourceOn;
    protected internal UESEventManager eventManager;
    protected double startTime;
    protected internal List<UESObject> objList;
    protected internal Dictionary<Type, int> instanceCounterDict;

    protected virtual void Awake()
    {
        startTime = EditorApplication.timeSinceStartup;
        instanceCounterDict = new Dictionary<Type, int>();
        objList = new List<UESObject>();
        eventManager = new UESEventManager();
        stage = UESTool.Create<UESStage>(this);
        stage.Name = "stage";
    }

    protected virtual void OnDisable() { }
    protected virtual void OnEnable() { }
    protected virtual void OnSelectionChange() { }
    protected virtual void OnFocus() { isFourceOn = true; }
    protected virtual void OnLostFocus() { isFourceOn = false; }
    protected virtual void OnHierarchyChange() { }
    protected virtual void OnInspectorUpdate() { }

    protected virtual void OnProjectChange()
    {
        Debug.Log("Project change.");
    }

    internal int GetChildInstanceID(UESObject obj)
    {
        var type = obj.GetType();
        if (!instanceCounterDict.ContainsKey(type)) instanceCounterDict.Add(type, 0);
        instanceCounterDict[type]++;

        return instanceCounterDict[type];
    }

    protected virtual void OnGUI()
    {
        //优先响应自定义事件
        eventManager.OnGUI();
        stage.OnGUI();
    }

    protected virtual void Update()
    {
        if (EditorApplication.isCompiling)
        {
            Close();
            DestroyImmediate(this);
            return;
        }

        foreach (var obj in objList)
        {
            obj.Update();
        }

        //优先响应自定义事件
        eventManager.Update();                  
        stage.Update();
    }

    protected virtual void OnDestroy()
    {
        eventManager.OnDestroy();
    }

    /// <summary>
    /// 注册
    /// </summary>
    /// <param name="obj"></param>
    internal void Register(UESObject obj)
    {
        if(!objList.Contains(obj)) objList.Add(obj);
    }

    /// <summary>
    /// 取消注册
    /// </summary>
    /// <param name="obj"></param>
    internal void Unregister(UESObject obj)
    {
        objList.Remove(obj);
    }

    /// <summary>
    /// 窗口运行时间
    /// </summary>
    public double TimeSinceStartup { get { return EditorApplication.timeSinceStartup - startTime; } }
}

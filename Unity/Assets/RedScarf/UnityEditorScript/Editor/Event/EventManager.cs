using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

/// <summary>
/// 事件管理器
/// </summary>
public sealed class UESEventManager{

    static Dictionary<string, Type> typeDict;               //key:事件类型(string),value:事件type. 如typeDict[MOUSE_DOWN]=UESMouseEVent

    internal Dictionary<string, List<UESEvent>> eveDict;
    List<UESEvent> addList;                                  //添加事件缓冲区
    List<UESEvent> removeList;                               //移除事件缓冲区

    public UESEventManager()
    {
        Init();

        eveDict = new Dictionary<string, List<UESEvent>>();
        addList = new List<UESEvent>();                            
        removeList = new List<UESEvent>();                         
    }

    public void Update()
    {
        UpdateRunner();

        foreach (var eves in eveDict.Values)
        {
            foreach (var eve in eves)
            {
                eve.Update();
            }
        }
    }

    public void OnGUI()
    {
        UpdateRunner();

        foreach (var eves in eveDict.Values)
        {
            foreach (var eve in eves)
            {
                eve.OnGUI();
            }
        }
    }

    void UpdateRunner()
    {
        foreach (var e in addList)
        {
            if (!eveDict.ContainsKey(e.Key))
            {
                eveDict.Add(e.Key, new List<UESEvent>());
            }

            bool includeCallback = false;
            foreach(var eve in eveDict[e.Key])
            {
                if (eve.callback == e.callback)
                {
                    includeCallback = true;
                    break;
                }
            }
            if (!includeCallback)
            {
                eveDict[e.Key].Add(e);
                e.OnAddToManager();
            }
        }
        addList.Clear();

        foreach (var e in removeList)
        {
            if (eveDict.ContainsKey(e.Key))
            {
                eveDict[e.Key].RemoveAll(
                    (x) =>
                    {
                        return (x.callback==e.callback)?true:false;
                    }
                    );
                if (eveDict[e.Key].Count == 0)
                {
                    eveDict.Remove(e.Key);
                    e.OnRemoveFromManager();
                }
            }
        }
        removeList.Clear();
    }

    public void OnDestroy()
    {
        foreach (var eve in eveDict.Values)
        {
            removeList.AddRange(eve);
        }
        UpdateRunner();
        eveDict.Clear();
    }

    static void Init()
    {
        typeDict = new Dictionary<string, Type>();

        var superClassType = typeof(UESEvent);
        Type[] types = Assembly.GetAssembly(superClassType).GetTypes();
        foreach (Type t in types)
        {
            if (t.IsClass)
            {
                if (t.IsSubclassOf(superClassType)||t==superClassType)
                {
                    var flags = BindingFlags.Static | BindingFlags.Public;
                    var fields = t.GetFields(flags);

                    foreach (var field in fields)
                    {
                        var value = (string)field.GetValue(t);
                        if (!typeDict.ContainsKey(value)) typeDict.Add(value, t);
                        else Debug.LogError(t + "事件字段定义重复！");
                    }
                }
            }
        }
    }

    static public void AddEventListener(UESObject target, string type, Action<UESEvent> callback)
    {
        var eve = CreatEvent(target, type, callback);
        if(eve != null) target.Window.eventManager.addList.Add(eve);
    }

    static public void RemoveEventListener(UESObject target, string type, Action<UESEvent> callback)
    {
        var eve = CreatEvent(target,type,callback);
        if (eve != null) target.Window.eventManager.removeList.Add(eve);
    }

    static UESEvent CreatEvent(UESObject target, string type, Action<UESEvent> callback)
    {
        if (!typeDict.ContainsKey(type)) return null;

        UESEvent eve = (UESEvent)Activator.CreateInstance(typeDict[type], new object[] { target, type, callback });

        return eve;
    }
}

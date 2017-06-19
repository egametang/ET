using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.EventSystems;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using System;

public enum TestEnums
{
    One,
    Two,
    Three,
    Four
}
public static class UESTool {

    static string resourceFolder;
    static bool m_IsInit;

    /// <summary>
    /// 创建对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="window"></param>
    /// <returns></returns>
    public static T Create<T>(UESWindow window) where T : UESObject
    {
        var obj = ScriptableObject.CreateInstance<T>();
        obj.Register(window);

        return obj;
    }

    public static UESObject Create(UESWindow window,string className)
    {
        UESObject obj = (UESObject)ScriptableObject.CreateInstance(className);
        obj.Register(window);
        return obj;
    }

    public static UESObject Create(UESWindow window,Type type)
    {
        UESObject obj = (UESObject)ScriptableObject.CreateInstance(type);
        obj.Register(window);

        return obj;
    }

    /// <summary>
    /// 销毁
    /// </summary>
    /// <param name="obj"></param>
    public static void Destroy(UESObject obj)
    {
        obj.m_Window.Unregister(obj);
        UESObject.DestroyImmediate(obj);
    }

    static void Init()
    {
        if (!m_IsInit)
        {
            Reset();
            m_IsInit = true;
        }
    }

    static void Reset()
    {
        var paths = AssetDatabase.GetAllAssetPaths();
        foreach (var path in paths)
        {
            if (path.EndsWith("UnityEditorScript/ResourceFolder"))
            {
                resourceFolder = path;
            }
        }
    }

    internal static Texture2D GetTexture(string name)
    {
        Init();

        return AssetDatabase.LoadAssetAtPath<Texture2D>(resourceFolder+"/Image/"+ name + ".png");
    }

    internal static string GetText(string name)
    {
        Init();

        return AssetDatabase.LoadAssetAtPath<TextAsset>(resourceFolder + "/Text/" + name + ".txt").text;
    }

    /// <summary>
    /// 获取与点有碰撞的元素列表
    /// </summary>
    /// <param name="stage"></param>
    /// <param name="globalPoint"></param>
    /// <returns></returns>
    public static List<UESDisplayObject> GetHitDisplayList(UESStage stage, Vector3 globalPoint)
    {
        var displayList = new List<UESDisplayObject>();

        foreach (var display in stage.RenderList)
        {
            if (display.IsHit(globalPoint))
            {
                displayList.Add(display);
            }
        }
        if (stage.IsHit(globalPoint)) displayList.Add(stage);

        return displayList;
    }

    /// <summary>
    /// 获取矩形的交叉矩形，如果无交叉返回Rect(0,0,0,0);
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static Rect GetRectIntersect(Rect a,Rect b)
    {
        if (!a.Overlaps(b)) return new Rect();

        return Rect.MinMaxRect(Mathf.Max(a.xMin,b.xMin),Mathf.Max(a.yMin,b.yMin),Mathf.Min(a.xMax,b.xMax),Mathf.Min(a.yMax,b.yMax));
    }

    /// <summary>
    /// 获取包含所有点的最小矩形
    /// </summary>
    /// <param name="pointList"></param>
    /// <returns></returns>
    public static Rect GetContainsPointsRect(Vector2[] points)
    {
        var rect = new Rect();
        if (points.Length<=1) return rect;

        var cloneList = new List<Vector2>(points);
        cloneList.Sort(
            (a, b) =>
            {
                if (a.x == b.x) return 0;
                return a.x > b.x ? 1 : -1;
            }
            );
        var minX = cloneList[0].x;
        var maxX = cloneList[cloneList.Count - 1].x;
        cloneList.Sort(
            (a, b) =>
            {
                if (a.y == b.y) return 0;
                return a.y > b.y ? 1 : -1;
            }
            );
        var minY = cloneList[0].y;
        var maxY = cloneList[cloneList.Count - 1].y;
        rect = Rect.MinMaxRect(minX, minY, maxX, maxY);

        return rect;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 组件预设面板
/// </summary>
public class UESElementWindow : UESWindow
{
    [MenuItem("Tools/UnityEditorScript/Element")]
    public static void Init()
    {
        var window = EditorWindow.GetWindow<UESElementWindow>();
        window.titleContent = new GUIContent("Element");
        window.Show();
    }

    UESScrollView scrollView;
    UESHelpBox tooltip;
    List<UESDisplayObject> elementList;

    protected override void Awake()
    {
        base.Awake();

        scrollView = UESTool.Create<UESScrollView>(this);
        scrollView.Rect = new Rect(10,10,800,600);
        stage.AddChild(scrollView);

        tooltip = UESTool.Create<UESHelpBox>(this);
        stage.AddChild(tooltip);
        tooltip.Rect = new Rect(0, 0, 130, 40);
        tooltip.Depth = 10000;
        tooltip.MouseEnabled = false;
        tooltip.Enable = false;

        elementList = new List<UESDisplayObject>();
        int index = 0;
        var superClassType = typeof(UESDisplayObject);
        var types = Assembly.GetAssembly(superClassType).GetTypes();
        foreach (var t in types)
        {
            if (t.IsClass)
            {
                if (t.IsSubclassOf(superClassType))
                {
                    if (t==typeof(UESStage)) continue;

                    UESDisplayObject display = (UESDisplayObject)ScriptableObject.CreateInstance(t);
                    display.Register(this);
                    scrollView.AddChild(display);
                    elementList.Add(display);

                    index++;

                    if(display is UESScrollView)
                    {
                        for (var i=0;i<5; i++)
                        {
                            var box = UESTool.Create<UESBox>(this);
                            display.AddChild(box);
                            box.Rect = new Rect(i*100,0, 100, 200);
                            box.MouseEnabled = false;
                            box.SetDrawingStyle("");
                        }
                    }
                }
            }
        }
        Reposition();

        stage.AddEventListener(UESEvent.RESIZE, OnStageResize);
        stage.AddEventListener(UESMouseEvent.MOUSE_MOVE, OnMouseMove);
    }

    void Reposition()
    {
        int hIndex = 0;
        int vIndex = 0;
        for (var i=1;i< elementList.Count;i++)
        {
            var pos = new Vector2(hIndex*350, elementList[i - 1].Position.y+ elementList[i - 1].Size.y + 15);
            if(vIndex > 16||pos.y+ elementList[i].Size.y > stage.Size.y)
            {
                hIndex++;
                vIndex = 0;
                pos = new Vector2(hIndex * 350,0);
            }
            elementList[i].Position = pos;
            vIndex++;
        }
    }

    private void OnMouseMove(UESEvent obj)
    {
        var display = obj.Target as UESDisplayObject;
        if (display==scrollView || display is UESStage)
        {
            tooltip.Enable = false;
            return;
        }

        tooltip.Enable = true;
        tooltip.Position = obj.CurrentEvent.mousePosition - new Vector2(tooltip.Rect.size.x, tooltip.Rect.size.y*0.5f);
        var className = display.GetType().Name;
        tooltip.SetDrawingStyle(className.Substring(3, className.Length-3), MessageType.Info);
    }

    private void OnStageResize(UESEvent obj)
    {
        Reposition();
        scrollView.Rect = new Rect(new Vector2(10,10),stage.Rect.size-new Vector2(10,10));
    }
}

using UnityEngine;
using System.Collections;
using UnityEditor;
using System;

/// <summary>
/// 范例，脚本必须位于Editor文件夹下，继承自UESWindow
/// </summary>
public class UESDemoWindow : UESWindow
{
    /// <summary>
    /// 此处打开面板
    /// </summary>
    [MenuItem("Tools/UnityEditorScript/Demo")]
    static void Init()
    {
        var window = EditorWindow.GetWindow<UESDemoWindow>();
        window.titleContent = new GUIContent("Demo Window");
        window.Show();
    }

    UESBox box;

    /// <summary>
    /// 创建对象
    /// </summary>
    protected override void Awake()
    {
        base.Awake();                                                   //必须

        box = UESTool.Create<UESBox>(this);                             //使用UESTool.Create<T>()创建对象
        box.Rect = new Rect(100,100,100,100);
        box.Mask = new Rect(-20, -20, 220, 300);                        //可以添加一个遮罩
        stage.AddChild(box);                                            //添加对象到舞台
        box.Text = "click！";

        var helpBox = UESTool.Create<UESHelpBox>(this);
        helpBox.Position = new Vector2(100, 100);                       //可以只设置位置,使用默认的尺寸
        helpBox.SetDrawingStyle("drag!");
        box.AddChild(helpBox);

        var toolBar = UESTool.Create<UESToolbar>(this);
        stage.AddChild(toolBar);
        //单个物体添加监听
        toolBar.AddEventListener(UESMouseEvent.MOUSE_CLICK, OnToolBarMouseClick);

        //也可以给舞台添加鼠标监听
        stage.AddEventListener(UESMouseEvent.MOUSE_CLICK, OnMouseClick);
        stage.AddEventListener(UESMouseEvent.MOUSE_DOWN, OnMouseDown);
        stage.AddEventListener(UESMouseEvent.MOUSE_UP, OnMouseUp);

        //添加键盘监听
        stage.AddEventListener(UESKeyboardEvent.KEY_UP, OnKeyUp);

        //添加一个计时器
        var timer = UESTool.Create<UESTimer>(this);
        timer.Init(1000);
        timer.Start();
        timer.AddEventListener(UESTimerEvent.TIMER, OnTimer);
    }

    private void OnTimer(UESEvent obj)
    {
        Debug.Log("1sec...");
        box.Size *= 1.1f;
    }

    private void OnKeyUp(UESEvent obj)
    {
        Debug.Log(obj.CurrentEvent.keyCode);
    }

    private void OnToolBarMouseClick(UESEvent obj)
    {
        Debug.Log("ToolBar selected is "+(obj.CurrentTarget as UESToolbar).Selected);
    }

    private void OnMouseUp(UESEvent obj)
    {
        var display = obj.Target as UESDisplayObject;
        display.StopDrag();
    }

    private void OnMouseDown(UESEvent obj)
    {
        var display = obj.Target as UESDisplayObject;
        display.StartDrag();
    }

    private void OnMouseClick(UESEvent obj)
    {
        var display = obj.Target as UESDisplayObject;
        display.Rect = new Rect(display.Rect.position, display.Rect.size*1.1f);
    }
}

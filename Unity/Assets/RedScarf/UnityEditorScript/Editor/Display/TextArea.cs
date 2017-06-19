using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class UESTextArea : UESDisplayObject
{
    protected string m_Text;
    protected int drawingStyle;
    protected GUIStyle m_Style;

    protected override void Awake()
    {
        base.Awake();
        SetDrawingStyle();
        Text = "This is TextArea...";
        Rect = new Rect(Vector2.zero,new Vector2(INIT_WIDTH,48));
    }

    public override void OnGUI()
    {
        switch (drawingStyle)
        {
            case 1:
                m_Text = EditorGUI.TextArea(m_DrawingRect, m_Text);
                break;

            case 2:
                m_Text = EditorGUI.TextArea(m_DrawingRect, m_Text,m_Style);
                break;
        }
    }

    public void SetDrawingStyle()
    {
        drawingStyle = 1;
    }

    public void SetDrawingStyle(GUIStyle style)
    {
        drawingStyle = 2;

        m_Style = style;
    }

    public string Text
    {
        get
        {
            return m_Text;
        }
        set
        {
            m_Text = value;
        }
    }
}

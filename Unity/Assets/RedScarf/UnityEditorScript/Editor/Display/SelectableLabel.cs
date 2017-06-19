using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class UESSelectableLabel : UESDisplayObject
{
    protected string m_Text;
    protected int drawingStyle;
    protected GUIStyle m_Style;

    protected override void Awake()
    {
        base.Awake();
        SetDrawingStyle("This is SelectableLabel...");
    }

    public override void OnGUI()
    {
        switch (drawingStyle)
        {
            case 1:
                EditorGUI.SelectableLabel(m_DrawingRect, m_Text);
                break;

            case 2:
                EditorGUI.SelectableLabel(m_DrawingRect, m_Text,m_Style);
                break;
        }
    }

    public void SetDrawingStyle(string text)
    {
        drawingStyle = 1;

        m_Text = text;
    }

    public void SetDrawingStyle(string text,GUIStyle style)
    {
        drawingStyle = 2;

        m_Text = text;
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

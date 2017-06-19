using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class UESTextField : UESDisplayObject
{
    protected string m_Text;
    protected int drawingStyle;
    protected GUIStyle m_Style;
    protected GUIContent m_Label;
    protected string m_LabelStr;

    protected override void Awake()
    {
        base.Awake();
        SetDrawingStyle("TextField");
        Text = "This is TextField...";
    }

    public override void OnGUI()
    {
        switch (drawingStyle)
        {
            case 1:
                m_Text = EditorGUI.TextField(m_DrawingRect, m_Text);
                break;

            case 2:
                m_Text = EditorGUI.TextField(m_DrawingRect,m_Label ,m_Text);
                break;

            case 3:
                m_Text = EditorGUI.TextField(m_DrawingRect, m_Text,m_Style);
                break;

            case 4:
                m_Text = EditorGUI.TextField(m_DrawingRect, m_LabelStr, m_Text);
                break;

            case 5:
                m_Text = EditorGUI.TextField(m_DrawingRect, m_Label, m_Text,m_Style);
                break;

            case 6:
                m_Text = EditorGUI.TextField(m_DrawingRect, m_LabelStr, m_Text, m_Style);
                break;
        }
    }

    public void SetDrawingStyle()
    {
        drawingStyle = 1;
    }

    public void SetDrawingStyle(GUIContent label)
    {
        drawingStyle = 2;

        m_Label = label;
    }

    public void SetDrawingStyle(GUIStyle style)
    {
        drawingStyle = 3;

        m_Style = style;
    }

    public void SetDrawingStyle(string label)
    {
        drawingStyle = 4;

        m_LabelStr = label;
    }

    public void SetDrawingStyle(GUIContent label, GUIStyle style)
    {
        drawingStyle = 5;

        m_Label = label;
        m_Style = style;
    }

    public void SetDrawingStyle(string label,GUIStyle style)
    {
        drawingStyle = 6;

        m_LabelStr = label;
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

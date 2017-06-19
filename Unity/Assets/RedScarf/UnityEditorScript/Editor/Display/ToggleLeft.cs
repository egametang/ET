using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class UESToggleLeft : UESDisplayObject
{
    public const int HEIGHT = 18;

    protected bool m_Value;
    protected int drawingStyle;
    protected GUIContent m_Label;
    protected string m_LabelStr;
    protected GUIStyle m_Style;

    protected override void Awake()
    {
        base.Awake();
        SetDrawingStyle("ToggleLeft");
    }

    public override void OnGUI()
    {
        switch (drawingStyle)
        {
            case 1:
                m_Value = EditorGUI.ToggleLeft(m_DrawingRect, m_Label, m_Value);
                break;

            case 2:
                m_Value = EditorGUI.ToggleLeft(m_DrawingRect, m_LabelStr, m_Value);
                break;

            case 3:
                m_Value = EditorGUI.ToggleLeft(m_DrawingRect, m_Label, m_Value,m_Style);
                break;

            case 4:
                m_Value = EditorGUI.ToggleLeft(m_DrawingRect, m_LabelStr, m_Value,m_Style);
                break;
        }
    }

    public void SetDrawingStyle(GUIContent label)
    {
        drawingStyle = 1;

        m_Label = label;
    }

    public void SetDrawingStyle(string label)
    {
        drawingStyle = 2;

        m_LabelStr = label;
    }

    public void SetDrawingStyle(GUIContent label, GUIStyle style)
    {
        drawingStyle = 3;

        m_Label = label;
        m_Style = style;
    }

    public void SetDrawingStyle(string label, GUIStyle style)
    {
        drawingStyle = 4;

        m_LabelStr = label;
        m_Style = style;
    }

    public override Rect Rect
    {
        get
        {
            return base.Rect;
        }

        set
        {
            value = new Rect(value.position,new Vector2(value.width, HEIGHT));
            base.Rect = value;
        }
    }

    public bool Value
    {
        get
        {
            return m_Value;
        }
        set
        {
            m_Value = value;
        }
    }
}

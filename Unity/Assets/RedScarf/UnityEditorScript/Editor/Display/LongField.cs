using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class UESLongField : UESDisplayObject
{
    protected long m_Value;
    protected GUIContent m_Label;
    protected string m_LabelStr;
    protected GUIStyle m_Style;
    protected int drawingStyle;

    protected override void Awake()
    {
        base.Awake();
        SetDrawingStyle("LongField");
        Value = long.MaxValue;
    }

    public override void OnGUI()
    {
        switch (drawingStyle)
        {
            case 1:
                m_Value = EditorGUI.LongField(m_DrawingRect, m_Value);
                break;

            case 2:
                m_Value = EditorGUI.LongField(m_DrawingRect, m_Label, m_Value);
                break;

            case 3:
                m_Value = EditorGUI.LongField(m_DrawingRect, m_Value, m_Style);
                break;

            case 4:
                m_Value = EditorGUI.LongField(m_DrawingRect, m_LabelStr, m_Value);
                break;

            case 5:
                m_Value = EditorGUI.LongField(m_DrawingRect, m_Label, m_Value, m_Style);
                break;

            case 6:
                m_Value = EditorGUI.LongField(m_DrawingRect, m_LabelStr, m_Value, m_Style);
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

    public void SetDrawingStyle(string label, GUIStyle style)
    {
        drawingStyle = 6;

        m_LabelStr = label;
        m_Style = style;
    }

    public long Value
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

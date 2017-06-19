using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class UESEnumMaskField : UESDisplayObject
{
    protected Enum m_EnumValue;
    protected int drawingStyle;
    protected GUIStyle m_Style;
    protected GUIContent m_Label;
    protected string m_LabelStr;

    protected override void Awake()
    {
        base.Awake();
        SetDrawingStyle("EnumMaskField");
        m_EnumValue = TestEnums.One;
    }

    public override void OnGUI()
    {
        switch (drawingStyle)
        {
            case 1:
                m_EnumValue = EditorGUI.EnumMaskField(m_DrawingRect, m_EnumValue);
                break;

            case 2:
                m_EnumValue = EditorGUI.EnumMaskField(m_DrawingRect, m_EnumValue,m_Style);
                break;

            case 3:
                m_EnumValue = EditorGUI.EnumMaskField(m_DrawingRect, m_Label, m_EnumValue);
                break;

            case 4:
                m_EnumValue = EditorGUI.EnumMaskField(m_DrawingRect, m_LabelStr, m_EnumValue);
                break;

            case 5:
                m_EnumValue = EditorGUI.EnumMaskField(m_DrawingRect, m_Label, m_EnumValue,m_Style);
                break;

            case 6:
                m_EnumValue = EditorGUI.EnumMaskField(m_DrawingRect, m_LabelStr, m_EnumValue,m_Style);
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

    public void SetDrawingStyle(GUIContent label)
    {
        drawingStyle = 3;

        m_Label = label;
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

    public Enum EnumValue
    {
        get
        {
            return m_EnumValue;
        }
        set
        {
            m_EnumValue = value;
        }
    }
}
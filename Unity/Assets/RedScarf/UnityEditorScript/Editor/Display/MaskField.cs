using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class UESMaskField : UESDisplayObject
{
    protected int m_MaskValue;
    protected string[] m_DisplayOptions;
    protected GUIContent m_Label;
    protected string m_LabelStr;
    protected int drawingStyle;
    protected GUIStyle m_Style;

    protected override void Awake()
    {
        base.Awake();
        var displayOptions = new string[]
        {
            "1",
            "2",
            "3"
        };
        SetDrawingStyle("MaskField", displayOptions);
        MaskValue = 0;
    }

    public override void OnGUI()
    {
        switch (drawingStyle)
        {
            case 1:
                m_MaskValue = EditorGUI.MaskField(m_DrawingRect,m_MaskValue,m_DisplayOptions);
                break;

            case 2:
                m_MaskValue = EditorGUI.MaskField(m_DrawingRect, m_Label, m_MaskValue, m_DisplayOptions);
                break;

            case 3:
                m_MaskValue = EditorGUI.MaskField(m_DrawingRect, m_MaskValue, m_DisplayOptions, m_Style);
                break;

            case 4:
                m_MaskValue = EditorGUI.MaskField(m_DrawingRect, m_LabelStr, m_MaskValue, m_DisplayOptions);
                break;

            case 5:
                m_MaskValue = EditorGUI.MaskField(m_DrawingRect, m_Label, m_MaskValue, m_DisplayOptions, m_Style);
                break;

            case 6:
                m_MaskValue = EditorGUI.MaskField(m_DrawingRect, m_LabelStr, m_MaskValue, m_DisplayOptions, m_Style);
                break;
        }
    }

    public void SetDrawingStyle(string[] displayOptions)
    {
        drawingStyle = 1;

        m_DisplayOptions = displayOptions;
    }

    public void SetDrawingStyle(GUIContent label, string[] displayOptions)
    {
        drawingStyle = 2;

        m_Label = label;
        m_DisplayOptions = displayOptions;
    }

    public void SetDrawingStyle(string[] displayOptions,GUIStyle style)
    {
        drawingStyle = 3;

        m_Style = style;
        m_DisplayOptions = displayOptions;
    }

    public void SetDrawingStyle(string label, string[] displayOptions)
    {
        drawingStyle = 4;

        m_LabelStr = label;
        m_DisplayOptions = displayOptions;
    }

    public void SetDrawingStyle(GUIContent label, string[] displayOptions,GUIStyle style)
    {
        drawingStyle = 5;

        m_Label = label;
        m_DisplayOptions = displayOptions;
        m_Style = style;
    }

    public void SetDrawingStyle(string label, string[] displayOptions, GUIStyle style)
    {
        drawingStyle = 6;

        m_LabelStr = label;
        m_DisplayOptions = displayOptions;
        m_Style = style;
    }

    public int MaskValue
    {
        get
        {
            return m_MaskValue;
        }
        set
        {
            m_MaskValue = value;
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class UESPopup : UESDisplayObject
{
    protected GUIContent[] m_DisplayOptions;
    protected string[] m_StrDisplayOptions;
    protected int m_SelectIndex;
    protected int drawingStyle;
    protected GUIContent m_Label;
    protected string m_LabelStr;
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
        SetDrawingStyle("Popup", displayOptions);
    }

    public override void OnGUI()
    {
        switch (drawingStyle)
        {
            case 1:
                m_SelectIndex = EditorGUI.Popup(m_DrawingRect, m_SelectIndex, m_DisplayOptions);
                break;

            case 2:
                m_SelectIndex = EditorGUI.Popup(m_DrawingRect, m_SelectIndex, m_StrDisplayOptions);
                break;

            case 3:
                m_SelectIndex = EditorGUI.Popup(m_DrawingRect, m_Label, m_SelectIndex, m_DisplayOptions);
                break;

            case 4:
                m_SelectIndex = EditorGUI.Popup(m_DrawingRect, m_SelectIndex, m_DisplayOptions, m_Style);
                break;

            case 5:
                m_SelectIndex = EditorGUI.Popup(m_DrawingRect, m_SelectIndex, m_StrDisplayOptions, m_Style);
                break;

            case 6:
                m_SelectIndex = EditorGUI.Popup(m_DrawingRect, m_LabelStr, m_SelectIndex, m_StrDisplayOptions);
                break;

            case 7:
                m_SelectIndex = EditorGUI.Popup(m_DrawingRect, m_Label, m_SelectIndex, m_DisplayOptions, m_Style);
                break;

            case 8:
                m_SelectIndex = EditorGUI.Popup(m_DrawingRect, m_LabelStr, m_SelectIndex, m_StrDisplayOptions, m_Style);
                break;
        }
    }

    public void SetDrawingStyle(GUIContent[] displayOptions)
    {
        drawingStyle = 1;

        m_DisplayOptions = displayOptions;
    }

    public void SetDrawingStyle(string[] displayOptions)
    {
        drawingStyle = 2;

        m_StrDisplayOptions = displayOptions;
    }

    public void SetDrawingStyle(GUIContent label,GUIContent[] displayOptions)
    {
        drawingStyle = 3;

        m_Label = label;
        m_DisplayOptions = displayOptions;
    }

    public void SetDrawingStyle(GUIContent[] displayOptions,GUIStyle style)
    {
        drawingStyle = 4;

        m_Style = style;
        m_DisplayOptions = displayOptions;
    }

    public void SetDrawingStyle(string[] displayOptions, GUIStyle style)
    {
        drawingStyle = 5;

        m_Style = style;
        m_StrDisplayOptions = displayOptions;
    }

    public void SetDrawingStyle(string label,string[] displayOptions)
    {
        drawingStyle = 6;

        m_LabelStr = label;
        m_StrDisplayOptions = displayOptions;
    }

    public void SetDrawingStyle(GUIContent label,GUIContent[] displayOptions, GUIStyle style)
    {
        drawingStyle = 7;

        m_Label = label;
        m_Style = style;
        m_DisplayOptions = displayOptions;
    }

    public void SetDrawingStyle(string label,string[] displayOptions, GUIStyle style)
    {
        drawingStyle = 8;

        m_LabelStr = label;
        m_Style = style;
        m_StrDisplayOptions = displayOptions;
    }

    public int SelectIndex
    {
        get
        {
            return m_SelectIndex;
        }

        set
        {
            m_SelectIndex = value;
        }
    }
}

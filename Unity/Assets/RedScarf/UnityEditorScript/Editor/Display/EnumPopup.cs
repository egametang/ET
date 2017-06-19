using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class UESEnumPopup : UESDisplayObject
{
    protected Enum m_Selected;
    protected int drawingStyle;
    protected GUIContent m_Label;
    protected GUIStyle m_Style;
    protected string m_LabelStr;

    protected override void Awake()
    {
        base.Awake();
        SetDrawingStyle("EnumPopup");
        m_Selected = TestEnums.One;
    }

    public override void OnGUI()
    {
        switch (drawingStyle)
        {
            case 1:
                m_Selected = EditorGUI.EnumPopup(m_DrawingRect, m_Selected);
                break;

            case 2:
                m_Selected = EditorGUI.EnumPopup(m_DrawingRect, m_Selected, m_Style);
                break;

            case 3:
                m_Selected = EditorGUI.EnumPopup(m_DrawingRect, m_Label, m_Selected);
                break;

            case 4:
                m_Selected = EditorGUI.EnumPopup(m_DrawingRect, m_LabelStr, m_Selected);
                break;

            case 5:
                m_Selected = EditorGUI.EnumPopup(m_DrawingRect, m_Label, m_Selected, m_Style);
                break;

            case 6:
                m_Selected = EditorGUI.EnumPopup(m_DrawingRect, m_LabelStr, m_Selected, m_Style);
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

    public Enum Selected
    {
        get
        {
            return m_Selected;
        }
        set
        {
            m_Selected = value;
        }
    }
}

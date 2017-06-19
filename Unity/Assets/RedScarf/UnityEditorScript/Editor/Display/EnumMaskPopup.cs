using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class UESEnumMaskPopup : UESDisplayObject
{
    protected Enum m_Selected;
    protected int drawingStyle;
    protected GUIContent m_Label;
    protected GUIStyle m_Style;

    protected override void Awake()
    {
        base.Awake();
        SetDrawingStyle(new GUIContent("EnumMaskPopup"));
        m_Selected = TestEnums.One;
    }

    public override void OnGUI()
    {
        switch (drawingStyle)
        {
            case 1:
                m_Selected = EditorGUI.EnumMaskPopup(m_DrawingRect, m_Label, m_Selected);
                break;

            case 2:
                m_Selected = EditorGUI.EnumMaskPopup(m_DrawingRect, m_Label, m_Selected,m_Style);
                break;
        }
    }

    public void SetDrawingStyle(GUIContent label)
    {
        drawingStyle = 1;

        m_Label = label;
    }

    public void SetDrawingStyle(GUIContent label,GUIStyle style)
    {
        drawingStyle = 2;

        m_Label = label;
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

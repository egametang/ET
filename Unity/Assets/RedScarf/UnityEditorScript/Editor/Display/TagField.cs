using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class UESTagField : UESDisplayObject
{
    protected string m_Tag;
    protected int drawingStyle;
    protected GUIContent m_Label;
    protected string m_LabelStr;
    protected GUIStyle m_Style;

    protected override void Awake()
    {
        base.Awake();
        SetDrawingStyle("TagField");
    }

    public override void OnGUI()
    {
        switch (drawingStyle)
        {
            case 1:
                m_Tag = EditorGUI.TagField(m_DrawingRect, m_Tag);
                break;

            case 2:
                m_Tag = EditorGUI.TagField(m_DrawingRect,m_Label, m_Tag);
                break;

            case 3:
                m_Tag = EditorGUI.TagField(m_DrawingRect, m_Tag,m_Style);
                break;

            case 4:
                m_Tag = EditorGUI.TagField(m_DrawingRect, m_LabelStr, m_Tag);
                break;

            case 5:
                m_Tag = EditorGUI.TagField(m_DrawingRect, m_Label, m_Tag,m_Style);
                break;

            case 6:
                m_Tag = EditorGUI.TagField(m_DrawingRect, m_LabelStr, m_Tag,m_Style);
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

    public void SetDrawingStyle(GUIContent label,GUIStyle style)
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

    public string Tag
    {
        get
        {
            return m_Tag;
        }
        set
        {
            m_Tag = value;
        }
    }
}

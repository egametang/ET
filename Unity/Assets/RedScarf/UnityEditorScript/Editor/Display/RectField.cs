using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class UESRectField : UESDisplayObject
{
    protected Rect m_Value;
    protected int drawingStyle;
    protected GUIContent m_Label;
    protected string m_LabelStr;

    protected override void Awake()
    {
        base.Awake();
        SetDrawingStyle("RectField");
        Rect = new Rect(Vector2.zero, new Vector2(INIT_WIDTH, 48));
    }

    public override void OnGUI()
    {
        switch (drawingStyle)
        {
            case 1:
                m_Value = EditorGUI.RectField(m_DrawingRect, m_Value);
                break;

            case 2:
                m_Value = EditorGUI.RectField(m_DrawingRect, m_Label, m_Value);
                break;

            case 3:
                m_Value = EditorGUI.RectField(m_DrawingRect, m_LabelStr, m_Value);
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

    public void SetDrawingStyle(string label)
    {
        drawingStyle = 3;

        m_LabelStr = label;
    }

    public Rect Value
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
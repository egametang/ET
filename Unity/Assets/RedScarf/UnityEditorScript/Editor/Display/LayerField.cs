using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class UESLayerField : UESDisplayObject {

    protected int m_Layer;
    protected int drawingStyle;
    protected GUIContent m_Label;
    protected GUIStyle m_Style;
    protected string m_LabelStr;

    protected override void Awake()
    {
        base.Awake();
        SetDrawingStyle("LayerField");
    }

    public override void OnGUI()
    {
        switch (drawingStyle)
        {
            case 1:
                m_Layer = EditorGUI.LayerField(m_DrawingRect, m_Layer);
                break;

            case 2:
                m_Layer = EditorGUI.LayerField(m_DrawingRect,m_Label,m_Layer);
                break;

            case 3:
                m_Layer = EditorGUI.LayerField(m_DrawingRect, m_Layer, m_Style);
                break;

            case 4:
                m_Layer = EditorGUI.LayerField(m_DrawingRect,m_LabelStr ,m_Layer);
                break;

            case 5:
                m_Layer = EditorGUI.LayerField(m_DrawingRect, m_Label, m_Layer,m_Style);
                break;

            case 6:
                m_Layer = EditorGUI.LayerField(m_DrawingRect, m_LabelStr, m_Layer, m_Style);
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

    public int Layer
    {
        get
        {
            return m_Layer;
        }
        set
        {
            m_Layer = value;
        }
    }
}

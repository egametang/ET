using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class UESLabelField : UESDisplayObject {

    protected string m_LabelStr;
    protected string m_LabelStr2;
    protected GUIContent m_Label;
    protected GUIStyle m_Style;
    protected GUIContent m_Label2;
    protected int drawingStyle;

    protected override void Awake()
    {
        base.Awake();
        SetDrawingStyle("This is LabelField...");
    }

    public override void OnGUI()
    {
        switch (drawingStyle)
        {
            case 1:
                EditorGUI.LabelField(m_DrawingRect, m_Label);
                break;

            case 2:
                EditorGUI.LabelField(m_DrawingRect, m_LabelStr);
                break;

            case 3:
                EditorGUI.LabelField(m_DrawingRect, m_Label, m_Label2);
                break;

            case 4:
                EditorGUI.LabelField(m_DrawingRect, m_Label, m_Style);
                break;

            case 5:
                EditorGUI.LabelField(m_DrawingRect, m_LabelStr, m_Style);
                break;

            case 6:
                EditorGUI.LabelField(m_DrawingRect, m_LabelStr,m_LabelStr2);
                break;

            case 7:
                EditorGUI.LabelField(m_DrawingRect, m_Label, m_Label2,m_Style);
                break;

            case 8:
                EditorGUI.LabelField(m_DrawingRect, m_LabelStr, m_LabelStr2,m_Style);
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

    public void SetDrawingStyle(GUIContent label,GUIContent label2)
    {
        drawingStyle = 3;

        m_Label = label;
        m_Label2 = label2;
    }

    public void SetDrawingStyle(GUIContent label,GUIStyle style)
    {
        drawingStyle = 4;

        m_Label = label;
        m_Style = style;
    }

    public void SetDrawingStyle(string label,GUIStyle style)
    {
        drawingStyle = 5;

        m_LabelStr = label;
        m_Style = style;
    }

    public void SetDrawingStyle(string label,string label2)
    {
        drawingStyle = 6;

        m_LabelStr = label;
        m_LabelStr2 = label2;
    }

    public void SetDrawingStyle(GUIContent label, GUIContent label2,GUIStyle style)
    {
        drawingStyle = 7;

        m_Label = label;
        m_Label2 = label2;
        m_Style = style;
    }

    public void SetDrawingStyle(string label, string label2,GUIStyle style)
    {
        drawingStyle = 6;

        m_LabelStr = label;
        m_LabelStr2 = label2;
        m_Style = style;
    }
}

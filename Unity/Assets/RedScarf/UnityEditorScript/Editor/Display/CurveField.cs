using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class UESCurveField : UESDisplayObject
{
    protected AnimationCurve m_Value;
    protected int drawingStyle;
    protected GUIContent m_Label;
    protected string m_LabelStr;
    protected Color m_Color;
    protected Rect m_Ranges;
    protected SerializedProperty m_SerializedValue;

    protected override void Awake()
    {
        base.Awake();
        SetDrawingStyle("CurveField");
        var keys = new Keyframe[]
        {
            new Keyframe(0,0),
            new Keyframe(1,1)
        };
        Value = new AnimationCurve(keys);
    }

    public override void OnGUI()
    {
        if (m_Value == null) return;

        switch (drawingStyle)
        {
            case 1:
                m_Value = EditorGUI.CurveField(m_DrawingRect, m_Value);
                break;

            case 2:
                m_Value = EditorGUI.CurveField(m_DrawingRect,m_Label, m_Value);
                break;

            case 3:
                m_Value = EditorGUI.CurveField(m_DrawingRect, m_LabelStr, m_Value);
                break;

            case 4:
                m_Value = EditorGUI.CurveField(m_DrawingRect, m_Value, m_Color, m_Ranges);
                break;

            case 5:
                EditorGUI.CurveField(m_DrawingRect, m_SerializedValue, m_Color, m_Ranges);
                break;

            case 6:
                m_Value = EditorGUI.CurveField(m_DrawingRect, m_Label, m_Value, m_Color, m_Ranges);
                break;

            case 7:
                m_Value = EditorGUI.CurveField(m_DrawingRect, m_LabelStr, m_Value, m_Color, m_Ranges);
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

    public void SetDrawingStyle(Color color,Rect ranges)
    {
        drawingStyle = 4;

        m_Color = color;
        m_Ranges = ranges;
    }

    public void SetDrawingStyle(SerializedProperty value, Color color, Rect ranges)
    {
        drawingStyle = 5;

        m_SerializedValue = value;
        m_Color = color;
        m_Ranges = ranges;
    }

    public void SetDrawingStyle(GUIContent label,Color color,Rect ranges)
    {
        drawingStyle = 6;

        m_Label = label;
        m_Color = color;
        m_Ranges = ranges;
    }

    public void SetDrawingStyle(string label, Color color, Rect ranges)
    {
        drawingStyle = 7;

        m_LabelStr = label;
        m_Color = color;
        m_Ranges = ranges;
    }

    /// <summary>
    /// 关键点
    /// </summary>
    public AnimationCurve Value
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